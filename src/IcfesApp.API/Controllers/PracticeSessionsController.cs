using IcfesApp.Application.Common.Interfaces;
using IcfesApp.Application.Practice.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IcfesApp.API.Controllers;

[Route("api/practice-sessions")]
[Authorize]
public class PracticeSessionsController(IPracticeSessionService practiceSessionService) : ApiControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Start(StartPracticeSessionRequest request, CancellationToken cancellationToken)
    {
        var startResult = await practiceSessionService.StartAsync(request, CurrentUserId, cancellationToken);
        if (!startResult.Succeeded)
        {
            return ToErrorResponse(startResult);
        }

        return CreatedAtAction(nameof(GetById), new { id = startResult.Value!.Id }, startResult.Value);
    }

    /// <summary>Sessions of the authenticated student, newest first.</summary>
    [HttpGet]
    public async Task<IActionResult> GetMine(CancellationToken cancellationToken)
        => Ok(await practiceSessionService.GetMineAsync(CurrentUserId, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var session = await practiceSessionService.GetByIdAsync(id, CurrentUserId, cancellationToken);
        return session is null ? NotFound() : Ok(session);
    }

    [HttpPost("{id:guid}/answers")]
    public async Task<IActionResult> SubmitAnswer(Guid id, SubmitAnswerRequest request, CancellationToken cancellationToken)
        => ToActionResult(await practiceSessionService.SubmitAnswerAsync(id, request, CurrentUserId, cancellationToken));

    [HttpPost("{id:guid}/finish")]
    public async Task<IActionResult> Finish(Guid id, CancellationToken cancellationToken)
        => ToActionResult(await practiceSessionService.FinishAsync(id, CurrentUserId, cancellationToken));
}
