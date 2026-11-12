using Vitalita.API.Monitoring.Domain.Model.Aggregates;
using Vitalita.API.Monitoring.Domain.Model.Exceptions;
using Vitalita.API.Tests.Monitoring.Fakes;

namespace Vitalita.API.Tests.Monitoring.Domain.Model.Aggregates;

public class MedicationTests
{
    [Fact]
    public void Constructor_SortsAndDeduplicatesTimes_AndTrimsText()
    {
        var medication = new Medication(1, MonitoringWorld.MedicationData(true, "20:00", " 08:00 ", "08:00"));

        Assert.Equal(["08:00", "20:00"], medication.ScheduleTimes);
        Assert.Equal("Losartán", medication.Name);
        Assert.True(medication.Active);
    }

    [Theory]
    [InlineData("8:00")]
    [InlineData("24:00")]
    [InlineData("12:60")]
    [InlineData("")]
    public void Constructor_WithAnInvalidTime_Throws(string time)
    {
        Assert.Throws<InvalidScheduleTimesException>(() =>
            new Medication(1, MonitoringWorld.MedicationData(true, "08:00", time)));
    }

    [Fact]
    public void Constructor_WithoutTimesOrWithTooMany_Throws()
    {
        var data = MonitoringWorld.MedicationData() with { ScheduleTimes = [] };
        var tooMany = MonitoringWorld.MedicationData() with
        {
            ScheduleTimes = Enumerable.Range(0, 13).Select(hour => $"{hour:00}:00").ToList()
        };

        Assert.Throws<InvalidScheduleTimesException>(() => new Medication(1, data));
        Assert.Throws<InvalidScheduleTimesException>(() => new Medication(1, tooMany));
    }

    [Fact]
    public void Constructor_WithoutNameOrDosage_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            new Medication(1, MonitoringWorld.MedicationData() with { Name = " " }));
        Assert.Throws<ArgumentException>(() =>
            new Medication(1, MonitoringWorld.MedicationData() with { Dosage = "" }));
    }

    [Fact]
    public void Update_CanDeactivate_ButNotMoveToAnotherOlderAdult()
    {
        var medication = new Medication(1, MonitoringWorld.MedicationData());

        medication.Update(1, MonitoringWorld.MedicationData(active: false));

        Assert.False(medication.Active);
        Assert.Throws<OlderAdultChangeNotAllowedException>(() =>
            medication.Update(2, MonitoringWorld.MedicationData()));
    }
}
