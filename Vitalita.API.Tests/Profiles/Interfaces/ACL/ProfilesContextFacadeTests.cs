using Vitalita.API.Tests.Profiles.Fakes;

namespace Vitalita.API.Tests.Profiles.Interfaces.ACL;

public class ProfilesContextFacadeTests
{
    private readonly ProfilesWorld _world = new();

    [Fact]
    public async Task Facade_AnswersAccessQuestionsByUserId()
    {
        var caregiver = await _world.AddCaregiverAsync(1);
        await _world.AddCaregiverAsync(2);
        var olderAdult = await _world.AddOlderAdultAsync(caregiver.Id);
        var active = await _world.AddFamilyMemberAsync(10);
        var revoked = await _world.AddFamilyMemberAsync(11);
        await _world.AddActiveAccessAsync(olderAdult.Id, active, "f10@vitalita.test", "VITA-AAAAAAAA");
        (await _world.AddActiveAccessAsync(olderAdult.Id, revoked, "f11@vitalita.test", "VITA-BBBBBBBB")).Revoke();
        var facade = _world.Facade();

        Assert.True(await facade.ExistsOlderAdultByIdAsync(olderAdult.Id));
        Assert.False(await facade.ExistsOlderAdultByIdAsync(99));
        Assert.False(await facade.ExistsOlderAdultByIdAsync(0));

        Assert.True(await facade.CanUserAccessOlderAdultAsync(1, olderAdult.Id));
        Assert.True(await facade.CanUserAccessOlderAdultAsync(10, olderAdult.Id));
        Assert.False(await facade.CanUserAccessOlderAdultAsync(11, olderAdult.Id));
        Assert.False(await facade.CanUserAccessOlderAdultAsync(2, olderAdult.Id));
        Assert.False(await facade.CanUserAccessOlderAdultAsync(1, 99));

        Assert.True(await facade.CanUserManageOlderAdultAsync(1, olderAdult.Id));
        Assert.False(await facade.CanUserManageOlderAdultAsync(10, olderAdult.Id));
        Assert.False(await facade.CanUserManageOlderAdultAsync(99, olderAdult.Id));

        Assert.Equal(caregiver.Id, await facade.GetCaregiverProfileIdByUserIdAsync(1));
        Assert.Null(await facade.GetCaregiverProfileIdByUserIdAsync(10));

        Assert.Equal([10], await facade.GetActiveFamilyUserIdsAsync(olderAdult.Id));
        Assert.Empty(await facade.GetActiveFamilyUserIdsAsync(99));
    }
}
