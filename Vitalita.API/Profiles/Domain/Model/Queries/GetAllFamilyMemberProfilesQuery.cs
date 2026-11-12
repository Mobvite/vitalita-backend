using Vitalita.API.Profiles.Domain.Model.ValueObjects;

namespace Vitalita.API.Profiles.Domain.Model.Queries;

/// <summary>
///     Query to list every family member profile (administrators only).
/// </summary>
/// <param name="Requester">The authenticated user.</param>
public record GetAllFamilyMemberProfilesQuery(Requester Requester);
