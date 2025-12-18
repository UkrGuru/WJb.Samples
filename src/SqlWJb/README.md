# **SqlWJb — Execute SQL via WJb using UkrGuru.Sql**

A minimal console demo where a **WJb action** runs SQL using **UkrGuru.Sql**.

---

## **What it demonstrates**
- Reading **connection string** from `appsettings.json`.
- **Dependency Injection** setup for `UkrGuru.Sql` client.
- A custom WJb action (`MySqlAction`) that:
  - Executes dynamic T-SQL from job payload.
  - Demonstrates JSON parsing in SQL using `OPENJSON`.
  - Calculates an **order total** from JSON items.
- Enqueue jobs with different payloads (SQL + JSON data) to show **override behavior**.

---

## **Notes**
- Update the connection string in `appsettings.json` to match your environment.
- The sample uses `Database=SqlWJbDemo`.
- Demonstrates:
  - Creating a simple table `dbo.Messages` (optional).
  - Executing **parameterized SQL** with JSON input.
  - Logging results via `ILogger`.

---

## **Example Job Payload**
```json
{
  "tsql": "SELECT CAST(SUM(TRY_CONVERT(decimal(18,4), quantity) * TRY_CONVERT(decimal(18,4), price)) AS decimal(18,2)) FROM OPENJSON(@Data, '$.items') WITH (quantity int '$.quantity', price decimal(18,4) '$.price')",
  "data": "{ "orderId": "12345", "restaurant": "McDonald's", "items": [ { "name": "Big Mac", "quantity": 1, "price": 5.99 }, { "name": "Medium Fries", "quantity": 1, "price": 2.49 }, { "name": "Coca-Cola", "size": "Medium", "quantity": 1, "price": 1.99 } ] }"
}
```

---

## **Highlights**
- Uses **`OPENJSON`** for flexible JSON parsing in SQL.
- Demonstrates **dynamic SQL execution** via WJb.
- Clean logging with `SimpleConsole` and timestamp formatting.


---

## **Output**
```
info: WJb.JobProcessor[0] JobProcessor started
info: MySqlAction[0] Order Total: 10.47
```
