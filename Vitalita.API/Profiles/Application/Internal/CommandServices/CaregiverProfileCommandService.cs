using Microsoft.EntityFrameworkCore;
using Vitalita.API.IAM.Interfaces.ACL;
using Vitalita.API.Profiles.Application.Errors;
using Vitalita.API.Profiles.Application.Services;
using Vitalita.API.Profiles.Domain.Model.Aggregates;
using Vitalita.API.Profiles.Domain.Model.Commands;
using Vitalita.API.Profiles.Domain.Model.Exceptions;
using Vitalita.API.Profiles.Domain.Repositories;
using Vitalita.API.Shared.Application.Patterns;
using Vitalita.API.Shared.Application.Persistence;
using Vitalita.API.Shared.Domain.Model.ValueObjects;
using Vitalita.API.Shared.Domain.Repositories;

namespace Vitalita.API.Profiles.Application.Internal.CommandServices;

/// <summary>
///     Application service that handles the <see cref="CaregiverProfile" /> commands.
/// </summary>
/// <remarks>
///     A caregiver creates and edits only their own profile; administrators can do it for any existing user. One
///     profile per user, also enforced by the unique index on <c>user_id</c>.
/// </remarks>
/// <param name="caregiverProfileRepository">Caregiver profile repository.</param>
/// <param name="iamContextFacade">IAM facade, used to check that the user exists.</param>
/// <param name="unitOfWork">Unit of work.</param>
/// <param name="logger">Logger.</param>
public class CaregiverProfileCommandService(
    ICaregiverProfileRepository caregiverProfileRepository,
    IIamContextFacade iamContextFacade,
    IUnitOfWork unitOfWork,
    ILogger<CaregiverProfileCommandService> logger)
    : ICaregiverProfileCommandService
{
    /// <inheritdoc />
    public async Task<Result<CaregiverProfile, CreateCaregiverProfileError>> Handle(
        CreateCaregiverProfileCommand command, CancellationToken cancellationToken = default)
    {
        var requester = command.Requester;
        if (!requester.IsAdmin && (!requester.IsCaregiver || requester.UserId != command.UserId))
            return Fail(CreateCaregiverProfileError.Forbidden);

        if (!await iamContextFacade.ExistsUserByIdAsync(command.UserId, cancellationToken))
            return Fail(CreateCaregiverProfileError.UserNotFound);

        if (await caregiverProfileRepository.ExistsByUserIdAsync(command.UserId, cancellationToken))
            return Fail(CreateCaregiverProfileError.DuplicateProfile);

        var profile = new CaregiverProfile(command.UserId, command.FirstName, command.LastName, command.PhoneNumber,
            command.ProfessionalTitle, command.Bio, command.PhotoUrl, command.Certifications);
        try
        {
            await caregiverProfileRepository.AddAsync(profile, cancellationToken);
            await unitOfWork.CompleteAsync(cancellationToken);
            logger.LogInformation("Caregiver profile {ProfileId} created for user {UserId}", profile.Id,
                profile.UserId);
            return new Result<CaregiverProfile, CreateCaregiverProfileError>.Success(profile);
        }
        catch (DbUpdateException ex) when (ex.IsDuplicateKeyViolation())
        {
            logger.LogWarning(ex, "Duplicate caregiver profile rejected by the unique index for user {UserId}",
                command.UserId);
            return Fail(CreateCaregiverProfileError.DuplicateProfile);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database update failed while creating a caregiver profile");
            return Fail(CreateCaregiverProfileError.UnexpectedError);
        }
    }

    /// <inheritdoc />
    public async Task<Result<CaregiverProfile, UpdateProfileError>> Handle(UpdateCaregiverProfileCommand command,
        CancellationToken cancellationToken = default)
    {
        var profile = await caregiverProfileRepository.FindByIdAsync(command.Id, cancellationToken);
        if (profile is null) return Fail(UpdateProfileError.NotFound);
        if (!command.Requester.IsAdmin && profile.UserId != command.Requester.UserId)
            return Fail(UpdateProfileError.Forbidden);

        try
        {
            profile.Update(command.UserId, command.FirstName, command.LastName, command.PhoneNumber,
                command.ProfessionalTitle, command.Bio, command.PhotoUrl, command.Certifications);
        }
        catch (OwnerChangeNotAllowedException)
        {
            return Fail(UpdateProfileError.OwnerChangeNotAllowed);
        }

        try
        {
            caregiverProfileRepository.Update(profile);
            await unitOfWork.CompleteAsync(cancellationToken);
            return new Result<CaregiverProfile, UpdateProfileError>.Success(profile);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database update failed while updating caregiver profile {ProfileId}", command.Id);
            return Fail(UpdateProfileError.UnexpectedError);
        }
    }

    private static Result<CaregiverProfile, CreateCaregiverProfileError> Fail(CreateCaregiverProfileError error) =>
        new Result<CaregiverProfile, CreateCaregiverProfileError>.Failure(error);

    private static Result<CaregiverProfile, UpdateProfileError> Fail(UpdateProfileError error) =>
        new Result<CaregiverProfile, UpdateProfileError>.Failure(error);
}
