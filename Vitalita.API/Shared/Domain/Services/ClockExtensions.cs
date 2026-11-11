namespace Vitalita.API.Shared.Domain.Services;

/// <summary>
///     Peru time helpers on top of <see cref="IClock" />.
/// </summary>
public static class ClockExtensions
{
    /// <summary>
    ///     Gets the current Peru wall-clock time.
    /// </summary>
    /// <param name="clock">The clock.</param>
    /// <returns>The current Peru time with <see cref="DateTimeKind.Unspecified" />.</returns>
    public static DateTime PeruNow(this IClock clock) => PeruTime.FromUtc(clock.UtcNow);

    /// <summary>
    ///     Gets today's calendar date in Peru.
    /// </summary>
    /// <param name="clock">The clock.</param>
    /// <returns>Today's date in Peru.</returns>
    public static DateOnly PeruToday(this IClock clock) => PeruTime.DateOf(clock.UtcNow);

    /// <summary>
    ///     Gets the UTC range that covers today in Peru, for "today" filters.
    /// </summary>
    /// <param name="clock">The clock.</param>
    /// <returns>The UTC start (inclusive) and end (exclusive) of today in Peru.</returns>
    public static (DateTime StartUtc, DateTime EndUtc) PeruTodayRangeUtc(this IClock clock) =>
        PeruTime.DayRangeUtc(clock.PeruToday());
}
