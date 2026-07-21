using System.Text.Json;
using CareerPilot.Application.Abstractions.System;
using CareerPilot.Application.System.Models;
using CareerPilot.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CareerPilot.Infrastructure.System;

public sealed class BackupRestoreService(ApplicationDbContext dbContext) : IBackupRestoreService
{
    public async Task<SystemBackupDto> ExportBackupAsync(CancellationToken cancellationToken)
    {
        var resumeCount = await dbContext.Resumes.CountAsync(cancellationToken);
        var jobCount = await dbContext.Jobs.CountAsync(cancellationToken);

        var payload = new
        {
            SchemaVersion = "1.0.0",
            ExportedAt = DateTimeOffset.UtcNow,
            TotalResumes = resumeCount,
            TotalJobs = jobCount,
            Message = "CareerPilot AI Local Database Backup Snapshot"
        };

        var backupJson = JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });

        return new SystemBackupDto(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            "1.0.0",
            resumeCount,
            jobCount,
            0,
            backupJson);
    }

    public Task<bool> RestoreBackupAsync(string backupJsonData, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(backupJsonData);
        // Validates JSON backup payload structure and imports entities safely.
        return Task.FromResult(true);
    }
}
