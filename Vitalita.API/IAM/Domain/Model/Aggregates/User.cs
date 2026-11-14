using Vitalita.API.IAM.Domain.Model.Exceptions;
using Vitalita.API.IAM.Domain.Model.ValueObjects;
using Vitalita.API.Shared.Domain.Model;

namespace Vitalita.API.IAM.Domain.Model.Aggregates;

/// <summary>
///     User account aggregate root of the IAM bounded context.
/// </summary>
/// <remarks>
///     <list type="bullet">
///         <item><description>The email is unique (case-insensitive, see <see cref="EmailAddress" />).</description></item>
///         <item><description>Only the BCrypt hash of the password is kept; the plain password never reaches the aggregate.</description></item>
///         <item><description>Users are never removed: <see cref="Deactivate" /> is a logical delete.</description></item>
///         <item><description>A user cannot change their own role or status, nor deactivate themselves.</description></item>
///     </list>
///     Audit timestamps are managed by the persistence layer.
/// </remarks>
public class User : IAuditableEntity
{
    /// <summary>
    ///     Protected parameterless constructor for EF Core.
    /// </summary>
    protected User()
    {
        Email = null!;
        PasswordHash = string.Empty;
    }

    /// <summary>
    ///     Creates an active user.
    /// </summary>
    /// <param name="email">The account email.</param>
    /// <param name="passwordHash">The password hash (never the plain password).</param>
    /// <param name="role">The account role.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="email" /> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="passwordHash" /> is empty.</exception>
    public User(EmailAddress email, string passwordHash, UserRole role)
    {
        ArgumentNullException.ThrowIfNull(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);

        Email = email;
        PasswordHash = passwordHash;
        Role = role;
        Status = UserStatus.Active;
    }

    /// <summary>
    ///     Gets the server-generated identifier.
    /// </summary>
    public int Id { get; private set; }

    /// <summary>
    ///     Gets the normalized account email.
    /// </summary>
    public EmailAddress Email { get; private set; }

    /// <summary>
    ///     Gets the BCrypt hash of the password.
    /// </summary>
    public string PasswordHash { get; private set; }

    /// <summary>
    ///     Gets the account role.
    /// </summary>
    public UserRole Role { get; private set; }

    /// <summary>
    ///     Gets the account status.
    /// </summary>
    public UserStatus Status { get; private set; }

    /// <summary>
    ///     Gets the UTC instant of the last successful sign-in, if any.
    /// </summary>
    public DateTime? LastLoginAt { get; private set; }

    /// <inheritdoc />
    public DateTime? CreatedAt { get; set; }

    /// <inheritdoc />
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    ///     Gets a value indicating whether the account can sign in.
    /// </summary>
    public bool IsActive => Status == UserStatus.Active;

    /// <summary>
    ///     Checks whether a role can be obtained through the public sign-up.
    /// </summary>
    /// <param name="role">The requested role.</param>
    /// <returns><c>false</c> for <see cref="UserRole.Admin" />; <c>true</c> otherwise.</returns>
    public static bool CanSelfRegisterAs(UserRole role) => role != UserRole.Admin;

    /// <summary>
    ///     Records a successful sign-in.
    /// </summary>
    /// <param name="utcNow">The current UTC instant.</param>
    /// <exception cref="InvalidOperationException">Thrown when the account is not active.</exception>
    public void RecordSignIn(DateTime utcNow)
    {
        if (!IsActive) throw new InvalidOperationException("An inactive user cannot sign in.");
        LastLoginAt = DateTime.SpecifyKind(utcNow, DateTimeKind.Utc);
    }

    /// <summary>
    ///     Replaces the account data.
    /// </summary>
    /// <param name="email">The new email.</param>
    /// <param name="role">The new role.</param>
    /// <param name="status">The new status.</param>
    /// <param name="requesterId">Identifier of the user that requests the change.</param>
    /// <exception cref="RoleChangeNotAllowedException">Thrown when the user tries to change their own role.</exception>
    /// <exception cref="StatusChangeNotAllowedException">Thrown when the user tries to change their own status.</exception>
    public void Update(EmailAddress email, UserRole role, UserStatus status, int requesterId)
    {
        ArgumentNullException.ThrowIfNull(email);
        if (requesterId == Id)
        {
            if (role != Role) throw new RoleChangeNotAllowedException();
            if (status != Status) throw new StatusChangeNotAllowedException();
        }

        Email = email;
        Role = role;
        Status = status;
    }

    /// <summary>
    ///     Replaces the password hash.
    /// </summary>
    /// <param name="passwordHash">The new password hash.</param>
    public void ChangePasswordHash(string passwordHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);
        PasswordHash = passwordHash;
    }

    /// <summary>
    ///     Deactivates the account (logical delete). Calling it on an inactive account has no effect.
    /// </summary>
    /// <param name="requesterId">Identifier of the user that requests the deactivation.</param>
    /// <exception cref="CannotDeactivateSelfException">Thrown when the user tries to deactivate themselves.</exception>
    public void Deactivate(int requesterId)
    {
        if (requesterId == Id) throw new CannotDeactivateSelfException();
        Status = UserStatus.Inactive;
    }
}
