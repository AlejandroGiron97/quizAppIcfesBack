namespace IcfesApp.Application.Common.Models;

public class Result
{
    public bool Succeeded { get; protected init; }
    public bool IsNotFound { get; protected init; }
    public IReadOnlyList<string> Errors { get; protected init; } = [];

    public static Result Success() => new() { Succeeded = true };
    public static Result NotFound() => new() { Succeeded = false, IsNotFound = true };

    public static Result Failed(IEnumerable<string> errors) => new()
    {
        Succeeded = false,
        Errors = errors.ToList()
    };
}

public class Result<T> : Result
{
    public T? Value { get; private init; }

    public static Result<T> Success(T value) => new() { Succeeded = true, Value = value };

    public new static Result<T> Failed(IEnumerable<string> errors) => new()
    {
        Succeeded = false,
        Errors = errors.ToList()
    };
}
