using Vitalita.API.Shared.Domain.Model.ValueObjects;

namespace Vitalita.API.Monitoring.Domain.Model.Queries;

/// <summary>
///     Query to get a vital sign by identifier.
/// </summary>
/// <param name="Requester">The authenticated user.</param>
/// <param name="Id">Measurement identifier.</param>
public record GetVitalSignByIdQuery(Requester Requester, int Id);
