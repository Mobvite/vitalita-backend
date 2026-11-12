using Vitalita.API.Shared.Domain.Model.ValueObjects;

namespace Vitalita.API.Monitoring.Domain.Model.Queries;

/// <summary>
///     Query to list the medications of an older adult.
/// </summary>
/// <param name="Requester">The authenticated user.</param>
/// <param name="OlderAdultId">Older adult identifier.</param>
/// <param name="Active">Optional filter by the active flag.</param>
public record GetMedicationsByOlderAdultIdQuery(Requester Requester, int OlderAdultId, bool? Active);
