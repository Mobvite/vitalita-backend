using Vitalita.API.Monitoring.Application.Errors;
using Vitalita.API.Monitoring.Application.Internal.Policies;
using Vitalita.API.Monitoring.Application.Services;
using Vitalita.API.Monitoring.Domain.Model.Aggregates;
using Vitalita.API.Monitoring.Domain.Model.Queries;
using Vitalita.API.Monitoring.Domain.Repositories;
using Vitalita.API.Shared.Application.Patterns;

namespace Vitalita.API.Monitoring.Application.Internal.QueryServices;

/// <summary>
///     Application service that handles the <see cref="VitalSign" /> queries.
/// </summary>
/// <remarks>
///     Reads need the caregiver in charge or a family member with an active access (administrators read
///     everything). An older adult the requester cannot read, or that does not exist, is <c>Forbidden</c>.
/// </remarks>
/// <param name="vitalSignRepository">Vital sign repository.</param>
/// <param name="accessPolicy">Access rules for the records of an older adult.</param>
public class VitalSignQueryService(
    IVitalSignRepository vitalSignRepository,
    OlderAdultAccessPolicy accessPolicy)
    : IVitalSignQueryService
{
    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<VitalSign>, MonitoringQueryError>> Handle(GetAllVitalSignsQuery query,
        CancellationToken cancellationToken = default)
    {
        if (!query.Requester.IsAdmin) return ListFailure(MonitoringQueryError.Forbidden);
        return ListSuccess((await vitalSignRepository.ListAsync(cancellationToken)).ToList());
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<VitalSign>, MonitoringQueryError>> Handle(
        GetVitalSignsByOlderAdultIdQuery query,
        CancellationToken cancellationToken = default)
    {
        if (!await accessPolicy.CanReadAsync(query.Requester, query.OlderAdultId, cancellationToken))
            return ListFailure(MonitoringQueryError.Forbidden);
        return ListSuccess(await vitalSignRepository.FindByOlderAdultIdAsync(query.OlderAdultId, cancellationToken));
    }

    /// <inheritdoc />
    public async Task<Result<VitalSign, MonitoringQueryError>> Handle(GetVitalSignByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var record = await vitalSignRepository.FindByIdAsync(query.Id, cancellationToken);
        if (record is null) return new Result<VitalSign, MonitoringQueryError>.Failure(MonitoringQueryError.NotFound);
        return await accessPolicy.CanReadAsync(query.Requester, record.OlderAdultId, cancellationToken)
            ? new Result<VitalSign, MonitoringQueryError>.Success(record)
            : new Result<VitalSign, MonitoringQueryError>.Failure(MonitoringQueryError.Forbidden);
    }

    private static Result<IReadOnlyList<VitalSign>, MonitoringQueryError>
        ListSuccess(IReadOnlyList<VitalSign> records) =>
        new Result<IReadOnlyList<VitalSign>, MonitoringQueryError>.Success(records);

    private static Result<IReadOnlyList<VitalSign>, MonitoringQueryError> ListFailure(MonitoringQueryError error) =>
        new Result<IReadOnlyList<VitalSign>, MonitoringQueryError>.Failure(error);
}
