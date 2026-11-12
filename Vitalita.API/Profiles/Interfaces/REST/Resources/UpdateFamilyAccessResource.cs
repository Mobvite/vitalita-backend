using System.ComponentModel.DataAnnotations;
using Vitalita.API.Profiles.Domain.Model.ValueObjects;

namespace Vitalita.API.Profiles.Interfaces.REST.Resources;

/// <summary>
///     Body of <c>PUT /family-access/{id}</c>.
/// </summary>
/// <remarks>
///     The frontend sends the whole resource; only <c>status</c>, <c>familyMemberId</c> and <c>relationship</c>
///     are used. <c>olderAdultId</c>, <c>invitedEmail</c>, <c>invitationCode</c> and the dates are controlled by the
///     server and ignored. <c>relationship</c> is optional (the current one is kept), because the public invitation
///     lookup is enough to accept.
/// </remarks>
/// <param name="Status">Pending, Active or Revoked (required).</param>
/// <param name="FamilyMemberId">Family member profile; required to accept (Pending → Active).</param>
/// <param name="Relationship">Son, Daughter, Spouse, Grandchild, Sibling or Other; optional.</param>
public record UpdateFamilyAccessResource(
    [Required(ErrorMessage = "RequiredField")]
    [AllowedValues(nameof(AccessStatus.Pending), nameof(AccessStatus.Active), nameof(AccessStatus.Revoked),
        ErrorMessage = "InvalidAccessStatus")]
    string Status,
    [Range(1, int.MaxValue, ErrorMessage = "InvalidIdentifier")]
    int? FamilyMemberId = null,
    [AllowedValues(null, nameof(FamilyRelationship.Son), nameof(FamilyRelationship.Daughter),
        nameof(FamilyRelationship.Spouse), nameof(FamilyRelationship.Grandchild), nameof(FamilyRelationship.Sibling),
        nameof(FamilyRelationship.Other), ErrorMessage = "InvalidRelationship")]
    string? Relationship = null);
