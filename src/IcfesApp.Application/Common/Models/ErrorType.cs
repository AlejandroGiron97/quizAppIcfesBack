namespace IcfesApp.Application.Common.Models;

/// <summary>
/// Categoría de un fallo. El controlador la traduce a un código HTTP
/// (ver ApiControllerBase), así los servicios no saben nada de HTTP.
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
