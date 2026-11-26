namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="ContainerAppReports" /> and what the page says of it
/// in <see cref="HourlyCheckText" />: the one entry the pipeline's hourly health reports keep for a container app -
/// the newest result of them and the two windows they count - and the words the box and its tooltip make of it.
/// <para>
/// <see cref="AvailabilityTests" /> pins the reading of the file - what an entry of <c>containerApps</c> is read as,
/// what a result is that is not one of the three words, what a window is that does not count - and the words of a
/// known report: the line at 27 minutes, at the very two hours and one second past them, the counts of a day and a
/// week and of a window that counted nothing. <see cref="HealthHelperCoverageTests" /> pins the tooltip of a known
/// report to the letter - the sentence of a stale result, the one the two hours are still said at, the app no
/// report asked, the week counted without a day, and the zone the caller has not got - and
/// <see cref="RuntimePayloadBuilderTests" /> pins the same words as the box says them: the line of the three words
/// and of a result two hours and one second old, the day counted without a week, and the two hours counted from
/// the moment asked and not from the file.
/// </para>
/// <para>
/// Left open by all of that, and pinned here: that a report is its four parts and nothing else - the name it keeps
/// as it was given, the empty name and the name of one letter among them, the check and the two windows it holds
/// the very instances it was given and falls apart into again, the two reports that are one while their parts are
/// and the difference in any one of them, a case in the name among them, and the copy a <c>with</c> makes; the three
/// words a result is told in, as they are written in the file, and the moment a check stands on, the ends of the
/// clock among them and the moment spelled in another viewer's hours; the newest result a report may still be said
/// to have - no reports and no last check, and a result that ends ahead of the clock, an hour ahead to one second
/// ahead, while the two hours it is said up to are <see cref="AvailabilityTests" />'; the line and the tooltip in
/// the hours the moment is written in, and the tooltip in the combinations none of those three files say: the
/// report found unhealthy with only the day counted, the report that ends at the very moment asked, the stale
/// result with only the day counted, and the history whose newest report never came while both windows counted.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: every moment is one the test
/// brought, in the hours the test chose.
/// </para>
/// </summary>
public class ContainerAppReportsEdgeCaseTests
{
    /// <summary>The moment the box is asked, and every "ago" below is counted from: a fixed moment, not the clock.</summary>
    private static readonly DateTimeOffset Now = new(2026, 10, 7, 1, 30, 0, TimeSpan.Zero);

    /// <summary>The moment the newest report below ended, forty minutes before the box was asked.</summary>
    private static readonly DateTimeOffset Checked = new(2026, 10, 7, 0, 50, 0, TimeSpan.Zero);

    private static readonly TimeZoneInfo Utc = TimeZoneInfo.Utc;

    private static readonly TimeZoneInfo EastFiveThirty =
        TimeZoneInfo.CreateCustomTimeZone("east-five-thirty", TimeSpan.FromMinutes(330), "East Five Thirty", "East Five Thirty");

    /// <summary>A report of one app, the parts it is told, the windows left out the way the file leaves them out.</summary>
    private static ContainerAppReports Reports(string name, HourlyCheck? last, HealthWindow? day = null, HealthWindow? week = null) =>
        new(name, last, day, week);

    // ---------------------------------------------------------------- the report: its four parts

    /// <summary>
    /// A report is its four parts and nothing else: the name it keeps as the file writes it, and the check and the
    /// two windows it holds the very instances it was given - the report the page reads and the words it says of it
    /// standing on the same facts, not on copies of them.
    /// </summary>
    [Fact]
    public void TheReportKeepsTheNameTheLastCheckAndTheTwoWindows()
    {
        var last = new HourlyCheck(HourlyCheckResult.Unreachable, Checked);
        var day = new HealthWindow(24, 23);
        var week = new HealthWindow(168, 160);
        var reports = new ContainerAppReports("ui", last, day, week);

        Assert.Equal("ui", reports.Name);
        Assert.Same(last, reports.Last);
        Assert.Same(day, reports.Last24Hours);
        Assert.Same(week, reports.Last7Days);
    }

