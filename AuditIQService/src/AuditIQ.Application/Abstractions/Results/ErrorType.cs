namespace AuditIQ.Application.Abstractions.Results;

public enum ErrorType
{
    Failure,
    Validation,
    NotFound,
    Conflict,
    Forbidden,
}
