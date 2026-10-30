using System.Security.Claims;
using IcfesApp.Application.Common.Models;
using Microsoft.AspNetCore.Mvc;

namespace IcfesApp.API.Controllers;

/// <summary>
/// Clase base de todos los controladores de la API.
///
/// Concentra lo que antes estaba copiado en cada controlador:
///   - <see cref="CurrentUserId"/>: Id del usuario autenticado, leído del JWT.
///   - <see cref="ToActionResult(Result)"/>: traduce un <see cref="Result"/> del servicio a una respuesta HTTP.
///
/// Regla: los servicios nunca devuelven códigos HTTP; devuelven un Result con su <see cref="ErrorType"/>
/// y aquí se decide el código (400, 401, 403, 404, 423).
/// </summary>
[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>Id del usuario autenticado. Solo usar en acciones con [Authorize].</summary>
    protected Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>Éxito → 204 No Content. Fallo → código según el tipo de error.</summary>
    protected IActionResult ToActionResult(Result result) =>
        result.Succeeded ? NoContent() : ToErrorResponse(result);

    /// <summary>Éxito → 200 OK con el valor. Fallo → código según el tipo de error.</summary>
    protected IActionResult ToActionResult<T>(Result<T> result) =>
        result.Succeeded ? Ok(result.Value) : ToErrorResponse(result);

    /// <summary>Respuesta de error con el formato común de la API: { "errors": [...] }.</summary>
    protected IActionResult ToErrorResponse(Result failedResult)
    {
        var errorBody = new { errors = failedResult.Errors };

        return failedResult.ErrorType switch
        {
            ErrorType.NotFound => failedResult.Errors.Count == 0 ? NotFound() : NotFound(errorBody),
            ErrorType.Unauthorized => Unauthorized(errorBody),
            ErrorType.Forbidden => StatusCode(StatusCodes.Status403Forbidden, errorBody),
            ErrorType.Locked => StatusCode(StatusCodes.Status423Locked, errorBody),
            _ => BadRequest(errorBody)
        };
    }
}
