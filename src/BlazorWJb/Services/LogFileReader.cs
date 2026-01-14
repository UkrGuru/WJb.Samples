using Microsoft.Extensions.Hosting;

namespace BlazorWJb.Services;

public interface ILogFileReader
{
    Task<IEnumerable<string>> ReadLogLinesAsync(DateTime date, CancellationToken ct = default);
}

public class LogFileReader : ILogFileReader
{
    private readonly IHostEnvironment _env;

    public LogFileReader(IHostEnvironment env) => _env = env;

    public async Task<IEnumerable<string>> ReadLogLinesAsync(DateTime date, CancellationToken ct = default)
    {
        var name = $"{date:yyyyMMdd}.log";
        var path = Path.Combine(_env.ContentRootPath, "Logs", name);

        if (!File.Exists(path))
            return Array.Empty<string>();

        // Stream read: avoid loading huge files into memory at once
        var list = new List<string>(capacity: 4096);
        using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var sr = new StreamReader(fs);
        while (!sr.EndOfStream && !ct.IsCancellationRequested)
        {
            var line = await sr.ReadLineAsync();
            if (line is not null) list.Add(line);
        }
        return list;
    }
}

