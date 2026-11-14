namespace Vitalita.API.IAM.Application.Errors;

/// <summary>
///     Errors that can occur when the user of a token is read.
/// </summary>
public enum CurrentUserError
{
    /// <summary>
    ///     The user of the token does not exist anymore.
    /// </summary>
    UserNotFound,

    /// <summary>
    ///     The user of the token was deactivated.
    /// </summary>
    InactiveAccount
}
