using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Vitalita.API.Resources;
using Vitalita.API.Shared.Domain.Repositories;
using Vitalita.API.Shared.Domain.Services;
using Vitalita.API.Shared.Infrastructure.Interfaces.ASP.Configuration;
using Vitalita.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using Vitalita.API.Shared.Infrastructure.Persistence.EFC.Interceptors;
using Vitalita.API.Shared.Infrastructure.Persistence.EFC.Repositories;
using Vitalita.API.Shared.Infrastructure.Time;

const string frontendCorsPolicy = "AllowFrontend";

var builder = WebApplication.CreateBuilder(args);

// Lowercase URLs; the [controller] token is converted to kebab-case by the convention below.
builder.Services.AddRouting(options => options.LowercaseUrls = true);

// Localization (Resources/SharedResource*.resx).
builder.Services.AddLocalization();

// Controllers: kebab-case routes, camelCase JSON, enums as strings, localized validation errors.
builder.Services.AddControllers(options => options.Conventions.Add(new KebabCaseRouteNamingConvention()))
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    })
    .AddDataAnnotationsLocalization(options =>
        options.DataAnnotationLocalizerProvider = (_, factory) => factory.Create(typeof(SharedResource)))
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var localizer = context.HttpContext.RequestServices
                .GetRequiredService<IStringLocalizer<SharedResource>>();
            var problemDetailsFactory = context.HttpContext.RequestServices
                .GetRequiredService<ProblemDetailsFactory>();
            var problemDetails = problemDetailsFactory.CreateValidationProblemDetails(
                context.HttpContext,
                context.ModelState,
                statusCode: StatusCodes.Status400BadRequest,
                title: localizer["InvalidModelStateTitle"].Value);

            var result = new BadRequestObjectResult(problemDetails);
            result.ContentTypes.Add("application/problem+json");
            return result;
        };
    });

// Same JSON settings for responses written outside MVC (exception handler, status code pages, OpenAPI schemas).
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// RFC 7807 ProblemDetails for every error, localized and with the "message" extension read by the frontend.
builder.Services.AddProblemDetails(options =>
    options.CustomizeProblemDetails = LocalizedProblemDetailsCustomizer.Customize);

// Built-in OpenAPI document generation (Microsoft.AspNetCore.OpenApi), rendered by Swagger UI (/swagger).
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer(LocalizedOpenApiDocumentTransformer.TransformAsync);
    options.AddSchemaTransformer(StringEnumSchemaTransformer.TransformAsync);
});

// CORS: allowed origins come from configuration (Cors:AllowedOrigins, or Cors__AllowedOrigins__0 in production).
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options => options.AddPolicy(frontendCorsPolicy, policy => policy
    .WithOrigins(allowedOrigins)
    .AllowAnyHeader()
    .AllowAnyMethod()
    .WithExposedHeaders("Location")));

// Shared kernel services.
builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddSingleton<AuditableEntityInterceptor>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Database: the connection string comes from user-secrets (development) or the
// ConnectionStrings__DefaultConnection environment variable (production). It is never stored in appsettings.
builder.Services.AddDbContext<AppDbContext>((serviceProvider, options) =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrWhiteSpace(connectionString))
        throw new InvalidOperationException(
            "Connection string 'DefaultConnection' is not set. Use 'dotnet user-secrets set " +
            "\"ConnectionStrings:DefaultConnection\" \"...\"' in development or the " +
            "'ConnectionStrings__DefaultConnection' environment variable in production.");

    options.UseMySQL(connectionString)
        .AddInterceptors(serviceProvider.GetRequiredService<AuditableEntityInterceptor>())
        .EnableDetailedErrors();

    if (builder.Environment.IsDevelopment())
        options.EnableSensitiveDataLogging();
});

// Bounded context registrations go here.

var app = builder.Build();

// Apply pending migrations on startup (no-op when the schema is up to date).
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.Migrate();
}

// CORS first, so error responses (404, 500) also carry the CORS headers.
app.UseCors(frontendCorsPolicy);

// Localization before the exception handler, so error messages follow Accept-Language.
string[] supportedCultures = ["en", "es"];
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture(supportedCultures[0])
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);
localizationOptions.ApplyCurrentCultureToResponseHeaders = true;
app.UseRequestLocalization(localizationOptions);

// Unhandled exceptions and empty error responses (404, 405, ...) become ProblemDetails.
app.UseExceptionHandler();
app.UseStatusCodePages();

app.UseHttpsRedirection();

app.UseAuthorization();

// OpenAPI document and Swagger UI are available in all environments (same as the reference project).
app.MapOpenApi("/swagger/{documentName}/swagger.json");
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Vitalita API v1");
    options.RoutePrefix = "swagger";
});
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
app.MapControllers();

app.Run();
