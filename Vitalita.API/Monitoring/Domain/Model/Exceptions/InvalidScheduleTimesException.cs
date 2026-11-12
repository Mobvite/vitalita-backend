namespace Vitalita.API.Monitoring.Domain.Model.Exceptions;

/// <summary>
///     Thrown when the schedule times of a medication are missing, too many, or not <c>HH:mm</c>.
/// </summary>
/// <param name="message">Description of the problem.</param>
public class InvalidScheduleTimesException(string message) : ArgumentException(message);
