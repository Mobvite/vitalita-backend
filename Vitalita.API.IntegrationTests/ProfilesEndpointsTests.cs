using System.Net;
using Vitalita.API.IntegrationTests.Infrastructure;

namespace Vitalita.API.IntegrationTests;

/// <summary>Profiles (TS02, TS03): /caregiver-profiles, /older-adults, /family-access, /family-member-profiles.</summary>
public sealed class ProfilesEndpointsTests(VitalitaApiFactory factory) : ApiTestBase(factory)
{
    [Fact]
    public async Task CreateCaregiverProfile_WithTheSignUpFormBody_ReturnsTheProfile()
    {
        var account = await Given.SignUpAsync("Caregiver", "cg-profile");

        var profile = (await Api.PostAsync("/caregiver-profiles", new
        {
            userId = account.UserId, firstName = "Andrea", lastName = "Mendoza", phoneNumber = "",
            professionalTitle = "", bio = "", photoUrl = "", certifications = Array.Empty<string>()
        }, account.Token)).ShouldBe(HttpStatusCode.Created);

        Assert.Equal(account.UserId, profile.Json.GetProperty("userId").GetInt32());
        Assert.Equal("Andrea", profile.Str("firstName"));
        Assert.Equal("", profile.Str("phoneNumber"));
        Assert.Empty(profile.Json.GetProperty("certifications").EnumerateArray());
        Assert.EndsWith($"/api/v1/caregiver-profiles/{profile.Id}", profile.Location?.ToString());

        var byUser = (await Api.GetAsync($"/caregiver-profiles?userId={account.UserId}", account.Token))
            .ShouldBe(HttpStatusCode.OK);
        Assert.Equal(profile.Id, Assert.Single(byUser.Items).GetProperty("id").GetInt32());
    }

    [Fact]
    public async Task CreateOlderAdult_ReturnsDateOnlyBirthDateAndClinicalLists()
    {
        var caregiver = await Given.CaregiverAsync();

        var olderAdult = (await Api.PostAsync("/older-adults", new
        {
            caregiverId = caregiver.ProfileId, firstName = "María", lastName = "Rodríguez", birthDate = "1948-03-12",
            documentNumber = "40218463", bloodType = "O+", allergies = new[] { "Penicilina" },
            chronicConditions = new[] { "Hipertensión arterial", "Diabetes tipo 2" },
            emergencyContact = new { name = "Renzo Rodríguez", phoneNumber = "987111222", relationship = "Son" },
            observations = "Requiere asistencia para caminar.", status = "Active"
        }, caregiver.Token)).ShouldBe(HttpStatusCode.Created);

        Assert.Equal(caregiver.ProfileId, olderAdult.Json.GetProperty("caregiverId").GetInt32());
        Assert.Equal("1948-03-12", olderAdult.Str("birthDate"));
        Assert.Equal("Penicilina", Assert.Single(olderAdult.Json.GetProperty("allergies").EnumerateArray()).GetString());
        Assert.Equal(2, olderAdult.Json.GetProperty("chronicConditions").GetArrayLength());
        Assert.Equal("Son", olderAdult.Json.GetProperty("emergencyContact").GetProperty("relationship").GetString());
        Assert.Equal("Active", olderAdult.Str("status"));
    }

    [Fact]
    public async Task CreateInvitation_IgnoresTheCodeStatusAndDatesSentByTheClient()
    {
        var caregiver = await Given.CaregiverAsync();
        var olderAdultId = await Given.OlderAdultAsync(caregiver);
        var email = Scenario.UniqueEmail("invited");

        var access = (await Api.PostAsync("/family-access", new
        {
            olderAdultId, invitedEmail = email, invitationCode = "VITA-AAAA", relationship = "Daughter",
            status = "Active", invitedAt = "2020-01-01T00:00:00Z", expiresAt = "2020-01-08T00:00:00Z"
        }, caregiver.Token)).ShouldBe(HttpStatusCode.Created);

        Assert.Matches("^VITA-[A-Z0-9]{8}$", access.Str("invitationCode"));
        Assert.Equal("Pending", access.Str("status"));
        Assert.Equal(email, access.Str("invitedEmail"));
        Assert.Equal(System.Text.Json.JsonValueKind.Null, access.Json.GetProperty("familyMemberId").ValueKind);
        var invitedAt = access.Json.GetProperty("invitedAt").GetDateTime();
        Assert.True(Math.Abs((DateTime.UtcNow - invitedAt).TotalMinutes) < 5, $"invitedAt: {invitedAt:O}");
        Assert.Equal(TimeSpan.FromDays(7), access.Json.GetProperty("expiresAt").GetDateTime() - invitedAt);
    }

