using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using WJb;
using WJb.Extensions;

Console.OutputEncoding = Encoding.UTF8;

var actions = new Dictionary<string, ActionItem>();

var host = Host.CreateDefaultBuilder(args)
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddSimpleConsole(opt =>
        {
            opt.SingleLine = true;
            opt.TimestampFormat = "HH:mm:ss ";
        });
        logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.None);
        logging.AddFilter("Microsoft.Extensions.Hosting", LogLevel.None);
    })
    .ConfigureServices((ctx, services) =>
    {
        // Load actions map
        var jsonPath = Path.Combine(AppContext.BaseDirectory, "actions.json");

        // WJb + Processor settings
        services.AddWJb(actions: ActionMapLoader.CreateFromPath(jsonPath));
        services.Configure<Dictionary<string, object>>(cfg => { cfg["MaxParallelJobs"] = 2; });

        // Expose action map if needed by listeners
        //services.AddSingleton<IReadOnlyDictionary<string, ActionItem>>(actions);

        // Event listener hosted service
        services.AddHostedService<FileEventListener>();
    })
    .Build();

Console.WriteLine("EventWJb started. Drop JSON files into the 'events' folder to trigger jobs.");
Console.WriteLine("Loaded actions:");
foreach (var kv in actions)
{
    var more = kv.Value.More?.ToJsonString();
    Console.WriteLine($" - {kv.Key}: {more}");
}

await host.RunAsync();

// IAction: Exec-only (current WJb)
public sealed class DummyAction(ILogger<DummyAction> logger) : IAction
{
    private readonly ILogger<DummyAction> _logger = logger;
    public Task ExecAsync(JsonObject? jobMore, CancellationToken stoppingToken)
    {
        var message = jobMore?.GetString("message") ?? "Hello from DummyAction!";
        var origin  = jobMore?.GetString("origin")  ?? "(unknown origin)";
        Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} → {message} [origin: {origin}]");
        return Task.CompletedTask;
    }
}

// Hosted service: listens for JSON event files and enqueues jobs
public sealed class FileEventListener(ILogger<FileEventListener> logger, IJobProcessor processor) : BackgroundService
{
    private readonly ILogger<FileEventListener> _logger = logger;
    private readonly IJobProcessor _processor = processor;
    private readonly string _eventsDir = Path.Combine(AppContext.BaseDirectory, "events");

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Directory.CreateDirectory(_eventsDir);
        _logger.LogInformation("Watching events in {dir}", _eventsDir);

        // Seed a demo event file
        var demoFile = Path.Combine(_eventsDir, "demo-event.json");
        if (!File.Exists(demoFile))
        {
            var demo = new JsonObject
            {
                ["code"] = "OnBusinessEvent",
                ["more"] = new JsonObject
                {
                    ["message"] = "Business event arrived",
                    ["origin"]  = "seed"
                }
            };
            await File.WriteAllTextAsync(demoFile, demo.ToJsonString(new JsonSerializerOptions{WriteIndented=true}), stoppingToken);
        }

        using var fsw = new FileSystemWatcher(_eventsDir, "*.json")
        {
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.Size | NotifyFilters.LastWrite,
            IncludeSubdirectories = false,
            EnableRaisingEvents = true
        };

        fsw.Created += async (_, e) => await HandleFileAsync(e.FullPath, stoppingToken);
        fsw.Changed += async (_, e) => await HandleFileAsync(e.FullPath, stoppingToken);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
                await Task.Delay(1000, stoppingToken);
        }
        catch (OperationCanceledException) { }
    }

    private async Task HandleFileAsync(string path, CancellationToken stoppingToken)
    {
        // Retry a few times until the file is ready
        for (int i = 0; i < 10; i++)
        {
            try
            {
                if (!File.Exists(path)) return;
                var text = await File.ReadAllTextAsync(path, stoppingToken);
                if (string.IsNullOrWhiteSpace(text)) return;

                if (text.TrimStart().StartsWith("{"))
                {
                    // JSON event: { code|actionCode, more }
                    var node = JsonNode.Parse(text) as JsonObject;
                    var code = node?.GetString("code") ?? node?.GetString("actionCode");
                    var moreNode = node?["more"] as JsonObject ?? new JsonObject();
                    if (code is null)
                    {
                        _logger.LogWarning("Event file {file} missing 'code'/'actionCode'", path);
                        return;
                    }

                    _logger.LogInformation("Enqueue {code} from file {file}", code, path);
                    var job = await _processor.CompactAsync(code, moreNode, stoppingToken);
                    await _processor.EnqueueJobAsync(job, stoppingToken: stoppingToken);
                }
                else
                {
                    // Raw compact job string
                    _logger.LogInformation("Process raw job from file {file}", path);
                    await _processor.ProcessJobAsync(text, stoppingToken: stoppingToken);
                }
                return;
            }
            catch (IOException)
            {
                await Task.Delay(50, stoppingToken);
            }
            catch (UnauthorizedAccessException)
            {
                await Task.Delay(50, stoppingToken);
            }
        }
        _logger.LogWarning("Failed to process event file {file}", path);
    }
}
