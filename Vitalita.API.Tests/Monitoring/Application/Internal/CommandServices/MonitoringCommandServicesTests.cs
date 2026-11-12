using Vitalita.API.Monitoring.Application.Errors;
using Vitalita.API.Monitoring.Domain.Model.Commands;
using Vitalita.API.Monitoring.Domain.Model.ValueObjects;
using Vitalita.API.Shared.Application.Patterns;
using Vitalita.API.Shared.Domain.Model.ValueObjects;
using Vitalita.API.Tests.Monitoring.Fakes;

namespace Vitalita.API.Tests.Monitoring.Application.Internal.CommandServices;

public class MonitoringCommandServicesTests
{
    private static readonly DateTime Now = MonitoringWorld.Now;
    private readonly MonitoringWorld _world = new();

    private static TError FailureOf<TValue, TError>(Result<TValue, TError> result) =>
        Assert.IsType<Result<TValue, TError>.Failure>(result).Error;

    private static TValue SuccessOf<TValue, TError>(Result<TValue, TError> result) =>
        Assert.IsType<Result<TValue, TError>.Success>(result).Value;

    private static CreateDailyReportCommand Report(Requester requester, int olderAdultId = MonitoringWorld.OlderAdult,
        DateTime? reportDate = null) =>
        new(requester, olderAdultId, reportDate ?? Now, Mood.Good, GeneralCondition.Stable, "Durmió bien.");

    private static CreateVitalSignCommand Sign(Requester requester, VitalSignType type, decimal value,
        decimal? secondary = null) =>
        new(requester, MonitoringWorld.OlderAdult, type, value, secondary, Now);

    // Daily reports

    [Fact]
    public async Task CreateReport_ByCaregiverInCharge_UsesTheRequesterAsAuthor()
    {
        var report = SuccessOf(await _world.ReportCommands().Handle(Report(MonitoringWorld.Caregiver)));

        Assert.Equal(MonitoringWorld.Caregiver.UserId, report.RecordedByUserId);
        Assert.Single(_world.Reports.All);
        Assert.Equal(1, _world.UnitOfWork.CompleteCalls);
    }

    [Fact]
    public async Task CreateReport_PermissionRules()
    {
        var service = _world.ReportCommands();

        Assert.Equal(DailyReportCommandError.Forbidden, FailureOf(await service.Handle(Report(MonitoringWorld.Family))));
        Assert.Equal(DailyReportCommandError.Forbidden,
            FailureOf(await service.Handle(Report(MonitoringWorld.OtherCaregiver))));
        Assert.Equal(DailyReportCommandError.Forbidden,
            FailureOf(await service.Handle(Report(MonitoringWorld.Caregiver, 999))));
        Assert.Equal(DailyReportCommandError.OlderAdultNotFound,
            FailureOf(await service.Handle(Report(MonitoringWorld.Admin, 999))));
        Assert.Empty(_world.Reports.All);

        SuccessOf(await service.Handle(Report(MonitoringWorld.Admin)));
    }

    [Fact]
    public async Task CreateReport_InTheFuture_IsRejected()
    {
        var result = await _world.ReportCommands().Handle(Report(MonitoringWorld.Caregiver, reportDate: Now.AddHours(1)));

        Assert.Equal(DailyReportCommandError.InstantInFuture, FailureOf(result));
    }

    [Fact]
    public async Task UpdateReport_Rules()
    {
        var report = SuccessOf(await _world.ReportCommands().Handle(Report(MonitoringWorld.Caregiver)));
        var service = _world.ReportCommands();
        UpdateDailyReportCommand Update(Requester requester, int olderAdultId = MonitoringWorld.OlderAdult, int? id = null) =>
            new(id ?? report.Id, requester, olderAdultId, Now, Mood.Bad, GeneralCondition.Attention, "Tos leve.");

        Assert.Equal(DailyReportCommandError.NotFound,
            FailureOf(await service.Handle(Update(MonitoringWorld.Caregiver, id: 99))));
        Assert.Equal(DailyReportCommandError.Forbidden, FailureOf(await service.Handle(Update(MonitoringWorld.Family))));
        Assert.Equal(DailyReportCommandError.OlderAdultChangeNotAllowed,
            FailureOf(await service.Handle(Update(MonitoringWorld.Caregiver, MonitoringWorld.OtherOlderAdult))));

        var updated = SuccessOf(await service.Handle(Update(MonitoringWorld.Admin)));
        Assert.Equal(GeneralCondition.Attention, updated.GeneralCondition);
        Assert.Equal(MonitoringWorld.Caregiver.UserId, updated.RecordedByUserId);
    }

    // Vital signs

    [Fact]
    public async Task CreateSign_Valid_IsSavedWithTheUnitOfTheType()
    {
        var sign = SuccessOf(await _world.SignCommands().Handle(
            Sign(MonitoringWorld.Caregiver, VitalSignType.BloodPressure, 120, 80)));

        Assert.Equal("mmHg", sign.Unit);
        Assert.Equal(80, sign.SecondaryValue);
    }

