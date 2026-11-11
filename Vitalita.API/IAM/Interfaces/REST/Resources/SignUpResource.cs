using System.ComponentModel.DataAnnotations;
using Vitalita.API.IAM.Domain.Model.ValueObjects;

namespace Vitalita.API.IAM.Interfaces.REST.Resources;

/// <summary>
///     Data required to register an account through the public sign-up.
/// </summary>
/// <remarks>
///     The frontend also sends <c>status</c>, <c>createdAt</c> and <c>lastLoginAt</c>; they are ignored because
///     the server sets them.
/// </remarks>
/// <param name="Email">Account email (required, valid format, up to 254 characters).</param>
/// <param name="Password">Password (8 to 72 characters).</param>
/// <param name="Role">"Caregiver" (default) or "FamilyMember". "Admin" is rejected.</param>
public record SignUpResource(
    [Required(ErrorMessage = "RequiredField")]
    [StringLength(EmailAddress.MaxLength, ErrorMessage = "EmailLength")]
    [RegularExpression(EmailAddress.Pattern, ErrorMessage = "InvalidEmail")]
    string Email,
    [Required(ErrorMessage = "RequiredField")]
    [StringLength(72, MinimumLength = 8, ErrorMessage = "PasswordLength")]
    string Password,
    [AllowedValues(null, nameof(UserRole.Caregiver), nameof(UserRole.FamilyMember), nameof(UserRole.Admin),
        ErrorMessage = "InvalidRole")]
    [DeniedValues(nameof(UserRole.Admin), ErrorMessage = "AdminSignUpNotAllowed")]
    string? Role = null);
