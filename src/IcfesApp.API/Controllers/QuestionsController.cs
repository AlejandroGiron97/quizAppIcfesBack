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

    [HttpPost]
    public async Task<IActionResult> Create(CreateQuestionRequest request, CancellationToken cancellationToken)
        => ToCreatedResult(await questionService.CreateAsync(request, CurrentUserId, cancellationToken));

    /// <summary>Returns the updated question so the client can update its list without reloading it.</summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateQuestionRequest request, CancellationToken cancellationToken)
        => ToActionResult(await questionService.UpdateAsync(id, request, cancellationToken));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        => ToActionResult(await questionService.DeleteAsync(id, cancellationToken));
}
