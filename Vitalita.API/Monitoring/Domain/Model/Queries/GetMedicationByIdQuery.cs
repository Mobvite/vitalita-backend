using Vitalita.API.Shared.Domain.Model.ValueObjects;

namespace Vitalita.API.Monitoring.Domain.Model.Queries;

/// <summary>
///     Query to get a medication by identifier.
/// </summary>
/// <param name="Requester">The authenticated user.</param>
/// <param name="Id">Medication identifier.</param>
public record GetMedicationByIdQuery(Requester Requester, int Id);
