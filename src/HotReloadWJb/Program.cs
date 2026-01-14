
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json.Nodes;
using WJb;
using WJb.Extensions;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureLogging(b => b
        .ClearProviders()
        .AddSimpleConsole(o => o.TimestampFormat = "HH:mm:ss "))
    .ConfigureServices(services =>
    {
        // Live actions dictionary (used for initial load and interactive modifications)
        var initialActions = new Dictionary<string, ActionItem>(StringComparer.OrdinalIgnoreCase)
        {
            ["Ping"] = new ActionItem
            {
                Type = "PingAction, HotReloadWJb",
                More = new JsonObject { ["cron"] = "* * * * *", ["priority"] = "High" }
            }
        };

        services
            .AddSingleton(initialActions)   // optional – access to the live map for toggling
            .AddWJb(initialActions, addScheduler: true);
    });

var host = builder.Build();
await host.StartAsync();

Console.WriteLine("\n=== HotReloadWJb Sample ===\n");
Console.WriteLine("Commands:");
Console.WriteLine(" [E] - Enqueue Ping action manually");
Console.WriteLine(" [R] - Toggle cron for Ping (triggers hot-reload)");
Console.WriteLine(" [S] - Reload actions from file (actions.json)");
Console.WriteLine(" [Q] - Quit\n");

var services = host.Services;

// Get required services
var liveActionsDict = services.GetRequiredService<Dictionary<string, ActionItem>>(); // if you registered it
var actionRegistry = services.GetRequiredService<IReloadableActionRegistry>();
var jobProcessor = services.GetRequiredService<IJobProcessor>();
var jobQueue = services.GetRequiredService<IJobQueue>();
var jobScheduler = services.GetRequiredService<JobScheduler>();
var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("ConsoleSample");

while (true)
{
    Console.Write("> ");
    var key = Console.ReadKey(true).Key;
    Console.WriteLine();

    if (key == ConsoleKey.Q) break;

    try
    {
        switch (key)
        {
            case ConsoleKey.E: // Enqueue Ping manually
                {
                    var job = await jobProcessor.CompactAsync("Ping", null);
                    liveActionsDict.TryGetValue("Ping", out var item);
                    var priority = item?.More?.GetPriority() ?? Priority.Normal;
                    await jobQueue.EnqueueAsync(job, priority);
                    logger.LogInformation("Manually enqueued Ping (priority: {Priority})", priority);
                    break;
                }

            case ConsoleKey.R: // Toggle cron for Ping → hot reload
                {
                    if (!liveActionsDict.TryGetValue("Ping", out var pingItem))
                    {
                        logger.LogWarning("Action 'Ping' not found");
                        break;
                    }

                    var more = pingItem.More ??= new JsonObject();
                    if (more.ContainsKey("cron"))
                    {
                        more.Remove("cron");
                        logger.LogInformation("Cron removed from Ping → no more automatic executions");
                    }
                    else
                    {
                        more["cron"] = "* * * * *";
                        logger.LogInformation("Cron enabled for Ping → every minute");
                    }

                    // Trigger reload so scheduler immediately sees the change
                    actionRegistry.Reload(liveActionsDict);
                    logger.LogInformation("Actions registry reloaded (via direct dictionary)");
                    break;
                }

            case ConsoleKey.S: // Reload from actions.json file
                {
                    Console.Write("Enter path to actions.json (or press Enter to skip): ");
                    var actionsPath = Console.ReadLine()?.Trim();
                    if (string.IsNullOrWhiteSpace(actionsPath))
                    {
                        logger.LogInformation("No path provided → reload skipped");
                        break;
                    }

                    try
                    {
                        actionRegistry.ReloadFromFile(actionsPath);
                        logger.LogInformation("Actions successfully reloaded from: {Path}", actionsPath);

                        // Optional: force scheduler reload (immediate effect without waiting for event)
                        // await jobScheduler.ReloadAsync();
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to reload actions from file");
                    }
                    break;
                }

            default:
                Console.WriteLine("Unknown command. Use E, R, S or Q");
                break;
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error during command execution");
    }
}

Console.WriteLine("\nStopping host...");
await host.StopAsync();
Console.WriteLine("Goodbye!");

// Sample PingAction
public sealed class PingAction : IAction
{
    public Task ExecAsync(JsonObject? jobMore, CancellationToken stoppingToken)
    {
        Console.WriteLine($"PingAction executed. priority={jobMore.GetPriority()}");
        return Task.CompletedTask;
    }

    public Task NextAsync(JsonObject more, CancellationToken stoppingToken) => Task.CompletedTask;
}

