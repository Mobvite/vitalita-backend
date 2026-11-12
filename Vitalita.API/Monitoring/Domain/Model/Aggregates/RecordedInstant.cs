using Vitalita.API.Monitoring.Domain.Model.Exceptions;
using Vitalita.API.Monitoring.Domain.Model.ValueObjects;

namespace Vitalita.API.Monitoring.Domain.Model.Aggregates;

/// <summary>
///     Validation of the instants recorded by the Monitoring aggregates.
/// </summary>
internal static class RecordedInstant
{
    /// <summary>
    ///     Normalizes an instant to UTC and checks that it is not in the future.
    /// </summary>
    /// <param name="instant">The instant sent by the client.</param>
    /// <param name="utcNow">The current UTC instant.</param>
    /// <returns>The instant as UTC.</returns>
    /// <exception cref="InstantInFutureException">
    ///     Thrown when it is later than <paramref name="utcNow" /> plus <see cref="MonitoringLimits.MaxClockSkew" />.
    /// </exception>
    public static DateTime Validate(DateTime instant, DateTime utcNow)
    {
        var utc = instant.Kind switch
        {
            DateTimeKind.Utc => instant,
            DateTimeKind.Local => instant.ToUniversalTime(),
            _ => DateTime.SpecifyKind(instant, DateTimeKind.Utc)
        };
        if (utc > utcNow + MonitoringLimits.MaxClockSkew) throw new InstantInFutureException();
        return utc;
    }
}
