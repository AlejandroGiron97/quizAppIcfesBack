using System.Security.Claims;
using IcfesApp.Application.Common.Models;
using Microsoft.AspNetCore.Mvc;

namespace IcfesApp.API.Controllers;

/// <summary>
/// Base class for every API controller.
///
/// Holds what used to be copied into each controller:
///   - <see cref="CurrentUserId"/>: id of the authenticated user, read from the JWT.
///   - <see cref="ToActionResult(Result)"/>: turns a service <see cref="Result"/> into an HTTP response.
///
/// Rule: services never return HTTP status codes; they return a Result with its <see cref="ErrorType"/>
/// and the status code (400, 401, 403, 404, 423) is chosen here.
/// </summary>
[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>Id of the authenticated user. Only use it in actions marked with [Authorize].</summary>
    protected Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>Success → 204 No Content. Failure → status code based on the error type.</summary>
    protected IActionResult ToActionResult(Result result) =>
        result.Succeeded ? NoContent() : ToErrorResponse(result);

    /// <summary>Success → 200 OK with the value. Failure → status code based on the error type.</summary>
    protected IActionResult ToActionResult<T>(Result<T> result) =>
        result.Succeeded ? Ok(result.Value) : ToErrorResponse(result);

    /// <summary>Error response using the common API format: { "errors": [...] }.</summary>
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
