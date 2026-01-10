
// Actions/SayHelloAction.cs
using Microsoft.Extensions.Logging;
using System.Text.Json.Nodes;
using WJb;
using WJb.Extensions;

public class SayHelloAction : IAction
{
    private readonly ILogger<SayHelloAction> _logger;
    private readonly IJobProcessor _proc;   // <-- inject the processor to enqueue next jobs

    public SayHelloAction(ILogger<SayHelloAction> logger, IJobProcessor proc)
    {
        _logger = logger;
        _proc = proc;
    }

    public Task ExecAsync(JsonObject more, CancellationToken stoppingToken)
    {
        var name = more.GetString("name") ?? "World";
        _logger.LogInformation("Hello {Name}!", name);
        return Task.CompletedTask;
    }

    public async Task NextAsync(JsonObject more, CancellationToken stoppingToken)
    {
        // __success and __priority are set by JobProcessor before calling NextAsync
        var success = more.GetBoolean("__success") ?? true;

        var (code, routedMore, overridePrio) = NextRouteHelper.BuildRoute(more, success);
        if (string.IsNullOrWhiteSpace(code)) return; // nothing to route

        // Build & enqueue next job
        var nextJob = await _proc.CompactAsync(code, routedMore, stoppingToken);

        // use override priority if provided, otherwise inherit from __priority (string)
        Priority prio = overridePrio
            ?? (Enum.TryParse<Priority>(more.GetString("__priority") ?? "Normal", out var p) ? p : Priority.Normal);

        await _proc.EnqueueJobAsync(nextJob, prio, stoppingToken);
    }
}
