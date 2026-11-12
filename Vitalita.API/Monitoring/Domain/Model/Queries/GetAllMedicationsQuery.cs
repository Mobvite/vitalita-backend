using Vitalita.API.Shared.Domain.Model.ValueObjects;

namespace Vitalita.API.Monitoring.Domain.Model.Queries;

/// <summary>
///     Query to list every medication (administrators only).
/// </summary>
/// <param name="Requester">The authenticated user.</param>
/// <param name="Active">Optional filter by the active flag.</param>
public record GetAllMedicationsQuery(Requester Requester, bool? Active);
