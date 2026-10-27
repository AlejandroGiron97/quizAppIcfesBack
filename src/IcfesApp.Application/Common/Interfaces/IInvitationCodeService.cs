using IcfesApp.Application.Common.Models;
using IcfesApp.Application.InvitationCodes.Dtos;

namespace IcfesApp.Application.Common.Interfaces;

public interface IInvitationCodeService
{
    Task<IReadOnlyList<InvitationCodeDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<InvitationCodeDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<InvitationCodeDto>> CreateAsync(
        CreateInvitationCodeRequest request,
        Guid? createdByUserId,
        CancellationToken cancellationToken = default);

    Task<Result> UpdateAsync(Guid id, UpdateInvitationCodeRequest request, CancellationToken cancellationToken = default);
    Task<Result> RevokeAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}