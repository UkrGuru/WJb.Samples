# TimerWJb

Scheduled jobs using delays (PeriodicTimer) using the **WJb** NuGet package.

## Run
```bash
dotnet restore
dotnet run --project TimerWJb
```

## actions.json
```json
{
  "MyTimerAction": {
    "Type": "TimerWJb.MyTimerAction, TimerWJb",
    "More": { "intervalMs": 1000, "items": ["A", "B", "C"] }
  }
}
```

## Sample Output
```
info: WJb.JobProcessor[0] JobProcessor started
info: TimerEnqueuer[0] TimerWJb started (actions: 1)
info: TimerEnqueuer[0] Enqueue: MyTimerAction (period=1000ms)
info: MyTimerAction[0] TimerWJb tick: A
info: MyTimerAction[0] TimerWJb tick: B
info: MyTimerAction[0] TimerWJb tick: C
info: MyTimerAction[0] TimerWJb: Done.
```
