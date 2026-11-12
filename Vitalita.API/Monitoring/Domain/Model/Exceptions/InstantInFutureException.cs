namespace Vitalita.API.Monitoring.Domain.Model.Exceptions;

/// <summary>
///     Thrown when a report or measurement instant is later than now plus the allowed clock skew.
/// </summary>
public class InstantInFutureException() : InvalidOperationException("The instant cannot be in the future.");
