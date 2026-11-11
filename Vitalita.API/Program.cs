using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Vitalita.API.IAM.Application.Internal.CommandServices;
using Vitalita.API.IAM.Application.Internal.OutboundServices;
using Vitalita.API.IAM.Application.Internal.QueryServices;
using Vitalita.API.IAM.Application.Services;
using Vitalita.API.IAM.Domain.Repositories;
using Vitalita.API.IAM.Infrastructure.Hashing.BCrypt.Services;
using Vitalita.API.IAM.Infrastructure.Persistence.EFC.Repositories;
using Vitalita.API.IAM.Infrastructure.Seeding;
using Vitalita.API.IAM.Infrastructure.Tokens.JWT.Configuration;
using Vitalita.API.IAM.Infrastructure.Tokens.JWT.Services;
using Vitalita.API.IAM.Interfaces.REST.Transform;
using Vitalita.API.Resources;
using Vitalita.API.Shared.Domain.Repositories;
using Vitalita.API.Shared.Domain.Services;
using Vitalita.API.Shared.Infrastructure.Interfaces.ASP.Configuration;
using Vitalita.API.Shared.Infrastructure.Interfaces.ASP.Security;
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
    options.AddDocumentTransformer(BearerSecuritySchemeTransformer.TransformDocumentAsync);
    options.AddOperationTransformer(BearerSecuritySchemeTransformer.TransformOperationAsync);
    options.AddOperationTransformer(LocalizedOpenApiAssembler.TransformOperationAsync);
    options.AddSchemaTransformer(LocalizedOpenApiAssembler.TransformSchemaAsync);
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

// IAM bounded context.
// JWT settings: Issuer, Audience and ExpirationMinutes (1-15) live in appsettings.json. The secret comes from
// user-secrets (development) or Authentication__Jwt__Secret (production) and is validated on startup.
builder.Services.AddOptions<JwtSettings>()
    .Bind(builder.Configuration.GetSection(JwtSettings.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<JwtSettings>>((options, jwtSettings) =>
    {
        var settings = jwtSettings.Value;
        options.MapInboundClaims = false; // keep the original claim names (sub, email, role)
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = settings.Issuer,
            ValidateAudience = true,
            ValidAudience = settings.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = TokenService.CreateSigningKey(settings.Secret),
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero, // the token lasts exactly ExpirationMinutes
            NameClaimType = TokenService.UserIdClaimType,
            RoleClaimType = TokenService.RoleClaimType
        };
    });
builder.Services.AddAuthorizationBuilder()
    .AddPolicy(AuthorizationPolicies.AdminOnly, policy => policy.RequireRole(AuthorizationPolicies.AdminRole));

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserCommandService, UserCommandService>();
builder.Services.AddScoped<IUserQueryService, UserQueryService>();
builder.Services.AddSingleton<IHashingService, HashingService>();
builder.Services.AddSingleton<ITokenService, TokenService>();

var app = builder.Build();

// Apply pending migrations on startup (no-op when the schema is up to date), then create the first
// administrator when Authentication:Admin:Email and Authentication:Admin:Password are configured.
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.Migrate();
    await AdminSeeder.SeedAsync(scope.ServiceProvider, app.Configuration);
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

app.UseAuthentication();
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
