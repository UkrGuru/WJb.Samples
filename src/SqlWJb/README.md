
# SqlWJb — Execute SQL commands via WJb (using UkrGuru.Sql)

A minimal console demo where a WJb action runs SQL using **UkrGuru.Sql**.

## What it demonstrates
- Connection string from `appsettings.json`.
- DI registration of `UkrGuru.Sql` client.
- A custom action (`MySqlAction`) that executes SQL:
  - Example: create table if not exists, insert a row, and select a row.
- Enqueue jobs with different payloads (SQL + parameters) to show override behavior.

## Run

```bash
cd src
 dotnet restore
 dotnet run
```

## Notes
- Update the connection string in `appsettings.json` to match your environment.
- The sample uses `Database=SqlWJbDemo` and will create a simple table `dbo.Messages`.
