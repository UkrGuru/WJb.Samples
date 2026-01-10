
// Actions/LogErrorAction.cs
using Microsoft.Extensions.Logging;
using System.Text.Json.Nodes;
using WJb;
using WJb.Extensions;

public class LogErrorAction : IAction
{
    private readonly ILogger<LogErrorAction> _logger;
    public LogErrorAction(ILogger<LogErrorAction> logger) => _logger = logger;

    public Task ExecAsync(JsonObject more, CancellationToken stoppingToken)
    {
        var message = more.GetString("message") ?? "Unknown error";
        var source = more.GetString("source") ?? "Unknown";
        _logger.LogError("LogErrorAction → source={Source}, message={Message}", source, message);
        return Task.CompletedTask;
    }

    public Task NextAsync(JsonObject more, CancellationToken stoppingToken) => Task.CompletedTask;
}
