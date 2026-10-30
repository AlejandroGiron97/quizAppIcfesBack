using IcfesApp.Application.Common.Models;
using IcfesApp.Application.InvitationCodes.Dtos;

namespace IcfesApp.Application.Common.Interfaces;

public interface IInvitationCodeService
{
    Task<IReadOnlyList<InvitationCodeDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Result<InvitationCodeDto>> CreateAsync(
        CreateInvitationCodeRequest request,
        Guid? createdByUserId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates uses, expiration and active state. Revoking a code = updating it with IsActive = false.
    /// Returns the updated code so the client can refresh its list without reloading it.
    /// </summary>
    Task<Result<InvitationCodeDto>> UpdateAsync(Guid id, UpdateInvitationCodeRequest request, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
