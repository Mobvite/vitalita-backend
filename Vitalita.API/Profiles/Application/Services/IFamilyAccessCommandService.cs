using Vitalita.API.Profiles.Application.Errors;
using Vitalita.API.Profiles.Domain.Model.Aggregates;
using Vitalita.API.Profiles.Domain.Model.Commands;
using Vitalita.API.Profiles.Domain.Model.Queries;
using Vitalita.API.Shared.Application.Patterns;

namespace Vitalita.API.Profiles.Application.Services;

/// <summary>
///     Command service of the <see cref="FamilyAccess" /> aggregate.
/// </summary>
public interface IFamilyAccessCommandService
{
    /// <summary>
    ///     Invites a family member; the server generates the code and the dates.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The pending access or the error.</returns>
    Task<Result<FamilyAccess, CreateFamilyAccessError>> Handle(CreateFamilyAccessCommand command,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Accepts, revokes or changes the relationship of a family access.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The updated access or the error.</returns>
    Task<Result<FamilyAccess, UpdateFamilyAccessError>> Handle(UpdateFamilyAccessCommand command,
        CancellationToken cancellationToken = default);
}
