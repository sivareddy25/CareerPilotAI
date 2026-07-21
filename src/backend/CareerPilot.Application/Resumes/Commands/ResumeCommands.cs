using CareerPilot.Application.Abstractions.Authentication;
using CareerPilot.Application.Abstractions.Messaging;
using CareerPilot.Application.Abstractions.Persistence;
using CareerPilot.Application.Abstractions.Resumes;
using CareerPilot.Application.Exceptions;
using CareerPilot.Application.Resumes.Models;
using CareerPilot.Application.Resumes.Services;
using CareerPilot.Domain.Resumes;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace CareerPilot.Application.Resumes.Commands;

/// <summary>
/// Changes a resume's template.
/// </summary>
/// <remarks>
/// Its own command, not a field on a general update. Switching template is presentation
/// only and provably cannot alter content — the handler touches nothing but
/// <see cref="Resume.Template"/>. Folding it into a document update would mean the
/// gallery had to round-trip the entire resume to change a look, and any staleness in
/// that payload would overwrite real content.
/// </remarks>
public sealed record SwitchResumeTemplateCommand(Guid ResumeId, ResumeTemplateKey Template)
    : ICommand<ResumeDto>;

internal sealed class SwitchResumeTemplateCommandValidator : AbstractValidator<SwitchResumeTemplateCommand>
{
    public SwitchResumeTemplateCommandValidator()
    {
        RuleFor(command => command.ResumeId).NotEmpty();

        // Model binding will deserialise an out-of-range integer into an enum without
        // complaint, so an unchecked value would be persisted and then fail to render.
        RuleFor(command => command.Template).IsInEnum();
    }
}

internal sealed class SwitchResumeTemplateCommandHandler(
    ICurrentUserService currentUser,
    IResumeRepository resumes,
    IUnitOfWork unitOfWork)
    : ICommandHandler<SwitchResumeTemplateCommand, ResumeDto>
{
    public async Task<ResumeDto> Handle(
        SwitchResumeTemplateCommand command,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId)
        {
            throw new InvalidCredentialsException();
        }

        var resume = await resumes.GetOwnedAsync(command.ResumeId, userId, cancellationToken)
            ?? throw new ResumeNotFoundException();

        resume.ApplyTemplate(command.Template);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ResumeDto.From(resume);
    }
}

/// <summary>Renames a resume. Title is metadata, not document content.</summary>
public sealed record RenameResumeCommand(Guid ResumeId, string Title) : ICommand<ResumeDto>;

internal sealed class RenameResumeCommandValidator : AbstractValidator<RenameResumeCommand>
{
    public RenameResumeCommandValidator()
    {
        RuleFor(command => command.ResumeId).NotEmpty();

        RuleFor(command => command.Title)
            .NotEmpty().WithMessage("A title is required.")
            .MaximumLength(200).WithMessage("Title must be at most 200 characters.");
    }
}

internal sealed class RenameResumeCommandHandler(
    ICurrentUserService currentUser,
    IResumeRepository resumes,
    IUnitOfWork unitOfWork)
    : ICommandHandler<RenameResumeCommand, ResumeDto>
{
    public async Task<ResumeDto> Handle(RenameResumeCommand command, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId)
        {
            throw new InvalidCredentialsException();
        }

        var resume = await resumes.GetOwnedAsync(command.ResumeId, userId, cancellationToken)
            ?? throw new ResumeNotFoundException();

        resume.Rename(command.Title);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ResumeDto.From(resume);
    }
}

/// <summary>
/// Soft-deletes a resume.
/// </summary>
/// <remarks>
/// Soft, consistent with the rest of the system: the row survives for audit and the
/// global query filter hides it. Included here because import can produce unwanted
/// resumes — an import feature with no way to undo a bad import is incomplete, and it
/// is also what makes the per-user quota escapable.
/// </remarks>
public sealed record DeleteResumeCommand(Guid ResumeId) : ICommand<Unit>;

internal sealed class DeleteResumeCommandHandler(
    ICurrentUserService currentUser,
    IResumeRepository resumes,
    IUnitOfWork unitOfWork,
    ILogger<DeleteResumeCommandHandler> logger)
    : ICommandHandler<DeleteResumeCommand, Unit>
{
    public async Task<Unit> Handle(DeleteResumeCommand command, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId)
        {
            throw new InvalidCredentialsException();
        }

        var resume = await resumes.GetOwnedAsync(command.ResumeId, userId, cancellationToken)
            ?? throw new ResumeNotFoundException();

        resume.IsDeleted = true;
        resume.DeletedAt = DateTime.UtcNow;
        resume.DeletedBy = userId.ToString();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Resume deleted. ResumeId: {ResumeId}, UserId: {UserId}", resume.Id, userId);

        return Unit.Value;
    }
}

/// <summary>
/// Renders a resume for download.
/// </summary>
/// <remarks>
/// A command rather than a query despite not mutating state: it is invoked by POST so
/// the template override travels in a body, and it produces a side effect worth
/// auditing. <see cref="TemplateOverride"/> lets the export dialog render in a template
/// without changing the one the resume is stored with.
/// </remarks>
public sealed record ExportResumeCommand(
    Guid ResumeId,
    ResumeFormat Format,
    ResumeTemplateKey? TemplateOverride) : ICommand<ResumeExportResult>;

internal sealed class ExportResumeCommandValidator : AbstractValidator<ExportResumeCommand>
{
    public ExportResumeCommandValidator()
    {
        RuleFor(command => command.ResumeId).NotEmpty();
        RuleFor(command => command.Format).IsInEnum();
        RuleFor(command => command.TemplateOverride).IsInEnum().When(c => c.TemplateOverride.HasValue);
    }
}

internal sealed class ExportResumeCommandHandler(
    ICurrentUserService currentUser,
    IResumeRepository resumes,
    ResumeExportService exportService)
    : ICommandHandler<ExportResumeCommand, ResumeExportResult>
{
    public async Task<ResumeExportResult> Handle(
        ExportResumeCommand command,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId)
        {
            throw new InvalidCredentialsException();
        }

        // Ownership is resolved before any rendering happens, so an export can never be
        // the path by which someone reads a resume they do not own.
        var resume = await resumes.GetOwnedAsync(command.ResumeId, userId, cancellationToken)
            ?? throw new ResumeNotFoundException();

        return await exportService.ExportAsync(
            resume,
            command.Format,
            command.TemplateOverride,
            cancellationToken);
    }
}
