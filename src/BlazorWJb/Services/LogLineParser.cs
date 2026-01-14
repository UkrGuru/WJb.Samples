using System.Text.RegularExpressions;
using BlazorWJb.Models;

namespace BlazorWJb.Services;

public static class LogLineParser
{
    // 3 forms supported:
    // 1) "HH:mm:ss.fff level: Category[0] Message"
    // 2) "yyyy-MM-dd HH:mm:ss.fff level: Category[0] Message"
    // 3) "level: Category[0] Message"
    private static readonly Regex Rx = new(
        @"^(?:(?<full>\d{4}-\d{2}-\d{2}\s+\d{2}:\d{2}:\d{2}(?:\.\d{1,9})?)|(?<time>\d{2}:\d{2}:\d{2}(?:\.\d{1,9})?))?\s*(?<level>\w+):\s+(?<cat>[^\s\[]+)(?:\[\d+\])?\s+(?<msg>.+)$",
        RegexOptions.Compiled);

    public static IEnumerable<LogEntry> Parse(IEnumerable<string> lines, DateTime selectedDate)
    {
        foreach (var line in lines)
        {
            var m = Rx.Match(line);
            if (!m.Success) continue;

            DateTime? dt = null;

            if (m.Groups["full"].Success)
            {
                // Full date-time present
                if (DateTime.TryParse(m.Groups["full"].Value, out var parsedFull))
                    dt = parsedFull;
            }
            else if (m.Groups["time"].Success)
            {
                // Time-of-day only; attach selected date
                if (TimeSpan.TryParse(m.Groups["time"].Value, out var t))
                    dt = selectedDate.Date.Add(t);
            }

            var level = m.Groups["level"].Value.ToLowerInvariant();
            var category = m.Groups["cat"].Value;
            var message = m.Groups["msg"].Value.Trim();

            yield return new LogEntry(dt, level, category, message);
        }
    }
}
