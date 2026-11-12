namespace Vitalita.API.Profiles.Application.Errors;

/// <summary>
///     Errors that can occur when an older adult is registered.
/// </summary>
public enum CreateOlderAdultError
{
    /// <summary>
    ///     The requester is not an administrator and the caregiver profile is not their own.
    /// </summary>
    Forbidden,

    /// <summary>
    ///     The caregiver profile does not exist (administrators only; others get Forbidden).
    /// </summary>
    CaregiverNotFound,

    /// <summary>
    ///     The birth date is later than today in Peru.
    /// </summary>
    BirthDateInFuture,

    /// <summary>
    ///     An unexpected error occurred while saving the older adult.
    /// </summary>
    UnexpectedError
}
