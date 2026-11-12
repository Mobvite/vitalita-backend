using Vitalita.API.Profiles.Domain.Model.ValueObjects;

namespace Vitalita.API.Profiles.Interfaces.REST.Resources;

/// <summary>
///     Family access (invitation and, once accepted, read access to an older adult).
/// </summary>
/// <param name="Id">Access identifier.</param>
/// <param name="OlderAdultId">Older adult shared.</param>
/// <param name="FamilyMemberId">Family member profile, or <c>null</c> while pending.</param>
/// <param name="InvitedEmail">Email of the invited person.</param>
/// <param name="InvitationCode">Code used to accept the invitation.</param>
/// <param name="Relationship">Relationship with the older adult.</param>
/// <param name="Status">Pending, Active or Revoked.</param>
/// <param name="InvitedAt">UTC instant of the invitation.</param>
/// <param name="AcceptedAt">UTC instant of the acceptance, or <c>null</c>.</param>
/// <param name="ExpiresAt">Last UTC instant to accept the invitation.</param>
public record FamilyAccessResource(
    int Id,
    int OlderAdultId,
    int? FamilyMemberId,
    string InvitedEmail,
    string InvitationCode,
    FamilyRelationship Relationship,
    AccessStatus Status,
    DateTime InvitedAt,
    DateTime? AcceptedAt,
    DateTime ExpiresAt);
