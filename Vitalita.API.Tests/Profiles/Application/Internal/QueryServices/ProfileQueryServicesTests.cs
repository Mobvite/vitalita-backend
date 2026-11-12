using Vitalita.API.Profiles.Application.Errors;
using Vitalita.API.Profiles.Domain.Model.Queries;
using Vitalita.API.Profiles.Domain.Model.ValueObjects;
using Vitalita.API.Shared.Application.Patterns;
using Vitalita.API.Tests.Profiles.Fakes;

namespace Vitalita.API.Tests.Profiles.Application.Internal.QueryServices;

public class ProfileQueryServicesTests
{
    private readonly ProfilesWorld _world = new();

    private static TError FailureOf<TValue, TError>(Result<TValue, TError> result) =>
        Assert.IsType<Result<TValue, TError>.Failure>(result).Error;

    private static TValue SuccessOf<TValue, TError>(Result<TValue, TError> result) =>
        Assert.IsType<Result<TValue, TError>.Success>(result).Value;

    [Fact]
    public async Task CaregiverProfiles_ByUserId_OnlyForThemselvesOrAdmin()
    {
        await _world.AddCaregiverAsync(1);
        var service = _world.CaregiverQueries();

        Assert.Single(SuccessOf(await service.Handle(new GetCaregiverProfilesByUserIdQuery(ProfilesWorld.Caregiver(1), 1))));
        Assert.Empty(SuccessOf(await service.Handle(new GetCaregiverProfilesByUserIdQuery(ProfilesWorld.Caregiver(3), 3))));
        Assert.Equal(ProfileQueryError.Forbidden,
            FailureOf(await service.Handle(new GetCaregiverProfilesByUserIdQuery(ProfilesWorld.Caregiver(2), 1))));
        Assert.Single(SuccessOf(await service.Handle(new GetCaregiverProfilesByUserIdQuery(ProfilesWorld.Admin, 1))));
        Assert.Equal(ProfileQueryError.Forbidden,
            FailureOf(await service.Handle(new GetAllCaregiverProfilesQuery(ProfilesWorld.Caregiver(1)))));
        Assert.Equal(ProfileQueryError.NotFound,
            FailureOf(await service.Handle(new GetCaregiverProfileByIdQuery(ProfilesWorld.Caregiver(1), 99))));
    }

    [Fact]
    public async Task OlderAdults_ByCaregiverAndByIds()
    {
        var caregiver = await _world.AddCaregiverAsync(1);
        var other = await _world.AddCaregiverAsync(2);
        var mine = await _world.AddOlderAdultAsync(caregiver.Id);
        var notMine = await _world.AddOlderAdultAsync(other.Id);
        var family = await _world.AddFamilyMemberAsync(10);
        await _world.AddActiveAccessAsync(mine.Id, family, "f10@vitalita.test");
        var service = _world.OlderAdultQueries();

        Assert.Equal([mine], SuccessOf(await service.Handle(
            new GetOlderAdultsByCaregiverIdQuery(ProfilesWorld.Caregiver(1), caregiver.Id))));
        Assert.Equal(ProfileQueryError.Forbidden, FailureOf(await service.Handle(
            new GetOlderAdultsByCaregiverIdQuery(ProfilesWorld.Caregiver(1), other.Id))));
        Assert.Equal(ProfileQueryError.Forbidden, FailureOf(await service.Handle(
            new GetOlderAdultsByCaregiverIdQuery(ProfilesWorld.Family(10), caregiver.Id))));

        Assert.Equal([mine], await service.Handle(
            new GetOlderAdultsByIdsQuery(ProfilesWorld.Family(10), [mine.Id, notMine.Id, 99])));
        Assert.Empty(await service.Handle(new GetOlderAdultsByIdsQuery(ProfilesWorld.Family(10), [])));
        Assert.Equal(2, (await service.Handle(
            new GetOlderAdultsByIdsQuery(ProfilesWorld.Admin, [mine.Id, notMine.Id]))).Count);

        Assert.Equal(ProfileQueryError.Forbidden, FailureOf(await service.Handle(
            new GetOlderAdultByIdQuery(ProfilesWorld.Family(10), notMine.Id))));
        Assert.Equal(mine, SuccessOf(await service.Handle(new GetOlderAdultByIdQuery(ProfilesWorld.Family(10), mine.Id))));
    }

