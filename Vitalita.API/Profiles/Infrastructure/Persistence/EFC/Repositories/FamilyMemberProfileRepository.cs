using Microsoft.EntityFrameworkCore;
using Vitalita.API.Profiles.Domain.Model.Aggregates;
using Vitalita.API.Profiles.Domain.Repositories;
using Vitalita.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using Vitalita.API.Shared.Infrastructure.Persistence.EFC.Repositories;

namespace Vitalita.API.Profiles.Infrastructure.Persistence.EFC.Repositories;

/// <summary>
///     EF Core repository for the <see cref="FamilyMemberProfile" /> aggregate.
/// </summary>
/// <param name="context">The EF Core database context.</param>
public class FamilyMemberProfileRepository(AppDbContext context)
    : BaseRepository<FamilyMemberProfile>(context), IFamilyMemberProfileRepository
{
    /// <inheritdoc />
    public async Task<FamilyMemberProfile?> FindByUserIdAsync(int userId,
        CancellationToken cancellationToken = default)
    {
        return await Context.Set<FamilyMemberProfile>()
            .FirstOrDefaultAsync(profile => profile.UserId == userId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await Context.Set<FamilyMemberProfile>()
            .AnyAsync(profile => profile.UserId == userId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<FamilyMemberProfile>> FindByIdsAsync(IReadOnlyCollection<int> ids,
        CancellationToken cancellationToken = default)
    {
        if (ids.Count == 0) return [];
        var distinctIds = ids.Distinct().ToList();
        return await Context.Set<FamilyMemberProfile>()
            .Where(profile => distinctIds.Contains(profile.Id))
            .OrderBy(profile => profile.Id)
            .ToListAsync(cancellationToken);
    }
}
