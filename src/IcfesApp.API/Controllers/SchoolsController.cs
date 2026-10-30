using IcfesApp.Application.Common.Interfaces;
using IcfesApp.Application.Schools.Dtos;
using IcfesApp.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IcfesApp.API.Controllers;

[Route("api/institutions")]
[Authorize]
public class InstitutionsController(ISchoolService schoolService) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        => Ok(await schoolService.GetAllAsync(cancellationToken));

    [HttpPost]
    [Authorize(Policy = Policies.RequireAdmin)]
    public async Task<IActionResult> Create(CreateSchoolRequest request, CancellationToken cancellationToken)
        => CreatedWithBody(await schoolService.CreateAsync(request, cancellationToken));

    /// <summary>Returns the updated institution so the client can update its list without reloading it.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = Policies.RequireAdmin)]
    public async Task<IActionResult> Update(Guid id, UpdateSchoolRequest request, CancellationToken cancellationToken)
        => ToActionResult(await schoolService.UpdateAsync(id, request, cancellationToken));

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Policies.RequireAdmin)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        => ToActionResult(await schoolService.DeleteAsync(id, cancellationToken));
}
