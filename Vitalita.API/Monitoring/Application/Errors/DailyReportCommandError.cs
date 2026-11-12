namespace Vitalita.API.Monitoring.Application.Errors;

/// <summary>
///     Errors that can occur when a daily report is created or updated.
/// </summary>
public enum DailyReportCommandError
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
    ///     The report instant is later than now plus the allowed clock skew.
    /// </summary>
    InstantInFuture,

    /// <summary>
    ///     An unexpected error occurred while saving the report.
    /// </summary>
    UnexpectedError
}
