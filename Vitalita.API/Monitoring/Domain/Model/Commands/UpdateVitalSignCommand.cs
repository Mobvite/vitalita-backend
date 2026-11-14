using Vitalita.API.Monitoring.Domain.Model.ValueObjects;
using Vitalita.API.Shared.Domain.Model.ValueObjects;

namespace Vitalita.API.Monitoring.Domain.Model.Commands;

/// <summary>
///     Command to correct a vital sign measurement.
/// </summary>
/// <param name="Id">Measurement identifier.</param>
/// <param name="Requester">The authenticated user.</param>
/// <param name="OlderAdultId">Older adult sent by the client; it must be the current one.</param>
/// <param name="Type">Vital sign type.</param>
/// <param name="Value">Main value (systolic for blood pressure).</param>
/// <param name="SecondaryValue">Diastolic value for blood pressure; <c>null</c> otherwise.</param>
/// <param name="MeasuredAt">UTC instant of the measurement.</param>
public record UpdateVitalSignCommand(
    int Id,
    Requester Requester,
    int OlderAdultId,
    VitalSignType Type,
    decimal Value,
    decimal? SecondaryValue,
    DateTime MeasuredAt);
