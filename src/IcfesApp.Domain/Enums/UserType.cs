namespace IcfesApp.Domain.Enums;

/// <summary>
/// Distinguishes the user's access model.
/// - Particular: accesses individually, pays a subscription (FREE/STANDARD/SEMESTRAL).
/// - Institutional: accesses through an institution code, does not pay (the school covers it).
/// 
/// IMPORTANT: Particular = 0 (not 1) to match the C# default value.
/// Users already stored in the database have Type = 0, which now means "Particular".
/// </summary>
public enum UserType
{
    Particular = 0,
    Institutional = 1
}