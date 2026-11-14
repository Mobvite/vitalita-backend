using System.ComponentModel.DataAnnotations;
using Vitalita.API.IAM.Domain.Model.ValueObjects;

namespace Vitalita.API.IAM.Interfaces.REST.Resources;

/// <summary>
///     Credentials sent to sign in.
/// </summary>
/// <remarks>The frontend also sends the <c>role</c> chosen in the form; it is ignored by the API.</remarks>
/// <param name="Email">Account email.</param>
/// <param name="Password">Account password.</param>
public record SignInResource(
    [Required(ErrorMessage = "RequiredField")]
    [StringLength(EmailAddress.MaxLength, ErrorMessage = "EmailLength")]
    [RegularExpression(EmailAddress.Pattern, ErrorMessage = "InvalidEmail")]
    string Email,
    [Required(ErrorMessage = "RequiredField")]
    [StringLength(72, ErrorMessage = "PasswordLength")]
    string Password);
