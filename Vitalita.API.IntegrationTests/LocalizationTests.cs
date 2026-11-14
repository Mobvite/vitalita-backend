using System.Net;
using Vitalita.API.IntegrationTests.Infrastructure;

namespace Vitalita.API.IntegrationTests;

/// <summary>Accept-Language (en default, es): same request, message in each language.</summary>
public sealed class LocalizationTests(VitalitaApiFactory factory) : ApiTestBase(factory)
{
    [Theory]
    [InlineData("en", "The request contains invalid values.", "Enter a valid email address.")]
    [InlineData("es", "La solicitud contiene valores no válidos.", "Ingresa un correo electrónico válido.")]
    public async Task InvalidSignUp_ReturnsTheValidationMessagesInTheRequestedLanguage(string language, string title,
        string emailError)
    {
        var response = await Api.PostAsync("/authentication/sign-up",
            new { email = "not-an-email", password = Scenario.Password }, language: language);

        response.ShouldBeValidationProblem("email", emailError, title);
        Assert.Contains(language, response.ContentLanguage);
    }

    [Theory]
    [InlineData("en", "You do not have access to the records of this older adult.")]
    [InlineData("es", "No tienes acceso a los registros de este adulto mayor.")]
    public async Task BusinessError_ReturnsTheMessageInTheRequestedLanguage(string language, string message)
    {
        var caregiver = await Given.CaregiverAsync("i18n");

        var response = await Api.GetAsync($"/daily-reports?olderAdultId={UnknownId}", caregiver.Token, language);

        response.ShouldBeProblem(HttpStatusCode.Forbidden, message);
        Assert.Equal(message, response.Str("title"));
    }

    [Theory]
    [InlineData("en", "Authentication is required to access this resource.")]
    [InlineData("es", "Se requiere autenticación para acceder a este recurso.")]
    public async Task FrameworkUnauthorized_ReturnsTheMessageInTheRequestedLanguage(string language, string message)
    {
        (await Api.GetAsync("/authentication/me", language: language))
            .ShouldBeProblem(HttpStatusCode.Unauthorized, message);
    }
}
