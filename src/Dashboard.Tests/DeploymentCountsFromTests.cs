using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The one moment a deployment's mark counts from, <see cref="Deployment" /> and the number
/// <see cref="DeploymentMark.Minutes" /> says of how long ago it was (#1631).
/// <para>
/// Which of the two moments of the file a deployment is counted from is a fact about the deployment beside
/// <c>InFlight</c> and <c>IsMarkedAt</c>, and how an age becomes the mark's number is a fact about the mark. The two
/// rules were written out in one argument of a constructor call and in the <c>Minutes</c> doc comment; these tests
/// pin them where they now live: the moment by state (its start while in flight, its end once ended, its start for a
/// state the page does not know as long as the file gives it no end, and nothing when the moment the file does name
/// does not parse), and the number through the public mark path (whole minutes truncated, a clock behind the file's
/// time reading 0, no number at all where the file's time does not parse).
/// </para>
/// <para>
/// <see cref="DeploymentEdgeCaseTests" /> pins the deployment's public surface as an exact list, and that list is the
/// point of it: the moment is therefore the record's <c>internal</c> one, so it is read here through the assembly, as
/// <see cref="MarkPlaceOrderEdgeCaseTests" /> reads the places of the marks and
/// <see cref="ProbeUrlEdgeCaseTests" /> reads the resolving of an address - the app declares no
/// <c>InternalsVisibleTo</c>, so a test cannot name an internal member but can reach it. The list pinned there is
/// unchanged, and the one assertion added here holds the member internal rather than public, which is what keeps the
/// two pins saying the same thing.
/// </para>
/// <para>
/// Nothing here reaches the network or asks the clock what time it is: every moment is one the test brings.
/// </para>
/// </summary>
public class DeploymentCountsFromTests
{
    /// <summary>Thursday a few minutes past four, UTC: the moment the page is asked.</summary>
    private static readonly DateTimeOffset Now = new(2026, 10, 8, 4, 23, 24, TimeSpan.Zero);

    /// <summary>Three minutes and seventeen seconds before the page is asked: when a deployment in flight started.</summary>
    private static readonly DateTimeOffset Since = new(2026, 10, 8, 4, 20, 7, TimeSpan.Zero);

    /// <summary>Five minutes and twenty-four seconds before the page is asked: when a deployment that ended ended.</summary>
    private static readonly DateTimeOffset Ended = new(2026, 10, 8, 4, 18, 0, TimeSpan.Zero);

    /// <summary>Five minutes and fifty-nine seconds before the page is asked: the age a mark truncates to 5.</summary>
    private const string FiveMinutesFiftyNineSecondsAgo = "2026-10-08T04:17:25Z";

    /// <summary>Fifty-nine seconds before the page is asked: the age a mark truncates to 0.</summary>
    private const string FiftyNineSecondsAgo = "2026-10-08T04:22:25Z";

    /// <summary>A minute and a half after the page is asked: a file whose time runs ahead of the reader's clock.</summary>
    private const string NinetySecondsAhead = "2026-10-08T04:24:54Z";

    /// <summary>Twelve minutes and twenty-four seconds before the page is asked: a start of an ended deployment, old
    /// enough that no mark could be read off it.</summary>
    private const string LongBefore = "2026-10-08T04:11:00Z";

    private const string Slug = "demo";

    private static readonly string[] Ui = ["ui"];

    /// <summary>The moment a deployment counts from, as the record declares it: internal, so no test can name it.</summary>
    private static readonly PropertyInfo CountsFrom =
        typeof(Deployment).GetProperty("CountsFrom", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)!;

    /// <summary>A deployment to uat of a state, said in the state's own word, with the two moments the file may give.</summary>
    private static Deployment Of(DeploymentState state, DateTimeOffset? since, DateTimeOffset? finished) =>
        new("demo-ui", "uat", "1", state, state.ToString().ToLowerInvariant(), since, finished, null);

    /// <summary>What the record says the deployment counts from.</summary>
    private static DateTimeOffset? CountsFromOf(Deployment deployment) => (DateTimeOffset?)CountsFrom.GetValue(deployment);

    /// <summary>A file of one deployment of a state, whose two moments are read as the file writes them.</summary>
    private static DeploymentsReport Report(string state, string? since, string? finished) =>
        DeploymentsReport.Parse($$"""
            { "deployments": [ { "project": "demo-ui", "environment": "uat", "release": "1", "state": "{{state}}", "since": {{Words(since)}}, "finished": {{Words(finished)}} } ] }
            """)!;

