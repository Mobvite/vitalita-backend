using Microsoft.Extensions.Logging.Abstractions;
using Vitalita.API.IAM.Application.Errors;
using Vitalita.API.IAM.Application.Internal.CommandServices;
using Vitalita.API.IAM.Application.Results;
using Vitalita.API.IAM.Domain.Model.Aggregates;
using Vitalita.API.IAM.Domain.Model.Commands;
using Vitalita.API.IAM.Domain.Model.ValueObjects;
using Vitalita.API.Shared.Application.Patterns;
using Vitalita.API.Tests.IAM.Fakes;

namespace Vitalita.API.Tests.IAM.Application.Internal.CommandServices;

public class UserCommandServiceTests
{
    private const string Password = "Vitalita123";
    private static readonly DateTime Now = new(2026, 11, 11, 15, 0, 0, DateTimeKind.Utc);

    private readonly InMemoryUserRepository _repository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly FakeHashingService _hashing = new();
    private readonly UserCommandService _service;

    public UserCommandServiceTests()
    {
        _service = new UserCommandService(_repository, _unitOfWork, _hashing, new FakeTokenService(),
            new FixedClock(Now), NullLogger<UserCommandService>.Instance);
    }

    private static EmailAddress Email(string value = "andrea.mendoza@vitalita.pe") => new(value);

    private async Task<User> AddUserAsync(string email, UserRole role = UserRole.Caregiver)
    {
        var user = new User(Email(email), _hashing.HashPassword(Password), role);
        await _repository.AddAsync(user);
        return user;
    }

    private static TError FailureOf<TValue, TError>(Result<TValue, TError> result) =>
        Assert.IsType<Result<TValue, TError>.Failure>(result).Error;

    private static TValue SuccessOf<TValue, TError>(Result<TValue, TError> result) =>
        Assert.IsType<Result<TValue, TError>.Success>(result).Value;

    // Sign-up

    [Fact]
    public async Task SignUp_StoresTheHashNeverThePlainPassword()
    {
        var user = SuccessOf(await _service.Handle(new SignUpCommand(Email(), Password, UserRole.Caregiver)));

        Assert.Equal(FakeHashingService.Prefix + Password, user.PasswordHash);
        Assert.NotEqual(Password, user.PasswordHash);
        Assert.Equal(UserStatus.Active, user.Status);
        Assert.Single(_repository.Users);
        Assert.Equal(1, _unitOfWork.CompleteCalls);
    }

    [Fact]
    public async Task SignUp_WithAdminRole_IsRejectedAndNothingIsSaved()
    {
        var result = await _service.Handle(new SignUpCommand(Email(), Password, UserRole.Admin));

        Assert.Equal(SignUpError.AdminRoleNotAllowed, FailureOf(result));
        Assert.Empty(_repository.Users);
        Assert.Equal(0, _unitOfWork.CompleteCalls);
    }

    [Fact]
    public async Task SignUp_WithExistingEmail_ReturnsDuplicateEmail()
    {
        await AddUserAsync("andrea.mendoza@vitalita.pe");

        var result = await _service.Handle(new SignUpCommand(Email("ANDREA.Mendoza@vitalita.pe"), Password,
            UserRole.FamilyMember));

        Assert.Equal(SignUpError.DuplicateEmail, FailureOf(result));
        Assert.Single(_repository.Users);
    }

    // Sign-in

    [Fact]
    public async Task SignIn_WithValidCredentials_ReturnsTokenAndRecordsTheLogin()
    {
        var user = await AddUserAsync("andrea.mendoza@vitalita.pe");

        var authenticated = SuccessOf(await _service.Handle(new SignInCommand(Email(), Password)));

        Assert.Same(user, authenticated.User);
        Assert.Equal($"token-for-{user.Id}", authenticated.Token);
        Assert.Equal(Now, user.LastLoginAt);
        Assert.Equal(1, _unitOfWork.CompleteCalls);
    }

    [Fact]
    public async Task SignIn_WithWrongPassword_ReturnsInvalidCredentials()
    {
        var user = await AddUserAsync("andrea.mendoza@vitalita.pe");

        var result = await _service.Handle(new SignInCommand(Email(), "Wrong12345"));

        Assert.Equal(SignInError.InvalidCredentials, FailureOf(result));
        Assert.Null(user.LastLoginAt);
    }

    [Fact]
    public async Task SignIn_WithUnknownEmail_StillVerifiesAndReturnsInvalidCredentials()
    {
        var result = await _service.Handle(new SignInCommand(Email("ghost@vitalita.pe"), Password));

        Assert.Equal(SignInError.InvalidCredentials, FailureOf(result));
        Assert.Equal(1, _hashing.VerifyCalls); // same work as a known email, so timing does not reveal it
    }

