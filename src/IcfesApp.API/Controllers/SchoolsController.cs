using IcfesApp.Application.Common.Interfaces;
using IcfesApp.Application.Common.Models;
using IcfesApp.Application.Schools.Dtos;
using IcfesApp.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IcfesApp.API.Controllers;

[ApiController]
[Route("api/institutions")]
[Authorize]
public class InstitutionsController(ISchoolService schoolService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        => Ok(await schoolService.GetAllAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var institution = await schoolService.GetByIdAsync(id, cancellationToken);
        return institution is null ? NotFound() : Ok(institution);
    }

    [HttpPost]
    [Authorize(Policy = Policies.RequireAdmin)]
    public async Task<IActionResult> Create(CreateSchoolRequest request, CancellationToken cancellationToken)
    {
        var created = await schoolService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Policies.RequireAdmin)]
    public async Task<IActionResult> Update(Guid id, UpdateSchoolRequest request, CancellationToken cancellationToken)
    {
        var result = await schoolService.UpdateAsync(id, request, cancellationToken);
        return ToActionResult(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Policies.RequireAdmin)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await schoolService.DeleteAsync(id, cancellationToken);
        return ToActionResult(result);
    }

    private IActionResult ToActionResult(Result result)
    {
        if (result.IsNotFound)
        {
            return NotFound();
        }

        return result.Succeeded ? NoContent() : BadRequest(new { errors = result.Errors });
    }
}