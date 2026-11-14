using Vitalita.API.IAM.Application.Errors;
using Vitalita.API.IAM.Application.Internal.QueryServices;
using Vitalita.API.IAM.Domain.Model.Aggregates;
using Vitalita.API.IAM.Domain.Model.Queries;
using Vitalita.API.Shared.Application.Patterns;
using Vitalita.API.Tests.IAM.Fakes;

namespace Vitalita.API.Tests.IAM.Application.Internal.QueryServices;

public class UserQueryServiceTests
{
    private readonly InMemoryUserRepository _repository = new();
    private readonly UserQueryService _service;

    public UserQueryServiceTests()
    {
        _service = new UserQueryService(_repository);
    }

    [Fact]
    public async Task GetCurrentUser_ForActiveUser_ReturnsTheUser()
    {
        var user = UserBuilder.Build(0);
        await _repository.AddAsync(user);

        var result = await _service.Handle(new GetCurrentUserQuery(user.Id));

        Assert.Same(user, Assert.IsType<Result<User, CurrentUserError>.Success>(result).Value);
    }

    [Fact]
    public async Task GetCurrentUser_ForInactiveUser_ReturnsInactiveAccount()
    {
        var user = UserBuilder.Build(0);
        await _repository.AddAsync(user);
        user.Deactivate(requesterId: 999);

        var result = await _service.Handle(new GetCurrentUserQuery(user.Id));

        Assert.Equal(CurrentUserError.InactiveAccount,
            Assert.IsType<Result<User, CurrentUserError>.Failure>(result).Error);
    }

    [Fact]
    public async Task GetCurrentUser_ForMissingUser_ReturnsUserNotFound()
    {
        var result = await _service.Handle(new GetCurrentUserQuery(42));

        Assert.Equal(CurrentUserError.UserNotFound,
            Assert.IsType<Result<User, CurrentUserError>.Failure>(result).Error);
    }
}
