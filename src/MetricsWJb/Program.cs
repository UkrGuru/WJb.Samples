
#nullable enable

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Nodes;
using WJb;
using WJb.Extensions;
using WJb.Telemetry; // <-- your MetricsJobProcessor & MetricsJobScheduler

// -------------------------------------------------------------
// Host setup
// -------------------------------------------------------------
var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(cfg =>
    {
        cfg.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
    })
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddSimpleConsole(o => o.SingleLine = true);
        logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.None);
        logging.AddFilter("Microsoft.Extensions.Hosting", LogLevel.None);
    })
    .ConfigureServices((ctx, services) =>
    {
        // --- WJb components (manual DI — same style you use everywhere) ---
        services.AddSingleton<IJobQueue, InMemoryJobQueue>();
        services.AddSingleton<IActionFactory, DemoActionFactory>();

        // Registry for runtime settings (JsonObject-based)
        services.AddSingleton<IReloadableSettingsRegistry, InMemorySettingsRegistry>();

        // --- Telemetry-aware JobProcessor ---
        services.AddSingleton<MetricsJobProcessor>(sp => new MetricsJobProcessor(
            sp.GetRequiredService<IJobQueue>(),
            sp.GetRequiredService<IActionFactory>(),
            sp.GetRequiredService<IReloadableSettingsRegistry>(),
            sp.GetRequiredService<ILogger<MetricsJobProcessor>>()
        ));
        services.AddSingleton<IJobProcessor>(sp => sp.GetRequiredService<MetricsJobProcessor>());
        services.AddHostedService(sp => sp.GetRequiredService<MetricsJobProcessor>());

        // --- Telemetry-aware Scheduler ---
        services.AddSingleton<MetricsJobScheduler>();
        services.AddHostedService(sp => sp.GetRequiredService<MetricsJobScheduler>());

        // --- OPTIONAL: OpenTelemetry Metrics exporter ---
        // You can enable this once you add OpenTelemetry packages.
        // See: https://github.com/open-telemetry/opentelemetry-dotnet (Metrics getting-started) [1](https://businessnetsolutionscouk-my.sharepoint.com/personal/alexander_businessnetsolutions_co_uk/Documents/Microsoft%20Copilot%20Chat%20Files/SqlJobProcessor.cs)
        //
        // services.AddOpenTelemetry()
        //     .WithMetrics(builder =>
        //     {
        //         builder
        //             .AddMeter("WJb.JobProcessor", "1.0.0")
        //             .AddMeter("WJb.JobScheduler", "1.0.0")
        //             .AddRuntimeInstrumentation()
        //             .AddProcessInstrumentation()
        //             .AddOtlpExporter(otlp =>
        //             {
        //                 otlp.Endpoint = new Uri("http://localhost:4318");
        //             });
        //     });
    })
    .Build();

// -------------------------------------------------------------
// Seed actions: one cron + one immediate job
// -------------------------------------------------------------
var factory = host.Services.GetRequiredService<IActionFactory>();

// Load a small action map (SayHello with cron every minute + default payload)

var actionsJson = JsonSerializer.Serialize(new Dictionary<string, ActionItem>
{
    ["SayHello"] = new ActionItem(
        "SayHello",
        new JsonObject
        {
            ["cron"] = "*/1 * * * *",
            ["priority"] = Priority.Normal.ToString(),  // <-- STRING, not number
            ["name"] = "Oleksandr"
        })
});

if (factory is IReloadableActionRegistry reloadable)
    reloadable.ReloadFromJson(actionsJson);

// Start host
await host.StartAsync();

// Demo: one immediate enqueue (so you see metrics right away)
var proc = host.Services.GetRequiredService<IJobProcessor>();
var job = await proc.CompactAsync("SayHello", new { name = "Viktor" });
await proc.EnqueueJobAsync(job, Priority.High);

// Compact & enqueue a sometimes-failing job
var job2 = await proc.CompactAsync("FailSometimes", new { should_fail = true });
await proc.EnqueueJobAsync(job2, Priority.Normal);


// Let scheduler tick a couple of times
await Task.Delay(TimeSpan.FromSeconds(75));

// Stop host
await host.StopAsync();


// ============================================================================
// Support types (kept here for a single-file demo). Split later if you like.
// ============================================================================

// Minimal reloadable settings registry (JsonObject-based)

public sealed class InMemorySettingsRegistry : IReloadableSettingsRegistry
{
    private JsonObject _values = new();

    public event Action? Reloaded;

    // 0.25.0-beta: Snapshot returns JsonObject (not dictionary)
    public JsonObject Snapshot() => (_values.DeepClone() as JsonObject)!;

    // 0.25.0-beta: Reload accepts JsonObject
    public void Reload(JsonObject newConfig)
    {
        _values = newConfig ?? new JsonObject();
        Reloaded?.Invoke();
    }

    public void ReloadFromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return;
        var node = JsonNode.Parse(json) as JsonObject;
        if (node is not null) Reload(node);
    }

    public void ReloadFromFile(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return;
        ReloadFromJson(File.ReadAllText(path));
    }

    public bool TryGetValue<T>(string key, out T value)
    {
        value = default!;
        if (_values.TryGetPropertyValue(key, out var node) && node is not null)
        {
            try
            {
                // robust conversion from JsonNode -> T
                value = node.Deserialize<T>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
                return true;
            }
            catch { /* ignore and return false */ }
        }
        return false;
    }

    public T GetValue<T>(string key, T defaultValue)
        => TryGetValue<T>(key, out var v) ? v : defaultValue;
}

// Correct IAction implementation (JsonObject? payload + cancellation)
public sealed class DemoAction : IAction
{
    private readonly string _type;
    public DemoAction(string type) => _type = type;

    public Task ExecAsync(JsonObject? more, CancellationToken stoppingToken = default)
    {
        var name = more.GetString("name") ?? "World"; // WJb.Extensions helper
        Console.WriteLine($"{_type} {name}!");
        return Task.CompletedTask;
    }
}

// Reloadable registry-backed ActionFactory (same pattern you used before)
public sealed class DemoActionFactory : IActionFactory, IReloadableActionRegistry
{
    private readonly Dictionary<string, ActionItem> _map =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["SayHello"] = new ActionItem("SayHello", new JsonObject { ["name"] = "Oleksandr" })
        };

    public IAction Create(string actionType) => new DemoAction(actionType);

    public ActionItem GetActionItem(string actionCode)
    {
        if (_map.TryGetValue(actionCode, out var item))
            return new ActionItem(item.Type, item.More?.DeepClone() as JsonObject);
        return new ActionItem(actionCode, more: null);
    }

    public event Action? Reloaded;
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

    public void ReloadFromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return;
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var parsed = JsonSerializer.Deserialize<Dictionary<string, ActionItem>>(json, options)
                     ?? new Dictionary<string, ActionItem>(StringComparer.OrdinalIgnoreCase);
        Reload(parsed);
    }

    public void ReloadFromFile(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return;
        ReloadFromJson(File.ReadAllText(path));
    }
}
