namespace IcfesApp.Domain.Enums;

/// <summary>
/// Possible states of a subscription.
/// </summary>
public enum SubscriptionStatus
{
    /// <summary>Created but not activated yet (e.g. waiting for payment confirmation).</summary>
    Pending = 0,

    /// <summary>Active and valid. It is the only state that grants access to the product.</summary>
    Active = 1,

    /// <summary>Expired over time. Requires renewal.</summary>
    Expired = 2,

    /// <summary>Cancelled by the user or by an administrator before expiring.</summary>
    Cancelled = 3,

    /// <summary>Failed charge attempt (applies to automatic renewals).</summary>
    PastDue = 4
}