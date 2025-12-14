
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Nodes;
using WJb;
using WJb.Extensions;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        // --- Load actions from actions.json ---
        var json = File.ReadAllText("actions.json");
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        // Deserialize to dictionary: { "ActionName": ActionItem }
        var actions = JsonSerializer.Deserialize<Dictionary<string, ActionItem>>(json, options)
                      ?? throw new InvalidOperationException("Failed to deserialize actions.json into ActionItem dictionary.");

        // Configure WJb and inject the loaded actions
        services.AddWJb(actions: actions);

        // Background scheduler
        services.AddHostedService<JobScheduler>();
    })
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddConsole();
        logging.SetMinimumLevel(LogLevel.Error);
    });

using var host = builder.Build();

Console.WriteLine("Scheduler running at the start of each minute. Await please...");
await host.RunAsync();

// ------------------------------
// Dummy action used by scheduler
// ------------------------------
public sealed class DummyAction(ILogger<DummyAction> logger) : IAction
{
    private readonly ILogger<DummyAction> _logger = logger;

    public Task ExecAsync(JsonObject? jobMore, CancellationToken stoppingToken)
    {
        var message = jobMore.GetString("message") ?? "Hello from DummyAction!";
        Console.WriteLine("DummyAction.ExecAsync: {0} at {1}", message, DateTime.Now);
        return Task.CompletedTask;
    }
}
