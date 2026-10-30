using IcfesApp.Application.Common.Models;
using IcfesApp.Application.Practice.Dtos;

namespace IcfesApp.Application.Common.Interfaces;

public interface IPracticeSessionService
{
    /// <summary>Starts a session with random questions. Returns the full session (questions and options, never the answers).</summary>
    Task<Result<PracticeSessionDto>> StartAsync(StartPracticeSessionRequest request, Guid studentUserId, CancellationToken cancellationToken = default);

    /// <summary>The student's sessions as lightweight summaries (counts only, no questions), newest first.</summary>
    Task<IReadOnlyList<PracticeSessionSummaryDto>> GetHistoryAsync(Guid studentUserId, CancellationToken cancellationToken = default);

    Task<Result<AnswerResultDto>> SubmitAnswerAsync(Guid sessionId, SubmitAnswerRequest request, Guid studentUserId, CancellationToken cancellationToken = default);

    Task<Result<PracticeSessionResultDto>> FinishAsync(Guid sessionId, Guid studentUserId, CancellationToken cancellationToken = default);
}
