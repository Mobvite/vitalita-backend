using Vitalita.API.Profiles.Application.Errors;
using Vitalita.API.Profiles.Domain.Model.Aggregates;
using Vitalita.API.Profiles.Domain.Model.Commands;
using Vitalita.API.Profiles.Domain.Model.Queries;
using Vitalita.API.Shared.Application.Patterns;

namespace Vitalita.API.Profiles.Application.Services;

/// <summary>
///     Command service of the <see cref="OlderAdult" /> aggregate.
/// </summary>
public interface IOlderAdultCommandService
{
    /// <summary>
    ///     Registers an older adult.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The created older adult or the error.</returns>
    Task<Result<OlderAdult, CreateOlderAdultError>> Handle(CreateOlderAdultCommand command,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Replaces the data of an older adult.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The updated older adult or the error.</returns>
    Task<Result<OlderAdult, UpdateOlderAdultError>> Handle(UpdateOlderAdultCommand command,
        CancellationToken cancellationToken = default);
}
