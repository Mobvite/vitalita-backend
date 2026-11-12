using Vitalita.API.Monitoring.Application.Errors;
using Vitalita.API.Monitoring.Domain.Model.Aggregates;
using Vitalita.API.Monitoring.Domain.Model.Commands;
using Vitalita.API.Monitoring.Domain.Model.Queries;
using Vitalita.API.Shared.Application.Patterns;

namespace Vitalita.API.Monitoring.Application.Services;

/// <summary>
///     Query service of the <see cref="VitalSign" /> aggregate.
/// </summary>
public interface IVitalSignQueryService
{
    /// <summary>
    ///     Lists every vital sign (administrators only).
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The records, or Forbidden.</returns>
    Task<Result<IReadOnlyList<VitalSign>, MonitoringQueryError>> Handle(GetAllVitalSignsQuery query,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Lists the vital signs of an older adult.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The records, or Forbidden when the requester cannot read the older adult.</returns>
    Task<Result<IReadOnlyList<VitalSign>, MonitoringQueryError>> Handle(GetVitalSignsByOlderAdultIdQuery query,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets a vital sign by identifier.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The record, NotFound or Forbidden.</returns>
    Task<Result<VitalSign, MonitoringQueryError>> Handle(GetVitalSignByIdQuery query,
        CancellationToken cancellationToken = default);
}
