using System.ComponentModel.DataAnnotations;

namespace IcfesApp.Application.Practice.Dtos;

public class StartPracticeSessionRequest
{
    [Required]
    public Guid SubjectId { get; set; }

    /// <summary>Si no se indica, la sesión incluye todas las preguntas de la materia.</summary>
    public int? QuestionCount { get; set; }
}
