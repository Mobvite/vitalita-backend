using System.Text;
using Vitalita.API.Profiles.Domain.Model.ValueObjects;

namespace Vitalita.API.Profiles.Domain.Model.Commands;

/// <summary>
///     Command to invite a family member to follow an older adult.
/// </summary>
/// <param name="Requester">The authenticated user.</param>
/// <param name="OlderAdultId">Older adult to share.</param>
/// <param name="InvitedEmail">Email of the invited person.</param>
/// <param name="Relationship">Relationship with the older adult.</param>
public record CreateFamilyAccessCommand(
    Requester Requester,
    int OlderAdultId,
    string InvitedEmail,
    FamilyRelationship Relationship)
{
    /// <summary>
    ///     Prints the members without the invited email, so the command can be logged safely.
    /// </summary>
    protected virtual bool PrintMembers(StringBuilder builder)
    {
        builder.Append($"Requester = {Requester}, OlderAdultId = {OlderAdultId}, Relationship = {Relationship}");
        return true;
    }
}
