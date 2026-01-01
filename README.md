
# WJb Demos

A curated collection of **20 minimal, focused demos** for the `WJb` package—showing scheduling, queues, APIs, UI integrations, reporting, and more.

> **Naming convention:** `FeatureWJb` (e.g., `CronWJb`, `BlazorWJb`) for clarity and consistency.

## ✅ **Basics (Getting Started)**

* [**1stWJb**](https://github.com/UkrGuru/WJb.Samples/tree/main/src/1stWJb) – Minimal console app, first job execution.
* [**ConfigWJb**](https://github.com/UkrGuru/WJb.Samples/tree/main/src/ConfigWJb) – Using configuration for job settings.
* [**SqlWJb**](https://github.com/UkrGuru/WJb.Samples/tree/main/src/SqlWJb) – Execute SQL commands via WJb.
* [**QueueWJb**](https://github.com/UkrGuru/WJb.Samples/tree/main/src/QueueWJb) – Simple queue processing.
* [**TimerWJb**](https://github.com/UkrGuru/WJb.Samples/tree/main/src/TimerWJb) – Scheduled jobs using delays.
* [**LogWJb**](https://github.com/UkrGuru/WJb.Samples/tree/main/src/LogWJb) – Custom `JobProcessor` that logs the full job lifecycle (Compacted → Queued → Expanded → Running → Completed/Failed).

***

## ✅ **Scheduling & Triggers**

*  [**CronWJb**](https://github.com/UkrGuru/WJb.Samples/tree/main/src/CronWJb) – Cron-based scheduling.
*  [**EventWJb**](https://github.com/UkrGuru/WJb.Samples/tree/main/src/EventWJb) – Trigger jobs on custom events.
*  **ApiWJb** – Trigger jobs via REST API.
*  **WebhookWJb** – Execute jobs from external webhooks.

***

## ✅ **Integration**

* **BlazorWJb** – Blazor UI for job management.
* **MvcWJb** – ASP.NET MVC integration.
* **WinFormsWJb** – Desktop app demo.
* **WorkerWJb** – Background service in .NET Worker.
* **DIWJb** – Using Dependency Injection with WJb.

***

## ✅ **Advanced Features**

* **FileWJb** – File upload & processing jobs.
* **MailWJb** – Sending emails via WJb.
* **ReportWJb** – Generate and deliver reports.
* **ParallelWJb** – Run multiple jobs concurrently.
* **SecureWJb** – Authentication & authorization for job execution.

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
