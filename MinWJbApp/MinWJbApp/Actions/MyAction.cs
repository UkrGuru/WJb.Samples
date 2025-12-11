using System.Text.Json.Nodes;
using WJb;

namespace Actions;

public class MyAction : IAction
{
    private string _name = "World";

    public Task InitAsync(JsonObject more, CancellationToken ct)
    {
        _name = more?["name"]?.ToString() ?? _name;
        return Task.CompletedTask;
    }

    public Task ExecAsync(CancellationToken ct)
    {
        Console.WriteLine($"Hello {_name}!");
        return Task.CompletedTask;
    }
}
