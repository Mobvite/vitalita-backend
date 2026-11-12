using Vitalita.API.Profiles.Domain.Model.ValueObjects;

namespace Vitalita.API.Profiles.Domain.Model.Queries;

/// <summary>
///     Query to list every older adult (administrators only).
/// </summary>
/// <param name="Requester">The authenticated user.</param>
public record GetAllOlderAdultsQuery(Requester Requester);