    /// <summary>A moment of the file as it is written: quoted, or the JSON null the file gives when it says nothing.</summary>
    private static string Words(string? moment) => moment is null ? "null" : $"\"{moment}\"";

    private static IReadOnlyList<DeploymentMark> Marks(DeploymentsReport report, DateTimeOffset now) =>
        report.Marks("uat", Slug, Ui, now);

    // ----- the moment is the record's own, and no part of its public shape

    /// <summary>The moment a mark counts from is declared on the deployment beside the flight it turns on: a read-only
    /// nullable moment of the record. The ten parts and the flight that
    /// <see cref="DeploymentEdgeCaseTests.TheDeploymentHasTenPartsTheFlightAndTheMembersEveryRecordIsAsked" /> pins are
    /// left exactly as that pin has them - the moment is internal, so the list of the public surface keeps its eleven
    /// names.</summary>
    [Fact]
    public void TheMomentIsTheRecordsOwnAndNoPublicPart()
    {
        Assert.NotNull(CountsFrom);
        Assert.Equal(typeof(DateTimeOffset?), CountsFrom.PropertyType);
        Assert.True(CountsFrom.CanRead);
        Assert.False(CountsFrom.CanWrite);
        Assert.Null(CountsFrom.SetMethod);
        Assert.True(CountsFrom.GetMethod!.IsAssembly);
        Assert.False(CountsFrom.GetMethod!.IsPublic);
        Assert.Equal("get_CountsFrom", CountsFrom.GetMethod!.Name);

        Assert.DoesNotContain(
            "CountsFrom",
            typeof(Deployment)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Select(property => property.Name));
    }

    // ----- what is in flight counts from when it started

    /// <summary>A queued, executing or waiting deployment counts from when it started, or was queued; the end the file
    /// gives it beside does not change that answer, because the state is what says it has not ended.</summary>
    [Theory]
    [InlineData(DeploymentState.Queued)]
    [InlineData(DeploymentState.Executing)]
    [InlineData(DeploymentState.Waiting)]
    public void WhatIsInFlightCountsFromWhenItStarted(DeploymentState state)
    {
        Assert.Equal(Since, CountsFromOf(Of(state, Since, null)));
        Assert.Equal(Since, CountsFromOf(Of(state, Since, Ended)));
        Assert.Equal(Since, CountsFromOf(Of(state, Since, Now.AddYears(1))));
    }

    // ----- what ended counts from when it ended

    /// <summary>A deployment that succeeded, failed or was canceled counts from when it ended, whatever the file says
    /// it started: a start written after its end, and no start at all, both leave the end as the answer.</summary>
    [Theory]
    [InlineData(DeploymentState.Succeeded)]
    [InlineData(DeploymentState.Failed)]
    [InlineData(DeploymentState.Canceled)]
    public void WhatEndedCountsFromWhenItEnded(DeploymentState state)
    {
        Assert.Equal(Ended, CountsFromOf(Of(state, Since, Ended)));
        Assert.Equal(Ended, CountsFromOf(Of(state, null, Ended)));
        Assert.Equal(Ended, CountsFromOf(Of(state, Now.AddYears(1), Ended)));
    }

    // ----- a state the page does not know, which way the file's end falls

    /// <summary>A state the page does not know counts from its start while the file gives it no end, and from its end
    /// once it does - the same turn <c>InFlight</c> makes, since it is the flight that decides the moment.</summary>
    [Fact]
    public void AStateThePageDoesNotKnowCountsFromWhicheverMomentTheFileGivesIt()
    {
        Assert.Equal(Since, CountsFromOf(Of(DeploymentState.Unknown, Since, null)));
        Assert.Equal(Ended, CountsFromOf(Of(DeploymentState.Unknown, Since, Ended)));

        // The turn is the flight's, so a number outside the seven that ended counts from its end as well.
        Assert.Equal(Ended, CountsFromOf(Of((DeploymentState)7, Since, Ended)));
    }

    // ----- no moment named, nothing counted from

    /// <summary>A deployment in flight with no start the file can say, and one that ended with no end it can say,
    /// count from nothing: the record gives no moment, and so a mark has no number to say.</summary>
    [Theory]
    [InlineData(DeploymentState.Queued)]
    [InlineData(DeploymentState.Executing)]
    [InlineData(DeploymentState.Waiting)]
    [InlineData(DeploymentState.Succeeded)]
    [InlineData(DeploymentState.Failed)]
    [InlineData(DeploymentState.Canceled)]
    [InlineData(DeploymentState.Unknown)]
    public void NothingIsCountedFromNoTime(DeploymentState state) => Assert.Null(CountsFromOf(Of(state, null, null)));

