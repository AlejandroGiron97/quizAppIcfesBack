using System.Text.Json;
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
        await SeedInvitationCodesAsync(context, schools);
        await SeedRealQuestionsAsync(context, userManager, subjects); // Modificado
        await SeedPracticeSessionsAsync(context, userManager, subjects);
        var plans = await SeedPlansAsync(context);
        await SeedSubscriptionsAsync(context, plans, schools);
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
        await EnsureUserAsync(userManager, "teacher1@test.com", "Carlos", "Ramírez", Roles.Teacher, schools["Colegio San José"].Id);
        await EnsureUserAsync(userManager, "teacher2@test.com", "María", "González", Roles.Teacher, schools["Liceo Moderno"].Id);

        await EnsureUserAsync(userManager, "student1@test.com", "Juan", "Pérez", Roles.Student, schools["Colegio San José"].Id);
        await EnsureUserAsync(userManager, "student2@test.com", "Pepito", "Rodríguez", Roles.Student, schools["Colegio San José"].Id);
        await EnsureUserAsync(userManager, "student3@test.com", "Ana", "Martínez", Roles.Student, schools["Liceo Moderno"].Id);
        await EnsureUserAsync(userManager, "student4@test.com", "Diego", "López", Roles.Student, null);
        await EnsureUserAsync(userManager, "student5@test.com", "Sofía", "Hernández", Roles.Student, null);
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
            SchoolId = schoolId,
            Type = schoolId.HasValue
                ? UserType.Institutional
                : UserType.Particular
        };

        var result = await userManager.CreateAsync(user, DefaultPassword);
        if (result.Succeeded)
            await userManager.AddToRoleAsync(user, role);
    }

    // ─────────────────────────────────────────────────────────────
    // 3.5. InvitationCodes (para probar el flujo institucional)
    // ─────────────────────────────────────────────────────────────
    private static async Task SeedInvitationCodesAsync(
        ApplicationDbContext context,
        Dictionary<string, School> schools)
    {
        if (await context.InvitationCodes.AnyAsync()) return;

        var sanJose = schools["Colegio San José"].Id;
        var liceo = schools["Liceo Moderno"].Id;

        var codes = new List<InvitationCode>
        {
            new()
            {
                Code = "COLEGIO-SJ-2026",
                InstitutionId = sanJose,
                MaxUses = 100,
                Uses = 0,
                ExpiresAtUtc = DateTime.UtcNow.AddMonths(6),
                IsActive = true
            },
            new()
            {
                Code = "LICEOMODERNO-2026",
                InstitutionId = liceo,
                MaxUses = 50,
                Uses = 0,
                ExpiresAtUtc = DateTime.UtcNow.AddMonths(6),
                IsActive = true
            },
            new()
            {
                Code = "PRUEBA-ILIMITADO",
                InstitutionId = sanJose,
                MaxUses = -1,
                Uses = 0,
                ExpiresAtUtc = null,
                IsActive = true
            }
        };

        context.InvitationCodes.AddRange(codes);
        await context.SaveChangesAsync();
    }

    // ─────────────────────────────────────────────────────────────
    // 4. PREGUNTAS REALES ICFES (Desde JSON)
    // ─────────────────────────────────────────────────────────────
    private static async Task SeedRealQuestionsAsync(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        Dictionary<string, Subject> subjects)
    {
        if (await context.Questions.AnyAsync()) return;

        var admin = await userManager.FindByEmailAsync(RoleSeeder.DefaultAdminEmail);
        if (admin is null) return;
        var createdBy = admin.Id;

        // Busca el archivo JSON en la carpeta de salida
        var jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "preguntas_icfes.json");
        if (!File.Exists(jsonPath)) return;

        var jsonString = await File.ReadAllTextAsync(jsonPath);
        var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var questionsData = JsonSerializer.Deserialize<List<QuestionSeedDto>>(jsonString, jsonOptions);

        if (questionsData is null) return;

        var questions = new List<Question>();

        foreach (var item in questionsData)
        {
            if (!subjects.TryGetValue(item.Subject, out var subject)) continue;

            var difficulty = item.Difficulty switch
            {
                "Easy" => QuestionDifficulty.Easy,
                "Medium" => QuestionDifficulty.Medium,
                "Hard" => QuestionDifficulty.Hard,
                _ => QuestionDifficulty.Medium
            };

            questions.Add(Q(subject.Id, createdBy, item.Text, item.Justification, difficulty, item.Options.ToArray(), item.CorrectIndex));
        }

        context.Questions.AddRange(questions);
        await context.SaveChangesAsync();
    }

    private class QuestionSeedDto
    {
        public string Subject { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public string Justification { get; set; } = string.Empty;
        public string Difficulty { get; set; } = "Medium";
        public List<string> Options { get; set; } = new();
        public int CorrectIndex { get; set; }
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

        await CreateCompletedSession(context, student1.Id, subjects["Matemáticas"].Id, DateTime.UtcNow.AddDays(-5), correctCount: 4, total: 5);
        await CreateCompletedSession(context, student1.Id, subjects["Lectura Crítica"].Id, DateTime.UtcNow.AddDays(-3), correctCount: 3, total: 5);
        await CreateCompletedSession(context, student2.Id, subjects["Matemáticas"].Id, DateTime.UtcNow.AddDays(-2), correctCount: 1, total: 5);
        await CreateInProgressSession(context, student3.Id, subjects["Ciencias Naturales"].Id, answeredCount: 2);
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

    // ─────────────────────────────────────────────────────────────
    // 6. Plans
    // ─────────────────────────────────────────────────────────────
    private static async Task<Dictionary<string, Plan>> SeedPlansAsync(ApplicationDbContext context)
    {
        var existing = await context.Plans.ToListAsync();
        if (existing.Count > 0)
            return existing.ToDictionary(p => p.Code, p => p);

        var plans = new List<Plan>
        {
            new() { Code = "FREE", Name = "Prueba gratuita", Description = "7 días de acceso completo. Se activa una vez por usuario.", Target = PlanTarget.Particular, DurationDays = 7, Price = 0m, Currency = "COP", DisplayOrder = 1, IsActive = true },
            new() { Code = "STANDARD", Name = "Plan Estándar", Description = "Acceso completo por 30 días.", Target = PlanTarget.Particular, DurationDays = 30, Price = 29900m, Currency = "COP", DisplayOrder = 2, IsActive = true },
            new() { Code = "SEMESTRAL", Name = "Plan Semestral", Description = "Acceso completo por 180 días. Ahorra 17%.", Target = PlanTarget.Particular, DurationDays = 180, Price = 149900m, Currency = "COP", DisplayOrder = 3, IsActive = true },
            new() { Code = "INSTITUTIONAL", Name = "Licencia Institucional", Description = "Acceso completo por 365 días. Hasta 200 estudiantes.", Target = PlanTarget.Institutional, DurationDays = 365, Price = 1800000m, Currency = "COP", DisplayOrder = 1, IsActive = true }
        };

        context.Plans.AddRange(plans);
        await context.SaveChangesAsync();
        return plans.ToDictionary(p => p.Code, p => p);
    }

    // ─────────────────────────────────────────────────────────────
    // 7. Subscriptions
    // ─────────────────────────────────────────────────────────────
    private static async Task SeedSubscriptionsAsync(
        ApplicationDbContext context,
        Dictionary<string, Plan> plans,
        Dictionary<string, School> schools)
    {
        if (await context.Subscriptions.AnyAsync()) return;

        var now = DateTime.UtcNow;
        var institutionalPlan = plans["INSTITUTIONAL"];

        var sanJoseSub = new Subscription { InstitutionId = schools["Colegio San José"].Id, PlanId = institutionalPlan.Id, Status = SubscriptionStatus.Active, StartedAtUtc = now.AddDays(-30), ExpiresAtUtc = now.AddDays(335), AutoRenew = false, Notes = "Licencia anual - Factura #SJ-2026-001 (demo)" };
        var liceoSub = new Subscription { InstitutionId = schools["Liceo Moderno"].Id, PlanId = institutionalPlan.Id, Status = SubscriptionStatus.Active, StartedAtUtc = now.AddDays(-15), ExpiresAtUtc = now.AddDays(350), AutoRenew = false, Notes = "Licencia anual - Factura #LM-2026-002 (demo)" };

        var student4 = await context.Users.FirstOrDefaultAsync(u => u.Email == "student4@test.com");
        var student5 = await context.Users.FirstOrDefaultAsync(u => u.Email == "student5@test.com");

        var subscriptions = new List<Subscription> { sanJoseSub, liceoSub };

        if (student4 is not null)
        {
            subscriptions.Add(new Subscription { UserId = student4.Id, PlanId = plans["STANDARD"].Id, Status = SubscriptionStatus.Active, StartedAtUtc = now.AddDays(-10), ExpiresAtUtc = now.AddDays(20), AutoRenew = true, Notes = "Suscripción demo - Stripe" });
        }

        if (student5 is not null)
        {
            subscriptions.Add(new Subscription { UserId = student5.Id, PlanId = plans["FREE"].Id, Status = SubscriptionStatus.Expired, StartedAtUtc = now.AddDays(-30), ExpiresAtUtc = now.AddDays(-23), AutoRenew = false, Notes = "Trial gratuito vencido" });
        }

        context.Subscriptions.AddRange(subscriptions);
        await context.SaveChangesAsync();
    }
}