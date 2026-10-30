using IcfesApp.Application.InvitationCodes.Dtos;
using IcfesApp.Domain.Entities;

namespace IcfesApp.Application.InvitationCodes;

public static class InvitationCodeMappings
{
    /// <remarks>Requiere que <see cref="InvitationCode.Institution"/> esté cargada.</remarks>
    public static InvitationCodeDto ToDto(this InvitationCode invitationCode) => new()
    {
        Id = invitationCode.Id,
        Code = invitationCode.Code,
        InstitutionId = invitationCode.InstitutionId,
        InstitutionName = invitationCode.Institution?.Name ?? string.Empty,
        MaxUses = invitationCode.MaxUses,
        Uses = invitationCode.Uses,
        ExpiresAtUtc = invitationCode.ExpiresAtUtc,
        IsActive = invitationCode.IsActive,
        IsUsable = invitationCode.IsUsable,
        CreatedAtUtc = invitationCode.CreatedAtUtc,
        UpdatedAtUtc = invitationCode.UpdatedAtUtc
    };
}
