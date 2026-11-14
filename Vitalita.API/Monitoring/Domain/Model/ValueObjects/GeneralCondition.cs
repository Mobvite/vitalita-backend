namespace Vitalita.API.Monitoring.Domain.Model.ValueObjects;

/// <summary>
///     General condition of the older adult recorded in a daily report. Values match the frontend.
/// </summary>
public enum GeneralCondition
{
    /// <summary>Stable.</summary>
    Stable,

    /// <summary>Needs attention.</summary>
    Attention,

    /// <summary>Critical.</summary>
    Critical
}
