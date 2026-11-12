using Vitalita.API.Profiles.Domain.Model.ValueObjects;

namespace Vitalita.API.Profiles.Domain.Model.Queries;

/// <summary>
///     Query to get a family member profile by identifier.
/// </summary>
/// <param name="Requester">The authenticated user.</param>
/// <param name="Id">Profile identifier.</param>
public record GetFamilyMemberProfileByIdQuery(Requester Requester, int Id);
