using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Nodes;
using WJb;
using WJb.Extensions;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(cfg =>
    {
        cfg.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
    })
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddSimpleConsole(opt => opt.SingleLine = true);
        logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.None);
    })
    .ConfigureServices((ctx, services) =>
    {
        var json = JsonSerializer.Serialize(ctx.Configuration.GetSection("WJb:Actions").Get<Dictionary<string, ActionItemDto>>() ?? []);
        var actions = JsonSerializer.Deserialize<Dictionary<string, ActionItem>>(json, 
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];
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

public sealed class ActionItemDto
{
    public string? Type { get; set; }
    public Dictionary<string, object>? More { get; set; }
}
