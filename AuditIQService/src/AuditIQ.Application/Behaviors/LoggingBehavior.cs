using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Abstractions.Results;
using Microsoft.Extensions.Logging;

namespace AuditIQ.Application.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse>(ILogger<TRequest> logger) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        var response = await next();

        if (response is Result { IsFailure: true } result)
            logger.LogWarning("{RequestName} failed: {ErrorCode} — {ErrorMessage}", requestName, result.FirstError.Code, result.FirstError.Message);
        else
            logger.LogInformation("{RequestName} handled successfully", requestName);

        return response;
    }
}
