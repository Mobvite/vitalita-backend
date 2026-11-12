namespace Vitalita.API.Profiles.Application.Errors;

/// <summary>
///     Errors that can occur when a caregiver profile is created.
/// </summary>
public enum CreateCaregiverProfileError
{
    /// <summary>
    ///     The requester is not an administrator and is not a caregiver creating their own profile.
    /// </summary>
    Forbidden,

    /// <summary>
    ///     The IAM user does not exist.
    /// </summary>
    UserNotFound,

    /// <summary>
    ///     The user already has a caregiver profile.
    /// </summary>
    DuplicateProfile,

    /// <summary>
    ///     An unexpected error occurred while saving the profile.
    /// </summary>
    UnexpectedError
}
