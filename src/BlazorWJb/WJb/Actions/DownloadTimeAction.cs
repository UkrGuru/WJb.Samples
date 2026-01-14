
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using WJb;
using WJb.Extensions;

//namespace WJb.Actions;

public class DownloadTimeAction(HttpClient? httpClient = null, ILogger<DownloadTimeAction>? logger = null) : IAction
{
    private readonly HttpClient _httpClient = httpClient ?? new HttpClient();
    private readonly ILogger<DownloadTimeAction>? _logger = logger;

    // Results
    public string SourceUrl { get; private set; } = "https://time.now/developer/api/ip";
    public string RawResponse { get; private set; } = string.Empty;
    public DateTimeOffset? LocalTime { get; private set; }
    public DateTimeOffset? UtcTime { get; private set; }
    public string? TimeZoneId { get; private set; }
    public string? Abbreviation { get; private set; }

    public async Task ExecAsync(JsonObject? jobMore, CancellationToken stoppingToken)
    {
        // Allow overriding URL if needed; default is the IP-based endpoint
        SourceUrl = jobMore.GetString("url") ?? "https://time.now/developer/api/ip";

        // 1) Download JSON
        RawResponse = await _httpClient.GetStringAsync(SourceUrl, stoppingToken);

        // 2) Parse minimal fields we care about
        using var doc = JsonDocument.Parse(RawResponse);
        var root = doc.RootElement;

        LocalTime = TryParseIso(root, "datetime");
        UtcTime = TryParseIso(root, "utc_datetime");
        TimeZoneId = TryGetString(root, "timezone");
        Abbreviation = TryGetString(root, "abbreviation");

        if (UtcTime is null && LocalTime is not null)
            UtcTime = LocalTime.Value.ToUniversalTime();

        // 3) Log in a concise, structured way
        if (UtcTime is not null)
        {
            var systemUtc = DateTimeOffset.UtcNow;
            var skew = UtcTime.Value - systemUtc;

            _logger?.LogInformation(
                "Time.now: Local={Local:o}, TZ={TZ}, Abbr={Abbr}, SkewVsSystemUTC={Skew}",
                LocalTime, TimeZoneId ?? "(n/a)", Abbreviation ?? "(n/a)", skew.ToString("c", CultureInfo.InvariantCulture));
        }
        else
        {
            _logger?.LogWarning(
                "Time.now: could not parse UTC time. Raw length={Length:N0}.",
                RawResponse.Length);
        }
    }

    public Task ExecAsync(dynamic? jobMore, CancellationToken stoppingToken)
        => throw new NotImplementedException();

    private static DateTimeOffset? TryParseIso(JsonElement root, string field)
    {
        if (!root.TryGetProperty(field, out var p) || p.ValueKind != JsonValueKind.String) return null;
        var s = p.GetString();
        if (string.IsNullOrWhiteSpace(s)) return null;

        return DateTimeOffset.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dto)
            ? dto
            : null;
    }

    private static string? TryGetString(JsonElement root, string field)
        => root.TryGetProperty(field, out var p) && p.ValueKind == JsonValueKind.String ? p.GetString() : null;
}
