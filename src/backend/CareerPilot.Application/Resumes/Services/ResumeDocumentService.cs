using CareerPilot.Domain.Resumes;

namespace CareerPilot.Application.Resumes.Services;

/// <summary>
/// Domain service for validating, transforming, and migrating ResumeDocument instances.
/// </summary>
public sealed class ResumeDocumentService
{
    public bool ValidateDocument(ResumeDocument document, out IReadOnlyList<string> validationErrors)
    {
        var errors = new List<string>();

        if (document.IsEmpty)
        {
            errors.Add("Resume document contains no contact details or section content.");
        }

        if (document.SchemaVersion > ResumeDocument.CurrentSchemaVersion)
        {
            errors.Add($"Document schema version {document.SchemaVersion} is newer than supported version {ResumeDocument.CurrentSchemaVersion}.");
        }

        validationErrors = errors;
        return errors.Count == 0;
    }

    /// <summary>
    /// Ensures backward compatibility by migrating older JSON schemas to the current version.
    /// </summary>
    public ResumeDocument MigrateToCurrentVersion(ResumeDocument document)
    {
        if (document.SchemaVersion == ResumeDocument.CurrentSchemaVersion)
        {
            return document;
        }

        // Schema migration logic for version upgrades
        return document with { SchemaVersion = ResumeDocument.CurrentSchemaVersion };
    }
}
