# EventWJb – Trigger jobs on custom events (WJb / .NET 10)

This sample shows how to trigger WJb jobs from **custom events** using a simple **file-based event listener**.
Drop JSON files into the `events` folder, and the listener will enqueue jobs via `IJobProcessor`.

- **Target Framework:** `net10.0`
- **Packages:** `Microsoft.Extensions.Hosting (10.0.1)`, `Microsoft.Extensions.Logging.Console (10.0.1)`, `WJb (0.13.1-beta)`
- **Action Contract:** `IAction.ExecAsync(JsonObject? jobMore, CancellationToken)`

## How it works
- `Program.cs` loads `actions.json` (action map with defaults) and registers WJb.
- `FileEventListener` (BackgroundService) watches the `events` directory for `*.json` files.
- Each file is interpreted as either:
  1. **JSON event**: `{ "code": "ActionCode", "more": { ... } }` → enqueued via `IJobProcessor.EnqueueJobAsync(actionCode, more, priority)`
  2. **Raw compact job string** → processed via `IJobProcessor.ProcessJobAsync(job)`

A demo event file `demo-event.json` is seeded on first run.

## Run
```bash
cd EventWJb
 dotnet restore
 dotnet run
```
Expected startup output (example):
```
EventWJb started. Drop JSON files into the 'events' folder to trigger jobs.
Loaded actions:
 - OnFileCreated: {"message":"File created event","origin":"fsw"}
 - OnBusinessEvent: {"message":"Business event handled","origin":"bus"}
```

When you drop an event file, the listener logs and enqueues the job. The `DummyAction` prints the message and origin.

## Create an event file
Create `events/new-order.json` with:
```json
{
  "code": "OnBusinessEvent",
  "more": { "message": "New order received #123", "origin": "api", "priority": 2 }
}
```
You should see:
```
YYYY-MM-DD HH:mm:ss → New order received #123 [origin: api]
```

## Notes
- The `actions.json` payload supports inheritance via `base` with **child-wins** precedence (if used in your map).
- For alternative event sources (message bus, HTTP endpoints, etc.), replace `FileEventListener` with your own listener that calls `IJobProcessor.EnqueueJobAsync(...)`.

