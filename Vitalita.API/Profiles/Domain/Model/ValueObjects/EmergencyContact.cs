namespace Vitalita.API.Profiles.Domain.Model.ValueObjects;

/// <summary>
///     Value object with the person to call in an emergency.
/// </summary>
/// <param name="Name">Full name (may be empty).</param>
/// <param name="PhoneNumber">Phone number (may be empty).</param>
/// <param name="Relationship">Relationship with the older adult, or <c>null</c> when unknown.</param>
public record EmergencyContact(string Name, string PhoneNumber, FamilyRelationship? Relationship)
{
    /// <summary>
    ///     An empty contact.
    /// </summary>
    public static EmergencyContact Empty { get; } = new(string.Empty, string.Empty, null);

    /// <summary>
    ///     Creates a contact with trimmed values.
    /// </summary>
    /// <param name="name">Full name.</param>
    /// <param name="phoneNumber">Phone number.</param>
    /// <param name="relationship">Relationship with the older adult.</param>
    /// <returns>The normalized contact.</returns>
    public static EmergencyContact Create(string? name, string? phoneNumber, FamilyRelationship? relationship) =>
        new(TextNormalizer.Clean(name), TextNormalizer.Clean(phoneNumber), relationship);

    /// <summary>
    ///     Gets a value indicating whether there is a name and a phone to call.
    /// </summary>
    public bool IsComplete => Name.Length > 0 && PhoneNumber.Length > 0;
}
