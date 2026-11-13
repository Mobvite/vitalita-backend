using System.Net;
using Vitalita.API.IntegrationTests.Infrastructure;

namespace Vitalita.API.IntegrationTests;

/// <summary>Monitoring (TS04, TS05): /daily-reports, /vital-signs, /medications with older adult ownership.</summary>
public sealed class MonitoringEndpointsTests(VitalitaApiFactory factory) : ApiTestBase(factory)
{
    private static string Now => DateTime.UtcNow.AddSeconds(-5).ToString("O");

    private sealed record World(Caregiver Caregiver, int OlderAdultId, FamilyMember Family, Caregiver Outsider);

    /// <summary>Caregiver in charge, older adult, family member with an active access and an outsider caregiver.</summary>
    private async Task<World> GivenWorldAsync()
    {
        var caregiver = await Given.CaregiverAsync("mon-caregiver");
        var olderAdultId = await Given.OlderAdultAsync(caregiver);
        var family = await Given.FamilyMemberWithAccessAsync(caregiver, olderAdultId);
        var outsider = await Given.CaregiverAsync("mon-outsider");
        return new World(caregiver, olderAdultId, family, outsider);
    }

    private static object DailyReport(int olderAdultId, string? reportDate = null) => new
    {
        olderAdultId, reportDate = reportDate ?? Now, mood = "Good", generalCondition = "Stable",
        observations = "Durmió bien y almorzó completo."
    };

    private async Task<int> CreateDailyReportAsync(World world) =>
        (await Api.PostAsync("/daily-reports", DailyReport(world.OlderAdultId), world.Caregiver.Token))
        .ShouldBe(HttpStatusCode.Created).Id;

    [Fact]
    public async Task Caregiver_CreatesADailyReport_AndRecordedByComesFromTheToken()
    {
        var world = await GivenWorldAsync();

        var report = (await Api.PostAsync("/daily-reports", new
        {
            olderAdultId = world.OlderAdultId, recordedByUserId = 999, reportDate = Now, mood = "Good",
            generalCondition = "Stable", observations = "Durmió bien."
        }, world.Caregiver.Token)).ShouldBe(HttpStatusCode.Created);

        Assert.Equal(world.Caregiver.Account.UserId, report.Json.GetProperty("recordedByUserId").GetInt32());
        Assert.Equal(world.OlderAdultId, report.Json.GetProperty("olderAdultId").GetInt32());
        Assert.Equal("Good", report.Str("mood"));
        Assert.Equal("Stable", report.Str("generalCondition"));
        Assert.EndsWith("Z", report.Str("reportDate"));
        Assert.EndsWith($"/api/v1/daily-reports/{report.Id}", report.Location?.ToString());
    }

    [Fact]
    public async Task VitalSigns_DeriveTheUnitFromTheType_AndKeepTheDiastolicValue()
    {
        var world = await GivenWorldAsync();

        var temperature = (await Api.PostAsync("/vital-signs", new
        {
            olderAdultId = world.OlderAdultId, type = "Temperature", value = 36.6m, unit = "F", measuredAt = Now
        }, world.Caregiver.Token)).ShouldBe(HttpStatusCode.Created);
        Assert.Equal("°C", temperature.Str("unit"));
        Assert.Equal(36.6m, temperature.Json.GetProperty("value").GetDecimal());

        var pressure = (await Api.PostAsync("/vital-signs", new
        {
            olderAdultId = world.OlderAdultId, type = "BloodPressure", value = 120, secondaryValue = 80,
            measuredAt = Now
        }, world.Caregiver.Token)).ShouldBe(HttpStatusCode.Created);
        Assert.Equal("BloodPressure", pressure.Str("type"));
        Assert.Equal("mmHg", pressure.Str("unit"));
        Assert.Equal(80m, pressure.Json.GetProperty("secondaryValue").GetDecimal());
    }