    /// <summary>
    /// The name the file does not write and the windows it does not hold are kept as they are: the empty name is no
    /// name but it is still the name the report stands on, a name of one letter is one letter, the newest result is
    /// no result, and the two windows are not counted - and the report falls apart into the four parts again, the
    /// absent parts falling apart as absent.
    /// </summary>
    [Fact]
    public void TheNameTheFileDoesNotWriteAndTheWindowsItDoesNotHoldAreKeptAsKept()
    {
        var reports = new ContainerAppReports(string.Empty, null, null, null);

        Assert.Equal(string.Empty, reports.Name);
        Assert.Null(reports.Last);
        Assert.Null(reports.Last24Hours);
        Assert.Null(reports.Last7Days);
        Assert.Equal(" ", new ContainerAppReports(" ", null, null, null).Name);

        var (name, last, day, week) = reports;

        Assert.Equal(string.Empty, name);
        Assert.Null(last);
        Assert.Null(day);
        Assert.Null(week);
    }

    /// <summary>
    /// Two reports are one while their four parts are - each a part's own equality, the name with the case the file
    /// writes it - and a difference in any one of them is another report: a name of another case, the word a check
    /// found, the moment it ended, either window, or a part the other does not hold. The hash code follows the
    /// equality, and a word is no report's equal.
    /// </summary>
    [Fact]
    public void TwoReportsAreOneWhileTheirPartsAre()
    {
        var one = new ContainerAppReports("ui", new HourlyCheck(HourlyCheckResult.Healthy, Checked), new HealthWindow(24, 23), new HealthWindow(168, 160));
        var two = new ContainerAppReports("ui", new HourlyCheck(HourlyCheckResult.Healthy, Checked), new HealthWindow(24, 23), new HealthWindow(168, 160));

        Assert.Equal(one, two);
        Assert.True(one == two);
        Assert.True(two == one);
        Assert.False(one != two);
        Assert.True(one.Equals((object)two));
        Assert.Equal(one.GetHashCode(), two.GetHashCode());
        Assert.False(one.Equals(null));
        Assert.False(one.Equals("ui"));

        Assert.NotEqual(one, one with { Name = "UI" });
        Assert.NotEqual(one, one with { Name = "worker" });
        Assert.NotEqual(one, one with { Last = new HourlyCheck(HourlyCheckResult.Unhealthy, Checked) });
        Assert.NotEqual(one, one with { Last = new HourlyCheck(HourlyCheckResult.Healthy, Checked.AddSeconds(1)) });
        Assert.NotEqual(one, one with { Last = null });
        Assert.NotEqual(one, one with { Last24Hours = new HealthWindow(24, 22) });
        Assert.NotEqual(one, one with { Last24Hours = null });
        Assert.NotEqual(one, one with { Last7Days = new HealthWindow(168, 159) });
        Assert.NotEqual(one, one with { Last7Days = null });
        Assert.NotEqual(one, new ContainerAppReports("ui", null, new HealthWindow(24, 23), new HealthWindow(168, 160)));
    }

    /// <summary>
    /// A copy of a report is the report again: the <c>with</c> that names no part and the <c>with</c> that names the
    /// part it turns keep the rest, and not the same reference. A part turned to no part keeps the other three.
    /// </summary>
    [Fact]
    public void ACopyOfAReportIsTheReportAgain()
    {
        var reports = new ContainerAppReports("ui", new HourlyCheck(HourlyCheckResult.Healthy, Checked), new HealthWindow(24, 23), new HealthWindow(168, 160));

        var copy = reports with { };
        Assert.Equal(reports, copy);
        Assert.NotSame(reports, copy);

        var weekless = reports with { Last7Days = null };
        Assert.Equal("ui", weekless.Name);
        Assert.Equal(reports.Last, weekless.Last);
        Assert.Equal(new HealthWindow(24, 23), weekless.Last24Hours);
        Assert.Null(weekless.Last7Days);
        Assert.NotEqual(reports, weekless);

        var repack = reports with { Name = reports.Name, Last = reports.Last, Last24Hours = reports.Last24Hours, Last7Days = reports.Last7Days };
        Assert.Equal(reports, repack);
        Assert.NotSame(reports, repack);
    }

