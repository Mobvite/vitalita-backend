using Vitalita.API.Profiles.Domain.Model.ValueObjects;

namespace Vitalita.API.Profiles.Domain.Model.Queries;

/// <summary>
///     Query to get a caregiver profile by identifier.
/// </summary>
/// <param name="Requester">The authenticated user.</param>
/// <param name="Id">Profile identifier.</param>
public record GetCaregiverProfileByIdQuery(Requester Requester, int Id);
