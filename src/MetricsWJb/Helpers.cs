
// Helpers/NextRouteHelper.cs
using System.Text.Json.Nodes;
using WJb;
using WJb.Extensions;

public static class NextRouteHelper
{
    public static (string? TargetCode, JsonObject RoutedMore, Priority? OverridePriority)
        BuildRoute(JsonObject more, bool success)
    {
        var routed = new JsonObject();
        string prefix = success ? "next_" : "fail_";
        string? code = more.GetString(success ? "next" : "fail");  // target action code
        Priority? overridePrio = null;

        // Copy prefixed keys into routed more with prefix stripped
        foreach (var kvp in more)
        {
            if (kvp.Key.StartsWith(prefix, StringComparison.Ordinal))
            {
                var keyWithout = kvp.Key.Substring(prefix.Length);
                routed[keyWithout] = kvp.Value?.DeepClone();
            }
        }

        // Allow priority override: next_priority / fail_priority can be int or string
        var prioNode = more[prefix + "priority"];
        if (prioNode is not null)
        {
            if (int.TryParse(prioNode.ToString(), out var i) && Enum.IsDefined(typeof(Priority), i))
                overridePrio = (Priority)i;
            else if (Enum.TryParse<Priority>(prioNode.ToString(), ignoreCase: true, out var p))
                overridePrio = p;
        }

        return (code, routed, overridePrio);
    }
}

public static class PriorityHelper
{
    public static Priority GetPriorityLoose(JsonObject? more, Priority fallback)
    {
        if (more is null) return fallback;

        // 1) Try string (what MoreExtensions expects)
        var s = more.GetString("priority");
        if (!string.IsNullOrWhiteSpace(s) && Enum.TryParse<Priority>(s, true, out var parsed))
            return parsed;

        // 2) Try numeric
        if (more.TryGetPropertyValue("priority", out var node) && node is JsonValue v)
        {
            if (v.TryGetValue<int>(out var i) && Enum.IsDefined(typeof(Priority), i))
                return (Priority)i;

            if (v.TryGetValue<long>(out var l) && Enum.IsDefined(typeof(Priority), (int)l))
                return (Priority)(int)l;
        }

        return fallback;
    }
}
