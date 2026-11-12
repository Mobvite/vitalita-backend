using Vitalita.API.Profiles.Application.Errors;
using Vitalita.API.Profiles.Domain.Model.Aggregates;
using Vitalita.API.Profiles.Domain.Model.Commands;
using Vitalita.API.Profiles.Domain.Model.Queries;
using Vitalita.API.Shared.Application.Patterns;

namespace Vitalita.API.Profiles.Application.Services;

/// <summary>
///     Command service of the <see cref="FamilyMemberProfile" /> aggregate.
/// </summary>
public interface IFamilyMemberProfileCommandService
{
    /// <summary>
    ///     Creates the profile of a family member.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The created profile or the error.</returns>
    Task<Result<FamilyMemberProfile, CreateFamilyMemberProfileError>> Handle(CreateFamilyMemberProfileCommand command,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Replaces the data of a family member profile.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The updated profile or the error.</returns>
    Task<Result<FamilyMemberProfile, UpdateProfileError>> Handle(UpdateFamilyMemberProfileCommand command,
        CancellationToken cancellationToken = default);
}
