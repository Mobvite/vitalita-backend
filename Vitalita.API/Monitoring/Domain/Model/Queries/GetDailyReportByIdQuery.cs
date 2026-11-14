using Vitalita.API.Shared.Domain.Model.ValueObjects;

namespace Vitalita.API.Monitoring.Domain.Model.Queries;

/// <summary>
///     Query to get a daily report by identifier.
/// </summary>
/// <param name="Requester">The authenticated user.</param>
/// <param name="Id">Report identifier.</param>
public record GetDailyReportByIdQuery(Requester Requester, int Id);
