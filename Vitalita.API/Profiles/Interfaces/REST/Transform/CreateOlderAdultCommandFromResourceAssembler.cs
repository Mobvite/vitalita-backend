using Vitalita.API.Profiles.Domain.Model.Commands;
using Vitalita.API.Profiles.Domain.Model.ValueObjects;
using Vitalita.API.Profiles.Interfaces.REST.Resources;

namespace Vitalita.API.Profiles.Interfaces.REST.Transform;

/// <summary>
///     Converts a <see cref="SaveOlderAdultResource" /> into a <see cref="CreateOlderAdultCommand" />.
/// </summary>
public static class CreateOlderAdultCommandFromResourceAssembler
{
    /// <summary>
    ///     Converts the validated resource into the command. A missing status means <c>Active</c>.
    /// </summary>
    /// <param name="requester">The authenticated user.</param>
    /// <param name="resource">The validated resource.</param>
    /// <returns>The command.</returns>
    public static CreateOlderAdultCommand ToCommandFromResource(Requester requester,
        SaveOlderAdultResource resource) =>
        new(requester, resource.CaregiverId!.Value,
            OlderAdultDataFromResourceAssembler.ToDataFromResource(resource, OlderAdultStatus.Active));
}
