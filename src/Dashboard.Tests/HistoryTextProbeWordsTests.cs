namespace Dashboard.Tests;

/// <summary>
/// The two named words a probe answer is said with: <c>HTTP 200</c> and <c>85 ms</c>. They are invariant
/// regardless of the viewer's culture, and <see cref="HistoryText.ResultFacts" />,
/// <see cref="ContainerAppAnswer.SaidShort" /> and the tile's latency cells all compose them from the same
/// two helpers.
/// </summary>
public class HistoryTextProbeWordsTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 7, 0, 0, 0, TimeSpan.Zero);

    // ----- the two helpers, invariant under a non-invariant reader

    [Fact]
    public void StatusIsTheHttpWordInvariant() =>
        Assert.Equal("HTTP 200", CultureScope.Says("fi-FI", () => HistoryText.Status(200)));

    [Fact]
    public void MillisecondsIsTheMsWordInvariant() =>
        Assert.Equal("85 ms", CultureScope.Says("fi-FI", () => HistoryText.Milliseconds(85)));

    [Fact]
    public void StatusAndMillisecondsAreInvariantUnderGerman()
    {
        ViewerCulture.German(() =>
        {
            Assert.Equal("HTTP 503", HistoryText.Status(503));
            Assert.Equal("12 ms", HistoryText.Milliseconds(12));
        });
    }

    // ----- SaidShort composes the helpers

    [Fact]
    public void SaidShortComposesTheHelpersForAnAnsweredProbe()
    {
        var answer = new ContainerAppAnswer("/h", true, 200, false, 85, Now);
        Assert.Equal("HTTP 200 · 85 ms", answer.SaidShort(false));
    }

    [Fact]
    public void SaidShortComposesTheHelpersForAStatusOnlyProbe()
    {
        var answer = new ContainerAppAnswer("/h", true, 200, false, null, Now);
        Assert.Equal("HTTP 200", answer.SaidShort(false));
    }

    [Fact]
    public void SaidShortComposesTheHelpersForAnOpaqueAnswer()
    {
        var answer = new ContainerAppAnswer("/h", true, null, true, 85, Now);
        Assert.Equal("answered · 85 ms", answer.SaidShort(false));
    }

    // ----- ResultFacts joins the helpers with the given separator

    [Fact]
    public void ResultFactsJoinsTheHelperWordsWithTheGivenSeparator()
    {
        var result = new ProbeResult(HealthState.Unhealthy, 503, 12, Now);
        Assert.Equal("HTTP 503, 12 ms", HistoryText.ResultFacts(result, ", "));
    }
}
