using CareerPilot.Application.Abstractions.Messaging;
using CareerPilot.Application.Abstractions.System;
using CareerPilot.Application.System.Models;

namespace CareerPilot.Application.System.Queries;

public sealed record GetSystemHealthQuery : IQuery<SystemHealthStatusDto>;

internal sealed class GetSystemHealthQueryHandler(ISystemHealthService systemHealthService)
    : IQueryHandler<GetSystemHealthQuery, SystemHealthStatusDto>
{
    public async Task<SystemHealthStatusDto> Handle(GetSystemHealthQuery query, CancellationToken cancellationToken)
    {
        return await systemHealthService.CheckSystemHealthAsync(cancellationToken);
    }
}
