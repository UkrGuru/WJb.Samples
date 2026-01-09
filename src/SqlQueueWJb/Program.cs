
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SqlQueueWJb;
using UkrGuru.Sql;
using WJb;

// Host setup
var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(cfg =>
    {
        cfg.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
    })
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddSimpleConsole(o => { o.SingleLine = true; });
        logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.None);
        logging.AddFilter("Microsoft.Extensions.Hosting", LogLevel.None);
    })
    .ConfigureServices((ctx, services) =>
    {
        // 1) DB (UkrGuru.Sql)
        var conn = ctx.Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Missing ConnectionStrings:DefaultConnection");
        services.AddSql(conn, true);

        // 2) WJb settings for queue + processor
        services.Configure<Dictionary<string, object>>(opts =>
        {
            opts["MaxParallelJobs"] = 4;

            // Queue capacities (backpressure)
            opts["CapacityASAP"] = 200;
            opts["CapacityHigh"] = 150;
            opts["CapacityNormal"] = 150;
            opts["CapacityLow"] = 100;

            // Per-priority concurrency ceilings
            opts["MaxInFlightASAP"] = 4;
            opts["MaxInFlightHigh"] = 2;
            opts["MaxInFlightNormal"] = 2;
            opts["MaxInFlightLow"] = 1;

            // Weighted fairness
            opts["WeightASAP"] = 4;
            opts["WeightHigh"] = 2;
            opts["WeightNormal"] = 1;
            opts["WeightLow"] = 1;
        });

        // 3) WJb components (explicit)
        services.AddSingleton<IActionFactory, DemoActionFactory>();
        services.AddSingleton<IJobQueue, InMemoryJobQueue>(); // <-- REQUIRED for 0.22.0-beta

        // 4) SqlJobProcessor as the ONLY hosted processor
        services.AddSingleton<SqlJobProcessor>();
        services.AddSingleton<IJobProcessor>(sp => sp.GetRequiredService<SqlJobProcessor>());
        services.AddHostedService(sp => sp.GetRequiredService<SqlJobProcessor>());

        // (Optional) If you later host JobScheduler, register it here and ensure actions have "cron".
        // services.AddSingleton<JobScheduler>();
        // services.AddHostedService(sp => sp.GetRequiredService<JobScheduler>());
    })
    .Build();

await host.StartAsync();

// Demo enqueue
var proc = host.Services.GetRequiredService<IJobProcessor>();
var job = await proc.CompactAsync("SayHello", new { name = "Viktor" });
await proc.EnqueueJobAsync(job, Priority.High);

// Let background service drain queues
await Task.Delay(1000);
await host.StopAsync();
