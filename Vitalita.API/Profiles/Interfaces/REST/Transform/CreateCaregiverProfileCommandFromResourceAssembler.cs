using Vitalita.API.Profiles.Domain.Model.Commands;
using Vitalita.API.Profiles.Domain.Model.ValueObjects;
using Vitalita.API.Profiles.Interfaces.REST.Resources;

namespace Vitalita.API.Profiles.Interfaces.REST.Transform;

/// <summary>
///     Converts a <see cref="SaveCaregiverProfileResource" /> into a <see cref="CreateCaregiverProfileCommand" />.
/// </summary>
public static class CreateCaregiverProfileCommandFromResourceAssembler
{
    /// <summary>
    ///     Converts the validated resource into the command.
    /// </summary>
    /// <param name="requester">The authenticated user.</param>
    /// <param name="resource">The validated resource.</param>
    /// <returns>The command.</returns>
    public static CreateCaregiverProfileCommand ToCommandFromResource(Requester requester,
        SaveCaregiverProfileResource resource) =>
        new(requester, resource.UserId!.Value, resource.FirstName, resource.LastName, resource.PhoneNumber,
            resource.ProfessionalTitle, resource.Bio, resource.PhotoUrl, resource.Certifications ?? []);
}
