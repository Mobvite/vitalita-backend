using Vitalita.API.IAM.Application.Services;
using Vitalita.API.IAM.Domain.Model.Queries;

namespace Vitalita.API.IAM.Interfaces.ACL.Services;

/// <summary>
///     Implementation of <see cref="IIamContextFacade" /> on top of the IAM query service.
/// </summary>
/// <param name="userQueryService">The user query service.</param>
public class IamContextFacade(IUserQueryService userQueryService) : IIamContextFacade
{
    /// <inheritdoc />
    public async Task<bool> ExistsUserByIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        if (userId <= 0) return false;
        return await userQueryService.Handle(new GetUserByIdQuery(userId), cancellationToken) is not null;
    }
}
