namespace IcfesApp.Infrastructure.Content.Proxies;

/// <summary>
/// "PracticeAccess" section of appsettings.json.
/// Read on every request through IOptionsMonitor, so it can be changed without restarting the API.
/// </summary>
public class PracticeAccessSettings
{
    public const string SectionName = "PracticeAccess";

    /// <summary>
    /// When true, students need an active subscription (their own or their institution's) to start a practice session.
    /// Off by default: there is no payment / free-trial flow yet, so new particular users would be locked out.
    /// </summary>
    public bool RequireActiveSubscription { get; set; }
}
