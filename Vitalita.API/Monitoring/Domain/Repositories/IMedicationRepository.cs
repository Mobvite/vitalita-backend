using Vitalita.API.Monitoring.Domain.Model.Aggregates;
using Vitalita.API.Shared.Domain.Repositories;

namespace Vitalita.API.Monitoring.Domain.Repositories;

/// <summary>
///     Repository contract for the <see cref="Medication" /> aggregate.
/// </summary>
public interface IMedicationRepository : IBaseRepository<Medication>
{
    /// <summary>
    ///     Lists the medications of an older adult.
    /// </summary>
    /// <param name="olderAdultId">Older adult identifier.</param>
    /// <param name="active">Optional filter by the active flag.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The medications, ordered by identifier.</returns>
    Task<IReadOnlyList<Medication>> FindByOlderAdultIdAsync(int olderAdultId, bool? active = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Lists every medication.
    /// </summary>
    /// <param name="active">Optional filter by the active flag.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The medications, ordered by identifier.</returns>
    Task<IReadOnlyList<Medication>> FindAllAsync(bool? active = null, CancellationToken cancellationToken = default);
}
