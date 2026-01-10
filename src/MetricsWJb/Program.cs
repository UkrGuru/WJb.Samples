
using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json.Nodes;
using WJb;

using var host = Host.CreateDefaultBuilder(args)
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddSimpleConsole(o => o.SingleLine = true);
    })
    .ConfigureServices(services =>
    {

        // Settings (optional)
        services.Configure<Dictionary<string, object>>(opts =>
        {
            opts["MaxParallelJobs"] = 2; // or whatever you prefer
        });

        // Register a queue implementation
        services.AddSingleton<IJobQueue, InMemoryJobQueue>();

        services.AddTransient<FailSometimesAction>();
        services.AddTransient<GreetDoneAction>(); // <-- must be present
        services.AddSingleton<IActionFactory, DemoActionFactory>();

        services.AddSingleton<JobProcessor>();
        services.AddSingleton<IJobProcessor>(sp => sp.GetRequiredService<JobProcessor>());
        services.AddHostedService(sp => sp.GetRequiredService<JobProcessor>());
        services.AddTransient<SayHelloAction>(); // constructor now takes IJobProcessor – DI will resolve it

        // --- Minimal Metrics Console Listener (no exporter required) ---
        services.AddSingleton<IMetricsPrinter, MetricsPrinter>();
        services.AddHostedService<MetricsListenerHostedService>();
    })
    .Build();

await host.StartAsync();

var proc = host.Services.GetRequiredService<IJobProcessor>();

// Compact & enqueue a success job
var job1 = await proc.CompactAsync("SayHello", new { name = "Oleksandr" });
await proc.EnqueueJobAsync(job1, Priority.High);

// Compact & enqueue a sometimes-failing job
var job2 = await proc.CompactAsync("FailSometimes", new { should_fail = true });
await proc.EnqueueJobAsync(job2, Priority.Normal);


// Allow background processor to run
await Task.Delay(1500);

await host.StopAsync();

// ============== Support classes ==============

public interface IMetricsPrinter { void Print(string name, double? value, ReadOnlySpan<KeyValuePair<string, object?>> tags); }

public class MetricsPrinter : IMetricsPrinter
{
    public void Print(string name, double? value, ReadOnlySpan<KeyValuePair<string, object?>> tags)
    {
        var tagsStr = string.Join(", ", tags.ToArray().Select(t => $"{t.Key}={t.Value}"));
        if (value.HasValue)
            Console.WriteLine($"metric: {name} value={value:F2} tags=[{tagsStr}]");
        else
            Console.WriteLine($"metric: {name} tags=[{tagsStr}]");
    }
}

public class MetricsListenerHostedService : IHostedService
{
    private readonly IMetricsPrinter _printer;
    private MeterListener? _listener;

    public MetricsListenerHostedService(IMetricsPrinter printer) => _printer = printer;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _listener = new MeterListener
        {
            InstrumentPublished = (inst, listener) =>
            {
                if (inst.Meter.Name == "WJb.JobProcessor") // the meter used in JobProcessor
                    listener.EnableMeasurementEvents(inst);
            }
        };

        _listener.SetMeasurementEventCallback<long>((inst, value, tags, state) =>
        {
            _printer.Print(inst.Name, null, tags);
        });

        _listener.SetMeasurementEventCallback<double>((inst, value, tags, state) =>
        {
            _printer.Print(inst.Name, value, tags);
        });

        _listener.Start();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _listener?.Dispose();
        return Task.CompletedTask;
    }
}
