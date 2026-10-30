using IcfesApp.Application.Auth.Dtos;
using IcfesApp.Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IcfesApp.API.Controllers;

/// <summary>Email confirmation for newly registered users.</summary>
[Route("api/auth")]
public class EmailConfirmationController(IEmailConfirmationService emailConfirmationService) : ApiControllerBase
{
    [HttpPost("confirm-email")]
    public async Task<IActionResult> ConfirmEmail(ConfirmEmailRequest request, CancellationToken cancellationToken)
    {
        var confirmEmailResult = await emailConfirmationService.ConfirmEmailAsync(request, cancellationToken);
        return confirmEmailResult.Succeeded
            ? Ok(new { message = "Correo confirmado correctamente. Ya puedes iniciar sesión." })
            : ToErrorResponse(confirmEmailResult);
    }

    [HttpPost("resend-confirmation-email")]
    public async Task<IActionResult> ResendConfirmationEmail(ResendConfirmationRequest request, CancellationToken cancellationToken)
    {
        await emailConfirmationService.ResendConfirmationEmailAsync(request, cancellationToken);
        return Ok(new { message = "Si el correo existe y no ha sido confirmado, se enviará un nuevo enlace." });
    }
}
