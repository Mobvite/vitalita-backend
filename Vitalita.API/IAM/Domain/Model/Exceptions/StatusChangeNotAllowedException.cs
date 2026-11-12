namespace Vitalita.API.IAM.Domain.Model.Exceptions;

/// <summary>
///     Thrown when a user tries to change their own status.
/// </summary>
public sealed class StatusChangeNotAllowedException()
    : InvalidOperationException("A user cannot change their own status.");
