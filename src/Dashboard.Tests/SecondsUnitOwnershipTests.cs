using System.Globalization;
using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The seconds word in the traffic and probe sentences comes from its owner, <see cref="TimeText.SecondsUnit"/>,
/// not from a " s" typed by hand beside each sentence (#2336). Each of the four sentences is checked against a
/// string built from that const, so a reword of the unit reaches them all while the text stays the same. The
/// sentences the dashboard says are unchanged: the word is read from its owner, not re-typed.
/// </summary>
public class SecondsUnitOwnershipTests
{
    private static TrafficPlan Plan(params string[] targets) =>
        new("uat", [new Uri("https://uat.example.net")], targets);

    /// <summary>
    /// <see cref="TrafficPlan.Started"/> says its length with the owner's word: the sentence is exactly the lead, the
    /// run's rate, its length in <see cref="TimeText.SecondsUnit"/>, and the targets - no hand-typed " s".
    /// </summary>
    [Fact]
    public void StartedReadsItsLengthFromSecondsUnit()
    {
        var plan = Plan("web at x.azurefd.net");
        var expected = $"Traffic started: {TrafficRun.Default.PerSecond} requests a second for {TrafficRun.Default.Seconds} {TimeText.SecondsUnit} to web at x.azurefd.net";

        Assert.Equal(expected, plan.Started(TrafficRun.Default));
    }

    /// <summary>
    /// <see cref="TrafficPlan.IntervalNote"/> says the check interval with the owner's word: <c>every 10 s while
    /// traffic runs</c> is built from <see cref="TimeText.SecondsUnit"/>, the number read from
    /// <see cref="TrafficPlan.CheckEverySeconds"/>.
    /// </summary>
    [Fact]
    public void IntervalNoteReadsItsLengthFromSecondsUnit()
    {
        var note = TrafficPlan.IntervalNote(running: true, interval: TimeSpan.FromSeconds(TrafficPlan.CheckEverySeconds + 1));

        Assert.Equal($"every {TrafficPlan.CheckEverySeconds} {TimeText.SecondsUnit} while traffic runs", note);
    }

    /// <summary>
    /// <see cref="TrafficCount.Status"/> says the seconds left with the owner's word, in the invariant culture: the
    /// seconds-left part of the line is the counts, then the seconds in <see cref="TimeText.SecondsUnit"/>, " left.".
    /// </summary>
    [Fact]
    public void StatusReadsItsSecondsLeftFromSecondsUnit()
    {
        var count = new TrafficCount(Sent: 119, Answered: 3, Failed: 0, Unanswered: 0, Sending: true);
        var expected = string.Create(CultureInfo.InvariantCulture, $"{count.Counts}; 42 {TimeText.SecondsUnit} left.");

        Assert.Equal(expected, count.Status(42));
    }

    /// <summary>
    /// <see cref="ProbeDefaults.NoAnswerWithin"/> says the wait in whole seconds with the owner's word, rounded and in
    /// the invariant culture: <c>no answer within 10 s</c> for ten, <c>no answer within 3 s</c> for 2.5 s, both built
    /// from <see cref="TimeText.SecondsUnit"/>. The class is internal, so the clause is reached by reflection, the
    /// way <see cref="TrafficCountProbeClauseTests"/> does.
    /// </summary>
    [Theory]
    [InlineData(10, 10)]
    [InlineData(2.5, 3)]
    public void NoAnswerWithinReadsItsLengthFromSecondsUnit(double seconds, int wholeSeconds)
    {
        var clause = NoAnswerWithin(TimeSpan.FromSeconds(seconds));
        var expected = string.Create(CultureInfo.InvariantCulture, $"no answer within {wholeSeconds} {TimeText.SecondsUnit}");

        Assert.Equal(expected, clause);
    }

    /// <summary>Reach the internal <c>ProbeDefaults.NoAnswerWithin</c> by reflection.</summary>
    private static string NoAnswerWithin(TimeSpan timeout)
    {
        var type = typeof(NodeProber).Assembly.GetType("Dashboard.Health.ProbeDefaults", throwOnError: true)!;
        var method = type.GetMethod("NoAnswerWithin", BindingFlags.Public | BindingFlags.Static, [typeof(TimeSpan)])!;
        return (string)method.Invoke(null, [timeout])!;
    }
}
