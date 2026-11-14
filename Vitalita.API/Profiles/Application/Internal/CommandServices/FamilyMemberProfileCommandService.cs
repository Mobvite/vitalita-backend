using Microsoft.EntityFrameworkCore;
using Vitalita.API.IAM.Interfaces.ACL;
using Vitalita.API.Profiles.Application.Errors;
using Vitalita.API.Profiles.Application.Services;
using Vitalita.API.Profiles.Domain.Model.Aggregates;
using Vitalita.API.Profiles.Domain.Model.Commands;
using Vitalita.API.Profiles.Domain.Model.Exceptions;
using Vitalita.API.Profiles.Domain.Model.ValueObjects;
using Vitalita.API.Profiles.Domain.Repositories;
using Vitalita.API.Shared.Application.Patterns;
using Vitalita.API.Shared.Application.Persistence;
using Vitalita.API.Shared.Domain.Model.ValueObjects;
using Vitalita.API.Shared.Domain.Repositories;
using Vitalita.API.Shared.Domain.Services;

namespace Vitalita.API.Profiles.Application.Internal.CommandServices;

/// <summary>
///     Application service that handles the <see cref="FamilyMemberProfile" /> commands.
/// </summary>
/// <remarks>
///     The public sign-up does not check invitations, so this service does: a family member can only create their
///     profile when their email (JWT claim) has a pending, unexpired invitation. Administrators skip that check.
/// </remarks>
/// <param name="familyMemberProfileRepository">Family member profile repository.</param>
/// <param name="familyAccessRepository">Family access repository, used to check the invitation.</param>
/// <param name="iamContextFacade">IAM facade, used to check that the user exists.</param>
/// <param name="clock">Clock used to check that the invitation has not expired.</param>
/// <param name="unitOfWork">Unit of work.</param>
/// <param name="logger">Logger.</param>
public class FamilyMemberProfileCommandService(
    IFamilyMemberProfileRepository familyMemberProfileRepository,
    IFamilyAccessRepository familyAccessRepository,
    IIamContextFacade iamContextFacade,
    IClock clock,
    IUnitOfWork unitOfWork,
    ILogger<FamilyMemberProfileCommandService> logger)
    : IFamilyMemberProfileCommandService
{
    /// <inheritdoc />
    public async Task<Result<FamilyMemberProfile, CreateFamilyMemberProfileError>> Handle(
        CreateFamilyMemberProfileCommand command, CancellationToken cancellationToken = default)
    {
        var requester = command.Requester;
        if (!requester.IsAdmin)
        {
            if (!requester.IsFamilyMember || requester.UserId != command.UserId)
                return Fail(CreateFamilyMemberProfileError.Forbidden);

            var email = TextNormalizer.NormalizeEmail(requester.Email);
            if (email.Length == 0 ||
                !await familyAccessRepository.ExistsValidInvitationForEmailAsync(email, clock.UtcNow,
                    cancellationToken))
                return Fail(CreateFamilyMemberProfileError.NoValidInvitation);
        }

        if (!await iamContextFacade.ExistsUserByIdAsync(command.UserId, cancellationToken))
            return Fail(CreateFamilyMemberProfileError.UserNotFound);

        if (await familyMemberProfileRepository.ExistsByUserIdAsync(command.UserId, cancellationToken))
            return Fail(CreateFamilyMemberProfileError.DuplicateProfile);

        var profile = new FamilyMemberProfile(command.UserId, command.FirstName, command.LastName,
            command.PhoneNumber);
        try
        {
            await familyMemberProfileRepository.AddAsync(profile, cancellationToken);
            await unitOfWork.CompleteAsync(cancellationToken);
            logger.LogInformation("Family member profile {ProfileId} created for user {UserId}", profile.Id,
                profile.UserId);
            return new Result<FamilyMemberProfile, CreateFamilyMemberProfileError>.Success(profile);
        }
        catch (DbUpdateException ex) when (ex.IsDuplicateKeyViolation())
        {
            logger.LogWarning(ex, "Duplicate family member profile rejected by the unique index for user {UserId}",
                command.UserId);
            return Fail(CreateFamilyMemberProfileError.DuplicateProfile);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database update failed while creating a family member profile");
            return Fail(CreateFamilyMemberProfileError.UnexpectedError);
        }
    }

    /// <inheritdoc />
    public async Task<Result<FamilyMemberProfile, UpdateProfileError>> Handle(
        UpdateFamilyMemberProfileCommand command, CancellationToken cancellationToken = default)
    {
        var profile = await familyMemberProfileRepository.FindByIdAsync(command.Id, cancellationToken);
        if (profile is null) return Fail(UpdateProfileError.NotFound);
        if (!command.Requester.IsAdmin && profile.UserId != command.Requester.UserId)
            return Fail(UpdateProfileError.Forbidden);

        try
        {
            profile.Update(command.UserId, command.FirstName, command.LastName, command.PhoneNumber);
        }
        catch (OwnerChangeNotAllowedException)
        {
            return Fail(UpdateProfileError.OwnerChangeNotAllowed);
        }

        try
        {
            familyMemberProfileRepository.Update(profile);
            await unitOfWork.CompleteAsync(cancellationToken);
            return new Result<FamilyMemberProfile, UpdateProfileError>.Success(profile);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database update failed while updating family member profile {ProfileId}",
                command.Id);
            return Fail(UpdateProfileError.UnexpectedError);
        }
    }

    private static Result<FamilyMemberProfile, CreateFamilyMemberProfileError> Fail(
        CreateFamilyMemberProfileError error) =>
        new Result<FamilyMemberProfile, CreateFamilyMemberProfileError>.Failure(error);

    private static Result<FamilyMemberProfile, UpdateProfileError> Fail(UpdateProfileError error) =>
        new Result<FamilyMemberProfile, UpdateProfileError>.Failure(error);
}
