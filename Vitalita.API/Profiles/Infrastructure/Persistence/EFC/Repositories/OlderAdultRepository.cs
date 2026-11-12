using Microsoft.EntityFrameworkCore;
using Vitalita.API.Profiles.Domain.Model.Aggregates;
using Vitalita.API.Profiles.Domain.Repositories;
using Vitalita.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using Vitalita.API.Shared.Infrastructure.Persistence.EFC.Repositories;

namespace Vitalita.API.Profiles.Infrastructure.Persistence.EFC.Repositories;

/// <summary>
///     EF Core repository for the <see cref="OlderAdult" /> aggregate.
/// </summary>
/// <param name="context">The EF Core database context.</param>
public class OlderAdultRepository(AppDbContext context) : BaseRepository<OlderAdult>(context), IOlderAdultRepository
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<OlderAdult>> FindByCaregiverIdAsync(int caregiverId,
        CancellationToken cancellationToken = default)
    {
        return await Context.Set<OlderAdult>()
            .Where(olderAdult => olderAdult.CaregiverId == caregiverId)
            .OrderBy(olderAdult => olderAdult.Id)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<OlderAdult>> FindByIdsAsync(IReadOnlyCollection<int> ids,
        CancellationToken cancellationToken = default)
    {
        if (ids.Count == 0) return [];
        var distinctIds = ids.Distinct().ToList();
        return await Context.Set<OlderAdult>()
            .Where(olderAdult => distinctIds.Contains(olderAdult.Id))
            .OrderBy(olderAdult => olderAdult.Id)
            .ToListAsync(cancellationToken);
    }
}
