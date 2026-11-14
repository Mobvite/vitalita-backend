using Microsoft.EntityFrameworkCore;
using Vitalita.API.Monitoring.Domain.Model.Aggregates;
using Vitalita.API.Monitoring.Domain.Repositories;
using Vitalita.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using Vitalita.API.Shared.Infrastructure.Persistence.EFC.Repositories;

namespace Vitalita.API.Monitoring.Infrastructure.Persistence.EFC.Repositories;

/// <summary>
///     EF Core repository for the <see cref="VitalSign" /> aggregate.
/// </summary>
/// <param name="context">The EF Core database context.</param>
public class VitalSignRepository(AppDbContext context) : BaseRepository<VitalSign>(context), IVitalSignRepository
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<VitalSign>> FindByOlderAdultIdAsync(int olderAdultId,
        CancellationToken cancellationToken = default)
    {
        return await Context.Set<VitalSign>()
            .Where(record => record.OlderAdultId == olderAdultId)
            .OrderBy(record => record.Id)
            .ToListAsync(cancellationToken);
    }
}
