namespace IcfesApp.Application.Practice.Dtos;

/// <summary>Opción de una pregunta vista por el estudiante mientras responde: nunca expone cuál es la correcta.</summary>
public class PracticeQuestionOptionDto
{
    public Guid Id { get; set; }
    public required string Text { get; set; }
}
