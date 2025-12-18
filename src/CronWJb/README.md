# CronWJb – Cron-based scheduling with WJb (.NET 10)

A tiny console app that runs **actions on cron schedules** using WJb.

## ✅ Requirements

*   **Target:** `net10.0`
*   **Packages:**  
    `Microsoft.Extensions.Hosting`, `Microsoft.Extensions.Logging.Console`, `WJb`

## ▶️ Run

```bash
dotnet run
```

Startup:

    CronWJb started. Waiting for cron ticks...
     - HelloEveryMinute: */1 * * * *
     - Hello9to5Weekdays: */3 9-21 * * 1-5

## 📂 Layout

    CronWJb/
    ├─ Program.cs
    ├─ JobScheduler.cs
    ├─ actions.json

## ⚙️ actions.json

```json
{
  "HelloEveryMinute": {
    "Type": "DummyAction",
    "More": { "cron": "*/1 * * * *", "message": "Minute tick ✅" }
  },
  "Hello9to5Weekdays": {
    "Type": "DummyAction",
    "More": { "cron": "*/3 9-21 * * 1-5", "message": "Working hours ping" }
  }
}
```

> **Cron format:** `minute hour day month weekday`  
> Examples: `*/1 * * * *` (every minute), `0 0 * * *` (midnight daily).

## 🔑 Key points

*   UTF‑8 output:
    ```csharp
    Console.OutputEncoding = Encoding.UTF8;
    ```
*   Register WJb + scheduler:
    ```csharp
    services.AddWJb(actions);
    services.AddHostedService<JobScheduler>();
    ```
*   Minimal action:
    ```csharp
    public sealed class DummyAction : IAction {
        public Task ExecAsync(JsonObject? more, CancellationToken ct) =>
            Task.Run(() => Console.WriteLine(more?["message"]));
    }
    ```
