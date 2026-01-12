
#nullable enable
using Microsoft.Extensions.Logging;
using System.Text.Json.Nodes;
using WJb;
using WJb.Extensions;

public sealed class SayHelloAction : IAction
{
    private readonly ILogger<SayHelloAction> _logger;
    private readonly IJobProcessor _proc;   // injected to enqueue next jobs

    public SayHelloAction(ILogger<SayHelloAction> logger, IJobProcessor proc)
    {
        _logger = logger;
        _proc = proc;
    }

    // ✔ Use nullable JsonObject? per WJb 0.25.0-beta
    public Task ExecAsync(JsonObject? more, CancellationToken stoppingToken = default)
    {
        var name = more.GetString("name") ?? "World";
        _logger.LogInformation("Hello {Name}!", name);
        return Task.CompletedTask;
    }

    // ✔ Called by JobProcessor; __success and __priority are set beforehand
    public async Task NextAsync(JsonObject? more, CancellationToken stoppingToken = default)
    {
        if (more is null) return;

        // read framework-provided flags
        var success = more.GetBoolean("__success") ?? true;

        // build routing decision (code, ready-to-compact more, override priority?)
        var (code, routedMore, overridePrio) = NextRouteHelper.BuildRoute(more, success);
        if (string.IsNullOrWhiteSpace(code)) return; // nothing to route

        // compact next job
        var nextJob = await _proc.CompactAsync(code, routedMore, stoppingToken);

        // compute final priority
        var prio = overridePrio ?? PriorityHelper.GetPriorityLoose(more, fallback: Priority.Normal);

        // enqueue
        await _proc.EnqueueJobAsync(nextJob, prio, stoppingToken);
    }
}

