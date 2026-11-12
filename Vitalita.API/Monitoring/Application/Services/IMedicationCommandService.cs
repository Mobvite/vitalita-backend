using Vitalita.API.Monitoring.Application.Errors;
using Vitalita.API.Monitoring.Domain.Model.Aggregates;
using Vitalita.API.Monitoring.Domain.Model.Commands;
using Vitalita.API.Monitoring.Domain.Model.Queries;
using Vitalita.API.Shared.Application.Patterns;

namespace Vitalita.API.Monitoring.Application.Services;

/// <summary>
///     Command service of the <see cref="Medication" /> aggregate.
/// </summary>
public interface IMedicationCommandService
{
    /// <summary>
    ///     Registers a medication.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The created record or the error.</returns>
    Task<Result<Medication, MedicationCommandError>> Handle(CreateMedicationCommand command,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Replaces a medication (also to deactivate it).
    /// </summary>
    /// <param name="command">The command.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The updated record or the error.</returns>
    Task<Result<Medication, MedicationCommandError>> Handle(UpdateMedicationCommand command,
        CancellationToken cancellationToken = default);
}
