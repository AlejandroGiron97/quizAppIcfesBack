using IcfesApp.Application.Common.Interfaces;
using IcfesApp.Domain.Entities;
using IcfesApp.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IcfesApp.Infrastructure.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options), IApplicationDbContext
{
    public DbSet<Institution> Institutions => Set<Institution>();
    public DbSet<InvitationCode> InvitationCodes => Set<InvitationCode>();
    public DbSet<Plan> Plans => Set<Plan>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<QuestionOption> QuestionOptions => Set<QuestionOption>();
    public DbSet<PracticeSession> PracticeSessions => Set<PracticeSession>();
    public DbSet<StudentAnswer> StudentAnswers => Set<StudentAnswer>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}