using Microsoft.Extensions.Configuration;
using Vitalita.API.Shared.Infrastructure.Configuration;

namespace Vitalita.API.Tests.Shared.Infrastructure.Configuration;

public class RequiredConfigurationValidatorTests
{
    private static IConfiguration Configuration(params (string Key, string? Value)[] values) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(values.Select(value => new KeyValuePair<string, string?>(value.Key, value.Value)))
            .Build();

    private static readonly (string, string?)[] Complete =
    [
        (RequiredConfigurationValidator.ConnectionStringKey, "server=db;database=vitalita"),
        (RequiredConfigurationValidator.JwtSecretKey, "a-secret-of-at-least-thirty-two-bytes!!"),
        ("Cors:AllowedOrigins:0", "https://vitalita.example")
    ];

    [Fact]
    public void FindProblems_CompleteProductionConfiguration_ReturnsNone()
    {
        Assert.Empty(RequiredConfigurationValidator.FindProblems(Configuration(Complete), isProduction: true));
    }

    [Fact]
    public void FindProblems_EmptyProductionConfiguration_ListsEveryMissingVariableByName()
    {
        var problems = RequiredConfigurationValidator.FindProblems(Configuration(), isProduction: true);

        Assert.Equal(3, problems.Count);
        Assert.Contains(problems, problem => problem.StartsWith("ConnectionStrings__DefaultConnection"));
        Assert.Contains(problems, problem => problem.StartsWith("Authentication__Jwt__Secret"));
        Assert.Contains(problems, problem => problem.StartsWith("Cors__AllowedOrigins__0"));
    }

    [Fact]
    public void FindProblems_OutsideProduction_DoesNotRequireCorsOrigins()
    {
        var withoutCors = Complete.Where(value => !value.Item1.StartsWith("Cors")).ToArray();

        Assert.Empty(RequiredConfigurationValidator.FindProblems(Configuration(withoutCors), isProduction: false));
    }

    [Fact]
    public void FindProblems_AdminEmailWithoutPassword_IsAProblem_AndBothUnsetIsNot()
    {
        var onlyEmail = Complete.Append((RequiredConfigurationValidator.AdminEmailKey, "admin@vitalita.pe")).ToArray();

        var problem = Assert.Single(RequiredConfigurationValidator.FindProblems(Configuration(onlyEmail), true));
        Assert.Contains("Authentication__Admin__Password", problem);
    }

    [Fact]
    public void FindProblems_NeverIncludesConfiguredValues()
    {
        const string secretPassword = "Sup3r-Secret-Password";
        var configuration = Configuration((RequiredConfigurationValidator.AdminPasswordKey, secretPassword));

        var problems = RequiredConfigurationValidator.FindProblems(configuration, isProduction: true);

        Assert.DoesNotContain(problems, problem => problem.Contains(secretPassword));
    }
}
