using System.Data.Common;

namespace Vitalita.API.IntegrationTests.Infrastructure;

/// <summary>
///     Resolves the MySQL connection string of the integration tests without storing it in the repository.
/// </summary>
/// <remarks>
///     Sources, in order: the <c>VITALITA_TEST_CONNECTION</c> environment variable, then the git-ignored file
///     <c>Vitalita.API.IntegrationTests/.test-connection</c> (one line). The database name must end with
///     <c>_test</c>, so the tests never run migrations or create users in the development database.
///     Error messages never include the connection string or its password.
/// </remarks>
public static class TestConnectionString
{
    public const string EnvironmentVariable = "VITALITA_TEST_CONNECTION";
    public const string FileName = ".test-connection";
    private const string ProjectFileName = "Vitalita.API.IntegrationTests.csproj";
    private const string RequiredDatabaseSuffix = "_test";

    private static readonly string HowToConfigure =
        $"""
         Set the MySQL connection string of the integration tests in one of these ways:
           1. Environment variable {EnvironmentVariable}:
                export {EnvironmentVariable}="server=127.0.0.1;port=3307;database=vitalita_test;user=vitalita;password=<your password>"
           2. File Vitalita.API.IntegrationTests/{FileName} (git-ignored), with that same string on a single line.
         The database must already exist and its name must end with '{RequiredDatabaseSuffix}'. See README > Integration tests.
         """;

    /// <summary>Returns the connection string, or throws with instructions when it is missing or unsafe.</summary>
    public static string Resolve()
    {
        var (value, source) = ReadFromEnvironment() ?? ReadFromFile() ??
            throw new InvalidOperationException($"The integration test database is not configured.{Environment.NewLine}{HowToConfigure}");

        var database = ReadDatabaseName(value, source);
        if (string.IsNullOrWhiteSpace(database))
            throw new InvalidOperationException(
                $"The connection string from {source} does not name a database.{Environment.NewLine}{HowToConfigure}");

        if (!database.EndsWith(RequiredDatabaseSuffix, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(
                $"The connection string from {source} points to database '{database}'. The integration tests only " +
                $"run against a database whose name ends with '{RequiredDatabaseSuffix}' (for example vitalita_test), " +
                $"so the development data is never touched.{Environment.NewLine}{HowToConfigure}");

        return value;
    }

    private static (string Value, string Source)? ReadFromEnvironment()
    {
        var value = Environment.GetEnvironmentVariable(EnvironmentVariable)?.Trim();
        return string.IsNullOrEmpty(value) ? null : (value, $"the {EnvironmentVariable} environment variable");
    }

    private static (string Value, string Source)? ReadFromFile()
    {
        var projectDirectory = FindProjectDirectory();
        if (projectDirectory is null) return null;

        var path = Path.Combine(projectDirectory, FileName);
        if (!File.Exists(path)) return null;

        var value = File.ReadLines(path).Select(line => line.Trim()).FirstOrDefault(line => line.Length > 0);
        return value is null ? null : (value, $"the file {FileName}");
    }

    // The tests run from bin/<configuration>/<framework>; walk up to the folder that holds the test project.
    private static string? FindProjectDirectory()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null;
             directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, ProjectFileName))) return directory.FullName;
        }

        return null;
    }

    private static string? ReadDatabaseName(string connectionString, string source)
    {
        var builder = new DbConnectionStringBuilder();
        try
        {
            builder.ConnectionString = connectionString;
        }
        catch (ArgumentException)
        {
            // The parser message may quote part of the string, so it is not included.
            throw new InvalidOperationException(
                $"The connection string from {source} is not valid.{Environment.NewLine}{HowToConfigure}");
        }

        foreach (var key in new[] { "database", "initial catalog" })
        {
            if (builder.TryGetValue(key, out var database)) return database?.ToString();
        }

        return null;
    }
}
