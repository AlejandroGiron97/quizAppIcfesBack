using System.Security.Claims;
using IcfesApp.Application.Common.Interfaces;
using IcfesApp.Application.Common.Models;
using IcfesApp.Application.Questions.Dtos;
using IcfesApp.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IcfesApp.API.Controllers;

[ApiController]
[Route("api/questions")]
[Authorize]
public class QuestionsController(IQuestionService questionService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? subjectId, CancellationToken cancellationToken)
        => Ok(await questionService.GetAllAsync(subjectId, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var question = await questionService.GetByIdAsync(id, cancellationToken);
        return question is null ? NotFound() : Ok(question);
    }

    [HttpPost]
    [Authorize(Policy = Policies.RequireTeacher)]
    public async Task<IActionResult> Create(CreateQuestionRequest request, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await questionService.CreateAsync(request, userId, cancellationToken);

        if (!result.Succeeded)
        {
            return BadRequest(new { errors = result.Errors });
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Policies.RequireTeacher)]
    public async Task<IActionResult> Update(Guid id, UpdateQuestionRequest request, CancellationToken cancellationToken)
    {
        var result = await questionService.UpdateAsync(id, request, cancellationToken);
        return ToActionResult(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Policies.RequireTeacher)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await questionService.DeleteAsync(id, cancellationToken);
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
