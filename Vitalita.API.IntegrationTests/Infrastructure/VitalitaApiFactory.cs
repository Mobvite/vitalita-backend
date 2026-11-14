using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Vitalita.API.IntegrationTests.Infrastructure;

/// <summary>
///     Hosts the real API in memory against the <c>vitalita_test</c> MySQL database.
/// </summary>
/// <remarks>
///     The environment is <c>Testing</c>, so the developer's user-secrets are not loaded. The connection string comes
///     from <see cref="TestConnectionString" />; the JWT secret and the seeded administrator are test-only values
///     injected as in-memory configuration. Migrations and the administrator seed run in the API startup.
/// </remarks>
public sealed class VitalitaApiFactory : WebApplicationFactory<Program>
{
    public const string AdminEmail = "admin@vitalita.test";
    public const string AdminPassword = "Admin-Integration-Tests-Only-1";

    // Test-only signing key (at least 32 bytes). It is not used anywhere else.
    private const string JwtSecret = "vitalita-integration-tests-only-signing-key-0123456789";

    private readonly string _connectionString = TestConnectionString.Resolve();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // UseSetting (in-memory host settings) is visible from WebApplication.CreateBuilder on, so the startup
        // configuration check in Program.cs sees these values. ConfigureAppConfiguration would only apply at Build().
        builder.UseSetting("ConnectionStrings:DefaultConnection", _connectionString);
        builder.UseSetting("Authentication:Jwt:Secret", JwtSecret);
        builder.UseSetting("Authentication:Admin:Email", AdminEmail);
        builder.UseSetting("Authentication:Admin:Password", AdminPassword);
    }
}

/// <summary>One API host shared by every integration test class; the classes of the collection run one at a time.</summary>
[CollectionDefinition(Name)]
public sealed class ApiCollection : ICollectionFixture<VitalitaApiFactory>
{
    public const string Name = "Api";
}
