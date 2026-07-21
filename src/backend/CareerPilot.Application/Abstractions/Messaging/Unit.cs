namespace CareerPilot.Application.Abstractions.Messaging;

/// <summary>
/// Represents the absence of a meaningful return value.
/// Use <c>ICommand&lt;Unit&gt;</c> for commands that produce no result,
/// so the dispatcher keeps a single generic code path.
/// </summary>
public readonly record struct Unit
{
    public static readonly Unit Value = default;
}
