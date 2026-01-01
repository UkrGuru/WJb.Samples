# LogWJb — WJb Demo (NuGet)

Minimal console demo showing a custom `JobProcessor` that logs the job lifecycle.

## What it does
- Registers `DemoActionFactory` and `LogJobProcessor`.
- Configures console logging with timestamps.
- Compacts and enqueues a demo job: `SayHello` → `{ name: "Viktor" }`.
- Background service processes the queue and logs:
  `Compacted → Queued → Expanded → Running → Completed/Failed`.

## Sample Output
```

info: LogWJb.LogJobProcessor[0] JobProcessor started
info: LogWJb.LogJobProcessor[0] Job Compacted
info: LogWJb.LogJobProcessor[0] Job Queued
info: LogWJb.LogJobProcessor[0] Job Expanded
info: LogWJb.LogJobProcessor[0] Job Running
SayHello Viktor!
info: LogWJb.LogJobProcessor[0] Job Completed
info: LogWJb.LogJobProcessor[0] JobProcessor stopped

```