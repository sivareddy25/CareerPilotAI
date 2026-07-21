using CareerPilot.Application.Abstractions.Messaging;
using CareerPilot.Application.Abstractions.System;
using CareerPilot.Application.System.Models;

namespace CareerPilot.Application.System.Commands;

public sealed record ExportSystemBackupCommand : ICommand<SystemBackupDto>;

internal sealed class ExportSystemBackupCommandHandler(IBackupRestoreService backupService)
    : ICommandHandler<ExportSystemBackupCommand, SystemBackupDto>
{
    public async Task<SystemBackupDto> Handle(ExportSystemBackupCommand command, CancellationToken cancellationToken)
    {
        return await backupService.ExportBackupAsync(cancellationToken);
    }
}

public sealed record RestoreSystemBackupCommand(string BackupJsonData) : ICommand<bool>;

internal sealed class RestoreSystemBackupCommandHandler(IBackupRestoreService backupService)
    : ICommandHandler<RestoreSystemBackupCommand, bool>
{
    public async Task<bool> Handle(RestoreSystemBackupCommand command, CancellationToken cancellationToken)
    {
        return await backupService.RestoreBackupAsync(command.BackupJsonData, cancellationToken);
    }
}
