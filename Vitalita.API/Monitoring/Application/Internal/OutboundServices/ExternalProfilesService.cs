using Vitalita.API.Profiles.Interfaces.ACL;

namespace Vitalita.API.Monitoring.Application.Internal.OutboundServices;

/// <summary>
///     <see cref="IExternalProfilesService" /> on top of the Profiles ACL facade.
/// </summary>
/// <param name="profilesContextFacade">The Profiles ACL facade.</param>
public class ExternalProfilesService(IProfilesContextFacade profilesContextFacade) : IExternalProfilesService
{
    /// <inheritdoc />
    public Task<bool> ExistsOlderAdultAsync(int olderAdultId, CancellationToken cancellationToken = default) =>
        profilesContextFacade.ExistsOlderAdultByIdAsync(olderAdultId, cancellationToken);

    /// <inheritdoc />
    public Task<bool> CanReadOlderAdultAsync(int userId, int olderAdultId,
        CancellationToken cancellationToken = default) =>
        profilesContextFacade.CanUserAccessOlderAdultAsync(userId, olderAdultId, cancellationToken);

    /// <inheritdoc />
    public Task<bool> CanManageOlderAdultAsync(int userId, int olderAdultId,
        CancellationToken cancellationToken = default) =>
        profilesContextFacade.CanUserManageOlderAdultAsync(userId, olderAdultId, cancellationToken);
}
