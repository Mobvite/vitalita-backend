using Vitalita.API.Profiles.Domain.Model.Exceptions;
using Vitalita.API.Profiles.Domain.Model.ValueObjects;
using Vitalita.API.Shared.Domain.Model;

namespace Vitalita.API.Profiles.Domain.Model.Aggregates;

/// <summary>
///     Older adult aggregate root. It is the center of the care follow-up.
/// </summary>
/// <remarks>
///     <list type="bullet">
///         <item><description>Belongs to one caregiver profile, which never changes.</description></item>
///         <item><description>The birth date is required and cannot be later than today in Peru.</description></item>
///         <item><description>There is no physical delete: the follow-up ends with <see cref="OlderAdultStatus.Archived" />.</description></item>
///     </list>
/// </remarks>
public class OlderAdult : IAuditableEntity
{
    /// <summary>
    ///     Protected parameterless constructor for EF Core.
    /// </summary>
    protected OlderAdult()
    {
        FirstName = LastName = DocumentNumber = BloodType = Observations = string.Empty;
        Allergies = ChronicConditions = [];
        EmergencyContact = EmergencyContact.Empty;
    }

    /// <summary>
    ///     Registers an older adult in the charge of a caregiver.
    /// </summary>
    /// <param name="caregiverId">Caregiver profile in charge.</param>
    /// <param name="data">The older adult data.</param>
    /// <param name="peruToday">Today in Peru, used to validate the birth date.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="caregiverId" /> is not positive.</exception>
    /// <exception cref="BirthDateInFutureException">Thrown when the birth date is later than <paramref name="peruToday" />.</exception>
    public OlderAdult(int caregiverId, OlderAdultData data, DateOnly peruToday) : this()
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(caregiverId);
        CaregiverId = caregiverId;
        SetData(data, peruToday);
    }

    /// <summary>Gets the server-generated identifier.</summary>
    public int Id { get; private set; }

    /// <summary>Gets the caregiver profile in charge.</summary>
    public int CaregiverId { get; private set; }

    /// <summary>Gets the first name.</summary>
    public string FirstName { get; private set; }

    /// <summary>Gets the last name.</summary>
    public string LastName { get; private set; }

    /// <summary>Gets the birth date.</summary>
    public DateOnly BirthDate { get; private set; }

    /// <summary>Gets the national ID (DNI), empty or 8 digits.</summary>
    public string DocumentNumber { get; private set; }

    /// <summary>Gets the blood type (see <see cref="BloodTypes" />), empty when unknown.</summary>
    public string BloodType { get; private set; }

    /// <summary>Gets the known allergies.</summary>
    public IReadOnlyList<string> Allergies { get; private set; }

    /// <summary>Gets the chronic conditions.</summary>
    public IReadOnlyList<string> ChronicConditions { get; private set; }

    /// <summary>Gets the person to call in an emergency.</summary>
    public EmergencyContact EmergencyContact { get; private set; }

    /// <summary>Gets the care observations.</summary>
    public string Observations { get; private set; }

    /// <summary>Gets the follow-up status.</summary>
    public OlderAdultStatus Status { get; private set; }

    /// <inheritdoc />
    public DateTime? CreatedAt { get; set; }

    /// <inheritdoc />
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    ///     Checks whether a caregiver profile is in charge of this older adult.
    /// </summary>
    /// <param name="caregiverProfileId">The caregiver profile identifier.</param>
    /// <returns><c>true</c> when it is the caregiver in charge.</returns>
    public bool IsInChargeOf(int caregiverProfileId) => CaregiverId == caregiverProfileId;

    /// <summary>
    ///     Replaces the older adult data.
    /// </summary>
    /// <param name="caregiverId">The caregiver sent by the client; it must be the current one.</param>
    /// <param name="data">The new data.</param>
    /// <param name="peruToday">Today in Peru, used to validate the birth date.</param>
    /// <exception cref="OwnerChangeNotAllowedException">Thrown when <paramref name="caregiverId" /> is another caregiver.</exception>
    /// <exception cref="BirthDateInFutureException">Thrown when the birth date is later than <paramref name="peruToday" />.</exception>
    public void Update(int caregiverId, OlderAdultData data, DateOnly peruToday)
    {
        if (caregiverId != CaregiverId) throw new OwnerChangeNotAllowedException();
        SetData(data, peruToday);
    }

    private void SetData(OlderAdultData data, DateOnly peruToday)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentException.ThrowIfNullOrWhiteSpace(data.FirstName);
        ArgumentException.ThrowIfNullOrWhiteSpace(data.LastName);
        if (data.BirthDate > peruToday) throw new BirthDateInFutureException();
        if (!BloodTypes.IsValid(data.BloodType))
            throw new ArgumentException("The blood type is not valid.", nameof(data));

        FirstName = TextNormalizer.Clean(data.FirstName);
        LastName = TextNormalizer.Clean(data.LastName);
        BirthDate = data.BirthDate;
        DocumentNumber = TextNormalizer.Clean(data.DocumentNumber);
        BloodType = TextNormalizer.Clean(data.BloodType).ToUpperInvariant();
        Allergies = TextNormalizer.CleanList(data.Allergies);
        ChronicConditions = TextNormalizer.CleanList(data.ChronicConditions);
        EmergencyContact = data.EmergencyContact ?? EmergencyContact.Empty;
        Observations = TextNormalizer.Clean(data.Observations);
        Status = data.Status;
    }
}

/// <summary>
///     The editable data of an older adult, shared by the create and update operations.
/// </summary>
/// <param name="FirstName">First name (required).</param>
/// <param name="LastName">Last name (required).</param>
/// <param name="BirthDate">Birth date (required, not in the future).</param>
/// <param name="DocumentNumber">National ID (DNI), empty or 8 digits.</param>
/// <param name="BloodType">Blood type, empty when unknown.</param>
/// <param name="Allergies">Known allergies.</param>
/// <param name="ChronicConditions">Chronic conditions.</param>
/// <param name="EmergencyContact">Person to call in an emergency.</param>
/// <param name="Observations">Care observations.</param>
/// <param name="Status">Follow-up status.</param>
public record OlderAdultData(
    string FirstName,
    string LastName,
    DateOnly BirthDate,
    string? DocumentNumber,
    string? BloodType,
    IReadOnlyList<string> Allergies,
    IReadOnlyList<string> ChronicConditions,
    EmergencyContact? EmergencyContact,
    string? Observations,
    OlderAdultStatus Status);
