using Microsoft.EntityFrameworkCore;
using Vitalita.API.Profiles.Domain.Model.Aggregates;
using Vitalita.API.Profiles.Domain.Repositories;
using Vitalita.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using Vitalita.API.Shared.Infrastructure.Persistence.EFC.Repositories;

namespace Vitalita.API.Profiles.Infrastructure.Persistence.EFC.Repositories;

/// <summary>
///     EF Core repository for the <see cref="CaregiverProfile" /> aggregate.
/// </summary>
/// <param name="context">The EF Core database context.</param>
public class CaregiverProfileRepository(AppDbContext context)
    : BaseRepository<CaregiverProfile>(context), ICaregiverProfileRepository
{
    /// <inheritdoc />
    public async Task<CaregiverProfile?> FindByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await Context.Set<CaregiverProfile>()
            .FirstOrDefaultAsync(profile => profile.UserId == userId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await Context.Set<CaregiverProfile>().AnyAsync(profile => profile.UserId == userId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await Context.Set<CaregiverProfile>().AnyAsync(profile => profile.Id == id, cancellationToken);
    }
}
