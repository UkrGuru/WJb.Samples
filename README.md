
# WJb Demos

A curated collection of **20 minimal, focused demos** for the `WJb` package—showing scheduling, queues, APIs, UI integrations, reporting, and more.

> **Naming convention:** `FeatureWJb` (e.g., `CronWJb`, `BlazorWJb`) for clarity and consistency.

## 📚 Demo Index

| # | Demo | Focus |
|---|------|-------|
| 1 | `1stWJb` | Minimal console app; first job execution |
| 2 | `ConfigWJb` | Configuration-driven job settings |
| 3 | `SqlWJb` | Execute SQL via WJb (using `UkrGuru.Sql`) |
| 4 | `QueueWJb` | Queue processing |
| 5 | `TimerWJb` | Scheduled timers / delays |
| 6 | `CronWJb` | Cron-based scheduling |
| 7 | `EventWJb` | Custom event triggers |
| 8 | `ApiWJb` | Trigger via REST API |
| 9 | `WebhookWJb` | External webhooks |
|10 | `RetryWJb` | Retry policies |
|11 | `BlazorWJb` | Blazor UI integration |
|12 | `MvcWJb` | ASP.NET MVC |
|13 | `WinFormsWJb` | Desktop app |
|14 | `WorkerWJb` | .NET Worker service |
|15 | `DIWJb` | Dependency Injection patterns |
|16 | `FileWJb` | File processing |
|17 | `MailWJb` | Email sending |
|18 | `ReportWJb` | Reporting and delivery |
|19 | `ParallelWJb` | Concurrent jobs |
|20 | `SecureWJb` | AuthN/AuthZ around job execution |

> Full roadmap in `docs/ROADMAP.md`

## 🔧 Prerequisites

- .NET SDK (>= 8.0)
- SQL Server (LocalDB or remote)
- Packages:
  - `WJb`
  - `UkrGuru.Sql` (preferred over EF Core, used across SQL demos)

## 🚀 Getting Started

```bash
git clone https://github.com/<your-org>/WJb-Demos.git
cd WJb-Demos

# Pick a demo
cd src/1stWJb/src

## Restore & run
dotnet restore
