# HotReloadWJb (sample)

Minimal, Moq‑free hot reload demo for WJb using `JobScheduler.ReloadAsync()` (and `JobProcessor.ReloadAsync()`).

## Run
```bash
cd HotReloadWJb
dotnet run
````

## Commands

*   **E** — enqueue `Ping` now
*   **R** — toggle `cron` (`* * * * *` / none) and reload (processor → scheduler; queue optional)
*   **Q** — quit
