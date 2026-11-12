namespace Vitalita.API.Monitoring.Application.Errors;

/// <summary>
///     Errors that can occur when a vital sign is created or updated.
/// </summary>
public enum VitalSignCommandError
{
    /// <summary>
    ///     The record does not exist (update only).
    /// </summary>
    NotFound,

    /// <summary>
    ///     The requester cannot write the records of the older adult (also when it does not exist, for non-administrators).
    /// </summary>
    Forbidden,

    /// <summary>
    ///     The older adult does not exist (administrators only; others get Forbidden).
    /// </summary>
    OlderAdultNotFound,

    /// <summary>
    ///     The update tries to move the record to another older adult.
    /// </summary>
    OlderAdultChangeNotAllowed,

    /// <summary>
    ///     The value is outside the range of its type.
    /// </summary>
    ValueOutOfRange,

    /// <summary>
    ///     A blood pressure has no diastolic value.
    /// </summary>
    SecondaryValueRequired,

    /// <summary>
    ///     The diastolic value is outside its range.
    /// </summary>
    SecondaryValueOutOfRange,

    /// <summary>
    ///     A type other than blood pressure has a secondary value.
    /// </summary>
    SecondaryValueNotAllowed,

    /// <summary>
    ///     The systolic value is not greater than the diastolic value.
    /// </summary>
    SystolicNotGreaterThanDiastolic,

    /// <summary>
    ///     The measurement instant is later than now plus the allowed clock skew.
    /// </summary>
    InstantInFuture,

    /// <summary>
    ///     An unexpected error occurred while saving the measurement.
    /// </summary>
    UnexpectedError
}
