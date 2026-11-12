using Vitalita.API.Profiles.Application.Errors;
using Vitalita.API.Profiles.Domain.Model.Commands;
using Vitalita.API.Profiles.Domain.Model.ValueObjects;
using Vitalita.API.Shared.Application.Patterns;
using Vitalita.API.Tests.Profiles.Fakes;

namespace Vitalita.API.Tests.Profiles.Application.Internal.CommandServices;

public class OlderAdultCommandServiceTests
{
    private readonly ProfilesWorld _world = new();

    private static TError FailureOf<TValue, TError>(Result<TValue, TError> result) =>
        Assert.IsType<Result<TValue, TError>.Failure>(result).Error;

    private static TValue SuccessOf<TValue, TError>(Result<TValue, TError> result) =>
        Assert.IsType<Result<TValue, TError>.Success>(result).Value;

    [Fact]
    public async Task Create_InOwnCharge_IsSaved()
    {
        var caregiver = await _world.AddCaregiverAsync(1);

        var olderAdult = SuccessOf(await _world.OlderAdultCommands().Handle(
            new CreateOlderAdultCommand(ProfilesWorld.Caregiver(1), caregiver.Id, OlderAdultTestData.Valid())));

        Assert.Equal(caregiver.Id, olderAdult.CaregiverId);
        Assert.Single(_world.OlderAdults.All);
    }

    [Fact]
    public async Task Create_ForAnotherCaregiverOrWithoutProfile_IsForbidden()
    {
        var mine = await _world.AddCaregiverAsync(1);
        var other = await _world.AddCaregiverAsync(2);
        var service = _world.OlderAdultCommands();

        Assert.Equal(CreateOlderAdultError.Forbidden, FailureOf(await service.Handle(
            new CreateOlderAdultCommand(ProfilesWorld.Caregiver(1), other.Id, OlderAdultTestData.Valid()))));
        Assert.Equal(CreateOlderAdultError.Forbidden, FailureOf(await service.Handle(
            new CreateOlderAdultCommand(ProfilesWorld.Caregiver(3), mine.Id, OlderAdultTestData.Valid()))));
        Assert.Empty(_world.OlderAdults.All);
    }

    [Fact]
    public async Task Create_ByAdmin_ChecksTheCaregiverProfile()
    {
        var caregiver = await _world.AddCaregiverAsync(1);
        var service = _world.OlderAdultCommands();

        Assert.Equal(CreateOlderAdultError.CaregiverNotFound, FailureOf(await service.Handle(
            new CreateOlderAdultCommand(ProfilesWorld.Admin, 99, OlderAdultTestData.Valid()))));
        SuccessOf(await service.Handle(
            new CreateOlderAdultCommand(ProfilesWorld.Admin, caregiver.Id, OlderAdultTestData.Valid())));
    }

    [Fact]
    public async Task Create_UsesTodayInPeruForTheBirthDate()
    {
        var caregiver = await _world.AddCaregiverAsync(1);
        // 2026-11-12 02:00 UTC is still 2026-11-11 in Peru (UTC-05:00).
        _world.AdvanceTo(new DateTime(2026, 11, 12, 2, 0, 0, DateTimeKind.Utc));
        var service = _world.OlderAdultCommands();

        Assert.Equal(CreateOlderAdultError.BirthDateInFuture, FailureOf(await service.Handle(
            new CreateOlderAdultCommand(ProfilesWorld.Caregiver(1), caregiver.Id,
                OlderAdultTestData.Valid(new DateOnly(2026, 11, 12))))));
        SuccessOf(await service.Handle(new CreateOlderAdultCommand(ProfilesWorld.Caregiver(1), caregiver.Id,
            OlderAdultTestData.Valid(new DateOnly(2026, 11, 11)))));
    }

    [Fact]
    public async Task Update_Rules()
    {
        var caregiver = await _world.AddCaregiverAsync(1);
        await _world.AddCaregiverAsync(2);
        var olderAdult = await _world.AddOlderAdultAsync(caregiver.Id);
        var family = await _world.AddFamilyMemberAsync(10);
        await _world.AddActiveAccessAsync(olderAdult.Id, family, "f10@vitalita.test");
        var service = _world.OlderAdultCommands();
        UpdateOlderAdultCommand Update(Requester requester, int caregiverId, int? id = null,
            OlderAdultStatus status = OlderAdultStatus.Archived) =>
            new(id ?? olderAdult.Id, requester, caregiverId, OlderAdultTestData.Valid(status: status));

        Assert.Equal(UpdateOlderAdultError.NotFound,
            FailureOf(await service.Handle(Update(ProfilesWorld.Caregiver(1), caregiver.Id, 99))));
        Assert.Equal(UpdateOlderAdultError.Forbidden,
            FailureOf(await service.Handle(Update(ProfilesWorld.Caregiver(2), caregiver.Id))));
        Assert.Equal(UpdateOlderAdultError.Forbidden,
            FailureOf(await service.Handle(Update(ProfilesWorld.Family(10), caregiver.Id))));
        Assert.Equal(UpdateOlderAdultError.OwnerChangeNotAllowed,
            FailureOf(await service.Handle(Update(ProfilesWorld.Caregiver(1), caregiver.Id + 1))));

        var updated = SuccessOf(await service.Handle(Update(ProfilesWorld.Caregiver(1), caregiver.Id)));
        Assert.Equal(OlderAdultStatus.Archived, updated.Status);
    }
}
