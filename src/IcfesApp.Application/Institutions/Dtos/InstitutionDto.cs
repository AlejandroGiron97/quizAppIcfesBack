namespace IcfesApp.Application.Institutions.Dtos;

public class InstitutionDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Domain { get; set; }
    public string? City { get; set; }
    public bool IsActive { get; set; }
}