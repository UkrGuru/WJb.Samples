
# ConfigWJb — Using configuration for job settings

A minimal console demo showing how to configure **WJb** via `appsettings.json`:
- Runtime **Settings** (e.g., `MaxParallelJobs`)
- **Actions** map (code → handler type + default `more` payload)

## Run

```bash
cd src
 dotnet restore
 dotnet run
```

## Highlights
- Reads `WJb:Settings` and `WJb:Actions` from configuration.
- Registers actions dynamically with `services.AddWJb(...)`.
- Enqueues two jobs to demonstrate default merging and overrides.
