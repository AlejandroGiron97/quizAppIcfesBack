using IcfesApp.Application.Common.Interfaces;
using IcfesApp.Application.Practice.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IcfesApp.API.Controllers;

[Route("api/practice-sessions")]
[Authorize]
public class PracticeSessionsController(IPracticeSessionService practiceSessionService) : ApiControllerBase
{
    /// <summary>Starts a session with random questions of a subject. Returns the questions and options (never the answers).</summary>
    [HttpPost]
    public async Task<IActionResult> Start(StartPracticeSessionRequest request, CancellationToken cancellationToken)
        => ToCreatedResult(await practiceSessionService.StartAsync(request, CurrentUserId, cancellationToken));

    /// <summary>Practice history of the authenticated student (summaries with counts, no questions), newest first.</summary>
    [HttpGet("history")]
    public async Task<IActionResult> GetHistory(CancellationToken cancellationToken)
        => Ok(await practiceSessionService.GetHistoryAsync(CurrentUserId, cancellationToken));

    [HttpPost("{id:guid}/answers")]
    public async Task<IActionResult> SubmitAnswer(Guid id, SubmitAnswerRequest request, CancellationToken cancellationToken)
        => ToActionResult(await practiceSessionService.SubmitAnswerAsync(id, request, CurrentUserId, cancellationToken));

    [HttpPost("{id:guid}/finish")]
    public async Task<IActionResult> Finish(Guid id, CancellationToken cancellationToken)
        => ToActionResult(await practiceSessionService.FinishAsync(id, CurrentUserId, cancellationToken));
}
