using System.Text.Json.Nodes;
using WJb;
using WJb.Extensions;

namespace LogWJb;

public sealed class DemoAction : IAction
{
    private readonly string _type;

    public DemoAction(string type) => _type = type;

    public Task ExecAsync(JsonObject? more, CancellationToken stoppingToken = default)
    {
        var name = more.GetString("name") ?? "World";
        Console.WriteLine($"{_type} {name}!");
        return Task.CompletedTask;
    }
}
