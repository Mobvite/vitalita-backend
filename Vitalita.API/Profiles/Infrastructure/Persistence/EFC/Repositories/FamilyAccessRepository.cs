using Microsoft.EntityFrameworkCore;
using Vitalita.API.Profiles.Domain.Model.Aggregates;
using Vitalita.API.Profiles.Domain.Model.ValueObjects;
using Vitalita.API.Profiles.Domain.Repositories;
using Vitalita.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using Vitalita.API.Shared.Infrastructure.Persistence.EFC.Repositories;

namespace Vitalita.API.Profiles.Infrastructure.Persistence.EFC.Repositories;

/// <summary>
///     EF Core repository for the <see cref="FamilyAccess" /> aggregate.
/// </summary>
/// <param name="context">The EF Core database context.</param>
public class FamilyAccessRepository(AppDbContext context)
    : BaseRepository<FamilyAccess>(context), IFamilyAccessRepository
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<FamilyAccess>> FindByOlderAdultIdsAsync(IReadOnlyCollection<int> olderAdultIds,
        AccessStatus? status = null, CancellationToken cancellationToken = default)
    {
        if (olderAdultIds.Count == 0) return [];
        var distinctIds = olderAdultIds.Distinct().ToList();
        return await WithStatus(status)
            .Where(access => distinctIds.Contains(access.OlderAdultId))
            .OrderBy(access => access.Id)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<FamilyAccess>> FindByFamilyMemberIdAsync(int familyMemberId,
        AccessStatus? status = null, CancellationToken cancellationToken = default)
    {
        return await WithStatus(status)
            .Where(access => access.FamilyMemberId == familyMemberId)
            .OrderBy(access => access.Id)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<FamilyAccess>> FindAllAsync(AccessStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        return await WithStatus(status).OrderBy(access => access.Id).ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<FamilyAccess?> FindByInvitationCodeAsync(InvitationCode code,
        CancellationToken cancellationToken = default)
    {
        return await Context.Set<FamilyAccess>()
            .FirstOrDefaultAsync(access => access.InvitationCode == code, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsByInvitationCodeAsync(InvitationCode code,
        CancellationToken cancellationToken = default)
    {
        return await Context.Set<FamilyAccess>().AnyAsync(access => access.InvitationCode == code, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsOpenAccessAsync(int olderAdultId, string invitedEmail,
        CancellationToken cancellationToken = default)
    {
        return await Context.Set<FamilyAccess>().AnyAsync(access =>
            access.OlderAdultId == olderAdultId &&
            access.InvitedEmail == invitedEmail &&
            access.Status != AccessStatus.Revoked, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsValidInvitationForEmailAsync(string invitedEmail, DateTime utcNow,
        CancellationToken cancellationToken = default)
    {
        return await Context.Set<FamilyAccess>().AnyAsync(access =>
            access.InvitedEmail == invitedEmail &&
            access.Status == AccessStatus.Pending &&
            access.ExpiresAt > utcNow, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsActiveAccessAsync(int familyMemberId, int olderAdultId,
        CancellationToken cancellationToken = default)
    {
        return await Context.Set<FamilyAccess>().AnyAsync(access =>
            access.FamilyMemberId == familyMemberId &&
            access.OlderAdultId == olderAdultId &&
            access.Status == AccessStatus.Active, cancellationToken);
    }

    private IQueryable<FamilyAccess> WithStatus(AccessStatus? status)
    {
        var accesses = Context.Set<FamilyAccess>().AsQueryable();
        return status is { } value ? accesses.Where(access => access.Status == value) : accesses;
    }
}
