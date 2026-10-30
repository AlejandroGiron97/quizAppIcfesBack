namespace IcfesApp.Infrastructure.Email.Messages;

/// <summary>Email sent on sign-up to confirm the email address.</summary>
public sealed class EmailConfirmationMessage(string recipientFirstName, string confirmEmailUrl, string email, string confirmationToken)
    : AuthEmailMessage(recipientFirstName, BuildActionLink(confirmEmailUrl, email, confirmationToken))
{
    public override string Subject => "Confirma tu correo - IcfesApp";

    protected override string ExplanationText =>
        "Gracias por registrarte en IcfesApp. Confirma tu correo para activar tu cuenta:";

    protected override string ActionLinkText => "Confirmar mi correo";

    protected override string IgnoreNoticeText => "Si no fuiste tú quien se registró, puedes ignorar este correo.";
}
