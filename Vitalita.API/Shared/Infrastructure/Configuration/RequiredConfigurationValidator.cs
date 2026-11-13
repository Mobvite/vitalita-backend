namespace Vitalita.API.Shared.Infrastructure.Configuration;

/// <summary>
///     Checks on startup that the settings that have no default are present, and stops the application with one
///     message listing every missing setting by its environment variable name.
/// </summary>
/// <remarks>
///     Required in every environment: <c>ConnectionStrings__DefaultConnection</c> and
///     <c>Authentication__Jwt__Secret</c> (its 32-byte minimum is validated by <c>JwtSettings</c>).
///     Required in Production: <c>Cors__AllowedOrigins__0</c> (without it the frontend is blocked by CORS).
///     <c>Authentication__Admin__Email</c> and <c>Authentication__Admin__Password</c> are optional, but must be set
///     together. Values are never included in the message.
/// </remarks>
public static class RequiredConfigurationValidator
{
    public const string ConnectionStringKey = "ConnectionStrings:DefaultConnection";
    public const string JwtSecretKey = "Authentication:Jwt:Secret";
    public const string CorsOriginsKey = "Cors:AllowedOrigins";
    public const string AdminEmailKey = "Authentication:Admin:Email";
    public const string AdminPasswordKey = "Authentication:Admin:Password";

    /// <summary>
    ///     Returns the problems found (empty when the configuration is complete).
    /// </summary>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="isProduction">Whether the application runs in the Production environment.</param>
    public static IReadOnlyList<string> FindProblems(IConfiguration configuration, bool isProduction)
    {
        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(configuration[ConnectionStringKey]))
            problems.Add($"{ToEnvironmentVariable(ConnectionStringKey)} is not set (MySQL connection string).");

        if (string.IsNullOrWhiteSpace(configuration[JwtSecretKey]))
            problems.Add($"{ToEnvironmentVariable(JwtSecretKey)} is not set (JWT signing secret, at least 32 bytes).");

        var origins = configuration.GetSection(CorsOriginsKey).Get<string[]>() ?? [];
        if (isProduction && !origins.Any(origin => !string.IsNullOrWhiteSpace(origin)))
            problems.Add($"{ToEnvironmentVariable(CorsOriginsKey)}__0 is not set (frontend origin allowed by CORS, " +
                         "e.g. the deployed frontend URL).");

        var hasAdminEmail = !string.IsNullOrWhiteSpace(configuration[AdminEmailKey]);
        var hasAdminPassword = !string.IsNullOrEmpty(configuration[AdminPasswordKey]);
        if (hasAdminEmail != hasAdminPassword)
            problems.Add($"{ToEnvironmentVariable(AdminEmailKey)} and {ToEnvironmentVariable(AdminPasswordKey)} " +
                         "must be set together (or both left unset to skip the first administrator).");

        return problems;
    }

    /// <summary>
    ///     Throws an <see cref="InvalidOperationException" /> listing every problem, if there is any.
    /// </summary>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="environment">The hosting environment.</param>
    public static void Validate(IConfiguration configuration, IHostEnvironment environment)
    {
        var problems = FindProblems(configuration, environment.IsProduction());
        if (problems.Count == 0) return;

        throw new InvalidOperationException(
            "The API configuration is incomplete:" + Environment.NewLine +
            string.Join(Environment.NewLine, problems.Select(problem => "  - " + problem)) + Environment.NewLine +
            "Set them as environment variables (production, Docker) or with 'dotnet user-secrets set' using ':' " +
            "instead of '__' (development). See README > Deployment.");
    }

    private static string ToEnvironmentVariable(string key) => key.Replace(":", "__");
}
