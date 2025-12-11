using System.Text.Json.Nodes;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WJb;

namespace Services;

/// <summary>
/// A minimal hosted service that enqueues one job when the app starts.
/// </summary>
public class DemoSeeder(IJobProcessor jobs, ILogger<DemoSeeder> logger) : IHostedService
{
    private readonly IJobProcessor _jobs = jobs;
    private readonly ILogger<DemoSeeder> _logger = logger;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var payload = new JsonObject
        {
            ["name"] = "Oleksandr"
        };

        _logger.LogInformation("Enqueuing MyAction with High priority...");

        var job = await _jobs.CompactAsync("MyAction", payload);

        await _jobs.EnqueueJobAsync(job, Priority.High);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
