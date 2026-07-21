using System.Net.Mime;
using Asp.Versioning;
using CareerPilot.Application.Abstractions.Messaging;
using Microsoft.AspNetCore.Mvc;

namespace CareerPilot.Api.Controllers;

/// <summary>
/// Base type for every API controller. Centralises routing, versioning, content
/// negotiation, and access to the CQRS dispatchers so derived controllers carry
/// no infrastructural boilerplate.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public abstract class BaseApiController : ControllerBase
{
    private ICommandDispatcher? _commands;
    private IQueryDispatcher? _queries;

    /// <summary>
    /// Resolved lazily from the request scope rather than injected, so derived
    /// controllers need no constructor and no base-constructor chaining. Constructor
    /// injection remains available for a controller's own dependencies.
    /// </summary>
    protected ICommandDispatcher Commands =>
        _commands ??= HttpContext.RequestServices.GetRequiredService<ICommandDispatcher>();

    protected IQueryDispatcher Queries =>
        _queries ??= HttpContext.RequestServices.GetRequiredService<IQueryDispatcher>();
}
