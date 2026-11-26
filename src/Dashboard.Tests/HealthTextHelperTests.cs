namespace Dashboard.Tests;

/// <summary>
/// A length of time in whole units: seconds under a minute, minutes under two hours, hours under two days, days
/// after that; never negative, and a unit is dropped by truncation, not by rounding.
/// </summary>
public class TimeTextHelperTests
{
    [Theory]
    [InlineData(-86400, "0 s")]
    [InlineData(-1, "0 s")]
    [InlineData(0, "0 s")]
    [InlineData(1, "1 s")]
    [InlineData(45, "45 s")]
    [InlineData(59, "59 s")]
    [InlineData(60, "1 min")]
    [InlineData(119, "1 min")]
    [InlineData(180, "3 min")]
    [InlineData(7199, "119 min")]
    [InlineData(7200, "2 h")]
    [InlineData(43199, "11 h")]
    [InlineData(172799, "47 h")]
    [InlineData(172800, "2 d")]
    [InlineData(345600, "4 d")]
    public void ALengthOfTimeIsOneWholeUnit(int seconds, string expected) =>
        Assert.Equal(expected, TimeText.Span(TimeSpan.FromSeconds(seconds)));

    [Theory]
    [InlineData(0.9, "0 s")]
    [InlineData(45.7, "45 s")]
    [InlineData(59.99, "59 s")]
    [InlineData(60.5, "1 min")]
    public void PartOfAUnitIsDroppedAndNeverRoundedUp(double seconds, string expected) =>
        Assert.Equal(expected, TimeText.Span(TimeSpan.FromSeconds(seconds)));

    [Fact]
    public void TheUnitIsWrittenAsTheDashboardWritesItAndNotAsTheViewerReadsOne() =>
        ViewerCulture.German(() => Assert.Equal("2 h", TimeText.Span(TimeSpan.FromHours(2.5))));
}

/// <summary>
/// The day a cost number covers, as the page writes it in its lines (<c>on 2026-10-03</c>, <c>as of 2026-10-06
/// (UTC)</c>): an ISO date of the invariant culture, whatever the viewer's.
/// </summary>
public class CostTextHelperTests
{
    [Theory]
    [InlineData(2026, 10, 3, "2026-10-03")]
    [InlineData(2026, 1, 9, "2026-01-09")]
    [InlineData(2026, 12, 31, "2026-12-31")]
    [InlineData(2024, 2, 29, "2024-02-29")]
    [InlineData(1, 1, 1, "0001-01-01")]
    [InlineData(9999, 12, 31, "9999-12-31")]
    public void ADateIsTheIsoOneWithEveryPartPadded(int year, int month, int day, string expected) =>
        Assert.Equal(expected, CostText.Date(new DateOnly(year, month, day)));

    [Fact]
    public void TheDayStaysTheIsoOneForAViewerWhoWritesDatesOtherwise() =>
        ViewerCulture.German(() => Assert.Equal("2026-10-03", CostText.Date(new DateOnly(2026, 10, 3))));
}

/// <summary>The mark of a detailed health check's entry: its shape and its place in the order of the worst first.</summary>
public class HealthDetailTextHelperTests
{
    [Theory]
    [InlineData(CheckState.Healthy, HealthState.Healthy)]
    [InlineData(CheckState.Degraded, HealthState.Unhealthy)]
    [InlineData(CheckState.Unhealthy, HealthState.Unreachable)]
    [InlineData(CheckState.Unknown, HealthState.Pending)]
    public void AMarkHasTheShapeOfItsState(CheckState state, HealthState icon) =>
        Assert.Equal(icon, HealthDetailText.Icon(state));

    [Theory]
    [InlineData(CheckState.Unhealthy, 0)]
    [InlineData(CheckState.Degraded, 1)]
    [InlineData(CheckState.Unknown, 2)]
    [InlineData(CheckState.Healthy, 3)]
    public void TheStatesAreInOrderOfSeverityTheWorstFirst(CheckState state, int severity) =>
        Assert.Equal(severity, HealthDetailText.Severity(state));

    [Fact]
    public void FourStatesMakeFourDifferentMarksInTheirOwnOrder()
    {
        var states = new[] { CheckState.Healthy, CheckState.Degraded, CheckState.Unhealthy, CheckState.Unknown };

        Assert.Equal(4, states.Select(HealthDetailText.Icon).Distinct().Count());
        Assert.Equal([0, 1, 2, 3], states.Select(HealthDetailText.Severity).Order());
    }
}

/// <summary>A part of the language bar as a whole percentage, with the one exception the page says out loud.</summary>
public class BuildTextHelperTests
{
    [Theory]
    [InlineData(0, "0 %")]
    [InlineData(0.001, "<1 %")]
    [InlineData(0.5, "<1 %")]
    [InlineData(0.999999, "<1 %")]
    [InlineData(1, "1 %")]
    [InlineData(1.4, "1 %")]
    [InlineData(1.5, "2 %")]
    [InlineData(3.5, "4 %")]
    [InlineData(12.34, "12 %")]
    [InlineData(73, "73 %")]
    [InlineData(99.6, "100 %")]
    [InlineData(100, "100 %")]
    public void AShareIsAWholePercentOrLessThanOne(double percent, string expected) =>
        Assert.Equal(expected, BuildText.SharePercent(percent));

