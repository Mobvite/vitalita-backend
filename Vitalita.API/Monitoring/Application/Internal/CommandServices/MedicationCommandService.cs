using Microsoft.EntityFrameworkCore;
using Vitalita.API.Monitoring.Application.Errors;
using Vitalita.API.Monitoring.Application.Internal.Policies;
using Vitalita.API.Monitoring.Application.Services;
using Vitalita.API.Monitoring.Domain.Model.Aggregates;
using Vitalita.API.Monitoring.Domain.Model.Commands;
using Vitalita.API.Monitoring.Domain.Model.Exceptions;
using Vitalita.API.Monitoring.Domain.Repositories;
using Vitalita.API.Shared.Application.Patterns;
using Vitalita.API.Shared.Domain.Repositories;
using Vitalita.API.Shared.Domain.Services;

namespace Vitalita.API.Monitoring.Application.Internal.CommandServices;

/// <summary>
///     Application service that handles the <see cref="Medication" /> commands.
/// </summary>
/// <remarks>
///     Only the caregiver in charge (or an administrator) registers and edits medications. There is no delete: a
///     medication is deactivated with an update (<c>active = false</c>).
/// </remarks>
/// <param name="medicationRepository">Medication repository.</param>
/// <param name="accessPolicy">Access rules for the records of an older adult.</param>
/// <param name="unitOfWork">Unit of work.</param>
/// <param name="logger">Logger.</param>
public class MedicationCommandService(
    IMedicationRepository medicationRepository,
    OlderAdultAccessPolicy accessPolicy,
    IUnitOfWork unitOfWork,
    ILogger<MedicationCommandService> logger)
    : IMedicationCommandService
{
    /// <inheritdoc />
    public async Task<Result<Medication, MedicationCommandError>> Handle(CreateMedicationCommand command,
        CancellationToken cancellationToken = default)
    {
        switch (await accessPolicy.CheckNewRecordAsync(command.Requester, command.OlderAdultId, cancellationToken))
        {
            case NewRecordAccess.Forbidden: return Fail(MedicationCommandError.Forbidden);
            case NewRecordAccess.OlderAdultNotFound: return Fail(MedicationCommandError.OlderAdultNotFound);
        }

        Medication medication;
        try
        {
            medication = new Medication(command.OlderAdultId, command.Data);
        }
        catch (InvalidScheduleTimesException)
        {
            return Fail(MedicationCommandError.InvalidScheduleTimes);
        }

        try
        {
            await medicationRepository.AddAsync(medication, cancellationToken);
            await unitOfWork.CompleteAsync(cancellationToken);
            logger.LogInformation("Medication {MedicationId} registered for older adult {OlderAdultId}", medication.Id,
                medication.OlderAdultId);
            return new Result<Medication, MedicationCommandError>.Success(medication);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database update failed while registering a medication");
            return Fail(MedicationCommandError.UnexpectedError);
        }
    }

    /// <inheritdoc />
    public async Task<Result<Medication, MedicationCommandError>> Handle(UpdateMedicationCommand command,
        CancellationToken cancellationToken = default)
    {
        var medication = await medicationRepository.FindByIdAsync(command.Id, cancellationToken);
        if (medication is null) return Fail(MedicationCommandError.NotFound);
        if (!await accessPolicy.CanWriteAsync(command.Requester, medication.OlderAdultId, cancellationToken))
            return Fail(MedicationCommandError.Forbidden);

        try
        {
            medication.Update(command.OlderAdultId, command.Data);
        }
        catch (OlderAdultChangeNotAllowedException)
        {
            return Fail(MedicationCommandError.OlderAdultChangeNotAllowed);
        }
        catch (InvalidScheduleTimesException)
        {
            return Fail(MedicationCommandError.InvalidScheduleTimes);
        }

        try
        {
            medicationRepository.Update(medication);
            await unitOfWork.CompleteAsync(cancellationToken);
            return new Result<Medication, MedicationCommandError>.Success(medication);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database update failed while updating medication {MedicationId}", command.Id);
            return Fail(MedicationCommandError.UnexpectedError);
        }
    }

    private static Result<Medication, MedicationCommandError> Fail(MedicationCommandError error) =>
        new Result<Medication, MedicationCommandError>.Failure(error);
}
