using System.Text.Json.Nodes;
using WJb;

namespace LogWJb;

public sealed class DemoActionFactory : IActionFactory
{
    private readonly Dictionary<string, JsonObject?> _defaults = new(StringComparer.OrdinalIgnoreCase)
    {
        ["SayHello"] = new JsonObject { ["name"] = "Oleksandr" }
    };

    public IAction Create(string actionType) => new DemoAction(actionType);

    // In WJb, GetActionItem returns an item with Type and More (defaults).
    // For the demo, we keep Type = actionCode and More = known defaults.
    public ActionItem GetActionItem(string actionCode)
    {
        var defaults = _defaults.TryGetValue(actionCode, out var more) ? more : null;
        return new ActionItem(actionCode, defaults);
    }
}
