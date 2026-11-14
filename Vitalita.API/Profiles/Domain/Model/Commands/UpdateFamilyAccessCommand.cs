using Vitalita.API.Profiles.Domain.Model.ValueObjects;
using Vitalita.API.Shared.Domain.Model.ValueObjects;

namespace Vitalita.API.Profiles.Domain.Model.Commands;

/// <summary>
///     Command to change a family access: accept it, revoke it or change the relationship.
/// </summary>
/// <remarks>
///     The other fields of the resource (older adult, email, code and dates) are controlled by the server.
/// </remarks>
/// <param name="Id">Family access identifier.</param>
/// <param name="Requester">The authenticated user.</param>
/// <param name="Status">The requested status.</param>
/// <param name="FamilyMemberId">Family member profile; required to accept.</param>
/// <param name="Relationship">The new relationship, or <c>null</c> to keep the current one.</param>
public record UpdateFamilyAccessCommand(
    int Id,
    Requester Requester,
    AccessStatus Status,
    int? FamilyMemberId,
    FamilyRelationship? Relationship);
