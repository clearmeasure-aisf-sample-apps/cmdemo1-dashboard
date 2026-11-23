namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="HourlyCheckText" />, the words the box and its
/// tooltip make of a container app's history of hourly health reports: the newest result it may still be said to
/// have, the line <c>hourly: healthy, 40 min ago</c>, the counts <c>24 h: 23 of 24 · 7 d: 160 of 168</c>, and the
/// tooltip under the line.
/// <para>
/// The fixed values the class stands on are pinned where the box says them: <see cref="AvailabilityTests" /> says
/// that <see cref="HourlyCheckText.Recent" /> is the two hours the runbook needs to catch up and that
/// <see cref="HourlyCheckText.NoRecentCheck" /> is the words the box writes without a result of them, and it,
/// <see cref="RuntimePayloadBuilderTests" /> and <see cref="ContainerAppReportsEdgeCaseTests" /> pin the line and
/// the counts at 27 minutes, at the very two hours and one second past them, for the word <c>unhealthy</c> and
/// <c>unreachable</c>, and for the windows counted alone and left out; <see cref="HealthHelperCoverageTests" />
/// pins the tooltip of a known report to the letter, the sentence of a stale result, the report the two hours are
/// still said at, the app no report asked, the week counted without a day, and the zone the caller has not got.
/// </para>
/// <para>
/// Left open by all of that, and pinned here: the line in the minutes no line says - the minute the moment is
/// counted in, from the sixtyth second to the one hundred and twentieth - and in the newest result's own two
/// hours counted to the tick, one second short of them to one second past, while the word is <c>healthy</c> and
/// the newest result is asked for itself and not read out of a line; the line and the tooltip in the word the check
/// holds rather than in one of the three it is told in - the other case, the empty word and the word of spaces, and
/// no word at all, which the record the reading never makes keeps as it is given; the counts where the day counted
/// nothing and the week counted, and the tooltip where a window the file counts counted no report, where neither
/// window is counted at all - the report said in the last two hours and the one that is not - and the stale result
/// in the hours the viewer is in.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: every moment is one the test
/// brought, in the hours the test chose.
/// </para>
/// </summary>
public class HourlyCheckTextEdgeCaseTests
{
    /// <summary>The moment the box is asked, and every "ago" below is counted from: a fixed moment, not the clock.</summary>
    private static readonly DateTimeOffset Now = new(2026, 10, 7, 1, 30, 0, TimeSpan.Zero);

    /// <summary>The moment the report below ended, forty minutes before the box was asked.</summary>
    private static readonly DateTimeOffset Checked = new(2026, 10, 7, 0, 50, 0, TimeSpan.Zero);

    /// <summary>The moment the stale report below ended, three hours before the box was asked.</summary>
    private static readonly DateTimeOffset Stale = new(2026, 10, 6, 22, 30, 0, TimeSpan.Zero);

    private static readonly TimeZoneInfo Utc = TimeZoneInfo.Utc;

    private static readonly TimeZoneInfo WestFive =
        TimeZoneInfo.CreateCustomTimeZone("west-five", TimeSpan.FromHours(-5), "West Five", "West Five");

    /// <summary>A report of one app, the parts it is told, the windows left out the way the file leaves them out.</summary>
    private static ContainerAppReports Reports(string name, HourlyCheck? last, HealthWindow? day = null, HealthWindow? week = null) =>
        new(name, last, day, week);

    // ---------------------------------------------------------------- the line, in the minutes it counts

    /// <summary>
    /// The line in the minutes no line pinned says: under a minute the moment is "just now", which
    /// <see cref="ContainerAppReportsEdgeCaseTests" /> pins at the fifty-ninth second - and from the sixtyth second
    /// the minute the moment stands in, the one minute that runs to the one hundred and nineteenth second and the
    /// two minutes from the one hundred and twentieth. The newest result is the report itself through all of them,
    /// since all of them stand inside the two hours.
    /// </summary>
    [Theory]
    [InlineData(60, "hourly: healthy, 1 min ago")]
    [InlineData(119, "hourly: healthy, 1 min ago")]
    [InlineData(120, "hourly: healthy, 2 min ago")]
    public void TheLineCountsTheWholeMinuteTheMomentStandsIn(int secondsAgo, string line)
    {
        var last = new HourlyCheck(HourlyCheck.Healthy, Now.AddSeconds(-secondsAgo));
        var reports = Reports("ui", last);

        Assert.Same(last, HourlyCheckText.RecentResult(reports, Now));
        Assert.Equal(line, HourlyCheckText.Result(reports, Now));
    }

