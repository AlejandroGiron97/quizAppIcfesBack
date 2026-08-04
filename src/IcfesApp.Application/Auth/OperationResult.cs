namespace IcfesApp.Application.Auth;

public class OperationResult
{
    public bool Succeeded { get; private init; }
    public IReadOnlyList<string> Errors { get; private init; } = [];

    public static OperationResult Success() => new() { Succeeded = true };

    public static OperationResult Failed(IEnumerable<string> errors) => new()
    {
        Succeeded = false,
        Errors = errors.ToList()
    };
}
