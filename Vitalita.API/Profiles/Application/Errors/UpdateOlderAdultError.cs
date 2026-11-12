namespace Vitalita.API.Profiles.Application.Errors;

/// <summary>
///     Errors that can occur when an older adult is updated.
/// </summary>
public enum UpdateOlderAdultError
{
    /// <summary>
    ///     The older adult does not exist.
    /// </summary>
    NotFound,

    /// <summary>
    ///     The requester is not an administrator and is not the caregiver in charge.
    /// </summary>
    Forbidden,

    /// <summary>
    ///     The request tries to move the older adult to another caregiver.
    /// </summary>
    OwnerChangeNotAllowed,

    /// <summary>
    ///     The birth date is later than today in Peru.
    /// </summary>
    BirthDateInFuture,

    /// <summary>
    ///     An unexpected error occurred while saving the older adult.
    /// </summary>
    UnexpectedError
}