    [Fact]
    public async Task CreateInvitation_ForAnEmailWithAPendingAccess_Returns409()
    {
        var caregiver = await Given.CaregiverAsync();
        var olderAdultId = await Given.OlderAdultAsync(caregiver);
        var email = Scenario.UniqueEmail("twice");
        await Given.InviteAsync(caregiver, olderAdultId, email);

        (await Api.PostAsync("/family-access", new { olderAdultId, invitedEmail = email, relationship = "Son" },
                caregiver.Token))
            .ShouldBeProblem(HttpStatusCode.Conflict, Messages.InvitationAlreadyExists);
    }

    [Fact]
    public async Task PublicInvitationLookup_WorksWithoutTokenAndIgnoresCase_AndUnknownCodesReturnEmpty()
    {
        var caregiver = await Given.CaregiverAsync();
        var olderAdultId = await Given.OlderAdultAsync(caregiver);
        var invitation = await Given.InviteAsync(caregiver, olderAdultId, Scenario.UniqueEmail("lookup"));
        var code = invitation.Str("invitationCode");

        var found = (await Api.GetAsync($"/family-access?invitationCode={code.ToLowerInvariant()}"))
            .ShouldBe(HttpStatusCode.OK);
        var match = Assert.Single(found.Items);
        Assert.Equal(invitation.Id, match.GetProperty("id").GetInt32());
        Assert.Equal(code, match.GetProperty("invitationCode").GetString());
        Assert.Equal("Pending", match.GetProperty("status").GetString());
        // Minimal public shape: nothing that identifies the older adult or the family member.
        Assert.False(match.TryGetProperty("olderAdultId", out _));
        Assert.False(match.TryGetProperty("familyMemberId", out _));

        var unknown = (await Api.GetAsync("/family-access?invitationCode=VITA-NOPE0000")).ShouldBe(HttpStatusCode.OK);
        Assert.Empty(unknown.Items);
    }

    [Fact]
    public async Task CreateFamilyMemberProfile_RequiresAPendingInvitationForTheTokenEmail()
    {
        var uninvited = await Given.SignUpAsync("FamilyMember", "uninvited");

        (await Api.PostAsync("/family-member-profiles",
                new { userId = uninvited.UserId, firstName = "Carla", lastName = "Rodríguez" }, uninvited.Token))
            .ShouldBeProblem(HttpStatusCode.Forbidden, Messages.NoValidInvitation);

        var caregiver = await Given.CaregiverAsync();
        var olderAdultId = await Given.OlderAdultAsync(caregiver);
        var email = Scenario.UniqueEmail("invited-family");
        await Given.InviteAsync(caregiver, olderAdultId, email);
        var invited = await Given.SignUpWithEmailAsync("FamilyMember", email);

        var profile = (await Api.PostAsync("/family-member-profiles",
                new { userId = invited.UserId, firstName = "Carla", lastName = "Rodríguez", phoneNumber = "945222333" },
                invited.Token))
            .ShouldBe(HttpStatusCode.Created);
        Assert.Equal(invited.UserId, profile.Json.GetProperty("userId").GetInt32());
        Assert.Equal("945222333", profile.Str("phoneNumber"));
    }

    [Fact]
    public async Task AcceptingTheInvitation_ActivatesTheAccessAndGrantsReadAccess()
    {
        var caregiver = await Given.CaregiverAsync();
        var olderAdultId = await Given.OlderAdultAsync(caregiver);
        var email = Scenario.UniqueEmail("accept");
        var invitation = await Given.InviteAsync(caregiver, olderAdultId, email);
        var family = await Given.SignUpWithEmailAsync("FamilyMember", email);
        var familyProfileId = (await Api.PostAsync("/family-member-profiles",
                new { userId = family.UserId, firstName = "Carla", lastName = "Rodríguez" }, family.Token))
            .ShouldBe(HttpStatusCode.Created).Id;

        var accepted = (await Api.PutAsync($"/family-access/{invitation.Id}",
                new { status = "Active", familyMemberId = familyProfileId }, family.Token))
            .ShouldBe(HttpStatusCode.OK);
        Assert.Equal("Active", accepted.Str("status"));
        Assert.Equal(familyProfileId, accepted.Json.GetProperty("familyMemberId").GetInt32());
        Assert.EndsWith("Z", accepted.Str("acceptedAt"));

        // Once accepted, the code is no longer valid for the public lookup.
        var lookup = (await Api.GetAsync($"/family-access?invitationCode={invitation.Str("invitationCode")}"))
            .ShouldBe(HttpStatusCode.OK);
        Assert.Empty(lookup.Items);

        var accesses = (await Api.GetAsync($"/family-access?familyMemberId={familyProfileId}&status=Active",
            family.Token)).ShouldBe(HttpStatusCode.OK);
        Assert.Equal(invitation.Id, Assert.Single(accesses.Items).GetProperty("id").GetInt32());

        // Batch filter: ids the requester cannot read are dropped silently.
        var olderAdults = (await Api.GetAsync($"/older-adults?id={olderAdultId}&id={UnknownId}", family.Token))
            .ShouldBe(HttpStatusCode.OK);
        Assert.Equal(olderAdultId, Assert.Single(olderAdults.Items).GetProperty("id").GetInt32());

        (await Api.GetAsync($"/caregiver-profiles/{caregiver.ProfileId}", family.Token)).ShouldBe(HttpStatusCode.OK);
        var names = (await Api.GetAsync($"/family-member-profiles?id={familyProfileId}", caregiver.Token))
            .ShouldBe(HttpStatusCode.OK);
        Assert.Equal("Carla", Assert.Single(names.Items).GetProperty("firstName").GetString());
    }

