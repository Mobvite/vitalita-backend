using Vitalita.API.Profiles.Domain.Model.ValueObjects;

namespace Vitalita.API.Profiles.Domain.Model.Queries;

/// <summary>
///     Query to list every family access (administrators only).
/// </summary>
/// <param name="Requester">The authenticated user.</param>
/// <param name="Status">Optional status filter.</param>
public record GetAllFamilyAccessesQuery(Requester Requester, AccessStatus? Status);
