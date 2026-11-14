namespace Vitalita.API.IAM.Application.Errors;

/// <summary>
///     Errors that can occur during the sign-in.
/// </summary>
public enum SignInError
{
    /// <summary>
    ///     The email does not exist or the password is wrong.
    /// </summary>
    InvalidCredentials,

    /// <summary>
    ///     The credentials are right, but the account is inactive.
    /// </summary>
    InactiveAccount
}
