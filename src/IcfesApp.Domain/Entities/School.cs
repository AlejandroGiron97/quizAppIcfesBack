using IcfesApp.Domain.Common;

namespace IcfesApp.Domain.Entities;

public class School : BaseEntity
{
    public required string Name { get; set; }

    /// <summary>Dominio institucional (ej: "colegiosanjose.edu.co").</summary>
    public string? Domain { get; set; }

    public string? City { get; set; }

    /// <summary>Permite desactivar sin borrar.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Qué SuperAdmin la creó. Auditoría.</summary>
    public Guid? CreatedByUserId { get; set; }
}