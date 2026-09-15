using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace AuditIQ.Application.Abstractions.Messaging;

internal sealed class Sender(IServiceProvider serviceProvider) : ISender
{
    private static readonly ConcurrentDictionary<Type, HandlerInvoker> Invokers = new();

    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        var invoker = Invokers.GetOrAdd(request.GetType(), CreateInvoker<TResponse>);
        return (Task<TResponse>)invoker(serviceProvider, request, cancellationToken);
    }

    private static HandlerInvoker CreateInvoker<TResponse>(Type requestType)
    {
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));
        var behaviorType = typeof(IPipelineBehavior<,>).MakeGenericType(requestType, typeof(TResponse));
        var handlerDelegateType = typeof(RequestHandlerDelegate<TResponse>);
        var handleMethod = handlerType.GetMethod(nameof(IRequestHandler<IRequest<TResponse>, TResponse>.Handle))!;
        var behaviorHandleMethod = behaviorType.GetMethod(nameof(IPipelineBehavior<IRequest<TResponse>, TResponse>.Handle))!;

        return (services, request, cancellationToken) =>
        {
            var handler = services.GetRequiredService(handlerType);
            RequestHandlerDelegate<TResponse> pipeline = () =>
                (Task<TResponse>)handleMethod.Invoke(handler, [request, cancellationToken])!;

            var behaviors = services.GetServices(behaviorType).Reverse();
            foreach (var behavior in behaviors)
            {
                var next = pipeline;
                pipeline = () => (Task<TResponse>)behaviorHandleMethod.Invoke(behavior, [request, next, cancellationToken])!;
            }

            return pipeline();
        };
    }

    private delegate Task HandlerInvoker(IServiceProvider services, object request, CancellationToken cancellationToken);
}
