using Vitalita.API.IAM.Application.Services;
using Vitalita.API.IAM.Domain.Model.Aggregates;
using Vitalita.API.IAM.Domain.Model.Queries;
using Vitalita.API.IAM.Domain.Repositories;

namespace Vitalita.API.IAM.Application.Internal.QueryServices;

/// <summary>
///     Application service that handles the <see cref="User" /> queries.
/// </summary>
/// <param name="userRepository">Repository for accessing user data.</param>
public class UserQueryService(IUserRepository userRepository) : IUserQueryService
{
    /// <inheritdoc />
    public async Task<User?> Handle(GetUserByIdQuery query, CancellationToken cancellationToken = default)
    {
        return await userRepository.FindByIdAsync(query.UserId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<User>> Handle(GetUsersByEmailQuery query,
        CancellationToken cancellationToken = default)
    {
        var user = await userRepository.FindByEmailAsync(query.Email, cancellationToken);
        return user is null ? [] : [user];
    }

    /// <inheritdoc />
    public async Task<IEnumerable<User>> Handle(GetAllUsersQuery query, CancellationToken cancellationToken = default)
    {
        return await userRepository.ListAsync(cancellationToken);
    }
}
