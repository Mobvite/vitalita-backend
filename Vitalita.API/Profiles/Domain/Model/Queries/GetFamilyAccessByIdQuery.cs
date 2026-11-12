using Vitalita.API.Profiles.Domain.Model.ValueObjects;

namespace Vitalita.API.Profiles.Domain.Model.Queries;

/// <summary>
///     Query to get a family access by identifier.
/// </summary>
/// <param name="Requester">The authenticated user.</param>
/// <param name="Id">Family access identifier.</param>
public record GetFamilyAccessByIdQuery(Requester Requester, int Id);
