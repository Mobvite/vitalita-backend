namespace Vitalita.API.Profiles.Application.Errors;

/// <summary>
///     Errors that can occur when a caregiver or family member profile is updated.
/// </summary>
public enum UpdateProfileError
{
    /// <summary>
    ///     The profile does not exist.
    /// </summary>
    NotFound,

    /// <summary>
    ///     The requester is not an administrator and does not own the profile.
    /// </summary>
    Forbidden,

    /// <summary>
    ///     The request tries to move the profile to another user.
    /// </summary>
    OwnerChangeNotAllowed,

    /// <summary>
    ///     An unexpected error occurred while saving the profile.
    /// </summary>
    UnexpectedError
}
