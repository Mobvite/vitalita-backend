using System.Net;
using Vitalita.API.IntegrationTests.Infrastructure;

namespace Vitalita.API.IntegrationTests;

/// <summary>IAM (TS01): /authentication and /users.</summary>
public sealed class IamEndpointsTests(VitalitaApiFactory factory) : ApiTestBase(factory)
{
    [Fact]
    public async Task SignUp_ReturnsCreatedUserWithLocation_AndNeverExposesThePassword()
    {
        var email = Scenario.UniqueEmail("signup");

        var response = (await Api.PostAsync("/authentication/sign-up",
            new { email, password = Scenario.Password, role = "FamilyMember" })).ShouldBe(HttpStatusCode.Created);

        Assert.True(response.Id > 0);
        Assert.Equal(email, response.Str("email"));
        Assert.Equal("FamilyMember", response.Str("role"));
        Assert.Equal("Active", response.Str("status"));
        Assert.EndsWith("Z", response.Str("createdAt"));
        Assert.EndsWith($"/api/v1/users/{response.Id}", response.Location?.ToString());
        Assert.False(response.Json.TryGetProperty("password", out _));
        Assert.False(response.Json.TryGetProperty("passwordHash", out _));
    }

    [Fact]
    public async Task SignIn_WithValidCredentials_ReturnsToken_AndWithWrongPasswordReturns401()
    {
        var account = await Given.SignUpAsync("Caregiver", "signin");

        var response = (await Api.PostAsync("/authentication/sign-in",
            new { email = account.Email, password = Scenario.Password })).ShouldBe(HttpStatusCode.OK);
        Assert.Equal(account.UserId, response.Id);
        Assert.Equal(account.Email, response.Str("email"));
        Assert.Equal("Caregiver", response.Str("role"));
        Assert.Equal("Active", response.Str("status"));
        Assert.Equal(3, response.Str("token").Split('.').Length);

        (await Api.PostAsync("/authentication/sign-in", new { email = account.Email, password = "Wrong-Password-1" }))
            .ShouldBeProblem(HttpStatusCode.Unauthorized, Messages.InvalidCredentials);
    }

    [Fact]
    public async Task Me_WithToken_ReturnsTheCurrentUser_AndWithoutTokenReturns401()
    {
        var account = await Given.SignUpAsync("Caregiver", "me");

        var me = (await Api.GetAsync("/authentication/me", account.Token)).ShouldBe(HttpStatusCode.OK);
        Assert.Equal(account.UserId, me.Id);
        Assert.Equal(account.Email, me.Str("email"));
        Assert.EndsWith("Z", me.Str("lastLoginAt"));

        (await Api.GetAsync("/authentication/me")).ShouldBeProblem(HttpStatusCode.Unauthorized, Messages.Unauthorized);
    }

    [Fact]
    public async Task Refresh_ReturnsTheSignInShapeWithATokenThatAuthenticates()
    {
        var account = await Given.SignUpAsync("FamilyMember", "refresh");

        var refreshed = (await Api.PostAsync("/authentication/refresh", null, account.Token))
            .ShouldBe(HttpStatusCode.OK);
        Assert.Equal(account.UserId, refreshed.Id);
        Assert.Equal("FamilyMember", refreshed.Str("role"));

        (await Api.GetAsync("/authentication/me", refreshed.Str("token"))).ShouldBe(HttpStatusCode.OK);
        (await Api.PostAsync("/authentication/refresh", null, "not-a-jwt"))
            .ShouldBeProblem(HttpStatusCode.Unauthorized, Messages.Unauthorized);
    }

    [Fact]
    public async Task SignUp_WithAdminRole_IsRejectedWith400()
    {
        (await Api.PostAsync("/authentication/sign-up",
                new { email = Scenario.UniqueEmail("admin-signup"), password = Scenario.Password, role = "Admin" }))
            .ShouldBeValidationProblem("role", Messages.AdminSignUpNotAllowed);
    }

    [Fact]
    public async Task SignUp_WithAnEmailInUse_Returns409_EvenWithDifferentCase()
    {
        var account = await Given.SignUpAsync("Caregiver", "duplicate");

        (await Api.PostAsync("/authentication/sign-up",
                new { email = account.Email.ToUpperInvariant(), password = Scenario.Password, role = "Caregiver" }))
            .ShouldBeProblem(HttpStatusCode.Conflict, Messages.EmailAlreadyInUse);
    }

