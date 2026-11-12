using Vitalita.API.Monitoring.Application.Errors;
using Vitalita.API.Monitoring.Domain.Model.Aggregates;
using Vitalita.API.Monitoring.Domain.Model.Queries;
using Vitalita.API.Monitoring.Domain.Model.ValueObjects;
using Vitalita.API.Shared.Application.Patterns;
using Vitalita.API.Tests.Monitoring.Fakes;

namespace Vitalita.API.Tests.Monitoring.Application.Internal.QueryServices;

public class MonitoringQueryServicesTests
{
    private static readonly DateTime Now = MonitoringWorld.Now;
    private readonly MonitoringWorld _world = new();

    private static TError FailureOf<TValue, TError>(Result<TValue, TError> result) =>
        Assert.IsType<Result<TValue, TError>.Failure>(result).Error;

    private static TValue SuccessOf<TValue, TError>(Result<TValue, TError> result) =>
        Assert.IsType<Result<TValue, TError>.Success>(result).Value;

    [Fact]
    public async Task Reports_ByOlderAdult_ForCaregiverAndFamily_NotForOthers()
    {
        var mine = new DailyReport(MonitoringWorld.OlderAdult, 1, Now, Mood.Good, GeneralCondition.Stable, "a", Now);
        await _world.Reports.AddAsync(mine);
        await _world.Reports.AddAsync(new DailyReport(MonitoringWorld.OtherOlderAdult, 2, Now, Mood.Good,
            GeneralCondition.Stable, "b", Now));
        var service = _world.ReportQueries();

        Assert.Equal([mine], SuccessOf(await service.Handle(
            new GetDailyReportsByOlderAdultIdQuery(MonitoringWorld.Caregiver, MonitoringWorld.OlderAdult))));
        Assert.Equal([mine], SuccessOf(await service.Handle(
            new GetDailyReportsByOlderAdultIdQuery(MonitoringWorld.Family, MonitoringWorld.OlderAdult))));
        Assert.Equal(MonitoringQueryError.Forbidden, FailureOf(await service.Handle(
            new GetDailyReportsByOlderAdultIdQuery(MonitoringWorld.OtherCaregiver, MonitoringWorld.OlderAdult))));
        Assert.Equal(MonitoringQueryError.Forbidden, FailureOf(await service.Handle(
            new GetDailyReportsByOlderAdultIdQuery(MonitoringWorld.Caregiver, 999))));
        Assert.Empty(SuccessOf(await service.Handle(new GetDailyReportsByOlderAdultIdQuery(MonitoringWorld.Admin, 999))));
    }

    [Fact]
    public async Task Lists_WithoutFilter_AreForAdminsOnly()
    {
        await _world.Signs.AddAsync(new VitalSign(MonitoringWorld.OlderAdult, VitalSignType.HeartRate, 70, null, Now,
            Now));

        Assert.Equal(MonitoringQueryError.Forbidden,
            FailureOf(await _world.SignQueries().Handle(new GetAllVitalSignsQuery(MonitoringWorld.Caregiver))));
        Assert.Single(SuccessOf(await _world.SignQueries().Handle(new GetAllVitalSignsQuery(MonitoringWorld.Admin))));
        Assert.Equal(MonitoringQueryError.Forbidden, FailureOf(await _world.ReportQueries()
            .Handle(new GetAllDailyReportsQuery(MonitoringWorld.Family))));
    }

    [Fact]
    public async Task SignById_NotFoundAndForbidden()
    {
        var sign = new VitalSign(MonitoringWorld.OlderAdult, VitalSignType.HeartRate, 70, null, Now, Now);
        await _world.Signs.AddAsync(sign);
        var service = _world.SignQueries();

        Assert.Equal(sign, SuccessOf(await service.Handle(new GetVitalSignByIdQuery(MonitoringWorld.Family, sign.Id))));
        Assert.Equal(MonitoringQueryError.Forbidden,
            FailureOf(await service.Handle(new GetVitalSignByIdQuery(MonitoringWorld.Outsider, sign.Id))));
        Assert.Equal(MonitoringQueryError.NotFound,
            FailureOf(await service.Handle(new GetVitalSignByIdQuery(MonitoringWorld.Caregiver, 99))));
    }

    [Fact]
    public async Task Medications_ActiveFilter()
    {
        var active = new Medication(MonitoringWorld.OlderAdult, MonitoringWorld.MedicationData());
        var inactive = new Medication(MonitoringWorld.OlderAdult, MonitoringWorld.MedicationData(active: false));
        await _world.Medications.AddAsync(active);
        await _world.Medications.AddAsync(inactive);
        var service = _world.MedicationQueries();

        Assert.Equal(2, SuccessOf(await service.Handle(new GetMedicationsByOlderAdultIdQuery(MonitoringWorld.Family,
            MonitoringWorld.OlderAdult, null))).Count);
        Assert.Equal([active], SuccessOf(await service.Handle(new GetMedicationsByOlderAdultIdQuery(
            MonitoringWorld.Caregiver, MonitoringWorld.OlderAdult, true))));
        Assert.Equal([inactive], SuccessOf(await service.Handle(new GetAllMedicationsQuery(MonitoringWorld.Admin,
            false))));
        Assert.Equal(MonitoringQueryError.Forbidden, FailureOf(await service.Handle(
            new GetMedicationByIdQuery(MonitoringWorld.OtherCaregiver, active.Id))));
    }
}
