using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Vitalita.API.Shared.Infrastructure.Persistence.EFC.Configuration.Converters;

/// <summary>
///     Value converter that keeps every <see cref="DateTime" /> column in UTC.
/// </summary>
/// <remarks>
///     MySQL <c>DATETIME</c> columns do not store a kind, so values come back as
///     <see cref="DateTimeKind.Unspecified" /> and System.Text.Json would serialize them without the <c>"Z"</c> suffix.
///     This converter normalizes values to UTC on write and marks them as <see cref="DateTimeKind.Utc" /> on read.
///     It is applied to all <see cref="DateTime" /> and <see cref="Nullable{DateTime}" /> properties in
///     <c>AppDbContext.ConfigureConventions</c>.
/// </remarks>
public sealed class UtcDateTimeConverter() : ValueConverter<DateTime, DateTime>(
    value => ToUtc(value),
    value => DateTime.SpecifyKind(value, DateTimeKind.Utc))
{
    private static DateTime ToUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };
}
