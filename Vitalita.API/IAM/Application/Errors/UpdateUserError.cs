namespace Vitalita.API.IAM.Application.Errors;

/// <summary>
///     Errors that can occur when a user is updated.
/// </summary>
public enum UpdateUserError
{
    /// <summary>
    ///     The user does not exist.
    /// </summary>
    NotFound,

    /// <summary>
    ///     The requester is not an administrator and tries to update another user.
    /// </summary>
    Forbidden,

    /// <summary>
    ///     The requester tries to change their own role.
    /// </summary>
    RoleChangeNotAllowed,

    /// <summary>
    ///     The requester tries to change their own status.
    /// </summary>
    StatusChangeNotAllowed,

    /// <summary>
    ///     The new email belongs to another user.
    /// </summary>
    DuplicateEmail,

    /// <summary>
    ///     An unexpected error occurred while saving the user.
    /// </summary>
    UnexpectedError
}
