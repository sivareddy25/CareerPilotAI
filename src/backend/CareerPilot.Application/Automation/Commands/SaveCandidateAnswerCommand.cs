using CareerPilot.Application.Abstractions.Authentication;
using CareerPilot.Application.Abstractions.Messaging;
using CareerPilot.Application.Abstractions.Persistence;
using CareerPilot.Domain.Entities.Automation;

namespace CareerPilot.Application.Automation.Commands;

public sealed record SaveCandidateAnswerCommand(
    string QuestionKey,
    string QuestionText,
    string AnswerText,
    string Category = "General") : ICommand<Guid>;

public sealed class SaveCandidateAnswerCommandHandler(
    ICurrentUserService currentUserService,
    ICandidateAnswerRepository answerRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<SaveCandidateAnswerCommand, Guid>
{
    public async Task<Guid> Handle(SaveCandidateAnswerCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId ?? Guid.Empty;
        var normalizedKey = CandidateAnswer.NormalizeKey(request.QuestionKey);

        var existing = await answerRepository.FindByQuestionKeyAsync(userId, normalizedKey, cancellationToken);
        if (existing is not null)
        {
            existing.UpdateAnswer(request.AnswerText);
            await answerRepository.UpdateAsync(existing, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return existing.Id;
        }

        var newAnswer = CandidateAnswer.Create(
            userId,
            normalizedKey,
            request.QuestionText,
            request.AnswerText,
            request.Category);

        await answerRepository.AddAsync(newAnswer, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return newAnswer.Id;
    }
}
