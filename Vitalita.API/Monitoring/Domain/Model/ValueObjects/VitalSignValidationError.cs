namespace Vitalita.API.Monitoring.Domain.Model.ValueObjects;

/// <summary>
///     Reasons why a vital sign measurement is not valid.
/// </summary>
public enum VitalSignValidationError
{
    /// <summary>The value is outside the range of its type.</summary>
    ValueOutOfRange,

    /// <summary>A blood pressure has no diastolic value.</summary>
    SecondaryValueRequired,

    /// <summary>The diastolic value is outside its range.</summary>
    SecondaryValueOutOfRange,

    /// <summary>A type other than blood pressure has a secondary value.</summary>
    SecondaryValueNotAllowed,

    /// <summary>The systolic value is not greater than the diastolic value.</summary>
    SystolicNotGreaterThanDiastolic
}
