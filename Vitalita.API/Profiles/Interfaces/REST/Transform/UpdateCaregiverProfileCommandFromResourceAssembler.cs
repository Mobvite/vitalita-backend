using Vitalita.API.Profiles.Domain.Model.Commands;
using Vitalita.API.Profiles.Interfaces.REST.Resources;
using Vitalita.API.Shared.Domain.Model.ValueObjects;

namespace Vitalita.API.Profiles.Interfaces.REST.Transform;

/// <summary>
///     Converts a <see cref="SaveCaregiverProfileResource" /> into an <see cref="UpdateCaregiverProfileCommand" />.
/// </summary>
public static class UpdateCaregiverProfileCommandFromResourceAssembler
{
    /// <summary>
    ///     Converts the validated resource into the command.
    /// </summary>
    /// <param name="id">Profile identifier (from the route).</param>
    /// <param name="requester">The authenticated user.</param>
    /// <param name="resource">The validated resource.</param>
    /// <returns>The command.</returns>
    public static UpdateCaregiverProfileCommand ToCommandFromResource(int id, Requester requester,
        SaveCaregiverProfileResource resource) =>
        new(id, requester, resource.UserId!.Value, resource.FirstName, resource.LastName, resource.PhoneNumber,
            resource.ProfessionalTitle, resource.Bio, resource.PhotoUrl, resource.Certifications ?? []);
}