    /// <summary>
    /// Reports that are not there are no reports: the record's default is no reports, and reports are never equal to
    /// no reports however the question is asked - the operators and <c>Equals</c> asked about the no reports - while
    /// the reports that are there are reports to both of them.
    /// </summary>
    [Fact]
    public void ReportsThatAreNotThereAreNoReports()
    {
        Assert.Null(default(ContainerAppReports));

        ContainerAppReports? none = null;
        var there = new ContainerAppReports("ui", null, null, null);

        Assert.False(none == there);
        Assert.True(none != there);
        Assert.False(there.Equals(none));
        Assert.False(there == none);
        Assert.True(there != none);
        Assert.True(there is not null);
    }

    // ---------------------------------------------------------------- the check: its word and its moment

    /// <summary>
    /// The three words a result is told in, as the file writes them, in lower case, and three distinct words: a
    /// fourth word is no result the page reads, which <see cref="AvailabilityTests" /> pins read from the file. The
    /// words are the <see cref="HourlyCheckResult" /> instances the closed vocabulary holds, and their <see
    /// cref="HourlyCheckResult.Word" /> is the lowercase spelling the runbook writes.
    /// </summary>
    [Fact]
    public void TheThreeWordsOfAResultAreTheThreeWordsTheFileWritesThemIn()
    {
        Assert.Equal("healthy", HourlyCheckResult.Healthy.Word);
        Assert.Equal("unhealthy", HourlyCheckResult.Unhealthy.Word);
        Assert.Equal("unreachable", HourlyCheckResult.Unreachable.Word);

        Assert.Equal(3, new[] { HourlyCheckResult.Healthy, HourlyCheckResult.Unhealthy, HourlyCheckResult.Unreachable }.Distinct().Count());
    }

    /// <summary>
    /// A check keeps its word and its moment: the moment it stands on, the ends of the clock among them, and the
    /// same instant spelled in another viewer's hours - a check of it is the check, since it is the instant a result
    /// is counted from and not the hours it is written in. A word turned or a moment moved is another check, and a
    /// copy of a check is the check again.
    /// </summary>
    [Fact]
    public void ACheckKeepsItsWordAndItsMomentInTheInstantItStandsOn()
    {
        var check = new HourlyCheck(HourlyCheckResult.Unhealthy, Checked);

        Assert.Equal(HourlyCheckResult.Unhealthy, check.Result);
        Assert.Equal(Checked, check.At);
        Assert.Equal(check, new HourlyCheck(HourlyCheckResult.Unhealthy, Checked));
        Assert.Equal(check, check with { At = Checked.ToOffset(TimeSpan.FromMinutes(330)) });
        Assert.Equal(DateTimeOffset.MinValue, new HourlyCheck(HourlyCheckResult.Healthy, DateTimeOffset.MinValue).At);
        Assert.Equal(DateTimeOffset.MaxValue, new HourlyCheck(HourlyCheckResult.Healthy, DateTimeOffset.MaxValue).At);

        Assert.NotEqual(check, new HourlyCheck(HourlyCheckResult.Healthy, Checked));
        Assert.NotEqual(check, new HourlyCheck(HourlyCheckResult.Unreachable, Checked));
        Assert.NotEqual(check, new HourlyCheck(HourlyCheckResult.Unhealthy, Checked.AddSeconds(1)));

        var copy = check with { };
        Assert.Equal(check, copy);
        Assert.NotSame(check, copy);
        Assert.Equal(check.GetHashCode(), copy.GetHashCode());
        Assert.False(check.Equals(null));
    }

    // ---------------------------------------------------------------- the newest result a report may still be said to have

    /// <summary>
    /// No reports at all, and a history whose newest report never came, are no newest result - with the windows
    /// counted and with neither counted alike, since it is the report that is looked for and not the count.
    /// </summary>
    [Fact]
    public void NoReportsAndNoLastCheckAreNoRecentResult()
    {
        Assert.Null(HourlyCheckText.RecentResult(null, Now));
        Assert.Null(HourlyCheckText.RecentResult(Reports("ui", null), Now));
        Assert.Null(HourlyCheckText.RecentResult(Reports("ui", null, new HealthWindow(24, 23), new HealthWindow(168, 160)), Now));
    }

