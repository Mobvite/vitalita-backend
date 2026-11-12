using Vitalita.API.Profiles.Application.Errors;
using Vitalita.API.Profiles.Domain.Model.Aggregates;
using Vitalita.API.Profiles.Domain.Model.Commands;
using Vitalita.API.Profiles.Domain.Model.Queries;
using Vitalita.API.Shared.Application.Patterns;

namespace Vitalita.API.Profiles.Application.Services;

/// <summary>
///     Query service of the <see cref="OlderAdult" /> aggregate.
/// </summary>
public interface IOlderAdultQueryService
{
    /// <summary>
    ///     Lists every older adult (administrators only).
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The older adults, or Forbidden.</returns>
    Task<Result<IReadOnlyList<OlderAdult>, ProfileQueryError>> Handle(GetAllOlderAdultsQuery query,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Lists the older adults in the charge of a caregiver.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The older adults, or Forbidden when the caregiver profile is not the requester's.</returns>
    Task<Result<IReadOnlyList<OlderAdult>, ProfileQueryError>> Handle(GetOlderAdultsByCaregiverIdQuery query,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets several older adults, leaving out the ones the requester cannot read.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The readable older adults.</returns>
    Task<IReadOnlyList<OlderAdult>> Handle(GetOlderAdultsByIdsQuery query,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets an older adult by identifier.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The older adult, NotFound or Forbidden.</returns>
    Task<Result<OlderAdult, ProfileQueryError>> Handle(GetOlderAdultByIdQuery query,
        CancellationToken cancellationToken = default);
}
