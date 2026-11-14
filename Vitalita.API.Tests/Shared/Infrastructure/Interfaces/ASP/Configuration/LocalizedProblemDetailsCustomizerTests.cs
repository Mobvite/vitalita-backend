using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Vitalita.API.Shared.Infrastructure.Interfaces.ASP.Configuration;

namespace Vitalita.API.Tests.Shared.Infrastructure.Interfaces.ASP.Configuration;

public class LocalizedProblemDetailsCustomizerTests
{
    private static ProblemDetailsContext Context(ProblemDetails problemDetails)
    {
        var services = new ServiceCollection().AddLogging().AddLocalization().BuildServiceProvider();
        return new ProblemDetailsContext
        {
            HttpContext = new DefaultHttpContext { RequestServices = services },
            ProblemDetails = problemDetails
        };
    }

    private static T WithCulture<T>(string culture, Func<T> action)
    {
        var previous = CultureInfo.CurrentUICulture;
        CultureInfo.CurrentUICulture = new CultureInfo(culture);
        try
        {
            return action();
        }
        finally
        {
            CultureInfo.CurrentUICulture = previous;
        }
    }

    [Theory]
    [InlineData("es", "La solicitud contiene valores no válidos.")]
    [InlineData("en", "The request contains invalid values.")]
    public void DefaultValidationTitle_IsLocalized(string culture, string expected)
    {
        var problem = new ValidationProblemDetails { Status = StatusCodes.Status400BadRequest };
        Assert.Equal("One or more validation errors occurred.", problem.Title);

        var title = WithCulture(culture, () =>
        {
            LocalizedProblemDetailsCustomizer.Customize(Context(problem));
            return problem.Title;
        });

        Assert.Equal(expected, title);
        Assert.Equal(expected, problem.Extensions[LocalizedProblemDetailsCustomizer.MessageExtension]);
    }

    [Fact]
    public void ExplicitTitle_IsKept()
    {
        var problem = new ProblemDetails { Status = StatusCodes.Status409Conflict, Title = "Custom title" };

        WithCulture("es", () =>
        {
            LocalizedProblemDetailsCustomizer.Customize(Context(problem));
            return 0;
        });

        Assert.Equal("Custom title", problem.Title);
        Assert.Equal("Custom title", problem.Extensions[LocalizedProblemDetailsCustomizer.MessageExtension]);
    }
}
