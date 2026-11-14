using Vitalita.API.Profiles.Domain.Model.ValueObjects;

namespace Vitalita.API.Profiles.Interfaces.REST.Resources;

/// <summary>
///     Public view of an invitation, returned by <c>GET /family-access?invitationCode=</c> without a token.
/// </summary>
/// <remarks>It never contains the older adult, the family member or other dates.</remarks>
/// <param name="Id">Access identifier, used later to accept the invitation.</param>
/// <param name="InvitedEmail">Email of the invited person (the sign-up form uses it).</param>
/// <param name="InvitationCode">The invitation code.</param>
/// <param name="Relationship">Relationship with the older adult.</param>
/// <param name="Status">Always Pending.</param>
/// <param name="ExpiresAt">Last UTC instant to accept the invitation.</param>
public record InvitationResource(
    int Id,
    string InvitedEmail,
    string InvitationCode,
    FamilyRelationship Relationship,
    AccessStatus Status,
    DateTime ExpiresAt);
