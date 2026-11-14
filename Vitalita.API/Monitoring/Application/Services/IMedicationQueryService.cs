using Vitalita.API.Monitoring.Application.Errors;
using Vitalita.API.Monitoring.Domain.Model.Aggregates;
using Vitalita.API.Monitoring.Domain.Model.Commands;
using Vitalita.API.Monitoring.Domain.Model.Queries;
using Vitalita.API.Shared.Application.Patterns;

namespace Vitalita.API.Monitoring.Application.Services;

/// <summary>
///     Query service of the <see cref="Medication" /> aggregate.
/// </summary>
public interface IMedicationQueryService
{
    /// <summary>
    ///     Lists every medication (administrators only).
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The records, or Forbidden.</returns>
    Task<Result<IReadOnlyList<Medication>, MonitoringQueryError>> Handle(GetAllMedicationsQuery query,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Lists the medications of an older adult.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The records, or Forbidden when the requester cannot read the older adult.</returns>
    Task<Result<IReadOnlyList<Medication>, MonitoringQueryError>> Handle(GetMedicationsByOlderAdultIdQuery query,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets a medication by identifier.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The record, NotFound or Forbidden.</returns>
    Task<Result<Medication, MonitoringQueryError>> Handle(GetMedicationByIdQuery query,
        CancellationToken cancellationToken = default);
}
