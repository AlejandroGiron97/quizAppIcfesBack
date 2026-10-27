using IcfesApp.Application.Common.Interfaces;
using IcfesApp.Application.Reports.Dtos;
using IcfesApp.Domain.Constants;
using IcfesApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IcfesApp.Infrastructure.Content;

public class ReportService(ApplicationDbContext dbContext) : IReportService
{
    public async Task<ReportResponseDto> GetStudentsReportAsync(Guid teacherId, CancellationToken cancellationToken = default)
    {
        var response = new ReportResponseDto();

        // 1. Obtener la institución del profesor
        var teacher = await dbContext.Users
            .Include(u => u.School)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == teacherId, cancellationToken);

        if (teacher?.SchoolId == null)
        {
            return response;
        }

        response.InstitutionName = teacher.School?.Name;

        // Buscar el ID del rol "Student" para filtrar
        var studentRoleId = await dbContext.Roles
            .Where(r => r.Name == Roles.Student)
            .Select(r => r.Id)
            .FirstOrDefaultAsync(cancellationToken);

        // 2. Buscar a los usuarios de esa institución que tengan el rol Student
        var students = await dbContext.Users
            .AsNoTracking()
            .Where(u => u.SchoolId == teacher.SchoolId)
            .Where(u => dbContext.UserRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == studentRoleId))
            .Select(u => new
            {
                u.Id,
                u.FirstName,
                u.LastName,
                u.Email,
                TotalAnswered = dbContext.PracticeSessions
                    .Where(ps => ps.StudentUserId == u.Id)
                    .SelectMany(ps => ps.Answers)
                    .Count(a => a.SelectedOptionId != null),
                CorrectAnswers = dbContext.PracticeSessions
                    .Where(ps => ps.StudentUserId == u.Id)
                    .SelectMany(ps => ps.Answers)
                    .Count(a => a.IsCorrect)
            })
            .ToListAsync(cancellationToken);

        // 3. Calcular porcentajes
        response.Students = students.Select(s => new StudentReportDto
        {
            StudentId = s.Id,
            FirstName = s.FirstName,
            LastName = s.LastName,
            Email = s.Email ?? string.Empty,
            TotalAnswered = s.TotalAnswered,
            CorrectAnswers = s.CorrectAnswers,
            ScorePercentage = s.TotalAnswered == 0
                ? 0
                : Math.Round((double)s.CorrectAnswers / s.TotalAnswered * 100, 1)
        })
        .OrderByDescending(r => r.ScorePercentage)
        .ToList();

        return response;
    }
}