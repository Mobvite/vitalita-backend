using System.ComponentModel.DataAnnotations;
using Vitalita.API.Profiles.Domain.Model.ValueObjects;

namespace Vitalita.API.Profiles.Interfaces.REST.Resources;

/// <summary>
///     Body of <c>POST /family-access</c>.
/// </summary>
/// <remarks>
///     The frontend also sends <c>invitationCode</c>, <c>status</c>, <c>invitedAt</c>, <c>expiresAt</c>,
///     <c>familyMemberId</c> and <c>acceptedAt</c>; they are ignored because the server sets them.
/// </remarks>
/// <param name="OlderAdultId">Older adult to share (required).</param>
/// <param name="InvitedEmail">Email of the invited person (required, stored in lowercase).</param>
/// <param name="Relationship">Son, Daughter, Spouse, Grandchild, Sibling or Other (required).</param>
public record CreateFamilyAccessResource(
    [Required(ErrorMessage = "RequiredField")]
    [Range(1, int.MaxValue, ErrorMessage = "InvalidIdentifier")]
    int? OlderAdultId,
    [Required(ErrorMessage = "RequiredField")]
    [StringLength(ProfileLimits.EmailMaxLength, ErrorMessage = "EmailLength")]
    [EmailAddress(ErrorMessage = "InvalidEmail")]
    string InvitedEmail,
    [Required(ErrorMessage = "RequiredField")]
    [AllowedValues(nameof(FamilyRelationship.Son), nameof(FamilyRelationship.Daughter),
        nameof(FamilyRelationship.Spouse), nameof(FamilyRelationship.Grandchild), nameof(FamilyRelationship.Sibling),
        nameof(FamilyRelationship.Other), ErrorMessage = "InvalidRelationship")]
    string Relationship);
