
# QueueWJb — WJb Demo (NuGet)

Minimal console demo using the **WJb** NuGet package (`0.13.0-beta`) to simulate simple **queue processing**.

## Run

```bash
cd src
 dotnet restore
 dotnet run
```

## What it does
- Registers a custom action `MyQueueAction` (implements `IAction`).
- Configures WJb with default payload (`more: { items: ["A", "B", "C"] }`).
- Enqueues two jobs:
  1. **Defaults only** → processes `A, B, C`.
  2. **Override defaults** → processes `X, Y`.
- Uses `ILogger` to log processing of each item.
- Starts the host so WJb background processing can pick up the jobs.