    [Theory]
    [InlineData(VitalSignType.HeartRate, 300, null, VitalSignCommandError.ValueOutOfRange)]
    [InlineData(VitalSignType.BloodPressure, 120, null, VitalSignCommandError.SecondaryValueRequired)]
    [InlineData(VitalSignType.BloodPressure, 120, 170.0, VitalSignCommandError.SecondaryValueOutOfRange)]
    [InlineData(VitalSignType.Glucose, 110, 1.0, VitalSignCommandError.SecondaryValueNotAllowed)]
    [InlineData(VitalSignType.BloodPressure, 90, 95.0, VitalSignCommandError.SystolicNotGreaterThanDiastolic)]
    public async Task CreateSign_InvalidValues_AreMappedToTheirError(VitalSignType type, double value,
        double? secondary, VitalSignCommandError expected)
    {
        var result = await _world.SignCommands().Handle(
            Sign(MonitoringWorld.Caregiver, type, (decimal)value, (decimal?)secondary));

        Assert.Equal(expected, FailureOf(result));
        Assert.Empty(_world.Signs.All);
    }

    [Fact]
    public async Task CreateSign_ByFamily_IsForbidden_AndInTheFuture_IsRejected()
    {
        var service = _world.SignCommands();

        Assert.Equal(VitalSignCommandError.Forbidden,
            FailureOf(await service.Handle(Sign(MonitoringWorld.Family, VitalSignType.HeartRate, 70))));
        Assert.Equal(VitalSignCommandError.InstantInFuture, FailureOf(await service.Handle(
            Sign(MonitoringWorld.Caregiver, VitalSignType.HeartRate, 70) with { MeasuredAt = Now.AddDays(1) })));
    }

    [Fact]
    public async Task UpdateSign_Rules()
    {
        var sign = SuccessOf(await _world.SignCommands().Handle(Sign(MonitoringWorld.Caregiver, VitalSignType.HeartRate,
            70)));
        var service = _world.SignCommands();
        UpdateVitalSignCommand Update(Requester requester, decimal value, int olderAdultId = MonitoringWorld.OlderAdult) =>
            new(sign.Id, requester, olderAdultId, VitalSignType.HeartRate, value, null, Now);

        Assert.Equal(VitalSignCommandError.Forbidden, FailureOf(await service.Handle(Update(MonitoringWorld.Outsider, 72))));
        Assert.Equal(VitalSignCommandError.ValueOutOfRange,
            FailureOf(await service.Handle(Update(MonitoringWorld.Caregiver, 10))));
        Assert.Equal(VitalSignCommandError.OlderAdultChangeNotAllowed,
            FailureOf(await service.Handle(Update(MonitoringWorld.Caregiver, 72, MonitoringWorld.OtherOlderAdult))));
        Assert.Equal(72, SuccessOf(await service.Handle(Update(MonitoringWorld.Caregiver, 72))).Value);
    }

    // Medications

    [Fact]
    public async Task CreateMedication_RulesAndNormalization()
    {
        var service = _world.MedicationCommands();

        var medication = SuccessOf(await service.Handle(new CreateMedicationCommand(MonitoringWorld.Caregiver,
            MonitoringWorld.OlderAdult, MonitoringWorld.MedicationData(true, "20:00", "08:00"))));
        Assert.Equal(["08:00", "20:00"], medication.ScheduleTimes);

        Assert.Equal(MedicationCommandError.Forbidden, FailureOf(await service.Handle(new CreateMedicationCommand(
            MonitoringWorld.Family, MonitoringWorld.OlderAdult, MonitoringWorld.MedicationData()))));
        Assert.Equal(MedicationCommandError.InvalidScheduleTimes, FailureOf(await service.Handle(
            new CreateMedicationCommand(MonitoringWorld.Caregiver, MonitoringWorld.OlderAdult,
                MonitoringWorld.MedicationData(true, "8:00")))));
        Assert.Equal(MedicationCommandError.OlderAdultNotFound, FailureOf(await service.Handle(
            new CreateMedicationCommand(MonitoringWorld.Admin, 999, MonitoringWorld.MedicationData()))));
    }

    [Fact]
    public async Task UpdateMedication_DeactivatesIt()
    {
        var service = _world.MedicationCommands();
        var medication = SuccessOf(await service.Handle(new CreateMedicationCommand(MonitoringWorld.Caregiver,
            MonitoringWorld.OlderAdult, MonitoringWorld.MedicationData())));

        var updated = SuccessOf(await service.Handle(new UpdateMedicationCommand(medication.Id,
            MonitoringWorld.Caregiver, MonitoringWorld.OlderAdult, MonitoringWorld.MedicationData(active: false))));

        Assert.False(updated.Active);
        Assert.Equal(MedicationCommandError.NotFound, FailureOf(await service.Handle(new UpdateMedicationCommand(99,
            MonitoringWorld.Caregiver, MonitoringWorld.OlderAdult, MonitoringWorld.MedicationData()))));
        Assert.Equal(MedicationCommandError.Forbidden, FailureOf(await service.Handle(new UpdateMedicationCommand(
            medication.Id, MonitoringWorld.Family, MonitoringWorld.OlderAdult, MonitoringWorld.MedicationData()))));
    }
}
