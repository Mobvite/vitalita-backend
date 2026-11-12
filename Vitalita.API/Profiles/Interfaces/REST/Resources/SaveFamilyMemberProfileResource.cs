using System.ComponentModel.DataAnnotations;
using Vitalita.API.Profiles.Domain.Model.ValueObjects;

namespace Vitalita.API.Profiles.Interfaces.REST.Resources;

/// <summary>
///     Body of <c>POST</c> and <c>PUT</c> <c>/family-member-profiles</c>. An <c>id</c> in the body is ignored.
/// </summary>
/// <param name="UserId">IAM user that owns the profile (the authenticated user, unless an administrator).</param>
/// <param name="FirstName">First name (required, up to 100 characters).</param>
/// <param name="LastName">Last name (required, up to 100 characters).</param>
/// <param name="PhoneNumber">Empty, or 6 to 20 digits, spaces or a leading <c>+</c>.</param>
public record SaveFamilyMemberProfileResource(
    [Required(ErrorMessage = "RequiredField")]
    [Range(1, int.MaxValue, ErrorMessage = "InvalidIdentifier")]
    int? UserId,
    [Required(ErrorMessage = "RequiredField")]
    [StringLength(ProfileLimits.NameMaxLength, ErrorMessage = "NameLength")]
    string FirstName,
    [Required(ErrorMessage = "RequiredField")]
    [StringLength(ProfileLimits.NameMaxLength, ErrorMessage = "NameLength")]
    string LastName,
    [RegularExpression(ProfileLimits.PhoneNumberPattern, ErrorMessage = "PhoneNumberFormat")]
    string? PhoneNumber = null);
