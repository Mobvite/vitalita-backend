using Vitalita.API.Profiles.Domain.Model.ValueObjects;

namespace Vitalita.API.Profiles.Domain.Model.Queries;

/// <summary>
///     Query to get several older adults; the ones the requester cannot see are left out.
/// </summary>
/// <param name="Requester">The authenticated user.</param>
/// <param name="Ids">Older adult identifiers.</param>
public record GetOlderAdultsByIdsQuery(Requester Requester, IReadOnlyCollection<int> Ids);
