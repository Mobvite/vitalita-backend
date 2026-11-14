using Vitalita.API.Shared.Domain.Model.ValueObjects;

namespace Vitalita.API.Profiles.Domain.Model.Commands;

/// <summary>
///     Command to create the profile of a family member.
/// </summary>
/// <param name="Requester">The authenticated user.</param>
/// <param name="UserId">IAM user that owns the profile.</param>
/// <param name="FirstName">First name.</param>
/// <param name="LastName">Last name.</param>
/// <param name="PhoneNumber">Phone number.</param>
public record CreateFamilyMemberProfileCommand(
    Requester Requester,
    int UserId,
    string FirstName,
    string LastName,
    string? PhoneNumber);
