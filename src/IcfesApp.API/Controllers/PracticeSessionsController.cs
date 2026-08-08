using System.Security.Claims;
using IcfesApp.Application.Common.Interfaces;
using IcfesApp.Application.Common.Models;
using IcfesApp.Application.Practice.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IcfesApp.API.Controllers;

[ApiController]
[Route("api/practice-sessions")]
[Authorize]
public class PracticeSessionsController(IPracticeSessionService practiceSessionService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Start(StartPracticeSessionRequest request, CancellationToken cancellationToken)
    {
        var result = await practiceSessionService.StartAsync(request, CurrentUserId, cancellationToken);
        if (!result.Succeeded)
        {
            return BadRequest(new { errors = result.Errors });
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }

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
    {
        var result = await practiceSessionService.SubmitAnswerAsync(id, request, CurrentUserId, cancellationToken);
        return result.Succeeded ? Ok(result.Value) : BadRequest(new { errors = result.Errors });
    }

    [HttpPost("{id:guid}/finish")]
    public async Task<IActionResult> Finish(Guid id, CancellationToken cancellationToken)
    {
        var result = await practiceSessionService.FinishAsync(id, CurrentUserId, cancellationToken);
        return result.Succeeded ? Ok(result.Value) : BadRequest(new { errors = result.Errors });
    }

    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
