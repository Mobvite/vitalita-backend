namespace Vitalita.API.Profiles.Domain.Model.ValueObjects;

/// <summary>
///     Follow-up status of an older adult.
/// </summary>
public enum OlderAdultStatus
{
    /// <summary>The older adult is followed today.</summary>
    Active,

    /// <summary>The follow-up is paused.</summary>
    Inactive,

    /// <summary>The follow-up ended; the records are kept (there is no physical delete).</summary>
    Archived
}
