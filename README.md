
# WJb Demos
[![Nuget](https://img.shields.io/nuget/v/WJb)](https://www.nuget.org/packages/WJb/)
[![Donate](https://img.shields.io/badge/Donate-PayPal-yellow.svg)](https://www.paypal.com/donate/?hosted_button_id=BPUF3H86X96YN)

A curated collection of **solution alive samples** for the `WJb` package—showing scheduling, queues, APIs, UI integrations, reporting, and more.

> **Naming convention:** `FeatureWJb` (e.g., `CronWJb`, `BlazorWJb`) for clarity and consistency.

## ✅ **Basics (Getting Started)**
* [**1stWJb**](https://github.com/UkrGuru/WJb.Samples/tree/main/src/1stWJb) – Minimal console app, first job execution.
* [**ConfigWJb**](https://github.com/UkrGuru/WJb.Samples/tree/main/src/ConfigWJb) – Using configuration for job settings.
* [**SqlWJb**](https://github.com/UkrGuru/WJb.Samples/tree/main/src/SqlWJb) – Execute SQL commands via WJb.
* [**QueueWJb**](https://github.com/UkrGuru/WJb.Samples/tree/main/src/QueueWJb) – Simple queue processing.
* [**TimerWJb**](https://github.com/UkrGuru/WJb.Samples/tree/main/src/TimerWJb) – Scheduled jobs using delays.
* [**BlazorWJb**](https://github.com/UkrGuru/WJb.Samples/tree/main/src/BlazorWJb) – Blazor integration for WJb jobs.

## ✅ **Scheduling & Triggers**
* [**CronWJb**](https://github.com/UkrGuru/WJb.Samples/tree/main/src/CronWJb) – Cron-based scheduling.
* [**EventWJb**](https://github.com/UkrGuru/WJb.Samples/tree/main/src/EventWJb) – Trigger jobs on custom events.

## ✅ **Logging & Monitoring**
* [**LogWJb**](https://github.com/UkrGuru/WJb.Samples/tree/main/src/LogWJb) – Custom `LogJobProcessor` with full lifecycle logging.
* [**MetricsWJb**](https://github.com/UkrGuru/WJb.Samples/tree/main/src/MetricsWJb) – Collect and display job metrics.

## ✅ **Advanced Queue**
* [**SqlQueueWJb**](https://github.com/UkrGuru/WJb.Samples/tree/main/src/SqlQueueWJb) – SQL-backed queue with job history and lifecycle logging.

## ✅ **Hot Reload**
* [**HotReloadWJb**](https://github.com/UkrGuru/WJb.Samples/tree/main/src/HotReloadWJb) – Demonstrates dynamic job updates without restarting.

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
