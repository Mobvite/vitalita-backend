using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Vitalita.API.IntegrationTests.Infrastructure;

/// <summary>Thin HTTP client for the tests: JSON bodies, optional Bearer token and Accept-Language.</summary>
public sealed class Api(HttpClient client)
{
    private const string Prefix = "/api/v1";

    public Task<ApiResponse> GetAsync(string path, string? token = null, string? language = null) =>
        SendAsync(HttpMethod.Get, path, null, token, language);

    public Task<ApiResponse> PostAsync(string path, object? body, string? token = null, string? language = null) =>
        SendAsync(HttpMethod.Post, path, body, token, language);

    public Task<ApiResponse> PutAsync(string path, object? body, string? token = null, string? language = null) =>
        SendAsync(HttpMethod.Put, path, body, token, language);

    public Task<ApiResponse> DeleteAsync(string path, string? token = null, string? language = null) =>
        SendAsync(HttpMethod.Delete, path, null, token, language);

    private async Task<ApiResponse> SendAsync(HttpMethod method, string path, object? body, string? token,
        string? language)
    {
        using var request = new HttpRequestMessage(method, Prefix + path);
        if (body is not null) request.Content = JsonContent.Create(body);
        if (token is not null) request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (language is not null) request.Headers.AcceptLanguage.Add(new StringWithQualityHeaderValue(language));

        using var response = await client.SendAsync(request);
        var text = await response.Content.ReadAsStringAsync();
        var json = text.Length == 0 ? default : JsonDocument.Parse(text).RootElement.Clone();
        return new ApiResponse(method, path, response.StatusCode, response.Headers.Location,
            response.Content.Headers.ContentType?.MediaType, response.Content.Headers.ContentLanguage.ToArray(),
            text, json);
    }
}

/// <summary>A response already read, with assertion helpers that print the body when they fail.</summary>
public sealed record ApiResponse(
    HttpMethod Method,
    string Path,
    HttpStatusCode Status,
    Uri? Location,
    string? ContentType,
    string[] ContentLanguage,
    string Body,
    JsonElement Json)
{
    public const string ProblemJson = "application/problem+json";

    public int Id => Json.GetProperty("id").GetInt32();

    public string? Message => Json.TryGetProperty("message", out var message) ? message.GetString() : null;

    public string Str(string property) => Json.GetProperty(property).GetString()!;

    public JsonElement[] Items => Json.EnumerateArray().ToArray();

    public ApiResponse ShouldBe(HttpStatusCode expected)
    {
        Assert.True(Status == expected,
            $"{Method} {Path}: expected {(int)expected} {expected}, got {(int)Status} {Status}. Body: {Body}");
        return this;
    }

    /// <summary>RFC 7807 error with the "message" extension read by the frontend.</summary>
    public ApiResponse ShouldBeProblem(HttpStatusCode expected, string message)
    {
        ShouldBe(expected);
        Assert.Equal(ProblemJson, ContentType);
        Assert.Equal((int)expected, Json.GetProperty("status").GetInt32());
        Assert.Equal(message, Message);
        return this;
    }

    /// <summary>400 validation problem: generic localized title plus the error of one field.</summary>
    public ApiResponse ShouldBeValidationProblem(string field, string fieldError, string title = Messages.InvalidModelState)
    {
        ShouldBeProblem(HttpStatusCode.BadRequest, title);
        var errors = Json.GetProperty("errors").EnumerateObject()
            .Where(entry => entry.Name.Equals(field, StringComparison.OrdinalIgnoreCase))
            .SelectMany(entry => entry.Value.EnumerateArray().Select(error => error.GetString()))
            .ToArray();
        Assert.True(errors.Contains(fieldError),
            $"{Method} {Path}: expected error '{fieldError}' for field '{field}'. Body: {Body}");
        return this;
    }
}
