namespace IcfesApp.Application.Reports.Dtos;

public class StudentReportDto
{
    public Guid StudentId { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public int TotalAnswered { get; set; }
    public int CorrectAnswers { get; set; }
    public double ScorePercentage { get; set; }
}