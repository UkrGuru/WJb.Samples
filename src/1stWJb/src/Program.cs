
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
            });

            // Hide logs from this category
            logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.None);
            logging.AddFilter("Microsoft.Extensions.Hosting", LogLevel.None);
        })
    // Configure DI container and hosted environment
    .ConfigureServices(services =>
    {
        // Register your action (the handler class that implements IAction).
        // Lifetime can be Transient (stateless, per-exec), Scoped, or Singleton as you prefer.
        services.AddTransient<MyAction>();

        // Register WJb infrastructure:
        // - configureSettings: runtime tunables (e.g., parallelism, delays, etc.)
        // - configureActions: map of action codes to ActionItem (type + default More)
        services.AddWJb(
            configureSettings: opts =>
            {
                // Limit concurrent job execution to 1
                // (applies to the internal job processor / worker pool)
                opts["MaxParallelJobs"] = 1;
            },
            configureActions: map =>
            {
                // Map an action code to its handler type and optional default "More" payload.
                // The code "MyAction" must match what you pass to EnqueueJobAsync.
                map["MyAction"] = new ActionItem(
                    // Fully qualified type name so the runtime can instantiate it via DI.
                    type: typeof(MyAction).AssemblyQualifiedName!,

                    // Default payload for this action.
                    more: new { name = "Oleksandr" }
                );
            });
    })
    .Build();

var jobs = host.Services.GetRequiredService<IJobProcessor>();

// 1) Enqueue a job using ONLY defaults -> prints "Oleksandr"
await jobs.EnqueueJobAsync("MyAction", null);

// 2) Enqueue a job that overrides default -> prints "Viktor"
await jobs.EnqueueJobAsync("MyAction", new { name = "Viktor" }, Priority.High);

// Start the hosted service infrastructure (e.g., workers, background processing).
await host.RunAsync();

/// <summary>
/// An example action that prints "Hello {name}!".
/// </summary>
/// <remarks>
/// Expected job payload (More) includes a "name" key. If not provided,
/// the action falls back to its internal default value ("World").
/// </remarks>
public class MyAction : IAction
{
    // Fallback if neither mapped defaults nor job payload provide "name"
    private readonly string _name = "World";

    /// <summary>
    /// Executes the action logic.
    /// </summary>
    /// <param name="jobMore">
    /// Dynamic payload passed by the job processor. It can be:
    /// - null: use mapped defaults or local fallback
    /// - anonymous object / POCO / JsonObject: will be converted to JsonObject
    /// </param>
    /// <param name="stoppingToken">Cancellation token for cooperative cancellation.</param>
    /// <returns>Completed task when the action finishes.</returns>
    /// <remarks>
    /// Signature matches <see cref="IAction.ExecAsync"/> requirements: (dynamic? more, CancellationToken).
    /// Use <see cref="MoreExtensions.ToJsonObject(object?)"/> to safely normalize arbitrary payloads.
    /// </remarks>
    public Task ExecAsync(JsonObject? jobMore, CancellationToken stoppingToken)
    {
        // Resolve "name" from payload; if absent, use the local fallback
        var name = jobMore.GetString("name") ?? _name;

        // Example side effect (replace with your business logic)
        Console.WriteLine($"Hello {name}!");

        return Task.CompletedTask;
    }
}
