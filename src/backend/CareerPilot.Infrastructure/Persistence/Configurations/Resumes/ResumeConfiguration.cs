using System.Text.Json;
using CareerPilot.Domain.Resumes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CareerPilot.Infrastructure.Persistence.Configurations.Resumes;

public sealed class ResumeConfiguration : EntityTypeConfiguration<Resume>
{
    private static readonly JsonSerializerOptions DocumentSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
    };

    public override void Configure(EntityTypeBuilder<Resume> builder)
    {
        base.Configure(builder);

        builder.ToTable("resumes");

        builder.Property(resume => resume.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(resume => resume.Template).HasConversion<int>();
        builder.Property(resume => resume.ImportedFrom).HasConversion<int?>();
        builder.Property(resume => resume.ImportedFileName).HasMaxLength(255);

        // The document is stored as jsonb, not shredded across a dozen tables.
        //
        // Resume content is document-shaped: ordered, nested, sparsely populated and
        // still evolving. Normalising it would fix a schema this phase is not in a
        // position to choose — the editing experience that will actually exercise it is
        // Phase 8's — and every read would become a six-way join to rebuild a value
        // object that is always loaded whole anyway.
        //
        // jsonb rather than json: it is stored parsed, so PostgreSQL can index into it
        // when later phases need to query inside a resume.
        builder.Property(resume => resume.Document)
            .HasColumnName("document")
            .HasColumnType("jsonb")
            .IsRequired()
            .HasConversion(
                document => JsonSerializer.Serialize(document, DocumentSerializerOptions),
                json => JsonSerializer.Deserialize<ResumeDocument>(json, DocumentSerializerOptions)!,
                // Without an explicit comparer EF compares the converted strings by
                // reference and never detects a change, so an edited document would
                // silently fail to save. Snapshotting via a round-trip is correct
                // because ResumeDocument is a deeply immutable record.
                new ValueComparer<ResumeDocument>(
                    (left, right) => JsonSerializer.Serialize(left, DocumentSerializerOptions)
                                     == JsonSerializer.Serialize(right, DocumentSerializerOptions),
                    document => JsonSerializer.Serialize(document, DocumentSerializerOptions).GetHashCode(),
                    document => JsonSerializer.Deserialize<ResumeDocument>(
                        JsonSerializer.Serialize(document, DocumentSerializerOptions),
                        DocumentSerializerOptions)!));

        // Every listing is "this user's resumes, newest first", so the index carries the
        // sort column too and the query never needs a separate sort step.
        builder.HasIndex(resume => new { resume.UserId, resume.CreatedAt })
            .HasDatabaseName("ix_resumes_user_id_created_at");

        builder.HasOne(resume => resume.User)
            .WithMany()
            .HasForeignKey(resume => resume.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        // Mirrors the owning user's soft-delete state as well as the resume's own, so a
        // deleted account's resumes cannot surface through this table.
        builder.HasQueryFilter(resume => !resume.IsDeleted && !resume.User!.IsDeleted);
    }
}
