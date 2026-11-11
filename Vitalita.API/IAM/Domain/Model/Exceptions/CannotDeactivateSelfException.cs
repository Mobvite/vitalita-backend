namespace Vitalita.API.IAM.Domain.Model.Exceptions;

/// <summary>
///     Thrown when an administrator tries to deactivate their own account.
/// </summary>
public sealed class CannotDeactivateSelfException()
    : InvalidOperationException("A user cannot deactivate their own account.");
