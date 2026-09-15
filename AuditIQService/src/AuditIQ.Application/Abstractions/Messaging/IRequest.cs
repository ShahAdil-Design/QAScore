namespace AuditIQ.Application.Abstractions.Messaging;

/// <summary>Marker for anything dispatchable through ISender — commands and queries both implement this.</summary>
public interface IRequest<TResponse>;
