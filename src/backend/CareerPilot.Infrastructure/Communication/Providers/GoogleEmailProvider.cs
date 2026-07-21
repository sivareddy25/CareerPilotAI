using CareerPilot.Application.Abstractions.Communication;
using CareerPilot.Application.Communication.Models;
using CareerPilot.Domain.Communication;

namespace CareerPilot.Infrastructure.Communication.Providers;

public sealed class GoogleEmailProvider : IEmailProvider
{
    public CommunicationProviderKind ProviderKind => CommunicationProviderKind.Google;

    public Task<IReadOnlyList<RecruiterThreadDto>> FetchRecruiterThreadsAsync(string accessToken, CancellationToken cancellationToken)
    {
        IReadOnlyList<RecruiterThreadDto> threads = new List<RecruiterThreadDto>();
        return Task.FromResult(threads);
    }

    public Task<bool> SendEmailAsync(string accessToken, string toAddress, string subject, string body, CancellationToken cancellationToken)
    {
        return Task.FromResult(true);
    }
}
