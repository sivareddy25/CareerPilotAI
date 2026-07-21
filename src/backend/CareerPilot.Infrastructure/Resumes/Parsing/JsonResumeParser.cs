using System.Text.Json;
using CareerPilot.Application.Abstractions.Resumes;
using CareerPilot.Domain.Resumes;

namespace CareerPilot.Infrastructure.Resumes.Parsing;

/// <summary>
/// Reads the internal JSON format.
/// </summary>
/// <remarks>
/// <para>
/// The only lossless import path, and the counterpart to the JSON exporter. Together
/// they make a resume portable: export, move, re-import, and the document is byte-for
/// byte the same. PDF and DOCX cannot promise that, because their structure has to be
/// inferred.
/// </para>
/// <para>
/// Deserialises straight into <see cref="ResumeDocument"/> rather than through an
/// intermediate shape. The document is the contract, so a second parallel type would be
/// one more thing to keep in step for no benefit.
/// </para>
/// </remarks>
internal sealed class JsonResumeParser : IResumeParser
{
    /// <summary>
    /// Case-insensitive so a hand-edited or camel-cased file still loads. Comments and
    /// trailing commas are tolerated for the same reason: this format is meant to be
    /// editable by hand, and rejecting a file over a trailing comma would be hostile.
    /// </summary>
    internal static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    public ResumeFormat Format => ResumeFormat.Json;

    public async Task<ResumeParseResult> ParseAsync(
        Stream content,
        CancellationToken cancellationToken = default)
    {
        var warnings = new List<string>();

        ResumeDocument? document;

        try
        {
            document = await JsonSerializer.DeserializeAsync<ResumeDocument>(
                content,
                SerializerOptions,
                cancellationToken);
        }
        catch (JsonException exception)
        {
            // The message names the line and position, which is genuinely useful for a
            // hand-edited file and discloses nothing — the file is the user's own.
            warnings.Add($"The JSON could not be read: {exception.Message}");
            return new ResumeParseResult(ResumeDocument.Empty(), warnings);
        }

        if (document is null)
        {
            warnings.Add("The JSON file was empty.");
            return new ResumeParseResult(ResumeDocument.Empty(), warnings);
        }

        // A future schema is readable but may carry fields this build ignores. Warning
        // is better than either refusing the import or silently discarding data.
        if (document.SchemaVersion > ResumeDocument.CurrentSchemaVersion)
        {
            warnings.Add(
                $"This resume uses a newer format (version {document.SchemaVersion}). " +
                "Some fields may not have been imported.");
        }

        // Collections are non-nullable on the record but absent properties leave them
        // null after deserialisation, so they are normalised here rather than leaving
        // null-reference hazards for every consumer.
        return new ResumeParseResult(
            document with
            {
                SchemaVersion = ResumeDocument.CurrentSchemaVersion,
                Contact = document.Contact ?? new ResumeContact(),
                Experience = document.Experience ?? [],
                Education = document.Education ?? [],
                Skills = document.Skills ?? [],
                Projects = document.Projects ?? [],
                Certifications = document.Certifications ?? [],
                UnparsedSections = document.UnparsedSections ?? [],
            },
            warnings);
    }
}
