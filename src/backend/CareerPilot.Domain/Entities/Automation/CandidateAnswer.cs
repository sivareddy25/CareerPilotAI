using CareerPilot.Domain.Entities;

namespace CareerPilot.Domain.Entities.Automation;

public sealed class CandidateAnswer : EntityBase
{
    private CandidateAnswer() { }

    private CandidateAnswer(
        Guid userId,
        string questionKey,
        string questionText,
        string answerText,
        string category)
    {
        UserId = userId;
        QuestionKey = NormalizeKey(questionKey);
        QuestionText = questionText;
        AnswerText = answerText;
        Category = category;
        CreatedAt = DateTimeOffset.UtcNow;
        LastUsedAt = DateTimeOffset.UtcNow;
    }

    public Guid UserId { get; private set; }
    public string QuestionKey { get; private set; } = string.Empty;
    public string QuestionText { get; private set; } = string.Empty;
    public string AnswerText { get; private set; } = string.Empty;
    public string Category { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset LastUsedAt { get; private set; }

    public static CandidateAnswer Create(
        Guid userId,
        string questionKey,
        string questionText,
        string answerText,
        string category = "General")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(questionText);
        ArgumentException.ThrowIfNullOrWhiteSpace(answerText);

        return new CandidateAnswer(userId, questionKey, questionText, answerText, category);
    }

    public void UpdateAnswer(string newAnswerText)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newAnswerText);
        AnswerText = newAnswerText;
        LastUsedAt = DateTimeOffset.UtcNow;
    }

    public void RecordUsage()
    {
        LastUsedAt = DateTimeOffset.UtcNow;
    }

    public static string NormalizeKey(string rawQuestion)
    {
        if (string.IsNullOrWhiteSpace(rawQuestion)) return "unknown";
        var clean = rawQuestion.ToLowerInvariant().Trim();
        clean = new string(clean.Where(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c)).ToArray());
        return clean.Replace(' ', '_');
    }
}
