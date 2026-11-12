using System.Text;
using Vitalita.API.IAM.Domain.Model.ValueObjects;

namespace Vitalita.API.IAM.Domain.Model.Commands;

/// <summary>
///     Command to create the first administrator from configuration at startup.
/// </summary>
/// <param name="Email">The administrator email.</param>
/// <param name="Password">The plain password. It is hashed and never stored or logged.</param>
public record SeedAdminCommand(EmailAddress Email, string Password)
{
    /// <summary>
    ///     Prints the members without the password, so the command can be logged safely.
    /// </summary>
    protected virtual bool PrintMembers(StringBuilder builder)
    {
        builder.Append($"Email = {Email}");
        return true;
    }
}
