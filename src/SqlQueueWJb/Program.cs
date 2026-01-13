
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SqlQueueWJb;
using System.Text.Json;
using System.Text.Json.Nodes;
using UkrGuru.Sql;
using WJb;
using WJb.Extensions;

// Host setup
var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(cfg =>
    {
        cfg.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
    })
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddSimpleConsole(o => o.SingleLine = true);
        //logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.None);
        //logging.AddFilter("Microsoft.Extensions.Hosting", LogLevel.None);
    })
    .ConfigureServices((ctx, services) =>
    {
        // 1) DB
        var conn = ctx.Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Missing connection string");
        services.AddSql(conn);

        // 2) WJb components (same as in LogWJb)
        services.AddSingleton<IActionFactory, DemoActionFactory>();
        services.AddSingleton<IJobQueue, InMemoryJobQueue>();

        services.AddSingleton<IReloadableSettingsRegistry, ReloadableSettingsRegistry>();

        // 3) SqlJobProcessor (same pattern as LogJobProcessor)
        services.AddSingleton<SqlJobProcessor>(sp => new SqlJobProcessor(
            sp.GetRequiredService<IJobQueue>(),
            sp.GetRequiredService<IActionFactory>(),
            sp.GetRequiredService<IReloadableSettingsRegistry>(),
            sp.GetRequiredService<ILogger<SqlJobProcessor>>(),
            sp.GetRequiredService<IDbService>()
        ));

        services.AddSingleton<IJobProcessor>(sp => sp.GetRequiredService<SqlJobProcessor>());
        services.AddHostedService(sp => sp.GetRequiredService<SqlJobProcessor>());
    })
    .Build();

// === Use processor ===
var proc = host.Services.GetRequiredService<IJobProcessor>();

var job = await proc.CompactAsync("SayHello", new { name = "Viktor" });
await proc.EnqueueJobAsync(job, Priority.High);

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
