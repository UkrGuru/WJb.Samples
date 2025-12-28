
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json.Nodes;
using WJb;
using WJb.Extensions;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureLogging(logging =>
    {
        // Optional: replace default providers to have full control
        logging.ClearProviders();

        logging.AddSimpleConsole(opt =>
        {
            opt.SingleLine = true;
            opt.TimestampFormat = "HH:mm:ss.fff ";
        });

        // Hide logs from this category
        logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.None);
        logging.AddFilter("Microsoft.Extensions.Hosting", LogLevel.None);
    })
    .ConfigureServices(services =>
    {
        // Register action
        services.AddTransient<MyQueueAction>();

        // Register WJb with actions map via DI
        var actions = new Dictionary<string, ActionItem>(StringComparer.OrdinalIgnoreCase)
        {
            ["MyQueueAction"] = new ActionItem(
                type: typeof(MyQueueAction).AssemblyQualifiedName!,
                more: new { items = new[] { "A", "B", "C" } }
            )
        };
        services.AddSingleton(actions);
        services.AddWJb();
    })
    .Build();

var jobs = host.Services.GetRequiredService<IJobProcessor>();

// 1) Use defaults → processes A, B, C
await jobs.EnqueueJobAsync("MyQueueAction", new JsonObject() );

// 2) Override defaults → processes X, Y
await jobs.EnqueueJobAsync("MyQueueAction", new { items = new[] { "X", "Y" }, priority = Priority.High.ToString() });

await host.RunAsync();

public class MyQueueAction(ILogger<MyQueueAction> logger) : IAction
{
    private readonly ILogger<MyQueueAction> _logger = logger;

    public Task ExecAsync(JsonObject? jobMore, CancellationToken stoppingToken)
    {
        var items = jobMore.GetArray("items");
        if (items is null || items.Count == 0)
        {
            _logger.LogInformation("QueueWJb: No items to process.");
            return Task.CompletedTask;
        }

        foreach (var node in items)
        {
            if (stoppingToken.IsCancellationRequested) break;

            var value = node?.GetValue<string>();
            _logger.LogInformation("Processing item: {Item}", value);

            Thread.Sleep(100);
        }

        _logger.LogInformation("QueueWJb: Done.");
        return Task.CompletedTask;
    }
}
