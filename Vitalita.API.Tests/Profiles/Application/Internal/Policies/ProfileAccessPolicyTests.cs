using Vitalita.API.Tests.Profiles.Fakes;

namespace Vitalita.API.Tests.Profiles.Application.Internal.Policies;

public class ProfileAccessPolicyTests
{
    private readonly ProfilesWorld _world = new();

    [Fact]
    public async Task OlderAdult_ReadAndManageRules()
    {
        var caregiver = await _world.AddCaregiverAsync(1);
        var otherCaregiver = await _world.AddCaregiverAsync(2);
        var olderAdult = await _world.AddOlderAdultAsync(caregiver.Id);
        var active = await _world.AddFamilyMemberAsync(10);
        var pendingOnly = await _world.AddFamilyMemberAsync(11);
        await _world.AddActiveAccessAsync(olderAdult.Id, active, "f10@vitalita.test");
        await _world.AddInvitationAsync(olderAdult.Id, "f11@vitalita.test");
        var policy = _world.Policy;

        Assert.True(await policy.CanManageOlderAdultAsync(ProfilesWorld.Caregiver(1), olderAdult));
        Assert.True(await policy.CanManageOlderAdultAsync(ProfilesWorld.Admin, olderAdult));
        Assert.False(await policy.CanManageOlderAdultAsync(ProfilesWorld.Caregiver(otherCaregiver.UserId), olderAdult));
        Assert.False(await policy.CanManageOlderAdultAsync(ProfilesWorld.Family(10), olderAdult));

        Assert.True(await policy.CanReadOlderAdultAsync(ProfilesWorld.Family(10), olderAdult));
        Assert.False(await policy.CanReadOlderAdultAsync(ProfilesWorld.Family(pendingOnly.UserId), olderAdult));
        Assert.False(await policy.CanManageOlderAdultAsync(ProfilesWorld.Caregiver(1), 999));
    }

    [Fact]
    public async Task RevokedAccess_NoLongerGivesReadAccess()
    {
        var caregiver = await _world.AddCaregiverAsync(1);
        var olderAdult = await _world.AddOlderAdultAsync(caregiver.Id);
        var family = await _world.AddFamilyMemberAsync(10);
        var access = await _world.AddActiveAccessAsync(olderAdult.Id, family, "f10@vitalita.test");

        access.Revoke();

        Assert.False(await _world.Policy.CanReadOlderAdultAsync(ProfilesWorld.Family(10), olderAdult));
        Assert.False(await _world.Policy.CanReadCaregiverProfileAsync(ProfilesWorld.Family(10), caregiver));
    }

    [Fact]
    public async Task CaregiverProfile_IsReadableByFamilyOfTheirOlderAdults()
    {
        var caregiver = await _world.AddCaregiverAsync(1);
        var other = await _world.AddCaregiverAsync(2);
        var olderAdult = await _world.AddOlderAdultAsync(caregiver.Id);
        var family = await _world.AddFamilyMemberAsync(10);
        await _world.AddActiveAccessAsync(olderAdult.Id, family, "f10@vitalita.test");

        Assert.True(await _world.Policy.CanReadCaregiverProfileAsync(ProfilesWorld.Family(10), caregiver));
        Assert.False(await _world.Policy.CanReadCaregiverProfileAsync(ProfilesWorld.Family(10), other));
        Assert.True(await _world.Policy.CanReadCaregiverProfileAsync(ProfilesWorld.Caregiver(2), other));
        Assert.False(await _world.Policy.CanReadCaregiverProfileAsync(ProfilesWorld.Caregiver(2), caregiver));
    }

    [Fact]
    public async Task FamilyMemberProfiles_AreFilteredForCaregivers()
    {
        var caregiver = await _world.AddCaregiverAsync(1);
        await _world.AddCaregiverAsync(2);
        var olderAdult = await _world.AddOlderAdultAsync(caregiver.Id);
        var linked = await _world.AddFamilyMemberAsync(10);
        var unrelated = await _world.AddFamilyMemberAsync(11);
        var access = await _world.AddActiveAccessAsync(olderAdult.Id, linked, "f10@vitalita.test");
        access.Revoke(); // a revoked access still shows the name in the caregiver list
        var profiles = new[] { linked, unrelated };

        Assert.Equal([linked], await _world.Policy.FilterReadableFamilyMemberProfilesAsync(
            ProfilesWorld.Caregiver(1), profiles));
        Assert.Empty(await _world.Policy.FilterReadableFamilyMemberProfilesAsync(ProfilesWorld.Caregiver(2), profiles));
        Assert.Equal([unrelated], await _world.Policy.FilterReadableFamilyMemberProfilesAsync(
            ProfilesWorld.Family(11), profiles));
        Assert.Equal(2, (await _world.Policy.FilterReadableFamilyMemberProfilesAsync(ProfilesWorld.Admin,
            profiles)).Count);
    }
}
