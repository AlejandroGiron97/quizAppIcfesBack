namespace IcfesApp.Infrastructure.Seeding;

/// <summary>
/// Names, emails and codes of the demo data, in one place so the seeders can refer to each other's data
/// (e.g. the invitation codes belong to <see cref="ColegioSanJose"/>) without repeating strings.
/// The values are Spanish because they are shown in the app.
/// </summary>
public static class DemoData
{
    /// <summary>Password of every demo user (Development only).</summary>
    public const string DefaultPassword = "Test12345!";

    // Institutions
    public const string ColegioSanJose = "Colegio San José";
    public const string LiceoModerno = "Liceo Moderno";
    public const string LaEsperanza = "I.E. La Esperanza";

    // Subjects (must match the "subject" field of preguntas_icfes.json)
    public const string Mathematics = "Matemáticas";
    public const string CriticalReading = "Lectura Crítica";
    public const string NaturalSciences = "Ciencias Naturales";
    public const string SocialSciences = "Ciencias Sociales";
    public const string English = "Inglés";

    // Users
    public const string Teacher1Email = "teacher1@test.com";
    public const string Teacher2Email = "teacher2@test.com";
    public const string Student1Email = "student1@test.com";
    public const string Student2Email = "student2@test.com";
    public const string Student3Email = "student3@test.com";
    public const string Student4Email = "student4@test.com";
    public const string Student5Email = "student5@test.com";

    // Plans
    public const string FreePlanCode = "FREE";
    public const string StandardPlanCode = "STANDARD";
    public const string SemestralPlanCode = "SEMESTRAL";
    public const string InstitutionalPlanCode = "INSTITUTIONAL";
}
