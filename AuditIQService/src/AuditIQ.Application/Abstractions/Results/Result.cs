namespace AuditIQ.Application.Abstractions.Results;

/// <summary>
/// Functional success/failure wrapper for command and query handlers — expected
/// business failures (validation, not-found, conflict) are returned as data, not
/// thrown as exceptions. Mirrors AccountManagementService's Result-based handler
/// pattern, without depending on its internal Sequensis.Functional package.
/// </summary>
public class Result
{
    protected Result(bool isSuccess, IReadOnlyList<Error> errors)
    {
        if (isSuccess && errors.Count > 0)
            throw new InvalidOperationException("A successful result cannot carry errors.");
        if (!isSuccess && errors.Count == 0)
            throw new InvalidOperationException("A failed result must carry at least one error.");

        IsSuccess = isSuccess;
        Errors = errors;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public IReadOnlyList<Error> Errors { get; }
    public Error FirstError => Errors.Count > 0 ? Errors[0] : Error.None;

    public static Result Success() => new(true, []);
    public static Result Failure(params Error[] errors) => new(false, errors);

    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, []);
    public static Result<TValue> Failure<TValue>(params Error[] errors) => new(default, false, errors);
}

public sealed class Result<TValue> : Result
{
    private readonly TValue? _value;

    internal Result(TValue? value, bool isSuccess, IReadOnlyList<Error> errors) : base(isSuccess, errors)
    {
        _value = value;
    }

    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access the value of a failed result.");

    public static implicit operator Result<TValue>(TValue value) => Success(value);
}
