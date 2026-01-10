# 1stWJb — WJb Demo (NuGet)

Minimal console demo using the **WJb** NuGet package.

## What it does
- Registers a custom action `MyAction` (implements `IAction`).
- Configures WJb with default `more` data (`{ name: "Oleksandr" }`).
- Enqueues two jobs:
  1. Uses defaults → prints **Hello Oleksandr!**
  2. Overrides defaults → prints **Hello Viktor!**
- Runs the WJb background job processor.

## Sample Output

```
Hello Viktor!
Hello Oleksandr!
```
