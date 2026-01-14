using System.Text;

namespace BlazorWJb.Logging;

internal sealed class FileLogger : ILogger
{
    private readonly string _categoryName;
    private readonly FileLoggerProvider _provider;

    public FileLogger(string categoryName, FileLoggerProvider provider)
    {
        _categoryName = categoryName;
        _provider = provider;
    }

    public IDisposable? BeginScope<TState>(TState state) => default;

    public bool IsEnabled(LogLevel logLevel) =>
        logLevel != LogLevel.None && logLevel >= _provider.MinimumLevel;


    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;

        var now = DateTime.Now;
        var timestamp = now.ToString("HH:mm:ss.fff");

        string message;
        try { message = formatter(state, exception); }
        catch { message = state?.ToString() ?? string.Empty; }

        // ✅ Use short category
        var shortCategory = GetShortCategory(_categoryName);

        var sb = new StringBuilder(256);
        sb.Append(timestamp).Append(' ')
          .Append(ToShortLowerLevel(logLevel)).Append(": ")
          .Append(shortCategory).Append('[').Append(eventId.Id).Append("] ")
          .Append(message);

        if (exception != null)
        {
            sb.AppendLine();
            sb.Append(exception);
        }

        _provider.WriteLine(now, sb.ToString());
    }

    //private static string ToShortLevel(LogLevel level) => level switch
    //{
    //    LogLevel.Trace => "TRC",
    //    LogLevel.Debug => "DBG",
    //    LogLevel.Information => "INF",
    //    LogLevel.Warning => "WRN",
    //    LogLevel.Error => "ERR",
    //    LogLevel.Critical => "CRT",
    //    _ => "NON"
    //};


    private static string ToShortLowerLevel(LogLevel level) => level switch
    {
        LogLevel.Trace => "trace",
        LogLevel.Debug => "debug",
        LogLevel.Information => "info",
        LogLevel.Warning => "warn",
        LogLevel.Error => "error",
        LogLevel.Critical => "crit",
        _ => "none"
    };


    private static string GetShortCategory(string category, int lastSegments = 2)
    {
        if (string.IsNullOrEmpty(category)) return string.Empty;
        var parts = category.Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length <= lastSegments) return category;
        return string.Join('.', parts[^lastSegments..]);
    }

}
