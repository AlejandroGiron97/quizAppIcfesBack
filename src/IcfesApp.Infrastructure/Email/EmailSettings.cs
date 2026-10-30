namespace IcfesApp.Infrastructure.Email;

public class EmailSettings
{
    public required string SmtpHost { get; set; }
    public int SmtpPort { get; set; } = 587;
    public required string Username { get; set; }
    public required string Password { get; set; }
    public required string FromEmail { get; set; }
    public string FromName { get; set; } = "IcfesApp";

    /// <summary>URL of the frontend screen where the user sets their new password.</summary>
    public string ResetPasswordUrlTemplate { get; set; } = "http://localhost:4200/reset-password";

    /// <summary>URL of the frontend screen that confirms the email (reads ?email=&token= and calls /api/auth/confirm-email).</summary>
    public string ConfirmEmailUrlTemplate { get; set; } = "http://localhost:4200/confirm-email";
}
