using System.ComponentModel.DataAnnotations;

namespace IcfesApp.Application.Auth.Dtos;

public class GoogleLoginRequest
{
    /// <summary>El ID token que devuelve Google Identity Services en el front, no un access token.</summary>
    [Required]
    public required string IdToken { get; set; }
}
