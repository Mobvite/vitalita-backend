using Vitalita.API.Shared.Infrastructure.Persistence.EFC.Configuration.Converters;

namespace Vitalita.API.Tests.Shared.Infrastructure.Persistence.EFC.Configuration.Converters;

public class ProfilesConvertersTests
{
    private readonly StringListJsonConverter _listConverter = new();
    private readonly DateOnlyConverter _dateConverter = new();

    [Fact]
    public void StringList_RoundTripsAsJsonArray()
    {
        IReadOnlyList<string> list = ["Penicilina", "Hipertensión \"arterial\""];

        var json = (string)_listConverter.ConvertToProvider(list)!;
        var back = (IReadOnlyList<string>)_listConverter.ConvertFromProvider(json)!;

        Assert.StartsWith("[", json);
        Assert.Equal(list, back);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not json")]
    [InlineData("null")]
    public void StringList_InvalidColumnValue_IsAnEmptyList(string value)
    {
        Assert.Empty((IReadOnlyList<string>)_listConverter.ConvertFromProvider(value)!);
    }

    [Fact]
    public void StringList_Comparer_ComparesByContentAndSnapshotsACopy()
    {
        var comparer = StringListJsonConverter.Comparer;
        IReadOnlyList<string> left = ["a", "b"];
        IReadOnlyList<string> same = new List<string> { "a", "b" };
        IReadOnlyList<string> other = ["b", "a"];

        Assert.True(comparer.Equals(left, same));
        Assert.False(comparer.Equals(left, other));
        Assert.Equal(comparer.GetHashCode(left), comparer.GetHashCode(same));
        Assert.NotSame(left, comparer.Snapshot(left));
        Assert.Equal(left, comparer.Snapshot(left));
    }

    [Fact]
    public void DateOnly_RoundTripsThroughMidnightDateTime()
    {
        var date = new DateOnly(1948, 3, 12);

        var stored = (DateTime)_dateConverter.ConvertToProvider(date)!;
        var back = (DateOnly)_dateConverter.ConvertFromProvider(new DateTime(1948, 3, 12, 0, 0, 0))!;

        Assert.Equal(new DateTime(1948, 3, 12), stored);
        Assert.Equal(TimeSpan.Zero, stored.TimeOfDay);
        Assert.Equal(date, back);
    }
}
