namespace CareerPilot.Application.System.Models;

public sealed record OllamaModelDto(
    string Name,
    string ModelFamily,
    long SizeBytes,
    string FormattedSize,
    string Digest,
    DateTimeOffset ModifiedAt);

public sealed record SystemBackupDto(
    Guid BackupId,
    DateTimeOffset ExportedAt,
    string SchemaVersion,
    int TotalResumes,
    int TotalJobs,
    int TotalApplications,
    string BackupJsonData);

public sealed record UpdateStatusDto(
    string CurrentVersion,
    string LatestVersion,
    bool IsUpdateAvailable,
    string ReleaseNotes,
    string DownloadUrl,
    DateTimeOffset CheckedAt);

public sealed record LogEntryDto(
    Guid Id,
    DateTimeOffset Timestamp,
    string LogLevel,
    string Message,
    string SourceCategory,
    string? ExceptionDetails);
