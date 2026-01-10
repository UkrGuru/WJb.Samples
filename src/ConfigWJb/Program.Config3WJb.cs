
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json.Nodes;
using WJb;
using WJb.Extensions;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddSimpleConsole(opt => opt.SingleLine = true);
        logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.None);
    })
    .ConfigureServices((ctx, services) =>
    {
        var actions = new Dictionary<string, ActionItem>
        {
            ["MyAction"] = new ActionItem
            {
                Type = "MyAction, ConfigWJb",
                More = new JsonObject { ["name"] = "Oleksandr" }
            }
        };
        services.AddWJbActions(actions).AddWJbBase();
    })
    .Build();

var proc = host.Services.GetRequiredService<IJobProcessor>();
await proc.EnqueueJobAsync(await proc.CompactAsync("MyAction"));
await proc.EnqueueJobAsync(await proc.CompactAsync("MyAction", new { name = "Viktor" }), Priority.High);
await host.RunAsync();

public class MyAction : IAction
{
    private readonly string _fallback = "World";

    public Task ExecAsync(JsonObject? jobMore, CancellationToken stoppingToken)
    {
        var name = jobMore.GetString("name") ?? _fallback;
        Console.WriteLine($"Hello {name}!");
        return Task.CompletedTask;
    }
}
