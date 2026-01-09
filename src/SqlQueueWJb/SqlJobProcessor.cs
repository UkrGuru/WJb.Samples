
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UkrGuru.Sql;
using WJb;
using WJb.Extensions;

namespace SqlQueueWJb;

/// <summary>
/// SQL-backed job processor: persists queue entries and logs lifecycle,
/// while delegating actual execution to base JobProcessor.
/// </summary>
public class SqlJobProcessor(
    ILogger<SqlJobProcessor> logger,
    IOptions<Dictionary<string, object>> options,
    IActionFactory actionFactory,
    IDbService db,
    IJobQueue queue) // <-- NEW: required by JobProcessor in 0.22.0-beta
    : JobProcessor(logger, options, actionFactory, queue) // <-- pass queue to base
{
    private readonly ILogger<SqlJobProcessor> _logger = logger;
    private readonly IDbService _db = db;

    // Compact → "Compacted" and persist job (returns JobId as string)
    public override async Task<string> CompactAsync(string actionCode, object? jobMore, CancellationToken stoppingToken = default)
    {
        var job = await base.CompactAsync(actionCode, jobMore, stoppingToken);

        var jobId = await _db.ExecAsync<int>(WJbQueue.Ins, job, cancellationToken: stoppingToken);

        _logger.LogInformation("Job Compacted");

        return jobId.ToString();
    }

    // Enqueue (job) → mark Queued in DB, then enqueue into in-memory queue
    public override async Task EnqueueJobAsync(string job, Priority priority = Priority.Normal, CancellationToken stoppingToken = default)
    {
        if (int.TryParse(job, out var jobId))
        {
            await _db.ExecAsync(WJbQueue.Set_Queued, new { jobId, priority }, cancellationToken: stoppingToken);
        }

        await base.EnqueueJobAsync(job, priority, stoppingToken);

        _logger.LogInformation("Job Queued");
    }

    // Expand → hydrate compacted JSON from DB using JobId
    public override async Task<(string Type, JsonObject More)> ExpandAsync(string job, CancellationToken stoppingToken = default)
    {
        long jobId = 0;

        if (long.TryParse(job, out var parsedId))
        {
            jobId = parsedId;

            var codemore = await _db.ExecAsync<string>(WJbQueue.Get_CodeMore, jobId, cancellationToken: stoppingToken);
            if (!string.IsNullOrEmpty(codemore))
                job = codemore;
        }

        var result = await base.ExpandAsync(job, stoppingToken);

        if (jobId > 0) result.More["__jobId"] = jobId;

        _logger.LogInformation("Job Expanded");

        return result;
    }

    /// <summary>
    /// Logs "JobRunning" then "JobCompleted" (or "JobFailed" / "Job Cancelled"),
    /// updating DB status accordingly. Base ProcessJobAsync still handles Next-chaining & error boundaries.
    /// </summary>
    protected override async Task JobProcessCoreAsync(string actionType, JsonObject mergedMore, CancellationToken stoppingToken)
    {
        var jobId = mergedMore.GetInt64("__jobId");

        try
        {
            _logger.LogInformation("Job Running");
            if (jobId > 0)
                await _db.ExecAsync(WJbQueue.Set_Running, jobId, cancellationToken: stoppingToken);

            await base.JobProcessCoreAsync(actionType, mergedMore, stoppingToken);

            _logger.LogInformation("Job Completed");
            await SetFinishAsync(jobId, (int)JobStatus.Completed, stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Job Cancelled");
            await SetFinishAsync(jobId, (int)JobStatus.Cancelled, stoppingToken);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Job Failed");
            await SetFinishAsync(jobId, (int)JobStatus.Failed, stoppingToken);
            throw;
        }
    }

    private async Task SetFinishAsync(long? jobId, int jobStatus, CancellationToken cancellationToken = default)
    {
        try
        {
            if (jobId > 0)
                await _db.ExecAsync(WJbQueue.Finish, new { jobId, jobStatus }, cancellationToken: cancellationToken);
        }
        catch
        {
            // swallow finish errors to avoid masking the primary job exception
        }
    }

    private enum JobStatus
    {
        None = 0,
        Queued = 1,
        Running = 2,
        Completed = 3,
        Failed = 4,
        Cancelled = 5
    }

    private class WJbQueue
    {
        public static readonly string Ins = """
            INSERT INTO dbo.WJbQueue (ActionCode, JobMore)
            OUTPUT inserted.JobId
            SELECT JSON_VALUE(@Data, '$.code'), JSON_QUERY(@Data, '$.more');
            """;

        public static readonly string Set_Queued = """
            UPDATE dbo.WJbQueue
            SET JobPriority = @priority, JobStatus = 1  -- Queued
            WHERE JobId = @jobId AND JobStatus = 0;     -- New
            """;

        public static readonly string Set_Running = """
            UPDATE dbo.WJbQueue
            SET Started = GETDATE(), JobStatus = 2  -- Running
            WHERE JobId = @Data AND JobStatus = 1;  -- Queued
            """;

        public static readonly string Get_CodeMore = """
            SELECT ActionCode [code], JSON_QUERY(JobMore) [more]
            FROM dbo.WJbQueue
            WHERE JobId = @Data
            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER;
            """;

        public static readonly string Finish = """
            DELETE FROM dbo.WJbQueue
            OUTPUT 
                deleted.JobId,
                deleted.JobPriority,
                deleted.Created,
                @jobStatus AS JobStatus,
                deleted.ActionCode,
                deleted.JobMore,
                deleted.Started,
                GETDATE() AS Finished
            INTO dbo.WJbHistory (
                JobId, 
                JobPriority, 
                Created, 
                JobStatus, 
                ActionCode, 
                JobMore, 
                Started, 
                Finished
            )
            WHERE JobId = @jobId;
            """;
    }
}
