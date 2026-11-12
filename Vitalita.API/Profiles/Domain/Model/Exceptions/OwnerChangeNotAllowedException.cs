namespace Vitalita.API.Profiles.Domain.Model.Exceptions;

/// <summary>
///     Thrown when an update tries to move a profile to another user, or an older adult to another caregiver.
/// </summary>
public class OwnerChangeNotAllowedException() : InvalidOperationException("The owner of a profile or older adult cannot be changed.");
