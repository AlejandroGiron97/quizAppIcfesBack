using IcfesApp.Application.Common.Interfaces;
using IcfesApp.Application.Institutions.Dtos;
using IcfesApp.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IcfesApp.API.Controllers;

[Route("api/institutions")]
[Authorize]
public class InstitutionsController(IInstitutionService institutionService) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        => Ok(await institutionService.GetAllAsync(cancellationToken));

    [HttpPost]
    [Authorize(Policy = Policies.RequireAdmin)]
    public async Task<IActionResult> Create(CreateInstitutionRequest request, CancellationToken cancellationToken)
        => CreatedWithBody(await institutionService.CreateAsync(request, cancellationToken));

    /// <summary>Returns the updated institution so the client can update its list without reloading it.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = Policies.RequireAdmin)]
    public async Task<IActionResult> Update(Guid id, UpdateInstitutionRequest request, CancellationToken cancellationToken)
        => ToActionResult(await institutionService.UpdateAsync(id, request, cancellationToken));

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Policies.RequireAdmin)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        => ToActionResult(await institutionService.DeleteAsync(id, cancellationToken));
}