    // ---------------------------------------------------------------- the two hours, counted to the tick

    /// <summary>
    /// The newest result asked for itself at the two hours the line is said up to, counted to the tick: one second
    /// short of them, the very two hours, and one second past. The word <c>unhealthy</c> at the very two hours and
    /// one second past them, and <c>unreachable</c> at the very two hours, stand in
    /// <see cref="AvailabilityTests" /> and <see cref="RuntimePayloadBuilderTests" /> - what is pinned here is the
    /// word <c>healthy</c> through all three moments, and the newest result that is asked for itself rather than
    /// read out of the line the box says.
    /// </summary>
    [Theory]
    [InlineData(7199, "hourly: healthy, 119 min ago")]
    [InlineData(7200, "hourly: healthy, 2 h ago")]
    [InlineData(7201, "no recent hourly check")]
    public void TheNewestResultIsSaidUpToTheVeryTwoHoursAndNotOneTickPast(int secondsAgo, string line)
    {
        var last = new HourlyCheck(HourlyCheck.Healthy, Now.AddSeconds(-secondsAgo));
        var reports = Reports("ui", last);

        var recent = HourlyCheckText.RecentResult(reports, Now);

        Assert.Equal(secondsAgo <= 7200, recent is not null);
        if (recent is { } result)
        {
            Assert.Same(last, result);
        }

        Assert.Equal(line, HourlyCheckText.Result(reports, Now));
    }

    // ---------------------------------------------------------------- the word the check holds, in the line and the tooltip

    /// <summary>
    /// The line says the word the check carries and the tooltip says it in its own sentence, whichever word the
    /// check carries: the reading makes the three words it knows, but a check built by hand carries the other case,
    /// the empty word and the word of spaces just as it holds them, which
    /// <see cref="HourlyCheckEdgeCaseTests" /> pins of the record itself - and the line and the tooltip say the
    /// word they are given, the empty word and the word of spaces saying themselves in the spaces they take.
    /// </summary>
    [Theory]
    [InlineData("HEALTHY", "hourly: HEALTHY, 40 min ago", "and found it HEALTHY.")]
    [InlineData("", "hourly: , 40 min ago", "and found it .")]
    [InlineData(" ", "hourly:  , 40 min ago", "and found it  .")]
    public void TheLineAndTheTooltipSayTheWordTheCheckHolds(string word, string line, string sentence)
    {
        var reports = Reports("ui", new HourlyCheck(word, Checked));

        Assert.Equal(line, HourlyCheckText.Result(reports, Now));
        Assert.Contains(sentence, HourlyCheckText.Title(reports, Now, Utc), StringComparison.Ordinal);
    }

    /// <summary>
    /// No word at all is the word the line and the tooltip say: a check built by hand with no word carries none, as
    /// it carries the empty word, and the line and the tooltip say the line they say with the word the check holds -
    /// nothing the page refuses and nothing the page fills in.
    /// </summary>
    [Fact]
    public void TheLineAndTheTooltipSayNoWordWhenTheCheckHoldsNone()
    {
        var reports = Reports("ui", new HourlyCheck(null!, Checked));

        Assert.Equal("hourly: , 40 min ago", HourlyCheckText.Result(reports, Now));
        Assert.Contains("and found it .", HourlyCheckText.Title(reports, Now, Utc), StringComparison.Ordinal);
    }

    // ---------------------------------------------------------------- the counts, where one window counts nothing and the other counts

    /// <summary>
    /// The counts where the day counted no report and the week counted: the day is said as <c>none</c> and the week
    /// as it counts, in the order the line says them - the other way round, the week said as <c>none</c> beside a
    /// day that counts, standing in <see cref="AvailabilityTests" />, and both windows said alone and both left out
    /// in it and in <see cref="ContainerAppReportsEdgeCaseTests" />.
    /// </summary>
    [Fact]
    public void AWeekThatCountsBesideADayThatCountedNothingIsSaidWithIt()
    {
        Assert.Equal("24 h: none · 7 d: 69 of 70", HourlyCheckText.Counts(Reports("scorecard", null, new HealthWindow(0, 0), new HealthWindow(70, 69))));
    }

