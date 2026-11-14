using Vitalita.API.Shared.Domain.Model.ValueObjects;

namespace Vitalita.API.Profiles.Domain.Model.Commands;

/// <summary>
///     Command to create the profile of a caregiver.
/// </summary>
/// <param name="Requester">The authenticated user.</param>
/// <param name="UserId">IAM user that owns the profile.</param>
/// <param name="FirstName">First name.</param>
/// <param name="LastName">Last name.</param>
/// <param name="PhoneNumber">Phone number.</param>
/// <param name="ProfessionalTitle">Professional title.</param>
/// <param name="Bio">Short presentation.</param>
/// <param name="PhotoUrl">Photo URL.</param>
/// <param name="Certifications">Certifications and courses.</param>
public record CreateCaregiverProfileCommand(
    Requester Requester,
    int UserId,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    string? ProfessionalTitle,
    string? Bio,
    string? PhotoUrl,
    IReadOnlyList<string> Certifications);
