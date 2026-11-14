using System.Text.RegularExpressions;
using Vitalita.API.Monitoring.Domain.Model.Exceptions;

namespace Vitalita.API.Monitoring.Domain.Model.ValueObjects;

/// <summary>
///     Rules of the wall-clock schedule times of a medication (<c>"HH:mm"</c>, not converted to UTC).
/// </summary>
public static partial class ScheduleTimes
{
    /// <summary>
    ///     Checks whether a value is a valid <c>HH:mm</c> time.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns><c>true</c> for 00:00 to 23:59 with two digits each.</returns>
    public static bool IsValid(string? value) => value is not null && TimePattern().IsMatch(value);

    /// <summary>
    ///     Trims, removes duplicates and sorts the times.
    /// </summary>
    /// <param name="times">The times.</param>
    /// <returns>The normalized times.</returns>
    /// <exception cref="InvalidScheduleTimesException">
    ///     Thrown when there is no time, more than <see cref="MonitoringLimits.ScheduleTimesMaxCount" />, or one is not
    ///     <c>HH:mm</c>.
    /// </exception>
    public static IReadOnlyList<string> Normalize(IEnumerable<string?>? times)
    {
        var normalized = (times ?? []).Select(time => time?.Trim() ?? string.Empty).Distinct()
            .Order(StringComparer.Ordinal).ToList();
        if (normalized.Count == 0 || normalized.Count > MonitoringLimits.ScheduleTimesMaxCount)
            throw new InvalidScheduleTimesException(
                $"A medication has 1 to {MonitoringLimits.ScheduleTimesMaxCount} schedule times.");
        if (!normalized.All(IsValid))
            throw new InvalidScheduleTimesException("Every schedule time must be HH:mm.");
        return normalized;
    }

    [GeneratedRegex(MonitoringLimits.ScheduleTimePattern)]
    private static partial Regex TimePattern();
}
