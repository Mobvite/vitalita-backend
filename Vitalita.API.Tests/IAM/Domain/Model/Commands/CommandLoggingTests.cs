using Vitalita.API.IAM.Domain.Model.Commands;
using Vitalita.API.IAM.Domain.Model.ValueObjects;

namespace Vitalita.API.Tests.IAM.Domain.Model.Commands;

public class CommandLoggingTests
{
    private const string Password = "SuperSecret123";
    private static readonly EmailAddress Email = new("andrea.mendoza@vitalita.pe");

    public static TheoryData<object> CommandsWithPassword() =>
    [
        new SignUpCommand(Email, Password, UserRole.Caregiver),
        new SignInCommand(Email, Password),
        new CreateUserCommand(Email, Password, UserRole.Admin),
        new UpdateUserCommand(1, 1, false, Email, Password, UserRole.Caregiver, UserStatus.Active),
        new SeedAdminCommand(Email, Password)
    ];

    [Theory]
    [MemberData(nameof(CommandsWithPassword))]
    public void ToString_NeverContainsThePassword(object command)
    {
        var text = command.ToString()!;

        Assert.DoesNotContain(Password, text);
        Assert.Contains(Email.Value, text);
    }
}
