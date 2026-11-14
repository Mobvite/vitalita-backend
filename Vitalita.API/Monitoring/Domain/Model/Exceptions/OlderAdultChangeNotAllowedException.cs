namespace Vitalita.API.Monitoring.Domain.Model.Exceptions;

/// <summary>
///     Thrown when an update tries to move a record to another older adult.
/// </summary>
public class OlderAdultChangeNotAllowedException() : InvalidOperationException(
    "The older adult of a monitoring record cannot be changed.");
