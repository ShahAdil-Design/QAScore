using System.Reflection;
using AuditIQ.Application.Abstractions.Messaging;
using AuditIQ.Application.Abstractions.Results;
using FluentValidation;

namespace AuditIQ.Application.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);
        var errors = validators
            .Select(v => v.Validate(context))
            .SelectMany(r => r.Errors)
            .Select(f => Error.Validation(f.PropertyName, f.ErrorMessage))
            .Distinct()
            .ToArray();

        return errors.Length == 0 ? await next() : ToFailureResponse(errors);
    }

    private static TResponse ToFailureResponse(Error[] errors)
    {
        if (typeof(TResponse) == typeof(Result))
            return (TResponse)(object)Result.Failure(errors);

        var valueType = typeof(TResponse).GetGenericArguments()[0];
        var genericFailure = typeof(Result)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(Result.Failure) && m.IsGenericMethodDefinition)
            .MakeGenericMethod(valueType);

        return (TResponse)genericFailure.Invoke(null, [errors])!;
    }
}
