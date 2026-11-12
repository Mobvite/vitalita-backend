namespace Vitalita.API.Profiles.Domain.Model.Exceptions;

/// <summary>
///     Thrown when somebody other than the invited person tries to accept an invitation.
/// </summary>
public class InvitationEmailMismatchException() : InvalidOperationException("The invitation belongs to another email.");
