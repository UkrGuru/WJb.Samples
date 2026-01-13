# **1stWJb – Minimal Console App**

A simple demo showing how to execute a job using the **WJb** library in a minimal console application.

***

## 🖥 **Main Code**

```csharp
// Prepare & Enqueue default job
var defaultJob = await jobProcessor.CompactAsync("MyAction");
await jobProcessor.EnqueueJobAsync(defaultJob);

// Prepare & Enqueue override job
var overrideJob = await jobProcessor.CompactAsync("MyAction", new { name = "Viktor" });
await jobProcessor.EnqueueJobAsync(overrideJob, Priority.High);
```

***

## 📌 **Expected Output**

```
    Hello Viktor!
    Hello Oleksandr!
```
