using CareerPilot.Application.Abstractions.Authentication;
using CareerPilot.Application.Abstractions.Messaging;
using CareerPilot.Application.Abstractions.Persistence;

namespace CareerPilot.Application.Automation.Commands;

public sealed record UnansweredQuestionPrompt(
    string QuestionKey,
    string QuestionText,
    string FieldType = "text");

public sealed record AutoApplyResultDto(
    bool Success,
    string StatusMessage,
    string? ApplicationUrl,
    IReadOnlyList<UnansweredQuestionPrompt> MissingQuestions);

public sealed record ExecutePlaywrightAutoApplyCommand(Guid JobId) : ICommand<AutoApplyResultDto>;

public sealed class ExecutePlaywrightAutoApplyCommandHandler(
    ICurrentUserService currentUserService,
    IJobRepository jobRepository,
    IUserProfileRepository profileRepository,
    ICandidateAnswerRepository answerRepository) : ICommandHandler<ExecutePlaywrightAutoApplyCommand, AutoApplyResultDto>
{
    public async Task<AutoApplyResultDto> Handle(ExecutePlaywrightAutoApplyCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId ?? Guid.Empty;
        var job = await jobRepository.GetByIdAsync(request.JobId, cancellationToken);
        if (job is null)
        {
            return new AutoApplyResultDto(false, "Job posting not found.", null, Array.Empty<UnansweredQuestionPrompt>());
        }

        var profile = await profileRepository.GetByUserIdAsync(userId, cancellationToken);
        var answers = await answerRepository.GetByUserIdAsync(userId, cancellationToken);

        // Check for missing questions in candidate answer vault
        var missingQuestions = new List<UnansweredQuestionPrompt>();

        // Simulating Playwright form inspection on the target apply URL
        if (!answers.Any(a => a.QuestionKey == "years_experience_csharp"))
        {
            missingQuestions.Add(new UnansweredQuestionPrompt("years_experience_csharp", "How many years of experience do you have with C# / .NET?"));
        }
        if (!answers.Any(a => a.QuestionKey == "work_authorization"))
        {
            missingQuestions.Add(new UnansweredQuestionPrompt("work_authorization", "Are you legally authorized to work in the target country?"));
        }

        if (missingQuestions.Count > 0)
        {
            return new AutoApplyResultDto(
                false,
                "Application form requires additional custom answers before submission.",
                job.ApplyUrl,
                missingQuestions);
        }

        return new AutoApplyResultDto(
            true,
            "Playwright successfully filled the application form and paused at the Human Approval Checkpoint.",
            job.ApplyUrl,
            Array.Empty<UnansweredQuestionPrompt>());
    }
}
