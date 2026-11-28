namespace Dashboard.Tests;

/// <summary>
/// Pins today's behavior of <c>WorkRelease.Read</c>'s item-number rule (the private <c>ItemNumber</c> helper),
/// exercised through <see cref="WorkItemsReport.Parse"/>: which array entries count, how halves are rounded
/// (banker's rounding via <c>Math.Round</c>, not <c>JsonRead.Count</c>'s away-from-zero), and how a release with
/// only skipped entries is refused or kept by its version.
/// </summary>
public class WorkReleaseItemNumberTests
{
    private static List<int>? Items(string itemsJson)
    {
        var json = "{ \"releases\": [ { \"version\": \"2.4.90\", \"items\": " + itemsJson + " } ] }";
        return WorkItemsReport.Parse(json)
            ?.Releases
            .SelectMany(release => release.Items)
            .ToList();
    }

    [Fact]
    public void OrderIsKeptFromTheFile()
    {
        Assert.Equal([462, 458], Items("[462, 458]"));
    }

    [Fact]
    public void AHalfIsRoundedToTheEvenNumber()
    {
        // Math.Round banker's rounding: 2.5 -> 2, 3.5 -> 4.
        Assert.Equal([2], Items("[2.5]"));
        Assert.Equal([4], Items("[3.5]"));
    }

    [Fact]
    public void AWholeDoubleReadsAsItsInt()
    {
        Assert.Equal([7], Items("[7.0]"));
    }

    [Theory]
    [InlineData("-1")]
    [InlineData("\"5\"")]
    [InlineData("{}")]
    [InlineData("null")]
    [InlineData("1e300")]
    [InlineData("3000000000")] // above int.MaxValue
    public void EntriesThatAreNotItemNumbersAreSkipped(string entry)
    {
        var items = Items("[" + entry + "]");
        Assert.NotNull(items);
        Assert.Empty(items);
    }

    [Fact]
    public void AReleaseWithOnlySkippedEntriesAndNoVersionIsRefused()
    {
        var report = WorkItemsReport.Parse("""{ "releases": [ { "items": [-1, "5", {}, null, 1e300] } ] }""")!;

        Assert.Empty(report.Releases);
    }

    [Fact]
    public void AReleaseWithOnlySkippedEntriesButAVersionIsKeptWithEmptyItems()
    {
        var report = WorkItemsReport.Parse("""{ "releases": [ { "version": "2.4.91", "items": [-1, "5", {}, null, 1e300] } ] }""")!;

        var release = Assert.Single(report.Releases);
        Assert.Equal("2.4.91", release.Version);
        Assert.Empty(release.Items);
    }
}
