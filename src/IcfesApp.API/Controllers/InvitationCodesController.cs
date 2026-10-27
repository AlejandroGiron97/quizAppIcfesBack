using System.Security.Claims;
using IcfesApp.Application.Common.Interfaces;
using IcfesApp.Application.Common.Models;
using IcfesApp.Application.InvitationCodes.Dtos;
using IcfesApp.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IcfesApp.API.Controllers;

[ApiController]
[Route("api/invitation-codes")]
[Authorize(Policy = Policies.RequireAdmin)]
public class InvitationCodesController(IInvitationCodeService invitationCodeService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        => Ok(await invitationCodeService.GetAllAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var code = await invitationCodeService.GetByIdAsync(id, cancellationToken);
        return code is null ? NotFound() : Ok(code);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateInvitationCodeRequest request, CancellationToken cancellationToken)
    {
        var result = await invitationCodeService.CreateAsync(request, CurrentUserId, cancellationToken);
        if (!result.Succeeded)
        {
            return BadRequest(new { errors = result.Errors });
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateInvitationCodeRequest request, CancellationToken cancellationToken)
    {
        var result = await invitationCodeService.UpdateAsync(id, request, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Revoca el código sin borrarlo. Se puede reactivar actualizándolo con IsActive = true.
    /// </summary>
    [HttpPost("{id:guid}/revoke")]
    public async Task<IActionResult> Revoke(Guid id, CancellationToken cancellationToken)
    {
        var result = await invitationCodeService.RevokeAsync(id, cancellationToken);
        return ToActionResult(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await invitationCodeService.DeleteAsync(id, cancellationToken);
        return ToActionResult(result);
    }

    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private IActionResult ToActionResult(Result result)
    {
        if (result.IsNotFound)
        {
            return NotFound();
        }

        return result.Succeeded ? NoContent() : BadRequest(new { errors = result.Errors });
    }
}