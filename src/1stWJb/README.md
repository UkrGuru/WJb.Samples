# 1stWJb — WJb Demo (NuGet)

Minimal console demo using the **WJb** NuGet package.

## What it does
- Registers your custom action `MyAction` (implements `IAction`).
- Configures WJb with a default payload (`more: { name: "Oleksandr" }`).
- Enqueues two jobs:
  1. **Defaults only** → prints `Hello Oleksandr!`
  2. **Override defaults** → prints `Hello Viktor!`
- Starts the host so WJb background processing can pick up the jobs.

## Sample Output

```
info: WJb.JobProcessor[0] JobProcessor started
Hello Viktor!
Hello Oleksandr!
```
