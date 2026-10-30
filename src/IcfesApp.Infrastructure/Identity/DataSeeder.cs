using System.Text.Json;
using IcfesApp.Domain.Builders;
using IcfesApp.Domain.Constants;
using IcfesApp.Domain.Entities;
using IcfesApp.Domain.Enums;
using IcfesApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IcfesApp.Infrastructure.Identity;

/// <summary>
/// Demo data (schools, test users, questions, sessions, plans...).
/// Only runs in Development (see Program.cs): it creates users with a known password.
/// </summary>
public static class DataSeeder
{
    public const string DefaultPassword = "Test12345!";
    private const string QuestionsSeedFileName = "preguntas_icfes.json";

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var schools = await SeedSchoolsAsync(context);
        var subjects = await SeedSubjectsAsync(context);
        await SeedUsersAsync(userManager, schools);
        await SeedInvitationCodesAsync(context, schools);
        await SeedIcfesQuestionsAsync(context, userManager, subjects);
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
            return existing.ToDictionary(school => school.Name);

        var schools = new List<School>
        {
            new() { Name = "Colegio San José", City = "Bogotá" },
            new() { Name = "Liceo Moderno", City = "Medellín" },
            new() { Name = "I.E. La Esperanza", City = "Cali" }
        };

        context.Schools.AddRange(schools);
        await context.SaveChangesAsync();
        return schools.ToDictionary(school => school.Name);
    }

    // ─────────────────────────────────────────────────────────────
    // 2. Subjects
    // ─────────────────────────────────────────────────────────────
    private static async Task<Dictionary<string, Subject>> SeedSubjectsAsync(ApplicationDbContext context)
    {
        var existing = await context.Subjects.ToListAsync();
        if (existing.Count > 0)
            return existing.ToDictionary(subject => subject.Name);

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
        return subjects.ToDictionary(subject => subject.Name);
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
        var existingUser = await userManager.FindByEmailAsync(email);
        if (existingUser is not null) return;

        var demoUser = ApplicationUserFactory.CreatePreconfirmed(email, firstName, lastName, schoolId);

        var createUserResult = await userManager.CreateAsync(demoUser, DefaultPassword);
        if (createUserResult.Succeeded)
            await userManager.AddToRoleAsync(demoUser, role);
    }

    // ─────────────────────────────────────────────────────────────
    // 3.5. InvitationCodes
    // ─────────────────────────────────────────────────────────────
    private static async Task SeedInvitationCodesAsync(
        ApplicationDbContext context,
        Dictionary<string, School> schools)
    {
        if (await context.InvitationCodes.AnyAsync()) return;

        var sanJoseSchoolId = schools["Colegio San José"].Id;
        var liceoModernoSchoolId = schools["Liceo Moderno"].Id;

        var codes = new List<InvitationCode>
        {
            new() { Code = "COLEGIO-SJ-2026", InstitutionId = sanJoseSchoolId, MaxUses = 100, Uses = 0, ExpiresAtUtc = DateTime.UtcNow.AddMonths(6), IsActive = true },
            new() { Code = "LICEOMODERNO-2026", InstitutionId = liceoModernoSchoolId, MaxUses = 50, Uses = 0, ExpiresAtUtc = DateTime.UtcNow.AddMonths(6), IsActive = true },
            new() { Code = "PRUEBA-ILIMITADO", InstitutionId = sanJoseSchoolId, MaxUses = -1, Uses = 0, ExpiresAtUtc = null, IsActive = true }
        };

        context.InvitationCodes.AddRange(codes);
        await context.SaveChangesAsync();
    }

    // ─────────────────────────────────────────────────────────────
    // 4. Real ICFES questions (from preguntas_icfes.json)
    // ─────────────────────────────────────────────────────────────
    private static async Task SeedIcfesQuestionsAsync(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        Dictionary<string, Subject> subjectsByName)
    {
        var adminUser = await userManager.FindByEmailAsync(RoleSeeder.DefaultAdminEmail);
        if (adminUser is null) return;

        var questionsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, QuestionsSeedFileName);
        if (!File.Exists(questionsFilePath)) return;

        var questionsJson = await File.ReadAllTextAsync(questionsFilePath);
        var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var questionSeeds = JsonSerializer.Deserialize<List<QuestionSeedDto>>(questionsJson, jsonOptions);
        if (questionSeeds is null) return;

        // A single query to know which questions already exist (it used to be one query per question).
        var existingQuestionKeys = (await context.Questions
                .Select(question => new { question.SubjectId, question.Text })
                .ToListAsync())
            .Select(existingQuestion => (existingQuestion.SubjectId, existingQuestion.Text))
            .ToHashSet();

        foreach (var questionSeed in questionSeeds)
        {
            if (!subjectsByName.TryGetValue(questionSeed.Subject, out var subject)) continue;
            if (existingQuestionKeys.Contains((subject.Id, questionSeed.Text.Trim()))) continue;

            var difficulty = Enum.TryParse<QuestionDifficulty>(questionSeed.Difficulty, ignoreCase: true, out var parsedDifficulty)
                ? parsedDifficulty
                : QuestionDifficulty.Medium;

            var questionBuilder = QuestionBuilder.ForSubject(subject.Id)
                .WithStatement(questionSeed.Text, questionSeed.ImageUrl)
                .WithJustification(questionSeed.Justification, questionSeed.JustificationImageUrl)
                .WithDifficulty(difficulty)
                .CreatedBy(adminUser.Id);

            foreach (var optionSeed in questionSeed.Options)
            {
                questionBuilder.AddOption(optionSeed.Text, optionSeed.IsCorrect, optionSeed.ImageUrl);
            }

            // A badly written question in the JSON is skipped instead of breaking startup.
            if (questionBuilder.Validate().Count > 0) continue;

            context.Questions.Add(questionBuilder.Build());
        }

        await context.SaveChangesAsync();
    }

    // ─────────────────────────────────────────────────────────────
    // Shape of each question in preguntas_icfes.json
    // ─────────────────────────────────────────────────────────────
    private class QuestionSeedDto
    {
        public string Subject { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string Justification { get; set; } = string.Empty;
        public string? JustificationImageUrl { get; set; }
        public string Difficulty { get; set; } = "Medium";
        public List<SeedOptionDto> Options { get; set; } = new();
    }

    private class SeedOptionDto
    {
        public string Text { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public bool IsCorrect { get; set; }
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
            .Include(question => question.Options)
            .Where(question => question.SubjectId == subjectId)
            .OrderBy(question => question.Text)
            .Take(total)
            .ToListAsync();

        if (questions.Count == 0) return;

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

        for (var questionIndex = 0; questionIndex < questions.Count; questionIndex++)
        {
            var question = questions[questionIndex];
            var correctOption = question.CorrectOption;
            var wrongOption = question.Options.FirstOrDefault(option => !option.IsCorrect) ?? correctOption;
            var answeredCorrectly = questionIndex < correctCount;

            context.StudentAnswers.Add(new StudentAnswer
            {
                PracticeSessionId = session.Id,
                QuestionId = question.Id,
                SelectedOptionId = (answeredCorrectly ? correctOption : wrongOption).Id,
                IsCorrect = answeredCorrectly,
                AnsweredAtUtc = startedAt.AddMinutes(questionIndex * 5 + 2)
            });
        }

        await context.SaveChangesAsync();
    }

    private static async Task CreateInProgressSession(
        ApplicationDbContext context, Guid studentId, Guid subjectId, int answeredCount)
    {
        var questions = await context.Questions
            .Include(question => question.Options)
            .Where(question => question.SubjectId == subjectId)
            .OrderBy(question => question.Text)
            .Take(5)
            .ToListAsync();

        if (questions.Count == 0) return;

        var session = new PracticeSession
        {
            StudentUserId = studentId,
            SubjectId = subjectId,
            Status = PracticeSessionStatus.InProgress,
            StartedAtUtc = DateTime.UtcNow.AddMinutes(-20)
        };

        context.PracticeSessions.Add(session);
        await context.SaveChangesAsync();

        for (var questionIndex = 0; questionIndex < answeredCount && questionIndex < questions.Count; questionIndex++)
        {
            var question = questions[questionIndex];

            context.StudentAnswers.Add(new StudentAnswer
            {
                PracticeSessionId = session.Id,
                QuestionId = question.Id,
                SelectedOptionId = question.CorrectOption.Id,
                IsCorrect = true,
                AnsweredAtUtc = session.StartedAtUtc.AddMinutes(questionIndex * 3 + 1)
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
            return existing.ToDictionary(plan => plan.Code);

        var plans = new List<Plan>
        {
            new() { Code = "FREE", Name = "Prueba gratuita", Description = "7 días de acceso completo. Se activa una vez por usuario.", Target = PlanTarget.Particular, DurationDays = 7, Price = 0m, Currency = "COP", DisplayOrder = 1, IsActive = true },
            new() { Code = "STANDARD", Name = "Plan Estándar", Description = "Acceso completo por 30 días.", Target = PlanTarget.Particular, DurationDays = 30, Price = 29900m, Currency = "COP", DisplayOrder = 2, IsActive = true },
            new() { Code = "SEMESTRAL", Name = "Plan Semestral", Description = "Acceso completo por 180 días. Ahorra 17%.", Target = PlanTarget.Particular, DurationDays = 180, Price = 149900m, Currency = "COP", DisplayOrder = 3, IsActive = true },
            new() { Code = "INSTITUTIONAL", Name = "Licencia Institucional", Description = "Acceso completo por 365 días. Hasta 200 estudiantes.", Target = PlanTarget.Institutional, DurationDays = 365, Price = 1800000m, Currency = "COP", DisplayOrder = 1, IsActive = true }
        };

        context.Plans.AddRange(plans);
        await context.SaveChangesAsync();
        return plans.ToDictionary(plan => plan.Code);
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

        var sanJoseSubscription = new Subscription { InstitutionId = schools["Colegio San José"].Id, PlanId = institutionalPlan.Id, Status = SubscriptionStatus.Active, StartedAtUtc = now.AddDays(-30), ExpiresAtUtc = now.AddDays(335), AutoRenew = false, Notes = "Licencia anual - Factura #SJ-2026-001 (demo)" };
        var liceoModernoSubscription = new Subscription { InstitutionId = schools["Liceo Moderno"].Id, PlanId = institutionalPlan.Id, Status = SubscriptionStatus.Active, StartedAtUtc = now.AddDays(-15), ExpiresAtUtc = now.AddDays(350), AutoRenew = false, Notes = "Licencia anual - Factura #LM-2026-002 (demo)" };

        var student4 = await context.Users.FirstOrDefaultAsync(user => user.Email == "student4@test.com");
        var student5 = await context.Users.FirstOrDefaultAsync(user => user.Email == "student5@test.com");

        var subscriptions = new List<Subscription> { sanJoseSubscription, liceoModernoSubscription };

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