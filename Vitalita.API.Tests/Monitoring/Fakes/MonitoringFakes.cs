using Microsoft.Extensions.Logging.Abstractions;
using Vitalita.API.Monitoring.Application.Internal.CommandServices;
using Vitalita.API.Monitoring.Application.Internal.OutboundServices;
using Vitalita.API.Monitoring.Application.Internal.Policies;
using Vitalita.API.Monitoring.Application.Internal.QueryServices;
using Vitalita.API.Monitoring.Domain.Model.Aggregates;
using Vitalita.API.Monitoring.Domain.Repositories;
using Vitalita.API.Shared.Domain.Model.ValueObjects;
using Vitalita.API.Tests.IAM.Fakes;
using Vitalita.API.Tests.Profiles.Fakes;

namespace Vitalita.API.Tests.Monitoring.Fakes;

internal sealed class InMemoryDailyReportRepository : InMemoryRepository<DailyReport>, IDailyReportRepository
{
    public Task<IReadOnlyList<DailyReport>> FindByOlderAdultIdAsync(int olderAdultId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<DailyReport>>(Items.Where(item => item.OlderAdultId == olderAdultId).ToList());
}

internal sealed class InMemoryVitalSignRepository : InMemoryRepository<VitalSign>, IVitalSignRepository
{
    public Task<IReadOnlyList<VitalSign>> FindByOlderAdultIdAsync(int olderAdultId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<VitalSign>>(Items.Where(item => item.OlderAdultId == olderAdultId).ToList());
}

internal sealed class InMemoryMedicationRepository : InMemoryRepository<Medication>, IMedicationRepository
{
    public Task<IReadOnlyList<Medication>> FindByOlderAdultIdAsync(int olderAdultId, bool? active = null,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Medication>>(Items
            .Where(item => item.OlderAdultId == olderAdultId && (active is null || item.Active == active)).ToList());

    public Task<IReadOnlyList<Medication>> FindAllAsync(bool? active = null,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Medication>>(Items.Where(item => active is null || item.Active == active)
            .ToList());
}

/// <summary>
///     Profiles answers for the tests: which older adults exist, who manages them and who can read them.
/// </summary>
internal sealed class FakeExternalProfilesService : IExternalProfilesService
{
    private readonly HashSet<int> _olderAdults = [];
    private readonly HashSet<(int UserId, int OlderAdultId)> _managers = [];
    private readonly HashSet<(int UserId, int OlderAdultId)> _readers = [];

    public FakeExternalProfilesService WithOlderAdult(int olderAdultId, int caregiverUserId,
        params int[] familyUserIds)
    {
        _olderAdults.Add(olderAdultId);
        _managers.Add((caregiverUserId, olderAdultId));
        _readers.Add((caregiverUserId, olderAdultId));
        foreach (var familyUserId in familyUserIds) _readers.Add((familyUserId, olderAdultId));
        return this;
    }

    public Task<bool> ExistsOlderAdultAsync(int olderAdultId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_olderAdults.Contains(olderAdultId));

    public Task<bool> CanReadOlderAdultAsync(int userId, int olderAdultId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(_readers.Contains((userId, olderAdultId)));

    public Task<bool> CanManageOlderAdultAsync(int userId, int olderAdultId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(_managers.Contains((userId, olderAdultId)));
}

/// <summary>
///     Monitoring world in memory. Older adult 1 is managed by caregiver user 1 and followed by family user 10;
///     older adult 2 is managed by caregiver user 2. User 3 is a caregiver with no older adults.
/// </summary>
internal sealed class MonitoringWorld
{
    public static readonly DateTime Now = new(2026, 11, 11, 15, 0, 0, DateTimeKind.Utc);

    public const int OlderAdult = 1;
    public const int OtherOlderAdult = 2;

    public InMemoryDailyReportRepository Reports { get; } = new();
    public InMemoryVitalSignRepository Signs { get; } = new();
    public InMemoryMedicationRepository Medications { get; } = new();
    public FakeUnitOfWork UnitOfWork { get; } = new();
    public FixedClock Clock { get; } = new(Now);

    public FakeExternalProfilesService Profiles { get; } = new FakeExternalProfilesService()
        .WithOlderAdult(OlderAdult, 1, 10)
        .WithOlderAdult(OtherOlderAdult, 2);

    public OlderAdultAccessPolicy Policy => new(Profiles);

    public static Requester Caregiver => new(1, Requester.CaregiverRole, "c1@vitalita.test");
    public static Requester OtherCaregiver => new(2, Requester.CaregiverRole, "c2@vitalita.test");
    public static Requester Outsider => new(3, Requester.CaregiverRole, "c3@vitalita.test");
    public static Requester Family => new(10, Requester.FamilyMemberRole, "f10@vitalita.test");
    public static Requester Admin => new(99, Requester.AdminRole, "admin@vitalita.test");

    public DailyReportCommandService ReportCommands() =>
        new(Reports, Policy, Clock, UnitOfWork, NullLogger<DailyReportCommandService>.Instance);

    public VitalSignCommandService SignCommands() =>
        new(Signs, Policy, Clock, UnitOfWork, NullLogger<VitalSignCommandService>.Instance);

    public MedicationCommandService MedicationCommands() =>
        new(Medications, Policy, UnitOfWork, NullLogger<MedicationCommandService>.Instance);

    public DailyReportQueryService ReportQueries() => new(Reports, Policy);

    public VitalSignQueryService SignQueries() => new(Signs, Policy);

    public MedicationQueryService MedicationQueries() => new(Medications, Policy);

    public static MedicationData MedicationData(bool active = true, params string[] times) =>
        new(" Losartán ", "50 mg", "Cada 24 horas", times.Length == 0 ? ["12:00"] : times, "Con el almuerzo.",
            active);
}
