namespace IcfesApp.Application.Reports.Dtos;

public class ReportResponseDto
{
    public string? InstitutionName { get; set; }
    public IReadOnlyList<StudentReportDto> Students { get; set; } = [];
}