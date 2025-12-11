using Actions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json.Nodes;
using WJb;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddTransient<MyAction>();

        services.AddWJb(
            configureSettings: opts => opts["MaxParallelJobs"] = 2,
            configureActions: map =>
            {
                map["MyAction"] = new ActionItem(
                    Type: typeof(MyAction).AssemblyQualifiedName!,
                    More: new JsonObject { ["name"] = "Oleksandr" }
                );
            });
    })
    .Build();

var jobs = host.Services.GetRequiredService<IJobProcessor>();

// 1) Job that uses ONLY defaults -> prints "Oleksandr"
var jobDefault = await jobs.CompactAsync("MyAction", new JsonObject()); // or pass null if allowed
await jobs.EnqueueJobAsync(jobDefault, Priority.High);

// 2) Job that overrides -> prints "Alexander"
var jobOverride = await jobs.CompactAsync("MyAction", new JsonObject { ["name"] = "Alexander" });
await jobs.EnqueueJobAsync(jobOverride, Priority.Normal);

await host.RunAsync();
