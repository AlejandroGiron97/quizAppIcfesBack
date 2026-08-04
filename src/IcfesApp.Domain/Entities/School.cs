using IcfesApp.Domain.Common;

namespace IcfesApp.Domain.Entities;

public class School : BaseEntity
{
    public required string Name { get; set; }
    public string? City { get; set; }
}