    /// <summary>
    /// The newest result is said of the newest report while its moment stands within the two hours the runbook needs
    /// to catch up - and the two hours are counted from the moment asked, so a report that ends ahead of it is
    /// within them still: an hour ahead to one second ahead, the newest report of the box and "just now" at once, up
    /// to one second short of the two hours. The other side of the two hours, the very tick and one second past it,
    /// stands in <see cref="AvailabilityTests" /> and <see cref="RuntimePayloadBuilderTests" />.
    /// </summary>
    [Theory]
    [InlineData(3600, "just now")]
    [InlineData(1, "just now")]
    [InlineData(0, "just now")]
    [InlineData(-1, "just now")]
    [InlineData(-59, "just now")]
    [InlineData(-3599, "59 min ago")]
    public void TheNewestResultIsSaidWhileItsMomentStandsWithinTheTwoHours(int secondsFromNow, string ago)
    {
        var last = new HourlyCheck(HourlyCheckResult.Healthy, Now.AddSeconds(secondsFromNow));
        var reports = Reports("ui", last);

        Assert.Equal(last, HourlyCheckText.RecentResult(reports, Now));
        Assert.Equal($"hourly: healthy, {ago}", HourlyCheckText.Result(reports, Now));
    }

    /// <summary>
    /// The line says the same in the hours the moment is written in: the report ended twenty-seven minutes before
    /// the box was asked, and the moment stands on the same instant spelled in the hours five and a half east of the
    /// meridian - the line the box shows, and not a line of the viewer's own clock.
    /// </summary>
    [Fact]
    public void TheLineIsTheSameInTheHoursTheMomentIsWrittenIn()
    {
        var utc = Reports("ui", new HourlyCheck(HourlyCheckResult.Unhealthy, Now.AddMinutes(-27)));
        var spelled = Reports("ui", new HourlyCheck(HourlyCheckResult.Unhealthy, Now.AddMinutes(-27).ToOffset(TimeSpan.FromMinutes(330))));

        Assert.Equal("hourly: unhealthy, 27 min ago", HourlyCheckText.Result(utc, Now));
        Assert.Equal("hourly: unhealthy, 27 min ago", HourlyCheckText.Result(spelled, Now));
    }

    // ---------------------------------------------------------------- the counts, alone

    /// <summary>
    /// A window the file counts is said for itself where the other is not counted: the week alone, the day alone,
    /// and a window that counted no report said as <c>none</c> for itself - the day and the week together, and both
    /// left out, standing in <see cref="AvailabilityTests" />.
    /// </summary>
    [Fact]
    public void AWindowCountedWithoutTheOtherIsSaidAlone()
    {
        Assert.Equal("7 d: 160 of 168", HourlyCheckText.Counts(Reports("ui", null, null, new HealthWindow(168, 160))));
        Assert.Equal("7 d: none", HourlyCheckText.Counts(Reports("ui", null, null, new HealthWindow(0, 0))));
        Assert.Equal("24 h: none", HourlyCheckText.Counts(Reports("ui", null, new HealthWindow(0, 0), null)));
    }

    // ---------------------------------------------------------------- the tooltip, in the words it says them

    /// <summary>
    /// The tooltip of a report found unhealthy, with only the day counted: the sentence of what it found, the count
    /// of the reports of the day that asked it, and the sentence no tooltip leaves out - the week the file does not
    /// hold writing no sentence, as it writes no number in the line.
    /// </summary>
    [Fact]
    public void TheTooltipOfAReportFoundUnhealthyCountsTheDayItCounts()
    {
        var reports = Reports("ui", new HourlyCheck(HourlyCheckResult.Unhealthy, Checked), new HealthWindow(24, 23));

        Assert.Equal(
            "Once an hour the pipeline's runbook \"Health report\" in Octopus Deploy asks the app whether it answers, "
            + "also while no page is open: the history, under the page's own probe. "
            + "The last report that asked it ended 2026-10-07 00:50 +00:00 (40 min ago) and found it unhealthy. "
            + "Healthy in 23 of the 24 reports of 24 hours that asked it. "
            + "Hourly checks by the pipeline, not continuous monitoring.",
            HourlyCheckText.Title(reports, Now, Utc));
    }

