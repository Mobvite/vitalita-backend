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
using Vitalita.API.Monitoring.Domain.Model.ValueObjects;

namespace Vitalita.API.Monitoring.Application.Internal.CommandServices;

/// <summary>
///     Application service that handles the <see cref="VitalSign" /> commands.
/// </summary>
/// <remarks>Only the caregiver in charge (or an administrator) records measurements.</remarks>
/// <param name="vitalSignRepository">Vital sign repository.</param>
/// <param name="accessPolicy">Access rules for the records of an older adult.</param>
/// <param name="clock">Clock used to reject instants in the future.</param>
/// <param name="unitOfWork">Unit of work.</param>
/// <param name="logger">Logger.</param>
public class VitalSignCommandService(
    IVitalSignRepository vitalSignRepository,
    OlderAdultAccessPolicy accessPolicy,
    IClock clock,
    IUnitOfWork unitOfWork,
    ILogger<VitalSignCommandService> logger)
    : IVitalSignCommandService
{
    /// <inheritdoc />
    public async Task<Result<VitalSign, VitalSignCommandError>> Handle(CreateVitalSignCommand command,
        CancellationToken cancellationToken = default)
    {
        switch (await accessPolicy.CheckNewRecordAsync(command.Requester, command.OlderAdultId, cancellationToken))
        {
            case NewRecordAccess.Forbidden: return Fail(VitalSignCommandError.Forbidden);
            case NewRecordAccess.OlderAdultNotFound: return Fail(VitalSignCommandError.OlderAdultNotFound);
        }

        VitalSign sign;
        try
        {
            sign = new VitalSign(command.OlderAdultId, command.Type, command.Value, command.SecondaryValue,
                command.MeasuredAt, clock.UtcNow);
        }
        catch (InvalidVitalSignException ex)
        {
            return Fail(ToCommandError(ex.Error));
        }
        catch (InstantInFutureException)
        {
            return Fail(VitalSignCommandError.InstantInFuture);
        }

        try
        {
            await vitalSignRepository.AddAsync(sign, cancellationToken);
            await unitOfWork.CompleteAsync(cancellationToken);
            logger.LogInformation("Vital sign {VitalSignId} ({Type}) recorded for older adult {OlderAdultId}", sign.Id,
                sign.Type, sign.OlderAdultId);
            return new Result<VitalSign, VitalSignCommandError>.Success(sign);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database update failed while recording a vital sign");
            return Fail(VitalSignCommandError.UnexpectedError);
        }
    }

    /// <inheritdoc />
    public async Task<Result<VitalSign, VitalSignCommandError>> Handle(UpdateVitalSignCommand command,
        CancellationToken cancellationToken = default)
    {
        var sign = await vitalSignRepository.FindByIdAsync(command.Id, cancellationToken);
        if (sign is null) return Fail(VitalSignCommandError.NotFound);
        if (!await accessPolicy.CanWriteAsync(command.Requester, sign.OlderAdultId, cancellationToken))
            return Fail(VitalSignCommandError.Forbidden);

        try
        {
            sign.Update(command.OlderAdultId, command.Type, command.Value, command.SecondaryValue, command.MeasuredAt,
                clock.UtcNow);
        }
        catch (OlderAdultChangeNotAllowedException)
        {
            return Fail(VitalSignCommandError.OlderAdultChangeNotAllowed);
        }
        catch (InvalidVitalSignException ex)
        {
            return Fail(ToCommandError(ex.Error));
        }
        catch (InstantInFutureException)
        {
            return Fail(VitalSignCommandError.InstantInFuture);
        }

        try
        {
            vitalSignRepository.Update(sign);
            await unitOfWork.CompleteAsync(cancellationToken);
            return new Result<VitalSign, VitalSignCommandError>.Success(sign);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database update failed while updating vital sign {VitalSignId}", command.Id);
            return Fail(VitalSignCommandError.UnexpectedError);
        }
    }

    private static VitalSignCommandError ToCommandError(VitalSignValidationError error) => error switch
    {
        VitalSignValidationError.ValueOutOfRange => VitalSignCommandError.ValueOutOfRange,
        VitalSignValidationError.SecondaryValueRequired => VitalSignCommandError.SecondaryValueRequired,
        VitalSignValidationError.SecondaryValueOutOfRange => VitalSignCommandError.SecondaryValueOutOfRange,
        VitalSignValidationError.SecondaryValueNotAllowed => VitalSignCommandError.SecondaryValueNotAllowed,
        _ => VitalSignCommandError.SystolicNotGreaterThanDiastolic
    };

    private static Result<VitalSign, VitalSignCommandError> Fail(VitalSignCommandError error) =>
        new Result<VitalSign, VitalSignCommandError>.Failure(error);
}