    [Fact]
    public void ThePercentIsWrittenAndNotFormattedAndItKeepsItsSpace() =>
        ViewerCulture.German(() => Assert.Equal("2 %", BuildText.SharePercent(1.5)));

    [Fact]
    public void TheBarInWordsSaysEveryPartTheSameWay() =>
        Assert.Equal(
            "C# 86 %, Razor 13 %, Other <1 %",
            BuildText.Describe(
            [
                new LanguageShare("C#", 729, 85.8, 1),
                new LanguageShare("Razor", 114, 13.4, 2),
                new LanguageShare(BuildText.Other, 7, 0.8, 0),
            ]));
}

/// <summary>The tooltip of a link to a commit: the seven characters the page shows, and what opening it costs.</summary>
public class LinkTextHelperTests
{
    [Theory]
    [InlineData("0a1b2c3d4e5f60718293a4b5c6d7e8f901234567", "Commit 0a1b2c3 on GitHub (opens in a new tab)")]
    [InlineData("0a1b2c3d", "Commit 0a1b2c3 on GitHub (opens in a new tab)")]
    [InlineData("0a1b2c3", "Commit 0a1b2c3 on GitHub (opens in a new tab)")]
    [InlineData("0a1b2c", "Commit 0a1b2c on GitHub (opens in a new tab)")]
    [InlineData("a", "Commit a on GitHub (opens in a new tab)")]
    public void ACommitIsNamedByItsFirstSevenCharacters(string commit, string title) =>
        Assert.Equal(title, LinkText.CommitTitle(commit));

    [Fact]
    public void WithoutACommitThereIsNoTitle() => Assert.Throws<ArgumentNullException>(() => LinkText.CommitTitle(null!));
}

/// <summary>What a deployment is doing, in the words the fleet's dashboard uses too.</summary>
public class DeploymentTextHelperTests
{
    /// <summary>When the page read the file.</summary>
    private static readonly DateTimeOffset Now = new(2026, 10, 8, 12, 0, 0, TimeSpan.Zero);

    private static Deployment One(
        DeploymentState state,
        string stateText,
        DateTimeOffset? finished = null,
        DeploymentWait? waitsFor = null) =>
        new("cmdemo2-ui", "uat", "2.4.43", state, stateText, Now, finished, null, null, waitsFor);

    [Theory]
    [InlineData(DeploymentState.Queued, "queued", "cmdemo2-ui 2.4.43 is queued for uat")]
    [InlineData(DeploymentState.Executing, "executing", "deploying cmdemo2-ui 2.4.43 to uat")]
    [InlineData(DeploymentState.Waiting, "waiting", "cmdemo2-ui 2.4.43 waits for a sign-off in uat")]
    [InlineData(DeploymentState.Succeeded, "succeeded", "cmdemo2-ui 2.4.43 reached uat")]
    [InlineData(DeploymentState.Failed, "failed", "cmdemo2-ui 2.4.43 failed in uat")]
    [InlineData(DeploymentState.Canceled, "canceled", "cmdemo2-ui 2.4.43 was canceled in uat")]
    [InlineData(DeploymentState.Unknown, "paused", "cmdemo2-ui 2.4.43 in uat: paused")]
    [InlineData(DeploymentState.Unknown, "3", "cmdemo2-ui 2.4.43 in uat: 3")]
    public void WithoutAnEndTimeNothingIsSaidAboutHowLongAgo(DeploymentState state, string stateText, string sentence) =>
        Assert.Equal(sentence, DeploymentText.Sentence(One(state, stateText), Now));

    [Theory]
    [InlineData(DeploymentWait.SignOff, "cmdemo2-ui 2.4.43 waits for a sign-off in uat")]
    [InlineData("Sign-Off", "cmdemo2-ui 2.4.43 waits for a sign-off in uat")]
    [InlineData("a question", "cmdemo2-ui 2.4.43 waits for a sign-off in uat")]
    [InlineData(DeploymentWait.GuidedFailure, "cmdemo2-ui 2.4.43 asks what to do after a failed step in uat")]
    [InlineData("Guided Failure", "cmdemo2-ui 2.4.43 asks what to do after a failed step in uat")]
    public void OnlyAGuidedFailureAsksWhatToDoAfterAFailedStep(string kind, string sentence) =>
        Assert.Equal(sentence, DeploymentText.Sentence(One(DeploymentState.Waiting, "waiting", waitsFor: new DeploymentWait(kind, null, null, null)), Now));

