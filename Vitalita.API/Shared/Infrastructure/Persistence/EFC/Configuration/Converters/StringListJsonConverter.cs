using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Vitalita.API.Shared.Infrastructure.Persistence.EFC.Configuration.Converters;

/// <summary>
///     Value converter that stores a list of strings as a JSON array (MySQL <c>json</c> column).
/// </summary>
/// <remarks>
///     Use it together with <see cref="Comparer" />, so EF Core detects changes by content and not by reference.
///     A <c>null</c> or invalid column value is read as an empty list.
/// </remarks>
public sealed class StringListJsonConverter() : ValueConverter<IReadOnlyList<string>, string>(
    value => Serialize(value),
    value => Deserialize(value))
{
    /// <summary>
    ///     Compares lists by their items, in order, and makes snapshots with a copy of the items.
    /// </summary>
    public static readonly ValueComparer<IReadOnlyList<string>> Comparer = new(
        (left, right) => left == null ? right == null : right != null && left.SequenceEqual(right),
        list => list.Aggregate(0, (hash, item) => HashCode.Combine(hash, item.GetHashCode(StringComparison.Ordinal))),
        list => list.ToList());

    private static string Serialize(IReadOnlyList<string>? value) => JsonSerializer.Serialize(value ?? []);

    private static IReadOnlyList<string> Deserialize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return [];
        try
        {
            return JsonSerializer.Deserialize<List<string>>(value) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }
}
