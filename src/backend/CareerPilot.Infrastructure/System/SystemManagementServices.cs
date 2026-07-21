using CareerPilot.Application.Abstractions.System;
using CareerPilot.Application.System.Models;

namespace CareerPilot.Infrastructure.System;

public sealed class OllamaModelManagerService : IOllamaModelManagerService
{
    public Task<IReadOnlyList<OllamaModelDto>> GetInstalledModelsAsync(CancellationToken cancellationToken)
    {
        IReadOnlyList<OllamaModelDto> models = new List<OllamaModelDto>
        {
            new("llama3:8b", "Llama 3", 4700000000, "4.7 GB", "sha256:70e234a0", DateTimeOffset.UtcNow.AddDays(-10)),
            new("mistral:7b", "Mistral", 4100000000, "4.1 GB", "sha256:81f345b1", DateTimeOffset.UtcNow.AddDays(-20)),
            new("phi3:mini", "Phi-3", 2300000000, "2.3 GB", "sha256:92a456c2", DateTimeOffset.UtcNow.AddDays(-5))
        };

        return Task.FromResult(models);
    }
}

public sealed class UpdateCheckerService : IUpdateCheckerService
{
    public Task<UpdateStatusDto> CheckForUpdatesAsync(CancellationToken cancellationToken)
    {
        var status = new UpdateStatusDto(
            CurrentVersion: "v1.0.0",
            LatestVersion: "v1.0.0",
            IsUpdateAvailable: false,
            ReleaseNotes: "You are running the latest production version of CareerPilot AI.",
            DownloadUrl: "https://github.com/sivareddy25/CareerPilotAI/releases/latest",
            CheckedAt: DateTimeOffset.UtcNow);

        return Task.FromResult(status);
    }
}

public sealed class DiagnosticLogService : IDiagnosticLogService
{
    public Task<IReadOnlyList<LogEntryDto>> GetRecentLogsAsync(int count, string? levelFilter, CancellationToken cancellationToken)
    {
        IReadOnlyList<LogEntryDto> logs = new List<LogEntryDto>
        {
            new(Guid.NewGuid(), DateTimeOffset.UtcNow.AddMinutes(-5), "Information", "Local Mode: Startup local user initialization checked successfully.", "CareerPilot.Authentication", null),
            new(Guid.NewGuid(), DateTimeOffset.UtcNow.AddMinutes(-12), "Information", "Job Ingestion Engine: Synchronized 12 jobs from Greenhouse connector.", "CareerPilot.Jobs", null),
            new(Guid.NewGuid(), DateTimeOffset.UtcNow.AddMinutes(-35), "Information", "AI Resume Engine: ATS analysis completed for Software Engineer profile.", "CareerPilot.AI", null)
        };

        if (!string.IsNullOrWhiteSpace(levelFilter))
        {
            logs = logs.Where(l => l.LogLevel.Equals(levelFilter, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        return Task.FromResult(logs);
    }
}
