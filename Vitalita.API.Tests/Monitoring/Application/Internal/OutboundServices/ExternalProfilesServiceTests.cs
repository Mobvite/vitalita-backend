using Vitalita.API.Monitoring.Application.Internal.OutboundServices;
using Vitalita.API.Profiles.Interfaces.ACL;

namespace Vitalita.API.Tests.Monitoring.Application.Internal.OutboundServices;

public class ExternalProfilesServiceTests
{
    private sealed class RecordingFacade : IProfilesContextFacade
    {
        public List<string> Calls { get; } = [];

        public Task<bool> ExistsOlderAdultByIdAsync(int olderAdultId, CancellationToken cancellationToken = default)
        {
            Calls.Add($"exists:{olderAdultId}");
            return Task.FromResult(true);
        }

        public Task<bool> CanUserAccessOlderAdultAsync(int userId, int olderAdultId,
            CancellationToken cancellationToken = default)
        {
            Calls.Add($"access:{userId}:{olderAdultId}");
            return Task.FromResult(true);
        }

        public Task<bool> CanUserManageOlderAdultAsync(int userId, int olderAdultId,
            CancellationToken cancellationToken = default)
        {
            Calls.Add($"manage:{userId}:{olderAdultId}");
            return Task.FromResult(false);
        }

        public Task<int?> GetCaregiverProfileIdByUserIdAsync(int userId,
            CancellationToken cancellationToken = default) => Task.FromResult<int?>(null);

        public Task<IReadOnlyList<int>> GetActiveFamilyUserIdsAsync(int olderAdultId,
            CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<int>>([]);
    }

    [Fact]
    public async Task DelegatesToTheProfilesFacade()
    {
        var facade = new RecordingFacade();
        var service = new ExternalProfilesService(facade);

        Assert.True(await service.ExistsOlderAdultAsync(4));
        Assert.True(await service.CanReadOlderAdultAsync(1, 4));
        Assert.False(await service.CanManageOlderAdultAsync(1, 4));
        Assert.Equal(["exists:4", "access:1:4", "manage:1:4"], facade.Calls);
    }
}
