using Vitalita.API.Profiles.Domain.Model.ValueObjects;
using Vitalita.API.Shared.Domain.Model.ValueObjects;

namespace Vitalita.API.Profiles.Domain.Model.Queries;

/// <summary>
///     Query to list the family accesses of a family member.
/// </summary>
/// <param name="Requester">The authenticated user.</param>
/// <param name="FamilyMemberId">Family member profile identifier.</param>
/// <param name="Status">Optional status filter.</param>
public record GetFamilyAccessesByFamilyMemberIdQuery(Requester Requester, int FamilyMemberId, AccessStatus? Status);
