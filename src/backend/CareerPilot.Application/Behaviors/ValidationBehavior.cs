using CareerPilot.Application.Abstractions.Messaging;
using FluentValidation;

namespace CareerPilot.Application.Behaviors;

/// <summary>
/// Runs every registered validator for a request before its handler sees it.
/// </summary>
/// <remarks>
/// Registered as an open generic, so it wraps commands and queries alike and a new
/// validator takes effect purely by existing in the assembly.
///
/// All validators run and their failures are aggregated rather than short-circuiting
/// on the first: a form should report every problem at once, not one per round-trip.
/// The resulting <see cref="ValidationException"/> is translated to an RFC 7807
/// response by the Api layer's <c>ValidationExceptionHandler</c>.
/// </remarks>
internal sealed class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var applicable = validators as IValidator<TRequest>[] ?? validators.ToArray();

        if (applicable.Length == 0)
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var results = await Task.WhenAll(
            applicable.Select(validator => validator.ValidateAsync(context, cancellationToken)));

        var failures = results
            .SelectMany(result => result.Errors)
            .Where(failure => failure is not null)
            .ToArray();

        if (failures.Length > 0)
        {
            throw new ValidationException(failures);
        }

        return await next();
    }
}
