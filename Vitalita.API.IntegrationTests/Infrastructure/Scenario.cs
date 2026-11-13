using System.Net;

namespace Vitalita.API.IntegrationTests.Infrastructure;

public sealed record Account(int UserId, string Email, string Token);

public sealed record Caregiver(Account Account, int ProfileId)
{
    public string Token => Account.Token;
}

public sealed record FamilyMember(Account Account, int ProfileId, int AccessId)
{
    public string Token => Account.Token;
}

/// <summary>
///     Builds the data each test needs through the public API. Every account gets a unique <c>@vitalita.test</c>
///     email, so tests do not depend on their order or on data left by earlier runs.
/// </summary>
public sealed class Scenario(Api api)
{
    public const string Password = "Integration-Test-1";

    public static string UniqueEmail(string prefix) => $"{prefix}.{Guid.NewGuid():N}@vitalita.test";

    public Task<Account> SignUpAsync(string role, string prefix) => SignUpWithEmailAsync(role, UniqueEmail(prefix));

    public async Task<string> SignInAsync(string email, string password = Password) =>
        (await api.PostAsync("/authentication/sign-in", new { email, password }))
        .ShouldBe(HttpStatusCode.OK).Str("token");

    public Task<string> AdminTokenAsync() =>
        SignInAsync(VitalitaApiFactory.AdminEmail, VitalitaApiFactory.AdminPassword);

    public async Task<Caregiver> CaregiverAsync(string prefix = "caregiver")
    {
        var account = await SignUpAsync("Caregiver", prefix);
        var profile = (await api.PostAsync("/caregiver-profiles",
                new { userId = account.UserId, firstName = "Andrea", lastName = "Mendoza" }, account.Token))
            .ShouldBe(HttpStatusCode.Created);
        return new Caregiver(account, profile.Id);
    }

    public async Task<int> OlderAdultAsync(Caregiver caregiver, string firstName = "María") =>
        (await api.PostAsync("/older-adults",
            new { caregiverId = caregiver.ProfileId, firstName, lastName = "Rodríguez", birthDate = "1948-03-12" },
            caregiver.Token))
        .ShouldBe(HttpStatusCode.Created).Id;

    public async Task<ApiResponse> InviteAsync(Caregiver caregiver, int olderAdultId, string email) =>
        (await api.PostAsync("/family-access",
            new { olderAdultId, invitedEmail = email, relationship = "Daughter" }, caregiver.Token))
        .ShouldBe(HttpStatusCode.Created);

    /// <summary>Invitation, family member account and profile, and the accepted (Active) access.</summary>
    public async Task<FamilyMember> FamilyMemberWithAccessAsync(Caregiver caregiver, int olderAdultId)
    {
        var email = UniqueEmail("family");
        var accessId = (await InviteAsync(caregiver, olderAdultId, email)).Id;
        var account = await SignUpWithEmailAsync("FamilyMember", email);
        var profileId = (await api.PostAsync("/family-member-profiles",
                new { userId = account.UserId, firstName = "Carla", lastName = "Rodríguez" }, account.Token))
            .ShouldBe(HttpStatusCode.Created).Id;
        (await api.PutAsync($"/family-access/{accessId}", new { status = "Active", familyMemberId = profileId },
            account.Token)).ShouldBe(HttpStatusCode.OK);
        return new FamilyMember(account, profileId, accessId);
    }

    public async Task<Account> SignUpWithEmailAsync(string role, string email)
    {
        var signUp = (await api.PostAsync("/authentication/sign-up", new { email, password = Password, role }))
            .ShouldBe(HttpStatusCode.Created);
        return new Account(signUp.Id, email, await SignInAsync(email));
    }
}
