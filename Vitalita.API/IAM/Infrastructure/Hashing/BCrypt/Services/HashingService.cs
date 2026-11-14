using Vitalita.API.IAM.Application.Internal.OutboundServices;
using BCryptNet = BCrypt.Net.BCrypt;

namespace Vitalita.API.IAM.Infrastructure.Hashing.BCrypt.Services;

/// <summary>
///     BCrypt implementation of <see cref="IHashingService" />.
/// </summary>
public class HashingService : IHashingService
{
    /// <summary>
    ///     BCrypt cost factor (2^12 rounds).
    /// </summary>
    public const int WorkFactor = 12;

    // Hash of a random value, used to spend the same time when the email does not exist.
    private static readonly Lazy<string> DummyHash =
        new(() => BCryptNet.HashPassword(Guid.NewGuid().ToString(), WorkFactor));

    /// <inheritdoc />
    public string HashPassword(string password)
    {
        ArgumentException.ThrowIfNullOrEmpty(password);
        return BCryptNet.HashPassword(password, WorkFactor);
    }

    /// <inheritdoc />
    public bool VerifyPassword(string password, string? passwordHash)
    {
        if (passwordHash is null)
        {
            BCryptNet.Verify(password ?? string.Empty, DummyHash.Value);
            return false;
        }

        return !string.IsNullOrEmpty(password) && BCryptNet.Verify(password, passwordHash);
    }
}
