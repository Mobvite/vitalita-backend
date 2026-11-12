using Vitalita.API.Monitoring.Domain.Model.Exceptions;
using Vitalita.API.Monitoring.Domain.Model.ValueObjects;
using Vitalita.API.Shared.Domain.Model;

namespace Vitalita.API.Monitoring.Domain.Model.Aggregates;

/// <summary>
///     Medication aggregate root: a medicine of an older adult with its daily schedule.
/// </summary>
/// <remarks>
///     <list type="bullet">
///         <item><description>Name, dosage and at least one schedule time are required.</description></item>
///         <item><description>Schedule times are wall-clock <c>HH:mm</c> values (not converted), sorted and without duplicates.</description></item>
///         <item><description>There is no delete: a medication is deactivated (<see cref="Active" /> = <c>false</c>).</description></item>
///         <item><description>The older adult of a medication never changes.</description></item>
///     </list>
/// </remarks>
public class Medication : IAuditableEntity
{
    /// <summary>
    ///     Protected parameterless constructor for EF Core.
    /// </summary>
    protected Medication()
    {
        Name = Dosage = Frequency = Instructions = string.Empty;
        ScheduleTimes = [];
    }

    /// <summary>
    ///     Registers a medication.
    /// </summary>
    /// <param name="olderAdultId">Older adult that takes it.</param>
    /// <param name="data">The medication data.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="olderAdultId" /> is not positive.</exception>
    /// <exception cref="ArgumentException">Thrown when a required value is missing or a time is not <c>HH:mm</c>.</exception>
    public Medication(int olderAdultId, MedicationData data) : this()
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(olderAdultId);
        OlderAdultId = olderAdultId;
        SetData(data);
    }

    /// <summary>Gets the server-generated identifier.</summary>
    public int Id { get; private set; }

    /// <summary>Gets the older adult that takes the medication.</summary>
    public int OlderAdultId { get; private set; }

    /// <summary>Gets the medicine name.</summary>
    public string Name { get; private set; }

    /// <summary>Gets the dosage, for example <c>50 mg</c>.</summary>
    public string Dosage { get; private set; }

    /// <summary>Gets the frequency, for example <c>Cada 12 horas</c> (may be empty).</summary>
    public string Frequency { get; private set; }

    /// <summary>Gets the daily schedule times (<c>HH:mm</c>, sorted).</summary>
    public IReadOnlyList<string> ScheduleTimes { get; private set; }

    /// <summary>Gets the instructions (may be empty).</summary>
    public string Instructions { get; private set; }

    /// <summary>Gets a value indicating whether the medication is still taken.</summary>
    public bool Active { get; private set; }

    /// <inheritdoc />
    public DateTime? CreatedAt { get; set; }

    /// <inheritdoc />
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    ///     Replaces the medication data, including <see cref="Active" />.
    /// </summary>
    /// <param name="olderAdultId">The older adult sent by the client; it must be the current one.</param>
    /// <param name="data">The new data.</param>
    /// <exception cref="OlderAdultChangeNotAllowedException">Thrown when the older adult is another one.</exception>
    public void Update(int olderAdultId, MedicationData data)
    {
        if (olderAdultId != OlderAdultId) throw new OlderAdultChangeNotAllowedException();
        SetData(data);
    }

    private void SetData(MedicationData data)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentException.ThrowIfNullOrWhiteSpace(data.Name);
        ArgumentException.ThrowIfNullOrWhiteSpace(data.Dosage);
        ScheduleTimes = ValueObjects.ScheduleTimes.Normalize(data.ScheduleTimes);
        Name = data.Name.Trim();
        Dosage = data.Dosage.Trim();
        Frequency = data.Frequency?.Trim() ?? string.Empty;
        Instructions = data.Instructions?.Trim() ?? string.Empty;
        Active = data.Active;
    }
}

/// <summary>
///     The editable data of a medication, shared by the create and update operations.
/// </summary>
/// <param name="Name">Medicine name (required).</param>
/// <param name="Dosage">Dosage (required).</param>
/// <param name="Frequency">Frequency.</param>
/// <param name="ScheduleTimes">Daily schedule times (<c>HH:mm</c>, at least one).</param>
/// <param name="Instructions">Instructions.</param>
/// <param name="Active">Whether the medication is still taken.</param>
public record MedicationData(
    string Name,
    string Dosage,
    string? Frequency,
    IReadOnlyList<string> ScheduleTimes,
    string? Instructions,
    bool Active);
