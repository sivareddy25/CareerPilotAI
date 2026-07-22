using CareerPilot.Domain.Entities.Automation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CareerPilot.Infrastructure.Persistence.Configurations;

public sealed class CandidateAnswerConfiguration : IEntityTypeConfiguration<CandidateAnswer>
{
    public void Configure(EntityTypeBuilder<CandidateAnswer> builder)
    {
        builder.ToTable("candidate_answers");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.UserId).IsRequired();
        builder.Property(a => a.QuestionKey).HasMaxLength(255).IsRequired();
        builder.Property(a => a.QuestionText).IsRequired();
        builder.Property(a => a.AnswerText).IsRequired();
        builder.Property(a => a.Category).HasMaxLength(100).IsRequired();
        builder.Property(a => a.CreatedAt).IsRequired();
        builder.Property(a => a.LastUsedAt).IsRequired();
    }
}
