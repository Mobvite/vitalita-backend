using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Localization;
using Vitalita.API.Resources;

namespace Vitalita.API.Shared.Infrastructure.Interfaces.ASP.Configuration;

/// <summary>
///     Customizes every RFC 7807 <c>ProblemDetails</c> response produced by the API.
/// </summary>
/// <remarks>
///     <list type="bullet">
///         <item>
///             <description>
///                 Replaces the framework default titles ("Not Found", "Bad Request", …, and the
///                 "One or more validation errors occurred." of <c>ValidationProblem(ModelState)</c>) with localized
///                 ones (<c>Accept-Language</c>). Titles set explicitly by a controller are kept.
///             </description>
///         </item>
///         <item><description>Adds a localized <c>detail</c> to 5xx responses that do not have one.</description></item>
///         <item>
///             <description>
///                 Adds the <c>message</c> extension (<c>detail ?? title</c>), which is the field the frontend
///                 error interceptor reads.
///             </description>
///         </item>
///     </list>
///     It runs for controller results (<c>Problem()</c>, <c>NotFound()</c>, validation errors), status code pages and
///     the global exception handler.
/// </remarks>
public static class LocalizedProblemDetailsCustomizer
{
    /// <summary>
    ///     Name of the extension member read by the frontend.
    /// </summary>
    public const string MessageExtension = "message";

    // Default title used by ASP.NET Core for 500 responses.
    private const string FrameworkServerErrorTitle = "An error occurred while processing your request.";

    // Default title of ValidationProblemDetails, used when a controller calls ValidationProblem(ModelState).
    private const string FrameworkValidationTitle = "One or more validation errors occurred.";

    private static readonly IReadOnlyDictionary<int, string> TitleResourceKeys = new Dictionary<int, string>
    {
        [StatusCodes.Status400BadRequest] = "BadRequestTitle",
        [StatusCodes.Status401Unauthorized] = "UnauthorizedTitle",
        [StatusCodes.Status403Forbidden] = "ForbiddenTitle",
        [StatusCodes.Status404NotFound] = "ResourceNotFound",
        [StatusCodes.Status405MethodNotAllowed] = "MethodNotAllowedTitle",
        [StatusCodes.Status409Conflict] = "ConflictTitle",
        [StatusCodes.Status415UnsupportedMediaType] = "UnsupportedMediaTypeTitle"
    };

    /// <summary>
    ///     Applies the localization and the <c>message</c> extension to a problem details context.
    /// </summary>
    /// <param name="context">The problem details context.</param>
    public static void Customize(ProblemDetailsContext context)
    {
        var problemDetails = context.ProblemDetails;
        var status = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
        var localizer = context.HttpContext.RequestServices.GetRequiredService<IStringLocalizer<SharedResource>>();

        if (status >= StatusCodes.Status500InternalServerError)
        {
            if (IsFrameworkDefaultTitle(problemDetails.Title, status))
                problemDetails.Title = localizer["UnexpectedServerError"].Value;
            problemDetails.Detail ??= localizer["UnexpectedErrorProcessingRequest"].Value;
        }
        else if (problemDetails.Title == FrameworkValidationTitle)
        {
            problemDetails.Title = localizer["InvalidModelStateTitle"].Value;
        }
        else if (IsFrameworkDefaultTitle(problemDetails.Title, status) &&
                 TitleResourceKeys.TryGetValue(status, out var titleKey))
        {
            problemDetails.Title = localizer[titleKey].Value;
        }

        problemDetails.Extensions[MessageExtension] = problemDetails.Detail ?? problemDetails.Title;
    }

    private static bool IsFrameworkDefaultTitle(string? title, int status) =>
        string.IsNullOrEmpty(title) ||
        title == ReasonPhrases.GetReasonPhrase(status) ||
        title == FrameworkServerErrorTitle;
}