    [Fact]
    public async Task Medication_NormalizesScheduleTimes_AndDeactivationHidesItFromTheActiveList()
    {
        var world = await GivenWorldAsync();
        var token = world.Caregiver.Token;

        var medication = (await Api.PostAsync("/medications", new
        {
            olderAdultId = world.OlderAdultId, name = "Metformina", dosage = "850 mg", frequency = "Cada 12 horas",
            scheduleTimes = new[] { "20:00", "08:00", "08:00" }, instructions = "Tomar después de comer."
        }, token)).ShouldBe(HttpStatusCode.Created);
        Assert.Equal(["08:00", "20:00"],
            medication.Json.GetProperty("scheduleTimes").EnumerateArray().Select(time => time.GetString()!).ToArray());
        Assert.True(medication.Json.GetProperty("active").GetBoolean());

        var deactivated = (await Api.PutAsync($"/medications/{medication.Id}", new
        {
            olderAdultId = world.OlderAdultId, name = "Metformina", dosage = "850 mg",
            scheduleTimes = new[] { "08:00", "20:00" }, active = false
        }, token)).ShouldBe(HttpStatusCode.OK);
        Assert.False(deactivated.Json.GetProperty("active").GetBoolean());

        var active = (await Api.GetAsync($"/medications?olderAdultId={world.OlderAdultId}&active=true", token))
            .ShouldBe(HttpStatusCode.OK);
        Assert.DoesNotContain(active.Items, item => item.GetProperty("id").GetInt32() == medication.Id);
        var inactive = (await Api.GetAsync($"/medications?olderAdultId={world.OlderAdultId}&active=false", token))
            .ShouldBe(HttpStatusCode.OK);
        Assert.Contains(inactive.Items, item => item.GetProperty("id").GetInt32() == medication.Id);
    }

    [Fact]
    public async Task FamilyMemberWithActiveAccess_ReadsReportsVitalSignsAndMedications()
    {
        var world = await GivenWorldAsync();
        var reportId = await CreateDailyReportAsync(world);
        var vitalSignId = (await Api.PostAsync("/vital-signs",
                new { olderAdultId = world.OlderAdultId, type = "HeartRate", value = 72, measuredAt = Now },
                world.Caregiver.Token))
            .ShouldBe(HttpStatusCode.Created).Id;
        var medicationId = (await Api.PostAsync("/medications",
                new { olderAdultId = world.OlderAdultId, name = "Losartán", dosage = "50 mg", scheduleTimes = new[] { "08:00" } },
                world.Caregiver.Token))
            .ShouldBe(HttpStatusCode.Created).Id;
        var token = world.Family.Token;
        var filter = $"?olderAdultId={world.OlderAdultId}";

        Assert.Equal(reportId, Assert.Single((await Api.GetAsync("/daily-reports" + filter, token))
            .ShouldBe(HttpStatusCode.OK).Items).GetProperty("id").GetInt32());
        Assert.Equal(vitalSignId, Assert.Single((await Api.GetAsync("/vital-signs" + filter, token))
            .ShouldBe(HttpStatusCode.OK).Items).GetProperty("id").GetInt32());
        Assert.Equal(medicationId, Assert.Single((await Api.GetAsync("/medications" + filter, token))
            .ShouldBe(HttpStatusCode.OK).Items).GetProperty("id").GetInt32());
        Assert.Equal(reportId, (await Api.GetAsync($"/daily-reports/{reportId}", token)).ShouldBe(HttpStatusCode.OK).Id);
    }

    [Fact]
    public async Task FamilyMember_CannotWriteRecords()
    {
        var world = await GivenWorldAsync();
        var reportId = await CreateDailyReportAsync(world);

        (await Api.PostAsync("/daily-reports", DailyReport(world.OlderAdultId), world.Family.Token))
            .ShouldBeProblem(HttpStatusCode.Forbidden, Messages.MonitoringAccessForbidden);
        (await Api.PutAsync($"/daily-reports/{reportId}", DailyReport(world.OlderAdultId), world.Family.Token))
            .ShouldBeProblem(HttpStatusCode.Forbidden, Messages.MonitoringAccessForbidden);
        (await Api.PostAsync("/vital-signs",
                new { olderAdultId = world.OlderAdultId, type = "HeartRate", value = 72, measuredAt = Now },
                world.Family.Token))
            .ShouldBeProblem(HttpStatusCode.Forbidden, Messages.MonitoringAccessForbidden);
    }

