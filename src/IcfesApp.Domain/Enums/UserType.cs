namespace IcfesApp.Domain.Enums;

/// <summary>
/// Distingue el modelo de acceso del usuario.
/// - Particular: accede individualmente, paga suscripción (FREE/STANDARD/SEMESTRAL).
/// - Institutional: accede mediante código de institución, no paga (lo cubre el colegio).
/// 
/// IMPORTANTE: Particular = 0 (no 1) para coincidir con el default de C#.
/// Los usuarios ya existentes en BD tienen Type = 0, que ahora es "Particular".
/// </summary>
public enum UserType
{
    Particular = 0,
    Institutional = 1
}