using Microsoft.EntityFrameworkCore;
using Vitalita.API.Profiles.Application.Errors;
using Vitalita.API.Profiles.Application.Internal.Policies;
using Vitalita.API.Profiles.Application.Services;
using Vitalita.API.Profiles.Domain.Model.Aggregates;
using Vitalita.API.Profiles.Domain.Model.Commands;
using Vitalita.API.Profiles.Domain.Model.Exceptions;
using Vitalita.API.Profiles.Domain.Model.ValueObjects;
using Vitalita.API.Profiles.Domain.Repositories;
using Vitalita.API.Profiles.Domain.Services;
using Vitalita.API.Shared.Application.Patterns;
using Vitalita.API.Shared.Application.Persistence;
using Vitalita.API.Shared.Domain.Repositories;
using Vitalita.API.Shared.Domain.Services;

namespace Vitalita.API.Profiles.Application.Internal.CommandServices;

/// <summary>
///     Application service that handles the <see cref="FamilyAccess" /> commands.
/// </summary>
/// <remarks>
///     <para>
///         The invitation code and dates are always set by the server. A code that collides with an existing one
///         (application check or unique index) is generated again, up to <see cref="MaxCodeAttempts" /> times.
///     </para>
///     <para>
///         Changes: same status (relationship only) and revoke are for the caregiver in charge; accept
///         (Pending → Active) is for the invited person with their own family member profile. Administrators can do
///         all of them. Logs never contain the invited email.
///     </para>
/// </remarks>
/// <param name="familyAccessRepository">Family access repository.</param>
/// <param name="olderAdultRepository">Older adult repository.</param>
/// <param name="familyMemberProfileRepository">Family member profile repository.</param>
/// <param name="accessPolicy">Access rules of the Profiles context.</param>
/// <param name="invitationCodeGenerator">Cryptographic invitation code generator.</param>
/// <param name="clock">Clock for the invitation and acceptance instants.</param>
/// <param name="unitOfWork">Unit of work.</param>
/// <param name="logger">Logger.</param>
public class FamilyAccessCommandService(
    IFamilyAccessRepository familyAccessRepository,
    IOlderAdultRepository olderAdultRepository,
    IFamilyMemberProfileRepository familyMemberProfileRepository,
    ProfileAccessPolicy accessPolicy,
    IInvitationCodeGenerator invitationCodeGenerator,
    IClock clock,
    IUnitOfWork unitOfWork,
    ILogger<FamilyAccessCommandService> logger)
    : IFamilyAccessCommandService
{
    /// <summary>
    ///     Maximum number of codes generated for one invitation before giving up.
    /// </summary>
    public const int MaxCodeAttempts = 5;

    /// <inheritdoc />
    public async Task<Result<FamilyAccess, CreateFamilyAccessError>> Handle(CreateFamilyAccessCommand command,
        CancellationToken cancellationToken = default)
    {
        var olderAdult = await olderAdultRepository.FindByIdAsync(command.OlderAdultId, cancellationToken);
        if (olderAdult is null)
            return Fail(command.Requester.IsAdmin
                ? CreateFamilyAccessError.OlderAdultNotFound
                : CreateFamilyAccessError.Forbidden);

        if (!await accessPolicy.CanManageOlderAdultAsync(command.Requester, olderAdult, cancellationToken))
            return Fail(CreateFamilyAccessError.Forbidden);

        var email = TextNormalizer.NormalizeEmail(command.InvitedEmail);
        if (await familyAccessRepository.ExistsOpenAccessAsync(olderAdult.Id, email, cancellationToken))
            return Fail(CreateFamilyAccessError.DuplicateInvitation);

        for (var attempt = 1; attempt <= MaxCodeAttempts; attempt++)
        {
            var code = invitationCodeGenerator.Generate();
            if (await familyAccessRepository.ExistsByInvitationCodeAsync(code, cancellationToken))
            {
                logger.LogWarning("Invitation code collision (attempt {Attempt})", attempt);
                continue;
            }

            var access = new FamilyAccess(olderAdult.Id, email, command.Relationship, code, clock.UtcNow);
            try
            {
                await familyAccessRepository.AddAsync(access, cancellationToken);
                await unitOfWork.CompleteAsync(cancellationToken);
                logger.LogInformation("Family access {AccessId} created for older adult {OlderAdultId}", access.Id,
                    access.OlderAdultId);
                return new Result<FamilyAccess, CreateFamilyAccessError>.Success(access);
            }
            catch (DbUpdateException ex) when (ex.IsDuplicateKeyViolation())
            {
                // The only unique index of family_accesses is the code: stop tracking the rejected row and retry.
                familyAccessRepository.Remove(access);
                logger.LogWarning(ex, "Invitation code rejected by the unique index (attempt {Attempt})", attempt);
            }
            catch (DbUpdateException ex)
            {
                logger.LogError(ex, "Database update failed while creating a family access");
                return Fail(CreateFamilyAccessError.UnexpectedError);
            }
        }

        logger.LogError("Could not generate a unique invitation code after {Attempts} attempts", MaxCodeAttempts);
        return Fail(CreateFamilyAccessError.UnexpectedError);
    }

    /// <inheritdoc />
    public async Task<Result<FamilyAccess, UpdateFamilyAccessError>> Handle(UpdateFamilyAccessCommand command,
        CancellationToken cancellationToken = default)
    {
        var access = await familyAccessRepository.FindByIdAsync(command.Id, cancellationToken);
        if (access is null) return Fail(UpdateFamilyAccessError.NotFound);

        var requester = command.Requester;
        var canManage = await accessPolicy.CanManageOlderAdultAsync(requester, access.OlderAdultId, cancellationToken);

        UpdateFamilyAccessError? error;
        if (command.Status == access.Status)
            error = canManage ? ChangeRelationship(access, command.Relationship) : UpdateFamilyAccessError.Forbidden;
        else if (command.Status == AccessStatus.Revoked)
            error = canManage ? Revoke(access) : UpdateFamilyAccessError.Forbidden;
        else if (command.Status == AccessStatus.Active && access.Status == AccessStatus.Pending)
            error = await AcceptAsync(access, command, cancellationToken);
        else
            error = canManage || await accessPolicy.CanReadFamilyAccessAsync(requester, access, cancellationToken)
                ? UpdateFamilyAccessError.InvalidStatusTransition
                : UpdateFamilyAccessError.Forbidden;

        if (error is { } failure) return Fail(failure);

        try
        {
            familyAccessRepository.Update(access);
            await unitOfWork.CompleteAsync(cancellationToken);
            logger.LogInformation("Family access {AccessId} is now {Status}", access.Id, access.Status);
            return new Result<FamilyAccess, UpdateFamilyAccessError>.Success(access);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database update failed while updating family access {AccessId}", command.Id);
            return Fail(UpdateFamilyAccessError.UnexpectedError);
        }
    }

    private static UpdateFamilyAccessError? ChangeRelationship(FamilyAccess access, FamilyRelationship? relationship)
    {
        if (relationship is null) return null;
        try
        {
            access.ChangeRelationship(relationship.Value);
            return null;
        }
        catch (InvalidAccessStatusTransitionException)
        {
            return UpdateFamilyAccessError.InvalidStatusTransition;
        }
    }

    private static UpdateFamilyAccessError? Revoke(FamilyAccess access)
    {
        access.Revoke();
        return null;
    }

    private async Task<UpdateFamilyAccessError?> AcceptAsync(FamilyAccess access, UpdateFamilyAccessCommand command,
        CancellationToken cancellationToken)
    {
        if (command.FamilyMemberId is not { } familyMemberId) return UpdateFamilyAccessError.FamilyMemberRequired;

        var requester = command.Requester;
        string? acceptingEmail;
        if (requester.IsAdmin)
        {
            if (await familyMemberProfileRepository.FindByIdAsync(familyMemberId, cancellationToken) is null)
                return UpdateFamilyAccessError.FamilyMemberNotFound;
            acceptingEmail = access.InvitedEmail; // an administrator accepts on behalf of the invited person
        }
        else
        {
            if (!requester.IsFamilyMember ||
                await accessPolicy.GetFamilyMemberProfileIdAsync(requester, cancellationToken) != familyMemberId)
                return UpdateFamilyAccessError.Forbidden;
            acceptingEmail = requester.Email;
        }

        try
        {
            access.Accept(familyMemberId, acceptingEmail, clock.UtcNow);
            return null;
        }
        catch (InvitationEmailMismatchException)
        {
            return UpdateFamilyAccessError.Forbidden;
        }
        catch (InvitationNotAcceptableException)
        {
            return UpdateFamilyAccessError.InvitationNotAcceptable;
        }
    }

    private static Result<FamilyAccess, CreateFamilyAccessError> Fail(CreateFamilyAccessError error) =>
        new Result<FamilyAccess, CreateFamilyAccessError>.Failure(error);

    private static Result<FamilyAccess, UpdateFamilyAccessError> Fail(UpdateFamilyAccessError error) =>
        new Result<FamilyAccess, UpdateFamilyAccessError>.Failure(error);
}
