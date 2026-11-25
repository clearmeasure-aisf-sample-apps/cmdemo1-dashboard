using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The page's own rhythm beside a press (#1775): how often <c>TrafficPanel</c> reads the counts of a run, and how
/// often that makes it ask for a check. That is one rule, with one home: <see cref="TrafficPlan.CheckEverySeconds" />
/// whole seconds for the sentence it is told in, <see cref="TrafficPlan.CheckEvery" /> as the duration every
/// comparison reads - the two shapes <see cref="ProbeDefaults" /> already gives its own shared wait - and the panel's
/// follow tick cut out of that duration rather than written as a second of its own beside the count it divides it by.
/// </summary>
/// <remarks>
/// The tick is read from the compiled panel (a private static of the class the razor writes, the way
/// <see cref="ProbeDefaultsSharedWaitSecondsEdgeCaseTests" /> reads an internal the app keeps to itself) and from the
/// panel's source, the way <see cref="TrafficPanelTests" /> and <see cref="TrafficRunNamedValueTests" /> read that
/// file: the drift this guards against - a tick of half a second silently doubling the checks - lives exactly where
/// those two meet, and no compiled behaviour of the page is involved. The numbers themselves stay pinned by
/// <see cref="TelemetryTests" /> and <see cref="TrafficRunNamedValueTests" />, which these tests join rather than
/// replace.
/// </remarks>
public class TrafficFollowIntervalTests
{
    // ---------------------------------------------------------------- the rule, in both of its shapes

    /// <summary>
    /// The interval the page checks on while traffic runs has its duration beside its whole seconds, made of those
    /// seconds and held the way <see cref="ProbeDefaults" /> holds its own wait: a public static readonly
    /// <see cref="TimeSpan" />, so a caller that holds a length reads it instead of rebuilding one.
    /// </summary>
    [Fact]
    public void TheCheckIntervalOfThePageHasADurationBesideItsWholeSeconds()
    {
        var field = typeof(TrafficPlan).GetField(nameof(TrafficPlan.CheckEvery), BindingFlags.Public | BindingFlags.Static);

        Assert.NotNull(field);
        Assert.True(field!.IsInitOnly, "The check interval stays a static readonly TimeSpan, the way ProbeDefaults.DefaultTimeout is.");
        Assert.Equal(typeof(TimeSpan), field.FieldType);
        Assert.Equal(TimeSpan.FromSeconds(10), TrafficPlan.CheckEvery);
        Assert.Equal(TimeSpan.FromSeconds(TrafficPlan.CheckEverySeconds), TrafficPlan.CheckEvery);
    }

    /// <summary>
    /// What stands next to the interval control compares the durations and is still worded in whole seconds: nothing
    /// on the very length the control equals the rule, the same ten-second words above it, and nothing at all when
    /// no traffic runs.
    /// </summary>
    [Fact]
    public void TheIntervalNoteComparesTheDurationAndIsStillWordedInWholeSeconds()
    {
        Assert.Null(TrafficPlan.IntervalNote(running: true, TrafficPlan.CheckEvery));
        Assert.Null(TrafficPlan.IntervalNote(running: true, TrafficPlan.CheckEvery - TimeSpan.FromSeconds(1)));
        Assert.Equal("every 10 s while traffic runs", TrafficPlan.IntervalNote(running: true, TrafficPlan.CheckEvery + TimeSpan.FromSeconds(1)));
        Assert.Equal("every 10 s while traffic runs", TrafficPlan.IntervalNote(running: true, TimeSpan.FromSeconds(30)));
        Assert.Null(TrafficPlan.IntervalNote(running: false, TrafficPlan.CheckEvery + TimeSpan.FromDays(1)));
    }

    // ---------------------------------------------------------------- the tick that reads the counts

    /// <summary>The follow loop reads on the named tick, and no literal second is left to agree with the rule by
    /// chance: the timer takes <c>ReadEvery</c>, and that tick is written as the plan's check interval cut into the
    /// ticks the loop counts.</summary>
    [Fact]
    public void TheFollowLoopReadsOnTheNamedTickAndNotOnALiteral()
    {
        var text = MarkupGuards.ComponentText("TrafficPanel");

        Assert.Contains("new PeriodicTimer(ReadEvery, Time)", text, StringComparison.Ordinal);
        Assert.DoesNotContain("new PeriodicTimer(TimeSpan", text, StringComparison.Ordinal);
        Assert.Contains(
            "private static readonly TimeSpan ReadEvery = TrafficPlan.CheckEvery / TrafficPlan.CheckEverySeconds;",
            text,
            StringComparison.Ordinal);
    }

    /// <summary>
    /// The tick and the checks it counts are the one rule the issue asked for: those ticks counted
    /// <see cref="TrafficPlan.CheckEverySeconds" /> at a time span exactly <see cref="TrafficPlan.CheckEvery"/>, so
    /// the page still checks every tenth second while traffic runs - and a tick of another length, which would
    /// silently move the checks with it, fails here rather than in the page.
    /// </summary>
    [Fact]
    public void TheCountedTicksSpanExactlyTheIntervalThePageChecksOn()
    {
        var readEvery = ReadEveryOfThePanel();

        Assert.Equal(TimeSpan.FromSeconds(1), readEvery);
        Assert.Equal(TrafficPlan.CheckEvery, readEvery * TrafficPlan.CheckEverySeconds);

        // And the loop still counts them in the plan's whole seconds, once, in its own rule (#1742).
        Assert.Contains("++ticks % TrafficPlan.CheckEverySeconds == 0", MarkupGuards.ComponentText("TrafficPanel"), StringComparison.Ordinal);
    }

    // ---------------------------------------------------------------- the machinery

    /// <summary>The follow tick as the app compiles it: a private static of the class <c>TrafficPanel.razor</c>
    /// writes, which the app keeps to itself - the app declares no <c>InternalsVisibleTo</c>, so it is read by
    /// reflection rather than opened up for a test.</summary>
    private static TimeSpan ReadEveryOfThePanel()
    {
        var panels = typeof(TrafficRun).Assembly.GetTypes().Where(type => type.Name == "TrafficPanel").ToList();
        Assert.Single(panels);

        var field = panels[0].GetField("ReadEvery", BindingFlags.NonPublic | BindingFlags.Static);

        Assert.True(field is not null, "The panel's follow tick was not read: the guard would be checking nothing");
        Assert.True(field!.IsInitOnly, "The follow tick stays a static readonly TimeSpan, so the loop cannot retune it between runs.");

        return (TimeSpan)field.GetValue(null)!;
    }
}
