namespace IcfesApp.Application.Common.Models;

/// <summary>
/// Category of a failure. The controller maps it to an HTTP status code
/// (see ApiControllerBase), so services know nothing about HTTP.
/// </summary>
public enum ErrorType
{
    None = 0,
    Validation,
    NotFound,
    Unauthorized,
    Forbidden,
    Locked
}
