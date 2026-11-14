using Vitalita.API.Profiles.Application.Errors;
using Vitalita.API.Profiles.Application.Internal.CommandServices;
using Vitalita.API.Profiles.Domain.Model.Aggregates;
using Vitalita.API.Profiles.Domain.Model.Commands;
using Vitalita.API.Profiles.Domain.Model.ValueObjects;
using Vitalita.API.Shared.Application.Patterns;
using Vitalita.API.Shared.Domain.Model.ValueObjects;
using Vitalita.API.Tests.Profiles.Fakes;

namespace Vitalita.API.Tests.Profiles.Application.Internal.CommandServices;

public class FamilyAccessCommandServiceTests
{
    private readonly ProfilesWorld _world = new();

    private static TError FailureOf<TValue, TError>(Result<TValue, TError> result) =>
        Assert.IsType<Result<TValue, TError>.Failure>(result).Error;

    private static TValue SuccessOf<TValue, TError>(Result<TValue, TError> result) =>
        Assert.IsType<Result<TValue, TError>.Success>(result).Value;

    private async Task<OlderAdult> ArrangeOlderAdultAsync()
    {
        var caregiver = await _world.AddCaregiverAsync(1);
        await _world.AddCaregiverAsync(2);
        return await _world.AddOlderAdultAsync(caregiver.Id);
    }

    private static CreateFamilyAccessCommand Invite(Requester requester, int olderAdultId,
        string email = " Carla@Correo.PE ") =>
        new(requester, olderAdultId, email, FamilyRelationship.Daughter);

    // Create

    [Fact]
    public async Task Create_UsesTheServerCodeAndDates()
    {
        var olderAdult = await ArrangeOlderAdultAsync();

        var access = SuccessOf(await _world.FamilyAccessCommands().Handle(Invite(ProfilesWorld.Caregiver(1),
            olderAdult.Id)));

        Assert.Equal("VITA-AAAAAAAA", access.InvitationCode.Value);
        Assert.Equal("carla@correo.pe", access.InvitedEmail);
        Assert.Equal(AccessStatus.Pending, access.Status);
        Assert.Equal(ProfilesWorld.Now, access.InvitedAt);
        Assert.Equal(ProfilesWorld.Now.AddDays(7), access.ExpiresAt);
    }

    [Fact]
    public async Task Create_RetriesWhenTheCodeAlreadyExists()
    {
        var olderAdult = await ArrangeOlderAdultAsync();
        await _world.AddInvitationAsync(olderAdult.Id, "other@correo.pe", "VITA-AAAAAAAA");
        _world.CodeGenerator = new QueuedInvitationCodeGenerator("VITA-AAAAAAAA", "VITA-BBBBBBBB");

        var access = SuccessOf(await _world.FamilyAccessCommands().Handle(Invite(ProfilesWorld.Caregiver(1),
            olderAdult.Id)));

        Assert.Equal("VITA-BBBBBBBB", access.InvitationCode.Value);
        Assert.Equal(2, _world.CodeGenerator.Calls);
    }

    [Fact]
    public async Task Create_GivesUpAfterTheMaximumAttempts()
    {
        var olderAdult = await ArrangeOlderAdultAsync();
        await _world.AddInvitationAsync(olderAdult.Id, "other@correo.pe", "VITA-AAAAAAAA");
        _world.CodeGenerator = new QueuedInvitationCodeGenerator("VITA-AAAAAAAA");

        var result = await _world.FamilyAccessCommands().Handle(Invite(ProfilesWorld.Caregiver(1), olderAdult.Id));

        Assert.Equal(CreateFamilyAccessError.UnexpectedError, FailureOf(result));
        Assert.Equal(FamilyAccessCommandService.MaxCodeAttempts, _world.CodeGenerator.Calls);
    }

    [Fact]
    public async Task Create_DuplicateOpenInvitation_IsAConflict_ButARevokedOneIsNot()
    {
        var olderAdult = await ArrangeOlderAdultAsync();
        var service = _world.FamilyAccessCommands();
        var first = SuccessOf(await service.Handle(Invite(ProfilesWorld.Caregiver(1), olderAdult.Id)));

        Assert.Equal(CreateFamilyAccessError.DuplicateInvitation,
            FailureOf(await service.Handle(Invite(ProfilesWorld.Caregiver(1), olderAdult.Id, "CARLA@correo.pe"))));

        first.Revoke();
        SuccessOf(await service.Handle(Invite(ProfilesWorld.Caregiver(1), olderAdult.Id)));
    }

