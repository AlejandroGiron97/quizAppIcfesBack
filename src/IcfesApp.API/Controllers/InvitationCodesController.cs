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

    [HttpPost]
    public async Task<IActionResult> Create(CreateInvitationCodeRequest request, CancellationToken cancellationToken)
        => ToCreatedResult(await invitationCodeService.CreateAsync(request, CurrentUserId, cancellationToken));

    /// <summary>
    /// Updates uses, expiration and active state. To revoke a code send IsActive = false
    /// (it is not deleted and can be reactivated later with IsActive = true).
    /// Returns the updated code so the client can update its list without reloading it.
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateInvitationCodeRequest request, CancellationToken cancellationToken)
        => ToActionResult(await invitationCodeService.UpdateAsync(id, request, cancellationToken));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        => ToActionResult(await invitationCodeService.DeleteAsync(id, cancellationToken));
}
