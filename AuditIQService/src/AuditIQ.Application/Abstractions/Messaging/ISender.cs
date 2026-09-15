namespace AuditIQ.Application.Abstractions.Messaging;

/// <summary>Dispatches a command or query to its handler, running the registered pipeline behaviors first.</summary>
public interface ISender
{
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
}
