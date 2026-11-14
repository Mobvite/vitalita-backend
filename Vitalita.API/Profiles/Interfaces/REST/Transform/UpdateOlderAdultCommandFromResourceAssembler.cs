using Vitalita.API.Profiles.Domain.Model.Commands;
using Vitalita.API.Profiles.Domain.Model.ValueObjects;
using Vitalita.API.Profiles.Interfaces.REST.Resources;
using Vitalita.API.Shared.Domain.Model.ValueObjects;

namespace Vitalita.API.Profiles.Interfaces.REST.Transform;

/// <summary>
///     Converts a <see cref="SaveOlderAdultResource" /> into an <see cref="UpdateOlderAdultCommand" />.
/// </summary>
public static class UpdateOlderAdultCommandFromResourceAssembler
{
    /// <summary>
    ///     Converts the validated resource into the command. The controller checks that the status is present.
    /// </summary>
    /// <param name="id">Older adult identifier (from the route).</param>
    /// <param name="requester">The authenticated user.</param>
    /// <param name="resource">The validated resource.</param>
    /// <returns>The command.</returns>
    public static UpdateOlderAdultCommand ToCommandFromResource(int id, Requester requester,
        SaveOlderAdultResource resource) =>
        new(id, requester, resource.CaregiverId!.Value,
            OlderAdultDataFromResourceAssembler.ToDataFromResource(resource, OlderAdultStatus.Active));
}
