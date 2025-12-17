
# TimerWJb — Load actions from actions.json (simple & success style)

This sample uses the **simplest** pattern to load actions via `actions.json` and registers them into WJb with `services.AddWJb(actions: actions)`. If the file changes, rebuild to pick up updates.

## Run
```bash
cd TimerWJb_Simple
 dotnet restore
 dotnet run
```

## actions.json
```json
{
  "MyTimerAction": {
    "Type": "TimerWJb_Simple.MyTimerAction, TimerWJb_Simple",
    "More": { "intervalMs": 1000, "items": ["A", "B", "C"] }
  }
}
```
> Property names are case‑insensitive during deserialization.

## Sample Output
```
info: TimerEnqueuer[0] TimerWJb started (actions: 1)
info: TimerEnqueuer[0] Enqueue: MyTimerAction (period=1000ms)
info: WJb.JobProcessor[0] JobProcessor started
info: MyTimerAction[0] TimerWJb tick: A
info: MyTimerAction[0] TimerWJb tick: B
info: MyTimerAction[0] TimerWJb tick: C
info: MyTimerAction[0] TimerWJb: Done.
```

## Notes
- Startup messages suppressed via `.UseConsoleLifetime(o => o.SuppressStatusMessages = true)`.
- `PeriodicTimer` drives job enqueues per action using `More.intervalMs`.
- You can enqueue overrides at runtime; **child‑wins** precedence will apply between job payload and defaults.
