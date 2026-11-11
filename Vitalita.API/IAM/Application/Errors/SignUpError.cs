namespace Vitalita.API.IAM.Application.Errors;

/// <summary>
///     Errors that can occur during the public sign-up.
/// </summary>
public enum SignUpError
{
    /// <summary>
    ///     The public sign-up cannot create administrators.
    /// </summary>
    AdminRoleNotAllowed,

    /// <summary>
    ///     The email is already registered.
    /// </summary>
    DuplicateEmail,

    /// <summary>
    ///     An unexpected error occurred while saving the user.
    /// </summary>
    UnexpectedError
}
