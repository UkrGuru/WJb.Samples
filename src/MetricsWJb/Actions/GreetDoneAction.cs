
using Microsoft.Extensions.Logging;
using System.Text.Json.Nodes;
using WJb;
using WJb.Extensions;

public class GreetDoneAction : IAction
{
    private readonly ILogger<GreetDoneAction> _logger;
    public GreetDoneAction(ILogger<GreetDoneAction> logger) => _logger = logger;

    public Task ExecAsync(JsonObject more, CancellationToken stoppingToken)
    {
        var to = more.GetString("to") ?? "(missing)";
        var note = more.GetString("note") ?? "(missing)";
        _logger.LogInformation("GreetDoneAction → to={To}, note={Note}", to, note);
        return Task.CompletedTask;
    }

    public Task NextAsync(JsonObject more, CancellationToken stoppingToken) => Task.CompletedTask;
}
