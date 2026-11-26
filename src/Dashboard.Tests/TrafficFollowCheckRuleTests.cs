using System.Reflection;
using System.Text.RegularExpressions;

namespace Dashboard.Tests;

/// <summary>
/// The follow loop's checks as a rule rather than as text (#1794): <c>TrafficPanel</c> asks for a check once every
/// <see cref="TrafficPlan.CheckEvery" /> while a run lasts, however it counts its ticks to get there. The guards
/// beside this one pin the loop's modulo as the literal <c>TrafficPlan.CheckEverySeconds</c>. This one accepts any
/// divisor the panel can name, a plan constant or a member of its own such as <c>TicksPerCheck</c>, and asks only
/// that this many of the panel's <c>ReadEvery</c> ticks come to exactly <see cref="TrafficPlan.CheckEvery" />.
/// </summary>
public partial class TrafficFollowCheckRuleTests
{
    /// <summary>The panel's tick fits a whole number of times into the check interval: ten ticks of a second, with
    /// nothing left over that a count of ticks could not reach.</summary>
    [Fact]
    public void ThePanelsTickDividesTheCheckIntervalExactly()
    {
        var readEvery = PanelStatic<TimeSpan>("ReadEvery");

        Assert.True(readEvery > TimeSpan.Zero, "The follow tick has a length.");
        Assert.Equal(0, TrafficPlan.CheckEvery.Ticks % readEvery.Ticks);
        Assert.Equal(10, TrafficPlan.CheckEvery / readEvery);
    }

    /// <summary>The loop has one modulo guard, and its divisor, wherever it is held, counts the ticks that make up
    /// exactly one <see cref="TrafficPlan.CheckEvery" />. So the page still checks every tenth second while a run
    /// lasts.</summary>
    [Fact]
    public void TheLoopAsksForACheckOnceEveryCheckIntervalHoweverItCountsItsTicks()
    {
        var guards = ModuloGuard().Matches(MarkupGuards.ComponentText("TrafficPanel"));
        Assert.Single(guards);

        var ticksPerCheck = DivisorValue(guards[0].Groups["divisor"].Value);
        var readEvery = PanelStatic<TimeSpan>("ReadEvery");

        Assert.True(ticksPerCheck > 0, "The loop counts a positive number of ticks between checks.");
        Assert.Equal(TrafficPlan.CheckEvery, readEvery * ticksPerCheck);
    }

    [GeneratedRegex(@"\+\+ticks % (?<divisor>[A-Za-z_][\w.]*) == 0")]
    private static partial Regex ModuloGuard();

    /// <summary>The value the modulo divides by: a <see cref="TrafficPlan" /> constant when the divisor is qualified
    /// with the plan, otherwise a static of the panel itself.</summary>
    private static int DivisorValue(string divisor)
    {
        const string Plan = "TrafficPlan.";
        if (!divisor.StartsWith(Plan, StringComparison.Ordinal))
        {
            return PanelStatic<int>(divisor);
        }

        var field = typeof(TrafficPlan).GetField(divisor[Plan.Length..], BindingFlags.Public | BindingFlags.Static);
        Assert.True(field is not null, $"The loop's divisor {divisor} was not found on the plan.");
        return (int)field!.GetValue(null)!;
    }

    /// <summary>A private static field or property of the class <c>TrafficPanel.razor</c> compiles to, read by
    /// reflection because the app declares no <c>InternalsVisibleTo</c>.</summary>
    private static T PanelStatic<T>(string name)
    {
        var panel = Assert.Single(typeof(TrafficRun).Assembly.GetTypes(), type => type.Name == "TrafficPanel");
        const BindingFlags Statics = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static;

        var value = panel.GetField(name, Statics)?.GetValue(null) ?? panel.GetProperty(name, Statics)?.GetValue(null);
        Assert.True(value is T, $"The panel's static {name} was not read as a {typeof(T).Name}: the guard would be checking nothing.");
        return (T)value!;
    }
}
