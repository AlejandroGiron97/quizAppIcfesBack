using IcfesApp.Application.Common.Interfaces;
using IcfesApp.Application.Questions.Dtos;
using IcfesApp.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IcfesApp.API.Controllers;

/// <summary>
/// Question bank. The whole controller is restricted to Teacher/Admin because QuestionDto
/// includes which option is correct. Students get their questions through
/// /api/practice-sessions, which never reveals the answer ahead of time.
/// </summary>
[Route("api/questions")]
[Authorize(Policy = Policies.RequireTeacher)]
public class QuestionsController(IQuestionService questionService) : ApiControllerBase
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
    public async Task<IActionResult> Create(CreateQuestionRequest request, CancellationToken cancellationToken)
    {
        var createResult = await questionService.CreateAsync(request, CurrentUserId, cancellationToken);
        if (!createResult.Succeeded)
        {
            return ToErrorResponse(createResult);
        }

        return CreatedAtAction(nameof(GetById), new { id = createResult.Value!.Id }, createResult.Value);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateQuestionRequest request, CancellationToken cancellationToken)
        => ToActionResult(await questionService.UpdateAsync(id, request, cancellationToken));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        => ToActionResult(await questionService.DeleteAsync(id, cancellationToken));
}
