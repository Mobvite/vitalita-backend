using Microsoft.Extensions.Logging.Abstractions;
using Vitalita.API.IAM.Interfaces.ACL;
using Vitalita.API.Profiles.Application.Internal.CommandServices;
using Vitalita.API.Profiles.Application.Internal.Policies;
using Vitalita.API.Profiles.Application.Internal.QueryServices;
using Vitalita.API.Profiles.Domain.Model.Aggregates;
using Vitalita.API.Profiles.Domain.Model.ValueObjects;
using Vitalita.API.Profiles.Domain.Repositories;
using Vitalita.API.Profiles.Domain.Services;
using Vitalita.API.Profiles.Interfaces.ACL.Services;
using Vitalita.API.Tests.IAM.Fakes;

namespace Vitalita.API.Tests.Profiles.Fakes;

/// <summary>Sets the private identifier of an aggregate, like the database does on insert.</summary>
internal static class EntityId
{
    public static void Set(object entity, int id) => entity.GetType().GetProperty("Id")!.SetValue(entity, id);
}

/// <summary>Generic in-memory repository; ids are assigned on add.</summary>
internal abstract class InMemoryRepository<T> where T : class
{
    protected readonly List<T> Items = [];
    private int _nextId = 1;

    public IReadOnlyList<T> All => Items;

    public Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        EntityId.Set(entity, _nextId++);
        Items.Add(entity);
        return Task.CompletedTask;
    }

    public Task<T?> FindByIdAsync(int id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.FirstOrDefault(item => IdOf(item) == id));

    public void Update(T entity)
    {
    }

    public void Remove(T entity) => Items.Remove(entity);

    public Task<IEnumerable<T>> ListAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IEnumerable<T>>(Items.ToList());

    private static int IdOf(T item) => (int)typeof(T).GetProperty("Id")!.GetValue(item)!;
}

internal sealed class InMemoryCaregiverProfileRepository
    : InMemoryRepository<CaregiverProfile>, ICaregiverProfileRepository
{
    public Task<CaregiverProfile?> FindByUserIdAsync(int userId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.FirstOrDefault(profile => profile.UserId == userId));

    public Task<bool> ExistsByUserIdAsync(int userId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.Any(profile => profile.UserId == userId));

    public Task<bool> ExistsByIdAsync(int id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.Any(profile => profile.Id == id));
}

