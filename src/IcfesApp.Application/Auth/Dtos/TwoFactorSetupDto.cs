namespace IcfesApp.Application.Auth.Dtos;

public class TwoFactorSetupDto
{
    public required string SharedKey { get; set; }
    public required string AuthenticatorUri { get; set; }
}