    [Fact]
    public async Task SignIn_InactiveUser_CannotSignIn()
    {
        var user = await AddUserAsync("andrea.mendoza@vitalita.pe");
        user.Deactivate(requesterId: 999);

        var result = await _service.Handle(new SignInCommand(Email(), Password));

        Assert.Equal(SignInError.InactiveAccount, FailureOf(result));
        Assert.Null(user.LastLoginAt);
        Assert.Equal(0, _unitOfWork.CompleteCalls);
    }

    // Create (admin)

    [Fact]
    public async Task CreateUser_CanCreateAnAdmin()
    {
        var user = SuccessOf(await _service.Handle(new CreateUserCommand(Email(), Password, UserRole.Admin)));

        Assert.Equal(UserRole.Admin, user.Role);
        Assert.Equal(FakeHashingService.Prefix + Password, user.PasswordHash);
    }

    [Fact]
    public async Task CreateUser_WithExistingEmail_ReturnsDuplicateEmail()
    {
        await AddUserAsync("andrea.mendoza@vitalita.pe");

        var result = await _service.Handle(new CreateUserCommand(Email(), Password, UserRole.Admin));

        Assert.Equal(CreateUserError.DuplicateEmail, FailureOf(result));
    }

    // Update

    [Fact]
    public async Task UpdateUser_Self_CannotChangeTheRole()
    {
        var user = await AddUserAsync("andrea.mendoza@vitalita.pe");

        var result = await _service.Handle(new UpdateUserCommand(user.Id, user.Id, false, user.Email, null,
            UserRole.Admin, UserStatus.Active));

        Assert.Equal(UpdateUserError.RoleChangeNotAllowed, FailureOf(result));
        Assert.Equal(UserRole.Caregiver, user.Role);
        Assert.Equal(0, _unitOfWork.CompleteCalls);
    }

    [Fact]
    public async Task UpdateUser_SelfAdmin_CannotChangeTheRoleEither()
    {
        var admin = await AddUserAsync("fixture.admin@vitalita.test", UserRole.Admin);

        var result = await _service.Handle(new UpdateUserCommand(admin.Id, admin.Id, true, admin.Email, null,
            UserRole.Caregiver, UserStatus.Active));

        Assert.Equal(UpdateUserError.RoleChangeNotAllowed, FailureOf(result));
        Assert.Equal(UserRole.Admin, admin.Role);
    }

    [Fact]
    public async Task UpdateUser_Self_CannotChangeTheStatus()
    {
        var user = await AddUserAsync("andrea.mendoza@vitalita.pe");

        var result = await _service.Handle(new UpdateUserCommand(user.Id, user.Id, false, user.Email, null,
            UserRole.Caregiver, UserStatus.Inactive));

        Assert.Equal(UpdateUserError.StatusChangeNotAllowed, FailureOf(result));
        Assert.Equal(UserStatus.Active, user.Status);
    }

    [Fact]
    public async Task UpdateUser_Self_CanChangeEmailAndPassword()
    {
        var user = await AddUserAsync("andrea.mendoza@vitalita.pe");

        var updated = SuccessOf(await _service.Handle(new UpdateUserCommand(user.Id, user.Id, false,
            Email("andrea.new@vitalita.pe"), "NewPassword1", UserRole.Caregiver, UserStatus.Active)));

        Assert.Equal("andrea.new@vitalita.pe", updated.Email.Value);
        Assert.Equal(FakeHashingService.Prefix + "NewPassword1", updated.PasswordHash);
        Assert.Equal(1, _unitOfWork.CompleteCalls);
    }

    [Fact]
    public async Task UpdateUser_WithoutPassword_KeepsTheCurrentHash()
    {
        var user = await AddUserAsync("andrea.mendoza@vitalita.pe");
        var hash = user.PasswordHash;

        SuccessOf(await _service.Handle(new UpdateUserCommand(user.Id, user.Id, false, user.Email, null,
            UserRole.Caregiver, UserStatus.Active)));

        Assert.Equal(hash, user.PasswordHash);
    }

    [Fact]
    public async Task UpdateUser_AnotherUserWithoutBeingAdmin_IsForbidden()
    {
        var user = await AddUserAsync("andrea.mendoza@vitalita.pe");
        var other = await AddUserAsync("renzo.rodriguez@vitalita.pe", UserRole.FamilyMember);

        var result = await _service.Handle(new UpdateUserCommand(other.Id, user.Id, false, other.Email, null,
            UserRole.FamilyMember, UserStatus.Inactive));

        Assert.Equal(UpdateUserError.Forbidden, FailureOf(result));
        Assert.Equal(UserStatus.Active, other.Status);
    }

