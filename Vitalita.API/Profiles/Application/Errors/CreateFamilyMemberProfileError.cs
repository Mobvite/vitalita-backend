namespace Vitalita.API.Profiles.Application.Errors;

/// <summary>
///     Errors that can occur when a family member profile is created.
/// </summary>
public enum CreateFamilyMemberProfileError
{
    /// <summary>
    ///     The requester is not an administrator and is not a family member creating their own profile.
    /// </summary>
    Forbidden,

    /// <summary>
    ///     The requester's email has no pending, unexpired invitation.
    /// </summary>
    NoValidInvitation,

    /// <summary>
    ///     The IAM user does not exist.
    /// </summary>
    UserNotFound,

    /// <summary>
    ///     The user already has a family member profile.
    /// </summary>
    DuplicateProfile,

    /// <summary>
    ///     An unexpected error occurred while saving the profile.
    /// </summary>
    UnexpectedError
}