    [Fact]
    public async Task Create_ByAnotherCaregiverOrForAMissingOlderAdult_IsForbidden()
    {
        var olderAdult = await ArrangeOlderAdultAsync();
        var service = _world.FamilyAccessCommands();

        Assert.Equal(CreateFamilyAccessError.Forbidden,
            FailureOf(await service.Handle(Invite(ProfilesWorld.Caregiver(2), olderAdult.Id))));
        Assert.Equal(CreateFamilyAccessError.Forbidden,
            FailureOf(await service.Handle(Invite(ProfilesWorld.Caregiver(1), 99))));
        Assert.Equal(CreateFamilyAccessError.OlderAdultNotFound,
            FailureOf(await service.Handle(Invite(ProfilesWorld.Admin, 99))));
    }

    // Update: accept

    [Fact]
    public async Task Accept_ByTheInvitedFamilyMemberWithOwnProfile_ActivatesTheAccess()
    {
        var olderAdult = await ArrangeOlderAdultAsync();
        var invitation = await _world.AddInvitationAsync(olderAdult.Id, "carla@correo.pe");
        var family = await _world.AddFamilyMemberAsync(10);

        var access = SuccessOf(await _world.FamilyAccessCommands().Handle(new UpdateFamilyAccessCommand(
            invitation.Id, ProfilesWorld.Family(10, "Carla@correo.pe"), AccessStatus.Active, family.Id, null)));

        Assert.Equal(AccessStatus.Active, access.Status);
        Assert.Equal(family.Id, access.FamilyMemberId);
        Assert.Equal(ProfilesWorld.Now, access.AcceptedAt);
        Assert.Equal(FamilyRelationship.Son, access.Relationship);
    }

    [Fact]
    public async Task Accept_Failures()
    {
        var olderAdult = await ArrangeOlderAdultAsync();
        var invitation = await _world.AddInvitationAsync(olderAdult.Id, "carla@correo.pe");
        var family = await _world.AddFamilyMemberAsync(10);
        var otherFamily = await _world.AddFamilyMemberAsync(11);
        var service = _world.FamilyAccessCommands();
        UpdateFamilyAccessCommand Accept(Requester requester, int? familyMemberId) =>
            new(invitation.Id, requester, AccessStatus.Active, familyMemberId, null);

        Assert.Equal(UpdateFamilyAccessError.FamilyMemberRequired,
            FailureOf(await service.Handle(Accept(ProfilesWorld.Family(10, "carla@correo.pe"), null))));
        Assert.Equal(UpdateFamilyAccessError.Forbidden,
            FailureOf(await service.Handle(Accept(ProfilesWorld.Family(10, "carla@correo.pe"), otherFamily.Id))));
        Assert.Equal(UpdateFamilyAccessError.Forbidden,
            FailureOf(await service.Handle(Accept(ProfilesWorld.Family(11, "other@correo.pe"), otherFamily.Id))));
        Assert.Equal(UpdateFamilyAccessError.Forbidden,
            FailureOf(await service.Handle(Accept(ProfilesWorld.Caregiver(1), family.Id))));
        Assert.Equal(AccessStatus.Pending, invitation.Status);

        _world.AdvanceTo(invitation.ExpiresAt);
        Assert.Equal(UpdateFamilyAccessError.InvitationNotAcceptable, FailureOf(await _world.FamilyAccessCommands()
            .Handle(Accept(ProfilesWorld.Family(10, "carla@correo.pe"), family.Id))));
    }

