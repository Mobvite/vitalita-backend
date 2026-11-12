using System.Security.Cryptography;
using Vitalita.API.Profiles.Domain.Model.ValueObjects;
using Vitalita.API.Profiles.Domain.Services;

namespace Vitalita.API.Profiles.Infrastructure.Security;

/// <summary>
///     Generates invitation codes with <see cref="RandomNumberGenerator" /> (cryptographically secure).
/// </summary>
public class InvitationCodeGenerator : IInvitationCodeGenerator
{
    /// <inheritdoc />
    public InvitationCode Generate()
    {
        var random = RandomNumberGenerator.GetItems<char>(InvitationCode.Alphabet, InvitationCode.RandomLength);
        return new InvitationCode(InvitationCode.Prefix + new string(random));
    }
}
