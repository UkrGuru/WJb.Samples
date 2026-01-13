# **MetricsWJb – Scheduled & Metrics Demo**

This sample demonstrates how to configure **scheduled actions**, enqueue jobs dynamically using the **WJb** library, and collect **OpenTelemetry metrics**. It also shows how to handle jobs with different priorities and simulate occasional failures.

***

## ✅ **Description**

*   Defines actions with **cron schedules** and **metadata**.
*   Demonstrates **priority-based job enqueueing**.
*   Includes an example of a job that may fail sometimes.
*   Integrates **OpenTelemetry Metrics** for observability.

***

### **1. Service Registration**

```csharp
// Register MetricsJobProcessor using new ctor order: (queue, factory, settingsRegistry, logger)
services.AddSingleton<MetricsJobProcessor>(sp => new MetricsJobProcessor(
    sp.GetRequiredService<IJobQueue>(),
    sp.GetRequiredService<IActionFactory>(),
    sp.GetRequiredService<IReloadableSettingsRegistry>(),
    sp.GetRequiredService<ILogger<MetricsJobProcessor>>()));

// Alias to IJobProcessor and host a single instance
services.AddSingleton<IJobProcessor>(sp => sp.GetRequiredService<MetricsJobProcessor>());
services.AddHostedService(sp => sp.GetRequiredService<MetricsJobProcessor>());

// --- OpenTelemetry Metrics: Console exporter ---
services.AddOpenTelemetry()
    .WithMetrics(builder =>
    {
        builder
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(
                serviceName: "WJb.Sample",
                serviceVersion: "1.0.0"))
            .AddMeter("WJb.JobProcessor") // Add actual meter names used by MetricsJobProcessor
            .AddRuntimeInstrumentation()   // Requires OpenTelemetry.Instrumentation.Runtime
            .AddProcessInstrumentation()   // Requires OpenTelemetry.Instrumentation.Process
            .AddConsoleExporter(o =>
            {
                // Optional: flush every 5s for faster feedback during development
                o.MetricReaderType = MetricReaderType.Periodic;
                o.PeriodicExportingMetricReaderOptions = new PeriodicExportingMetricReaderOptions
                {
                    ExportIntervalMilliseconds = 5000
                };
            });
    });
```

***

### **2. Execute a Job**

```csharp
// Use processor
var proc = host.Services.GetRequiredService<IJobProcessor>();
var job = await proc.CompactAsync("SayHello", new { name = "Viktor" });
await proc.EnqueueJobAsync(job, Priority.High);
```

***

Additionally, OpenTelemetry will periodically output metrics such as:

*   `jobs_started`, `jobs_completed`, `job_duration_ms` (custom WJb metrics)
*   `process.memory.usage`, `dotnet.gc.collections` (runtime/process metrics)

***

