using System.ComponentModel.DataAnnotations;

namespace IcfesApp.Application.Subjects.Dtos;

public class CreateSubjectRequest
{
    [Required, MaxLength(150)]
    public required string Name { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }
}
