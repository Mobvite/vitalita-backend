using Vitalita.API.Shared.Domain.Model.ValueObjects;

namespace Vitalita.API.Monitoring.Domain.Model.Queries;

/// <summary>
///     Query to list every vital sign (administrators only).
/// </summary>
/// <param name="Requester">The authenticated user.</param>
public record GetAllVitalSignsQuery(Requester Requester);