    [Fact]
    public async Task FamilyMember_CannotEditTheOlderAdult()
    {
        var caregiver = await Given.CaregiverAsync();
        var olderAdultId = await Given.OlderAdultAsync(caregiver);
        var family = await Given.FamilyMemberWithAccessAsync(caregiver, olderAdultId);

        (await Api.PutAsync($"/older-adults/{olderAdultId}", new
            {
                caregiverId = caregiver.ProfileId, firstName = "X", lastName = "Y", birthDate = "1948-03-12",
                status = "Active"
            }, family.Token))
            .ShouldBeProblem(HttpStatusCode.Forbidden, Messages.ProfileAccessForbidden);
    }

    [Fact]
    public async Task AnotherCaregiver_CannotReadTheOlderAdultOrFilterByAForeignCaregiver()
    {
        var caregiver = await Given.CaregiverAsync();
        var olderAdultId = await Given.OlderAdultAsync(caregiver);
        var outsider = await Given.CaregiverAsync("outsider");

        (await Api.GetAsync($"/older-adults/{olderAdultId}", outsider.Token))
            .ShouldBeProblem(HttpStatusCode.Forbidden, Messages.ProfileAccessForbidden);
        (await Api.GetAsync($"/older-adults?caregiverId={caregiver.ProfileId}", outsider.Token))
            .ShouldBeProblem(HttpStatusCode.Forbidden, Messages.ProfileAccessForbidden);

        var own = (await Api.GetAsync($"/older-adults?caregiverId={caregiver.ProfileId}", caregiver.Token))
            .ShouldBe(HttpStatusCode.OK);
        Assert.Equal(olderAdultId, Assert.Single(own.Items).GetProperty("id").GetInt32());
    }

    [Fact]
    public async Task ListsWithoutFilters_AreAdminOnly()
    {
        var caregiver = await Given.CaregiverAsync();

        (await Api.GetAsync("/older-adults", caregiver.Token))
            .ShouldBeProblem(HttpStatusCode.Forbidden, Messages.ProfileAccessForbidden);
        (await Api.GetAsync("/caregiver-profiles", caregiver.Token))
            .ShouldBeProblem(HttpStatusCode.Forbidden, Messages.ProfileAccessForbidden);

        var adminToken = await Given.AdminTokenAsync();
        var all = (await Api.GetAsync("/caregiver-profiles", adminToken)).ShouldBe(HttpStatusCode.OK);
        Assert.Contains(all.Items, profile => profile.GetProperty("id").GetInt32() == caregiver.ProfileId);
    }

    [Fact]
    public async Task DuplicateProfile_Returns409_CaregiverChange_Returns409_AndInvalidDniReturns400()
    {
        var caregiver = await Given.CaregiverAsync();
        var other = await Given.CaregiverAsync("other-owner");
        var olderAdultId = await Given.OlderAdultAsync(caregiver);

        (await Api.PostAsync("/caregiver-profiles",
                new { userId = caregiver.Account.UserId, firstName = "Andrea", lastName = "Mendoza" }, caregiver.Token))
            .ShouldBeProblem(HttpStatusCode.Conflict, Messages.ProfileAlreadyExists);

        (await Api.PutAsync($"/older-adults/{olderAdultId}",
                new
                {
                    caregiverId = other.ProfileId, firstName = "María", lastName = "Rodríguez",
                    birthDate = "1948-03-12", status = "Active"
                },
                caregiver.Token))
            .ShouldBeProblem(HttpStatusCode.Conflict, Messages.OlderAdultCaregiverChangeNotAllowed);

        (await Api.PostAsync("/older-adults", new
            {
                caregiverId = caregiver.ProfileId, firstName = "Jorge", lastName = "Salazar", birthDate = "1941-07-25",
                documentNumber = "123"
            }, caregiver.Token))
            .ShouldBeValidationProblem("documentNumber", Messages.DocumentNumberFormat);
    }
}
