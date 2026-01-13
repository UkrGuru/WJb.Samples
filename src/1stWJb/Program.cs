
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json.Nodes;
using WJb;
using WJb.Extensions;

// Build host
var host = Host.CreateDefaultBuilder(args)
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddSimpleConsole(o => { o.SingleLine = true; });
        logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.None);
    })
    .ConfigureServices(services =>
    {
        // Register WJb actions
        services.AddWJbActions(
            configureActions: map =>
            {
                map["MyAction"] = new ActionItem(
                    type: typeof(MyAction).AssemblyQualifiedName!,
                    more: new { name = "Oleksandr" }
                );
            });

        services.AddWJbBase(); // Core WJb services
    })
    .Build();


// Resolve the job processor service
var jobProcessor = host.Services.GetRequiredService<IJobProcessor>();

// Prepare & Enqueue default job
var defaultJob = await jobProcessor.CompactAsync("MyAction");
await jobProcessor.EnqueueJobAsync(defaultJob);

// Prepare & Enqueue override job
var overrideJob = await jobProcessor.CompactAsync("MyAction", new { name = "Viktor" });
await jobProcessor.EnqueueJobAsync(overrideJob, Priority.High);

// Run the host
await host.RunAsync();


// Custom action
public class MyAction : IAction
{
    private readonly string _name = "World"; // fallback

    public Task ExecAsync(JsonObject? jobMore, CancellationToken stoppingToken)
    {
        var name = jobMore.GetString("name") ?? _name;
        Console.WriteLine($"Hello {name}!");
        return Task.CompletedTask;
    }
}
