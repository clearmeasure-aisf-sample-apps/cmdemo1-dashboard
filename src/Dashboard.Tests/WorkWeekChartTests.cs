using System.Globalization;
using System.Runtime.CompilerServices;

namespace Dashboard.Tests;

/// <summary>
/// The weekly chart of the work items view: <see cref="WorkWeekChart"/>, its <see cref="WorkWeekBar"/>s and the
/// bot-against-team <see cref="WorkComparison"/>, read from <see cref="WorkItemsText"/> itself rather than through a
/// render, plus the markup <c>WeekChart.razor</c> draws them with. The weeks are fixed dates and fixed counts, so a
/// bar's height, a label and a comparison are asserted as the exact value a reader sees.
/// </summary>
public class WorkWeekChartTests
{
    private static readonly DateTimeOffset Week1 = new(2026, 10, 26, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Week2 = new(2026, 11, 2, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Week3 = new(2026, 11, 9, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Started = new(2026, 11, 9, 8, 0, 0, TimeSpan.Zero);

    private static string WeekChartComponent([CallerFilePath] string here = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "Dashboard", "Components", "WeekChart.razor"));

    private static WorkItemsStats Stats(
        WorkDuration? cycle = null,
        IReadOnlyList<WorkWeek>? weeks = null) =>
        new(new Dictionary<string, int>(), cycle, null, weeks ?? [], null, null);

    private static WorkItemsReport Report(
        WorkItemsStats? bot = null,
        WorkItemsStats? team = null,
        IReadOnlyList<WorkItem>? items = null) =>
        new(null, null, Stats(), items ?? [], [], bot, team);

    private static WorkItem Item(int number, WorkWorker worker, DateTimeOffset doneAt, IReadOnlyList<WorkPullRequest>? pulls = null) =>
        new(
            new WorkItemIdentity(number, $"Item {number}", null, null, WorkColumns.Done),
            [new ColumnVisit(WorkColumns.Review, Started), new ColumnVisit(WorkColumns.Done, doneAt)],
            null,
            null,
            [],
            pulls ?? [],
            null,
            WorkItemEnvironments.None,
            null,
            worker);

    /// <summary>A bar as the page says it: its label, its two counts, and the sentence a reader of the bar hears.</summary>
    private static string Said(WorkWeekBar bar) => $"{bar.Label} | {bar.Merged} | {bar.DeployedProd} | {bar.Title}";

    // ---- Chart: the bars of fixed weeks ----

    [Fact]
    public void FixedWeeksBecomeBarsOfTheirTwoCounts()
    {
        var chart = WorkItemsText.Chart([new WorkWeek(Week1, 6, 2), new WorkWeek(Week2, 0, 0), new WorkWeek(Week3, 3, 4)])!;

        Assert.Equal(
            [
                "Week of 2026-10-26 | 6 | 2 | Week of 2026-10-26: 6 merged, 2 deployed to prod",
                "Week of 2026-11-02 | 0 | 0 | Week of 2026-11-02: 0 merged, 0 deployed to prod",
                "Week of 2026-11-09 | 3 | 4 | Week of 2026-11-09: 3 merged, 4 deployed to prod",
            ],
            chart.Bars.Select(Said));
        Assert.Equal(6, chart.Scale);
    }

    [Fact]
    public void ABarsOwnWordsAndCountsAreTheBarsOwnWordsAndCounts()
    {
        var chart = WorkItemsText.Chart([new WorkWeek(Week1, 6, 2), new WorkWeek(Week2, 0, 0), new WorkWeek(Week3, 3, 4)])!;

        Assert.Equal(new WorkWeekBar("Week of 2026-10-26", 6, 2, "Week of 2026-10-26: 6 merged, 2 deployed to prod"), chart.Bars[0]);
        Assert.Equal(new WorkWeekBar("Week of 2026-11-02", 0, 0, "Week of 2026-11-02: 0 merged, 0 deployed to prod"), chart.Bars[1]);
        Assert.Equal([36d, 0d, 18d], chart.Bars.Select(bar => WorkItemsText.BarHeight(bar.Merged, chart.Scale)));
        Assert.Equal([12d, 0d, 24d], chart.Bars.Select(bar => WorkItemsText.BarHeight(bar.DeployedProd, chart.Scale)));
    }

    [Fact]
    public void TheChartsWordsAreItsOwnSentenceThenEveryBarsSentence()
    {
        var chart = WorkItemsText.Chart([new WorkWeek(Week1, 6, 2), new WorkWeek(Week3, 3, 4)])!;

        Assert.Equal(
            "Merged and deployed to prod per week. "
            + "Week of 2026-10-26: 6 merged, 2 deployed to prod "
            + "Week of 2026-11-09: 3 merged, 4 deployed to prod",
            chart.Title);
    }

    [Fact]
    public void AWeekStartIsNamedInUtcEvenWhenTheFileGaveAnotherOffset() =>
        Assert.Equal(
            "Week of 2026-11-09: 1 merged, 1 deployed to prod",
            WorkItemsText.Chart([new WorkWeek(new DateTimeOffset(2026, 11, 9, 2, 0, 0, TimeSpan.FromHours(2)), 1, 1)])!.Bars[0].Title);

    [Fact]
    public void TheBarsStayInTheOrderTheFileGaveThem()
    {
        var chart = WorkItemsText.Chart([new WorkWeek(Week3, 1, 1), new WorkWeek(Week1, 2, 0)])!;

        Assert.Equal(["Week of 2026-11-09", "Week of 2026-10-26"], chart.Bars.Select(bar => bar.Label));
        Assert.Equal("Week of 2026-10-26: 2 merged, 0 deployed to prod", WorkItemsText.LatestWeek(chart));
    }

    [Fact]
    public void TheScaleIsTheTallestOfBothCountsNotTheFirstCount()
    {
        var chart = WorkItemsText.Chart([new WorkWeek(Week1, 1, 9), new WorkWeek(Week2, 4, 2)])!;

        Assert.Equal(9, chart.Scale);
        Assert.Equal([4d, 16d], chart.Bars.Select(bar => WorkItemsText.BarHeight(bar.Merged, chart.Scale)));
        Assert.Equal([36d, 8d], chart.Bars.Select(bar => WorkItemsText.BarHeight(bar.DeployedProd, chart.Scale)));
    }

    // ---- Chart: weeks that are empty ----

    [Fact]
    public void NoWeekAtAllIsNoChart() => Assert.Null(WorkItemsText.Chart([]));

    [Fact]
    public void AWeekThatGivesNothingIsAWeekOfZerosAndTheWeekHasNoLabel()
    {
        var chart = WorkItemsText.Chart([new WorkWeek(null, null, null)])!;

        Assert.Equal("Week", chart.Bars[0].Label);
        Assert.Equal("Week: 0 merged, 0 deployed to prod", chart.Bars[0].Title);
        Assert.Equal(0, chart.Bars[0].Merged);
        Assert.Equal(0, chart.Bars[0].DeployedProd);
        Assert.Equal(1, chart.Scale);
        Assert.Equal("Merged and deployed to prod per week. Week: 0 merged, 0 deployed to prod", chart.Title);
        Assert.Equal(0d, WorkItemsText.BarHeight(chart.Bars[0].Merged, chart.Scale));
        Assert.Equal(0d, WorkItemsText.BarHeight(chart.Bars[0].DeployedProd, chart.Scale));
    }

    [Fact]
    public void AWeekOfNothingAtAllIsStillAWeekAndHasAScaleOfOne()
    {
        var chart = WorkItemsText.Chart([new WorkWeek(Week2, 0, 0), new WorkWeek(Week3, null, 0)])!;

        Assert.Equal(1, chart.Scale);
        Assert.Equal(
            [
                "Week of 2026-11-02: 0 merged, 0 deployed to prod",
                "Week of 2026-11-09: 0 merged, 0 deployed to prod",
            ],
            chart.Bars.Select(bar => bar.Title));
        Assert.Equal([0d, 0d], chart.Bars.Select(bar => WorkItemsText.BarHeight(bar.Merged, chart.Scale)));
    }

    [Theory]
    [InlineData(0, 6)]
    [InlineData(-5, 6)]
    [InlineData(0, 0)]
    [InlineData(6, 0)]
    [InlineData(6, -3)]
    public void NothingToDrawIsNoBarAtAll(int value, int scale) => Assert.Equal(0d, WorkItemsText.BarHeight(value, scale));

    // ---- BarHeight ----

    [Theory]
    [InlineData(0, 6, 0d)]
    [InlineData(1, 6, 6d)]
    [InlineData(2, 6, 12d)]
    [InlineData(3, 6, 18d)]
    [InlineData(4, 6, 24d)]
    [InlineData(5, 6, 30d)]
    [InlineData(6, 6, 36d)]
    [InlineData(1, 1, 36d)]
    [InlineData(1, 3, 12d)]
    [InlineData(2, 7, 10.29d)]
    [InlineData(5, 7, 25.71d)]
    [InlineData(3, 12, 9d)]
    public void TheTallestBarFillsTheChartAndTheRestAreAFractionOfIt(int value, int scale, double height) =>
        Assert.Equal(height, WorkItemsText.BarHeight(value, scale));

    [Fact]
    public void TheChartIsThirtySixUnitsTallAndItsTallestBarFillsIt() =>
        Assert.Equal(WorkItemsText.ChartHeight, WorkItemsText.BarHeight(9, 9));

    /// <summary>
    /// The issue's own examples, pinned: <c>BarHeight(1, 3)</c> is 12, and <c>BarHeight(1, 7)</c> gives 5.14 - a bar's
    /// height keeps at most two decimals, the same two the shared <see cref="WorkItemsText.PixelFormat" /> writes.
    /// </summary>
    [Fact]
    public void BarHeightKeepsAtMostTwoDecimalsAndTheIssueExamplesHold()
    {
        Assert.Equal(12d, WorkItemsText.BarHeight(1, 3));
        Assert.Equal(5.14d, WorkItemsText.BarHeight(1, 7));

        // For a sweep of shares a bar can be - whole, fractional, and the bottom of an int - the height never
        // carries more than two decimals.
        var scales = new[] { 1, 3, 7, 12, 31, 32, 97, 1000, int.MaxValue };
        foreach (var scale in scales)
        {
            foreach (var value in new[] { 0, 1, 2, 3, 5, 6, 7, 31, 32, int.MaxValue })
            {
                var height = WorkItemsText.BarHeight(value, scale);
                Assert.Equal(Math.Round(height, 2), height);
            }
        }
    }

    /// <summary>
    /// Writing a <c>BarHeight</c> result with the shared <see cref="WorkItemsText.PixelFormat" /> loses no digit: the
    /// string the razor puts in the markup parses back to the very height the bar is drawn at, in every culture the
    /// page could be read in.
    /// </summary>
    [Fact]
    public void ThePixelFormatRoundTripsBarHeightExactlyWithNoLostDigits()
    {
        var samples = new[] { 0, 1, 2, 3, 5, 6, 7, 31, 97 };
        foreach (var scale in samples)
        {
            foreach (var value in samples)
            {
                var height = WorkItemsText.BarHeight(value, scale);
                var text = height.ToString(WorkItemsText.PixelFormat, CultureInfo.InvariantCulture);
                Assert.Equal(height, double.Parse(text, CultureInfo.InvariantCulture));
            }
        }

        // The two decimals the rounding keeps are exactly the two the format writes: 5.14 stays 5.14, not 5.1.
        Assert.Equal("5.14", WorkItemsText.BarHeight(1, 7).ToString(WorkItemsText.PixelFormat, CultureInfo.InvariantCulture));
        Assert.Equal("12", WorkItemsText.BarHeight(1, 3).ToString(WorkItemsText.PixelFormat, CultureInfo.InvariantCulture));
    }

    // ---- LatestWeek ----

    [Fact]
    public void TheNewestWeekIsTheLastBarOfTheFile()
    {
        var chart = WorkItemsText.Chart([new WorkWeek(Week1, 6, 2), new WorkWeek(Week3, 3, 4)])!;

        Assert.Equal("Week of 2026-11-09: 3 merged, 4 deployed to prod", WorkItemsText.LatestWeek(chart));
    }

    // ---- MergedAndDeployed helper (through the public facade) ----

    /// <summary>
    /// The bar Title sentence for plain counts: the helper's wording as the public facade says it, invariant culture.
    /// </summary>
    [Fact]
    public void TheBarTitleSaysTheSentenceForPlainCounts()
    {
        var chart = WorkItemsText.Chart([new WorkWeek(Week1, 3, 2)])!;

        Assert.Equal("Week of 2026-10-26: 3 merged, 2 deployed to prod", chart.Bars[0].Title);
    }

    /// <summary>
    /// The bar Title sentence for an empty who: a week with no start and zero counts says "Week" as the who.
    /// </summary>
    [Fact]
    public void TheBarTitleSaysTheSentenceForAnEmptyWeekLabel()
    {
        var chart = WorkItemsText.Chart([new WorkWeek(null, 1, 0)])!;

        Assert.Equal("Week: 1 merged, 0 deployed to prod", chart.Bars[0].Title);
    }

    /// <summary>
    /// Under fi-FI a negative count is written with the culture's minus sign (U+2212) in the current-culture
    /// call sites (LatestWeek, ThroughputLine) but with the ASCII hyphen (U+002D) in the invariant-culture bar Title.
    /// This characterizes today's split so a follow-up can change it on purpose.
    /// </summary>
    [Fact]
    public void UnderFiFiANegativeCountUsesTheCulturesMinusSignInLatestWeekButAnAsciiHyphenInTheBarTitle()
    {
        const string FiMinus = "\u2212";
        var chart = WorkItemsText.Chart([new WorkWeek(Week1, -1, int.MinValue)])!;

        // The bar Title is built with the invariant culture: ASCII hyphen.
        var barTitle = chart.Bars[0].Title;
        Assert.Contains("-1 merged", barTitle, StringComparison.Ordinal);
        Assert.Contains("-2147483648 deployed", barTitle, StringComparison.Ordinal);
        Assert.DoesNotContain(FiMinus, barTitle, StringComparison.Ordinal);

        // LatestWeek is built with the current culture: fi-FI's U+2212.
        var latest = CultureScope.Says("fi-FI", () => WorkItemsText.LatestWeek(chart));
        Assert.Contains(FiMinus + "1 merged", latest, StringComparison.Ordinal);
        Assert.Contains(FiMinus + "2147483648 deployed", latest, StringComparison.Ordinal);
        Assert.DoesNotContain("-1 merged", latest, StringComparison.Ordinal);
    }

    // ---- Comparison ----

    [Fact]
    public void AFileThatSaysNothingAboutWhoWorkedHasNoComparison() =>
        Assert.Null(WorkItemsText.Comparison(Report(items: [Item(1, WorkWorker.Unknown, Started.AddHours(8))])));

    [Fact]
    public void ASplitThatHoldsNoNumbersIsNoComparison() => Assert.Null(WorkItemsText.Comparison(Report(bot: Stats())));

    [Fact]
    public void TheBotCycleAgainstTheTeamCycleIsTwelveHoursUp()
    {
        var comparison = WorkItemsText.Comparison(
            Report(bot: Stats(new WorkDuration(20, 46, 5)), team: Stats(new WorkDuration(8, 12, 3))))!;

        Assert.Equal(new WorkDuration(20, 46, 5), comparison.BotCycle);
        Assert.Equal(new WorkDuration(8, 12, 3), comparison.TeamCycle);
        Assert.Equal(12d, comparison.BotCycle!.Median!.Value - comparison.TeamCycle!.Median!.Value);
        Assert.Equal("median 20 h · p85 46 h · 5 items", WorkItemsText.Duration(comparison.BotCycle));
        Assert.Equal("median 8 h · p85 12 h · 3 items", WorkItemsText.Duration(comparison.TeamCycle));
        Assert.Null(comparison.BotWeeks);
        Assert.Null(comparison.TeamWeeks);
    }

    [Fact]
    public void TheBotCycleAgainstTheTeamCycleIsEightHoursDown()
    {
        var comparison = WorkItemsText.Comparison(
            Report(bot: Stats(new WorkDuration(6, 9, 4)), team: Stats(new WorkDuration(14, 30, 6))))!;

        Assert.Equal(-8d, comparison.BotCycle!.Median!.Value - comparison.TeamCycle!.Median!.Value);
        Assert.Equal("median 6 h · p85 9 h · 4 items", WorkItemsText.Duration(comparison.BotCycle));
        Assert.Equal("median 14 h · p85 30 h · 6 items", WorkItemsText.Duration(comparison.TeamCycle));
    }

    [Fact]
    public void TheBotCycleAgainstTheTeamCycleIsUnchanged()
    {
        var comparison = WorkItemsText.Comparison(
            Report(bot: Stats(new WorkDuration(9, 15, 4)), team: Stats(new WorkDuration(9, 15, 4))))!;

        Assert.Equal(comparison.TeamCycle, comparison.BotCycle);
        Assert.Equal(0d, comparison.BotCycle!.Median!.Value - comparison.TeamCycle!.Median!.Value);
        Assert.Equal(WorkItemsText.Duration(comparison.TeamCycle), WorkItemsText.Duration(comparison.BotCycle));
        Assert.Equal("median 9 h · p85 15 h · 4 items", WorkItemsText.Duration(comparison.BotCycle));
    }

    [Fact]
    public void EachSideKeepsItsOwnWeeks()
    {
        var comparison = WorkItemsText.Comparison(
            Report(
                bot: Stats(new WorkDuration(20, null, null), [new WorkWeek(Week2, 1, 0)]),
                team: Stats(null, [new WorkWeek(Week2, 0, 0), new WorkWeek(Week3, 4, 5)])))!;

        Assert.Equal(["Week of 2026-11-02 | 1 | 0 | Week of 2026-11-02: 1 merged, 0 deployed to prod"], comparison.BotWeeks!.Bars.Select(Said));
        Assert.Equal(1, comparison.BotWeeks!.Scale);
        Assert.Equal(5, comparison.TeamWeeks!.Scale);
        Assert.Equal(
            [
                "Week of 2026-11-02 | 0 | 0 | Week of 2026-11-02: 0 merged, 0 deployed to prod",
                "Week of 2026-11-09 | 4 | 5 | Week of 2026-11-09: 4 merged, 5 deployed to prod",
            ],
            comparison.TeamWeeks!.Bars.Select(Said));
        Assert.Equal("Week of 2026-11-09: 4 merged, 5 deployed to prod", WorkItemsText.LatestWeek(comparison.TeamWeeks!));
        Assert.Equal("n/a", WorkItemsText.Duration(comparison.TeamCycle));
    }

    [Fact]
    public void TheSideTheFileDoesNotSplitOutMayHaveNothingToShow()
    {
        var comparison = WorkItemsText.Comparison(Report(team: Stats(null, [new WorkWeek(Week1, 2, 1)])))!;

        Assert.Null(comparison.BotWeeks);
        Assert.Equal("n/a", WorkItemsText.Duration(comparison.BotCycle));
        Assert.Equal(["Week of 2026-10-26 | 2 | 1 | Week of 2026-10-26: 2 merged, 1 deployed to prod"], comparison.TeamWeeks!.Bars.Select(Said));
    }

    [Fact]
    public void WithoutAPublishedSplitTheComparisonIsCountedFromTheItems()
    {
        var comparison = WorkItemsText.Comparison(
            Report(items:
            [
                Item(1, WorkWorker.Bot, Started.AddHours(8)),
                Item(2, WorkWorker.Team, Started.AddHours(20)),
            ]))!;

        Assert.Equal(new WorkDuration(8, 8, 1), comparison.BotCycle);
        Assert.Equal(new WorkDuration(20, 20, 1), comparison.TeamCycle);
        Assert.Equal(12d, comparison.TeamCycle!.Median!.Value - comparison.BotCycle!.Median!.Value);
        Assert.Equal("median 8 h · p85 8 h · 1 item", WorkItemsText.Duration(comparison.BotCycle));
        Assert.Null(comparison.BotWeeks);
        Assert.Null(comparison.TeamWeeks);
    }

    [Fact]
    public void TheWeeksOfASplitTheFileDoesNotGiveAreCountedFromMerges()
    {
        var merged = new DateTimeOffset(2026, 11, 4, 10, 0, 0, TimeSpan.Zero);
        var comparison = WorkItemsText.Comparison(
            Report(items: [Item(3, WorkWorker.Bot, Started.AddHours(8), [new WorkPullRequest(11, null, "merged", merged, null)])]))!;

        Assert.Equal(["Week of 2026-11-02 | 1 | 0 | Week of 2026-11-02: 1 merged, 0 deployed to prod"], comparison.BotWeeks!.Bars.Select(Said));
        Assert.Equal(1, comparison.BotWeeks!.Scale);
        Assert.Equal(36d, WorkItemsText.BarHeight(comparison.BotWeeks!.Bars[0].Merged, comparison.BotWeeks!.Scale));
        Assert.Null(comparison.TeamWeeks);
    }

    [Fact]
    public void AComparisonOfEqualSidesIsTheSameComparisonTwice()
    {
        var report = Report(bot: Stats(new WorkDuration(9, 15, 4)), team: Stats(new WorkDuration(9, 15, 4)));
        var comparison = WorkItemsText.Comparison(report);

        Assert.NotNull(comparison);
        Assert.Equal(comparison, WorkItemsText.Comparison(report));
    }

    // ---- Null arguments ----

    [Fact]
    public void NoWeeksAndNoReportAndNoChartAreRefusedByName()
    {
        Assert.Equal("weeks", Assert.Throws<ArgumentNullException>(() => WorkItemsText.Chart(null!)).ParamName);
        Assert.Equal("report", Assert.Throws<ArgumentNullException>(() => WorkItemsText.Comparison(null!)).ParamName);
        Assert.Equal("chart", Assert.Throws<ArgumentNullException>(() => WorkItemsText.LatestWeek(null!)).ParamName);
    }

    // ---- What WeekChart.razor draws ----

    [Fact]
    public void TheChartIsNamedByItsOwnSentenceAndEveryBarRepeatsIt()
    {
        var text = File.ReadAllText(WeekChartComponent());

        Assert.Contains(
            """<svg class="weeks" viewBox="0 0 @Pixel(width) @Pixel(WorkItemsText.ChartHeight)" width="@Pixel(width * DrawnWidthScale)" height="@Pixel(WorkItemsText.ChartHeight * DrawnHeightScale)" role="img" aria-label="@Chart.Title" focusable="false">""",
            text,
            StringComparison.Ordinal);
        Assert.Contains("var width = Chart.Bars.Count * BarPitch;", text, StringComparison.Ordinal);
        Assert.Contains("<title>@Chart.Title</title>", text, StringComparison.Ordinal);
        Assert.Contains("<title>@bar.Title</title>", text, StringComparison.Ordinal);
    }

    [Fact]
    public void EveryBarIsDrawnFromBarHeightAndHangsFromTheTopOfTheChart()
    {
        var text = File.ReadAllText(WeekChartComponent());

        Assert.Contains("var merged = WorkItemsText.BarHeight(bar.Merged, Chart.Scale);", text, StringComparison.Ordinal);
        Assert.Contains("var deployed = WorkItemsText.BarHeight(bar.DeployedProd, Chart.Scale);", text, StringComparison.Ordinal);
        Assert.Contains(
            """<rect class="weeks__merged" x="@Pixel(x)" y="@Pixel(WorkItemsText.ChartHeight - merged)" width="@BarWidth" height="@Pixel(merged)">""",
            text,
            StringComparison.Ordinal);
        Assert.Contains(
            """<rect class="weeks__deployed" x="@Pixel(x + DeployedOffset)" y="@Pixel(WorkItemsText.ChartHeight - deployed)" width="@BarWidth" height="@Pixel(deployed)">""",
            text,
            StringComparison.Ordinal);
        Assert.Contains("var x = index * BarPitch;", text, StringComparison.Ordinal);
    }

    /// <summary>
    /// The bar geometry of the chart is named, not written as bare numbers: the pitch of a week's slot, the width of
    /// each bar, the offset of the deployed bar from its merged bar, and the scales the drawn size takes against the
    /// viewBox. The numbers themselves are the ones a reader of the chart has always seen.
    /// </summary>
    [Fact]
    public void TheBarsGeometryIsNamedAndItsNumbersAreTheOnesTheChartHasAlwaysDrawn()
    {
        var text = File.ReadAllText(WeekChartComponent());

        Assert.Contains("private const int BarPitch = 14;", text, StringComparison.Ordinal);
        Assert.Contains("private const int BarWidth = 5;", text, StringComparison.Ordinal);
        Assert.Contains("private const int DeployedOffset = 6;", text, StringComparison.Ordinal);
        Assert.Contains("private const int DrawnWidthScale = 3;", text, StringComparison.Ordinal);
        Assert.Contains("private const double DrawnHeightScale = 1.4;", text, StringComparison.Ordinal);
        // No bare number is left where a name belongs: the pitch, the offset and the two scales are not written inline.
        Assert.DoesNotContain("Bars.Count * 14", text, StringComparison.Ordinal);
        Assert.DoesNotContain("index * 14", text, StringComparison.Ordinal);
        Assert.DoesNotContain("x + 6", text, StringComparison.Ordinal);
        Assert.DoesNotContain("width * 3", text, StringComparison.Ordinal);
        Assert.DoesNotContain("ChartHeight * 1.4", text, StringComparison.Ordinal);
    }

    /// <summary>
    /// The chart writes every bar coordinate with the shared <see cref="WorkItemsText.PixelFormat" />, not a format it
    /// spells for itself, and that format keeps exactly the decimals <c>BarHeight</c> rounds to, so a bar's number and
    /// its drawn height agree.
    /// </summary>
    [Fact]
    public void ThePixelFormatIsSharedAndKeepsTheDecimalsBarHeightRoundsTo()
    {
        var text = File.ReadAllText(WeekChartComponent());

        Assert.Contains("value.ToString(WorkItemsText.PixelFormat, CultureInfo.InvariantCulture)", text, StringComparison.Ordinal);
        Assert.DoesNotContain("0.##", text, StringComparison.Ordinal);
        Assert.Equal("0.##", WorkItemsText.PixelFormat);

        // The format keeps exactly two places: two decimals are written, and a whole number is not padded.
        Assert.Equal("5.14", 5.14.ToString(WorkItemsText.PixelFormat, CultureInfo.InvariantCulture));
        Assert.Equal("12", 12d.ToString(WorkItemsText.PixelFormat, CultureInfo.InvariantCulture));
        Assert.Equal("0", 0d.ToString(WorkItemsText.PixelFormat, CultureInfo.InvariantCulture));
    }
}
