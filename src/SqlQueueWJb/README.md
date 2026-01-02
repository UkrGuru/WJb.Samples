# SqlQueueWJb

SQL-backed job queue for **WJb** actions with background processing.

## What it does

*   Registers custom actions via `IActionFactory`.
*   Persists jobs in `dbo.WJbQueue`, archives to `dbo.WJbHistory`.
*   Runs as `BackgroundService` implementing `IJobProcessor`.
*   Logs lifecycle: Compacted → Queued → Running → Completed/Failed.

## Before You Run

**Initialize the database first**:  
Run `Resources/InitDb.sql` in SQL Server Management Studio or via `sqlcmd` to create the database and tables:

    IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = 'SqlQueueWJb')
    BEGIN
        CREATE DATABASE [SqlQueueWJb];
    END
    GO
    USE [SqlQueueWJb];
    GO
    -- Creates WJbQueue and WJbHistory tables

## Quick Start

```csharp
services.AddSql(conn);
services.AddSingleton<IActionFactory, DemoActionFactory>();
services.AddSingleton<IJobProcessor, SqlJobProcessor>();
services.AddHostedService(sp => (SqlJobProcessor)sp.GetRequiredService<IJobProcessor>());
```

Enqueue:

```csharp
var job = await proc.CompactAsync("SayHello", new { name = "Viktor" });
await proc.EnqueueJobAsync(job);
```

## Sample Output

    info: SqlQueueWJb.SqlJobProcessor[0] JobProcessor started
    info: SqlQueueWJb.SqlJobProcessor[0] Job Compacted
    info: SqlQueueWJb.SqlJobProcessor[0] Job Queued
    info: SqlQueueWJb.SqlJobProcessor[0] Job Expanded
    info: SqlQueueWJb.SqlJobProcessor[0] Job Running
    SayHello Viktor!
    info: SqlQueueWJb.SqlJobProcessor[0] Job Completed
    info: SqlQueueWJb.SqlJobProcessor[0] JobProcessor stopped

***
