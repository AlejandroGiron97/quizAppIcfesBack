using IcfesApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IcfesApp.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<School> Schools { get; }
    DbSet<Subject> Subjects { get; }
    DbSet<Question> Questions { get; }
    DbSet<QuestionOption> QuestionOptions { get; }
    DbSet<PracticeSession> PracticeSessions { get; }
    DbSet<StudentAnswer> StudentAnswers { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
