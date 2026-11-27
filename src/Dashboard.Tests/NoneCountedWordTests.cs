using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The word a window with no ended report says in place of <see cref="AvailabilityText.Count"/> -
/// <c>NoneCounted</c> in <see cref="AvailabilityText"/> - is pinned to the letter, and the two places the word is
/// said are checked against it: the seven-days part of the availability line after a day it takes before, and the
/// counts of a container app's box.
/// <para>
/// The constant is internal and the app declares no <c>InternalsVisibleTo</c>, so the word is read by reflection
/// under its own name; the agreement is pinned against the rendered output of <see cref="AvailabilityText.Parts"/>
/// and <see cref="HourlyCheckText.Counts"/>, so a re-spelled literal on either side is seen here.
/// </para>
/// </summary>
public class NoneCountedWordTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 7, 1, 30, 0, TimeSpan.Zero);

    /// <summary>The <c>NoneCounted</c> constant, reached by reflection under its own name.</summary>
    private static string NoneCounted()
    {
        var field = typeof(AvailabilityText).GetField("NoneCounted", BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(field);
        Assert.True(field!.IsLiteral);
        Assert.True(field.IsAssembly, "NoneCounted is internal, so the box's word can read it.");
        Assert.Null(typeof(AvailabilityText).GetField("NoneCounted", BindingFlags.Public | BindingFlags.Static));
        return (string)field.GetRawConstantValue()!;
    }

    /// <summary>The constant is the word <c>none</c>, and not a different spelling.</summary>
    [Fact]
    public void TheConstIsTheWordNone() => Assert.Equal("none", NoneCounted());

    /// <summary>
    /// The seven-days part after a day reads <c>NoneCounted</c> as its strong word when the week counted no
    /// report: the part is the empty before, the const, and the window's after, with the tooltip the week alone
    /// would take not.
    /// </summary>
    [Fact]
    public void TheWeekAfterTheDaySaysNoneCountedWhenNoReportEnded()
    {
        var word = NoneCounted();
        var parts = AvailabilityText.Parts(new HealthReports(new HealthWindow(1, 1), new HealthWindow(0, 0), null), Now);

        Assert.Equal(word, parts[1].Strong);
        Assert.Equal(new AvailabilityPart(string.Empty, word, " in 7 days"), parts[1]);
    }

    /// <summary>
    /// The counts of a container app with a window that counted no report contains <c>NoneCounted</c> for that
    /// window, in the position the box says it.
    /// </summary>
    [Fact]
    public void TheCountsWithZeroReportsContainsNoneCounted()
    {
        var word = NoneCounted();
        var dayOnly = new ContainerAppReports("scorecard", null, new HealthWindow(0, 0), null);
        var weekOnly = new ContainerAppReports("scorecard", null, null, new HealthWindow(0, 0));

        Assert.Equal($"24 h: {word}", HourlyCheckText.Counts(dayOnly));
        Assert.Equal($"7 d: {word}", HourlyCheckText.Counts(weekOnly));
    }
}
