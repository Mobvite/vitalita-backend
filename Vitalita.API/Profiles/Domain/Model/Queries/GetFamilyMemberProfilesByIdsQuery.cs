using Vitalita.API.Profiles.Domain.Model.ValueObjects;

namespace Vitalita.API.Profiles.Domain.Model.Queries;

/// <summary>
///     Query to get several family member profiles; the ones the requester cannot see are left out.
/// </summary>
/// <param name="Requester">The authenticated user.</param>
/// <param name="Ids">Profile identifiers.</param>
public record GetFamilyMemberProfilesByIdsQuery(Requester Requester, IReadOnlyCollection<int> Ids);