    // ---------------------------------------------------------------- the tooltip, the windows it counts and the sentences it leaves

    /// <summary>
    /// A window the file counts and that counted no report is said in the tooltip as it counts: the tooltip counts
    /// what the window holds and says nothing of <c>none</c>, which is the line's word for the same window in
    /// <see cref="HourlyCheckText.Counts" /> - the day that counted nothing, the week that counted nothing, and the
    /// two that did, the newest report said at the same moment as the reports it counted.
    /// </summary>
    [Fact]
    public void TheTooltipSaysZeroOfZeroForAWindowThatCountedNothing()
    {
        var checkedApp = new HourlyCheck(HourlyCheck.Healthy, Checked);
        const string intro =
            "Once an hour the pipeline's runbook \"Health report\" in Octopus Deploy asks the app whether it answers, "
            + "also while no page is open: the history, under the page's own probe. ";
        const string report = "The last report that asked it ended 2026-10-07 00:50 +00:00 (40 min ago) and found it healthy. ";

        Assert.Equal(
            intro + report
            + "Healthy in 0 of the 0 reports of 24 hours that asked it. "
            + "Healthy in 0 of the 0 reports of 7 days that asked it. "
            + "Hourly checks by the pipeline, not continuous monitoring.",
            HourlyCheckText.Title(Reports("scorecard", checkedApp, new HealthWindow(0, 0), new HealthWindow(0, 0)), Now, Utc));

        Assert.Equal(
            intro + report
            + "Healthy in 0 of the 0 reports of 24 hours that asked it. "
            + "Hourly checks by the pipeline, not continuous monitoring.",
            HourlyCheckText.Title(Reports("scorecard", checkedApp, new HealthWindow(0, 0), null), Now, Utc));

        Assert.Equal(
            intro + report
            + "Healthy in 0 of the 0 reports of 7 days that asked it. "
            + "Hourly checks by the pipeline, not continuous monitoring.",
            HourlyCheckText.Title(Reports("scorecard", checkedApp, null, new HealthWindow(0, 0)), Now, Utc));
    }

    /// <summary>
    /// The tooltip of a report the file counts no window for: the report the runbook last found, said in the last
    /// two hours and therefore said as news, and the sentence no tooltip leaves out - the windows that are not
    /// counted writing no sentence of their own, as the line leaves them out. Said for the word <c>unhealthy</c>
    /// with the day counted and for the app no report asked with the week counted, the report found in the last two
    /// hours with neither window counted stands open until here, and so does the word <c>unreachable</c>.
    /// </summary>
    [Fact]
    public void TheTooltipOfAReportWithoutCountedWindowsSaysTheReportAndNothingElse()
    {
        const string intro =
            "Once an hour the pipeline's runbook \"Health report\" in Octopus Deploy asks the app whether it answers, "
            + "also while no page is open: the history, under the page's own probe. ";
        const string note = "Hourly checks by the pipeline, not continuous monitoring.";

        Assert.Equal(
            intro + "The last report that asked it ended 2026-10-07 00:50 +00:00 (40 min ago) and found it healthy. " + note,
            HourlyCheckText.Title(Reports("scorecard", new HourlyCheck(HourlyCheck.Healthy, Checked)), Now, Utc));

        Assert.Equal(
            intro + "The last report that asked it ended 2026-10-07 00:50 +00:00 (40 min ago) and found it unreachable. " + note,
            HourlyCheckText.Title(Reports("scorecard", new HourlyCheck(HourlyCheck.Unreachable, Checked)), Now, Utc));
    }

    /// <summary>
    /// The stale result with no window counted: the sentence that says the box has stopped talking about the app,
    /// in the very words the box writes, and the sentence no tooltip leaves out - the windows the file does not
    /// hold writing no sentence of their own, and the word <c>unreachable</c> standing where
    /// <see cref="ContainerAppReportsEdgeCaseTests" /> says <c>unhealthy</c> with the day counted.
    /// </summary>
    [Fact]
    public void TheStaleSentenceStandsAloneWhenNoWindowIsCounted()
    {
        Assert.Equal(
            "Once an hour the pipeline's runbook \"Health report\" in Octopus Deploy asks the app whether it answers, "
            + "also while no page is open: the history, under the page's own probe. "
            + "The last report that asked it ended 2026-10-06 22:30 +00:00 (3 h ago) and found it unreachable: "
            + "none in the last two hours, so the box says no recent hourly check. "
            + "Hourly checks by the pipeline, not continuous monitoring.",
            HourlyCheckText.Title(Reports("scorecard", new HourlyCheck(HourlyCheck.Unreachable, Stale)), Now, Utc));
    }

