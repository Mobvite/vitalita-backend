namespace Vitalita.API.IntegrationTests.Infrastructure;

/// <summary>Base class of the integration tests: a fresh client per test over the shared API host.</summary>
[Collection(ApiCollection.Name)]
public abstract class ApiTestBase(VitalitaApiFactory factory)
{
    protected Api Api { get; } = new(factory.CreateClient());

    protected Scenario Given => new(Api);

    /// <summary>An id that no test creates (records are auto-increment and the test database starts empty).</summary>
    protected const int UnknownId = int.MaxValue;
}
