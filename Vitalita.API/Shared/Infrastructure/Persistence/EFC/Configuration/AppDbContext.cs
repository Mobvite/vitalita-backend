using Microsoft.EntityFrameworkCore;
using Vitalita.API.IAM.Infrastructure.Persistence.EFC.Configuration.Extensions;
using Vitalita.API.Monitoring.Infrastructure.Persistence.EFC.Configuration.Extensions;
using Vitalita.API.Profiles.Infrastructure.Persistence.EFC.Configuration.Extensions;
using Vitalita.API.Shared.Infrastructure.Persistence.EFC.Configuration.Converters;
using Vitalita.API.Shared.Infrastructure.Persistence.EFC.Configuration.Extensions;

namespace Vitalita.API.Shared.Infrastructure.Persistence.EFC.Configuration;

/// <summary>
///     Application database context shared by all bounded contexts.
/// </summary>
/// <remarks>
///     Each bounded context adds its Fluent API configuration in <see cref="OnModelCreating" />.
///     The audit interceptor is added when the context is registered in <c>Program.cs</c>.
/// </remarks>
/// <param name="options">The context options.</param>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    /// <inheritdoc />
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        // Every DateTime is a UTC instant (see UtcDateTimeConverter).
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
        configurationBuilder.Properties<DateTime?>().HaveConversion<UtcDateTimeConverter>();

        // Every DateOnly is a MySQL date column, read by the provider as DateTime (see DateOnlyConverter).
        configurationBuilder.Properties<DateOnly>().HaveConversion<DateOnlyConverter>().HaveColumnType("date");
        configurationBuilder.Properties<DateOnly?>().HaveConversion<DateOnlyConverter>().HaveColumnType("date");
    }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Bounded context configurations go here.
        builder.ApplyIamConfiguration();
        builder.ApplyProfilesConfiguration();
        builder.ApplyMonitoringConfiguration();

        builder.UseSnakeCaseNamingConvention();
    }
}
