using IcfesApp.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IcfesApp.API.Controllers;

[ApiController]
[Route("api/plans")]
public class PlansController(IPlanService planService) : ControllerBase
{
    /// <summary>
    /// Devuelve los planes activos. Endpoint público para la página de precios.
    /// Filtra por target: "Particular" o "Institutional" (query opcional).
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? target,
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var plans = await planService.GetPlansAsync(target, includeInactive, cancellationToken);
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