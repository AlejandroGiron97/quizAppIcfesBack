namespace IcfesApp.Application.InvitationCodes.Dtos;

public class InvitationCodeDto
{
    public Guid Id { get; set; }
    public required string Code { get; set; }
    public Guid InstitutionId { get; set; }
    public required string InstitutionName { get; set; }
    public int MaxUses { get; set; }
    public int Uses { get; set; }
    public DateTime? ExpiresAtUtc { get; set; }
    public bool IsActive { get; set; }
    public bool IsUsable { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}