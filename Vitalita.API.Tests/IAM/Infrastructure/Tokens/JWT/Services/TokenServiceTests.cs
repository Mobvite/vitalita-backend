using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Vitalita.API.IAM.Domain.Model.ValueObjects;
using Vitalita.API.IAM.Infrastructure.Tokens.JWT.Configuration;
using Vitalita.API.IAM.Infrastructure.Tokens.JWT.Services;
using Vitalita.API.Tests.IAM.Fakes;

namespace Vitalita.API.Tests.IAM.Infrastructure.Tokens.JWT.Services;

public class TokenServiceTests
{
    private const string Secret = "test-secret-with-at-least-32-bytes-0123456789";

    private static JwtSettings Settings(int expirationMinutes = 15, string secret = Secret) => new()
    {
        Issuer = "vitalita-api",
        Audience = "vitalita-frontend",
        ExpirationMinutes = expirationMinutes,
        Secret = secret
    };

    private static bool IsValid(JwtSettings settings) =>
        Validator.TryValidateObject(settings, new ValidationContext(settings), [], validateAllProperties: true);

    [Fact]
    public async Task GenerateToken_HasIdEmailRoleIssuerAudienceAndLifetime()
    {
        var now = DateTime.UtcNow;
        var service = new TokenService(Options.Create(Settings()), new FixedClock(now));
        var user = UserBuilder.Build(7, role: UserRole.FamilyMember);

        var token = service.GenerateToken(user);

        var validation = await new JsonWebTokenHandler().ValidateTokenAsync(token, new TokenValidationParameters
        {
            ValidIssuer = "vitalita-api",
            ValidAudience = "vitalita-frontend",
            IssuerSigningKey = TokenService.CreateSigningKey(Secret),
            ClockSkew = TimeSpan.Zero
        });
        Assert.True(validation.IsValid, validation.Exception?.Message);

        var jwt = (JsonWebToken)validation.SecurityToken;
        Assert.Equal("7", jwt.Subject);
        Assert.Equal("andrea.mendoza@vitalita.pe", jwt.GetClaim("email").Value);
        Assert.Equal("FamilyMember", jwt.GetClaim("role").Value);
        Assert.Equal(TimeSpan.FromMinutes(15), jwt.ValidTo - jwt.IssuedAt);
        Assert.Equal(SecurityAlgorithms.HmacSha256, jwt.Alg);
    }

    [Fact]
    public async Task GenerateToken_SignedWithAnotherSecret_IsRejected()
    {
        var service = new TokenService(Options.Create(Settings()), new FixedClock(DateTime.UtcNow));

        var token = service.GenerateToken(UserBuilder.Build(1));

        var validation = await new JsonWebTokenHandler().ValidateTokenAsync(token, new TokenValidationParameters
        {
            ValidIssuer = "vitalita-api",
            ValidAudience = "vitalita-frontend",
            IssuerSigningKey = TokenService.CreateSigningKey("another-secret-with-at-least-32-bytes-000")
        });
        Assert.False(validation.IsValid);
    }

    [Fact]
    public void Settings_DefaultExpiration_Is15Minutes()
    {
        Assert.Equal(15, new JwtSettings().ExpirationMinutes);
    }

    [Theory]
    [InlineData(15, true)]
    [InlineData(1, true)]
    [InlineData(16, false)]
    [InlineData(0, false)]
    public void Settings_Expiration_IsAtMost15Minutes(int minutes, bool expected)
    {
        Assert.Equal(expected, IsValid(Settings(minutes)));
    }

    [Fact]
    public void Settings_ShortSecret_IsRejected()
    {
        Assert.False(IsValid(Settings(secret: "too-short")));
    }
}
