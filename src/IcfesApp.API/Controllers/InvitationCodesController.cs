using IcfesApp.Application.Common.Interfaces;
using IcfesApp.Application.InvitationCodes.Dtos;
using IcfesApp.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IcfesApp.API.Controllers;

[Route("api/invitation-codes")]
[Authorize(Policy = Policies.RequireAdmin)]
public class InvitationCodesController(IInvitationCodeService invitationCodeService) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        => Ok(await invitationCodeService.GetAllAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var invitationCode = await invitationCodeService.GetByIdAsync(id, cancellationToken);
        return invitationCode is null ? NotFound() : Ok(invitationCode);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateInvitationCodeRequest request, CancellationToken cancellationToken)
    {
        var createResult = await invitationCodeService.CreateAsync(request, CurrentUserId, cancellationToken);
        if (!createResult.Succeeded)
        {
            return ToErrorResponse(createResult);
        }

        return CreatedAtAction(nameof(GetById), new { id = createResult.Value!.Id }, createResult.Value);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateInvitationCodeRequest request, CancellationToken cancellationToken)
        => ToActionResult(await invitationCodeService.UpdateAsync(id, request, cancellationToken));

    /// <summary>
    /// Revoca el código sin borrarlo. Se puede reactivar actualizándolo con IsActive = true.
    /// </summary>
    [HttpPost("{id:guid}/revoke")]
    public async Task<IActionResult> Revoke(Guid id, CancellationToken cancellationToken)
        => ToActionResult(await invitationCodeService.RevokeAsync(id, cancellationToken));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        => ToActionResult(await invitationCodeService.DeleteAsync(id, cancellationToken));
}
