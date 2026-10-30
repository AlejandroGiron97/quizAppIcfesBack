using IcfesApp.Domain.Common;

namespace IcfesApp.Domain.Entities;

/// <summary>
/// Una pregunta dentro de una sesión de práctica y la respuesta del estudiante.
/// Se crea sin respuesta al iniciar la sesión y se completa con <see cref="RegisterSelectedOption"/>.
/// </summary>
public class StudentAnswer : BaseEntity
{
    public Guid PracticeSessionId { get; set; }
    public PracticeSession PracticeSession { get; set; } = null!;

    public Guid QuestionId { get; set; }
    public Question Question { get; set; } = null!;

    /// <summary>Opción elegida. Null mientras la pregunta no se ha respondido.</summary>
    public Guid? SelectedOptionId { get; set; }
    public QuestionOption? SelectedOption { get; set; }

    public bool IsCorrect { get; set; }

    public DateTime AnsweredAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>No se guarda en BD.</summary>
    public bool IsAnswered => SelectedOptionId is not null;

    /// <summary>Guarda la opción elegida por el estudiante y si fue correcta.</summary>
    public void RegisterSelectedOption(QuestionOption selectedOption)
    {
        var answeredAtUtc = DateTime.UtcNow;

        SelectedOptionId = selectedOption.Id;
        IsCorrect = selectedOption.IsCorrect;
        AnsweredAtUtc = answeredAtUtc;
        UpdatedAtUtc = answeredAtUtc;
    }
}
