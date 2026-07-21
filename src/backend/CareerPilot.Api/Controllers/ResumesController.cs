using CareerPilot.Application.Abstractions.Resumes;
using CareerPilot.Application.Abstractions.Storage;
using CareerPilot.Application.Resumes.Commands;
using CareerPilot.Application.Resumes.Commands.ImportResumes;
using CareerPilot.Application.Resumes.Models;
using CareerPilot.Application.Resumes.Queries;
using CareerPilot.Domain.Resumes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareerPilot.Api.Controllers;

/// <summary>
/// Resume import, export, templates and retrieval.
/// </summary>
[Authorize]
[Route("api/v{version:apiVersion}/resumes")]
public sealed class ResumesController : BaseApiController
{
    /// <summary>Lists the caller's resumes. Metadata only, no documents.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ResumeSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ResumeSummaryDto>>> List(CancellationToken cancellationToken)
    {
        var resumes = await Queries.Query(new ListResumesQuery(), cancellationToken);
        Response.Headers.CacheControl = "no-store, private";
        return Ok(resumes);
    }

    /// <summary>Returns one resume with its full document. Drives the live preview.</summary>
    [HttpGet("{resumeId:guid}")]
    [ProducesResponseType(typeof(ResumeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ResumeDto>> Get(Guid resumeId, CancellationToken cancellationToken)
    {
        var resume = await Queries.Query(new GetResumeQuery(resumeId), cancellationToken);
        Response.Headers.CacheControl = "no-store, private";
        return Ok(resume);
    }

    /// <summary>The template catalogue.</summary>
    [HttpGet("templates")]
    [ProducesResponseType(typeof(IReadOnlyList<ResumeTemplateDescriptor>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ResumeTemplateDescriptor>>> Templates(
        CancellationToken cancellationToken)
    {
        var templates = await Queries.Query(new GetResumeTemplatesQuery(), cancellationToken);
        Response.Headers.CacheControl = "private, max-age=3600";
        return Ok(templates);
    }

    /// <summary>Renders live HTML preview of a resume.</summary>
    [HttpGet("{resumeId:guid}/preview-html")]
    [ProducesResponseType(typeof(ContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPreviewHtml(
        Guid resumeId,
        [FromQuery] ResumeTemplateKey? templateKey,
        [FromServices] IResumeRenderer renderer,
        [FromServices] IResumeTemplateCatalog catalog,
        CancellationToken cancellationToken)
    {
        var resume = await Queries.Query(new GetResumeQuery(resumeId), cancellationToken);
        var targetTemplateKey = templateKey ?? (ResumeTemplateKey)resume.Template;
        var descriptor = catalog.Get(targetTemplateKey);

        var doc = new ResumeDocument
        {
            Contact = new ResumeContact
            {
                FullName = resume.Document.Contact.FullName,
                Headline = resume.Document.Contact.Headline,
                Email = resume.Document.Contact.Email,
                Phone = resume.Document.Contact.Phone,
                Location = resume.Document.Contact.Location,
                Website = resume.Document.Contact.Website,
                LinkedIn = resume.Document.Contact.LinkedIn,
                GitHub = resume.Document.Contact.GitHub
            },
            Summary = resume.Document.Summary,
            Experience = resume.Document.Experience.Select(e => new ResumeExperience
            {
                Company = e.Company,
                Role = e.Role,
                Location = e.Location,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                IsCurrent = e.IsCurrent,
                Highlights = e.Highlights
            }).ToList(),
            Education = resume.Document.Education.Select(e => new ResumeEducation
            {
                Institution = e.Institution,
                Degree = e.Degree,
                FieldOfStudy = e.FieldOfStudy,
                Location = e.Location,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                Grade = e.Grade
            }).ToList(),
            Skills = resume.Document.Skills.Select(s => new ResumeSkill
            {
                Name = s.Name,
                Category = s.Category
            }).ToList(),
            Projects = resume.Document.Projects.Select(p => new ResumeProject
            {
                Name = p.Name,
                Description = p.Description,
                Url = p.Url,
                Highlights = p.Highlights
            }).ToList(),
            Certifications = resume.Document.Certifications.Select(c => new ResumeCertification
            {
                Name = c.Name,
                Issuer = c.Issuer,
                IssuedDate = c.IssuedDate,
                ExpiryDate = c.ExpiryDate,
                CredentialUrl = c.CredentialUrl
            }).ToList()
        };

        var html = renderer.RenderHtml(doc, descriptor);
        return Content(html, "text/html");
    }

    /// <summary>Imports one or more resume files.</summary>
    [HttpPost("import")]
    [RequestSizeLimit(55 * 1024 * 1024)]
    [ProducesResponseType(typeof(ResumeImportResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ResumeImportResultDto>> Import(
        [FromForm] IFormFileCollection files,
        CancellationToken cancellationToken)
    {
        if (files is null || files.Count == 0)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "No files were uploaded.",
                Detail = "Attach at least one file using the 'files' form field.",
            });
        }

        var streams = new List<Stream>(files.Count);

        try
        {
            var requests = new List<FileUploadRequest>(files.Count);

            foreach (var file in files)
            {
                var stream = file.OpenReadStream();
                streams.Add(stream);

                requests.Add(new FileUploadRequest(stream, file.FileName, file.ContentType, file.Length));
            }

            var result = await Commands.Send(new ImportResumesCommand(requests), cancellationToken);
            return Ok(result);
        }
        finally
        {
            foreach (var stream in streams)
            {
                await stream.DisposeAsync();
            }
        }
    }

    /// <summary>Renders a resume and returns it as a download.</summary>
    [HttpPost("{resumeId:guid}/export")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Export(
        Guid resumeId,
        [FromBody] ExportResumeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await Commands.Send(
            new ExportResumeCommand(resumeId, request.Format, request.Template),
            cancellationToken);

        return File(result.Content, result.ContentType, result.FileName);
    }

    /// <summary>Switches a resume's template.</summary>
    [HttpPut("{resumeId:guid}/template")]
    [ProducesResponseType(typeof(ResumeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ResumeDto>> SwitchTemplate(
        Guid resumeId,
        [FromBody] SwitchTemplateRequest request,
        CancellationToken cancellationToken)
    {
        var resume = await Commands.Send(
            new SwitchResumeTemplateCommand(resumeId, request.Template),
            cancellationToken);

        return Ok(resume);
    }

    /// <summary>Renames a resume.</summary>
    [HttpPut("{resumeId:guid}/title")]
    [ProducesResponseType(typeof(ResumeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ResumeDto>> Rename(
        Guid resumeId,
        [FromBody] RenameResumeRequest request,
        CancellationToken cancellationToken)
    {
        var resume = await Commands.Send(
            new RenameResumeCommand(resumeId, request.Title),
            cancellationToken);

        return Ok(resume);
    }

    /// <summary>Soft-deletes a resume.</summary>
    [HttpDelete("{resumeId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid resumeId, CancellationToken cancellationToken)
    {
        await Commands.Send(new DeleteResumeCommand(resumeId), cancellationToken);
        return NoContent();
    }
}

public sealed record ExportResumeRequest(
    ResumeFormat Format,
    ResumeTemplateKey? Template,
    ResumePageSize PageSize = ResumePageSize.A4,
    ResumeMarginSize Margin = ResumeMarginSize.Normal,
    bool IncludePageNumbers = true,
    bool IncludeHeaderFooter = false);

public sealed record SwitchTemplateRequest(ResumeTemplateKey Template);

public sealed record RenameResumeRequest(string Title);
