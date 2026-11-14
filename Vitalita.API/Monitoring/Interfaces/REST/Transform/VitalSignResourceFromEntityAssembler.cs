using Vitalita.API.Monitoring.Domain.Model.Aggregates;
using Vitalita.API.Monitoring.Interfaces.REST.Resources;

namespace Vitalita.API.Monitoring.Interfaces.REST.Transform;

/// <summary>
///     Converts a <see cref="VitalSign" /> into a <see cref="VitalSignResource" />.
/// </summary>
public static class VitalSignResourceFromEntityAssembler
{
    /// <summary>
    ///     Converts the entity into its resource. Values are normalized, so <c>70.00</c> is written as <c>70</c>.
    /// </summary>
    /// <param name="entity">The vital sign.</param>
    /// <returns>The resource.</returns>
    public static VitalSignResource ToResourceFromEntity(VitalSign entity) =>
        new(entity.Id, entity.OlderAdultId, entity.Type, Normalize(entity.Value), Normalize(entity.SecondaryValue),
            entity.Unit, entity.MeasuredAt);

    private static decimal Normalize(decimal value) => value / 1.000000000000000000000000000000000m;

    private static decimal? Normalize(decimal? value) => value is { } number ? Normalize(number) : null;
}
