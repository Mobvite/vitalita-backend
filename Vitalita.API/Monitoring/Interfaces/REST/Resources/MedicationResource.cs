namespace Vitalita.API.Monitoring.Interfaces.REST.Resources;

/// <summary>
///     Medication of an older adult.
/// </summary>
/// <param name="Id">Medication identifier.</param>
/// <param name="OlderAdultId">Older adult that takes it.</param>
/// <param name="Name">Medicine name.</param>
/// <param name="Dosage">Dosage.</param>
/// <param name="Frequency">Frequency (may be empty).</param>
/// <param name="ScheduleTimes">Daily schedule times (<c>HH:mm</c>, sorted).</param>
/// <param name="Instructions">Instructions (may be empty).</param>
/// <param name="Active">Whether the medication is still taken.</param>
public record MedicationResource(
    int Id,
    int OlderAdultId,
    string Name,
    string Dosage,
    string Frequency,
    IReadOnlyList<string> ScheduleTimes,
    string Instructions,
    bool Active);
