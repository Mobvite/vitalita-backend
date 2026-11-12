namespace Vitalita.API.Profiles.Application.Errors;

/// <summary>
///     Errors that can occur when a family member is invited.
/// </summary>
public enum CreateFamilyAccessError
{
    /// <summary>
    ///     The requester is not an administrator and is not the caregiver in charge of the older adult.
    /// </summary>
    Forbidden,

    /// <summary>
    ///     The older adult does not exist (administrators only; others get Forbidden).
    /// </summary>
    OlderAdultNotFound,

    /// <summary>
    ///     The older adult already has a pending or active access for that email.
    /// </summary>
    DuplicateInvitation,

    /// <summary>
    ///     An unexpected error occurred while saving the invitation (including repeated code collisions).
    /// </summary>
    UnexpectedError
}
