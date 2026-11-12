using Vitalita.API.IAM.Domain.Model.Aggregates;
using Vitalita.API.IAM.Domain.Model.Exceptions;
using Vitalita.API.IAM.Domain.Model.ValueObjects;
using Vitalita.API.Tests.IAM.Fakes;

namespace Vitalita.API.Tests.IAM.Domain.Model.Aggregates;

public class UserTests
{
    private static readonly EmailAddress Email = new("andrea.mendoza@vitalita.pe");

    [Fact]
    public void Constructor_CreatesAnActiveUserWithoutLastLogin()
    {
        var user = new User(Email, "hash", UserRole.Caregiver);

        Assert.Equal(UserStatus.Active, user.Status);
        Assert.True(user.IsActive);
        Assert.Null(user.LastLoginAt);
    }

    [Fact]
    public void Constructor_RejectsAnEmptyHash()
    {
        Assert.Throws<ArgumentException>(() => new User(Email, " ", UserRole.Caregiver));
    }

    [Theory]
    [InlineData(UserRole.Caregiver, true)]
    [InlineData(UserRole.FamilyMember, true)]
    [InlineData(UserRole.Admin, false)]
    public void CanSelfRegisterAs_RejectsOnlyAdmin(UserRole role, bool expected)
    {
        Assert.Equal(expected, User.CanSelfRegisterAs(role));
    }

    [Fact]
    public void RecordSignIn_SetsTheUtcInstant()
    {
        var user = UserBuilder.Build(1);
        var now = new DateTime(2026, 11, 11, 15, 0, 0, DateTimeKind.Utc);

        user.RecordSignIn(now);

        Assert.Equal(now, user.LastLoginAt);
        Assert.Equal(DateTimeKind.Utc, user.LastLoginAt!.Value.Kind);
    }

    [Fact]
    public void RecordSignIn_OnInactiveUser_Throws()
    {
        var user = UserBuilder.Build(1);
        user.Deactivate(requesterId: 2);

        Assert.Throws<InvalidOperationException>(() => user.RecordSignIn(DateTime.UtcNow));
    }

    [Fact]
    public void Deactivate_ChangesTheStatusToInactive()
    {
        var user = UserBuilder.Build(1);

        user.Deactivate(requesterId: 2);

        Assert.Equal(UserStatus.Inactive, user.Status);
        Assert.False(user.IsActive);
    }

    [Fact]
    public void Deactivate_Self_Throws()
    {
        var user = UserBuilder.Build(1, role: UserRole.Admin);

        Assert.Throws<CannotDeactivateSelfException>(() => user.Deactivate(requesterId: 1));
        Assert.Equal(UserStatus.Active, user.Status);
    }

    [Fact]
    public void Update_Self_CannotChangeTheRole()
    {
        var user = UserBuilder.Build(1);

        Assert.Throws<RoleChangeNotAllowedException>(() =>
            user.Update(Email, UserRole.Admin, UserStatus.Active, requesterId: 1));
        Assert.Equal(UserRole.Caregiver, user.Role);
    }

    [Fact]
    public void Update_Self_CannotChangeTheStatus()
    {
        var user = UserBuilder.Build(1);

        Assert.Throws<StatusChangeNotAllowedException>(() =>
            user.Update(Email, UserRole.Caregiver, UserStatus.Inactive, requesterId: 1));
        Assert.Equal(UserStatus.Active, user.Status);
    }

    [Fact]
    public void Update_Self_CanChangeTheEmail()
    {
        var user = UserBuilder.Build(1);
        var newEmail = new EmailAddress("andrea.new@vitalita.pe");

        user.Update(newEmail, UserRole.Caregiver, UserStatus.Active, requesterId: 1);

        Assert.Equal(newEmail, user.Email);
    }

    [Fact]
    public void Update_ByAnotherUser_CanChangeRoleAndStatus()
    {
        var user = UserBuilder.Build(1);

        user.Update(Email, UserRole.FamilyMember, UserStatus.Inactive, requesterId: 99);

        Assert.Equal(UserRole.FamilyMember, user.Role);
        Assert.Equal(UserStatus.Inactive, user.Status);
    }
}
