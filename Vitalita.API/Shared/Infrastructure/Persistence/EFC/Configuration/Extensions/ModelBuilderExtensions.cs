using Microsoft.EntityFrameworkCore;

namespace Vitalita.API.Shared.Infrastructure.Persistence.EFC.Configuration.Extensions;

/// <summary>
///     Model builder extensions for the database naming conventions.
/// </summary>
public static class ModelBuilderExtensions
{
    /// <summary>
    ///     Applies the snake_case naming convention to tables, columns, keys, foreign keys and indexes,
    ///     and pluralizes table names (<c>OlderAdult</c> → <c>older_adults</c>).
    /// </summary>
    /// <remarks>
    ///     Call it at the end of <c>OnModelCreating</c>, after every entity has been configured.
    /// </remarks>
    /// <param name="builder">The model builder.</param>
    public static void UseSnakeCaseNamingConvention(this ModelBuilder builder)
    {
        foreach (var entity in builder.Model.GetEntityTypes())
        {
            var tableName = entity.GetTableName();
            if (!string.IsNullOrEmpty(tableName)) entity.SetTableName(tableName.ToPlural().ToSnakeCase());

            foreach (var property in entity.GetProperties())
                property.SetColumnName(property.GetColumnName().ToSnakeCase());

            foreach (var key in entity.GetKeys())
            {
                var keyName = key.GetName();
                if (!string.IsNullOrEmpty(keyName)) key.SetName(keyName.ToSnakeCase());
            }

            foreach (var foreignKey in entity.GetForeignKeys())
            {
                var foreignKeyName = foreignKey.GetConstraintName();
                if (!string.IsNullOrEmpty(foreignKeyName)) foreignKey.SetConstraintName(foreignKeyName.ToSnakeCase());
            }

            foreach (var index in entity.GetIndexes())
            {
                var indexDatabaseName = index.GetDatabaseName();
                if (!string.IsNullOrEmpty(indexDatabaseName)) index.SetDatabaseName(indexDatabaseName.ToSnakeCase());
            }
        }
    }
}
