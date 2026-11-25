using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// Where the follow tick lives (#1797): the page's rhythm rule is <see cref="TrafficPlan" />'s
/// (<see cref="TrafficPlan.CheckEverySeconds" />, <see cref="TrafficPlan.CheckEvery" />), and the tick that cuts that
/// duration into readings stays the panel's own, cut from the rule. Whichever home the tick has, it has one: a second
/// <c>ReadEvery</c> on the plan beside the panel's would be two numbers to keep agreeing.
/// </summary>
public class TrafficFollowTickHomeTests
{
    /// <summary>The plan holds the check rule and no follow tick of its own while the panel holds one.</summary>
    [Fact]
    public void TheFollowTickHasOneHomeAndThePlanHoldsOnlyTheCheckRule()
    {
        var panel = typeof(TrafficRun).Assembly.GetTypes().Single(type => type.Name == "TrafficPanel");
        var panelTick = panel.GetField("ReadEvery", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.True(panelTick is not null, "The panel's follow tick was not found: the guard would be checking nothing");

        var planTicks = typeof(TrafficPlan)
            .GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)
            .Where(member => member.Name.StartsWith("ReadEvery", StringComparison.Ordinal))
            .Select(member => member.Name)
            .ToList();

        Assert.Empty(planTicks);
    }

    /// <summary>
    /// The panel's tick is still made only of the plan's rule, so keeping it in the panel adds no number: one
    /// <see cref="TrafficPlan.CheckEvery" /> is exactly <see cref="TrafficPlan.CheckEverySeconds" /> ticks of a second.
    /// </summary>
    [Fact]
    public void ThePanelsTickIsMadeOnlyOfThePlansRule()
    {
        var panel = typeof(TrafficRun).Assembly.GetTypes().Single(type => type.Name == "TrafficPanel");
        var tick = (TimeSpan)panel.GetField("ReadEvery", BindingFlags.NonPublic | BindingFlags.Static)!.GetValue(null)!;

        Assert.Equal(TrafficPlan.CheckEvery / TrafficPlan.CheckEverySeconds, tick);
        Assert.Equal(TimeSpan.FromSeconds(1), tick);
        Assert.Equal(10, TrafficPlan.CheckEvery / tick);
    }
}
