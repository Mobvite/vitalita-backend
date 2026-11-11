using System.ComponentModel.DataAnnotations;
using Vitalita.API.IAM.Domain.Model.ValueObjects;

namespace Vitalita.API.IAM.Interfaces.REST.Resources;

/// <summary>
///     Data used by an administrator to create an account.
/// </summary>
/// <param name="Email">Account email (required, valid format, up to 254 characters).</param>
/// <param name="Password">Password (8 to 72 characters).</param>
/// <param name="Role">"Caregiver", "FamilyMember" or "Admin".</param>
public record CreateUserResource(
    [Required(ErrorMessage = "RequiredField")]
    [StringLength(EmailAddress.MaxLength, ErrorMessage = "EmailLength")]
    [RegularExpression(EmailAddress.Pattern, ErrorMessage = "InvalidEmail")]
    string Email,
    [Required(ErrorMessage = "RequiredField")]
    [StringLength(72, MinimumLength = 8, ErrorMessage = "PasswordLength")]
    string Password,
    [Required(ErrorMessage = "RequiredField")]
    [AllowedValues(nameof(UserRole.Caregiver), nameof(UserRole.FamilyMember), nameof(UserRole.Admin),
        ErrorMessage = "InvalidRole")]
    string Role);
