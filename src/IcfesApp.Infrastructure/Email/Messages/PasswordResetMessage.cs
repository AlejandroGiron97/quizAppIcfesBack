namespace IcfesApp.Infrastructure.Email.Messages;

/// <summary>Email with the link to create a new password ("forgot my password").</summary>
public sealed class PasswordResetMessage(string recipientFirstName, string resetPasswordUrl, string email, string resetToken)
    : AuthEmailMessage(recipientFirstName, BuildActionLink(resetPasswordUrl, email, resetToken))
{
    public override string Subject => "Recupera tu contraseña - IcfesApp";

    protected override string ExplanationText =>
        "Recibimos una solicitud para restablecer tu contraseña en IcfesApp.";

    protected override string ActionLinkText => "Haz clic aquí para crear una nueva contraseña";

    protected override string IgnoreNoticeText => "Si no fuiste tú quien lo solicitó, puedes ignorar este correo.";
}
