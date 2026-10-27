using System.ComponentModel.DataAnnotations;

namespace IcfesApp.Application.Schools.Dtos;

public class CreateSchoolRequest
{
    [Required, MaxLength(200)]
    public required string Name { get; set; }

    [MaxLength(150)]
    public string? Domain { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }
}