    [Fact]
    public async Task FamilyAccesses_FiltersAndPermissions()
    {
        var caregiver = await _world.AddCaregiverAsync(1);
        await _world.AddCaregiverAsync(2);
        var olderAdult = await _world.AddOlderAdultAsync(caregiver.Id);
        var family = await _world.AddFamilyMemberAsync(10);
        var active = await _world.AddActiveAccessAsync(olderAdult.Id, family, "f10@vitalita.test");
        await _world.AddInvitationAsync(olderAdult.Id, "pending@correo.pe");
        var service = _world.FamilyAccessQueries();

        Assert.Equal(2, SuccessOf(await service.Handle(
            new GetFamilyAccessesByOlderAdultIdQuery(ProfilesWorld.Caregiver(1), olderAdult.Id, null))).Count);
        Assert.Equal([active], SuccessOf(await service.Handle(
            new GetFamilyAccessesByOlderAdultIdQuery(ProfilesWorld.Caregiver(1), olderAdult.Id, AccessStatus.Active))));
        Assert.Equal(ProfileQueryError.Forbidden, FailureOf(await service.Handle(
            new GetFamilyAccessesByOlderAdultIdQuery(ProfilesWorld.Caregiver(2), olderAdult.Id, null))));
        Assert.Equal(ProfileQueryError.Forbidden, FailureOf(await service.Handle(
            new GetFamilyAccessesByOlderAdultIdQuery(ProfilesWorld.Caregiver(1), 99, null))));

        Assert.Equal([active], SuccessOf(await service.Handle(
            new GetFamilyAccessesByFamilyMemberIdQuery(ProfilesWorld.Family(10), family.Id, AccessStatus.Active))));
        Assert.Equal(ProfileQueryError.Forbidden, FailureOf(await service.Handle(
            new GetFamilyAccessesByFamilyMemberIdQuery(ProfilesWorld.Family(11), family.Id, null))));

        Assert.Equal(active, SuccessOf(await service.Handle(new GetFamilyAccessByIdQuery(ProfilesWorld.Family(10),
            active.Id))));
        Assert.Equal(ProfileQueryError.Forbidden, FailureOf(await service.Handle(
            new GetFamilyAccessByIdQuery(ProfilesWorld.Caregiver(2), active.Id))));
        Assert.Equal(ProfileQueryError.Forbidden, FailureOf(await service.Handle(
            new GetAllFamilyAccessesQuery(ProfilesWorld.Caregiver(1), null))));
    }

    [Fact]
    public async Task ValidInvitationByCode_OnlyPendingAndNotExpired()
    {
        var caregiver = await _world.AddCaregiverAsync(1);
        var olderAdult = await _world.AddOlderAdultAsync(caregiver.Id);
        var family = await _world.AddFamilyMemberAsync(10);
        var pending = await _world.AddInvitationAsync(olderAdult.Id, "pending@correo.pe", "VITA-PPPPPPPP");
        await _world.AddActiveAccessAsync(olderAdult.Id, family, "f10@vitalita.test", "VITA-QQQQQQQQ");

        Assert.Equal([pending], await _world.FamilyAccessQueries().Handle(
            new GetValidInvitationByCodeQuery(new InvitationCode("vita-pppppppp"))));
        Assert.Empty(await _world.FamilyAccessQueries().Handle(
            new GetValidInvitationByCodeQuery(new InvitationCode("VITA-QQQQQQQQ"))));
        Assert.Empty(await _world.FamilyAccessQueries().Handle(
            new GetValidInvitationByCodeQuery(new InvitationCode("VITA-NOTFOUND"))));

        _world.AdvanceTo(pending.ExpiresAt);
        Assert.Empty(await _world.FamilyAccessQueries().Handle(
            new GetValidInvitationByCodeQuery(new InvitationCode("VITA-PPPPPPPP"))));
    }
}
