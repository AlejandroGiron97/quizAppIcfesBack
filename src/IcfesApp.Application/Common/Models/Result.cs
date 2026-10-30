namespace IcfesApp.Application.Common.Models;

/// <summary>
/// Resultado de una operación de negocio sin valor de retorno.
/// Es la clase base de <see cref="Result{T}"/> y de AuthResult.
/// Los servicios devuelven Result en vez de lanzar excepciones para los fallos esperados
/// (validaciones, recurso inexistente, credenciales inválidas...).
/// </summary>
public class Result
{
    public bool Succeeded { get; protected init; }

    /// <summary>Categoría del fallo. <see cref="ErrorType.None"/> cuando la operación tuvo éxito.</summary>
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

/// <summary>Resultado de una operación de negocio que devuelve un valor cuando tiene éxito.</summary>
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
