using IcfesApp.Application.Common.Interfaces;
using IcfesApp.Application.Common.Models;
using IcfesApp.Application.Subjects.Dtos;
using IcfesApp.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IcfesApp.API.Controllers;

[ApiController]
[Route("api/subjects")]
[Authorize]
public class SubjectsController(ISubjectService subjectService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        => Ok(await subjectService.GetAllAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var subject = await subjectService.GetByIdAsync(id, cancellationToken);
        return subject is null ? NotFound() : Ok(subject);
    }

    [HttpPost]
    [Authorize(Policy = Policies.RequireAdmin)]
    public async Task<IActionResult> Create(CreateSubjectRequest request, CancellationToken cancellationToken)
    {
        var created = await subjectService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Policies.RequireAdmin)]
    public async Task<IActionResult> Update(Guid id, UpdateSubjectRequest request, CancellationToken cancellationToken)
    {
        var result = await subjectService.UpdateAsync(id, request, cancellationToken);
        return ToActionResult(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Policies.RequireAdmin)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await subjectService.DeleteAsync(id, cancellationToken);
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
