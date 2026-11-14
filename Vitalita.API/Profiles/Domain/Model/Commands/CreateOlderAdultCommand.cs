using Vitalita.API.Profiles.Domain.Model.Aggregates;
using Vitalita.API.Shared.Domain.Model.ValueObjects;

namespace Vitalita.API.Profiles.Domain.Model.Commands;

/// <summary>
///     Command to register an older adult.
/// </summary>
/// <param name="Requester">The authenticated user.</param>
/// <param name="CaregiverId">Caregiver profile in charge.</param>
/// <param name="Data">The older adult data.</param>
public record CreateOlderAdultCommand(Requester Requester, int CaregiverId, OlderAdultData Data);
