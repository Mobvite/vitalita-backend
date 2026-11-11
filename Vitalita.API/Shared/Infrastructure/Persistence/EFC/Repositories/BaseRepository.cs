using Microsoft.EntityFrameworkCore;
using Vitalita.API.Shared.Domain.Repositories;
using Vitalita.API.Shared.Infrastructure.Persistence.EFC.Configuration;

namespace Vitalita.API.Shared.Infrastructure.Persistence.EFC.Repositories;

/// <summary>
///     Generic EF Core repository with the CRUD operations of <see cref="IBaseRepository{TEntity}" />.
/// </summary>
/// <remarks>
///     Context repositories inherit from this class and add their own queries through <see cref="Context" />.
///     Changes are persisted by <see cref="UnitOfWork" />, not by the repository.
/// </remarks>
/// <typeparam name="TEntity">The entity type.</typeparam>
/// <param name="context">The EF Core database context.</param>
public class BaseRepository<TEntity>(AppDbContext context) : IBaseRepository<TEntity> where TEntity : class
{
    /// <summary>
    ///     The database context available to derived repositories.
    /// </summary>
    protected readonly AppDbContext Context = context;

    /// <inheritdoc />
    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await Context.Set<TEntity>().AddAsync(entity, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<TEntity?> FindByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await Context.Set<TEntity>().FindAsync([id], cancellationToken);
    }

    /// <inheritdoc />
    public void Update(TEntity entity)
    {
        Context.Set<TEntity>().Update(entity);
    }

    /// <inheritdoc />
    public void Remove(TEntity entity)
    {
        Context.Set<TEntity>().Remove(entity);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<TEntity>> ListAsync(CancellationToken cancellationToken = default)
    {
        return await Context.Set<TEntity>().ToListAsync(cancellationToken);
    }
}
