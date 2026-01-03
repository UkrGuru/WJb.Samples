using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using WJb;
using WJb.Extensions;

Console.OutputEncoding = Encoding.UTF8;

var builder = WebApplication.CreateBuilder(args);

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(opt =>
{
    opt.SingleLine = true;
    opt.TimestampFormat = "HH:mm:ss ";
});

builder.Logging.AddFilter("Microsoft.AspNetCore.Mvc.Infrastructure", LogLevel.None);
builder.Logging.AddFilter("Microsoft.AspNetCore.Watch.BrowserRefresh", LogLevel.None);
builder.Logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.None);
builder.Logging.AddFilter("Microsoft.Extensions.Hosting", LogLevel.None);

// Load actions.json
var actionsJsonPath = Path.Combine(AppContext.BaseDirectory, "actions.json");
var actionsJson = File.Exists(actionsJsonPath) ? await File.ReadAllTextAsync(actionsJsonPath) : "{}";
var actions = JsonSerializer.Deserialize<Dictionary<string, ActionItem>>(actionsJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
    ?? new Dictionary<string, ActionItem>(StringComparer.OrdinalIgnoreCase);

// WJb services
builder.Services.AddWJbActions(actions).AddWJbOther();
builder.Services.Configure<Dictionary<string, object>>(cfg => { cfg["MaxParallelJobs"] = 2; });

// Minimal API
var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "ok", time = DateTimeOffset.Now }));

// POST /events { code|actionCode, more?, priority? }
app.MapPost("/events", async (HttpContext http, IJobProcessor processor) =>
{
    var dto = await http.Request.ReadFromJsonAsync<EventDto>();
    if (dto is null) return Results.BadRequest(new { error = "Invalid JSON" });

    var code = dto.Code ?? dto.ActionCode;
    if (string.IsNullOrWhiteSpace(code)) return Results.BadRequest(new { error = "'code' (or 'actionCode') is required" });

    var more = dto.More ?? [];
    var prio = dto.Priority is int ip ? (Priority)ip : Priority.Normal;

    var job = await processor.CompactAsync(code, more, http.RequestAborted);
    await processor.EnqueueJobAsync(job, prio, http.RequestAborted);
    return Results.Accepted($"/events/{Guid.NewGuid()}", new { enqueued = true, code });
});

// POST /jobs/compact { code, more? } -> returns compact string
app.MapPost("/jobs/compact", async (HttpContext http, IJobProcessor processor) =>
{
    var dto = await http.Request.ReadFromJsonAsync<EventDto>();
    if (dto is null || string.IsNullOrWhiteSpace(dto.Code))
        return Results.BadRequest(new { error = "'code' is required" });
    var job = await processor.CompactAsync(dto.Code!, dto.More ?? [], http.RequestAborted);
    return Results.Ok(new { job });
});

// POST /jobs/process { job, priority? } -> process immediately
app.MapPost("/jobs/process", async (HttpContext http, IJobProcessor processor) =>
{
    var dto = await http.Request.ReadFromJsonAsync<ProcessDto>();
    if (dto is null || string.IsNullOrWhiteSpace(dto.Job))
        return Results.BadRequest(new { error = "'job' is required" });
    await processor.ProcessJobAsync(dto.Job!, stoppingToken: http.RequestAborted);
    return Results.Ok(new { processed = true });
});

app.Lifetime.ApplicationStarted.Register(() =>
{
    Console.WriteLine("ApiWJb started. Try: POST /events { code, more } or GET /health");
    Console.WriteLine("Loaded actions:");
    foreach (var kv in actions)
    {
        Console.WriteLine($" - {kv.Key}: {kv.Value.More?.ToJsonString()}");
    }
});

await app.RunAsync();

// IAction: Exec-only (current WJb)
public sealed class DummyAction(ILogger<DummyAction> logger) : IAction
{
    private readonly ILogger<DummyAction> _logger = logger;
    public Task ExecAsync(JsonObject? jobMore, CancellationToken stoppingToken)
    {
        var message = jobMore.GetString("message") ?? "Hello from DummyAction!";
        var origin = jobMore.GetString("origin") ?? "(unknown origin)";
        Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} → {message} [origin: {origin}]");
        return Task.CompletedTask;
    }
}

// DTOs
public record EventDto
{
    public string? Code { get; init; }
    public string? ActionCode { get; init; }
    public JsonObject? More { get; init; }
    public int? Priority { get; init; }
}

public record ProcessDto
{
    public string? Job { get; init; }
    public int? Priority { get; init; }
}
