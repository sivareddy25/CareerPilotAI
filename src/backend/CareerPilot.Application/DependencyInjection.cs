using System.Reflection;
using CareerPilot.Application.Abstractions.Messaging;
using CareerPilot.Application.Authentication;
using CareerPilot.Application.Behaviors;
using CareerPilot.Application.Messaging;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace CareerPilot.Application;

/// <summary>
/// Composition entry point for the Application layer. The Api project calls this;
/// it must never need to know which concrete types live inside.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        var assembly = Assembly.GetExecutingAssembly();

        services.AddScoped<ICommandDispatcher, CommandDispatcher>();
        services.AddScoped<IQueryDispatcher, QueryDispatcher>();

        services.AddHandlers(assembly);

        // Discovers AbstractValidator<T> implementations.
        services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);

        // Open generic, so it closes over every command and query the dispatchers
        // resolve. Registered first, and therefore outermost in the pipeline: a
        // request must not reach a later behavior — or a handler — unvalidated.
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        services.AddScoped<AuthenticationSessionFactory>();

        return services;
    }

    /// <summary>
    /// Registers every closed <see cref="ICommandHandler{TCommand,TResponse}"/> and
    /// <see cref="IQueryHandler{TQuery,TResponse}"/> in the assembly, so adding a
    /// handler never requires editing this file.
    /// </summary>
    private static void AddHandlers(this IServiceCollection services, Assembly assembly)
    {
        Type[] handlerInterfaces = [typeof(ICommandHandler<,>), typeof(IQueryHandler<,>)];

        var implementations = assembly.GetTypes()
            .Where(type => type is { IsAbstract: false, IsInterface: false, IsGenericTypeDefinition: false });

        foreach (var implementation in implementations)
        {
            var closedInterfaces = implementation.GetInterfaces()
                .Where(i => i.IsGenericType && handlerInterfaces.Contains(i.GetGenericTypeDefinition()));

            foreach (var closedInterface in closedInterfaces)
            {
                services.AddScoped(closedInterface, implementation);
            }
        }
    }
}
