using IcfesApp.Application.Common.Interfaces;
using IcfesApp.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IcfesApp.API.Controllers;

[Route("api/plans")]
public class PlansController(IPlanService planService) : ApiControllerBase
{
    /// <summary>
    /// Returns the active plans. Public endpoint for the pricing page.
    /// Filters by target: "Particular" or "Institutional" (optional query).
    /// includeInactive only takes effect for an Admin; it is ignored for everyone else.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? target,
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var canSeeInactivePlans = includeInactive && User.IsInRole(Roles.Admin);
        var plans = await planService.GetPlansAsync(target, canSeeInactivePlans, cancellationToken);
        return Ok(plans);
    }
}
