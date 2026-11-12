using Vitalita.API.Monitoring.Application.Internal.Policies;
using Vitalita.API.Tests.Monitoring.Fakes;

namespace Vitalita.API.Tests.Monitoring.Application.Internal.Policies;

public class OlderAdultAccessPolicyTests
{
    private readonly MonitoringWorld _world = new();

    [Fact]
    public async Task Read_CaregiverFamilyAndAdmin()
    {
        var policy = _world.Policy;

        Assert.True(await policy.CanReadAsync(MonitoringWorld.Caregiver, MonitoringWorld.OlderAdult));
        Assert.True(await policy.CanReadAsync(MonitoringWorld.Family, MonitoringWorld.OlderAdult));
        Assert.True(await policy.CanReadAsync(MonitoringWorld.Admin, 999));
        Assert.False(await policy.CanReadAsync(MonitoringWorld.OtherCaregiver, MonitoringWorld.OlderAdult));
        Assert.False(await policy.CanReadAsync(MonitoringWorld.Caregiver, 999));
    }

    [Fact]
    public async Task Write_OnlyCaregiverInChargeAndAdmin()
    {
        var policy = _world.Policy;

        Assert.True(await policy.CanWriteAsync(MonitoringWorld.Caregiver, MonitoringWorld.OlderAdult));
        Assert.True(await policy.CanWriteAsync(MonitoringWorld.Admin, MonitoringWorld.OlderAdult));
        Assert.False(await policy.CanWriteAsync(MonitoringWorld.Family, MonitoringWorld.OlderAdult));
        Assert.False(await policy.CanWriteAsync(MonitoringWorld.OtherCaregiver, MonitoringWorld.OlderAdult));
    }

    [Fact]
    public async Task NewRecord_AdminChecksExistence_OthersGetForbidden()
    {
        var policy = _world.Policy;

        Assert.Equal(NewRecordAccess.Allowed,
            await policy.CheckNewRecordAsync(MonitoringWorld.Caregiver, MonitoringWorld.OlderAdult));
        Assert.Equal(NewRecordAccess.Forbidden,
            await policy.CheckNewRecordAsync(MonitoringWorld.Family, MonitoringWorld.OlderAdult));
        Assert.Equal(NewRecordAccess.Forbidden, await policy.CheckNewRecordAsync(MonitoringWorld.Caregiver, 999));
        Assert.Equal(NewRecordAccess.Allowed,
            await policy.CheckNewRecordAsync(MonitoringWorld.Admin, MonitoringWorld.OtherOlderAdult));
        Assert.Equal(NewRecordAccess.OlderAdultNotFound, await policy.CheckNewRecordAsync(MonitoringWorld.Admin, 999));
    }
}
