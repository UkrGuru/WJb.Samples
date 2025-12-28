
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
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
            opt.TimestampFormat = "HH:mm:ss ";
        });

        // Hide logs from this category
        logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.None);
        logging.AddFilter("Microsoft.Extensions.Hosting", LogLevel.None);
    })
    .ConfigureServices((ctx, services) =>
    {

        // Register action type(s)
        services.AddTransient<MyTimerAction>();

        // Load actions from actions.json (simple & success)
        var json = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "actions.json"));
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var actions = JsonSerializer.Deserialize<Dictionary<string, ActionItem>>(json, options)
            ?? throw new InvalidOperationException("Failed to deserialize actions.json");

        // Register WJb with loaded actions
        services.AddWJb(actions: actions);

        // ALSO expose the actions map to DI for TimerEnqueuer
        services.AddSingleton<IReadOnlyDictionary<string, ActionItem>>(actions);

        // Periodic enqueuer that uses intervalMs from action defaults
        services.AddHostedService<TimerEnqueuer>();
    })
    .Build();

await host.RunAsync();

public sealed class TimerEnqueuer(ILogger<TimerEnqueuer> logger, IJobProcessor jobs, IReadOnlyDictionary<string, ActionItem> actions) : BackgroundService
{
    private readonly ILogger<TimerEnqueuer> _logger = logger;
    private readonly IJobProcessor _jobs = jobs;
    private readonly IReadOnlyDictionary<string, ActionItem> _actions = actions;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("TimerWJb started (actions: {Count})", _actions.Count);

        var tasks = new List<Task>();
        foreach (var (code, item) in _actions)
        {
            var more = item.More as JsonObject;
            var intervalMs = more?.TryGetPropertyValue("intervalMs", out var node) == true ? node!.GetValue<int>() : 1000;
            tasks.Add(RunPeriodicAsync(code, TimeSpan.FromMilliseconds(intervalMs), stoppingToken));
        }
        await Task.WhenAll(tasks);
    }

    private async Task RunPeriodicAsync(string code, TimeSpan period, CancellationToken ct)
    {
        using var timer = new PeriodicTimer(period);
        while (await timer.WaitForNextTickAsync(ct))
        {
            _logger.LogInformation("Enqueue: {Code} (period={Period}ms)", code, (int)period.TotalMilliseconds);
            await _jobs.EnqueueJobAsync(code, new { priority = Priority.Normal.ToString() } );
        }
    }
}

public class MyTimerAction(ILogger<MyTimerAction> logger) : IAction
{
    private readonly ILogger<MyTimerAction> _logger = logger;

    public async Task ExecAsync(JsonObject? jobMore, CancellationToken stoppingToken)
    {
        var items = jobMore.GetArray("items");
        if (items is null || items.Count == 0) { _logger.LogInformation("TimerWJb: No items to process."); return; }

        foreach (var node in items)
        {
            if (stoppingToken.IsCancellationRequested) break;
            var value = node?.GetValue<string>();
            _logger.LogInformation("TimerWJb tick: {Item}", value);
            await Task.Delay(100, stoppingToken);
        }
        _logger.LogInformation("TimerWJb: Done.");
    }
}
