
// DemoActionFactory.cs
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json.Nodes;
using WJb;
using WJb.Extensions;

public class DemoActionFactory : IActionFactory
{
    private readonly IServiceProvider _sp;
    public DemoActionFactory(IServiceProvider sp) => _sp = sp;

    public IAction Create(string type) => type switch
    {
        nameof(SayHelloAction) => _sp.GetRequiredService<SayHelloAction>(),
        nameof(FailSometimesAction) => _sp.GetRequiredService<FailSometimesAction>(),
        nameof(GreetDoneAction) => _sp.GetRequiredService<GreetDoneAction>(), // ensure this line exists
        _ => throw new InvalidOperationException($"Unknown action type: {type}")
    };

    public ActionItem GetActionItem(string code) => code switch
    {
        // SUCCESS → NEXT route demo



        "SayHello" => new ActionItem
        {
            Type = nameof(SayHelloAction),
            More = new JsonObject
            {
                ["next"] = "GreetDone",
                ["next_to"] = "Alex",
                ["next_note"] = "Telemetry demo",
                ["next_priority"] = (int)Priority.High, // optional
                ["priority"] = (int)Priority.High
            }
        },
        "GreetDone" => new ActionItem
        {
            Type = nameof(GreetDoneAction),
            More = new JsonObject()
        },



        // DemoActionFactory.cs (GetActionItem)
        "FailSometimes" => new ActionItem
        {
            Type = nameof(FailSometimesAction),
            More = new JsonObject
            {
                // Route to LogError on failure
                ["fail"] = "LogError",

                // Forward these to the fail target (prefix removed):
                ["fail_message"] = "FailSometimes crashed",
                ["fail_source"] = "FailSometimesAction",

                // Optional: change fail hop priority
                ["fail_priority"] = (int)Priority.High,

                // Current action defaults
                ["priority"] = (int)Priority.Normal
            }
        },

        "LogError" => new ActionItem
        {
            Type = nameof(LogErrorAction),
            More = new JsonObject()
        },

        _ => throw new InvalidOperationException($"Unknown action code: {code}")
    };
}