    [Fact]
    public async Task UpdateUser_UnknownIdWithoutBeingAdmin_IsForbiddenNotNotFound()
    {
        var user = await AddUserAsync("andrea.mendoza@vitalita.pe");

        var result = await _service.Handle(new UpdateUserCommand(999, user.Id, false, user.Email, null,
            UserRole.Caregiver, UserStatus.Active));

        Assert.Equal(UpdateUserError.Forbidden, FailureOf(result));
    }

    [Fact]
    public async Task UpdateUser_ByAdmin_CanChangeRoleAndStatusOfAnotherUser()
    {
        var admin = await AddUserAsync("fixture.admin@vitalita.test", UserRole.Admin);
        var user = await AddUserAsync("andrea.mendoza@vitalita.pe");

        SuccessOf(await _service.Handle(new UpdateUserCommand(user.Id, admin.Id, true, user.Email, null,
            UserRole.FamilyMember, UserStatus.Inactive)));

        Assert.Equal(UserRole.FamilyMember, user.Role);
        Assert.Equal(UserStatus.Inactive, user.Status);
    }

    [Fact]
    public async Task UpdateUser_ByAdmin_UnknownId_ReturnsNotFound()
    {
        var admin = await AddUserAsync("fixture.admin@vitalita.test", UserRole.Admin);

        var result = await _service.Handle(new UpdateUserCommand(999, admin.Id, true, Email(), null,
            UserRole.Caregiver, UserStatus.Active));

        Assert.Equal(UpdateUserError.NotFound, FailureOf(result));
    }

    [Fact]
    public async Task UpdateUser_WithEmailOfAnotherUser_ReturnsDuplicateEmail()
    {
        var admin = await AddUserAsync("fixture.admin@vitalita.test", UserRole.Admin);
        var user = await AddUserAsync("andrea.mendoza@vitalita.pe");

        var result = await _service.Handle(new UpdateUserCommand(user.Id, admin.Id, true, admin.Email, null,
            UserRole.Caregiver, UserStatus.Active));

        Assert.Equal(UpdateUserError.DuplicateEmail, FailureOf(result));
    }

    // Deactivate

    [Fact]
    public async Task DeactivateUser_SetsInactiveAndDoesNotRemoveTheRow()
    {
        var admin = await AddUserAsync("fixture.admin@vitalita.test", UserRole.Admin);
        var user = await AddUserAsync("andrea.mendoza@vitalita.pe");

        SuccessOf(await _service.Handle(new DeactivateUserCommand(user.Id, admin.Id)));

        Assert.Equal(UserStatus.Inactive, user.Status);
        Assert.Contains(user, _repository.Users);
        Assert.Equal(0, _repository.RemoveCalls);
        Assert.Equal(1, _unitOfWork.CompleteCalls);
    }

    [Fact]
    public async Task DeactivateUser_ThenSignIn_IsRejected()
    {
        var admin = await AddUserAsync("fixture.admin@vitalita.test", UserRole.Admin);
        var user = await AddUserAsync("andrea.mendoza@vitalita.pe");
        await _service.Handle(new DeactivateUserCommand(user.Id, admin.Id));

        var result = await _service.Handle(new SignInCommand(user.Email, Password));

        Assert.Equal(SignInError.InactiveAccount, FailureOf(result));
    }

    [Fact]
    public async Task DeactivateUser_Self_ReturnsCannotDeactivateSelf()
    {
        var admin = await AddUserAsync("fixture.admin@vitalita.test", UserRole.Admin);

        var result = await _service.Handle(new DeactivateUserCommand(admin.Id, admin.Id));

        Assert.Equal(DeactivateUserError.CannotDeactivateSelf, FailureOf(result));
        Assert.True(admin.IsActive);
    }

    [Fact]
    public async Task DeactivateUser_UnknownId_ReturnsNotFound()
    {
        var result = await _service.Handle(new DeactivateUserCommand(999, 1));

        Assert.Equal(DeactivateUserError.NotFound, FailureOf(result));
    }

    // Seed admin

    [Fact]
    public async Task SeedAdmin_CreatesAnAdministrator()
    {
        var admin = SuccessOf(await _service.Handle(new SeedAdminCommand(Email("fixture.admin@vitalita.test"), Password)));

        Assert.Equal(UserRole.Admin, admin.Role);
        Assert.Single(_repository.Users);
    }

    [Fact]
    public async Task SeedAdmin_WithExistingEmail_LeavesTheUserUnchanged()
    {
        var existing = await AddUserAsync("fixture.admin@vitalita.test");
        var hash = existing.PasswordHash;

        var result = await _service.Handle(new SeedAdminCommand(Email("fixture.admin@vitalita.test"), "Different123"));

        Assert.Equal(SeedAdminError.EmailAlreadyRegistered, FailureOf(result));
        Assert.Equal(UserRole.Caregiver, existing.Role);
        Assert.Equal(hash, existing.PasswordHash);
        Assert.Single(_repository.Users);
    }
}
