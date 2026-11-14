namespace Vitalita.API.Monitoring.Domain.Model.ValueObjects;

/// <summary>
///     Unit and accepted range of a vital sign type.
/// </summary>
/// <param name="Unit">Unit of measure.</param>
/// <param name="Min">Minimum accepted value.</param>
/// <param name="Max">Maximum accepted value.</param>
/// <param name="SecondaryMin">Minimum of the secondary value, or <c>null</c> when the type has none.</param>
/// <param name="SecondaryMax">Maximum of the secondary value, or <c>null</c> when the type has none.</param>
public record VitalSignRule(string Unit, decimal Min, decimal Max, decimal? SecondaryMin = null,
    decimal? SecondaryMax = null)
{
    /// <summary>
    ///     Gets a value indicating whether the type needs a secondary value (diastolic pressure).
    /// </summary>
    public bool HasSecondaryValue => SecondaryMin is not null;

    /// <summary>
    ///     The rules of every type; the same ones as <c>VitalSignRules</c> in the frontend.
    /// </summary>
    public static readonly IReadOnlyDictionary<VitalSignType, VitalSignRule> ByType =
        new Dictionary<VitalSignType, VitalSignRule>
        {
            [VitalSignType.HeartRate] = new("bpm", 20, 250),
            [VitalSignType.BloodPressure] = new("mmHg", 50, 260, 30, 160),
            [VitalSignType.OxygenSaturation] = new("%", 50, 100),
            [VitalSignType.Temperature] = new("°C", 30, 45),
            [VitalSignType.Glucose] = new("mg/dL", 20, 600)
        };

    /// <summary>
    ///     Gets the rule of a type.
    /// </summary>
    /// <param name="type">The vital sign type.</param>
    /// <returns>The rule.</returns>
    public static VitalSignRule For(VitalSignType type) => ByType[type];
}
