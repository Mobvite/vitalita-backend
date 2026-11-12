using Vitalita.API.Monitoring.Domain.Model.Aggregates;
using Vitalita.API.Shared.Domain.Repositories;

namespace Vitalita.API.Monitoring.Domain.Repositories;

/// <summary>
///     Repository contract for the <see cref="VitalSign" /> aggregate.
/// </summary>
public interface IVitalSignRepository : IBaseRepository<VitalSign>
{
    /// <summary>
    ///     Lists the records of an older adult.
    /// </summary>
    /// <param name="olderAdultId">Older adult identifier.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The records, ordered by identifier.</returns>
    Task<IReadOnlyList<VitalSign>> FindByOlderAdultIdAsync(int olderAdultId, CancellationToken cancellationToken = default);
}
