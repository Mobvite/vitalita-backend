using Vitalita.API.Profiles.Domain.Model.Aggregates;
using Vitalita.API.Profiles.Domain.Model.ValueObjects;
using Vitalita.API.Profiles.Interfaces.REST.Resources;

namespace Vitalita.API.Profiles.Interfaces.REST.Transform;

/// <summary>
///     Converts a <see cref="SaveOlderAdultResource" /> into the <see cref="OlderAdultData" /> shared by the create
///     and update commands.
/// </summary>
public static class OlderAdultDataFromResourceAssembler
{
    /// <summary>
    ///     Converts the validated resource into the older adult data.
    /// </summary>
    /// <param name="resource">The validated resource.</param>
    /// <param name="defaultStatus">Status used when the resource has none.</param>
    /// <returns>The data.</returns>
    public static OlderAdultData ToDataFromResource(SaveOlderAdultResource resource, OlderAdultStatus defaultStatus)
    {
        var contact = resource.EmergencyContact;
        return new OlderAdultData(
            resource.FirstName,
            resource.LastName,
            resource.BirthDate!.Value,
            resource.DocumentNumber,
            resource.BloodType,
            resource.Allergies ?? [],
            resource.ChronicConditions ?? [],
            contact is null
                ? EmergencyContact.Empty
                : EmergencyContact.Create(contact.Name, contact.PhoneNumber, ParseRelationship(contact.Relationship)),
            resource.Observations,
            Enum.TryParse<OlderAdultStatus>(resource.Status, out var status) ? status : defaultStatus);
    }

    private static FamilyRelationship? ParseRelationship(string? value) =>
        Enum.TryParse<FamilyRelationship>(value, out var relationship) ? relationship : null;
}