    [Fact]
    public async Task Admin_CreatesReadsUpdatesAndSoftDeletesUsers()
    {
        var adminToken = await Given.AdminTokenAsync();
        var email = Scenario.UniqueEmail("managed");

        var created = (await Api.PostAsync("/users", new { email, password = Scenario.Password, role = "Caregiver" },
            adminToken)).ShouldBe(HttpStatusCode.Created);
        var id = created.Id;

        var all = (await Api.GetAsync("/users", adminToken)).ShouldBe(HttpStatusCode.OK);
        Assert.Contains(all.Items, user => user.GetProperty("id").GetInt32() == id);

        var updated = (await Api.PutAsync($"/users/{id}", new { email, role = "FamilyMember", status = "Active" },
            adminToken)).ShouldBe(HttpStatusCode.OK);
        Assert.Equal("FamilyMember", updated.Str("role"));

        (await Api.DeleteAsync($"/users/{id}", adminToken)).ShouldBe(HttpStatusCode.NoContent);

        // Logical delete: the user still exists, now inactive.
        var deleted = (await Api.GetAsync($"/users/{id}", adminToken)).ShouldBe(HttpStatusCode.OK);
        Assert.Equal("Inactive", deleted.Str("status"));
    }

    [Fact]
    public async Task NonAdmin_CannotListReadOthersOrDeleteUsers_AndTheEmailLookupIsMinimal()
    {
        var caregiver = await Given.SignUpAsync("Caregiver", "nonadmin");
        var other = await Given.SignUpAsync("Caregiver", "other");

        (await Api.GetAsync("/users", caregiver.Token))
            .ShouldBeProblem(HttpStatusCode.Forbidden, Messages.ForbiddenTitle);
        (await Api.GetAsync($"/users/{other.UserId}", caregiver.Token))
            .ShouldBeProblem(HttpStatusCode.Forbidden, Messages.UserAccessForbidden);
        (await Api.DeleteAsync($"/users/{other.UserId}", caregiver.Token))
            .ShouldBeProblem(HttpStatusCode.Forbidden, Messages.ForbiddenTitle);
        (await Api.GetAsync("/users")).ShouldBeProblem(HttpStatusCode.Unauthorized, Messages.Unauthorized);

        // Anonymous compatibility lookup used by the frontend before sign-up: only id and email.
        var lookup = (await Api.GetAsync($"/users?email={Uri.EscapeDataString(other.Email)}"))
            .ShouldBe(HttpStatusCode.OK);
        var match = Assert.Single(lookup.Items);
        Assert.Equal(["id", "email"], match.EnumerateObject().Select(property => property.Name));
        Assert.Equal(other.UserId, match.GetProperty("id").GetInt32());
    }

    [Fact]
    public async Task Users_CannotChangeTheirOwnRole_AndAdminCannotDeactivateThemselves()
    {
        var caregiver = await Given.SignUpAsync("Caregiver", "own-role");
        (await Api.PutAsync($"/users/{caregiver.UserId}",
                new { email = caregiver.Email, role = "FamilyMember", status = "Active" }, caregiver.Token))
            .ShouldBeProblem(HttpStatusCode.Forbidden, Messages.RoleChangeNotAllowed);

        var adminToken = await Given.AdminTokenAsync();
        var adminId = (await Api.GetAsync("/authentication/me", adminToken)).ShouldBe(HttpStatusCode.OK).Id;
        (await Api.DeleteAsync($"/users/{adminId}", adminToken))
            .ShouldBeProblem(HttpStatusCode.Conflict, Messages.CannotDeactivateSelf);
    }

    [Fact]
    public async Task DeactivatedUser_CannotSignInRefreshOrReadMe()
    {
        var account = await Given.SignUpAsync("Caregiver", "deactivated");
        var adminToken = await Given.AdminTokenAsync();

        (await Api.DeleteAsync($"/users/{account.UserId}", adminToken)).ShouldBe(HttpStatusCode.NoContent);

        (await Api.PostAsync("/authentication/sign-in", new { email = account.Email, password = Scenario.Password }))
            .ShouldBeProblem(HttpStatusCode.Unauthorized, Messages.AccountInactive);
        (await Api.PostAsync("/authentication/refresh", null, account.Token))
            .ShouldBeProblem(HttpStatusCode.Unauthorized, Messages.AccountInactive);
        (await Api.GetAsync("/authentication/me", account.Token))
            .ShouldBeProblem(HttpStatusCode.Unauthorized, Messages.AccountInactive);
    }
}
