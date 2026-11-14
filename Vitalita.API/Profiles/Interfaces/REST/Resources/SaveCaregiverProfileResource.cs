using System.ComponentModel.DataAnnotations;
using Vitalita.API.Profiles.Domain.Model.ValueObjects;
using Vitalita.API.Shared.Infrastructure.Interfaces.ASP.Validation;

namespace Vitalita.API.Profiles.Interfaces.REST.Resources;

/// <summary>
///     Body of <c>POST</c> and <c>PUT</c> <c>/caregiver-profiles</c>. An <c>id</c> in the body is ignored.
/// </summary>
/// <param name="UserId">IAM user that owns the profile (the authenticated user, unless an administrator).</param>
/// <param name="FirstName">First name (required, up to 100 characters).</param>
/// <param name="LastName">Last name (required, up to 100 characters).</param>
/// <param name="PhoneNumber">Empty, or 6 to 20 digits, spaces or a leading <c>+</c>.</param>
/// <param name="ProfessionalTitle">Up to 100 characters.</param>
/// <param name="Bio">Up to 500 characters.</param>
/// <param name="PhotoUrl">Empty, or an <c>http(s)</c> URL of up to 500 characters.</param>
/// <param name="Certifications">Up to 20 items of up to 100 characters.</param>
public record SaveCaregiverProfileResource(
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
    string? PhoneNumber = null,
    [StringLength(ProfileLimits.ProfessionalTitleMaxLength, ErrorMessage = "ProfessionalTitleLength")]
    string? ProfessionalTitle = null,
    [StringLength(ProfileLimits.BioMaxLength, ErrorMessage = "BioLength")]
    string? Bio = null,
    [StringLength(ProfileLimits.PhotoUrlMaxLength, ErrorMessage = "PhotoUrlFormat")]
    [RegularExpression(ProfileLimits.PhotoUrlPattern, ErrorMessage = "PhotoUrlFormat")]
    string? PhotoUrl = null,
    [MaxLength(ProfileLimits.CertificationsMaxCount, ErrorMessage = "CertificationsLimit")]
    [ItemsMaxLength(ProfileLimits.ListItemMaxLength, ErrorMessage = "CertificationsLimit")]
    IReadOnlyList<string>? Certifications = null);
