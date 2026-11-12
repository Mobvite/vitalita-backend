using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Vitalita.API.IAM.Infrastructure.Tokens.JWT.Configuration;

/// <summary>
///     JWT settings, bound from the <c>Authentication:Jwt</c> configuration section.
/// </summary>
/// <remarks>
///     <see cref="Secret" /> is never stored in the repository: use <c>dotnet user-secrets</c> in development and the
///     <c>Authentication__Jwt__Secret</c> environment variable in production. The settings are validated on startup.
/// </remarks>
public class JwtSettings : IValidatableObject
{
    /// <summary>
    ///     Configuration section name.
    /// </summary>
    public const string SectionName = "Authentication:Jwt";

    /// <summary>
    ///     Maximum token lifetime allowed, in minutes.
    /// </summary>
    public const int MaxExpirationMinutes = 15;

    /// <summary>
    ///     Minimum secret size for HMAC-SHA256, in bytes.
    /// </summary>
    public const int MinSecretBytes = 32;

    /// <summary>
    ///     Gets or sets the token issuer (<c>iss</c>).
    /// </summary>
    [Required]
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the token audience (<c>aud</c>).
    /// </summary>
    [Required]
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the token lifetime in minutes (1 to <see cref="MaxExpirationMinutes" />, default 15).
    /// </summary>
    [Range(1, MaxExpirationMinutes)]
    public int ExpirationMinutes { get; set; } = MaxExpirationMinutes;

    /// <summary>
    ///     Gets or sets the HMAC-SHA256 signing secret (at least 32 bytes).
    /// </summary>
    public string Secret { get; set; } = string.Empty;

    /// <inheritdoc />
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Encoding.UTF8.GetByteCount(Secret ?? string.Empty) < MinSecretBytes)
            yield return new ValidationResult(
                $"'{SectionName}:Secret' must have at least {MinSecretBytes} bytes. Use 'dotnet user-secrets set " +
                "\"Authentication:Jwt:Secret\" \"...\"' in development or the 'Authentication__Jwt__Secret' " +
                "environment variable in production.",
                [nameof(Secret)]);
    }
}