    /// <summary>An ended deployment whose start parses and whose end does not counts from nothing: the page does not
    /// fall back to the start it can read, which would say an age of a deployment whose age it cannot tell. A state the
    /// page does not know is the exception the flight makes: with no end to count from it has not ended, and counts
    /// from its start (<see cref="AStateThePageDoesNotKnowCountsFromWhicheverMomentTheFileGivesIt" />).</summary>
    [Theory]
    [InlineData(DeploymentState.Succeeded)]
    [InlineData(DeploymentState.Failed)]
    [InlineData(DeploymentState.Canceled)]
    public void AFailedToParseEndIsNotCountedFromTheStart(DeploymentState state) => Assert.Null(CountsFromOf(Of(state, Since, null)));

    // ----- the number a mark says of that moment

    /// <summary>Through the public mark path the age of the moment is whole minutes truncated: five minutes and
    /// fifty-nine seconds of a deployment running, or of one that ended, are 5, and fifty-nine seconds are 0.</summary>
    [Theory]
    [InlineData("executing", FiveMinutesFiftyNineSecondsAgo, 5)]
    [InlineData("executing", FiftyNineSecondsAgo, 0)]
    [InlineData("queued", FiveMinutesFiftyNineSecondsAgo, 5)]
    [InlineData("failed", FiveMinutesFiftyNineSecondsAgo, 5)]
    [InlineData("failed", FiftyNineSecondsAgo, 0)]
    [InlineData("succeeded", FiveMinutesFiftyNineSecondsAgo, 5)]
    public void AMarkSaysWholeMinutesAndTruncatesThem(string state, string countedFrom, int expected)
    {
        var ended = state is "failed" or "succeeded";

        var mark = Assert.Single(Marks(Report(state, ended ? LongBefore : countedFrom, ended ? countedFrom : null), Now));

        Assert.Equal(expected, mark.Minutes);
    }

    /// <summary>A clock that runs behind the file's time reads 0 rather than a negative number, for a deployment still
    /// in flight and for one that ended: a deployment that has not begun to be old is none minutes old.</summary>
    [Theory]
    [InlineData("executing")]
    [InlineData("queued")]
    [InlineData("failed")]
    [InlineData("canceled")]
    public void AFileThatRunsAheadOfTheClockIsNotOld(string state)
    {
        var ended = state is "failed" or "canceled";

        var mark = Assert.Single(Marks(Report(state, ended ? LongBefore : NinetySecondsAhead, ended ? NinetySecondsAhead : null), Now));

        Assert.Equal(0, mark.Minutes);
        Assert.True(mark.Minutes >= 0);
    }

    /// <summary>Where the file's time does not parse the mark says no number at all: null for a deployment in flight
    /// whose start the page cannot read, and a deployment that ended at a time it cannot read is marked no longer.</summary>
    [Fact]
    public void AnAgeThePageCannotReadIsSaidAsNoNumber()
    {
        var running = Assert.Single(Marks(Report("executing", "a while ago", null), Now));

        Assert.True(running.InFlight);
        Assert.Null(running.Minutes);
        Assert.Equal("deploying demo-ui 1 to uat", running.Sentence);

        // One that ended at a time the page cannot read is marked no longer: nothing says for how long it has been so.
        Assert.Empty(Marks(Report("failed", LongBefore, "just now"), Now));
    }

    /// <summary>The moment a mark counts from is the deployment's own, and not a second reading of it: the mark of a
    /// deployment in flight is the age of its start and the mark of one that ended the age of its end, whatever the
    /// other moment the file happens to give.</summary>
    [Fact]
    public void TheMarkOfADeploymentIsTheAgeOfTheMomentItCountsFrom()
    {
        // Both files hold the same two moments; which one the mark counts from is the deployment's to say.
        var running = Assert.Single(Marks(Report("executing", FiveMinutesFiftyNineSecondsAgo, NinetySecondsAhead), Now));
        var ended = Assert.Single(Marks(Report("succeeded", FiveMinutesFiftyNineSecondsAgo, FiftyNineSecondsAgo), Now));

        Assert.Equal((5, 0), (running.Minutes, ended.Minutes));
        Assert.Equal((true, false), (running.InFlight, ended.InFlight));
        Assert.Null(Assert.Single(Marks(Report("queued", null, null), Now)).Minutes);
    }
}
