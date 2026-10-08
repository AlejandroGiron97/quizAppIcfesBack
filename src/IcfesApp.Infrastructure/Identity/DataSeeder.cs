using IcfesApp.Domain.Constants;
using IcfesApp.Domain.Entities;
using IcfesApp.Domain.Enums;
using IcfesApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IcfesApp.Infrastructure.Identity;

public static class DataSeeder
{
    public const string DefaultPassword = "Test12345!";

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var schools = await SeedSchoolsAsync(context);
        var subjects = await SeedSubjectsAsync(context);
        await SeedUsersAsync(userManager, schools);
        await SeedQuestionsAsync(context, userManager, subjects);
        await SeedPracticeSessionsAsync(context, userManager, subjects);
    }

    // ─────────────────────────────────────────────────────────────
    // 1. Schools
    // ─────────────────────────────────────────────────────────────
    private static async Task<Dictionary<string, School>> SeedSchoolsAsync(ApplicationDbContext context)
    {
        var existing = await context.Schools.ToListAsync();
        if (existing.Count > 0)
            return existing.ToDictionary(s => s.Name, s => s);

        var schools = new List<School>
        {
            new() { Name = "Colegio San José", City = "Bogotá" },
            new() { Name = "Liceo Moderno", City = "Medellín" },
            new() { Name = "I.E. La Esperanza", City = "Cali" }
        };

        context.Schools.AddRange(schools);
        await context.SaveChangesAsync();
        return schools.ToDictionary(s => s.Name, s => s);
    }

    // ─────────────────────────────────────────────────────────────
    // 2. Subjects
    // ─────────────────────────────────────────────────────────────
    private static async Task<Dictionary<string, Subject>> SeedSubjectsAsync(ApplicationDbContext context)
    {
        var existing = await context.Subjects.ToListAsync();
        if (existing.Count > 0)
            return existing.ToDictionary(s => s.Name, s => s);

        var subjects = new List<Subject>
        {
            new() { Name = "Matemáticas",       Description = "Razonamiento cuantitativo y resolución de problemas" },
            new() { Name = "Lectura Crítica",   Description = "Comprensión lectora y análisis de textos" },
            new() { Name = "Ciencias Naturales", Description = "Biología, Química y Física" },
            new() { Name = "Ciencias Sociales", Description = "Historia, Geografía y Constitución" },
            new() { Name = "Inglés",            Description = "Comprensión y uso del idioma inglés" }
        };

        context.Subjects.AddRange(subjects);
        await context.SaveChangesAsync();
        return subjects.ToDictionary(s => s.Name, s => s);
    }

    // ─────────────────────────────────────────────────────────────
    // 3. Users (Teachers + Students)
    // ─────────────────────────────────────────────────────────────
    private static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager, Dictionary<string, School> schools)
    {
        await EnsureUserAsync(userManager, "teacher1@test.com", "Carlos", "Ramírez",  Roles.Teacher, schools["Colegio San José"].Id);
        await EnsureUserAsync(userManager, "teacher2@test.com", "María",  "González", Roles.Teacher, schools["Liceo Moderno"].Id);

        await EnsureUserAsync(userManager, "student1@test.com", "Juan",   "Pérez",     Roles.Student, schools["Colegio San José"].Id);
        await EnsureUserAsync(userManager, "student2@test.com", "Pepito", "Rodríguez", Roles.Student, schools["Colegio San José"].Id);
        await EnsureUserAsync(userManager, "student3@test.com", "Ana",    "Martínez",  Roles.Student, schools["Liceo Moderno"].Id);
        await EnsureUserAsync(userManager, "student4@test.com", "Diego",  "López",     Roles.Student, null);
        await EnsureUserAsync(userManager, "student5@test.com", "Sofía",  "Hernández", Roles.Student, null);
    }

    private static async Task EnsureUserAsync(
        UserManager<ApplicationUser> userManager,
        string email, string firstName, string lastName,
        string role, Guid? schoolId)
    {
        var existing = await userManager.FindByEmailAsync(email);
        if (existing is not null) return;

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            EmailConfirmed = true,
            SchoolId = schoolId
        };

        var result = await userManager.CreateAsync(user, DefaultPassword);
        if (result.Succeeded)
            await userManager.AddToRoleAsync(user, role);
    }

    // ─────────────────────────────────────────────────────────────
    // 4. Questions (5 por materia = 25)
    // ─────────────────────────────────────────────────────────────
    private static async Task SeedQuestionsAsync(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        Dictionary<string, Subject> subjects)
    {
        if (await context.Questions.AnyAsync()) return;

        var admin = await userManager.FindByEmailAsync(RoleSeeder.DefaultAdminEmail);
        if (admin is null) return;
        var createdBy = admin.Id;

        var math = subjects["Matemáticas"].Id;
        var read = subjects["Lectura Crítica"].Id;
        var sci  = subjects["Ciencias Naturales"].Id;
        var soc  = subjects["Ciencias Sociales"].Id;
        var eng  = subjects["Inglés"].Id;

        var questions = new List<Question>
        {
            // ── Matemáticas ──
            Q(math, createdBy, "Si x + 5 = 12, ¿cuál es el valor de x?", "Restamos 5 a ambos lados: x = 12 − 5 = 7.",
                QuestionDifficulty.Easy, new[] { "5", "7", "12", "17" }, 1),
            Q(math, createdBy, "¿Cuánto es 3/4 + 1/2?", "Se convierte 1/2 a 2/4. Luego 3/4 + 2/4 = 5/4.",
                QuestionDifficulty.Medium, new[] { "4/6", "5/4", "1", "3/2" }, 1),
            Q(math, createdBy, "En un triángulo rectángulo los catetos miden 3 y 4. ¿Cuánto mide la hipotenusa?", "Por Pitágoras: √(3² + 4²) = √25 = 5.",
                QuestionDifficulty.Easy, new[] { "5", "6", "7", "12" }, 0),
            Q(math, createdBy, "La derivada de f(x) = x³ + 2x es:", "Aplicando regla de potencias: 3x² + 2.",
                QuestionDifficulty.Hard, new[] { "x² + 2", "3x² + 2", "3x + 2", "x³ + 2" }, 1),
            Q(math, createdBy, "Un tren viaja a 80 km/h durante 2.5 horas. ¿Qué distancia recorre?", "Distancia = velocidad × tiempo = 80 × 2.5 = 200 km.",
                QuestionDifficulty.Easy, new[] { "160 km", "180 km", "200 km", "220 km" }, 2),

            // ── Lectura Crítica ──
            Q(read, createdBy, "La palabra 'efímero' puede reemplazarse por:", "Efímero significa que dura poco tiempo; 'pasajero' es su sinónimo.",
                QuestionDifficulty.Medium, new[] { "eterno", "pasajero", "sólido", "grande" }, 1),
            Q(read, createdBy, "La idea principal de un párrafo suele estar en:", "Por convención, la idea principal suele ubicarse en la primera oración.",
                QuestionDifficulty.Easy, new[] { "la primera oración", "el título", "la última coma", "las notas al pie" }, 0),
            Q(read, createdBy, "El propósito de un texto argumentativo es:", "Buscar convencer al lector mediante razones y evidencias.",
                QuestionDifficulty.Medium, new[] { "narrar hechos", "convencer al lector", "describir un lugar", "listar datos" }, 1),
            Q(read, createdBy, "El texto expositivo se caracteriza por:", "Explicar o informar sobre un tema de forma objetiva.",
                QuestionDifficulty.Medium, new[] { "expresar sentimientos", "informar objetivamente", "contar una historia", "dar órdenes" }, 1),
            Q(read, createdBy, "Una inferencia es:", "Una conclusión que se obtiene a partir de la información explícita del texto.",
                QuestionDifficulty.Hard, new[] { "un dato literal", "una conclusión implícita", "una cita textual", "un resumen" }, 1),

            // ── Ciencias Naturales ──
            Q(sci, createdBy, "El proceso mediante el cual las plantas producen su alimento es:", "La fotosíntesis convierte luz, agua y CO₂ en glucosa.",
                QuestionDifficulty.Easy, new[] { "respiración", "fotosíntesis", "digestión", "transpiración" }, 1),
            Q(sci, createdBy, "¿Qué estado de la materia tiene forma y volumen definidos?", "El estado sólido mantiene forma y volumen constante.",
                QuestionDifficulty.Easy, new[] { "sólido", "líquido", "gaseoso", "plasma" }, 0),
            Q(sci, createdBy, "La unidad básica de la vida es:", "La célula es la unidad estructural y funcional de todo ser vivo.",
                QuestionDifficulty.Easy, new[] { "el átomo", "la célula", "el tejido", "el órgano" }, 1),
            Q(sci, createdBy, "El ADN se encuentra principalmente en:", "En células eucariotas el ADN está en el núcleo.",
                QuestionDifficulty.Medium, new[] { "el citoplasma", "el núcleo", "la pared celular", "los ribosomas" }, 1),
            Q(sci, createdBy, "La ley de acción y reacción es la:", "Corresponde a la tercera ley de Newton.",
                QuestionDifficulty.Medium, new[] { "primera ley", "segunda ley", "tercera ley", "ley de gravitación" }, 2),

            // ── Ciencias Sociales ──
            Q(soc, createdBy, "La Revolución Francesa comenzó en el año:", "Se inició en 1789 con la toma de la Bastilla.",
                QuestionDifficulty.Medium, new[] { "1776", "1789", "1810", "1914" }, 1),
            Q(soc, createdBy, "El poder ejecutivo en Colombia está representado por:", "El Presidente de la República encabeza el poder ejecutivo.",
                QuestionDifficulty.Easy, new[] { "el Congreso", "el Presidente", "la Corte", "los gobernadores" }, 1),
            Q(soc, createdBy, "El organismo que regula el comercio internacional es:", "La OMC (Organización Mundial del Comercio) regula el comercio global.",
                QuestionDifficulty.Medium, new[] { "la ONU", "la OMC", "la OTAN", "la UNESCO" }, 1),
            Q(soc, createdBy, "La globalización se caracteriza principalmente por:", "Integración económica, cultural y tecnológica entre países.",
                QuestionDifficulty.Medium, new[] { "aislamiento regional", "integración mundial", "guerra comercial", "autarquía" }, 1),
            Q(soc, createdBy, "La Constitución de Colombia de 1991 define al país como:", "Un Estado social de derecho, democrático y participativo.",
                QuestionDifficulty.Medium, new[] { "una monarquía", "un Estado social de derecho", "un Estado confesional", "una dictadura" }, 1),

            // ── Inglés ──
            Q(eng, createdBy, "Choose the correct form: 'She ___ to school every day.'", "Tercera persona singular del presente simple: 'goes'.",
                QuestionDifficulty.Easy, new[] { "go", "goes", "going", "gone" }, 1),
            Q(eng, createdBy, "What is the past tense of 'go'?", "El pasado irregular de 'go' es 'went'.",
                QuestionDifficulty.Easy, new[] { "goed", "goes", "went", "gone" }, 2),
            Q(eng, createdBy, "Select the correct sentence:", "Uso correcto del present perfect con 'for'.",
                QuestionDifficulty.Medium, new[] { "I live here since 5 years.", "I have lived here for 5 years.", "I am living here 5 years.", "I lived here for 5 years ago." }, 1),
            Q(eng, createdBy, "The word 'beautiful' is a(n):", "'Beautiful' describe un sustantivo, por lo tanto es un adjetivo.",
                QuestionDifficulty.Easy, new[] { "noun", "verb", "adjective", "adverb" }, 2),
            Q(eng, createdBy, "Complete: 'If it rains, I ___ stay home.'", "Primera condicional: 'if' + presente, futuro con 'will'.",
                QuestionDifficulty.Medium, new[] { "would", "will", "did", "am" }, 1),
        };

        context.Questions.AddRange(questions);
        await context.SaveChangesAsync();
    }

    private static Question Q(
        Guid subjectId, Guid createdBy,
        string text, string justification,
        QuestionDifficulty difficulty,
        string[] options, int correctIndex)
    {
        var question = new Question
        {
            Text = text,
            Justification = justification,
            Difficulty = difficulty,
            SubjectId = subjectId,
            CreatedByUserId = createdBy
        };

        for (int i = 0; i < options.Length; i++)
        {
            question.Options.Add(new QuestionOption
            {
                Text = options[i],
                IsCorrect = i == correctIndex
            });
        }

        return question;
    }

    // ─────────────────────────────────────────────────────────────
    // 5. PracticeSessions + StudentAnswers
    // ─────────────────────────────────────────────────────────────
    private static async Task SeedPracticeSessionsAsync(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        Dictionary<string, Subject> subjects)
    {
        if (await context.PracticeSessions.AnyAsync()) return;

        var student1 = await userManager.FindByEmailAsync("student1@test.com");
        var student2 = await userManager.FindByEmailAsync("student2@test.com");
        var student3 = await userManager.FindByEmailAsync("student3@test.com");
        var student4 = await userManager.FindByEmailAsync("student4@test.com");
        if (student1 is null || student2 is null || student3 is null || student4 is null) return;

        // Sesión 1 — student1 — Matemáticas — Completada, 4/5 correctas
        await CreateCompletedSession(context, student1.Id, subjects["Matemáticas"].Id, DateTime.UtcNow.AddDays(-5), correctCount: 4, total: 5);
        // Sesión 2 — student1 — Lectura Crítica — Completada, 3/5
        await CreateCompletedSession(context, student1.Id, subjects["Lectura Crítica"].Id, DateTime.UtcNow.AddDays(-3), correctCount: 3, total: 5);
        // Sesión 3 — student2 — Matemáticas — Completada, 1/5
        await CreateCompletedSession(context, student2.Id, subjects["Matemáticas"].Id, DateTime.UtcNow.AddDays(-2), correctCount: 1, total: 5);
        // Sesión 4 — student3 — Ciencias Naturales — En progreso, 2/5
        await CreateInProgressSession(context, student3.Id, subjects["Ciencias Naturales"].Id, answeredCount: 2);
        // Sesión 5 — student4 — Inglés — Abandonada
        await CreateAbandonedSession(context, student4.Id, subjects["Inglés"].Id);
    }

    private static async Task CreateCompletedSession(
        ApplicationDbContext context, Guid studentId, Guid subjectId,
        DateTime startedAt, int correctCount, int total)
    {
        var questions = await context.Questions
            .Include(q => q.Options)
            .Where(q => q.SubjectId == subjectId)
            .OrderBy(q => q.Text)
            .Take(total)
            .ToListAsync();

        var session = new PracticeSession
        {
            StudentUserId = studentId,
            SubjectId = subjectId,
            Status = PracticeSessionStatus.Completed,
            StartedAtUtc = startedAt,
            FinishedAtUtc = startedAt.AddMinutes(45)
        };

        context.PracticeSessions.Add(session);
        await context.SaveChangesAsync();

        for (int i = 0; i < questions.Count; i++)
        {
            var q = questions[i];
            var correctOption = q.Options.First(o => o.IsCorrect);
            var wrongOption = q.Options.First(o => !o.IsCorrect);
            var isCorrect = i < correctCount;

            context.StudentAnswers.Add(new StudentAnswer
            {
                PracticeSessionId = session.Id,
                QuestionId = q.Id,
                SelectedOptionId = (isCorrect ? correctOption : wrongOption).Id,
                IsCorrect = isCorrect,
                AnsweredAtUtc = startedAt.AddMinutes(i * 5 + 2)
            });
        }

        await context.SaveChangesAsync();
    }

    private static async Task CreateInProgressSession(
        ApplicationDbContext context, Guid studentId, Guid subjectId, int answeredCount)
    {
        var questions = await context.Questions
            .Include(q => q.Options)
            .Where(q => q.SubjectId == subjectId)
            .OrderBy(q => q.Text)
            .Take(5)
            .ToListAsync();

        var session = new PracticeSession
        {
            StudentUserId = studentId,
            SubjectId = subjectId,
            Status = PracticeSessionStatus.InProgress,
            StartedAtUtc = DateTime.UtcNow.AddMinutes(-20)
        };

        context.PracticeSessions.Add(session);
        await context.SaveChangesAsync();

        for (int i = 0; i < answeredCount && i < questions.Count; i++)
        {
            var q = questions[i];
            var correctOption = q.Options.First(o => o.IsCorrect);

            context.StudentAnswers.Add(new StudentAnswer
            {
                PracticeSessionId = session.Id,
                QuestionId = q.Id,
                SelectedOptionId = correctOption.Id,
                IsCorrect = true,
                AnsweredAtUtc = session.StartedAtUtc.AddMinutes(i * 3 + 1)
            });
        }

        await context.SaveChangesAsync();
    }

    private static async Task CreateAbandonedSession(
        ApplicationDbContext context, Guid studentId, Guid subjectId)
    {
        var session = new PracticeSession
        {
            StudentUserId = studentId,
            SubjectId = subjectId,
            Status = PracticeSessionStatus.Abandoned,
            StartedAtUtc = DateTime.UtcNow.AddDays(-1),
            FinishedAtUtc = DateTime.UtcNow.AddDays(-1).AddMinutes(15)
        };

        context.PracticeSessions.Add(session);
        await context.SaveChangesAsync();
    }
}