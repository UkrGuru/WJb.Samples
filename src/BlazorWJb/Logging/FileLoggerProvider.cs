using System.Text;
using System.Globalization;

namespace BlazorWJb.Logging;

internal sealed class FileLoggerProvider : ILoggerProvider
{
    private readonly string _logsDirectory;
    private readonly object _sync = new();
    private StreamWriter? _writer;
    private string _currentDateFileName = string.Empty;

    public LogLevel MinimumLevel { get; }
    private readonly int _retentionDays;

    // ✅ 3-argument constructor required by your extension
    public FileLoggerProvider(string logsDirectory, LogLevel minimumLevel, int retentionDays)
    {
        _logsDirectory = string.IsNullOrWhiteSpace(logsDirectory) ? "Logs" : logsDirectory;
        MinimumLevel = minimumLevel;
        _retentionDays = Math.Max(1, retentionDays);

        Directory.CreateDirectory(_logsDirectory);

        // Initialize writer and cleanup once at startup
        RolloverIfNeeded(DateTime.Now);
        CleanupOldFiles();
    }

    public ILogger CreateLogger(string categoryName) => new FileLogger(categoryName, this);

    public void Dispose()
    {
        lock (_sync)
        {
            _writer?.Flush();
            _writer?.Dispose();
            _writer = null!;
        }
    }

    internal void WriteLine(DateTime now, string line)
    {
        lock (_sync)
        {
            RolloverIfNeeded(now);
            _writer!.WriteLine(line);
            _writer.Flush(); // comment out for buffered writes if preferred
        }
    }

    private void RolloverIfNeeded(DateTime now)
    {
        var fileName = Path.Combine(_logsDirectory, $"{now:yyyyMMdd}.log");

        if (_writer == null || !string.Equals(_currentDateFileName, fileName, StringComparison.Ordinal))
        {
            _writer?.Flush();
            _writer?.Dispose();

            var stream = new FileStream(fileName, FileMode.Append, FileAccess.Write, FileShare.Read);
            _writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false))
            {
                AutoFlush = false
            };
            _currentDateFileName = fileName;

            // Cleanup on rollover as well
            CleanupOldFiles();
        }
    }

    /// <summary>
    /// Deletes log files older than the configured retention window (yyyyMMdd.log only).
    /// </summary>
    private void CleanupOldFiles()
    {
        try
        {
            var cutoffDate = DateTime.Now.Date.AddDays(-_retentionDays);

            foreach (var path in Directory.EnumerateFiles(_logsDirectory, "*.log"))
            {
                var name = Path.GetFileNameWithoutExtension(path);

                // Expect names like 20260102 (yyyyMMdd)
                if (name.Length == 8 &&
                    DateTime.TryParseExact(name, "yyyyMMdd", CultureInfo.InvariantCulture,
                                           DateTimeStyles.None, out var fileDate))
                {
                    if (fileDate.Date < cutoffDate)
                    {
                        TryDelete(path);
                    }
                }
            }
        }
        catch
        {
            // Intentionally ignore cleanup errors so logging never breaks.
        }
    }

    private static void TryDelete(string path)
    {
        try
        {
            File.Delete(path);
        }
        catch
        {
            // Ignore individual file delete failures (e.g., locked file).
        }
    }
}
