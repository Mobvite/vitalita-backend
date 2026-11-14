using Vitalita.API.IAM.Application.Errors;
using Vitalita.API.IAM.Application.Services;
using Vitalita.API.IAM.Domain.Model.Aggregates;
using Vitalita.API.IAM.Domain.Model.Commands;
using Vitalita.API.IAM.Domain.Model.ValueObjects;
using Vitalita.API.Shared.Application.Patterns;

namespace Vitalita.API.IAM.Infrastructure.Seeding;

/// <summary>
///     Creates the first administrator on startup from configuration.
/// </summary>
/// <remarks>
///     <para>
///         It only runs when both <c>Authentication:Admin:Email</c> and <c>Authentication:Admin:Password</c> are set
///         (user-secrets in development, <c>Authentication__Admin__Email</c> and
///         <c>Authentication__Admin__Password</c> in production). The repository never contains default values.
///     </para>
///     <para>
///         When the email is already registered, the existing user is left unchanged (its password and role are
///         not overwritten). The password is never logged.
///     </para>
/// </remarks>
public static class AdminSeeder
{
    /// <summary>
    ///     Configuration section name.
    /// </summary>
    public const string SectionName = "Authentication:Admin";

    /// <summary>
    ///     Creates the administrator if it is configured and does not exist yet.
    /// </summary>
    /// <param name="services">A scoped service provider.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(AdminSeeder));
        var section = configuration.GetSection(SectionName);
        var email = section["Email"];
        var password = section["Password"];

        if (string.IsNullOrWhiteSpace(email) && string.IsNullOrEmpty(password))
        {
            logger.LogInformation("First administrator not configured ({Section}); skipping", SectionName);
            return;
        }

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrEmpty(password))
        {
            logger.LogWarning("Both {Section}:Email and {Section}:Password are required; administrator not created",
                SectionName, SectionName);
            return;
        }

        if (!EmailAddress.IsValid(email) || password.Length is < 8 or > 72)
        {
            logger.LogWarning("{Section} has an invalid email or a password outside 8-72 characters; " +
                              "administrator not created", SectionName);
            return;
        }

        var commandService = services.GetRequiredService<IUserCommandService>();
        var result = await commandService.Handle(new SeedAdminCommand(new EmailAddress(email), password),
            cancellationToken);

        if (result is Result<User, SeedAdminError>.Failure)
            logger.LogInformation("The configured administrator email is already registered; left unchanged");
    }
}
