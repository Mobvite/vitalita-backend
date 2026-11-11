namespace Vitalita.API.IAM.Application.Errors;

/// <summary>
///     Errors that can occur when an administrator creates a user.
/// </summary>
public enum CreateUserError
{
    /// <summary>
    ///     The email is already registered.
    /// </summary>
    DuplicateEmail,

    /// <summary>
    ///     An unexpected error occurred while saving the user.
    /// </summary>
    UnexpectedError
}
