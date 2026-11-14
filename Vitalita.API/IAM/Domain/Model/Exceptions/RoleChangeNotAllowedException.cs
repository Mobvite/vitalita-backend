namespace Vitalita.API.IAM.Domain.Model.Exceptions;

/// <summary>
///     Thrown when a user tries to change their own role.
/// </summary>
public sealed class RoleChangeNotAllowedException() : InvalidOperationException("A user cannot change their own role.");