    [Fact]
    public async Task AnotherCaregiver_IsForbidden_AndAnUnknownOlderAdultIs403Not404()
    {
        var world = await GivenWorldAsync();
        var reportId = await CreateDailyReportAsync(world);

        (await Api.GetAsync($"/vital-signs?olderAdultId={world.OlderAdultId}", world.Outsider.Token))
            .ShouldBeProblem(HttpStatusCode.Forbidden, Messages.MonitoringAccessForbidden);
        (await Api.GetAsync($"/daily-reports/{reportId}", world.Outsider.Token))
            .ShouldBeProblem(HttpStatusCode.Forbidden, Messages.MonitoringAccessForbidden);
        (await Api.PostAsync("/daily-reports", DailyReport(world.OlderAdultId), world.Outsider.Token))
            .ShouldBeProblem(HttpStatusCode.Forbidden, Messages.MonitoringAccessForbidden);
        (await Api.GetAsync($"/daily-reports?olderAdultId={UnknownId}", world.Caregiver.Token))
            .ShouldBeProblem(HttpStatusCode.Forbidden, Messages.MonitoringAccessForbidden);
    }

    [Fact]
    public async Task UnknownRecordIs404_AndListsWithoutFiltersAreAdminOnly()
    {
        var caregiver = await Given.CaregiverAsync("mon-lists");

        (await Api.GetAsync($"/vital-signs/{UnknownId}", caregiver.Token))
            .ShouldBeProblem(HttpStatusCode.NotFound, Messages.VitalSignNotFound);
        (await Api.GetAsync("/medications", caregiver.Token))
            .ShouldBeProblem(HttpStatusCode.Forbidden, Messages.MonitoringAccessForbidden);

        (await Api.GetAsync("/medications", await Given.AdminTokenAsync())).ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task VitalSign_OutOfRangeOrWithoutDiastolic_Returns400WithTheFieldMessage()
    {
        var world = await GivenWorldAsync();

        (await Api.PostAsync("/vital-signs",
                new { olderAdultId = world.OlderAdultId, type = "HeartRate", value = 300, measuredAt = Now },
                world.Caregiver.Token))
            .ShouldBeValidationProblem("value", Messages.HeartRateOutOfRange);
        (await Api.PostAsync("/vital-signs",
                new { olderAdultId = world.OlderAdultId, type = "BloodPressure", value = 120, measuredAt = Now },
                world.Caregiver.Token))
            .ShouldBeValidationProblem("secondaryValue", Messages.DiastolicRequired);
    }

    [Fact]
    public async Task FutureReportDate_AndInvalidMedicationData_Return400()
    {
        var world = await GivenWorldAsync();

        (await Api.PostAsync("/daily-reports",
                DailyReport(world.OlderAdultId, DateTime.UtcNow.AddHours(1).ToString("O")), world.Caregiver.Token))
            .ShouldBeValidationProblem("reportDate", Messages.InstantInFuture);

        var invalid = await Api.PostAsync("/medications",
            new { olderAdultId = world.OlderAdultId, name = "", dosage = "10 mg", scheduleTimes = new[] { "8:00" } },
            world.Caregiver.Token);
        invalid.ShouldBeValidationProblem("name", Messages.RequiredField);
        invalid.ShouldBeValidationProblem("scheduleTimes", Messages.ScheduleTimesLimit);
    }

    [Fact]
    public async Task MovingARecordToAnotherOlderAdult_Returns409_AndNoTokenReturns401()
    {
        var world = await GivenWorldAsync();
        var reportId = await CreateDailyReportAsync(world);
        var otherOlderAdultId = await Given.OlderAdultAsync(world.Caregiver, "Jorge");

        (await Api.PutAsync($"/daily-reports/{reportId}", DailyReport(otherOlderAdultId), world.Caregiver.Token))
            .ShouldBeProblem(HttpStatusCode.Conflict, Messages.MonitoringOlderAdultChangeNotAllowed);
        (await Api.GetAsync($"/medications?olderAdultId={world.OlderAdultId}"))
            .ShouldBeProblem(HttpStatusCode.Unauthorized, Messages.Unauthorized);
    }
}
