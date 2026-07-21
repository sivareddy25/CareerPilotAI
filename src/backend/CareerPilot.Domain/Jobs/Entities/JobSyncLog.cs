using CareerPilot.Domain.Entities;

namespace CareerPilot.Domain.Jobs.Entities;

public sealed class JobSyncLog : EntityBase
{
    private JobSyncLog()
    {
    }

    public JobSyncLog(
        JobProviderKind provider,
        DateTimeOffset startedAt,
        DateTimeOffset completedAt,
        bool success,
        int processedCount,
        int insertedCount,
        int updatedCount,
        string? failureReason = null)
    {
        Provider = provider;
        StartedAt = startedAt;
        CompletedAt = completedAt;
        Success = success;
        ProcessedCount = processedCount;
        InsertedCount = insertedCount;
        UpdatedCount = updatedCount;
        FailureReason = failureReason;
    }

    public JobProviderKind Provider { get; private set; }
    public DateTimeOffset StartedAt { get; private set; }
    public DateTimeOffset CompletedAt { get; private set; }
    public bool Success { get; private set; }
    public int ProcessedCount { get; private set; }
    public int InsertedCount { get; private set; }
    public int UpdatedCount { get; private set; }
    public string? FailureReason { get; private set; }
}
