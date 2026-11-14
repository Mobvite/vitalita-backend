using Vitalita.API.Shared.Domain.Model.ValueObjects;

namespace Vitalita.API.Monitoring.Domain.Model.Queries;

/// <summary>
///     Query to list the daily reports of an older adult.
/// </summary>
/// <param name="Requester">The authenticated user.</param>
/// <param name="OlderAdultId">Older adult identifier.</param>
public record GetDailyReportsByOlderAdultIdQuery(Requester Requester, int OlderAdultId);
