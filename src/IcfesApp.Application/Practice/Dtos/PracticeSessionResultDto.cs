namespace IcfesApp.Application.Practice.Dtos;

public class PracticeSessionResultDto
{
    public Guid Id { get; set; }
    public int TotalQuestions { get; set; }
    public int CorrectAnswers { get; set; }
    public double ScorePercentage { get; set; }
    public List<AnswerReviewDto> Answers { get; set; } = [];
}
