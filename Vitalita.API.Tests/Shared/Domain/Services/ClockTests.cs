using Vitalita.API.Shared.Domain.Services;
using Vitalita.API.Shared.Infrastructure.Time;

namespace Vitalita.API.Tests.Shared.Domain.Services;

public class ClockTests
{
    private sealed class FixedClock(DateTime utcNow) : IClock
    {
        public DateTime UtcNow { get; } = utcNow;
    }

    private static DateTime Utc(int year, int month, int day, int hour, int minute = 0) =>
        new(year, month, day, hour, minute, 0, DateTimeKind.Utc);

    [Fact]
    public void SystemClock_ReturnsUtc()
    {
        Assert.Equal(DateTimeKind.Utc, new SystemClock().UtcNow.Kind);
    }

    [Fact]
    public void PeruNow_IsFiveHoursBehindUtc()
    {
        var clock = new FixedClock(Utc(2026, 11, 11, 16, 30));

        var peruNow = clock.PeruNow();

        Assert.Equal(new DateTime(2026, 11, 11, 11, 30, 0), peruNow);
        Assert.Equal(DateTimeKind.Unspecified, peruNow.Kind);
    }

    [Theory]
    [InlineData(4, 59, 10)] // 23:59 in Peru, still the previous day
    [InlineData(5, 0, 11)] // 00:00 in Peru
    [InlineData(23, 0, 11)]
    public void PeruToday_ChangesAtPeruMidnight(int utcHour, int utcMinute, int expectedDay)
    {
        var clock = new FixedClock(Utc(2026, 11, 11, utcHour, utcMinute));

        Assert.Equal(new DateOnly(2026, 11, expectedDay), clock.PeruToday());
    }

    [Fact]
    public void PeruTodayRangeUtc_CoversPeruMidnightToMidnight()
    {
        var clock = new FixedClock(Utc(2026, 11, 11, 3, 0)); // 22:00 on Nov 10 in Peru

        var (startUtc, endUtc) = clock.PeruTodayRangeUtc();

        Assert.Equal(Utc(2026, 11, 10, 5), startUtc);
        Assert.Equal(Utc(2026, 11, 11, 5), endUtc);
        Assert.Equal(DateTimeKind.Utc, startUtc.Kind);
        Assert.Equal(DateTimeKind.Utc, endUtc.Kind);
    }

    [Fact]
    public void PeruTime_ToUtcAndFromUtc_RoundTrip()
    {
        var peru = new DateTime(2026, 12, 31, 20, 15, 0);

        var utc = PeruTime.ToUtc(peru);

        Assert.Equal(Utc(2027, 1, 1, 1, 15), utc);
        Assert.Equal(DateTimeKind.Utc, utc.Kind);
        Assert.Equal(peru, PeruTime.FromUtc(utc));
    }

    [Fact]
    public void PeruTime_FromUtc_TreatsUnspecifiedAsUtc()
    {
        var unspecified = new DateTime(2026, 11, 11, 16, 0, 0, DateTimeKind.Unspecified);

        Assert.Equal(new DateTime(2026, 11, 11, 11, 0, 0), PeruTime.FromUtc(unspecified));
    }

    [Fact]
    public void PeruTime_UtcOffset_IsMinusFiveHours()
    {
        Assert.Equal(TimeSpan.FromHours(-5), PeruTime.UtcOffset);
    }
}
