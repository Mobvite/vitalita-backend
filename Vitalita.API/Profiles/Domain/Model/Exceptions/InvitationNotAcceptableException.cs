namespace Vitalita.API.Profiles.Domain.Model.Exceptions;

/// <summary>
///     Thrown when an invitation that is not pending, or already expired, is accepted.
/// </summary>
public class InvitationNotAcceptableException() : InvalidOperationException("The invitation is not pending or has expired.");
