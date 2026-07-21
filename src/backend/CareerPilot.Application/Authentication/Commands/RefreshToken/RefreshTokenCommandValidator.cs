using FluentValidation;

namespace CareerPilot.Application.Authentication.Commands.RefreshToken;

internal sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        // Presence only. The token's actual validity is a server-side lookup, and any
        // structural hint returned here would help an attacker shape forgeries.
        RuleFor(command => command.RefreshToken)
            .NotEmpty().WithMessage("A refresh token is required.");
    }
}
