using IcfesApp.Application.Common.Interfaces;
using IcfesApp.Application.Reports.Dtos;
using IcfesApp.Domain.Constants;
using IcfesApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IcfesApp.Infrastructure.Content;

/// <summary>
/// Usa ApplicationDbContext (y no IApplicationDbContext) porque necesita leer usuarios y roles,
/// que pertenecen a Identity y viven en Infrastructure.
/// </summary>
public class ReportService(ApplicationDbContext dbContext) : IReportService
{
    public async Task<ReportResponseDto> GetStudentsReportAsync(Guid teacherId, CancellationToken cancellationToken = default)
    {
        var report = new ReportResponseDto();

        var teacher = await dbContext.Users
            .AsNoTracking()
            .Include(user => user.School)
            .FirstOrDefaultAsync(user => user.Id == teacherId, cancellationToken);

        if (teacher?.SchoolId is null)
        {
            return report;
        }

        report.InstitutionName = teacher.School?.Name;

        var studentRoleId = await dbContext.Roles
            .Where(role => role.Name == Roles.Student)
            .Select(role => role.Id)
            .FirstOrDefaultAsync(cancellationToken);

        var studentsWithAnswerCounts = await dbContext.Users
            .AsNoTracking()
            .Where(user => user.SchoolId == teacher.SchoolId)
            .Where(user => dbContext.UserRoles.Any(userRole => userRole.UserId == user.Id && userRole.RoleId == studentRoleId))
            .Select(student => new
            {
                student.Id,
                student.FirstName,
                student.LastName,
                student.Email,
                TotalAnswered = dbContext.PracticeSessions
                    .Where(practiceSession => practiceSession.StudentUserId == student.Id)
                    .SelectMany(practiceSession => practiceSession.Answers)
                    .Count(answer => answer.SelectedOptionId != null),
                CorrectAnswers = dbContext.PracticeSessions
                    .Where(practiceSession => practiceSession.StudentUserId == student.Id)
                    .SelectMany(practiceSession => practiceSession.Answers)
                    .Count(answer => answer.IsCorrect)
            })
            .ToListAsync(cancellationToken);

        report.Students = studentsWithAnswerCounts
            .Select(student => new StudentReportDto
            {
                StudentId = student.Id,
                FirstName = student.FirstName,
                LastName = student.LastName,
                Email = student.Email ?? string.Empty,
                TotalAnswered = student.TotalAnswered,
                CorrectAnswers = student.CorrectAnswers,
                ScorePercentage = student.TotalAnswered == 0
                    ? 0
                    : Math.Round((double)student.CorrectAnswers / student.TotalAnswered * 100, 1)
            })
            .OrderByDescending(studentReport => studentReport.ScorePercentage)
            .ToList();

        return report;
    }
}
