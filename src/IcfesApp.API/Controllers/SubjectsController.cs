using IcfesApp.Application.Common.Interfaces;
using IcfesApp.Application.Subjects.Dtos;
using IcfesApp.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IcfesApp.API.Controllers;

[Route("api/subjects")]
[Authorize]
public class SubjectsController(ISubjectService subjectService) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        => Ok(await subjectService.GetAllAsync(cancellationToken));

    [HttpPost]
    [Authorize(Policy = Policies.RequireAdmin)]
    public async Task<IActionResult> Create(CreateSubjectRequest request, CancellationToken cancellationToken)
        => CreatedWithBody(await subjectService.CreateAsync(request, cancellationToken));

    /// <summary>Returns the updated subject so the client can update its list without reloading it.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = Policies.RequireAdmin)]
    public async Task<IActionResult> Update(Guid id, UpdateSubjectRequest request, CancellationToken cancellationToken)
        => ToActionResult(await subjectService.UpdateAsync(id, request, cancellationToken));

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Policies.RequireAdmin)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        => ToActionResult(await subjectService.DeleteAsync(id, cancellationToken));
}
