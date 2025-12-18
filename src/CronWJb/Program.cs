using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using WJb;
using WJb.Extensions;

Console.OutputEncoding = Encoding.UTF8;

var actions = new Dictionary<string, ActionItem>();

var host = Host.CreateDefaultBuilder(args)
    .ConfigureLogging(logging =>
    {
        // Optional: replace default providers to have full control
        logging.ClearProviders();

        logging.AddSimpleConsole(opt =>
        {
            opt.SingleLine = true;
            opt.TimestampFormat = "HH:mm:ss ";
        });

        // Hide logs from this category
        logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.None);
        logging.AddFilter("Microsoft.Extensions.Hosting", LogLevel.None);
    })
    .ConfigureServices((ctx, services) =>
    {

        // Load actions from actions.json (simple & success)
        var json = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "actions.json"));
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        actions = JsonSerializer.Deserialize<Dictionary<string, ActionItem>>(json, options)
            ?? throw new InvalidOperationException("Failed to deserialize actions.json");

        // Register WJb with loaded actions
        services.AddWJb(actions: actions);

        services.Configure<Dictionary<string, object>>(cfg => { cfg["MaxParallelJobs"] = 2; });

        // ALSO expose the actions map to DI for TimerEnqueuer
        services.AddSingleton<IReadOnlyDictionary<string, ActionItem>>(actions);

        services.AddHostedService<JobScheduler>();
    })
    .Build();

Console.WriteLine("CronWJb started. Waiting for cron ticks...");
foreach (var kv in actions)
{
    var cron = kv.Value.More?.GetString("cron") ?? "(none)";
    Console.WriteLine($" - {kv.Key}: {cron}");
}

await host.RunAsync();

// Minimal action for demo implements IAction with ExecAsync only
public sealed class DummyAction(ILogger<DummyAction> logger) : IAction
{
    private readonly ILogger<DummyAction> _logger = logger;
    public Task ExecAsync(JsonObject? jobMore, CancellationToken stoppingToken)
    {
        var message = jobMore?.GetString("message") ?? "Hello from DummyAction!";
        Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} → {message}");
        return Task.CompletedTask;
    }
}
