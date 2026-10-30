using IcfesApp.Application.Common.Interfaces;
using IcfesApp.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IcfesApp.API.Controllers;

[Route("api/plans")]
public class PlansController(IPlanService planService) : ApiControllerBase
{
    /// <summary>
    /// Devuelve los planes activos. Endpoint público para la página de precios.
    /// Filtra por target: "Particular" o "Institutional" (query opcional).
    /// includeInactive solo tiene efecto para un Admin; para los demás se ignora.
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

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var plan = await planService.GetByIdAsync(id, cancellationToken);
        return plan is null ? NotFound() : Ok(plan);
    }
}
