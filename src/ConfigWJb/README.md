# ConfigWJb — Using configuration for job settings

A minimal console demo showing how to configure **WJb** via `appsettings.json` for runtime **settings** and load the **actions map** from a separate `actions.json` file.

## ✅ What it demonstrates

*   Reads `WJb:Settings` (e.g., `MaxParallelJobs`) from `appsettings.json`.
*   Loads actions from `actions.json` and registers them with `services.AddWJb(actions: ...)`.
*   Enqueues two jobs to demonstrate:
    *   Default payload from `actions.json` (`Hello Oleksandr!`)
    *   Override payload (`Hello Viktor!`).

***

## 🗂 Files

*   **appsettings.json** — contains WJb settings like `MaxParallelJobs`.
*   **actions.json** — defines action map (code → handler type + default `more` payload).
*   **Program.cs** — loads both files, configures WJb, enqueues jobs, and runs the host.

***

## 🔍 Code Highlights

```csharp
// Load actions from actions.json
var json = File.ReadAllText("actions.json");
var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
var actions = JsonSerializer.Deserialize<Dictionary<string, ActionItem>>(json, options)
    ?? throw new InvalidOperationException("Failed to deserialize actions.json");

// Register WJb with loaded actions
services.AddWJb(actions: actions);
```

***

## ✅ Expected Output

    info: WJb.JobProcessor[0]
          JobProcessor started
    info: Microsoft.Hosting.Lifetime[0]
          Application started. Press Ctrl+C to shut down.
    Hello Viktor!
    Hello Oleksandr!

***