using System.Text;
using Vitalita.API.IAM.Domain.Model.ValueObjects;

namespace Vitalita.API.IAM.Domain.Model.Commands;

/// <summary>
///     Command to register an account through the public sign-up.
/// </summary>
/// <param name="Email">The account email.</param>
/// <param name="Password">The plain password. It is hashed and never stored or logged.</param>
/// <param name="Role">The requested role. <see cref="UserRole.Admin" /> is rejected.</param>
public record SignUpCommand(EmailAddress Email, string Password, UserRole Role)
{
    /// <summary>
    ///     Prints the members without the password, so the command can be logged safely.
    /// </summary>
    protected virtual bool PrintMembers(StringBuilder builder)
    {
        builder.Append($"Email = {Email}, Role = {Role}");
        return true;
    }
}
