using Vitalita.API.Monitoring.Domain.Model.Aggregates;
using Vitalita.API.Monitoring.Domain.Model.Exceptions;
using Vitalita.API.Monitoring.Domain.Model.ValueObjects;

namespace Vitalita.API.Tests.Monitoring.Domain.Model.Aggregates;

public class DailyReportTests
{
    private static readonly DateTime Now = new(2026, 11, 11, 15, 0, 0, DateTimeKind.Utc);

    private static DailyReport Report(DateTime? reportDate = null) =>
        new(1, 7, reportDate ?? Now.AddHours(-1), Mood.Good, GeneralCondition.Stable, "  Durmió bien.  ", Now);

    [Fact]
    public void Constructor_KeepsTheAuthorAndTrimsObservations()
    {
        var report = Report();

        Assert.Equal(1, report.OlderAdultId);
        Assert.Equal(7, report.RecordedByUserId);
        Assert.Equal("Durmió bien.", report.Observations);
        Assert.Equal(DateTimeKind.Utc, report.ReportDate.Kind);
    }

    [Fact]
    public void Constructor_AcceptsTheClockSkewButNotLater()
    {
        Assert.Equal(Now.AddMinutes(5), Report(Now.AddMinutes(5)).ReportDate);
        Assert.Throws<InstantInFutureException>(() => Report(Now.AddMinutes(5).AddSeconds(1)));
    }

    [Fact]
    public void Constructor_UnspecifiedKind_IsTreatedAsUtc()
    {
        var report = Report(new DateTime(2026, 11, 11, 10, 0, 0, DateTimeKind.Unspecified));

        Assert.Equal(new DateTime(2026, 11, 11, 10, 0, 0, DateTimeKind.Utc), report.ReportDate);
        Assert.Equal(DateTimeKind.Utc, report.ReportDate.Kind);
    }

    [Fact]
    public void Constructor_WithoutObservations_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            new DailyReport(1, 7, Now, Mood.Good, GeneralCondition.Stable, "   ", Now));
    }

    [Fact]
    public void Update_KeepsTheAuthor_AndRejectsAnotherOlderAdult()
    {
        var report = Report();

        report.Update(1, Now, Mood.Bad, GeneralCondition.Attention, "Tos leve.", Now);

        Assert.Equal(7, report.RecordedByUserId);
        Assert.Equal(GeneralCondition.Attention, report.GeneralCondition);
        Assert.Throws<OlderAdultChangeNotAllowedException>(() =>
            report.Update(2, Now, Mood.Bad, GeneralCondition.Attention, "x", Now));
    }
}
