using System.Text;
using Vitalita.API.IAM.Domain.Model.ValueObjects;

namespace Vitalita.API.IAM.Domain.Model.Commands;

/// <summary>
///     Command to replace the data of an account.
/// </summary>
/// <param name="UserId">Identifier of the account to update.</param>
/// <param name="RequesterId">Identifier of the authenticated user that sends the request.</param>
/// <param name="RequesterIsAdmin">Whether the requester has the <see cref="UserRole.Admin" /> role.</param>
/// <param name="Email">The new email.</param>
/// <param name="Password">The new plain password, or <c>null</c> to keep the current one.</param>
/// <param name="Role">The new role.</param>
/// <param name="Status">The new status.</param>
public record UpdateUserCommand(
    int UserId,
    int RequesterId,
    bool RequesterIsAdmin,
    EmailAddress Email,
    string? Password,
    UserRole Role,
    UserStatus Status)
{
    /// <summary>
    ///     Prints the members without the password, so the command can be logged safely.
    /// </summary>
    protected virtual bool PrintMembers(StringBuilder builder)
    {
        builder.Append($"UserId = {UserId}, RequesterId = {RequesterId}, RequesterIsAdmin = {RequesterIsAdmin}, ");
        builder.Append($"Email = {Email}, PasswordChanged = {Password is not null}, Role = {Role}, Status = {Status}");
        return true;
    }
}
