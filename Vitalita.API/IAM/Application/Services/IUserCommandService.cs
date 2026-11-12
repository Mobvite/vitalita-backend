using Vitalita.API.IAM.Application.Errors;
using Vitalita.API.IAM.Application.Results;
using Vitalita.API.IAM.Domain.Model.Aggregates;
using Vitalita.API.IAM.Domain.Model.Commands;
using Vitalita.API.Shared.Application.Patterns;

namespace Vitalita.API.IAM.Application.Services;

/// <summary>
///     Command service of the <see cref="User" /> aggregate.
/// </summary>
public interface IUserCommandService
{
    /// <summary>
    ///     Registers an account through the public sign-up.
    /// </summary>
    Task<Result<User, SignUpError>> Handle(SignUpCommand command, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Authenticates a user and issues an access token.
    /// </summary>
    Task<Result<AuthenticatedUser, SignInError>> Handle(SignInCommand command,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates an account with any role (administrators only).
    /// </summary>
    Task<Result<User, CreateUserError>> Handle(CreateUserCommand command,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Replaces the data of an account.
    /// </summary>
    Task<Result<User, UpdateUserError>> Handle(UpdateUserCommand command,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deactivates an account (logical delete).
    /// </summary>
    Task<Result<User, DeactivateUserError>> Handle(DeactivateUserCommand command,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates the first administrator when its email is not registered yet.
    /// </summary>
    Task<Result<User, SeedAdminError>> Handle(SeedAdminCommand command,
        CancellationToken cancellationToken = default);
}
