using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using CareerPilot.Application.Abstractions.Resumes;
using CareerPilot.Domain.Resumes;

namespace CareerPilot.Infrastructure.Resumes.Export;

/// <summary>
/// Writes the internal JSON format.
/// </summary>
/// <remarks>
/// <para>
/// The lossless export, and the one that makes a resume genuinely portable: what this
/// writes, <c>JsonResumeParser</c> reads back identically. PDF and DOCX are for humans
/// and applicant systems; this is for keeping ownership of your own data.
/// </para>
/// <para>
/// Ignores the template argument, and should. A template is presentation, and JSON
/// carries none — writing the chosen template into the file would embed a rendering
/// decision in the portable format and break the rule that data and presentation stay
/// separate.
/// </para>
/// </remarks>
internal sealed class JsonResumeExporter : IResumeExporter
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        // Indented because these files are meant to be read and hand-edited; a resume is
        // small enough that the extra bytes do not matter.
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        // Absent rather than null: a resume is sparsely filled, and omitting empties
        // keeps the file readable instead of a wall of nulls.
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public ResumeFormat Format => ResumeFormat.Json;

    public Task<ResumeExportResult> ExportAsync(
        ResumeDocument document,
        ResumeTemplateKey template,
        string fileNameWithoutExtension,
        CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(document, SerializerOptions);

        return Task.FromResult(new ResumeExportResult(
            Encoding.UTF8.GetBytes(json),
            "application/json",
            $"{fileNameWithoutExtension}.json"));
    }
}
