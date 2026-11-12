using Vitalita.API.Profiles.Domain.Model.ValueObjects;

namespace Vitalita.API.Profiles.Domain.Model.Commands;

/// <summary>
///     Command to replace the data of a family member profile.
/// </summary>
/// <param name="Id">Profile identifier.</param>
/// <param name="Requester">The authenticated user.</param>
/// <param name="UserId">Owner sent by the client; it must be the current owner.</param>
/// <param name="FirstName">First name.</param>
/// <param name="LastName">Last name.</param>
/// <param name="PhoneNumber">Phone number.</param>
public record UpdateFamilyMemberProfileCommand(
    int Id,
    Requester Requester,
    int UserId,
    string FirstName,
    string LastName,
    string? PhoneNumber);
