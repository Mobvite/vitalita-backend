using Vitalita.API.IAM.Domain.Model.Aggregates;
using Vitalita.API.IAM.Domain.Model.Queries;

namespace Vitalita.API.IAM.Application.Services;

/// <summary>
///     Query service of the <see cref="User" /> aggregate.
/// </summary>
public interface IUserQueryService
{
    /// <summary>
    ///     Gets a user by identifier.
    /// </summary>
    /// <returns>The user, or <c>null</c> when it does not exist.</returns>
    Task<User?> Handle(GetUserByIdQuery query, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets the users with a given email.
    /// </summary>
    /// <returns>Zero or one user.</returns>
    Task<IEnumerable<User>> Handle(GetUsersByEmailQuery query, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Lists every user.
    /// </summary>
    Task<IEnumerable<User>> Handle(GetAllUsersQuery query, CancellationToken cancellationToken = default);
}
