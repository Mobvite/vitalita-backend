using Microsoft.EntityFrameworkCore;
using Vitalita.API.IAM.Application.Errors;
using Vitalita.API.IAM.Application.Internal.OutboundServices;
using Vitalita.API.IAM.Application.Results;
using Vitalita.API.IAM.Application.Services;
using Vitalita.API.IAM.Domain.Model.Aggregates;
using Vitalita.API.IAM.Domain.Model.Commands;
using Vitalita.API.IAM.Domain.Model.Exceptions;
using Vitalita.API.IAM.Domain.Model.ValueObjects;
using Vitalita.API.IAM.Domain.Repositories;
using Vitalita.API.Shared.Application.Patterns;
using Vitalita.API.Shared.Domain.Repositories;
using Vitalita.API.Shared.Domain.Services;

namespace Vitalita.API.IAM.Application.Internal.CommandServices;

/// <summary>
///     Application service that handles the <see cref="User" /> commands.
/// </summary>
/// <remarks>
///     Passwords are hashed with <see cref="IHashingService" /> and are never logged. Logs only carry user
///     identifiers, never emails or passwords. Duplicate emails are detected with an application check and, for
///     concurrent requests, with the unique index (MySQL error 1062).
/// </remarks>
/// <param name="userRepository">Repository for accessing user data.</param>
/// <param name="unitOfWork">Unit of work for managing transaction scope.</param>
/// <param name="hashingService">Password hashing service.</param>
/// <param name="tokenService">Access token service.</param>
/// <param name="clock">Clock used for the last sign-in instant.</param>
/// <param name="logger">Logger for diagnostic and error reporting.</param>
public class UserCommandService(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IHashingService hashingService,
    ITokenService tokenService,
    IClock clock,
    ILogger<UserCommandService> logger)
    : IUserCommandService
{
    /// <inheritdoc />
    public async Task<Result<User, SignUpError>> Handle(SignUpCommand command,
        CancellationToken cancellationToken = default)
    {
        if (!User.CanSelfRegisterAs(command.Role))
        {
            logger.LogWarning("Public sign-up rejected because it requested the {Role} role", command.Role);
            return new Result<User, SignUpError>.Failure(SignUpError.AdminRoleNotAllowed);
        }

        if (await userRepository.ExistsByEmailAsync(command.Email, cancellationToken))
            return new Result<User, SignUpError>.Failure(SignUpError.DuplicateEmail);

        var user = new User(command.Email, hashingService.HashPassword(command.Password), command.Role);
        var saveError = await TryAddAsync(user, cancellationToken);
        return saveError switch
        {
            null => new Result<User, SignUpError>.Success(user),
            SaveError.DuplicateEmail => new Result<User, SignUpError>.Failure(SignUpError.DuplicateEmail),
            _ => new Result<User, SignUpError>.Failure(SignUpError.UnexpectedError)
        };
    }

    /// <inheritdoc />
    public async Task<Result<AuthenticatedUser, SignInError>> Handle(SignInCommand command,
        CancellationToken cancellationToken = default)
    {
        var user = await userRepository.FindByEmailAsync(command.Email, cancellationToken);

        // Always verify (against a dummy hash when the user does not exist), so timing does not reveal the email.
        if (!hashingService.VerifyPassword(command.Password, user?.PasswordHash) || user is null)
            return new Result<AuthenticatedUser, SignInError>.Failure(SignInError.InvalidCredentials);

        if (!user.IsActive)
        {
            logger.LogInformation("Sign-in rejected for inactive user {UserId}", user.Id);
            return new Result<AuthenticatedUser, SignInError>.Failure(SignInError.InactiveAccount);
        }

        user.RecordSignIn(clock.UtcNow);
        userRepository.Update(user);
        await unitOfWork.CompleteAsync(cancellationToken);

        return new Result<AuthenticatedUser, SignInError>.Success(
            new AuthenticatedUser(user, tokenService.GenerateToken(user)));
    }

    /// <inheritdoc />
    public async Task<Result<User, CreateUserError>> Handle(CreateUserCommand command,
        CancellationToken cancellationToken = default)
    {
        if (await userRepository.ExistsByEmailAsync(command.Email, cancellationToken))
            return new Result<User, CreateUserError>.Failure(CreateUserError.DuplicateEmail);

        var user = new User(command.Email, hashingService.HashPassword(command.Password), command.Role);
        var saveError = await TryAddAsync(user, cancellationToken);
        return saveError switch
        {
            null => new Result<User, CreateUserError>.Success(user),
            SaveError.DuplicateEmail => new Result<User, CreateUserError>.Failure(CreateUserError.DuplicateEmail),
            _ => new Result<User, CreateUserError>.Failure(CreateUserError.UnexpectedError)
        };
    }

    /// <inheritdoc />
    public async Task<Result<User, UpdateUserError>> Handle(UpdateUserCommand command,
        CancellationToken cancellationToken = default)
    {
        // A non-admin gets 403 before 404, so they cannot discover which identifiers exist.
        if (!command.RequesterIsAdmin && command.RequesterId != command.UserId)
            return new Result<User, UpdateUserError>.Failure(UpdateUserError.Forbidden);

        var user = await userRepository.FindByIdAsync(command.UserId, cancellationToken);
        if (user is null)
            return new Result<User, UpdateUserError>.Failure(UpdateUserError.NotFound);

        var owner = await userRepository.FindByEmailAsync(command.Email, cancellationToken);
        if (owner is not null && owner.Id != user.Id)
            return new Result<User, UpdateUserError>.Failure(UpdateUserError.DuplicateEmail);

        try
        {
            user.Update(command.Email, command.Role, command.Status, command.RequesterId);
        }
        catch (RoleChangeNotAllowedException)
        {
            return new Result<User, UpdateUserError>.Failure(UpdateUserError.RoleChangeNotAllowed);
        }
        catch (StatusChangeNotAllowedException)
        {
            return new Result<User, UpdateUserError>.Failure(UpdateUserError.StatusChangeNotAllowed);
        }

        if (command.Password is not null)
            user.ChangePasswordHash(hashingService.HashPassword(command.Password));

        try
        {
            userRepository.Update(user);
            await unitOfWork.CompleteAsync(cancellationToken);
            return new Result<User, UpdateUserError>.Success(user);
        }
        catch (DbUpdateException ex) when (IsDuplicateKeyViolation(ex))
        {
            logger.LogWarning(ex, "Duplicate email while updating user {UserId}", command.UserId);
            return new Result<User, UpdateUserError>.Failure(UpdateUserError.DuplicateEmail);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database update failed while updating user {UserId}", command.UserId);
            return new Result<User, UpdateUserError>.Failure(UpdateUserError.UnexpectedError);
        }
    }

    /// <inheritdoc />
    public async Task<Result<User, DeactivateUserError>> Handle(DeactivateUserCommand command,
        CancellationToken cancellationToken = default)
    {
        var user = await userRepository.FindByIdAsync(command.UserId, cancellationToken);
        if (user is null)
            return new Result<User, DeactivateUserError>.Failure(DeactivateUserError.NotFound);

        try
        {
            user.Deactivate(command.RequesterId);
        }
        catch (CannotDeactivateSelfException)
        {
            return new Result<User, DeactivateUserError>.Failure(DeactivateUserError.CannotDeactivateSelf);
        }

        userRepository.Update(user);
        await unitOfWork.CompleteAsync(cancellationToken);
        logger.LogInformation("User {UserId} deactivated by user {RequesterId}", user.Id, command.RequesterId);
        return new Result<User, DeactivateUserError>.Success(user);
    }

    /// <inheritdoc />
    public async Task<Result<User, SeedAdminError>> Handle(SeedAdminCommand command,
        CancellationToken cancellationToken = default)
    {
        if (await userRepository.ExistsByEmailAsync(command.Email, cancellationToken))
            return new Result<User, SeedAdminError>.Failure(SeedAdminError.EmailAlreadyRegistered);

        var admin = new User(command.Email, hashingService.HashPassword(command.Password), UserRole.Admin);
        await userRepository.AddAsync(admin, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
        logger.LogInformation("First administrator created with id {UserId}", admin.Id);
        return new Result<User, SeedAdminError>.Success(admin);
    }

    private enum SaveError
    {
        DuplicateEmail,
        Unexpected
    }

    /// <summary>
    ///     Adds a new user and commits. Returns <c>null</c> on success.
    /// </summary>
    private async Task<SaveError?> TryAddAsync(User user, CancellationToken cancellationToken)
    {
        try
        {
            await userRepository.AddAsync(user, cancellationToken);
            await unitOfWork.CompleteAsync(cancellationToken);
            return null;
        }
        catch (DbUpdateException ex) when (IsDuplicateKeyViolation(ex))
        {
            logger.LogWarning(ex, "Duplicate email rejected by the unique index while creating a user");
            return SaveError.DuplicateEmail;
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database update failed while creating a user");
            return SaveError.Unexpected;
        }
    }

    /// <summary>
    ///     Determines whether a <see cref="DbUpdateException" /> is a MySQL duplicate key violation (error 1062).
    /// </summary>
    private static bool IsDuplicateKeyViolation(DbUpdateException exception)
    {
        for (Exception? current = exception; current is not null; current = current.InnerException)
        {
            if (!string.Equals(current.GetType().Name, "MySqlException", StringComparison.Ordinal)) continue;
            var numberProperty = current.GetType().GetProperty("Number");
            if (numberProperty?.PropertyType == typeof(int) &&
                numberProperty.GetValue(current) is int errorCode &&
                errorCode == 1062)
                return true;
        }

        return false;
    }
}
