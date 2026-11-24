using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The tests of what <see cref="RecordSurface" /> gathers last: <see cref="RecordSurface.NullableParts(Type)" />, the one
/// reading of which parts may be nothing that the <see cref="NullabilityInfoContext" /> block of each
/// <c>*EdgeCaseTests</c> file repeats - it says, for every record such a block pins, the very flags that block pins - and
/// the typed copies of the surface walk no other test yet holds against <see cref="RecordSurface.Of(Type)" />.
/// </summary>
public class RecordSurfaceNullablePartsTests
{
    private const BindingFlags Private = BindingFlags.NonPublic | BindingFlags.Static;

    [Fact]
    public void NullablePartsSaysWhatNodeInfoEdgeCaseTestsPins() =>
        Assert.Equal([false, true, false, false, true], RecordSurface.NullableParts<NodeInfo>());

    [Fact]
    public void NullablePartsSaysWhatRuntimeEdgeEdgeCaseTestsPins() =>
        Assert.Equal([false, false, false, false, true], RecordSurface.NullableParts<RuntimeEdge>());

    [Fact]
    public void NullablePartsSaysWhatRuntimeManifestEdgeCaseTestsPins() =>
        Assert.Equal([false, false, false, false, true, true], RecordSurface.NullableParts<RuntimeManifest>());

    [Fact]
    public void NullablePartsSaysWhatRuntimeNameEdgeCaseTestsPins() =>
        Assert.Equal([false, false], RecordSurface.NullableParts<RuntimeName>());

    [Fact]
    public void NullablePartsSaysWhatWorkItemEnvironmentsEdgeCaseTestsPins() =>
        Assert.Equal([true, true, true], RecordSurface.NullableParts<WorkItemEnvironments>());

    [Fact]
    public void NullablePartsSaysWhatRuntimeManifestRecordsEdgeCaseTestsPins()
    {
        Assert.Equal([false, true], RecordSurface.NullableParts<RuntimeIndex>());
        Assert.Equal([false, false], RecordSurface.NullableParts<RuntimeDatabaseFiles>());
    }

    [Theory]
    [InlineData(typeof(NodeInfo))]
    [InlineData(typeof(RuntimeEdge))]
    [InlineData(typeof(RuntimeManifest))]
    [InlineData(typeof(RuntimeName))]
    [InlineData(typeof(WorkItemEnvironments))]
    [InlineData(typeof(RuntimeIndex))]
    [InlineData(typeof(RuntimeDatabaseFiles))]
    [InlineData(typeof(RuntimeIndexResult))]
    public void NullablePartsSaysWhatEachFilesOwnNullabilityBlockComputes(Type record)
    {
        // The block every file carries, written out once more here as the oracle the shared reading is held against.
        var nullability = new NullabilityInfoContext();
        var parameters = Assert.Single(record.GetConstructors()).GetParameters();

        Assert.Equal(
            [.. parameters.Select(parameter => nullability.Create(parameter).WriteState == NullabilityState.Nullable)],
            RecordSurface.NullableParts(record));
    }

    [Fact]
    public void NullablePartsOfTIsOfTheType()
    {
        var type = typeof(NodeInfo);

        Assert.Equal(RecordSurface.NullableParts(type), RecordSurface.NullableParts<NodeInfo>());
    }

    [Fact]
    public void NullablePartsFollowsTheOrderTheConstructorNamesItsParts()
    {
        Assert.Equal([true, false, true, false], RecordSurface.NullableParts<Sample>());
        Assert.Equal(
            ["First", "Second", "Third", "Fourth"],
            [.. RecordSurface.ParametersOf<Sample>().Select(parameter => parameter.Name!)]);
    }

    [Fact]
    public void NullablePartsOfARecordWithNoPartsIsEmpty() =>
        Assert.Empty(RecordSurface.NullableParts<NoParts>());

    [Fact]
    public void NullablePartsRefusesATypeWithMoreThanOnePublicConstructor() =>
        Assert.Throws<InvalidOperationException>(() => RecordSurface.NullableParts<TwoWays>());

    [Theory]
    [InlineData(typeof(RuntimeIndexResultEdgeCaseTests), typeof(RuntimeIndexResult))]
    [InlineData(typeof(RuntimeManifestRecordsEdgeCaseTests), typeof(RuntimeIndex))]
    [InlineData(typeof(RuntimeManifestRecordsEdgeCaseTests), typeof(RuntimeDatabaseFiles))]
    public void OfSaysWhatTheRemainingTypedCopiesOfTheWalkSay(Type tests, Type record)
    {
        var copy = tests.GetMethod("PublicSurface", Private, [typeof(Type)]);
        Assert.NotNull(copy);

        var expected = Assert.IsType<List<string>>(copy.Invoke(null, [record]));

        Assert.Equal(expected, RecordSurface.Of(record));
    }

    private sealed record Sample(string? First, int Second, int? Third, string Fourth);

    private sealed record NoParts;

    private sealed class TwoWays
    {
        public TwoWays()
        {
        }

        public TwoWays(string? word) => Word = word;

        public string? Word { get; }
    }
}
