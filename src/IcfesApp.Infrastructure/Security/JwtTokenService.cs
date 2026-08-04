using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using IcfesApp.Application.Common.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace IcfesApp.Infrastructure.Security;

public class JwtTokenService(IOptions<JwtSettings> options) : IJwtTokenService
{
    private readonly JwtSettings _settings = options.Value;

    public (string Token, DateTime ExpiresAtUtc) CreateAccessToken(Guid userId, string email, IEnumerable<string> roles)
    {
        var expiresAtUtc = DateTime.UtcNow.AddMinutes(_settings.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(ClaimTypes.NameIdentifier, userId.ToString()),
        };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

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
}
