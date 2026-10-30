namespace IcfesApp.Application.Practice.Dtos;

/// <summary>Question option as seen by the student while answering: it never reveals which one is correct.</summary>
public class PracticeQuestionOptionDto
{
    public Guid Id { get; set; }
    public required string Text { get; set; }
    public string? ImageUrl { get; set; }
}