using System.Globalization;

namespace Dashboard.Tests;

/// <summary>
/// The start of a traffic run writes its sentence from the plan, not from markup: <see cref="TrafficPlan.Started"/>
/// takes the run a press sends and says its rate, length and targets, the way the end sentence of
/// <see cref="TrafficCount.Ended"/> says its counts - invariant culture, the words read from the run, the targets
/// joined with the list's separator. The panel hands the sentence to the log as given.
/// </summary>
public class TrafficPlanStartedTests
{
    private static TrafficPlan Plan(params string[] targets) =>
        new("uat", [new Uri("https://uat.example.net")], targets);

    /// <summary>The exact sentence for one target: the lead, the run's rate and length, then the single target.</summary>
    [Fact]
    public void SaysTheRunAndTheOneTarget()
    {
        var plan = Plan("ui at uat.example.net");

        Assert.Equal(
            "Traffic started: 2 requests a second for 60 s to ui at uat.example.net",
            plan.Started(TrafficRun.Default));
    }

    /// <summary>Two targets are joined with the list's separator, in the order the plan carries them.</summary>
    [Fact]
    public void JoinsTwoTargetsWithTheListSeparator()
    {
        var plan = Plan("ui at uat.example.net", "api at api.uat.example.net");

        Assert.Equal(
            "Traffic started: 2 requests a second for 60 s to ui at uat.example.net, api at api.uat.example.net",
            plan.Started(TrafficRun.Default));
    }

    /// <summary>
    /// The numbers said are the run's own, read by name: the sentence is composed of the run's
    /// <see cref="TrafficRun.PerSecond"/> and <see cref="TrafficRun.Seconds"/>, so a change of the run's numbers would
    /// change the sentence - it is not a copy of the default's words re-typed beside the run.
    /// </summary>
    [Fact]
    public void SaysTheRunItIsPassedAndNotLiterals()
    {
        var plan = Plan("ui at uat.example.net");

        // The sentence is exactly what the run's named values say, in the words the plan gives: reading the numbers
        // from the run is what keeps them from being literals a change of the run would not reach.
        var expected = $"Traffic started: {TrafficRun.Default.PerSecond} requests a second for {TrafficRun.Default.Seconds} s to ui at uat.example.net";
        Assert.Equal(expected, plan.Started(TrafficRun.Default));
    }

    /// <summary>
    /// The sentence is written in the invariant culture: under a reader who would otherwise turn the numbers, the
    /// words of a run stay the same.
    /// </summary>
    [Fact]
    public void SaysTheSameWordsInANonInvariantCulture()
    {
        var plan = Plan("ui at uat.example.net", "api at api.uat.example.net");
        var expected = "Traffic started: 2 requests a second for 60 s to ui at uat.example.net, api at api.uat.example.net";

        using var scope = new CultureScope(CultureInfo.GetCultureInfo("de-DE"));
        Assert.Equal(expected, plan.Started(TrafficRun.Default));
    }
}
