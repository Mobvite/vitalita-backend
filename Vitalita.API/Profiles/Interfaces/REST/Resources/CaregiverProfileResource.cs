namespace Vitalita.API.Profiles.Interfaces.REST.Resources;

/// <summary>
///     Caregiver profile.
/// </summary>
/// <param name="Id">Profile identifier.</param>
/// <param name="UserId">IAM user that owns the profile.</param>
/// <param name="FirstName">First name.</param>
/// <param name="LastName">Last name.</param>
/// <param name="PhoneNumber">Phone number (may be empty).</param>
/// <param name="ProfessionalTitle">Professional title (may be empty).</param>
/// <param name="Bio">Short presentation (may be empty).</param>
/// <param name="PhotoUrl">Photo URL (may be empty).</param>
/// <param name="Certifications">Certifications and courses.</param>
public record CaregiverProfileResource(
    int Id,
    int UserId,
    string FirstName,
    string LastName,
    string PhoneNumber,
    string ProfessionalTitle,
    string Bio,
    string PhotoUrl,
    IReadOnlyList<string> Certifications);