    /// <summary>
    /// The report that ends at the very moment the box is asked, in the hours both the file and the viewer write
    /// it in: "just now" with both windows counted, and the same instant the tooltip spells in the hours five and a
    /// half east of the meridian the viewer is in - the moment the tooltip names and the moment the line says of it
    /// being one moment, whichever hours it stands in.
    /// </summary>
    [Fact]
    public void TheTooltipOfAReportThatEndsNowIsJustNowInTheHoursItIsWrittenIn()
    {
        var reports = Reports("ui", new HourlyCheck(HourlyCheckResult.Healthy, Now), new HealthWindow(24, 24), new HealthWindow(168, 168));

        Assert.Equal(
            "Once an hour the pipeline's runbook \"Health report\" in Octopus Deploy asks the app whether it answers, "
            + "also while no page is open: the history, under the page's own probe. "
            + "The last report that asked it ended 2026-10-07 01:30 +00:00 (just now) and found it healthy. "
            + "Healthy in 24 of the 24 reports of 24 hours that asked it. "
            + "Healthy in 168 of the 168 reports of 7 days that asked it. "
            + "Hourly checks by the pipeline, not continuous monitoring.",
            HourlyCheckText.Title(reports, Now, Utc));

        Assert.Contains(
            "The last report that asked it ended 2026-10-07 07:00 +05:30 (just now) and found it healthy.",
            HourlyCheckText.Title(reports, Now, EastFiveThirty),
            StringComparison.Ordinal);
    }

    /// <summary>
    /// The stale result with only the day counted: the sentence that says the box has stopped talking about the
    /// app, in the very words the box writes, the count of the reports of the day that asked it, and the sentence no
    /// tooltip leaves out - the week the file does not hold writing no sentence of its own.
    /// </summary>
    [Fact]
    public void TheTooltipOfAStaleResultCountsTheDayItCounts()
    {
        var reports = Reports("ui", new HourlyCheck(HourlyCheckResult.Unreachable, Now.AddHours(-3)), new HealthWindow(12, 12));

        Assert.Equal(
            "Once an hour the pipeline's runbook \"Health report\" in Octopus Deploy asks the app whether it answers, "
            + "also while no page is open: the history, under the page's own probe. "
            + "The last report that asked it ended 2026-10-06 22:30 +00:00 (3 h ago) and found it unreachable: "
            + "none in the last two hours, so the box says no recent hourly check. "
            + "Healthy in 12 of the 12 reports of 24 hours that asked it. "
            + "Hourly checks by the pipeline, not continuous monitoring.",
            HourlyCheckText.Title(reports, Now, Utc));
    }

    /// <summary>
    /// The history whose newest report never came, while both windows counted: the app was asked by the runbook and
    /// answered in the two hours and the seven days, and no report of it is the newest - said apart from the app no
    /// report asked yet, which <see cref="HealthHelperCoverageTests" /> pins.
    /// </summary>
    [Fact]
    public void TheTooltipOfAHistoryWithoutANewestReportCountsBothWindowsItCounts()
    {
        var reports = Reports("ui", null, new HealthWindow(24, 23), new HealthWindow(168, 160));

        Assert.Equal(
            "Once an hour the pipeline's runbook \"Health report\" in Octopus Deploy asks the app whether it answers, "
            + "also while no page is open: the history, under the page's own probe. "
            + "No report of the last 7 days asked it. "
            + "Healthy in 23 of the 24 reports of 24 hours that asked it. "
            + "Healthy in 160 of the 168 reports of 7 days that asked it. "
            + "Hourly checks by the pipeline, not continuous monitoring.",
            HourlyCheckText.Title(reports, Now, Utc));
    }
}
