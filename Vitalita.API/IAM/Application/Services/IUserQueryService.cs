using Vitalita.API.IAM.Application.Errors;
using Vitalita.API.IAM.Domain.Model.Aggregates;
using Vitalita.API.IAM.Domain.Model.Queries;
using Vitalita.API.Shared.Application.Patterns;

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

    /// <summary>
    ///     Gets the user of an access token, checking that it still exists and is active.
    /// </summary>
    /// <returns>The user, or <see cref="CurrentUserError.UserNotFound" /> / <see cref="CurrentUserError.InactiveAccount" />.</returns>
    Task<Result<User, CurrentUserError>> Handle(GetCurrentUserQuery query,
        CancellationToken cancellationToken = default);
}
