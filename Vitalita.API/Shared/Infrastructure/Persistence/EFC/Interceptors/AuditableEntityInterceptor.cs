using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Vitalita.API.Shared.Domain.Model;
using Vitalita.API.Shared.Domain.Services;

namespace Vitalita.API.Shared.Infrastructure.Persistence.EFC.Interceptors;

/// <summary>
///     EF Core interceptor that populates the audit timestamps of every <see cref="IAuditableEntity" />.
/// </summary>
/// <remarks>
///     <list type="bullet">
///         <item><description><c>CreatedAt</c>: set once when the entity is first added.</description></item>
///         <item><description><c>UpdatedAt</c>: refreshed on every add or update.</description></item>
///     </list>
///     The current instant comes from <see cref="IClock" /> (UTC). The interceptor is registered as a singleton and
///     added to <c>AppDbContext</c> in <c>Program.cs</c>, so it applies to all bounded contexts.
/// </remarks>
/// <param name="clock">The clock that provides the current UTC instant.</param>
public sealed class AuditableEntityInterceptor(IClock clock) : SaveChangesInterceptor
{
    /// <inheritdoc />
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        ApplyAuditTimestamps(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    /// <inheritdoc />
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ApplyAuditTimestamps(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void ApplyAuditTimestamps(DbContext? context)
    {
        if (context is null) return;

        var now = clock.UtcNow;

        foreach (var entry in context.ChangeTracker.Entries<IAuditableEntity>())
        {
            if (entry.State is EntityState.Added or EntityState.Modified) entry.Entity.UpdatedAt = now;
            if (entry.State == EntityState.Added) entry.Entity.CreatedAt ??= now;
        }
    }
}
