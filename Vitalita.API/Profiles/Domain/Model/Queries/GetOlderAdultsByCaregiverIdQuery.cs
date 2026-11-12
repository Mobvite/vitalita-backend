using Vitalita.API.Profiles.Domain.Model.ValueObjects;

namespace Vitalita.API.Profiles.Domain.Model.Queries;

/// <summary>
///     Query to list the older adults in the charge of a caregiver.
/// </summary>
/// <param name="Requester">The authenticated user.</param>
/// <param name="CaregiverId">Caregiver profile identifier.</param>
public record GetOlderAdultsByCaregiverIdQuery(Requester Requester, int CaregiverId);
