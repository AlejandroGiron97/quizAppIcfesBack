namespace IcfesApp.Application.Subjects.Dtos;

public class SubjectDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
}
