using Vitalita.API.Monitoring.Domain.Model.Aggregates;
using Vitalita.API.Shared.Domain.Model.ValueObjects;

namespace Vitalita.API.Monitoring.Domain.Model.Commands;

/// <summary>
///     Command to register a medication.
/// </summary>
/// <param name="Requester">The authenticated user.</param>
/// <param name="OlderAdultId">Older adult that takes it.</param>
/// <param name="Data">The medication data.</param>
public record CreateMedicationCommand(Requester Requester, int OlderAdultId, MedicationData Data);