    /// <summary>
    /// The stale result with only the week counted: the sentence that says the box has stopped talking about the
    /// app and the count of the reports of the week that asked it, the day the file does not hold writing no
    /// sentence - the stale result said with the week counted and the day counted, and with the day counted and the
    /// week left out, standing in <see cref="HealthHelperCoverageTests" /> and <see cref="ContainerAppReportsEdgeCaseTests" />.
    /// </summary>
    [Fact]
    public void TheTooltipOfAStaleResultCountsOnlyTheWeekItCounts()
    {
        Assert.Equal(
            "Once an hour the pipeline's runbook \"Health report\" in Octopus Deploy asks the app whether it answers, "
            + "also while no page is open: the history, under the page's own probe. "
            + "The last report that asked it ended 2026-10-06 22:30 +00:00 (3 h ago) and found it unhealthy: "
            + "none in the last two hours, so the box says no recent hourly check. "
            + "Healthy in 160 of the 168 reports of 7 days that asked it. "
            + "Hourly checks by the pipeline, not continuous monitoring.",
            HourlyCheckText.Title(Reports("scorecard", new HourlyCheck(HourlyCheck.Unhealthy, Stale), null, new HealthWindow(168, 160)), Now, Utc));
    }

    /// <summary>
    /// The stale sentence in the hours the viewer is in: the same report, three hours west of the meridian, said in
    /// the hours the viewer reads it in, with the offset it stands on - the moments the tooltip names are said in
    /// the viewer's hours everywhere, but the sentence that says the box has stopped talking about the app is pinned
    /// in the hours the file wrote them in, and this is it in the viewer's own.
    /// </summary>
    [Fact]
    public void TheStaleSentenceStandsInTheHoursTheViewerIsIn()
    {
        var reports = Reports("scorecard", new HourlyCheck(HourlyCheck.Unhealthy, Stale));

        Assert.Contains(
            "The last report that asked it ended 2026-10-06 17:30 -05:00 (3 h ago) and found it unhealthy: "
            + "none in the last two hours, so the box says no recent hourly check.",
            HourlyCheckText.Title(reports, Now, WestFive),
            StringComparison.Ordinal);
    }
}

/// <summary>
/// The tooltip's sentences are named once, at the top of <see cref="HourlyCheckText" />, and
/// <see cref="HourlyCheckText.Title" /> only chooses which of them are said: no sentence of it is a string literal
/// in the method's body, so a test that pins the tooltip pins the one template, not a copy of it.
/// </summary>
public class HourlyCheckTextTemplateTests
{
    [Fact]
    public void NoSentenceOfTheTooltipIsALiteralInsideTitle()
    {
        var body = TitleBody();

        Assert.Contains("HourlyReportWords.Intro", body, StringComparison.Ordinal);
        var sentences = System.Text.RegularExpressions.Regex.Matches(body, "\"(?:[^\"\\\\]|\\\\.)*\"")
            .Select(match => match.Value)
            .Where(literal => literal.Any(char.IsLetter))
            .ToList();
        Assert.Empty(sentences);
    }

    /// <summary>The text of <c>Title</c>, from its declaration to the closing brace at its own indent.</summary>
    private static string TitleBody([System.Runtime.CompilerServices.CallerFilePath] string here = "")
    {
        var source = File.ReadAllText(Path.GetFullPath(
            Path.Combine(Path.GetDirectoryName(here)!, "..", "Dashboard", "Health", "ContainerAppReports.cs")));
        var start = source.IndexOf("public static string Title(", StringComparison.Ordinal);
        Assert.True(start >= 0, "HourlyCheckText.Title was not found where the tooltip is written");
        var end = source.IndexOf("\n    }\n", start, StringComparison.Ordinal);
        Assert.True(end > start, "the end of HourlyCheckText.Title was not found");
        return source[start..end];
    }
}
