using System.Net;

namespace IcfesApp.Infrastructure.Email.Messages;

/// <summary>
/// Base class for authentication emails (Template Method pattern).
///
/// Every one of these emails has the same structure: greeting, explanation, action link and
/// an "if it wasn't you, ignore it" notice. <see cref="BuildHtmlBody"/> builds that structure and each subclass
/// only defines its texts. For a new email: create a subclass and override the properties.
/// </summary>
public abstract class AuthEmailMessage(string recipientFirstName, string actionLink)
{
    public abstract string Subject { get; }

    protected abstract string ExplanationText { get; }

    protected abstract string ActionLinkText { get; }

    protected abstract string IgnoreNoticeText { get; }

    /// <summary>The "template method": the order of the parts is fixed, each subclass provides the content.</summary>
    public string BuildHtmlBody() => $"""
        <p>Hola {WebUtility.HtmlEncode(recipientFirstName)},</p>
        <p>{ExplanationText}</p>
        <p><a href="{actionLink}">{ActionLinkText}</a></p>
        <p>{IgnoreNoticeText}</p>
        """;

    /// <summary>Builds the frontend link with the email and the token as URL parameters.</summary>
    protected static string BuildActionLink(string frontendUrl, string email, string token) =>
        $"{frontendUrl}?email={WebUtility.UrlEncode(email)}&token={WebUtility.UrlEncode(token)}";
}
