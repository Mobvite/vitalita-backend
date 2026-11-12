using Microsoft.EntityFrameworkCore;
using Vitalita.API.Monitoring.Domain.Model.Aggregates;
using Vitalita.API.Monitoring.Domain.Repositories;
using Vitalita.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using Vitalita.API.Shared.Infrastructure.Persistence.EFC.Repositories;

namespace Vitalita.API.Monitoring.Infrastructure.Persistence.EFC.Repositories;

/// <summary>
///     EF Core repository for the <see cref="Medication" /> aggregate.
/// </summary>
/// <param name="context">The EF Core database context.</param>
public class MedicationRepository(AppDbContext context) : BaseRepository<Medication>(context), IMedicationRepository
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<Medication>> FindByOlderAdultIdAsync(int olderAdultId, bool? active = null,
        CancellationToken cancellationToken = default)
    {
        return await WithActive(active)
            .Where(medication => medication.OlderAdultId == olderAdultId)
            .OrderBy(medication => medication.Id)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Medication>> FindAllAsync(bool? active = null,
        CancellationToken cancellationToken = default)
    {
        return await WithActive(active).OrderBy(medication => medication.Id).ToListAsync(cancellationToken);
    }

    private IQueryable<Medication> WithActive(bool? active)
    {
        var medications = Context.Set<Medication>().AsQueryable();
        return active is { } value ? medications.Where(medication => medication.Active == value) : medications;
    }
}
