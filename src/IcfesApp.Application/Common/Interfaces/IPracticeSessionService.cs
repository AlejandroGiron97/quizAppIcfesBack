using IcfesApp.Application.Common.Models;
using IcfesApp.Application.Practice.Dtos;

namespace IcfesApp.Application.Common.Interfaces;

public interface IPracticeSessionService
{
    Task<Result<PracticeSessionDto>> StartAsync(StartPracticeSessionRequest request, Guid studentUserId, CancellationToken cancellationToken = default);
    Task<PracticeSessionDto?> GetByIdAsync(Guid id, Guid studentUserId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PracticeSessionDto>> GetMineAsync(Guid studentUserId, CancellationToken cancellationToken = default);
    Task<Result<AnswerResultDto>> SubmitAnswerAsync(Guid sessionId, SubmitAnswerRequest request, Guid studentUserId, CancellationToken cancellationToken = default);
    Task<Result<PracticeSessionResultDto>> FinishAsync(Guid sessionId, Guid studentUserId, CancellationToken cancellationToken = default);
}
