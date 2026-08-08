namespace IcfesApp.Application.Schools.Dtos;

public class SchoolDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? City { get; set; }
}
