using Vitalita.API.Profiles.Domain.Model.ValueObjects;

namespace Vitalita.API.Profiles.Domain.Model.Queries;

/// <summary>
///     Query to list the family accesses of an older adult.
/// </summary>
/// <param name="Requester">The authenticated user.</param>
/// <param name="OlderAdultId">Older adult identifier.</param>
/// <param name="Status">Optional status filter.</param>
public record GetFamilyAccessesByOlderAdultIdQuery(Requester Requester, int OlderAdultId, AccessStatus? Status);
