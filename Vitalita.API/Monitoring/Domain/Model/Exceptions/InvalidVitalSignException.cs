using Vitalita.API.Monitoring.Domain.Model.ValueObjects;

namespace Vitalita.API.Monitoring.Domain.Model.Exceptions;

/// <summary>
///     Thrown when a vital sign measurement breaks the rules of its type.
/// </summary>
/// <param name="error">The reason.</param>
public class InvalidVitalSignException(VitalSignValidationError error)
    : ArgumentException($"The vital sign is not valid: {error}.")
{
    /// <summary>
    ///     Gets the reason why the measurement is not valid.
    /// </summary>
    public VitalSignValidationError Error { get; } = error;
}