    [Fact]
    public void WaitingThatSaysNothingOfWhatItWaitsForIsTheSignOff() =>
        Assert.Equal(
            "cmdemo2-ui 2.4.43 waits for a sign-off in uat",
            DeploymentText.Sentence(One(DeploymentState.Waiting, "waiting", finished: Now.AddHours(-1)), Now));

    [Theory]
    [InlineData(0, "1 min")]
    [InlineData(45, "45 min")]
    [InlineData(59, "59 min")]
    [InlineData(60, "1 h")]
    [InlineData(90, "1 h")]
    [InlineData(1739, "28 h")]
    [InlineData(2879, "47 h")]
    [InlineData(2880, "2 d")]
    [InlineData(4320, "3 d")]
    public void WhatEndedSaysHowLongAgoInOneUnit(int minutes, string age) =>
        Assert.Equal(
            $"cmdemo2-ui 2.4.43 reached uat {age} ago",
            DeploymentText.Sentence(One(DeploymentState.Succeeded, "succeeded", finished: Now.AddMinutes(-minutes)), Now));

    [Theory]
    [InlineData(DeploymentState.Failed, "cmdemo2-ui 2.4.43 failed in uat 5 min ago")]
    [InlineData(DeploymentState.Canceled, "cmdemo2-ui 2.4.43 was canceled in uat 5 min ago")]
    public void AnEndThatIsNotGoodKeepsItsOwnVerb(DeploymentState state, string sentence) =>
        Assert.Equal(sentence, DeploymentText.Sentence(One(state, state.ToString().ToLowerInvariant(), Now.AddMinutes(-5)), Now));

    [Fact]
    public void WithoutADeploymentThereIsNoSentence() =>
        Assert.Throws<ArgumentNullException>(() => DeploymentText.Sentence(null!, Now));
}

/// <summary>The trend of a node's own count of its outgoing HTTP calls.</summary>
public class TrendsHelperTests
{
    private static readonly DateTimeOffset ReadAt = new(2026, 10, 4, 22, 0, 0, TimeSpan.Zero);

    private static TelemetrySnapshot Sample(int http) => new(0, 0, 0, 0, null, 0, 0, 0, null, http, ReadAt);

    private static TargetStatus Node(params TelemetrySnapshot?[] samples)
    {
        var node = new TargetStatus(TargetKind.Node, "uat-west", new Uri("https://uat-west.example.net"), "westus3", "primary");
        foreach (var sample in samples)
        {
            node.RecordTelemetry(sample);
        }

        return node;
    }

    [Fact]
    public void TheCallsOfTwoChecksMakeALineOfTheirOwn()
    {
        var trend = Trends.Http(Node(Sample(4), Sample(9)))!;

        Assert.Equal([0.444, 1], trend.Points);
        Assert.Equal("Outgoing HTTP calls per minute, last 2 checks: 4 to 9, now 9", trend.Title);
    }

    [Fact]
    public void ACheckWithoutTelemetryIsAGapInTheLine()
    {
        var trend = Trends.Http(Node(Sample(2), null, Sample(6)))!;

        Assert.Equal([0.333, 1], trend.Points);
        Assert.Equal("Outgoing HTTP calls per minute, last 2 checks: 2 to 6, now 6", trend.Title);
    }

    [Fact]
    public void CallsThatStayAtZeroAreAFlatLineAtTheBottom()
    {
        var trend = Trends.Http(Node(Sample(0), Sample(0)))!;

        Assert.Equal([0, 0], trend.Points);
        Assert.Equal("Outgoing HTTP calls per minute, last 2 checks: 0 to 0, now 0", trend.Title);
    }

    [Theory]
    [InlineData(7, "0 to 7, now 7")]
    [InlineData(0, "0 to 0, now 0")]
    [InlineData(12, "0 to 12, now 12")]
    public void TheTitleNamesTheSmallestTheLargestAndTheLastReading(int http, string numbers) =>
        Assert.Equal($"Outgoing HTTP calls per minute, last 3 checks: {numbers}", Trends.Http(Node(Sample(0), Sample(http), Sample(http)))!.Title);

    [Fact]
    public void OneReadingOrNoneIsNoLine()
    {
        Assert.Null(Trends.Http(Node(Sample(4))));
        Assert.Null(Trends.Http(Node()));
        Assert.Null(Trends.Http(Node((TelemetrySnapshot?)null)));
        Assert.Null(Trends.Http(Node((TelemetrySnapshot?)null, Sample(4))));
    }

    [Fact]
    public void WithoutANodeThereIsNothingToDraw() => Assert.Throws<ArgumentNullException>(() => Trends.Http(null!));
}

/// <summary>
/// Runs what the page formats as a viewer whose culture writes dates and numbers differently: the dashboard's own
/// words are the invariant culture's, so a number or a day never changes with whoever looks.
/// </summary>
internal static class ViewerCulture
{
    public static void German(Action what) => CultureScope.Runs("de-DE", what);
}
