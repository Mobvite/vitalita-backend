using Vitalita.API.Profiles.Domain.Model.ValueObjects;

namespace Vitalita.API.Profiles.Domain.Model.Queries;

/// <summary>
///     Query to get an older adult by identifier.
/// </summary>
/// <param name="Requester">The authenticated user.</param>
/// <param name="Id">Older adult identifier.</param>
public record GetOlderAdultByIdQuery(Requester Requester, int Id);
