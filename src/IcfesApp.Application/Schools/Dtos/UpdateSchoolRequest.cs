namespace IcfesApp.Application.Schools.Dtos;

public class UpdateSchoolRequest : CreateSchoolRequest
{
    public bool IsActive { get; set; } = true;
}