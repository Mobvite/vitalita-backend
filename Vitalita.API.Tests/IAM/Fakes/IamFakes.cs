using Vitalita.API.IAM.Application.Internal.OutboundServices;
using Vitalita.API.IAM.Domain.Model.Aggregates;
using Vitalita.API.IAM.Domain.Model.ValueObjects;
using Vitalita.API.IAM.Domain.Repositories;
using Vitalita.API.Shared.Domain.Repositories;
using Vitalita.API.Shared.Domain.Services;

namespace Vitalita.API.Tests.IAM.Fakes;

/// <summary>In-memory repository. Ids are assigned on add, like the database does.</summary>
internal sealed class InMemoryUserRepository : IUserRepository
{
    private readonly List<User> _users = [];
    private int _nextId = 1;

    public int RemoveCalls { get; private set; }
    public IReadOnlyList<User> Users => _users;

    public Task AddAsync(User entity, CancellationToken cancellationToken = default)
    {
        UserBuilder.SetId(entity, _nextId++);
        _users.Add(entity);
        return Task.CompletedTask;
    }

    public Task<User?> FindByIdAsync(int id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_users.FirstOrDefault(user => user.Id == id));

    public void Update(User entity)
    {
    }

    public void Remove(User entity)
    {
        RemoveCalls++;
        _users.Remove(entity);
    }

    public Task<IEnumerable<User>> ListAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IEnumerable<User>>(_users);

    public Task<User?> FindByEmailAsync(EmailAddress email, CancellationToken cancellationToken = default) =>
        Task.FromResult(_users.FirstOrDefault(user => user.Email == email));

    public Task<bool> ExistsByEmailAsync(EmailAddress email, CancellationToken cancellationToken = default) =>
        Task.FromResult(_users.Any(user => user.Email == email));

    public Task<bool> ExistsByIdAsync(int id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_users.Any(user => user.Id == id));
}

internal sealed class FakeUnitOfWork : IUnitOfWork
{
    public int CompleteCalls { get; private set; }

    public Task CompleteAsync(CancellationToken cancellationToken = default)
    {
        CompleteCalls++;
        return Task.CompletedTask;
    }
}

/// <summary>Reversible fake hash, so tests can check that the plain password is never stored.</summary>
internal sealed class FakeHashingService : IHashingService
{
    public const string Prefix = "hashed:";

    public int VerifyCalls { get; private set; }

    public string HashPassword(string password) => Prefix + password;

    public bool VerifyPassword(string password, string? passwordHash)
    {
        VerifyCalls++;
        return passwordHash is not null && passwordHash == Prefix + password;
    }
}

internal sealed class FakeTokenService : ITokenService
{
    public string GenerateToken(User user) => $"token-for-{user.Id}";
}

internal sealed class FixedClock(DateTime utcNow) : IClock
{
    public DateTime UtcNow { get; } = utcNow;
}

internal static class UserBuilder
{
    public static void SetId(User user, int id) => typeof(User).GetProperty(nameof(User.Id))!.SetValue(user, id);

    public static User Build(int id, string email = "andrea.mendoza@vitalita.pe", UserRole role = UserRole.Caregiver,
        string password = "Vitalita123")
    {
        var user = new User(new EmailAddress(email), FakeHashingService.Prefix + password, role);
        SetId(user, id);
        return user;
    }
}
