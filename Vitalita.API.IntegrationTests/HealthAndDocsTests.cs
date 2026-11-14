using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Vitalita.API.IntegrationTests.Infrastructure;

namespace Vitalita.API.IntegrationTests;

/// <summary>Platform endpoints: GET /health (Render health check) and the public API documentation.</summary>
[Collection(ApiCollection.Name)]
public sealed class HealthAndDocsTests(VitalitaApiFactory factory)
{
    private readonly HttpClient _client =
        factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

    [Fact]
    public async Task Health_IsAnonymous_AndReturnsHealthy()
    {
        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("Healthy", json.RootElement.GetProperty("status").GetString());
    }

    [Fact]
    public async Task OpenApiDocument_DocumentsHealth_AndRootRedirectsToSwaggerUi()
    {
        var document = await _client.GetAsync("/swagger/v1/swagger.json");
        Assert.Equal(HttpStatusCode.OK, document.StatusCode);
        using var json = JsonDocument.Parse(await document.Content.ReadAsStringAsync());
        var health = json.RootElement.GetProperty("paths").GetProperty("/health").GetProperty("get");
        Assert.Equal("Health check", health.GetProperty("summary").GetString());
        Assert.False(health.TryGetProperty("security", out _));

        var root = await _client.GetAsync("/");
        Assert.Equal(HttpStatusCode.Redirect, root.StatusCode);
        Assert.Equal("/swagger", root.Headers.Location?.ToString());

        Assert.Equal(HttpStatusCode.OK, (await _client.GetAsync("/swagger/index.html")).StatusCode);
    }
}
