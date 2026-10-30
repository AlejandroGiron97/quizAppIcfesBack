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
    /// Returns the subscription that applies to the authenticated user (null if they have none):
    /// - Particular: their own.
    /// - Institutional: their institution's.
    /// </summary>
    [HttpGet("current-user")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetForCurrentUser(CancellationToken cancellationToken)
        => Ok(await subscriptionService.GetForUserAsync(CurrentUserId, cancellationToken));

    /// <summary>Lists every subscription, optionally only the ones of one institution. Super Admin only.</summary>
    [HttpGet]
    [Authorize(Policy = Policies.RequireAdmin)]
    public async Task<IActionResult> GetAll([FromQuery] Guid? institutionId, CancellationToken cancellationToken)
        => Ok(await subscriptionService.GetAllAsync(institutionId, cancellationToken));
}
