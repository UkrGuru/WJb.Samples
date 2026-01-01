using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WJb;

namespace LogWJb;

/// <summary>
/// Logs the lifecycle messages you standardized, using only supported virtual hooks.
/// </summary>
public class LogJobProcessor : JobProcessor
{
    private readonly ILogger<LogJobProcessor> _logger;

    public LogJobProcessor(ILogger<LogJobProcessor> logger, IOptions<Dictionary<string, object>> options, IActionFactory actionFactory)
        : base(logger, options, actionFactory)
    {
        _logger = logger;
    }

    // Compact → "Compacted"
    public override async Task<string> CompactAsync(string actionCode, object? jobMore, CancellationToken stoppingToken = default)
    {
        var job = await base.CompactAsync(actionCode, jobMore, stoppingToken);
        _logger.LogInformation("Job Compacted");
        return job;
    }

    // Enqueue (job) → "JobQueued"
    public override async Task EnqueueJobAsync(string job, Priority priority = Priority.Normal, CancellationToken stoppingToken = default)
    {
        await base.EnqueueJobAsync(job, priority, stoppingToken);
        _logger.LogInformation("Job Queued");
    }

    // Expand → "Expanded"
    public override async Task<(string Type, JsonObject More)> ExpandAsync(string job, CancellationToken stoppingToken = default)
    {
        var result = await base.ExpandAsync(job, stoppingToken);
        _logger.LogInformation("Job Expanded");
        return result;
    }

    /// <summary>
    /// Logs "JobRunning" then "JobCompleted" (or "JobFailed" on error) around the actual action execution.
    /// Base ProcessJobAsync still handles Next-chaining & error boundaries.
    /// </summary>
    protected override async Task JobProcessCoreAsync(string actionType, JsonObject mergedMore, CancellationToken stoppingToken)
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
