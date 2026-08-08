using System.ComponentModel.DataAnnotations;

namespace IcfesApp.Application.Practice.Dtos;

public class SubmitAnswerRequest
{
    [Required]
    public Guid QuestionId { get; set; }

    [Required]
    public Guid SelectedOptionId { get; set; }
}
