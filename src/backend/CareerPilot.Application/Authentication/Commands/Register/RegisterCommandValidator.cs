using CareerPilot.Application.Authentication.Validation;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace CareerPilot.Application.Authentication.Commands.Register;

/// <summary>
/// Enforces email shape and the full password policy at registration.
/// </summary>
/// <remarks>
/// Duplicate-email detection is *not* here. A validator that queries the database
/// would still race another registration between check and insert; uniqueness is
/// owned by the database index, and the handler translates the violation. Keeping the
/// check out of the validator avoids implying a guarantee it cannot make.
/// </remarks>
internal sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator(IOptions<AuthenticationOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);

        RuleFor(command => command.Email).Email();

        RuleFor(command => command.Password).Password(options.Value);

        RuleFor(command => command.FirstName)
            .MaximumLength(100).WithMessage("First name must be at most 100 characters.");

        RuleFor(command => command.LastName)
            .MaximumLength(100).WithMessage("Last name must be at most 100 characters.");
    }
}
