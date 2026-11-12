using Vitalita.API.Profiles.Domain.Model.ValueObjects;

namespace Vitalita.API.Profiles.Domain.Model.Queries;

/// <summary>
///     Query to get the caregiver profile of a user (zero or one element).
/// </summary>
/// <param name="Requester">The authenticated user.</param>
/// <param name="UserId">IAM user identifier.</param>
public record GetCaregiverProfilesByUserIdQuery(Requester Requester, int UserId);
