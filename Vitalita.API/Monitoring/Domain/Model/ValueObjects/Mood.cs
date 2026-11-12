namespace Vitalita.API.Monitoring.Domain.Model.ValueObjects;

/// <summary>
///     Mood of the older adult recorded in a daily report. Values match the frontend enum.
/// </summary>
public enum Mood
{
    /// <summary>Very good.</summary>
    VeryGood,

    /// <summary>Good.</summary>
    Good,

    /// <summary>Neutral.</summary>
    Neutral,

    /// <summary>Bad.</summary>
    Bad,

    /// <summary>Very bad.</summary>
    VeryBad
}
