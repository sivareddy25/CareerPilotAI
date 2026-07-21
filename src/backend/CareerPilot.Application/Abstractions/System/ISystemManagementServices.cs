using CareerPilot.Application.System.Models;

namespace CareerPilot.Application.Abstractions.System;

public interface ISystemHealthService
{
    Task<SystemHealthStatusDto> CheckSystemHealthAsync(CancellationToken cancellationToken);
}

public interface IBackupRestoreService
{
    Task<SystemBackupDto> ExportBackupAsync(CancellationToken cancellationToken);
    Task<bool> RestoreBackupAsync(string backupJsonData, CancellationToken cancellationToken);
}

public interface IOllamaModelManagerService
{
    Task<IReadOnlyList<OllamaModelDto>> GetInstalledModelsAsync(CancellationToken cancellationToken);
}

public interface IUpdateCheckerService
{
    Task<UpdateStatusDto> CheckForUpdatesAsync(CancellationToken cancellationToken);
}

public interface IDiagnosticLogService
{
    Task<IReadOnlyList<LogEntryDto>> GetRecentLogsAsync(int count, string? levelFilter, CancellationToken cancellationToken);
}
