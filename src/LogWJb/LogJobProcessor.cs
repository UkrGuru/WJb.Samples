
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WJb;

namespace LogWJb;

public class LogJobProcessor : JobProcessor
{
    private readonly ILogger<LogJobProcessor> _logger;

    public LogJobProcessor(
        ILogger<LogJobProcessor> logger,
        IOptions<Dictionary<string, object>> options,
        IActionFactory actionFactory,
        IJobQueue queue)
        : base(logger, options, actionFactory, queue)
    {
        _logger = logger;
    }

    // 🔥 1. Logs when processor starts
    public override async Task StartAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("JobProcessor started");
        await base.StartAsync(cancellationToken);
    }

    // 🔥 2. Logs when processor stops
    public override async Task StopAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("JobProcessor stopped");
        await base.StopAsync(cancellationToken);
    }

    // 🔥 3. Job compacted
    public override async Task<string> CompactAsync(
        string actionCode,
        object? jobMore,
        CancellationToken stoppingToken = default)
    {
        var job = await base.CompactAsync(actionCode, jobMore, stoppingToken);
        _logger.LogInformation("Job Compacted");
        return job;
    }

    // 🔥 4. Job queued
    public override async Task EnqueueJobAsync(
        string job,
        Priority priority = Priority.Normal,
        CancellationToken stoppingToken = default)
    {
        await base.EnqueueJobAsync(job, priority, stoppingToken);
        _logger.LogInformation("Job Queued");
    }

    // 🔥 5. Job expanded
    public override async Task<(string Type, JsonObject More)> ExpandAsync(
        string job,
        CancellationToken stoppingToken = default)
    {
        var result = await base.ExpandAsync(job, stoppingToken);
        _logger.LogInformation("Job Expanded");
        return result;
    }

    // 🔥 6. Job execution logs
    protected override async Task JobProcessCoreAsync(
        string actionType,
        JsonObject mergedMore,
        CancellationToken stoppingToken)
    {
        _logger.LogInformation("Job Running");

        try
        {
            await base.JobProcessCoreAsync(actionType, mergedMore, stoppingToken);
            _logger.LogInformation("Job Completed");
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Job Failed");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Job Failed");
            throw;
        }
    }
}
