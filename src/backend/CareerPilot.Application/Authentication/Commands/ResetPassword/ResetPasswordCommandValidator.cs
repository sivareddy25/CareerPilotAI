using CareerPilot.Application.Authentication.Validation;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace CareerPilot.Application.Authentication.Commands.ResetPassword;

internal sealed class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator(IOptions<AuthenticationOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);

        RuleFor(command => command.Email).Email();

        RuleFor(command => command.Token)
            .NotEmpty().WithMessage("A reset token is required.");

        RuleFor(command => command.NewPassword).Password(options.Value);
    }
}
