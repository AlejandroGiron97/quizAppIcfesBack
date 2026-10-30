using IcfesApp.Application.Common.Interfaces;
using IcfesApp.Application.Common.Models;
using IcfesApp.Application.Practice.Dtos;
using IcfesApp.Domain.Constants;
using IcfesApp.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace IcfesApp.Infrastructure.Content.Proxies;

/// <summary>
/// Protection proxy (Proxy pattern) in front of the real <see cref="PracticeSessionService"/>.
///
/// It implements the same <see cref="IPracticeSessionService"/> interface, so controllers do not know it exists.
/// Before starting a practice session it checks that the student has an active subscription;
/// every other operation is forwarded untouched (a student whose subscription expires can still
/// finish a session in progress and review their history).
///
/// Rules:
///   - Only enforced when <see cref="PracticeAccessSettings.RequireActiveSubscription"/> is true.
///   - Teachers and admins are never blocked (they practice to test content, not as customers).
///
/// Keeping this check here, instead of inside PracticeSessionService, means the practice logic does not
/// know anything about subscriptions and the rule can be turned on/off in one place.
/// </summary>
public class SubscriptionCheckingPracticeSessionService(
    IPracticeSessionService realPracticeSessionService,
    ISubscriptionService subscriptionService,
    UserManager<ApplicationUser> userManager,
    IOptionsMonitor<PracticeAccessSettings> practiceAccessOptions) : IPracticeSessionService
{
    private const string SubscriptionRequiredMessage = "Necesitas una suscripción activa para iniciar una práctica.";

    public async Task<Result<PracticeSessionDto>> StartAsync(StartPracticeSessionRequest request, Guid studentUserId, CancellationToken cancellationToken = default)
    {
        if (!await CanStartPracticeAsync(studentUserId, cancellationToken))
        {
            return Result<PracticeSessionDto>.Failed([SubscriptionRequiredMessage], ErrorType.Forbidden);
        }

        return await realPracticeSessionService.StartAsync(request, studentUserId, cancellationToken);
    }

    public Task<IReadOnlyList<PracticeSessionSummaryDto>> GetHistoryAsync(Guid studentUserId, CancellationToken cancellationToken = default) =>
        realPracticeSessionService.GetHistoryAsync(studentUserId, cancellationToken);

    public Task<Result<AnswerResultDto>> SubmitAnswerAsync(Guid sessionId, SubmitAnswerRequest request, Guid studentUserId, CancellationToken cancellationToken = default) =>
        realPracticeSessionService.SubmitAnswerAsync(sessionId, request, studentUserId, cancellationToken);

    public Task<Result<PracticeSessionResultDto>> FinishAsync(Guid sessionId, Guid studentUserId, CancellationToken cancellationToken = default) =>
        realPracticeSessionService.FinishAsync(sessionId, studentUserId, cancellationToken);

    private async Task<bool> CanStartPracticeAsync(Guid userId, CancellationToken cancellationToken)
    {
        if (!practiceAccessOptions.CurrentValue.RequireActiveSubscription)
        {
            return true;
        }

        if (await IsStaffAsync(userId))
        {
            return true;
        }

        return await subscriptionService.HasActiveAccessAsync(userId, cancellationToken);
    }

    private async Task<bool> IsStaffAsync(Guid userId)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return false;
        }

        var roles = await userManager.GetRolesAsync(user);
        return roles.Contains(Roles.Admin) || roles.Contains(Roles.Teacher);
    }
}
