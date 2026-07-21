using CareerPilot.Application.Abstractions.Messaging;
using CareerPilot.Application.Abstractions.System;
using CareerPilot.Application.System.Models;

namespace CareerPilot.Application.System.Queries;

public sealed record GetOllamaModelsQuery : IQuery<IReadOnlyList<OllamaModelDto>>;

internal sealed class GetOllamaModelsQueryHandler(IOllamaModelManagerService ollamaService)
    : IQueryHandler<GetOllamaModelsQuery, IReadOnlyList<OllamaModelDto>>
{
    public async Task<IReadOnlyList<OllamaModelDto>> Handle(GetOllamaModelsQuery query, CancellationToken cancellationToken)
    {
        return await ollamaService.GetInstalledModelsAsync(cancellationToken);
    }
}

public sealed record CheckUpdatesQuery : IQuery<UpdateStatusDto>;

internal sealed class CheckUpdatesQueryHandler(IUpdateCheckerService updateCheckerService)
    : IQueryHandler<CheckUpdatesQuery, UpdateStatusDto>
{
    public async Task<UpdateStatusDto> Handle(CheckUpdatesQuery query, CancellationToken cancellationToken)
    {
        return await updateCheckerService.CheckForUpdatesAsync(cancellationToken);
    }
}

public sealed record GetDiagnosticLogsQuery(int Count = 50, string? LevelFilter = null) : IQuery<IReadOnlyList<LogEntryDto>>;

internal sealed class GetDiagnosticLogsQueryHandler(IDiagnosticLogService logService)
    : IQueryHandler<GetDiagnosticLogsQuery, IReadOnlyList<LogEntryDto>>
{
    public async Task<IReadOnlyList<LogEntryDto>> Handle(GetDiagnosticLogsQuery query, CancellationToken cancellationToken)
    {
        return await logService.GetRecentLogsAsync(query.Count, query.LevelFilter, cancellationToken);
    }
}
