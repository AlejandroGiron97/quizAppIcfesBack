using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using IcfesApp.Application.Common.Interfaces;
using IcfesApp.Domain.Enums;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace IcfesApp.Infrastructure.Security;

public class JwtTokenService(IOptions<JwtSettings> options) : IJwtTokenService
{
    private readonly JwtSettings _settings = options.Value;

    public (string Token, DateTime ExpiresAtUtc) CreateAccessToken(
        Guid userId,
        string email,
        IEnumerable<string> roles,
        UserType? type = null)
    {
        var expiresAtUtc = DateTime.UtcNow.AddMinutes(_settings.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Email, email),
        };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        // User type as a claim so /session can return it without querying the database.
        if (type.HasValue)
        {
            claims.Add(new Claim("user_type", type.Value.ToString()));
        }

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAtUtc);
    }

    public string CreateRefreshToken()
    {
        var randomBytes = new byte[64];
        RandomNumberGenerator.Fill(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    private const string TwoFactorPurposeClaim = "purpose";
    private const string TwoFactorPurposeValue = "2fa_challenge";

    public string CreateTwoFactorChallengeToken(Guid userId)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(TwoFactorPurposeClaim, TwoFactorPurposeValue),
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(5),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public Guid? ValidateTwoFactorChallengeToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));

        try
        {
            var principal = handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _settings.Issuer,
                ValidateAudience = true,
                ValidAudience = _settings.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out _);

            var tokenPurpose = principal.Claims.FirstOrDefault(claim => claim.Type == TwoFactorPurposeClaim)?.Value;
            if (tokenPurpose != TwoFactorPurposeValue)
            {
                return null;
            }

            var userIdClaimValue = principal.Claims
                .FirstOrDefault(claim => claim.Type == JwtRegisteredClaimNames.Sub || claim.Type == ClaimTypes.NameIdentifier)?.Value;

            return userIdClaimValue is not null && Guid.TryParse(userIdClaimValue, out var userId) ? userId : null;
        }
        // A malformed token (e.g. "abc") throws SecurityTokenMalformedException, which derives from
        // ArgumentException and not from SecurityTokenException; it used to escape as a 500 error.
        catch (Exception exception) when (exception is SecurityTokenException or ArgumentException)
        {
            return null;
        }
    }
}