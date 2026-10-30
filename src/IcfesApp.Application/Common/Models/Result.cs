namespace IcfesApp.Application.Common.Models;

/// <summary>
/// Result of a business operation with no return value.
/// It is the base class of <see cref="Result{T}"/> and of AuthResult.
/// Services return a Result instead of throwing exceptions for expected failures
/// (validation errors, missing resources, invalid credentials...).
/// </summary>
public class Result
{
    public bool Succeeded { get; protected init; }

    /// <summary>Category of the failure. <see cref="ErrorType.None"/> when the operation succeeded.</summary>
    public ErrorType ErrorType { get; protected init; }

    public IReadOnlyList<string> Errors { get; protected init; } = [];

    public bool IsNotFound => ErrorType == ErrorType.NotFound;

    public static Result Success() => new() { Succeeded = true };

    public static Result NotFound(string? errorMessage = null) => new()
    {
        ErrorType = ErrorType.NotFound,
        Errors = errorMessage is null ? [] : [errorMessage]
    };

    public static Result Failed(IEnumerable<string> errors, ErrorType errorType = ErrorType.Validation) => new()
    {
        ErrorType = errorType,
        Errors = errors.ToList()
    };
}

/// <summary>Result of a business operation that returns a value when it succeeds.</summary>
public class Result<T> : Result
{
    public T? Value { get; private init; }

    public static Result<T> Success(T value) => new() { Succeeded = true, Value = value };

    public new static Result<T> NotFound(string? errorMessage = null) => new()
    {
        ErrorType = ErrorType.NotFound,
        Errors = errorMessage is null ? [] : [errorMessage]
    };

    public new static Result<T> Failed(IEnumerable<string> errors, ErrorType errorType = ErrorType.Validation) => new()
    {
        ErrorType = errorType,
        Errors = errors.ToList()
    };
}
