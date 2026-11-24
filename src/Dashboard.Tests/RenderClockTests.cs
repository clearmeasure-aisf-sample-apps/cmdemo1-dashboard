using Dashboard.Health;
using Microsoft.Extensions.Time.Testing;

namespace Dashboard.Tests;

/// <summary>
/// The page reads the clock once per render and every tile of that pass gets the same instant: two cost lines of
/// one rendering agree on "yesterday" and "this month" even when the clock crosses midnight between them.
/// </summary>
public class RenderClockTests
{
    // A second before a new month: the moment where two separate clock reads can disagree about the day words.
    private static readonly DateTimeOffset LastMomentOfOctober = new(2026, 10, 31, 23, 59, 59, 999, TimeSpan.Zero);
    private static readonly DateOnly AsOf = new(2026, 10, 30);
    private static readonly CostAmounts Amounts = new(1.52, 9.80, 11.02);

    [Fact]
    public void EveryAskOfOneRenderingIsTheSameInstantWhileTheClockRuns()
    {
        var clock = new FakeTimeProvider(LastMomentOfOctober);
        var render = new RenderClock(clock);

        var first = render.GetUtcNow();
        clock.Advance(TimeSpan.FromSeconds(5));

        Assert.Equal(LastMomentOfOctober, first);
        Assert.Equal(first, render.GetUtcNow());
    }

    [Fact]
    public void PinReadsTheClockForTheNextRendering()
    {
        var clock = new FakeTimeProvider(LastMomentOfOctober);
        var render = new RenderClock(clock);

        clock.Advance(TimeSpan.FromMinutes(1));
        render.Pin();

        Assert.Equal(LastMomentOfOctober.AddMinutes(1), render.GetUtcNow());
    }

    [Fact]
    public void ARunningClockIsRequired() =>
        Assert.Throws<ArgumentNullException>(() => new RenderClock(null!));

    [Fact]
    public void TwoCostLinesOfOnePassAgreeOnTheDayWordsAcrossMidnight()
    {
        var clock = new FakeTimeProvider(LastMomentOfOctober);
        var render = new RenderClock(clock);
        render.Pin();

        var systemLine = CostText.Parts(Amounts, "USD", AsOf, render.GetUtcNow());
        clock.Advance(TimeSpan.FromMilliseconds(2));
        var environmentLine = CostText.Parts(Amounts, "USD", AsOf, render.GetUtcNow());

        Assert.Equal(systemLine.Select(part => part.Period), environmentLine.Select(part => part.Period));
        Assert.Equal(["yesterday", "in 7 days", "this month"], environmentLine.Select(part => part.Period));

        // What each tile asking the clock itself would have said: the same pass, two different days.
        var liveLine = CostText.Parts(Amounts, "USD", AsOf, clock.GetUtcNow());
        Assert.NotEqual(systemLine.Select(part => part.Period), liveLine.Select(part => part.Period));
    }

    [Fact]
    public void AppPinsTheClockOnceAtTheTopOfItsMarkupAndReadsTheRunningClockOnlyWhenOpened()
    {
        var app = MarkupGuards.AppText();

        Assert.Equal(1, MarkupGuards.CountOccurrences(app, "@{ Time.Pin(); }"));
        Assert.True(app.IndexOf("@{ Time.Pin(); }", StringComparison.Ordinal) < app.IndexOf("<PageTitle>", StringComparison.Ordinal));
        Assert.True(app.IndexOf("@{ Time.Pin(); }", StringComparison.Ordinal) < app.IndexOf("Time.GetUtcNow()", StringComparison.Ordinal));

        // The tiles' Time is the render's instant; the running clock is read once, for when the page was opened.
        Assert.Contains("@inject TimeProvider Clock", app, StringComparison.Ordinal);
        Assert.Contains("private RenderClock Time { get; set; }", app, StringComparison.Ordinal);
        Assert.Equal(1, MarkupGuards.CountOccurrences(app, "Clock.GetUtcNow()"));
        Assert.Contains("_opened = Clock.GetUtcNow();", app, StringComparison.Ordinal);
    }
}
