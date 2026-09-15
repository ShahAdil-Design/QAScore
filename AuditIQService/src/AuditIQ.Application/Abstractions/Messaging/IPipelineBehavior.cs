namespace AuditIQ.Application.Abstractions.Messaging;

public delegate Task<TResponse> RequestHandlerDelegate<TResponse>();

/// <summary>Cross-cutting middleware wrapped around every request dispatch (validation, logging, ...).</summary>
public interface IPipelineBehavior<in TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken);
}
