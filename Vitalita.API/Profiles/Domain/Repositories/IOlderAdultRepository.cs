using Vitalita.API.Profiles.Domain.Model.Aggregates;
using Vitalita.API.Shared.Domain.Repositories;

namespace Vitalita.API.Profiles.Domain.Repositories;

/// <summary>
///     Repository contract for the <see cref="OlderAdult" /> aggregate.
/// </summary>
public interface IOlderAdultRepository : IBaseRepository<OlderAdult>
{
    /// <summary>
    ///     Lists the older adults in the charge of a caregiver.
    /// </summary>
    /// <param name="caregiverId">Caregiver profile identifier.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The older adults, ordered by identifier.</returns>
    Task<IReadOnlyList<OlderAdult>> FindByCaregiverIdAsync(int caregiverId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Finds several older adults by identifier.
    /// </summary>
    /// <param name="ids">Older adult identifiers.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The older adults found, ordered by identifier.</returns>
    Task<IReadOnlyList<OlderAdult>> FindByIdsAsync(IReadOnlyCollection<int> ids,
        CancellationToken cancellationToken = default);
}
