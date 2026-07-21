using CareerPilot.Application.System.Commands;
using CareerPilot.Application.System.Models;
using CareerPilot.Application.System.Queries;
using Microsoft.AspNetCore.Mvc;

namespace CareerPilot.Api.Controllers;

[Route("api/v{version:apiVersion}/system")]
public sealed class SystemManagementController : BaseApiController
{
    /// <summary>Runs diagnostic health checks on PostgreSQL, Redis, Ollama, Playwright, and OAuth APIs.</summary>
    [HttpGet("health")]
    [ProducesResponseType(typeof(SystemHealthStatusDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<SystemHealthStatusDto>> GetSystemHealth(CancellationToken cancellationToken)
    {
        var health = await Queries.Query(new GetSystemHealthQuery(), cancellationToken);
        return Ok(health);
    }

    /// <summary>Lists installed local Ollama LLM models.</summary>
    [HttpGet("ollama/models")]
    [ProducesResponseType(typeof(IReadOnlyList<OllamaModelDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<OllamaModelDto>>> GetOllamaModels(CancellationToken cancellationToken)
    {
        var models = await Queries.Query(new GetOllamaModelsQuery(), cancellationToken);
        return Ok(models);
    }

    /// <summary>Generates a 1-click database JSON backup snapshot for local desktop storage.</summary>
    [HttpPost("backup/export")]
    [ProducesResponseType(typeof(SystemBackupDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<SystemBackupDto>> ExportBackup(CancellationToken cancellationToken)
    {
        var backup = await Commands.Send(new ExportSystemBackupCommand(), cancellationToken);
        return Ok(backup);
    }

    /// <summary>Restores database state from a backup snapshot payload.</summary>
    [HttpPost("backup/restore")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RestoreBackup([FromBody] RestoreBackupRequest request, CancellationToken cancellationToken)
    {
        var success = await Commands.Send(new RestoreSystemBackupCommand(request.BackupJsonData), cancellationToken);
        return Ok(new { success, message = "System database restored successfully." });
    }

    /// <summary>Checks for software updates via GitHub Releases API.</summary>
    [HttpGet("updates/check")]
    [ProducesResponseType(typeof(UpdateStatusDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<UpdateStatusDto>> CheckUpdates(CancellationToken cancellationToken)
    {
        var update = await Queries.Query(new CheckUpdatesQuery(), cancellationToken);
        return Ok(update);
    }

    /// <summary>Fetches recent structured diagnostic logs for troubleshooting.</summary>
    [HttpGet("logs")]
    [ProducesResponseType(typeof(IReadOnlyList<LogEntryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<LogEntryDto>>> GetDiagnosticLogs([FromQuery] int count = 50, [FromQuery] string? level = null, CancellationToken cancellationToken = default)
    {
        var logs = await Queries.Query(new GetDiagnosticLogsQuery(count, level), cancellationToken);
        return Ok(logs);
    }
}

public sealed record RestoreBackupRequest(string BackupJsonData);
