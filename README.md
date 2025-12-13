# WJb.Samples
[![Nuget](https://img.shields.io/nuget/v/WJb)](https://www.nuget.org/packages/WJb/)
[![Donate](https://img.shields.io/badge/Donate-PayPal-yellow.svg)](https://www.paypal.com/donate/?hosted_button_id=BPUF3H86X96YN)

This repository contains **ready-to-run examples** demonstrating how to use WJb — a lightweight, priority-aware background job runner for .NET.

***

## 📚 Samples Overview

| Sample Name       | Description                                                                                     |
| ----------------- | --------------------------------------------------------------------------------------------- |
| **MinWJbApp**     | Minimal console app using `AddWJb()` extension, one custom action, and job enqueue.          |
| **CronWJbApp**    | Console app demonstrating scheduled jobs using cron expressions with `AddWJb()` and hosted service. |

***

## ✅ How to Run a Sample

1.  Clone the repo:
    ```bash
    git clone https://github.com/UkrGuru/WJb.Samples.git
    cd WJb.Samples
    ```

2.  Navigate to a sample folder:
    ```bash
    cd MinWJbApp
    ```

3.  Restore and run:
    ```bash
    dotnet restore
    dotnet run
    ```

***

## License

MIT License. See LICENSE for details.
