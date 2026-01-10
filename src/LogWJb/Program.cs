using LogWJb;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WJb;



// Host setup
var host = Host.CreateDefaultBuilder(args)
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddSimpleConsole(o =>
        {
            o.SingleLine = true;
        });
        logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.None);
    })
    .ConfigureServices(services =>
    {
        // 🔥 Required because jobProcessor=false suppresses queue registration
        services.AddSingleton<IJobQueue, InMemoryJobQueue>();

        // Register your ActionFactory
        services.AddSingleton<IActionFactory, DemoActionFactory>();

        // Register your processor + hosted service
        services.AddSingleton<LogJobProcessor>();
        services.AddSingleton<IJobProcessor>(sp => sp.GetRequiredService<LogJobProcessor>());
        services.AddHostedService(sp => sp.GetRequiredService<LogJobProcessor>());
    })
    .Build();

// Use processor
var proc = host.Services.GetRequiredService<IJobProcessor>();

var job = await proc.CompactAsync("SayHello", new { name = "Viktor" });
await proc.EnqueueJobAsync(job, Priority.High);

// Start the hosted service infrastructure (e.g., workers, background processing).
await host.RunAsync();
