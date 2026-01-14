using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Nodes;
using WJb;
using WJb.Extensions;
using WJb.Telemetry;

using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Instrumentation.Runtime;
using OpenTelemetry.Instrumentation.Process;

// Host setup
var host = Host.CreateDefaultBuilder(args)
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddSimpleConsole(o => { o.SingleLine = true; });
        logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.None);
    })
    .ConfigureServices(services =>
    {
        // 1) Settings registry (required by LogJobProcessor and for queue options materialization)
        services.AddSingleton<IReloadableSettingsRegistry, ReloadableSettingsRegistry>();

        // 2) Materialize IOptions<Dictionary<string, object>> for the queue once at startup
        services.AddSingleton<IOptions<Dictionary<string, object>>>(sp =>
        {
            // For this sample, we can start with an empty config dictionary.
            // If you want capacities/weights, populate keys here (e.g., "CapacityASAP", "MaxInFlightHigh", etc.)
            var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            return Options.Create(dict);
        });

        // 3) Register the queue using the new ctor order: options first, logger last
        services.AddSingleton<IJobQueue>(sp =>
            new InMemoryJobQueue(
                sp.GetRequiredService<IOptions<Dictionary<string, object>>>(),
                sp.GetRequiredService<ILogger<InMemoryJobQueue>>()));

        // 4) Register your ActionFactory (implements IActionFactory + reload registry contract)
        services.AddSingleton<IActionFactory, DemoActionFactory>();

        // 5) Register LogJobProcessor using new ctor order: (queue, factory, settingsRegistry, logger)
        services.AddSingleton<MetricsJobProcessor>(sp => new MetricsJobProcessor(
            sp.GetRequiredService<IJobQueue>(),
            sp.GetRequiredService<IActionFactory>(),
            sp.GetRequiredService<IReloadableSettingsRegistry>(),
            sp.GetRequiredService<ILogger<MetricsJobProcessor>>()));

        // Alias to IJobProcessor and host a single instance
        services.AddSingleton<IJobProcessor>(sp => sp.GetRequiredService<MetricsJobProcessor>());
        services.AddHostedService(sp => sp.GetRequiredService<MetricsJobProcessor>());

        // --- OpenTelemetry Metrics: Console exporter ---

        services.AddOpenTelemetry()
            .WithMetrics(builder =>
            {
                builder
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("WJb.Sample", "1.0.0"))
                    .AddMeter("WJb", "WJb.Telemetry", "WJb.JobProcessor")
                    .AddRuntimeInstrumentation()   // requires OpenTelemetry.Instrumentation.Runtime
                    .AddProcessInstrumentation()   // requires OpenTelemetry.Instrumentation.Process
                    .AddConsoleExporter();         // <-- now recognized
            });

    })
    .Build();

// Use processor
var proc = host.Services.GetRequiredService<IJobProcessor>();
var job = await proc.CompactAsync("SayHello", new { name = "Viktor" });
await proc.EnqueueJobAsync(job, Priority.High);

// Start the hosted service infrastructure
await host.RunAsync();

// -------------------------------
// DemoAction & DemoActionFactory
// -------------------------------

public sealed class DemoAction : IAction
{
    private readonly string _type;
    public DemoAction(string type) => _type = type;

    public Task ExecAsync(JsonObject? more, CancellationToken stoppingToken = default)
    {
        var name = more.GetString("name") ?? "World";
        Console.WriteLine($"{_type} {name}!");
        return Task.CompletedTask;
    }
}

/// <summary>
/// Minimal demo ActionFactory that implements both creation and the reloadable registry.
/// - Create(): returns DemoAction by CLR type name (here we treat actionCode as "type" for demo).
/// - GetActionItem(): returns ActionItem(Type=code, More=defaults or reloaded config).
/// - Reload*: updates the in-memory registry and fires Reloaded.
/// </summary>
public sealed class DemoActionFactory : IActionFactory
{
    // Simple in-memory registry: action code -> ActionItem
    private readonly Dictionary<string, ActionItem> _map =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["SayHello"] = new ActionItem("SayHello", new JsonObject { ["name"] = "Oleksandr" })
        };

    public event Action? Reloaded;

    public IAction Create(string actionType) => new DemoAction(actionType);

    public ActionItem GetActionItem(string actionCode)
    {
        if (_map.TryGetValue(actionCode, out var item))
            return new ActionItem(item.Type, item.More?.DeepClone() as JsonObject);

        return new ActionItem(actionCode, more: null);
    }

    public IReadOnlyDictionary<string, ActionItem> Snapshot() => _map;

    public void Reload(IDictionary<string, ActionItem> newConfig)
    {
        if (newConfig is null) return;

        _map.Clear();
        foreach (var kv in newConfig)
        {
            var src = kv.Value;
            _map[kv.Key] = new ActionItem(src.Type, src.More?.DeepClone() as JsonObject);
        }
        Reloaded?.Invoke();
    }

    private static JsonSerializerOptions GetOptions() => new() { PropertyNameCaseInsensitive = true };

    public void ReloadFromJson(string json, JsonSerializerOptions? options = default)
    {
        if (string.IsNullOrWhiteSpace(json)) return;
        var map = JsonSerializer.Deserialize<Dictionary<string, ActionItem>>(json, options ?? GetOptions())
                  ?? new(StringComparer.OrdinalIgnoreCase);
        Reload(map);
    }

    public void ReloadFromFile(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return;
        ReloadFromJson(File.ReadAllText(path));
    }
}
