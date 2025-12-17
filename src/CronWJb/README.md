# CronWJb – Cron-based scheduling (WJb / .NET 10)

A minimal console sample that schedules actions using **cron expressions** and executes them via **WJb**.

- **Target Framework:** `net10.0`
- **Packages:** `Microsoft.Extensions.Hosting (10.0.1)`, `Microsoft.Extensions.Logging.Console (10.0.1)`, `WJb (0.13.1-beta)`
- **Action Contract:** `IAction.ExecAsync(JsonObject? jobMore, CancellationToken)` — *no `InitAsync` in current WJb*

## Project layout
```
CronWJb/
├─ CronWJb.csproj
├─ Program.cs
├─ actions.json
```

## Run
```bash
cd CronWJb
 dotnet restore
 dotnet run
```
Expected startup output:
```
CronWJb started. Waiting for cron ticks...
 - HelloEveryMinute: */1 * * * *
 - Hello9to5Weekdays: */3 9-21 * * 1-5
```

Then you should see tick messages on schedule.

## Configuration
`actions.json` holds a map of **action code → ActionItem**. Each item includes the CLR `Type` and default `More` payload.

```json
{
  "HelloEveryMinute": {
    "Type": "DummyAction",
    "More": { "cron": "*/1 * * * *", "priority": 2, "message": "Minute tick ✅" }
  },
  "Hello9to5Weekdays": {
    "Type": "DummyAction",
    "More": { "cron": "*/3 9-21 * * 1-5", "priority": 3, "message": "Working hours ping (every 5 minutes, Mon–Fri)" }
  }
}
```

> **Cron format** is 5 fields: `minute hour day-of-month month day-of-week`.
> Examples: `*/1 * * * *` (every minute), `0 0 * * *` (midnight daily), `*/5 9-17 * * 1-3` (every 3 minutes, 9–21, Mon–Fri).

## Program.cs key points
- Ensures **UTF‑8 output** so emojis/non‑ASCII render correctly:
  ```csharp
  Console.OutputEncoding = Encoding.UTF8;
  ```
- Loads `actions.json` and registers WJb services:
  ```csharp
  var actions = JsonSerializer.Deserialize<Dictionary<string, ActionItem>>(json, options)!;
  services.AddWJb(actions: actions);
  services.AddHostedService<JobScheduler>();
  ```
- Minimal action implements only `ExecAsync`:
  ```csharp
  public sealed class DummyAction : IAction {
      public Task ExecAsync(JsonObject? jobMore, CancellationToken stoppingToken) { /* ... */ }
  }
  ```

## Manual enqueue (optional)
You can enqueue jobs programmatically using `IJobProcessor`:
```csharp
var processor = host.Services.GetRequiredService<IJobProcessor>();
var job = await processor.CompactAsync("DummyAction", new { message = "ASAP ping" });
await processor.EnqueueJobAsync(job, Priority.ASAP);
// or
await processor.EnqueueJobAsync("DummyAction", new { message = "High priority" }, Priority.High);
```

## Troubleshooting
- **Emoji shows as `?` or `␦`**: ensure `Console.OutputEncoding = Encoding.UTF8;` and, on Windows cmd, run `chcp 65001`. Windows Terminal and Linux/macOS are UTF‑8 by default.
- **Invalid cron**: hours must be `0–23`. Example `9-17` (not `9-31`).

## Notes
- WJb’s scheduler reads `More.cron` and enqueues jobs to the processor respecting priority and parallelism.
- The action `More` payload supports inheritance via `base` with **child‑wins** precedence (if you choose to use it).

## References
- WJb tutorials demonstrating scheduler + processor patterns (action map, DI, priority): see DEV posts. 
- Cron expression semantics in .NET (NCrontab): 5‑field format and examples.
