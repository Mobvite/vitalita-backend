using Vitalita.API.Shared.Domain.Repositories;
using Vitalita.API.Shared.Infrastructure.Persistence.EFC.Configuration;

namespace Vitalita.API.Shared.Infrastructure.Persistence.EFC.Repositories;

/// <summary>
///     EF Core unit of work: persists all pending changes in a single <c>SaveChangesAsync</c> call.
/// </summary>
/// <param name="context">The EF Core database context.</param>
public class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    /// <inheritdoc />
    public async Task CompleteAsync(CancellationToken cancellationToken = default)
    {
        await context.SaveChangesAsync(cancellationToken);
    }
}
