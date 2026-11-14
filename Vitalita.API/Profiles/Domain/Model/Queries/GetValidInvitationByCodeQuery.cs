using Vitalita.API.Profiles.Domain.Model.ValueObjects;

namespace Vitalita.API.Profiles.Domain.Model.Queries;

/// <summary>
///     Public query that looks for an invitation that can still be accepted (pending and not expired).
/// </summary>
/// <param name="Code">The invitation code.</param>
public record GetValidInvitationByCodeQuery(InvitationCode Code);
