namespace Vitalita.API.Profiles.Interfaces.REST.Resources;

/// <summary>
///     Family member profile.
/// </summary>
/// <param name="Id">Profile identifier.</param>
/// <param name="UserId">IAM user that owns the profile.</param>
/// <param name="FirstName">First name.</param>
/// <param name="LastName">Last name.</param>
/// <param name="PhoneNumber">Phone number (may be empty).</param>
public record FamilyMemberProfileResource(int Id, int UserId, string FirstName, string LastName, string PhoneNumber);
