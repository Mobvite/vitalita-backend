using Vitalita.API.Profiles.Domain.Model.ValueObjects;

namespace Vitalita.API.Profiles.Interfaces.REST.Resources;

/// <summary>
///     Older adult.
/// </summary>
/// <param name="Id">Older adult identifier.</param>
/// <param name="CaregiverId">Caregiver profile in charge.</param>
/// <param name="FirstName">First name.</param>
/// <param name="LastName">Last name.</param>
/// <param name="BirthDate">Birth date (<c>yyyy-MM-dd</c>).</param>
/// <param name="DocumentNumber">National ID (DNI), empty or 8 digits.</param>
/// <param name="BloodType">Blood type, empty when unknown.</param>
/// <param name="Allergies">Known allergies.</param>
/// <param name="ChronicConditions">Chronic conditions.</param>
/// <param name="EmergencyContact">Person to call in an emergency.</param>
/// <param name="Observations">Care observations.</param>
/// <param name="Status">Active, Inactive or Archived.</param>
public record OlderAdultResource(
    int Id,
    int CaregiverId,
    string FirstName,
    string LastName,
    DateOnly BirthDate,
    string DocumentNumber,
    string BloodType,
    IReadOnlyList<string> Allergies,
    IReadOnlyList<string> ChronicConditions,
    EmergencyContactResource EmergencyContact,
    string Observations,
    OlderAdultStatus Status);
