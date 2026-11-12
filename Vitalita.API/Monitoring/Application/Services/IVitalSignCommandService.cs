using Vitalita.API.Monitoring.Application.Errors;
using Vitalita.API.Monitoring.Domain.Model.Aggregates;
using Vitalita.API.Monitoring.Domain.Model.Commands;
using Vitalita.API.Monitoring.Domain.Model.Queries;
using Vitalita.API.Shared.Application.Patterns;

namespace Vitalita.API.Monitoring.Application.Services;

/// <summary>
///     Command service of the <see cref="VitalSign" /> aggregate.
/// </summary>
public interface IVitalSignCommandService
{
    /// <summary>
    ///     Records a vital sign.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The created record or the error.</returns>
    Task<Result<VitalSign, VitalSignCommandError>> Handle(CreateVitalSignCommand command,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Replaces a vital sign.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The updated record or the error.</returns>
    Task<Result<VitalSign, VitalSignCommandError>> Handle(UpdateVitalSignCommand command,
        CancellationToken cancellationToken = default);
}
