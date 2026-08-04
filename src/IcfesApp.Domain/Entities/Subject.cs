using IcfesApp.Domain.Common;

namespace IcfesApp.Domain.Entities;

public class Subject : BaseEntity
{
    public required string Name { get; set; }
    public string? Description { get; set; }

    public ICollection<Question> Questions { get; set; } = [];
}
