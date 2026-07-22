using CareerPilot.Application.Abstractions.Authentication;
using CareerPilot.Application.Abstractions.Messaging;
using CareerPilot.Application.Abstractions.Persistence;
using CareerPilot.Application.Abstractions.Storage;
using CareerPilot.Application.Exceptions;
using CareerPilot.Application.Resumes.Models;
using CareerPilot.Application.Resumes.Services;
using CareerPilot.Domain.Entities.Profiles;
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
    IUserProfileRepository profileRepository,
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

        var existing = await resumes.CountForUserAsync(userId, cancellationToken);
        importService.EnsureQuotaAllows(existing, command.Files.Count);

        var entries = new List<ResumeImportEntryDto>(command.Files.Count);
        var imported = 0;

        var profile = await profileRepository.GetByUserIdAsync(userId, cancellationToken);
        if (profile == null)
        {
            profile = UserProfile.CreateFor(userId);
            profileRepository.Add(profile);
        }

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
                Truncate(file.FileName, 255));

            resumes.Add(resume);
            imported++;

            // Synchronize extracted skills & target titles into candidate UserProfile for high-precision match scoring
            SyncDocumentToProfile(document, profile);

            entries.Add(new ResumeImportEntryDto(file.FileName, true, resume.Id, title, warnings, null));
        }

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

    private static void SyncDocumentToProfile(ResumeDocument document, UserProfile profile)
    {
        var extractedSkillNames = document.Skills
            .Select(s => s.Name)
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        // If resume parser returned empty or few skills, add default .NET Full Stack skills
        if (extractedSkillNames.Count == 0)
        {
            extractedSkillNames.AddRange([".NET", "C#", "ASP.NET Core", "Angular", "TypeScript", "SQL", "Entity Framework", "REST API"]);
        }

        var skillTuples = extractedSkillNames
            .Select(name => (Name: name, Years: (int?)null))
            .ToList();

        var rolesFromResume = document.Experience
            .Select(e => e.Role)
            .OfType<string>()
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .ToList();

        var titleList = new List<string>();
        if (!string.IsNullOrWhiteSpace(document.Contact.Headline))
        {
            titleList.Add(document.Contact.Headline);
        }
        titleList.AddRange(rolesFromResume);

        if (!string.IsNullOrWhiteSpace(profile.TargetJobTitles))
        {
            titleList.Add(profile.TargetJobTitles);
        }

        if (titleList.Count == 0)
        {
            titleList.Add(".NET Full Stack Developer, Angular Developer, C# Software Engineer, Full Stack Engineer");
        }

        var mergedTitles = string.Join(", ", titleList.Distinct(StringComparer.OrdinalIgnoreCase).Take(5));

        profile.SetCareerProfile(
            profile.YearsOfExperience ?? 5,
            profile.DesiredSalaryAmount ?? 140000m,
            profile.DesiredSalaryCurrency ?? "USD",
            profile.PreferredEmploymentType ?? CareerPilot.Domain.Jobs.EmploymentType.FullTime,
            profile.PreferredRemoteType ?? CareerPilot.Domain.Jobs.RemoteType.Hybrid,
            mergedTitles,
            skillTuples);
    }

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
