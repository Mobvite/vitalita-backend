using Vitalita.API.Profiles.Domain.Model.Aggregates;
using Vitalita.API.Profiles.Interfaces.REST.Resources;

namespace Vitalita.API.Profiles.Interfaces.REST.Transform;

/// <summary>
///     Converts a <see cref="FamilyAccess" /> into its full resource or its public invitation view.
/// </summary>
public static class FamilyAccessResourceFromEntityAssembler
{
    /// <summary>
    ///     Converts the entity into the full resource (authenticated requests).
    /// </summary>
    /// <param name="entity">The family access.</param>
    /// <returns>The resource.</returns>
    public static FamilyAccessResource ToResourceFromEntity(FamilyAccess entity) =>
        new(entity.Id, entity.OlderAdultId, entity.FamilyMemberId, entity.InvitedEmail, entity.InvitationCode.Value,
            entity.Relationship, entity.Status, entity.InvitedAt, entity.AcceptedAt, entity.ExpiresAt);

    /// <summary>
    ///     Converts the entity into the minimal public view used by the family sign-up form.
    /// </summary>
    /// <param name="entity">The family access.</param>
    /// <returns>The invitation resource.</returns>
    public static InvitationResource ToInvitationResourceFromEntity(FamilyAccess entity) =>
        new(entity.Id, entity.InvitedEmail, entity.InvitationCode.Value, entity.Relationship, entity.Status,
            entity.ExpiresAt);
}
