using System.Text.Json;

namespace Dashboard.Tests;

/// <summary>
/// The one home of the stale rule, <see cref="ClusterStaleness" />, which the record's
/// <see cref="ClusterStatus.StaleAfter" /> and <see cref="ClusterStatus.IsStale" /> read; and the one place that says
/// what a list of a file is, which <see cref="JsonRead.HasArray" />, <see cref="JsonRead.Items" /> and
/// <see cref="JsonRead.Strings" /> - and so <see cref="ClusterStatus.Parse" /> - agree on.
/// </summary>
public class StaleRuleAndListHomeTests
{
    private static readonly DateTimeOffset Generated = new(2026, 11, 25, 12, 0, 0, TimeSpan.Zero);

    [Theory]
    [InlineData(null, 60)]
    [InlineData(int.MinValue, 60)]
    [InlineData(-15, 60)]
    [InlineData(0, 60)]
    [InlineData(15, 60)]
    [InlineData(16, 64)]
    [InlineData(30, 120)]
    [InlineData(900, 3600)]
    public void TheStaleRuleIsFourIntervalsAndAMinuteAtLeast(int? interval, double seconds) =>
        Assert.Equal(TimeSpan.FromSeconds(seconds), new ClusterStaleness(interval).After);

    [Theory]
    [InlineData(null)]
    [InlineData(int.MinValue)]
    [InlineData(0)]
    [InlineData(15)]
    [InlineData(16)]
    [InlineData(900)]
    [InlineData(int.MaxValue)]
    public void TheRecordsStaleAfterIsTheRuleOfItsInterval(int? interval)
    {
        var status = new ClusterStatus(Generated, interval, null, [], []);

        Assert.Equal(new ClusterStaleness(interval).After, status.StaleAfter);
        Assert.False(status.IsStale(Generated + new ClusterStaleness(interval).After));
        Assert.True(status.IsStale(Generated + new ClusterStaleness(interval).After + TimeSpan.FromTicks(1)));
    }

    [Fact]
    public void TheRuleIsNeverBelowTheMinuteAndCountsInTheIntervalsItNames()
    {
        Assert.Equal(ClusterStatus.MinimumStaleAfter, new ClusterStaleness(null).After);
        Assert.Equal(
            TimeSpan.FromSeconds(ClusterStatus.StaleIntervals * 600d),
            new ClusterStaleness(600).After);
    }

    [Theory]
    [InlineData("""{ "list": [] }""", true, 0)]
    [InlineData("""{ "list": [ "a", 1 ] }""", true, 2)]
    [InlineData("""{ "list": {} }""", false, 0)]
    [InlineData("""{ "list": "a" }""", false, 0)]
    [InlineData("""{ "list": null }""", false, 0)]
    [InlineData("""{ }""", false, 0)]
    [InlineData("""[ [] ]""", false, 0)]
    public void HasArrayAndItemsAgreeOnWhatAListIs(string json, bool hasArray, int items)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        Assert.Equal(hasArray, JsonRead.HasArray(root, "list"));
        Assert.Equal(items, JsonRead.Items(root, "list").Count());
        Assert.Equal(hasArray && items > 0 ? 1 : 0, JsonRead.Strings(root, "list")?.Count ?? 0);
    }

    [Theory]
    [InlineData("""{ "nodes": [] }""")]
    [InlineData("""{ "namespaces": [] }""")]
    [InlineData("""{ "nodes": {}, "namespaces": [] }""")]
    public void AnEmptyListIsStillAListToParse(string json)
    {
        var parsed = ClusterStatus.Parse(json);

        Assert.Null(parsed.Problem);
        Assert.NotNull(parsed.Value);
    }

    [Theory]
    [InlineData("""{ "nodes": {}, "namespaces": "none" }""")]
    [InlineData("""{ "nodes": null }""")]
    [InlineData("""{ "generated": "2026-11-25T12:00:00Z" }""")]
    public void AFileWithoutAListIsAProblemToParse(string json)
    {
        var parsed = ClusterStatus.Parse(json);

        Assert.Null(parsed.Value);
        Assert.Equal("the file has neither nodes nor namespaces", parsed.Problem);
    }
}
