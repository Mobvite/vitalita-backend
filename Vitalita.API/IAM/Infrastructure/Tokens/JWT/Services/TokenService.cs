using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Vitalita.API.IAM.Application.Internal.OutboundServices;
using Vitalita.API.IAM.Domain.Model.Aggregates;
using Vitalita.API.IAM.Infrastructure.Tokens.JWT.Configuration;
using Vitalita.API.Shared.Domain.Services;

namespace Vitalita.API.IAM.Infrastructure.Tokens.JWT.Services;

/// <summary>
///     JWT implementation of <see cref="ITokenService" /> (HMAC-SHA256).
/// </summary>
/// <remarks>
///     Claims: <c>sub</c> (user id), <c>email</c> and <c>role</c>. The issue and expiration instants come from
///     <see cref="IClock" />.
/// </remarks>
/// <param name="options">The JWT settings.</param>
/// <param name="clock">The clock.</param>
public class TokenService(IOptions<JwtSettings> options, IClock clock) : ITokenService
{
    /// <summary>
    ///     Name of the role claim.
    /// </summary>
    public const string RoleClaimType = "role";

    /// <summary>
    ///     Name of the email claim.
    /// </summary>
    public const string EmailClaimType = JwtRegisteredClaimNames.Email;

    /// <summary>
    ///     Name of the user identifier claim.
    /// </summary>
    public const string UserIdClaimType = JwtRegisteredClaimNames.Sub;

    /// <summary>
    ///     Builds the signing key from a secret.
    /// </summary>
    /// <param name="secret">The configured secret.</param>
    /// <returns>The symmetric security key.</returns>
    public static SymmetricSecurityKey CreateSigningKey(string secret) => new(Encoding.UTF8.GetBytes(secret));

    /// <inheritdoc />
    public string GenerateToken(User user)
    {
        ArgumentNullException.ThrowIfNull(user);
        var settings = options.Value;
        var now = clock.UtcNow;

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = settings.Issuer,
            Audience = settings.Audience,
            Subject = new ClaimsIdentity(
            [
                new Claim(UserIdClaimType, user.Id.ToString()),
                new Claim(EmailClaimType, user.Email.Value),
                new Claim(RoleClaimType, user.Role.ToString())
            ]),
            IssuedAt = now,
            NotBefore = now,
            Expires = now.AddMinutes(settings.ExpirationMinutes),
            SigningCredentials = new SigningCredentials(CreateSigningKey(settings.Secret),
                SecurityAlgorithms.HmacSha256)
        };

        return new JsonWebTokenHandler { SetDefaultTimesOnTokenCreation = false }.CreateToken(descriptor);
    }
}
