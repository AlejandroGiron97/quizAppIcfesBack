using IcfesApp.Application.Auth.Dtos;
using Microsoft.AspNetCore.Identity;

namespace IcfesApp.Infrastructure.Identity;

/// <summary>Helper methods for ASP.NET Identity types, shared by the authentication services.</summary>
public static class IdentityExtensions
{
    /// <summary>Error messages of an Identity operation (create user, change password...).</summary>
    public static IEnumerable<string> ErrorDescriptions(this IdentityResult identityResult) =>
        identityResult.Errors.Select(identityError => identityError.Description);

    /// <summary>Public user data sent to the frontend.</summary>
    public static UserInfoDto ToUserInfoDto(this ApplicationUser user, IEnumerable<string> roles) => new()
    {
        Id = user.Id,
        Email = user.Email!,
        FirstName = user.FirstName,
        LastName = user.LastName,
        Roles = roles.ToList(),
        Type = user.Type.ToString(),
        InstitutionId = user.InstitutionId
    };
}
