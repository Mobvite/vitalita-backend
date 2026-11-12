using Vitalita.API.Profiles.Application.Errors;
using Vitalita.API.Profiles.Domain.Model.Aggregates;
using Vitalita.API.Profiles.Domain.Model.Commands;
using Vitalita.API.Profiles.Domain.Model.Queries;
using Vitalita.API.Shared.Application.Patterns;

namespace Vitalita.API.Profiles.Application.Services;

/// <summary>
///     Query service of the <see cref="CaregiverProfile" /> aggregate.
/// </summary>
public interface ICaregiverProfileQueryService
{
    /// <summary>
    ///     Lists every caregiver profile (administrators only).
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The profiles, or Forbidden.</returns>
    Task<Result<IReadOnlyList<CaregiverProfile>, ProfileQueryError>> Handle(GetAllCaregiverProfilesQuery query,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets the caregiver profile of a user (zero or one element).
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The profiles, or Forbidden when the user is another one.</returns>
    Task<Result<IReadOnlyList<CaregiverProfile>, ProfileQueryError>> Handle(GetCaregiverProfilesByUserIdQuery query,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets a caregiver profile by identifier.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The profile, NotFound or Forbidden.</returns>
    Task<Result<CaregiverProfile, ProfileQueryError>> Handle(GetCaregiverProfileByIdQuery query,
        CancellationToken cancellationToken = default);
}
