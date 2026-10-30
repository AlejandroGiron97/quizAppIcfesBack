using System.ComponentModel.DataAnnotations;

namespace IcfesApp.Application.Practice.Dtos;

public class StartPracticeSessionRequest
{
    [Required]
    public Guid SubjectId { get; set; }

    /// <summary>If not provided, the session includes every question of the subject.</summary>
    public int? QuestionCount { get; set; }
}
