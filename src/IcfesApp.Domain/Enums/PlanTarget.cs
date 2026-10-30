namespace IcfesApp.Domain.Enums;

/// <summary>
/// Segment a plan is aimed at.
/// </summary>
public enum PlanTarget
{
    /// <summary>Plan for users who access individually (self-service payment).</summary>
    Particular = 0,

    /// <summary>Plan for institutions (yearly license, manual payment).</summary>
    Institutional = 1
}