    [Fact]
    public async Task Accept_ByAdmin_RequiresAnExistingProfile()
    {
        var olderAdult = await ArrangeOlderAdultAsync();
        var invitation = await _world.AddInvitationAsync(olderAdult.Id, "carla@correo.pe");
        var family = await _world.AddFamilyMemberAsync(10);
        var service = _world.FamilyAccessCommands();

        Assert.Equal(UpdateFamilyAccessError.FamilyMemberNotFound, FailureOf(await service.Handle(
            new UpdateFamilyAccessCommand(invitation.Id, ProfilesWorld.Admin, AccessStatus.Active, 99, null))));
        SuccessOf(await service.Handle(
            new UpdateFamilyAccessCommand(invitation.Id, ProfilesWorld.Admin, AccessStatus.Active, family.Id, null)));
    }

    // Update: revoke, relationship and transitions

    [Fact]
    public async Task Revoke_OnlyTheCaregiverInCharge_AndRevokedIsFinal()
    {
        var olderAdult = await ArrangeOlderAdultAsync();
        var family = await _world.AddFamilyMemberAsync(10);
        var access = await _world.AddActiveAccessAsync(olderAdult.Id, family, "f10@vitalita.test");
        var service = _world.FamilyAccessCommands();
        UpdateFamilyAccessCommand Change(Requester requester, AccessStatus status) =>
            new(access.Id, requester, status, family.Id, FamilyRelationship.Son);

        Assert.Equal(UpdateFamilyAccessError.Forbidden,
            FailureOf(await service.Handle(Change(ProfilesWorld.Family(10), AccessStatus.Revoked))));
        Assert.Equal(UpdateFamilyAccessError.Forbidden,
            FailureOf(await service.Handle(Change(ProfilesWorld.Caregiver(2), AccessStatus.Revoked))));

        Assert.Equal(AccessStatus.Revoked,
            SuccessOf(await service.Handle(Change(ProfilesWorld.Caregiver(1), AccessStatus.Revoked))).Status);
        Assert.Equal(UpdateFamilyAccessError.InvalidStatusTransition,
            FailureOf(await service.Handle(Change(ProfilesWorld.Caregiver(1), AccessStatus.Active))));
        Assert.Equal(UpdateFamilyAccessError.InvalidStatusTransition,
            FailureOf(await service.Handle(Change(ProfilesWorld.Caregiver(1), AccessStatus.Pending))));
    }

    [Fact]
    public async Task ActiveBackToPending_IsInvalid_AndOutsidersAreForbidden()
    {
        var olderAdult = await ArrangeOlderAdultAsync();
        var family = await _world.AddFamilyMemberAsync(10);
        var access = await _world.AddActiveAccessAsync(olderAdult.Id, family, "f10@vitalita.test");
        var service = _world.FamilyAccessCommands();

        Assert.Equal(UpdateFamilyAccessError.InvalidStatusTransition, FailureOf(await service.Handle(
            new UpdateFamilyAccessCommand(access.Id, ProfilesWorld.Family(10), AccessStatus.Pending, null, null))));
        Assert.Equal(UpdateFamilyAccessError.Forbidden, FailureOf(await service.Handle(
            new UpdateFamilyAccessCommand(access.Id, ProfilesWorld.Family(11), AccessStatus.Pending, null, null))));
        Assert.Equal(UpdateFamilyAccessError.NotFound, FailureOf(await service.Handle(
            new UpdateFamilyAccessCommand(99, ProfilesWorld.Caregiver(1), AccessStatus.Revoked, null, null))));
    }

    [Fact]
    public async Task SameStatus_ChangesOnlyTheRelationship()
    {
        var olderAdult = await ArrangeOlderAdultAsync();
        var invitation = await _world.AddInvitationAsync(olderAdult.Id, "carla@correo.pe");
        var service = _world.FamilyAccessCommands();

        var updated = SuccessOf(await service.Handle(new UpdateFamilyAccessCommand(invitation.Id,
            ProfilesWorld.Caregiver(1), AccessStatus.Pending, null, FamilyRelationship.Spouse)));

        Assert.Equal(FamilyRelationship.Spouse, updated.Relationship);
        Assert.Equal(UpdateFamilyAccessError.Forbidden, FailureOf(await service.Handle(new UpdateFamilyAccessCommand(
            invitation.Id, ProfilesWorld.Family(10, "carla@correo.pe"), AccessStatus.Pending, null,
            FamilyRelationship.Son))));
    }
}