internal sealed class InMemoryFamilyMemberProfileRepository
    : InMemoryRepository<FamilyMemberProfile>, IFamilyMemberProfileRepository
{
    public Task<FamilyMemberProfile?> FindByUserIdAsync(int userId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.FirstOrDefault(profile => profile.UserId == userId));

    public Task<bool> ExistsByUserIdAsync(int userId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.Any(profile => profile.UserId == userId));

    public Task<IReadOnlyList<FamilyMemberProfile>> FindByIdsAsync(IReadOnlyCollection<int> ids,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<FamilyMemberProfile>>(
            Items.Where(profile => ids.Contains(profile.Id)).OrderBy(profile => profile.Id).ToList());
}

internal sealed class InMemoryOlderAdultRepository : InMemoryRepository<OlderAdult>, IOlderAdultRepository
{
    public Task<IReadOnlyList<OlderAdult>> FindByCaregiverIdAsync(int caregiverId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<OlderAdult>>(Items.Where(item => item.CaregiverId == caregiverId).ToList());

    public Task<IReadOnlyList<OlderAdult>> FindByIdsAsync(IReadOnlyCollection<int> ids,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<OlderAdult>>(
            Items.Where(item => ids.Contains(item.Id)).OrderBy(item => item.Id).ToList());
}

internal sealed class InMemoryFamilyAccessRepository : InMemoryRepository<FamilyAccess>, IFamilyAccessRepository
{
    public Task<IReadOnlyList<FamilyAccess>> FindByOlderAdultIdsAsync(IReadOnlyCollection<int> olderAdultIds,
        AccessStatus? status = null, CancellationToken cancellationToken = default) =>
        Result(access => olderAdultIds.Contains(access.OlderAdultId), status);

    public Task<IReadOnlyList<FamilyAccess>> FindByFamilyMemberIdAsync(int familyMemberId,
        AccessStatus? status = null, CancellationToken cancellationToken = default) =>
        Result(access => access.FamilyMemberId == familyMemberId, status);

    public Task<IReadOnlyList<FamilyAccess>> FindAllAsync(AccessStatus? status = null,
        CancellationToken cancellationToken = default) =>
        Result(_ => true, status);

    public Task<FamilyAccess?> FindByInvitationCodeAsync(InvitationCode code,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.FirstOrDefault(access => access.InvitationCode == code));

    public Task<bool> ExistsByInvitationCodeAsync(InvitationCode code, CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.Any(access => access.InvitationCode == code));

    public Task<bool> ExistsOpenAccessAsync(int olderAdultId, string invitedEmail,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.Any(access => access.OlderAdultId == olderAdultId &&
                                            access.InvitedEmail == invitedEmail &&
                                            access.Status != AccessStatus.Revoked));

    public Task<bool> ExistsValidInvitationForEmailAsync(string invitedEmail, DateTime utcNow,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.Any(access => access.InvitedEmail == invitedEmail && access.CanBeAccepted(utcNow)));

    public Task<bool> ExistsActiveAccessAsync(int familyMemberId, int olderAdultId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.Any(access => access.FamilyMemberId == familyMemberId &&
                                            access.OlderAdultId == olderAdultId && access.IsActive));

    private Task<IReadOnlyList<FamilyAccess>> Result(Func<FamilyAccess, bool> filter, AccessStatus? status) =>
        Task.FromResult<IReadOnlyList<FamilyAccess>>(Items.Where(filter)
            .Where(access => status is null || access.Status == status).OrderBy(access => access.Id).ToList());
}

/// <summary>IAM facade with a fixed set of existing users.</summary>
internal sealed class FakeIamContextFacade(params int[] existingUserIds) : IIamContextFacade
{
    public Task<bool> ExistsUserByIdAsync(int userId, CancellationToken cancellationToken = default) =>
        Task.FromResult(existingUserIds.Contains(userId));
}

/// <summary>Returns the queued codes in order, then repeats the last one.</summary>
internal sealed class QueuedInvitationCodeGenerator(params string[] codes) : IInvitationCodeGenerator
{
    private int _index;

    public int Calls { get; private set; }

    public InvitationCode Generate()
    {
        Calls++;
        var code = codes[Math.Min(_index, codes.Length - 1)];
        _index++;
        return new InvitationCode(code);
    }
}

/// <summary>
///     Profiles world in memory: repositories, policy, services and helpers to build the common scenario
///     (a caregiver with an older adult, a family member with an active access, and outsiders).
/// </summary>
internal sealed class ProfilesWorld
{
    public static readonly DateTime Now = new(2026, 11, 11, 15, 0, 0, DateTimeKind.Utc);

    public InMemoryCaregiverProfileRepository Caregivers { get; } = new();
    public InMemoryFamilyMemberProfileRepository FamilyMembers { get; } = new();
    public InMemoryOlderAdultRepository OlderAdults { get; } = new();
    public InMemoryFamilyAccessRepository Accesses { get; } = new();
    public FakeUnitOfWork UnitOfWork { get; } = new();
    public FixedClock Clock { get; private set; } = new(Now);
    public FakeIamContextFacade Iam { get; set; } = new(Enumerable.Range(1, 50).ToArray());
    public QueuedInvitationCodeGenerator CodeGenerator { get; set; } = new("VITA-AAAAAAAA", "VITA-BBBBBBBB");

    public ProfileAccessPolicy Policy => new(Caregivers, FamilyMembers, OlderAdults, Accesses);

    public static Requester Admin => new(99, Requester.AdminRole, "admin@vitalita.test");

    public static Requester Caregiver(int userId) => new(userId, Requester.CaregiverRole, $"c{userId}@vitalita.test");

    public static Requester Family(int userId, string? email = null) =>
        new(userId, Requester.FamilyMemberRole, email ?? $"f{userId}@vitalita.test");

    public void AdvanceTo(DateTime utcNow) => Clock = new FixedClock(utcNow);

    public CaregiverProfileCommandService CaregiverCommands() =>
        new(Caregivers, Iam, UnitOfWork, NullLogger<CaregiverProfileCommandService>.Instance);

    public FamilyMemberProfileCommandService FamilyMemberCommands() =>
        new(FamilyMembers, Accesses, Iam, Clock, UnitOfWork, NullLogger<FamilyMemberProfileCommandService>.Instance);

    public OlderAdultCommandService OlderAdultCommands() =>
        new(OlderAdults, Caregivers, Policy, Clock, UnitOfWork, NullLogger<OlderAdultCommandService>.Instance);

    public FamilyAccessCommandService FamilyAccessCommands() =>
        new(Accesses, OlderAdults, FamilyMembers, Policy, CodeGenerator, Clock, UnitOfWork,
            NullLogger<FamilyAccessCommandService>.Instance);

    public CaregiverProfileQueryService CaregiverQueries() => new(Caregivers, Policy);

    public FamilyMemberProfileQueryService FamilyMemberQueries() => new(FamilyMembers, Policy);

    public OlderAdultQueryService OlderAdultQueries() => new(OlderAdults, Policy);

    public FamilyAccessQueryService FamilyAccessQueries() => new(Accesses, Policy, Clock);

    public ProfilesContextFacade Facade() => new(OlderAdults, Caregivers, FamilyMembers, Accesses, Policy);

    public async Task<CaregiverProfile> AddCaregiverAsync(int userId)
    {
        var profile = new CaregiverProfile(userId, "Andrea", "Mendoza", "987654321", "Lic.", "", "", []);
        await Caregivers.AddAsync(profile);
        return profile;
    }

    public async Task<FamilyMemberProfile> AddFamilyMemberAsync(int userId)
    {
        var profile = new FamilyMemberProfile(userId, "Renzo", "Rodríguez", "987111222");
        await FamilyMembers.AddAsync(profile);
        return profile;
    }

    public async Task<OlderAdult> AddOlderAdultAsync(int caregiverId)
    {
        var olderAdult = new OlderAdult(caregiverId, OlderAdultTestData.Valid(), new DateOnly(2026, 11, 11));
        await OlderAdults.AddAsync(olderAdult);
        return olderAdult;
    }

    public async Task<FamilyAccess> AddInvitationAsync(int olderAdultId, string email, string code = "VITA-ZZZZZZZZ")
    {
        var access = new FamilyAccess(olderAdultId, email, FamilyRelationship.Son, new InvitationCode(code), Now);
        await Accesses.AddAsync(access);
        return access;
    }

    public async Task<FamilyAccess> AddActiveAccessAsync(int olderAdultId, FamilyMemberProfile familyMember,
        string email, string code = "VITA-YYYYYYYY")
    {
        var access = await AddInvitationAsync(olderAdultId, email, code);
        access.Accept(familyMember.Id, email, Now);
        return access;
    }
}

internal static class OlderAdultTestData
{
    public static OlderAdultData Valid(DateOnly? birthDate = null, OlderAdultStatus status = OlderAdultStatus.Active,
        string? bloodType = "O+") =>
        new("  María ", "Rodríguez", birthDate ?? new DateOnly(1948, 3, 12), "40218463", bloodType,
            [" Penicilina ", "", "penicilina"], ["Hipertensión arterial"],
            EmergencyContact.Create(" Renzo Rodríguez ", "987111222", FamilyRelationship.Son),
            "Requiere asistencia.", status);
}
