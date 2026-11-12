namespace Vitalita.API.IAM.Domain.Model.ValueObjects;

/// <summary>
///     Status of a Vitalita account. Serialized as strings (<c>"Active"</c>, <c>"Inactive"</c>).
/// </summary>
public enum UserStatus
{
    /// <summary>
    ///     The account can sign in.
    /// </summary>
    Active,

    /// <summary>
    ///     The account was deactivated (logical delete) and cannot sign in.
    /// </summary>
    Inactive
}
