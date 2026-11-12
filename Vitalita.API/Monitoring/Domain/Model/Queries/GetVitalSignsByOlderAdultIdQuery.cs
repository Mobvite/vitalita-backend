using Vitalita.API.Shared.Domain.Model.ValueObjects;

namespace Vitalita.API.Monitoring.Domain.Model.Queries;

/// <summary>
///     Query to list the vital signs of an older adult.
/// </summary>
/// <param name="Requester">The authenticated user.</param>
/// <param name="OlderAdultId">Older adult identifier.</param>
public record GetVitalSignsByOlderAdultIdQuery(Requester Requester, int OlderAdultId);
