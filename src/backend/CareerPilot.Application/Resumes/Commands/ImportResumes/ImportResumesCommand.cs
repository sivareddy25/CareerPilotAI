using CareerPilot.Application.Abstractions.Authentication;
using CareerPilot.Application.Abstractions.Messaging;
using CareerPilot.Application.Abstractions.Persistence;
using CareerPilot.Application.Abstractions.Storage;
using CareerPilot.Application.Exceptions;
using CareerPilot.Application.Resumes.Models;
using CareerPilot.Application.Resumes.Services;
using CareerPilot.Domain.Resumes;
using Microsoft.Extensions.Logging;

namespace CareerPilot.Application.Resumes.Commands.ImportResumes;

/// <summary>
/// Imports one or more resume files.
/// </summary>
/// <remarks>
/// Takes transport-neutral <see cref="FileUploadRequest"/> values rather than
/// <c>IFormFile</c>, so nothing in the Application layer depends on ASP.NET and the
/// command is exercisable without a web host.
/// </remarks>
public sealed record ImportResumesCommand(IReadOnlyList<FileUploadRequest> Files)
    : ICommand<ResumeImportResultDto>;

internal sealed class ImportResumesCommandHandler(
    ICurrentUserService currentUser,
    IResumeRepository resumes,
    ResumeImportService importService,
    IUnitOfWork unitOfWork,
    ILogger<ImportResumesCommandHandler> logger)
    : ICommandHandler<ImportResumesCommand, ResumeImportResultDto>
{
    public async Task<ResumeImportResultDto> Handle(
        ImportResumesCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (currentUser.UserId is not { } userId)
        {
            throw new InvalidCredentialsException();
        }

        importService.EnsureBatchIsAcceptable(command.Files.Count);

        // Quota is checked against the whole batch up front rather than per file, so an
        // import either fits or is refused — importing four of ten and then stopping at
        // the limit would be a confusing partial result for a reason the user could
        // have been told immediately.
        var existing = await resumes.CountForUserAsync(userId, cancellationToken);
        importService.EnsureQuotaAllows(existing, command.Files.Count);

        var entries = new List<ResumeImportEntryDto>(command.Files.Count);
        var imported = 0;

        foreach (var file in command.Files)
        {
            var (document, format, warnings, error) = await importService.ParseAsync(file, cancellationToken);

            if (document is null)
            {
                entries.Add(new ResumeImportEntryDto(file.FileName, false, null, null, warnings, error));
                continue;
            }

            var title = DeriveTitle(document, file.FileName);

            var resume = Resume.CreateFromImport(
                userId,
                title,
                document,
                format,
                // Truncated because the name is user-controlled and only ever displayed.
                Truncate(file.FileName, 255));

            resumes.Add(resume);
            imported++;

            entries.Add(new ResumeImportEntryDto(file.FileName, true, resume.Id, title, warnings, null));
        }

        // One save for the batch: the files were parsed independently, but persisting
        // them is a single unit of work, so a database failure cannot leave half a batch
        // committed.
        if (imported > 0)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        logger.LogInformation(
            "Resume import completed. UserId: {UserId}, Files: {FileCount}, Imported: {ImportedCount}",
            userId,
            command.Files.Count,
            imported);

        return new ResumeImportResultDto(command.Files.Count, imported, entries);
    }

    /// <summary>
    /// Names the resume from its contents where possible, falling back to the file name.
    /// </summary>
    /// <remarks>
    /// A list of resumes all called "document" is useless, and the person's own name
    /// plus headline is what they would have typed anyway.
    /// </remarks>
    private static string DeriveTitle(ResumeDocument document, string fileName)
    {
        var name = document.Contact.FullName?.Trim();
        var headline = document.Contact.Headline?.Trim();

        if (!string.IsNullOrWhiteSpace(name))
        {
            return Truncate(
                string.IsNullOrWhiteSpace(headline) ? name : $"{name} — {headline}",
                200);
        }

        var withoutExtension = Path.GetFileNameWithoutExtension(fileName);

        return Truncate(
            string.IsNullOrWhiteSpace(withoutExtension) ? "Imported resume" : withoutExtension,
            200);
    }

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength];
}
