namespace Dashboard.Tests;

/// <summary>
/// The ten bounds of the readers of the runtime folder, pinned by their names in <see cref="RuntimeReaderBounds" />:
/// each reader states exactly the bounds its nested shape names, each with the same value, so a new bound cannot go
/// unnamed and a changed one cannot go unnoticed.
/// </summary>
public class RuntimeReaderBoundsTests
{
    public static TheoryData<Type> Readers() => new(RuntimeReaderBounds.ShapeOf.Keys);

    public static TheoryData<Type, string, int> NamedBounds()
    {
        var data = new TheoryData<Type, string, int>();
        foreach (var (reader, shape) in RuntimeReaderBounds.ShapeOf)
        {
            foreach (var (name, value) in RuntimeReaderBounds.BoundsOf(shape))
            {
                data.Add(reader, name, value);
            }
        }

        return data;
    }

    /// <summary>Each named bound is the number the reader holds under the same name.</summary>
    [Theory]
    [MemberData(nameof(NamedBounds))]
    public void EachNamedBoundIsTheOneTheReaderHolds(Type reader, string name, int bound) =>
        Assert.Equal(bound, RuntimeReaderBounds.HeldBy(reader, name));

    /// <summary>The bounds a reader states are the ones its shape names: a new one is to be named there too.</summary>
    [Theory]
    [MemberData(nameof(Readers))]
    public void EveryBoundOfTheReaderIsNamed(Type reader)
    {
        var named = RuntimeReaderBounds.BoundsOf(RuntimeReaderBounds.ShapeOf[reader]);

        Assert.Equal(named.OrderBy(pair => pair.Key, StringComparer.Ordinal), RuntimeReaderBounds.BoundsOf(reader).OrderBy(pair => pair.Key, StringComparer.Ordinal));
    }

    /// <summary>The three readers state ten bounds between them (the issue says nine but lists ten), each with its value unchanged.</summary>
    [Fact]
    public void TheTenBoundsKeepTheirValues()
    {
        Assert.Equal(2000, RuntimeReaderBounds.Database.MostTables);
        Assert.Equal(1000, RuntimeReaderBounds.Database.MostColumns);
        Assert.Equal(2000, RuntimeReaderBounds.Application.MostAssemblies);
        Assert.Equal(1000, RuntimeReaderBounds.Application.MostEntries);
        Assert.Equal(300, RuntimeReaderBounds.Application.LongestText);
        Assert.Equal(300, RuntimeReaderBounds.Manifest.LongestUnplacedText);
        Assert.Equal(500, RuntimeReaderBounds.Manifest.MostUnplaced);
        Assert.Equal(2000, RuntimeReaderBounds.Manifest.LongestAddress);
        Assert.Equal(200, RuntimeReaderBounds.Manifest.LongestPath);
        Assert.Equal(200, RuntimeReaderBounds.Manifest.MostIds);
        Assert.Equal(10, RuntimeReaderBounds.ShapeOf.Values.Sum(shape => RuntimeReaderBounds.BoundsOf(shape).Count));
    }

    /// <summary>The application reader's shape and the older <see cref="ParserBounds" /> agree name for name.</summary>
    [Fact]
    public void TheApplicationShapeAgreesWithTheOlderHome() =>
        Assert.Equal(
            ParserBounds.ByProductionName.OrderBy(pair => pair.Key, StringComparer.Ordinal),
            RuntimeReaderBounds.BoundsOf(typeof(RuntimeReaderBounds.Application)).OrderBy(pair => pair.Key, StringComparer.Ordinal));
}
