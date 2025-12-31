using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
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
    .ConfigureAppConfiguration(cfg =>
    {
        cfg.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
    })
    .ConfigureServices((ctx, services) =>
    {
        // Load actions from actions.json
        var json = File.ReadAllText("actions.json");
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var actions = JsonSerializer.Deserialize<Dictionary<string, ActionItem>>(json, options)
            ?? throw new InvalidOperationException("Failed to deserialize actions.json");

        // Register WJb with loaded actions
        services.AddWJb(actions: actions);
    })
    .Build();

var jobs = host.Services.GetRequiredService<IJobProcessor>();

await jobs.EnqueueJobAsync("MyAction", null);
await jobs.EnqueueJobAsync("MyAction", new { name = "Viktor"}, Priority.High);

await host.RunAsync();

public class MyAction : IAction
{
    private readonly string _fallback = "World";

    public Task ExecAsync(JsonObject? jobMore, CancellationToken stoppingToken)
    {
        var name = jobMore?.GetString("name") ?? _fallback;
        Console.WriteLine($"Hello {name}!");
        return Task.CompletedTask;
    }
}