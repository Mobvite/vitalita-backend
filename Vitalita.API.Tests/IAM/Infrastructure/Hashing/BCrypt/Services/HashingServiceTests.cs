using Vitalita.API.IAM.Infrastructure.Hashing.BCrypt.Services;

namespace Vitalita.API.Tests.IAM.Infrastructure.Hashing.BCrypt.Services;

public class HashingServiceTests
{
    private readonly HashingService _service = new();

    [Fact]
    public void HashPassword_ProducesABCryptHashThatVerifies()
    {
        var hash = _service.HashPassword("Vitalita123");

        Assert.StartsWith($"$2a${HashingService.WorkFactor}$", hash);
        Assert.Equal(60, hash.Length);
        Assert.DoesNotContain("Vitalita123", hash);
        Assert.True(_service.VerifyPassword("Vitalita123", hash));
        Assert.False(_service.VerifyPassword("vitalita123", hash));
    }

    [Fact]
    public void VerifyPassword_WithoutHash_ReturnsFalse()
    {
        Assert.False(_service.VerifyPassword("Vitalita123", null));
    }
}
