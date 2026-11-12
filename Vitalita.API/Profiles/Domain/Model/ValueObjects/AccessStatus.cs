namespace Vitalita.API.Profiles.Domain.Model.ValueObjects;

/// <summary>
///     Status of a family access.
/// </summary>
public enum AccessStatus
{
    /// <summary>The invitation was sent and has not been accepted yet.</summary>
    Pending,

    /// <summary>The family member accepted the invitation and can read the follow-up.</summary>
    Active,

    /// <summary>The caregiver removed the access. It is a final status.</summary>
    Revoked
}
