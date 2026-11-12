namespace Vitalita.API.IAM.Application.Errors;

/// <summary>
///     Errors that can occur when an access token is refreshed.
/// </summary>
public enum RefreshTokenError
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
