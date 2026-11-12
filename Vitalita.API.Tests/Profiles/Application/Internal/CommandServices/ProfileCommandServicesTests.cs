using Vitalita.API.Profiles.Application.Errors;
using Vitalita.API.Profiles.Domain.Model.Commands;
using Vitalita.API.Profiles.Domain.Model.ValueObjects;
using Vitalita.API.Shared.Application.Patterns;
using Vitalita.API.Tests.Profiles.Fakes;

namespace Vitalita.API.Tests.Profiles.Application.Internal.CommandServices;

public class ProfileCommandServicesTests
{
    private readonly ProfilesWorld _world = new();

    private static TError FailureOf<TValue, TError>(Result<TValue, TError> result) =>
        Assert.IsType<Result<TValue, TError>.Failure>(result).Error;

    private static TValue SuccessOf<TValue, TError>(Result<TValue, TError> result) =>
        Assert.IsType<Result<TValue, TError>.Success>(result).Value;

    private static CreateCaregiverProfileCommand CreateCaregiver(Requester requester, int userId) =>
        new(requester, userId, "Andrea", "Mendoza", "", "", "", "", []);

    // Caregiver profiles

    [Fact]
    public async Task CreateCaregiver_ForOwnUser_IsSaved()
    {
        var profile = SuccessOf(await _world.CaregiverCommands().Handle(CreateCaregiver(ProfilesWorld.Caregiver(1), 1)));

        Assert.Equal(1, profile.UserId);
        Assert.Single(_world.Caregivers.All);
        Assert.Equal(1, _world.UnitOfWork.CompleteCalls);
    }

    [Fact]
    public async Task CreateCaregiver_ForAnotherUserOrWithAnotherRole_IsForbidden()
    {
        var service = _world.CaregiverCommands();

        Assert.Equal(CreateCaregiverProfileError.Forbidden,
            FailureOf(await service.Handle(CreateCaregiver(ProfilesWorld.Caregiver(1), 2))));
        Assert.Equal(CreateCaregiverProfileError.Forbidden,
            FailureOf(await service.Handle(CreateCaregiver(ProfilesWorld.Family(1), 1))));
        Assert.Empty(_world.Caregivers.All);
    }

    [Fact]
    public async Task CreateCaregiver_ByAdmin_ChecksThatTheUserExists()
    {
        _world.Iam = new FakeIamContextFacade(5);
        var service = _world.CaregiverCommands();

        Assert.Equal(CreateCaregiverProfileError.UserNotFound,
            FailureOf(await service.Handle(CreateCaregiver(ProfilesWorld.Admin, 6))));
        SuccessOf(await service.Handle(CreateCaregiver(ProfilesWorld.Admin, 5)));
    }

    [Fact]
    public async Task CreateCaregiver_Twice_ReturnsDuplicate()
    {
        await _world.AddCaregiverAsync(1);

        var result = await _world.CaregiverCommands().Handle(CreateCaregiver(ProfilesWorld.Caregiver(1), 1));

        Assert.Equal(CreateCaregiverProfileError.DuplicateProfile, FailureOf(result));
    }

    [Fact]
    public async Task UpdateCaregiver_RulesForOwnerAndUserId()
    {
        var profile = await _world.AddCaregiverAsync(1);
        var service = _world.CaregiverCommands();
        UpdateCaregiverProfileCommand Update(Requester requester, int userId, int id = 0) =>
            new(id == 0 ? profile.Id : id, requester, userId, "Andrea", "Mendoza", "999888777", "Lic.", "", "",
                ["Primeros auxilios"]);

        Assert.Equal(UpdateProfileError.NotFound,
            FailureOf(await service.Handle(Update(ProfilesWorld.Caregiver(1), 1, 99))));
        Assert.Equal(UpdateProfileError.Forbidden, FailureOf(await service.Handle(Update(ProfilesWorld.Caregiver(2), 1))));
        Assert.Equal(UpdateProfileError.OwnerChangeNotAllowed,
            FailureOf(await service.Handle(Update(ProfilesWorld.Caregiver(1), 2))));

        var updated = SuccessOf(await service.Handle(Update(ProfilesWorld.Caregiver(1), 1)));
        Assert.Equal("999888777", updated.PhoneNumber);
        Assert.Equal(["Primeros auxilios"], updated.Certifications);
    }

    // Family member profiles

    [Fact]
    public async Task CreateFamilyMember_RequiresAValidInvitationForTheTokenEmail()
    {
        var caregiver = await _world.AddCaregiverAsync(1);
        var olderAdult = await _world.AddOlderAdultAsync(caregiver.Id);
        var service = _world.FamilyMemberCommands();
        var command = new CreateFamilyMemberProfileCommand(ProfilesWorld.Family(10, "Carla@Correo.pe"), 10, "Carla",
            "Rodríguez", "945222333");

        Assert.Equal(CreateFamilyMemberProfileError.NoValidInvitation, FailureOf(await service.Handle(command)));

        await _world.AddInvitationAsync(olderAdult.Id, "carla@correo.pe");
        var profile = SuccessOf(await service.Handle(command));

        Assert.Equal(10, profile.UserId);
        Assert.Equal(CreateFamilyMemberProfileError.DuplicateProfile, FailureOf(await service.Handle(command)));
    }

    [Fact]
    public async Task CreateFamilyMember_WithExpiredInvitation_IsRejected()
    {
        var caregiver = await _world.AddCaregiverAsync(1);
        var olderAdult = await _world.AddOlderAdultAsync(caregiver.Id);
        await _world.AddInvitationAsync(olderAdult.Id, "carla@correo.pe");
        _world.AdvanceTo(ProfilesWorld.Now.AddDays(8));

        var result = await _world.FamilyMemberCommands().Handle(new CreateFamilyMemberProfileCommand(
            ProfilesWorld.Family(10, "carla@correo.pe"), 10, "Carla", "Rodríguez", null));

        Assert.Equal(CreateFamilyMemberProfileError.NoValidInvitation, FailureOf(result));
    }

    [Fact]
    public async Task CreateFamilyMember_AsCaregiverOrForAnotherUser_IsForbidden()
    {
        var service = _world.FamilyMemberCommands();

        Assert.Equal(CreateFamilyMemberProfileError.Forbidden, FailureOf(await service.Handle(
            new CreateFamilyMemberProfileCommand(ProfilesWorld.Caregiver(10), 10, "A", "B", null))));
        Assert.Equal(CreateFamilyMemberProfileError.Forbidden, FailureOf(await service.Handle(
            new CreateFamilyMemberProfileCommand(ProfilesWorld.Family(10), 11, "A", "B", null))));
    }

    [Fact]
    public async Task CreateFamilyMember_ByAdmin_SkipsTheInvitationCheck()
    {
        var result = await _world.FamilyMemberCommands().Handle(
            new CreateFamilyMemberProfileCommand(ProfilesWorld.Admin, 12, "A", "B", null));

        Assert.Equal(12, SuccessOf(result).UserId);
    }

    [Fact]
    public async Task UpdateFamilyMember_OnlyTheOwner()
    {
        var profile = await _world.AddFamilyMemberAsync(10);
        var service = _world.FamilyMemberCommands();

        Assert.Equal(UpdateProfileError.Forbidden, FailureOf(await service.Handle(
            new UpdateFamilyMemberProfileCommand(profile.Id, ProfilesWorld.Family(11), 10, "A", "B", null))));
        Assert.Equal("A", SuccessOf(await service.Handle(
            new UpdateFamilyMemberProfileCommand(profile.Id, ProfilesWorld.Family(10), 10, "A", "B", null))).FirstName);
    }
}
