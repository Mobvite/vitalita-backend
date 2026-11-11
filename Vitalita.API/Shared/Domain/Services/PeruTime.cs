namespace Vitalita.API.Shared.Domain.Services;

/// <summary>
///     Conversions between UTC instants and Peru wall-clock time.
/// </summary>
/// <remarks>
///     Peru uses UTC-05:00 all year (no daylight saving time), so a fixed offset is used instead of
///     <see cref="TimeZoneInfo" />. This avoids depending on the tzdata package inside Docker images.
///     Instants are always stored and returned in UTC; Peru time is only used to compute "today" and
///     day boundaries on the server.
/// </remarks>
public static class PeruTime
{
    /// <summary>
    ///     The fixed offset of Peru time from UTC (-5 hours).
    /// </summary>
    public static readonly TimeSpan UtcOffset = TimeSpan.FromHours(-5);

    /// <summary>
    ///     Converts a UTC instant to Peru wall-clock time.
    /// </summary>
    /// <param name="instant">
    ///     The instant. <see cref="DateTimeKind.Local" /> values are converted to UTC first;
    ///     <see cref="DateTimeKind.Unspecified" /> values are treated as UTC.
    /// </param>
    /// <returns>The Peru wall-clock time with <see cref="DateTimeKind.Unspecified" />.</returns>
    public static DateTime FromUtc(DateTime instant)
    {
        var utc = AsUtc(instant);
        return DateTime.SpecifyKind(utc + UtcOffset, DateTimeKind.Unspecified);
    }

    /// <summary>
    ///     Converts a Peru wall-clock time to a UTC instant.
    /// </summary>
    /// <param name="peruDateTime">The Peru wall-clock time (its kind is ignored).</param>
    /// <returns>The UTC instant with <see cref="DateTimeKind.Utc" />.</returns>
    public static DateTime ToUtc(DateTime peruDateTime) =>
        DateTime.SpecifyKind(peruDateTime - UtcOffset, DateTimeKind.Utc);

    /// <summary>
    ///     Gets the Peru calendar date of a UTC instant.
    /// </summary>
    /// <param name="instant">The instant (see <see cref="FromUtc" /> for kind handling).</param>
    /// <returns>The calendar date in Peru.</returns>
    public static DateOnly DateOf(DateTime instant) => DateOnly.FromDateTime(FromUtc(instant));

    /// <summary>
    ///     Gets the UTC range that covers a Peru calendar day.
    /// </summary>
    /// <param name="peruDate">The calendar date in Peru.</param>
    /// <returns>
    ///     The UTC start (inclusive, Peru midnight) and end (exclusive, next Peru midnight) of the day.
    ///     Use it as <c>start &lt;= value &amp;&amp; value &lt; end</c>.
    /// </returns>
    public static (DateTime StartUtc, DateTime EndUtc) DayRangeUtc(DateOnly peruDate)
    {
        var start = ToUtc(peruDate.ToDateTime(TimeOnly.MinValue));
        return (start, start.AddDays(1));
    }

    private static DateTime AsUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };
}
