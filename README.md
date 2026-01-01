
# WJb Demos

A curated collection of **20 minimal, focused demos** for the `WJb` package—showing scheduling, queues, APIs, UI integrations, reporting, and more.

> **Naming convention:** `FeatureWJb` (e.g., `CronWJb`, `BlazorWJb`) for clarity and consistency.

## ✅ **Basics (Getting Started)**

1. [**1stWJb**](https://github.com/UkrGuru/WJb.Samples/tree/main/src/1stWJb) – Minimal console app, first job execution.
2. [**ConfigWJb**](https://github.com/UkrGuru/WJb.Samples/tree/main/src/ConfigWJb) – Using configuration for job settings.
3. [**SqlWJb**](https://github.com/UkrGuru/WJb.Samples/tree/main/src/SqlWJb) – Execute SQL commands via WJb.
4. [**QueueWJb**](https://github.com/UkrGuru/WJb.Samples/tree/main/src/QueueWJb) – Simple queue processing.
5. [**TimerWJb**](https://github.com/UkrGuru/WJb.Samples/tree/main/src/TimerWJb) – Scheduled jobs using delays.
6. [**LogWJb**](https://github.com/UkrGuru/WJb.Samples/tree/main/src/LogWJb) – Custom `JobProcessor` that logs the full job lifecycle (Compacted → Queued → Expanded → Running → Completed/Failed).

***

## ✅ **Scheduling & Triggers**

7.  [**CronWJb**](https://github.com/UkrGuru/WJb.Samples/tree/main/src/CronWJb) – Cron-based scheduling.
8.  [**EventWJb**](https://github.com/UkrGuru/WJb.Samples/tree/main/src/EventWJb) – Trigger jobs on custom events.
9.  **ApiWJb** – Trigger jobs via REST API.
10.  **WebhookWJb** – Execute jobs from external webhooks.

***

## ✅ **Integration**

11. **BlazorWJb** – Blazor UI for job management.
12. **MvcWJb** – ASP.NET MVC integration.
13. **WinFormsWJb** – Desktop app demo.
14. **WorkerWJb** – Background service in .NET Worker.
15. **DIWJb** – Using Dependency Injection with WJb.

***

## ✅ **Advanced Features**

16. **FileWJb** – File upload & processing jobs.
17. **MailWJb** – Sending emails via WJb.
18. **ReportWJb** – Generate and deliver reports.
19. **ParallelWJb** – Run multiple jobs concurrently.
20. **SecureWJb** – Authentication & authorization for job execution.

***

## 🔧 Prerequisites

- .NET SDK (>= 10.0)
- SQL Server (LocalDB or remote)
- Packages:
  - `WJb`
  - `UkrGuru.Sql` (preferred over EF Core, used across SQL demos)

## 🚀 Getting Started

```bash
git clone https://github.com/UkrGuru/WJb.Samples.git
cd WJb.Samples

# Pick a demo
cd src/1stWJb

# Restore & run
dotnet restore
dotnet run
```
