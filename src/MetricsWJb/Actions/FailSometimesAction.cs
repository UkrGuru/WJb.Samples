
// FailSometimesAction.cs
using System.Text.Json.Nodes;
using WJb;
using WJb.Extensions;

public class FailSometimesAction : IAction
{
    public Task ExecAsync(JsonObject more, CancellationToken stoppingToken)
    {
        var shouldFail = more.GetBoolean("should_fail") ?? false;
        if (shouldFail) throw new InvalidOperationException("boom");
        return Task.CompletedTask;
    }

    public Task NextAsync(JsonObject more, CancellationToken stoppingToken) => Task.CompletedTask;
}

