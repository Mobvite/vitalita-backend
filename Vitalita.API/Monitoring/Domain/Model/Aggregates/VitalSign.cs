using Vitalita.API.Monitoring.Domain.Model.Exceptions;
using Vitalita.API.Monitoring.Domain.Model.ValueObjects;
using Vitalita.API.Shared.Domain.Model;

namespace Vitalita.API.Monitoring.Domain.Model.Aggregates;

/// <summary>
///     Vital sign aggregate root: one measurement of one type (US14).
/// </summary>
/// <remarks>
///     <list type="bullet">
///         <item><description>The value must be inside the range of its type (<see cref="VitalSignRule" />).</description></item>
///         <item><description>Blood pressure needs a diastolic value (<see cref="SecondaryValue" />) lower than the systolic one; other types have none.</description></item>
///         <item><description>The unit is derived from the type.</description></item>
///         <item><description>The measurement instant cannot be in the future (5 minutes of clock skew are accepted).</description></item>
///     </list>
/// </remarks>
public class VitalSign : IAuditableEntity
{
    /// <summary>
    ///     Protected parameterless constructor for EF Core.
    /// </summary>
    protected VitalSign()
    {
        Unit = string.Empty;
    }

    /// <summary>
    ///     Records a measurement.
    /// </summary>
    /// <param name="olderAdultId">Older adult measured.</param>
    /// <param name="type">Vital sign type.</param>
    /// <param name="value">Main value (systolic for blood pressure).</param>
    /// <param name="secondaryValue">Diastolic value for blood pressure; <c>null</c> otherwise.</param>
    /// <param name="measuredAt">UTC instant of the measurement.</param>
    /// <param name="utcNow">The current UTC instant.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="olderAdultId" /> is not positive.</exception>
    /// <exception cref="InvalidVitalSignException">Thrown when the values break the rules of the type.</exception>
    /// <exception cref="InstantInFutureException">Thrown when the measurement instant is in the future.</exception>
    public VitalSign(int olderAdultId, VitalSignType type, decimal value, decimal? secondaryValue,
        DateTime measuredAt, DateTime utcNow) : this()
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(olderAdultId);
        OlderAdultId = olderAdultId;
        SetData(type, value, secondaryValue, measuredAt, utcNow);
    }

    /// <summary>Gets the server-generated identifier.</summary>
    public int Id { get; private set; }

    /// <summary>Gets the older adult measured.</summary>
    public int OlderAdultId { get; private set; }

    /// <summary>Gets the vital sign type.</summary>
    public VitalSignType Type { get; private set; }

    /// <summary>Gets the main value (systolic for blood pressure).</summary>
    public decimal Value { get; private set; }

    /// <summary>Gets the diastolic value for blood pressure; <c>null</c> for the other types.</summary>
    public decimal? SecondaryValue { get; private set; }

    /// <summary>Gets the unit of the type.</summary>
    public string Unit { get; private set; }

    /// <summary>Gets the UTC instant of the measurement.</summary>
    public DateTime MeasuredAt { get; private set; }

    /// <inheritdoc />
    public DateTime? CreatedAt { get; set; }

    /// <inheritdoc />
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    ///     Validates a measurement against the rules of its type.
    /// </summary>
    /// <param name="type">Vital sign type.</param>
    /// <param name="value">Main value.</param>
    /// <param name="secondaryValue">Secondary value.</param>
    /// <returns>The first rule broken, or <c>null</c> when the measurement is valid.</returns>
    public static VitalSignValidationError? Check(VitalSignType type, decimal value, decimal? secondaryValue)
    {
        var rule = VitalSignRule.For(type);
        if (value < rule.Min || value > rule.Max) return VitalSignValidationError.ValueOutOfRange;
        if (!rule.HasSecondaryValue)
            return secondaryValue is null ? null : VitalSignValidationError.SecondaryValueNotAllowed;
        if (secondaryValue is not { } secondary) return VitalSignValidationError.SecondaryValueRequired;
        if (secondary < rule.SecondaryMin || secondary > rule.SecondaryMax)
            return VitalSignValidationError.SecondaryValueOutOfRange;
        return value > secondary ? null : VitalSignValidationError.SystolicNotGreaterThanDiastolic;
    }

    /// <summary>
    ///     Replaces the measurement (a correction).
    /// </summary>
    /// <param name="olderAdultId">The older adult sent by the client; it must be the current one.</param>
    /// <param name="type">Vital sign type.</param>
    /// <param name="value">Main value.</param>
    /// <param name="secondaryValue">Secondary value.</param>
    /// <param name="measuredAt">UTC instant of the measurement.</param>
    /// <param name="utcNow">The current UTC instant.</param>
    /// <exception cref="OlderAdultChangeNotAllowedException">Thrown when the older adult is another one.</exception>
    /// <exception cref="InvalidVitalSignException">Thrown when the values break the rules of the type.</exception>
    /// <exception cref="InstantInFutureException">Thrown when the measurement instant is in the future.</exception>
    public void Update(int olderAdultId, VitalSignType type, decimal value, decimal? secondaryValue,
        DateTime measuredAt, DateTime utcNow)
    {
        if (olderAdultId != OlderAdultId) throw new OlderAdultChangeNotAllowedException();
        SetData(type, value, secondaryValue, measuredAt, utcNow);
    }

    private void SetData(VitalSignType type, decimal value, decimal? secondaryValue, DateTime measuredAt,
        DateTime utcNow)
    {
        if (Check(type, value, secondaryValue) is { } error) throw new InvalidVitalSignException(error);
        var instant = RecordedInstant.Validate(measuredAt, utcNow);
        Type = type;
        Value = value;
        SecondaryValue = secondaryValue;
        Unit = VitalSignRule.For(type).Unit;
        MeasuredAt = instant;
    }
}
