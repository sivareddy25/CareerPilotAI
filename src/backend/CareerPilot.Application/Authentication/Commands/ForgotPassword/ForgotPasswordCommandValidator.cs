using CareerPilot.Application.Authentication.Validation;
using FluentValidation;

namespace CareerPilot.Application.Authentication.Commands.ForgotPassword;

internal sealed class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(command => command.Email).Email();
    }
}
