using Vitalita.API.Monitoring.Domain.Model.Aggregates;
using Vitalita.API.Shared.Domain.Model.ValueObjects;

namespace Vitalita.API.Monitoring.Domain.Model.Commands;

/// <summary>
///     Command to replace a medication (also used to deactivate it).
/// </summary>
/// <param name="Id">Medication identifier.</param>
/// <param name="Requester">The authenticated user.</param>
/// <param name="OlderAdultId">Older adult sent by the client; it must be the current one.</param>
/// <param name="Data">The new data.</param>
public record UpdateMedicationCommand(int Id, Requester Requester, int OlderAdultId, MedicationData Data);
