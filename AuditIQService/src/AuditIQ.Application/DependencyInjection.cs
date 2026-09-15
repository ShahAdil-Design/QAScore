using System.Reflection;
using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Behaviors;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace AuditIQ.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddScoped<ISender, Sender>();
        RegisterHandlers(services, assembly);
        services.AddValidatorsFromAssembly(assembly);

        // Order matters: logging wraps validation wraps the handler.
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }

    private static void RegisterHandlers(IServiceCollection services, Assembly assembly)
    {
        var handlerRegistrations = assembly.GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false })
            .SelectMany(t => t.GetInterfaces(), (implementation, @interface) => (implementation, @interface))
            .Where(x => x.@interface.IsGenericType && x.@interface.GetGenericTypeDefinition() == typeof(IRequestHandler<,>));

        foreach (var (implementation, @interface) in handlerRegistrations)
            services.AddScoped(@interface, implementation);
    }
}
