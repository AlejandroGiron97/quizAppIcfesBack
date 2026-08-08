using IcfesApp.Application.Common.Models;
using IcfesApp.Application.Questions.Dtos;

namespace IcfesApp.Application.Common.Interfaces;

public interface IQuestionService
{
    Task<IReadOnlyList<QuestionDto>> GetAllAsync(Guid? subjectId, CancellationToken cancellationToken = default);
    Task<QuestionDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<QuestionDto>> CreateAsync(CreateQuestionRequest request, Guid createdByUserId, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(Guid id, UpdateQuestionRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
