using CareerPilot.Application.Authentication.Validation;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace CareerPilot.Application.Authentication.Commands.ChangePassword;

internal sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator(IOptions<AuthenticationOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);

        RuleFor(command => command.CurrentPassword)
            .NotEmpty().WithMessage("Your current password is required.");

        RuleFor(command => command.NewPassword)
            .Password(options.Value)
            .NotEqual(command => command.CurrentPassword)
                .WithMessage("The new password must differ from the current one.");
    }
}
