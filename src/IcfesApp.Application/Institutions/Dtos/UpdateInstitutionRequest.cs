namespace IcfesApp.Application.Institutions.Dtos;

public class UpdateInstitutionRequest : CreateInstitutionRequest
{
    public bool IsActive { get; set; } = true;
}