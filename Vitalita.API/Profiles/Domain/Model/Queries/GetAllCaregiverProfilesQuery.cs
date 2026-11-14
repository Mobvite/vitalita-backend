using Vitalita.API.Shared.Domain.Model.ValueObjects;

namespace Vitalita.API.Profiles.Domain.Model.Queries;

/// <summary>
///     Query to list every caregiver profile (administrators only).
/// </summary>
/// <param name="Requester">The authenticated user.</param>
public record GetAllCaregiverProfilesQuery(Requester Requester);
