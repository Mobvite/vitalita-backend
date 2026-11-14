using Vitalita.API.Monitoring.Application.Errors;
using Vitalita.API.Monitoring.Application.Internal.Policies;
using Vitalita.API.Monitoring.Application.Services;
using Vitalita.API.Monitoring.Domain.Model.Aggregates;
using Vitalita.API.Monitoring.Domain.Model.Queries;
using Vitalita.API.Monitoring.Domain.Repositories;
using Vitalita.API.Shared.Application.Patterns;

namespace Vitalita.API.Monitoring.Application.Internal.QueryServices;

/// <summary>
///     Application service that handles the <see cref="Medication" /> queries.
/// </summary>
/// <remarks>
///     Reads need the caregiver in charge or a family member with an active access (administrators read
///     everything). An older adult the requester cannot read, or that does not exist, is <c>Forbidden</c>.
/// </remarks>
/// <param name="medicationRepository">Medication repository.</param>
/// <param name="accessPolicy">Access rules for the records of an older adult.</param>
public class MedicationQueryService(
    IMedicationRepository medicationRepository,
    OlderAdultAccessPolicy accessPolicy)
    : IMedicationQueryService
{
    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<Medication>, MonitoringQueryError>> Handle(GetAllMedicationsQuery query,
        CancellationToken cancellationToken = default)
    {
        if (!query.Requester.IsAdmin) return ListFailure(MonitoringQueryError.Forbidden);
        return ListSuccess(await medicationRepository.FindAllAsync(query.Active, cancellationToken));
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<Medication>, MonitoringQueryError>> Handle(
        GetMedicationsByOlderAdultIdQuery query,
        CancellationToken cancellationToken = default)
    {
        if (!await accessPolicy.CanReadAsync(query.Requester, query.OlderAdultId, cancellationToken))
            return ListFailure(MonitoringQueryError.Forbidden);
        return ListSuccess(
            await medicationRepository.FindByOlderAdultIdAsync(query.OlderAdultId, query.Active, cancellationToken));
    }

    /// <inheritdoc />
    public async Task<Result<Medication, MonitoringQueryError>> Handle(GetMedicationByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var record = await medicationRepository.FindByIdAsync(query.Id, cancellationToken);
        if (record is null) return new Result<Medication, MonitoringQueryError>.Failure(MonitoringQueryError.NotFound);
        return await accessPolicy.CanReadAsync(query.Requester, record.OlderAdultId, cancellationToken)
            ? new Result<Medication, MonitoringQueryError>.Success(record)
            : new Result<Medication, MonitoringQueryError>.Failure(MonitoringQueryError.Forbidden);
    }

    private static Result<IReadOnlyList<Medication>, MonitoringQueryError>
        ListSuccess(IReadOnlyList<Medication> records) =>
        new Result<IReadOnlyList<Medication>, MonitoringQueryError>.Success(records);

    private static Result<IReadOnlyList<Medication>, MonitoringQueryError> ListFailure(MonitoringQueryError error) =>
        new Result<IReadOnlyList<Medication>, MonitoringQueryError>.Failure(error);
}
