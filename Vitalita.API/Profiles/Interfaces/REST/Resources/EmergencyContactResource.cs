using System.ComponentModel.DataAnnotations;
using Vitalita.API.Profiles.Domain.Model.ValueObjects;

namespace Vitalita.API.Profiles.Interfaces.REST.Resources;

/// <summary>
///     Person to call in an emergency. Every field may be empty.
/// </summary>
/// <param name="Name">Full name (up to 100 characters).</param>
/// <param name="PhoneNumber">Empty, or 6 to 20 digits, spaces or a leading <c>+</c>.</param>
/// <param name="Relationship">Son, Daughter, Spouse, Grandchild, Sibling, Other, or empty.</param>
public record EmergencyContactResource(
    [StringLength(ProfileLimits.NameMaxLength, ErrorMessage = "NameLength")]
    string? Name = null,
    [RegularExpression(ProfileLimits.PhoneNumberPattern, ErrorMessage = "PhoneNumberFormat")]
    string? PhoneNumber = null,
    [AllowedValues(null, "", nameof(FamilyRelationship.Son), nameof(FamilyRelationship.Daughter),
        nameof(FamilyRelationship.Spouse), nameof(FamilyRelationship.Grandchild), nameof(FamilyRelationship.Sibling),
        nameof(FamilyRelationship.Other), ErrorMessage = "InvalidRelationship")]
    string? Relationship = null);
