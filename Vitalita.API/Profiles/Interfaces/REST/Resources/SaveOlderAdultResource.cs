using System.ComponentModel.DataAnnotations;
using Vitalita.API.Profiles.Domain.Model.ValueObjects;
using Vitalita.API.Shared.Infrastructure.Interfaces.ASP.Validation;

namespace Vitalita.API.Profiles.Interfaces.REST.Resources;

/// <summary>
///     Body of <c>POST</c> and <c>PUT</c> <c>/older-adults</c>. An <c>id</c> in the body is ignored.
/// </summary>
/// <param name="CaregiverId">Caregiver profile in charge (the requester's own profile, unless an administrator).</param>
/// <param name="FirstName">First name (required, up to 100 characters).</param>
/// <param name="LastName">Last name (required, up to 100 characters).</param>
/// <param name="BirthDate">Birth date (required, <c>yyyy-MM-dd</c>, not in the future).</param>
/// <param name="DocumentNumber">National ID (DNI): empty or 8 digits.</param>
/// <param name="BloodType">Empty, O+, O-, A+, A-, B+, B-, AB+ or AB-.</param>
/// <param name="Allergies">Up to 30 items of up to 100 characters.</param>
/// <param name="ChronicConditions">Up to 30 items of up to 100 characters.</param>
/// <param name="EmergencyContact">Person to call in an emergency.</param>
/// <param name="Observations">Up to 1000 characters.</param>
/// <param name="Status">Active (default on create), Inactive or Archived. Required on update.</param>
public record SaveOlderAdultResource(
    [Required(ErrorMessage = "RequiredField")]
    [Range(1, int.MaxValue, ErrorMessage = "InvalidIdentifier")]
    int? CaregiverId,
    [Required(ErrorMessage = "RequiredField")]
    [StringLength(ProfileLimits.NameMaxLength, ErrorMessage = "NameLength")]
    string FirstName,
    [Required(ErrorMessage = "RequiredField")]
    [StringLength(ProfileLimits.NameMaxLength, ErrorMessage = "NameLength")]
    string LastName,
    [Required(ErrorMessage = "RequiredField")]
    DateOnly? BirthDate,
    [RegularExpression(ProfileLimits.DocumentNumberPattern, ErrorMessage = "DocumentNumberFormat")]
    string? DocumentNumber = null,
    [AllowedValues(null, "", "O+", "O-", "A+", "A-", "B+", "B-", "AB+", "AB-", ErrorMessage = "InvalidBloodType")]
    string? BloodType = null,
    [MaxLength(ProfileLimits.ClinicalListMaxCount, ErrorMessage = "ClinicalListLimit")]
    [ItemsMaxLength(ProfileLimits.ListItemMaxLength, ErrorMessage = "ClinicalListLimit")]
    IReadOnlyList<string>? Allergies = null,
    [MaxLength(ProfileLimits.ClinicalListMaxCount, ErrorMessage = "ClinicalListLimit")]
    [ItemsMaxLength(ProfileLimits.ListItemMaxLength, ErrorMessage = "ClinicalListLimit")]
    IReadOnlyList<string>? ChronicConditions = null,
    EmergencyContactResource? EmergencyContact = null,
    [StringLength(ProfileLimits.ObservationsMaxLength, ErrorMessage = "ObservationsLength")]
    string? Observations = null,
    [AllowedValues(null, nameof(OlderAdultStatus.Active), nameof(OlderAdultStatus.Inactive),
        nameof(OlderAdultStatus.Archived), ErrorMessage = "InvalidOlderAdultStatus")]
    string? Status = null);
