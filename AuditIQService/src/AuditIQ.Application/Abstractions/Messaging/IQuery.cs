using AuditIQ.Application.Abstractions.Results;

namespace AuditIQ.Application.Abstractions.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>;
