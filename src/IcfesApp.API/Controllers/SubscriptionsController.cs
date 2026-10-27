using System.Security.Claims;
using IcfesApp.Application.Common.Interfaces;
using IcfesApp.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IcfesApp.API.Controllers;

[ApiController]
[Route("api/subscriptions")]
[Authorize]
public class SubscriptionsController(ISubscriptionService subscriptionService) : ControllerBase
{
    /// <summary>
    /// Devuelve la suscripción relevante para el usuario autenticado:
    /// - Particular: la suya.
    /// - Institutional: la de su institución.
    /// </summary>
    [HttpGet("current")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCurrent(CancellationToken cancellationToken)
    {
        var subscription = await subscriptionService.GetMineAsync(CurrentUserId, cancellationToken);
        return Ok(subscription);
    }

    /// <summary>Lista todas las suscripciones. Solo Super Admin.</summary>
    [HttpGet]
    [Authorize(Policy = Policies.RequireAdmin)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        => Ok(await subscriptionService.GetAllAsync(cancellationToken));

    /// <summary>Devuelve la suscripción de una institución específica. Solo Super Admin.</summary>
    [HttpGet("institution/{institutionId:guid}")]
    [Authorize(Policy = Policies.RequireAdmin)]
    public async Task<IActionResult> GetForInstitution(Guid institutionId, CancellationToken cancellationToken)
    {
        var subscription = await subscriptionService.GetForInstitutionAsync(institutionId, cancellationToken);
        return subscription is null ? NotFound() : Ok(subscription);
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}