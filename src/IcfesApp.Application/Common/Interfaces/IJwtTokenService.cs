namespace IcfesApp.Application.Common.Interfaces;

public interface IJwtTokenService
{
    (string Token, DateTime ExpiresAtUtc) CreateAccessToken(Guid userId, string email, IEnumerable<string> roles);
    string CreateRefreshToken();
    string CreateTwoFactorChallengeToken(Guid userId);
    Guid? ValidateTwoFactorChallengeToken(string token);
}
