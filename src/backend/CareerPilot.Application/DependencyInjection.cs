using System.Reflection;
using CareerPilot.Application.Abstractions.Messaging;
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

        // Discovers AbstractValidator<T> implementations. None exist yet — validators
        // arrive with the first feature slice.
        services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);

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
