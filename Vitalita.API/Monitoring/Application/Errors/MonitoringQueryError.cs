namespace Vitalita.API.Monitoring.Application.Errors;

/// <summary>
///     Errors that can occur when a Monitoring query is handled.
/// </summary>
public enum MonitoringQueryError
{
    /// <summary>
    ///     The record does not exist.
    /// </summary>
    NotFound,

    /// <summary>
    ///     The requester cannot read the records of the older adult, or the filter points to an older adult they cannot read.
    /// </summary>
    Forbidden
}
