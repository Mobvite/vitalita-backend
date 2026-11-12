using Microsoft.EntityFrameworkCore;
using Vitalita.API.IAM.Domain.Model.Aggregates;
using Vitalita.API.IAM.Domain.Model.ValueObjects;
using Vitalita.API.IAM.Domain.Repositories;
using Vitalita.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using Vitalita.API.Shared.Infrastructure.Persistence.EFC.Repositories;

namespace Vitalita.API.IAM.Infrastructure.Persistence.EFC.Repositories;

/// <summary>
///     EF Core repository for the <see cref="User" /> aggregate.
/// </summary>
/// <param name="context">The EF Core database context.</param>
public class UserRepository(AppDbContext context) : BaseRepository<User>(context), IUserRepository
{
    /// <inheritdoc />
    public async Task<User?> FindByEmailAsync(EmailAddress email, CancellationToken cancellationToken = default)
    {
        return await Context.Set<User>().FirstOrDefaultAsync(user => user.Email == email, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsByEmailAsync(EmailAddress email, CancellationToken cancellationToken = default)
    {
        return await Context.Set<User>().AnyAsync(user => user.Email == email, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await Context.Set<User>().AnyAsync(user => user.Id == id, cancellationToken);
    }
}
