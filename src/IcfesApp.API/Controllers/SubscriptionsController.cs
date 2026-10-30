using IcfesApp.Application.Common.Interfaces;
using IcfesApp.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IcfesApp.API.Controllers;

[Route("api/subscriptions")]
[Authorize]
public class SubscriptionsController(ISubscriptionService subscriptionService) : ApiControllerBase
{
    /// <summary>
    /// Returns the subscription that applies to the authenticated user:
    /// - Particular: their own.
    /// - Institutional: their institution's.
    /// </summary>
    [HttpGet("current")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCurrent(CancellationToken cancellationToken)
        => Ok(await subscriptionService.GetMineAsync(CurrentUserId, cancellationToken));

    /// <summary>Lists every subscription. Super Admin only.</summary>
    [HttpGet]
    [Authorize(Policy = Policies.RequireAdmin)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        => Ok(await subscriptionService.GetAllAsync(cancellationToken));

    /// <summary>Returns the subscription of a specific institution. Super Admin only.</summary>
    [HttpGet("institution/{institutionId:guid}")]
    [Authorize(Policy = Policies.RequireAdmin)]
    public async Task<IActionResult> GetForInstitution(Guid institutionId, CancellationToken cancellationToken)
    {
        var institutionSubscription = await subscriptionService.GetForInstitutionAsync(institutionId, cancellationToken);
        return institutionSubscription is null ? NotFound() : Ok(institutionSubscription);
    }
}
