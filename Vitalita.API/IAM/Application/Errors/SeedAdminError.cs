namespace Vitalita.API.IAM.Application.Errors;

/// <summary>
///     Reasons why the first administrator was not created.
/// </summary>
public enum SeedAdminError
{
    /// <summary>
    ///     A user with the configured email already exists; it is left unchanged.
    /// </summary>
    EmailAlreadyRegistered
}
