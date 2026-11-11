namespace Vitalita.API.Shared.Domain.Repositories;

/// <summary>
///     Unit of work contract that commits all pending changes as a single transaction.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    ///     Commits all pending changes to the database.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A task representing the asynchronous commit.</returns>
    Task CompleteAsync(CancellationToken cancellationToken = default);
}
