namespace Dashboard.Tests;

/// <summary>
/// The words and the icon of the events strip, pinned against the rules <see cref="EventText" /> owns once: where an
/// event is (both parts, the one that is there, the page's own word), the tooltip word of its level's mark, and the
/// icon state its level stands on - every arm of every rule, the undefined level, and a null event refused.
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: every moment is one the test brings.
/// </para>
/// </summary>
public class EventTextTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 19, 12, 0, 0, TimeSpan.Zero);

    /// <summary>An event the test names: its place parts and its level, with the rest at its record defaults.</summary>
    private static DashboardEvent Event(string? environment = null, string? node = null, EventLevel level = EventLevel.Info) =>
        new(Now, EventKind.Health, level, environment, node, string.Empty);

    // ----- Where the event is -----

    /// <summary>An event of an environment and a node is said to be at both, in the order the record holds them.</summary>
    [Fact]
    public void AnEventOfAnEnvironmentAndANodeIsSaidToBeAtBoth() =>
        Assert.Equal("production · eu-west1", EventText.Where(Event("production", "eu-west1")));

    /// <summary>An event of an environment alone is said to be at the environment, without a separator behind it.</summary>
    [Fact]
    public void AnEventOfAnEnvironmentAloneIsSaidToBeAtTheEnvironment() =>
        Assert.Equal("production", EventText.Where(Event("production")));

    /// <summary>An event of a node alone is said to be at the node; the cluster's events come through the node,
    /// <c>cluster</c>, and are said by the same rule without special-casing.</summary>
    [Fact]
    public void AnEventOfANodeAloneIsSaidToBeAtTheNode()
    {
        Assert.Equal("eu-west1", EventText.Where(Event(node: "eu-west1")));
        Assert.Equal("cluster", EventText.Where(Event(node: ClusterEventDetector.ClusterPlace)));
    }

    /// <summary>An event of the page itself - neither an environment nor a node - is said to be at the page.</summary>
    [Fact]
    public void AnEventOfThePageItselfIsSaidToBeAtThePage() =>
        Assert.Equal("page", EventText.Where(Event()));

    /// <summary>A null event is no event: the place is not guessed.</summary>
    [Fact]
    public void ANullEventHasNoPlace() =>
        Assert.Throws<ArgumentNullException>(() => EventText.Where(null!));

    // ----- The tooltip word of the level's mark -----

    /// <summary>Each named level says its own word, and a level no detector hands out says the information word.</summary>
    [Theory]
    [InlineData(EventLevel.Info, "For information")]
    [InlineData(EventLevel.Good, "Good news")]
    [InlineData(EventLevel.Warning, "Worth a look")]
    [InlineData(EventLevel.Problem, "A problem")]
    public void EachEventLevelSaysItsOwnTooltipWord(EventLevel level, string word) =>
        Assert.Equal(word, EventText.LevelWord(level));

    /// <summary>A level the enum does not name says the information word, the rule's open arm.</summary>
    [Fact]
    public void AnUndefinedEventLevelSaysTheInformationWord() =>
        Assert.Equal("For information", EventText.LevelWord((EventLevel)99));

    // ----- The icon state the level stands on -----

    /// <summary>Each named level stands on its own icon state, and a level no detector hands out stands on
    /// <see cref="HealthState.Pending" />, the rule's open arm.</summary>
    [Theory]
    [InlineData(EventLevel.Info, HealthState.Pending)]
    [InlineData(EventLevel.Good, HealthState.Healthy)]
    [InlineData(EventLevel.Warning, HealthState.Unhealthy)]
    [InlineData(EventLevel.Problem, HealthState.Unreachable)]
    public void EachEventLevelStandsOnItsOwnIconState(EventLevel level, HealthState state) =>
        Assert.Equal(state, EventText.Icon(level));

    /// <summary>A level the enum does not name stands on <see cref="HealthState.Pending" />, the rule's open arm.</summary>
    [Fact]
    public void AnUndefinedEventLevelStandsOnPending() =>
        Assert.Equal(HealthState.Pending, EventText.Icon((EventLevel)99));
}
