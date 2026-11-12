using System.Text;

namespace Vitalita.API.Profiles.Domain.Model.ValueObjects;

/// <summary>
///     The authenticated user that sends a request, taken from the JWT claims.
/// </summary>
/// <param name="UserId">IAM user identifier (claim <c>sub</c>).</param>
/// <param name="Role">Role claim: <c>Caregiver</c>, <c>FamilyMember</c> or <c>Admin</c>.</param>
/// <param name="Email">Email claim, used to check that an invitation belongs to the requester.</param>
public record Requester(int UserId, string? Role, string? Email)
{
    /// <summary>Value of the role claim of caregivers.</summary>
    public const string CaregiverRole = "Caregiver";

    /// <summary>Value of the role claim of family members.</summary>
    public const string FamilyMemberRole = "FamilyMember";

    /// <summary>Value of the role claim of administrators.</summary>
    public const string AdminRole = "Admin";

    /// <summary>Gets a value indicating whether the requester is an administrator.</summary>
    public bool IsAdmin => Role == AdminRole;

    /// <summary>Gets a value indicating whether the requester is a caregiver.</summary>
    public bool IsCaregiver => Role == CaregiverRole;

    /// <summary>Gets a value indicating whether the requester is a family member.</summary>
    public bool IsFamilyMember => Role == FamilyMemberRole;

    /// <summary>
    ///     Prints the members without the email, so commands and queries can be logged safely.
    /// </summary>
    protected virtual bool PrintMembers(StringBuilder builder)
    {
        builder.Append($"UserId = {UserId}, Role = {Role}");
        return true;
    }
}
