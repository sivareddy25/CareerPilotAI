using CareerPilot.Application.Abstractions.Authentication;
using CareerPilot.Application.Abstractions.Messaging;
using CareerPilot.Application.Abstractions.Persistence;
using CareerPilot.Domain.Entities.Automation;

namespace CareerPilot.Application.Automation.Queries;

public sealed record CandidateAnswerDto(
    Guid Id,
    string QuestionKey,
    string QuestionText,
    string AnswerText,
    string Category,
    DateTimeOffset LastUsedAt);

public sealed record GetCandidateAnswersQuery : IQuery<IReadOnlyList<CandidateAnswerDto>>;

public sealed class GetCandidateAnswersQueryHandler(
    ICurrentUserService currentUserService,
    ICandidateAnswerRepository answerRepository) : IQueryHandler<GetCandidateAnswersQuery, IReadOnlyList<CandidateAnswerDto>>
{
    public async Task<IReadOnlyList<CandidateAnswerDto>> Handle(GetCandidateAnswersQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId ?? Guid.Empty;
        var answers = await answerRepository.GetByUserIdAsync(userId, cancellationToken);

        return answers.Select(a => new CandidateAnswerDto(
            a.Id,
            a.QuestionKey,
            a.QuestionText,
            a.AnswerText,
            a.Category,
            a.LastUsedAt)).ToList();
    }
}
