namespace Vitalita.API.Shared.Domain.Repositories;

/// <summary>
///     Base repository contract with the CRUD operations shared by every aggregate repository.
/// </summary>
/// <remarks>
///     Context repositories extend this interface with their own query methods (filters, batch lookups).
/// </remarks>
/// <typeparam name="TEntity">The entity type.</typeparam>
public interface IBaseRepository<TEntity>
{
    /// <summary>
    ///     Adds an entity to the repository.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A task representing the asynchronous add operation.</returns>
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Finds an entity by its numeric identifier.
    /// </summary>
    /// <param name="id">The entity identifier.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The entity if found; otherwise <c>null</c>.</returns>
    Task<TEntity?> FindByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Marks an entity as updated.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    void Update(TEntity entity);

    /// <summary>
    ///     Marks an entity as removed.
    /// </summary>
    /// <param name="entity">The entity to remove.</param>
    void Remove(TEntity entity);

    /// <summary>
    ///     Lists all entities.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>All the entities in the repository.</returns>
    Task<IEnumerable<TEntity>> ListAsync(CancellationToken cancellationToken = default);
}
