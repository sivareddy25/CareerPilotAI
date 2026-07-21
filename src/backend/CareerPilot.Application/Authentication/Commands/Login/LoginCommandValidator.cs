using CareerPilot.Application.Authentication.Validation;
using FluentValidation;

namespace CareerPilot.Application.Authentication.Commands.Login;

/// <summary>
/// Validates only that the fields are present and well-formed.
/// </summary>
/// <remarks>
/// Password strength is deliberately *not* enforced here. Login must accept any
/// stored password, including ones predating a policy tightening — rejecting them at
/// the validator would lock those users out of the very screen they would use to
/// change it. Composition rules belong to registration and change-password.
/// </remarks>
internal sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(command => command.Email).Email();

        RuleFor(command => command.Password)
            .NotEmpty().WithMessage("A password is required.");
    }
}
