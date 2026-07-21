using CareerPilot.Application.Abstractions.Resumes;
using CareerPilot.Domain.Resumes;

namespace CareerPilot.Infrastructure.Resumes;

/// <summary>
/// Resolves parsers by format.
/// </summary>
/// <remarks>
/// Built from whatever <see cref="IResumeParser"/> implementations are registered, so
/// adding a format is a single DI registration — nothing here or above changes. The
/// last registration for a format wins, which lets a deployment substitute a better
/// parser without removing the original.
/// </remarks>
internal sealed class ResumeParserRegistry : IResumeParserRegistry
{
    private readonly Dictionary<ResumeFormat, IResumeParser> _parsers;

    public ResumeParserRegistry(IEnumerable<IResumeParser> parsers)
    {
        _parsers = parsers.ToDictionary(parser => parser.Format);
    }

    public IResumeParser? For(ResumeFormat format) =>
        _parsers.GetValueOrDefault(format);

    public IReadOnlyCollection<ResumeFormat> SupportedFormats => _parsers.Keys;
}

/// <summary>
/// Resolves exporters by format. Mirrors <see cref="ResumeParserRegistry"/>.
/// </summary>
internal sealed class ResumeExporterRegistry : IResumeExporterRegistry
{
    private readonly Dictionary<ResumeFormat, IResumeExporter> _exporters;

    public ResumeExporterRegistry(IEnumerable<IResumeExporter> exporters)
    {
        _exporters = exporters.ToDictionary(exporter => exporter.Format);
    }

    public IResumeExporter? For(ResumeFormat format) =>
        _exporters.GetValueOrDefault(format);

    public IReadOnlyCollection<ResumeFormat> SupportedFormats => _exporters.Keys;
}
