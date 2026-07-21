using CareerPilot.Application.Abstractions.Communication;
using CareerPilot.Application.Abstractions.Messaging;
using CareerPilot.Application.Communication.Models;
using CareerPilot.Domain.Communication;

namespace CareerPilot.Application.Communication.Commands;

public sealed record GenerateReplyDraftCommand(
    Guid UserId,
    Guid ThreadId,
    ReplyTone Tone,
    string? CustomInstruction) : ICommand<ReplyDraftDto>;

internal sealed class GenerateReplyDraftCommandHandler(IReplyGenerationService replyGenerationService)
    : ICommandHandler<GenerateReplyDraftCommand, ReplyDraftDto>
{
    public async Task<ReplyDraftDto> Handle(GenerateReplyDraftCommand command, CancellationToken cancellationToken)
    {
        var dummyThread = new RecruiterThreadDto(
            command.ThreadId,
            command.UserId,
            Guid.NewGuid(),
            "Interview Invitation: Senior Full Stack Engineer at TechCorp Systems",
            "TechCorp Systems",
            "Sarah Jenkins",
            "sarah.jenkins@techcorp.com",
            EmailCategory.InterviewInvitation,
            "Interview Invitation",
            EmailPriority.High,
            true,
            DateTimeOffset.UtcNow,
            new List<EmailMessageDto>(),
            new List<string>());

        return await replyGenerationService.GenerateReplyDraftAsync(dummyThread, command.Tone, command.CustomInstruction, cancellationToken);
    }
}
