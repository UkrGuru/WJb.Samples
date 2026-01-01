
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
            //o.TimestampFormat = "yyyy-MM-dd HH:mm:ss.fff ";
        });

        // Hide logs from this category
        logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.None);
        logging.AddFilter("Microsoft.Extensions.Hosting", LogLevel.None);
    })
    .ConfigureServices(services =>
    {
        // register our demo ActionFactory
        services.AddSingleton<IActionFactory, DemoActionFactory>();

        // register LogJobProcessor (BackgroundService + IJobProcessor)
        services.AddSingleton<IJobProcessor, LogJobProcessor>();
        services.AddHostedService(sp => (LogJobProcessor)sp.GetRequiredService<IJobProcessor>());
    })
    .Build();

await host.StartAsync();

// Resolve processor for demo enqueues
var proc = host.Services.GetRequiredService<IJobProcessor>();

var job = await proc.CompactAsync("SayHello", new { name = "Viktor" });
await proc.EnqueueJobAsync(job, Priority.High);

// Let background service drain queues
await Task.Delay(1000);

await host.StopAsync();
