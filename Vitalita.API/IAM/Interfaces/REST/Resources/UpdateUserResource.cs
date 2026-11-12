using System.ComponentModel.DataAnnotations;
using Vitalita.API.IAM.Domain.Model.ValueObjects;

namespace Vitalita.API.IAM.Interfaces.REST.Resources;

/// <summary>
///     Full replacement of an account (<c>PUT</c>).
/// </summary>
/// <param name="Email">Account email (required, valid format, up to 254 characters).</param>
/// <param name="Role">"Caregiver", "FamilyMember" or "Admin". A user cannot change their own role.</param>
/// <param name="Status">"Active" or "Inactive". A user cannot change their own status.</param>
/// <param name="Password">New password (8 to 72 characters), or <c>null</c> to keep the current one.</param>
public record UpdateUserResource(
    [Required(ErrorMessage = "RequiredField")]
    [StringLength(EmailAddress.MaxLength, ErrorMessage = "EmailLength")]
    [RegularExpression(EmailAddress.Pattern, ErrorMessage = "InvalidEmail")]
    string Email,
    [Required(ErrorMessage = "RequiredField")]
    [AllowedValues(nameof(UserRole.Caregiver), nameof(UserRole.FamilyMember), nameof(UserRole.Admin),
        ErrorMessage = "InvalidRole")]
    string Role,
    [Required(ErrorMessage = "RequiredField")]
    [AllowedValues(nameof(UserStatus.Active), nameof(UserStatus.Inactive), ErrorMessage = "InvalidStatus")]
    string Status,
    [StringLength(72, MinimumLength = 8, ErrorMessage = "PasswordLength")]
    string? Password = null);
