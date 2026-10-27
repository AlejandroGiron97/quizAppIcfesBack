using IcfesApp.Application.Reports.Dtos;

namespace IcfesApp.Application.Common.Interfaces;

public interface IReportService
{
    Task<ReportResponseDto> GetStudentsReportAsync(Guid teacherId, CancellationToken cancellationToken = default);
}