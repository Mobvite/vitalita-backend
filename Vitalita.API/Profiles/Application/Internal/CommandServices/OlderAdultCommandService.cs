using Microsoft.EntityFrameworkCore;
using Vitalita.API.Profiles.Application.Errors;
using Vitalita.API.Profiles.Application.Internal.Policies;
using Vitalita.API.Profiles.Application.Services;
using Vitalita.API.Profiles.Domain.Model.Aggregates;
using Vitalita.API.Profiles.Domain.Model.Commands;
using Vitalita.API.Profiles.Domain.Model.Exceptions;
using Vitalita.API.Profiles.Domain.Repositories;
using Vitalita.API.Shared.Application.Patterns;
using Vitalita.API.Shared.Domain.Repositories;
using Vitalita.API.Shared.Domain.Services;

namespace Vitalita.API.Profiles.Application.Internal.CommandServices;

/// <summary>
///     Application service that handles the <see cref="OlderAdult" /> commands.
/// </summary>
/// <remarks>
///     A caregiver registers older adults only in their own charge and edits only those. The plan limit is not
///     checked yet (it will come from the Subscriptions context).
/// </remarks>
/// <param name="olderAdultRepository">Older adult repository.</param>
/// <param name="caregiverProfileRepository">Caregiver profile repository.</param>
/// <param name="accessPolicy">Access rules of the Profiles context.</param>
/// <param name="clock">Clock used to get today in Peru.</param>
/// <param name="unitOfWork">Unit of work.</param>
/// <param name="logger">Logger.</param>
public class OlderAdultCommandService(
    IOlderAdultRepository olderAdultRepository,
    ICaregiverProfileRepository caregiverProfileRepository,
    ProfileAccessPolicy accessPolicy,
    IClock clock,
    IUnitOfWork unitOfWork,
    ILogger<OlderAdultCommandService> logger)
    : IOlderAdultCommandService
{
    /// <inheritdoc />
    public async Task<Result<OlderAdult, CreateOlderAdultError>> Handle(CreateOlderAdultCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.Requester.IsAdmin)
        {
            if (!await caregiverProfileRepository.ExistsByIdAsync(command.CaregiverId, cancellationToken))
                return Fail(CreateOlderAdultError.CaregiverNotFound);
        }
        else if (await accessPolicy.GetCaregiverProfileIdAsync(command.Requester, cancellationToken) !=
                 command.CaregiverId)
        {
            return Fail(CreateOlderAdultError.Forbidden);
        }

        OlderAdult olderAdult;
        try
        {
            olderAdult = new OlderAdult(command.CaregiverId, command.Data, clock.PeruToday());
        }
        catch (BirthDateInFutureException)
        {
            return Fail(CreateOlderAdultError.BirthDateInFuture);
        }

        try
        {
            await olderAdultRepository.AddAsync(olderAdult, cancellationToken);
            await unitOfWork.CompleteAsync(cancellationToken);
            logger.LogInformation("Older adult {OlderAdultId} registered for caregiver profile {CaregiverId}",
                olderAdult.Id, olderAdult.CaregiverId);
            return new Result<OlderAdult, CreateOlderAdultError>.Success(olderAdult);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database update failed while registering an older adult");
            return Fail(CreateOlderAdultError.UnexpectedError);
        }
    }

    /// <inheritdoc />
    public async Task<Result<OlderAdult, UpdateOlderAdultError>> Handle(UpdateOlderAdultCommand command,
        CancellationToken cancellationToken = default)
    {
        var olderAdult = await olderAdultRepository.FindByIdAsync(command.Id, cancellationToken);
        if (olderAdult is null) return Fail(UpdateOlderAdultError.NotFound);
        if (!await accessPolicy.CanManageOlderAdultAsync(command.Requester, olderAdult, cancellationToken))
            return Fail(UpdateOlderAdultError.Forbidden);

        try
        {
            olderAdult.Update(command.CaregiverId, command.Data, clock.PeruToday());
        }
        catch (OwnerChangeNotAllowedException)
        {
            return Fail(UpdateOlderAdultError.OwnerChangeNotAllowed);
        }
        catch (BirthDateInFutureException)
        {
            return Fail(UpdateOlderAdultError.BirthDateInFuture);
        }

        try
        {
            olderAdultRepository.Update(olderAdult);
            await unitOfWork.CompleteAsync(cancellationToken);
            return new Result<OlderAdult, UpdateOlderAdultError>.Success(olderAdult);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database update failed while updating older adult {OlderAdultId}", command.Id);
            return Fail(UpdateOlderAdultError.UnexpectedError);
        }
    }

    private static Result<OlderAdult, CreateOlderAdultError> Fail(CreateOlderAdultError error) =>
        new Result<OlderAdult, CreateOlderAdultError>.Failure(error);

    private static Result<OlderAdult, UpdateOlderAdultError> Fail(UpdateOlderAdultError error) =>
        new Result<OlderAdult, UpdateOlderAdultError>.Failure(error);
}
