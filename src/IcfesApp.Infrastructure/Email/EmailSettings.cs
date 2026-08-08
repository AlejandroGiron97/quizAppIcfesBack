namespace IcfesApp.Infrastructure.Email;

public class EmailSettings
{
    public required string SmtpHost { get; set; }
    public int SmtpPort { get; set; } = 587;
    public required string Username { get; set; }
    public required string Password { get; set; }
    public required string FromEmail { get; set; }
    public string FromName { get; set; } = "IcfesApp";

    /// <summary>URL de la pantalla del front donde el usuario define su nueva contraseña.</summary>
    public string ResetPasswordUrlTemplate { get; set; } = "http://localhost:4200/reset-password";

    /// <summary>URL de la pantalla del front que confirma el email (lee ?email=&token= y llama a /api/auth/confirm-email).</summary>
    public string ConfirmEmailUrlTemplate { get; set; } = "http://localhost:4200/confirm-email";
}
