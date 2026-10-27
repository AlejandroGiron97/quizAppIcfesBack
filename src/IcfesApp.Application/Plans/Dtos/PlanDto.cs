namespace IcfesApp.Application.Plans.Dtos;

public class PlanDto
{
    public Guid Id { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }

    /// <summary>"Particular" o "Institutional"</summary>
    public required string Target { get; set; }

    public int DurationDays { get; set; }
    public decimal Price { get; set; }
    public required string Currency { get; set; }
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
}