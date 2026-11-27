namespace Dashboard.Tests;

/// <summary>
/// #1830 (step 3 of #534): every region mark and its helpers live in <c>Runtime/RuntimeRegionMarks.cs</c>, and
/// <c>RuntimePayloadBuilder</c> keeps the tiles and the edges and asks <c>RuntimeRegionMarks.Region</c> for each
/// region boundary. The source-scan guards below pin that the builder holds no second copy for them to miss, and
/// that neither class reaches into the other's private members.
/// </summary>
public class RuntimeRegionMarksFileTests
{
    private static readonly string[] Moved =
    [
        "Region",
    ];

    private static string Builder => File.ReadAllText(MarkupGuards.SourcePath(Path.Combine("Runtime", "RuntimePayloadBuilder.cs")));

    private static string RegionMarks => File.ReadAllText(MarkupGuards.SourcePath(Path.Combine("Runtime", "RuntimeRegionMarks.cs")));

    [Fact]
    public void EveryMovedMethodIsDeclaredInTheRegionMarksAndNotInTheBuilder()
    {
        var builder = Builder;
        var regionMarks = RegionMarks;

        Assert.Contains("internal static class RuntimeRegionMarks", regionMarks, StringComparison.Ordinal);
        Assert.Matches(@"static RuntimeRegionMark Region\(", regionMarks);
        Assert.DoesNotMatch(@"static RuntimeRegionMark Region\(", builder);
    }

    [Fact]
    public void TheBuilderMakesNoRegionMark()
    {
        var builder = Builder;

        Assert.True(builder.Length > 10_000, "only a few characters read from the builder: the guard is checking nothing");
        Assert.Equal(0, MarkupGuards.CountOccurrences(builder, "new RuntimeRegionMark("));
        Assert.Equal(1, MarkupGuards.CountOccurrences(builder, "RuntimeRegionMarks.Region(region, index, reachable.Contains(region.Alias))"));
    }

    [Fact]
    public void TheRegionMarksMakeAllTheMarksTheBuilderOnceMade()
    {
        // The new file makes the region marks the builder once made: 6 new RuntimeRegionMark calls
        // (one per state: neutral-unprobed, neutral-not-checked, serving, standby, checking, down).
        Assert.Equal(6, MarkupGuards.CountOccurrences(RegionMarks, "new RuntimeRegionMark("));
    }

    [Fact]
    public void TheEntryIsOneSharedInternalTypeAndTheRegionMarksReachNoPrivateOfTheBuilder()
    {
        var builder = Builder;
        var regionMarks = RegionMarks;
        var entry = typeof(RuntimePayloadBuilder).GetNestedType("Entry", System.Reflection.BindingFlags.NonPublic);

        // Internal (assembly), not private: the one entry type the builder and the region marks share.
        Assert.NotNull(entry);
        Assert.True(entry.IsNestedAssembly, "RuntimePayloadBuilder.Entry is not internal");
        Assert.Equal(1, MarkupGuards.CountOccurrences(builder, "internal sealed record Entry("));
        Assert.DoesNotContain("record Entry(", regionMarks, StringComparison.Ordinal);

        // The region marks read the builder's entry type, never one of its private helpers.
        var reaches = System.Text.RegularExpressions.Regex.Matches(regionMarks, @"RuntimePayloadBuilder\.(\w+)")
            .Select(match => match.Groups[1].Value)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal);
        Assert.Equal(["Entry"], reaches);
    }
}
