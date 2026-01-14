
namespace BlazorWJb.Logging
{
    public sealed class FileLoggerOptions
    {
        public string LogsDirectory { get; set; } = "Logs";
        public LogLevel MinimumLevel { get; set; } = LogLevel.Information;
        public int RetentionDays { get; set; } = 30;
        public bool UseUtc { get; set; } = false; // optional if you want to log UTC
        public int ShortCategoryLastSegments { get; set; } = 2; // optional (Counter vs Pages.Counter)
    }

    public static class FileLoggerExtensions
    {
        /// <summary>
        /// Adds the daily file logger using options from configuration section: Logging:File
        /// </summary>
        public static ILoggingBuilder AddDailyFile(this ILoggingBuilder builder, IConfiguration configuration)
        {
            var section = configuration.GetSection("Logging:File");
            var opts = section.Get<FileLoggerOptions>() ?? new FileLoggerOptions();

            builder.AddProvider(new FileLoggerProvider(
                logsDirectory: opts.LogsDirectory,
                minimumLevel: opts.MinimumLevel,
                retentionDays: opts.RetentionDays));

            // Optional: also add filters using the general Logging:LogLevel settings
            // builder.AddFilter((category, level) => level >= opts.MinimumLevel && category.StartsWith("BlazorApp11"));

            return builder;
        }
    }
}
