
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json.Nodes;
using WJb;
using WJb.Extensions;

// Create and configure a generic host for the application
using var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        // Register DummyAction as a transient service (new instance per request)
        services.AddTransient<DummyAction>();

        // Configure WJb (Worker Jobs) settings and actions
        services.AddWJb(
            configureSettings: opts =>
            {
                // Limit concurrent job execution to 1
                // This controls the internal job processor's parallelism
                opts["MaxParallelJobs"] = 1;
            },
            configureActions: map =>
            {
                // Register a job action named "Dummy"
                map["Dummy"] = new ActionItem(
                    type: typeof(DummyAction).AssemblyQualifiedName!, // Fully qualified type name of DummyAction

                    more: new
                    {
                        cron = "* * * * *",              // Schedule: run every minute
                        priority = (int)Priority.Normal // Job priority level
                    }
                );
            });

        // Register JobScheduler as a hosted background service
        services.AddHostedService<JobScheduler>();
    })
    .ConfigureLogging(logging =>
    {
        // Configure logging: only console output, minimal level = Error
        logging.ClearProviders();
        logging.AddConsole();
        logging.SetMinimumLevel(LogLevel.Error); // Reduce log noise
    })
    .Build();

// Inform user that the scheduler is running
Console.WriteLine("Scheduler running at the start of each minute. Await please...");

// Start the host and run background services
await host.RunAsync();

// ------------------------------
// Dummy action used by scheduler
// ------------------------------
public sealed class DummyAction(ILogger<DummyAction> logger) : IAction
{
    private readonly ILogger<DummyAction> _logger = logger;
    private JsonObject _jobMore = new();

    // ExecAsync: main execution logic for the job
    public Task ExecAsync(dynamic? jobMore, CancellationToken stoppingToken)
    {
        // Capture job-specific parameters (if any)
        _jobMore = jobMore ?? new JsonObject();
        Console.WriteLine("DummyAction.InitAsync. jobMore={0}", _jobMore.ToJsonString());

        // Retrieve custom message or use default
        var message = _jobMore.GetString("message") ?? "Hello from DummyAction!";
        Console.WriteLine("DummyAction.ExecAsync: {0} at {1}", message, DateTime.Now);

        return Task.CompletedTask; // No async work here, so return completed        return Task.CompletedTask; // No async work here, so return completed task
    }
}