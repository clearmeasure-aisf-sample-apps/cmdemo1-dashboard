using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The two window words of the hourly reports - <c>24 hours</c> and <c>7 days</c> - have one owner each:
/// <see cref="AvailabilityText.DayWindow"/> and <see cref="AvailabilityText.WeekWindow"/>. This test pins both
/// constants by reflection under their own names (the app declares no <c>InternalsVisibleTo</c>), and asserts that
/// <see cref="HourlyCheckText.Title"/> for a report with both windows and <see cref="AvailabilityText.Line"/> for
/// the same windows contain the same words, so the tooltip and the availability line read the same owner.
/// </summary>
public class AvailabilityWindowWordsTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 7, 1, 30, 0, TimeSpan.Zero);
    private static readonly TimeZoneInfo Utc = TimeZoneInfo.Utc;

    /// <summary>Reads an <see cref="AvailabilityText"/> internal const by name; returns null if absent.</summary>
    private static string? WindowWord(string fieldName)
    {
        var field = typeof(AvailabilityText).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(field);
        Assert.True(field!.IsLiteral, $"{fieldName} is a compile-time const.");
        Assert.True(field.IsAssembly, $"{fieldName} is internal, so the box's word can read it.");
        Assert.Null(typeof(AvailabilityText).GetField(fieldName, BindingFlags.Public | BindingFlags.Static));
        return (string)field.GetRawConstantValue()!;
    }

    /// <summary>The day window word is exactly <c>24 hours</c>.</summary>
    [Fact]
    public void TheDayWindowConstIsTwentyFourHours() => Assert.Equal("24 hours", WindowWord("DayWindow"));

    /// <summary>The week window word is exactly <c>7 days</c>.</summary>
    [Fact]
    public void TheWeekWindowConstIsSevenDays() => Assert.Equal("7 days", WindowWord("WeekWindow"));

    /// <summary>
    /// A <see cref="ContainerAppReports"/> with both windows: <see cref="HourlyCheckText.Title"/> contains both
    /// window words, and <see cref="AvailabilityText.Line"/> for the same windows contains the same words.
    /// </summary>
    [Fact]
    public void TheTooltipAndTheLineReadTheSameWindowWords()
    {
        var dayWord = WindowWord("DayWindow")!;
        var weekWord = WindowWord("WeekWindow")!;

        var reports = new ContainerAppReports(
            "scorecard",
            new HourlyCheck(HourlyCheckResult.Healthy, Now),
            new HealthWindow(24, 23),
            new HealthWindow(168, 164));

        var title = HourlyCheckText.Title(reports, Now, Utc);
        var line = AvailabilityText.Line(new HealthReports(new HealthWindow(24, 23), new HealthWindow(168, 164), null), Now);

        Assert.Contains(dayWord, title);
        Assert.Contains(weekWord, title);
        Assert.Contains(dayWord, line);
        Assert.Contains(weekWord, line);
    }
}
