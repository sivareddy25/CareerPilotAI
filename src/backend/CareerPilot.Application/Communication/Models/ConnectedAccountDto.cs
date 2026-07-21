using CareerPilot.Domain.Communication;

namespace CareerPilot.Application.Communication.Models;

public sealed record ConnectedAccountDto(
    Guid Id,
    Guid UserId,
    CommunicationProviderKind ProviderKind,
    string ProviderName,
    string AccountEmail,
    bool IsConnected,
    DateTimeOffset ConnectedAt,
    DateTimeOffset? LastSyncedAt);
