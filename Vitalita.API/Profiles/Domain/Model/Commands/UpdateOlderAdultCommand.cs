using Vitalita.API.Profiles.Domain.Model.Aggregates;
using Vitalita.API.Profiles.Domain.Model.ValueObjects;

namespace Vitalita.API.Profiles.Domain.Model.Commands;

/// <summary>
///     Command to replace the data of an older adult.
/// </summary>
/// <param name="Id">Older adult identifier.</param>
/// <param name="Requester">The authenticated user.</param>
/// <param name="CaregiverId">Caregiver sent by the client; it must be the current one.</param>
/// <param name="Data">The new data.</param>
public record UpdateOlderAdultCommand(int Id, Requester Requester, int CaregiverId, OlderAdultData Data);
