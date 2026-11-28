using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// <see cref="Dashboard.Work.WorkMeasureText" /> is internal to an app that declares no <c>InternalsVisibleTo</c>, so
/// these tests read it by reflection. They pin that the rework rate's percent is cut through the dashboard's one rule
/// for a share (<see cref="Dashboard.Shares" />), not a private scale of its own: <c>0.125</c> says <c>12.5 %</c>, a
/// rate above one is kept as the file gave it, and the class declares no <c>PercentPerWhole</c> field.
/// </summary>
public class WorkMeasureTextShareTests
{
    private static readonly Assembly App = typeof(Dashboard.Work.WorkItemsText).Assembly;

    private static Type Home => App.GetType("Dashboard.Work.WorkMeasureText", throwOnError: true)!;

    private static string Rework(double? rate) =>
        (string)Home.GetMethod("Rework", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)!
            .Invoke(null, [new Dashboard.Work.WorkRework(null, null, rate)])!;

    /// <summary>A rate of 0.125 is a share of one, cut by <see cref="Dashboard.Shares.Of" /> to 12.5 percent and
    /// kept to one decimal without a trailing zero.</summary>
    [Fact]
    public void AQuarterFractionOfOneIsTwelvePointFivePercent()
    {
        Assert.Equal("12.5 %", Rework(0.125));
    }

    /// <summary>A rate of 0 is a share of zero of one: 0 percent.</summary>
    [Fact]
    public void AZeroRateIsZeroPercent()
    {
        Assert.Equal("0 %", Rework(0));
    }

    /// <summary>A rate of exactly 1 is a whole of one: 100 percent, not 100.0.</summary>
    [Fact]
    public void AWholeRateIsOneHundredPercent()
    {
        Assert.Equal("100 %", Rework(1.0));
    }

    /// <summary>A rate near a rounding boundary keeps one decimal: 0.1234 rounds to 12.3 percent.</summary>
    [Fact]
    public void ARateNearARoundingBoundaryKeepsOneDecimal()
    {
        Assert.Equal("12.3 %", Rework(0.1234));
    }

    /// <summary>A rate above one is not a fraction: it is kept as the file gave it, to two decimals, no percent
    /// suffix.</summary>
    [Fact]
    public void ARateAboveOneIsKeptAsTheFileGaveIt()
    {
        Assert.Equal("1.5", Rework(1.5));
    }

    /// <summary>The percent scale has one owner. <see cref="Dashboard.Work.WorkMeasureText" /> declares no
    /// <c>PercentPerWhole</c> field of its own: the rework rate's percent comes from <see cref="Dashboard.Shares"
    /// />, so the day the dashboard changes how it cuts a percent the stats strip follows it.</summary>
    [Fact]
    public void TheMeasureDeclaresNoPercentPerWholeFieldOfItsOwn()
    {
        var field = Home.GetField("PercentPerWhole", BindingFlags.NonPublic | BindingFlags.Static);

        Assert.Null(field);
    }
}
