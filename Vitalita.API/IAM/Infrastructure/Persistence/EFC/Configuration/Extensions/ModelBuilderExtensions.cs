using Microsoft.EntityFrameworkCore;
using Vitalita.API.IAM.Domain.Model.Aggregates;
using Vitalita.API.IAM.Domain.Model.ValueObjects;

namespace Vitalita.API.IAM.Infrastructure.Persistence.EFC.Configuration.Extensions;

/// <summary>
///     Fluent API configuration of the IAM bounded context.
/// </summary>
public static class ModelBuilderExtensions
{
    /// <summary>
    ///     Maximum length of the role and status columns.
    /// </summary>
    private const int EnumMaxLength = 20;

    /// <summary>
    ///     Length of a BCrypt hash.
    /// </summary>
    private const int PasswordHashLength = 60;

    /// <summary>
    ///     Configures the <see cref="User" /> aggregate (table <c>users</c>).
    /// </summary>
    /// <param name="builder">The model builder.</param>
    public static void ApplyIamConfiguration(this ModelBuilder builder)
    {
        builder.Entity<User>().HasKey(user => user.Id);
        builder.Entity<User>().Property(user => user.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Entity<User>().Property(user => user.Email)
            .HasConversion(valueObject => valueObject.Value, value => new EmailAddress(value))
            .HasMaxLength(EmailAddress.MaxLength)
            .IsRequired();
        builder.Entity<User>().HasIndex(user => user.Email).IsUnique();
        builder.Entity<User>().Property(user => user.PasswordHash).HasMaxLength(PasswordHashLength).IsRequired();
        builder.Entity<User>().Property(user => user.Role)
            .HasConversion<string>().HasMaxLength(EnumMaxLength).IsRequired();
        builder.Entity<User>().Property(user => user.Status)
            .HasConversion<string>().HasMaxLength(EnumMaxLength).IsRequired();
        builder.Entity<User>().Property(user => user.LastLoginAt);
        builder.Entity<User>().Ignore(user => user.IsActive);
    }
}
