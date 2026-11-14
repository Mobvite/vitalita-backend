namespace Vitalita.API.IAM.Application.Errors;

/// <summary>
///     Errors that can occur when a user is deactivated.
/// </summary>
public enum DeactivateUserError
{
    /// <summary>
    ///     The user does not exist.
    /// </summary>
    NotFound,

    /// <summary>
    ///     The administrator tries to deactivate their own account.
    /// </summary>
    CannotDeactivateSelf
}
