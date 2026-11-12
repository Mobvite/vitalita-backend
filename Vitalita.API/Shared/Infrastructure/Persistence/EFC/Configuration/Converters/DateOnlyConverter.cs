using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Vitalita.API.Shared.Infrastructure.Persistence.EFC.Configuration.Converters;

/// <summary>
///     Value converter that stores a <see cref="DateOnly" /> in a MySQL <c>date</c> column through a
///     <see cref="DateTime" />.
/// </summary>
/// <remarks>
///     <c>MySql.EntityFrameworkCore</c> reads <c>date</c> columns as <see cref="DateTime" />, so reading a
///     <see cref="DateOnly" /> property directly throws an <see cref="InvalidCastException" />. The time part is always
///     midnight and is dropped on read. It is applied to all <see cref="DateOnly" /> and <see cref="Nullable{DateOnly}" />
///     properties in <c>AppDbContext.ConfigureConventions</c>; the column type stays <c>date</c>.
/// </remarks>
public sealed class DateOnlyConverter() : ValueConverter<DateOnly, DateTime>(
    value => value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified),
    value => DateOnly.FromDateTime(value));
