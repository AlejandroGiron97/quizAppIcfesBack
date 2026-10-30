using System.ComponentModel.DataAnnotations;

namespace IcfesApp.Application.Auth.Dtos;

public class GoogleLoginRequest
{
    /// <summary>The ID token returned by Google Identity Services on the frontend, not an access token.</summary>
    [Required]
    public required string IdToken { get; set; }
}
