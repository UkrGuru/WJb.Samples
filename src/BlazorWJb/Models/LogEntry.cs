namespace BlazorWJb.Models;

public record LogEntry(
    DateTime? Time,   // Parsed from prefix if present
    string Level,     // info | warn | error | etc.
    string Category,  // e.g., DownloadTimeAction, WJb.JobScheduler
    string Message    // trailing message
);
