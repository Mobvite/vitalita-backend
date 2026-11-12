using Vitalita.API.Profiles.Domain.Model.ValueObjects;

namespace Vitalita.API.Profiles.Domain.Services;

/// <summary>
///     Creates new invitation codes.
/// </summary>
/// <remarks>Implementations must use a cryptographic random number generator.</remarks>
public interface IInvitationCodeGenerator
{
    /// <summary>
    ///     Generates a code with the format <c>VITA-</c> + 8 characters of <see cref="InvitationCode.Alphabet" />.
    /// </summary>
    /// <returns>A new code. Uniqueness is checked by the caller and by the unique index.</returns>
    InvitationCode Generate();
}
