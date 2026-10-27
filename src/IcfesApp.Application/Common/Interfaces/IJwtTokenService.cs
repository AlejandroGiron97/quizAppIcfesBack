using IcfesApp.Domain.Enums;

namespace IcfesApp.Application.Common.Interfaces;

public interface IJwtTokenService
{
    (string Token, DateTime ExpiresAtUtc) CreateAccessToken(
        Guid userId,
        string email,
        IEnumerable<string> roles,
        UserType? type = null);

    string CreateRefreshToken();
    string CreateTwoFactorChallengeToken(Guid userId);
    Guid? ValidateTwoFactorChallengeToken(string token);
}