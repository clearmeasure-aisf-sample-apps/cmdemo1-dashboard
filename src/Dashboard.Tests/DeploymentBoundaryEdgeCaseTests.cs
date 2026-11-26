using System.Globalization;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests left open for <see cref="Deployment" />, the one entry of the
/// deployments file's <c>deployments</c> - declared in <c>src/Dashboard/Health/DeploymentsReport.cs</c>, beside the
/// report, the state and the mark it is read into.
/// <para>
/// <see cref="DeploymentEdgeCaseTests" /> already holds the record's own edge cases - and holds them under the name
/// this issue asked for, so these are set down under a name of their own rather than over that file, whose every test
/// stands. What is pinned there, and not pinned again here: the ten parts landing on the properties of their names,
/// the two the constructor gives up on standing in as nothing, every word kept letter for letter, the state and the
/// word of it as the two parts they are, the two moments kept to the tick, the address and the wait held and asked
/// nothing of, the parts coming back out in their order, the equality, the hash, the operators, the copy, the words
/// the record says and the surface it has; and the flight and the marking at the ordinary moments, at the ten
/// minutes' two ends to the tick, in another hour, and with no end at all.
/// <see cref="DeploymentsTests" /> pins the same two said of a file read, and <see cref="DeploymentsReportEdgeCaseTests" />
/// the ten minutes as the fixed length <see cref="DeploymentsReport.EndedFor" /> names.
/// </para>
/// <para>
/// Left open by all of that, and pinned here: the two ends of the clock standing as ends of the record's two moments
/// and not as their absence - an end at the bottom of the clock is an end, which is the whole of what
/// <see cref="Deployment.InFlight" /> asks of <see cref="Deployment.Finished" />, and an end at its top is a moment no
/// asking can be ten minutes after; the window counted as a difference, so that the ten thousand years between the
/// two ends of the clock are counted without spilling the length it is counted in; every number the page does not
/// name - below the first state and above the last, at the two ends of an <see cref="int" /> among them - being a
/// deployment that ended, told apart from its neighbours and said as the number it is; the moment a deployment waits
/// for having nothing to say about either the flight or the marking, in whichever state it stands and even as a wait
/// of nothing at all; one task address spelled another way being one deployment, since the record asks the address
/// only its own equality; and that asking the flight and the marking turns nothing, both answering the same twice
/// over, the same for two equal deployments, and leaving the parts they were built of where they were.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: every moment is one the test brings,
/// and the one address below is held and never fetched.
/// </para>
/// </summary>
public class DeploymentBoundaryEdgeCaseTests
{
    /// <summary>Thursday a few minutes past four, UTC: the moment the page is asked.</summary>
    private static readonly DateTimeOffset Now = new(2026, 10, 8, 4, 23, 24, TimeSpan.Zero);

    /// <summary>Five minutes before the page is asked: the moment a deployment that ended ended.</summary>
    private static readonly DateTimeOffset Ended = new(2026, 10, 8, 4, 18, 0, TimeSpan.Zero);

    /// <summary>Twenty minutes past four, UTC: the moment the sample's deployment started.</summary>
    private static readonly DateTimeOffset Since = new(2026, 10, 8, 4, 20, 7, TimeSpan.Zero);

    private const string Tasks = "https://octopus.example.net/app#/Spaces-356/tasks/ServerTasks-";

    /// <summary>The whole of the clock, from the first moment it runs to the last.</summary>
    private static readonly TimeSpan WholeClock = DateTimeOffset.MaxValue - DateTimeOffset.MinValue;

    /// <summary>A sign-off whose title says no more than the kind does, asked at the minute the sample ended.</summary>
    private static readonly DeploymentWait SignOff = new(DeploymentWait.SignOff, "Sign-off", Ended, "demo approvers");

    /// <summary>A guided failure, whose title says more than the kind does.</summary>
    private static readonly DeploymentWait Question = new(DeploymentWait.GuidedFailure, "Database is unreachable", Ended, "jeffrey");

    /// <summary>A deployment of no address, nobody and nowhere to act on: the eight parts a file must give.</summary>
    private static Deployment Bare() => Deployments.Bare();

    /// <summary>One deployment of a state and an end, of nothing else: the flight and the marking ask no more.</summary>
    private static Deployment Of(DeploymentState state, DateTimeOffset? finished) =>
        Deployments.InState(state, finished, since: Since, stateText: "as the file wrote it");

    /// <summary>The words a deployment says its parts in, taken in the invariant culture whatever the reader of this
    /// test happens to be, since the record's own words follow whoever reads them.</summary>
    private static string Words(Deployment deployment)
    {
        ArgumentNullException.ThrowIfNull(deployment);

        var reader = Thread.CurrentThread.CurrentCulture;
        try
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            return deployment.ToString()!;
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = reader;
        }
    }

    // ----- the two ends of the clock, as ends and not as the absence of a moment

    /// <summary>An end at either end of the clock is an end: the bottom of it, the year one written out, is a moment
    /// the deployment ended at and so closes its flight - the flight looks at whether an end is there and never at
    /// how early or how late it stands, and a deployment whose end the page cannot read is a deployment with no end
    /// at all and not one at the epoch. An end at the top, on the other hand, is a moment no asking is ever ten
    /// minutes after: the marking wants a difference, nothing offers one, and so it marks at every moment there is to
    /// ask at.</summary>
    [Fact]
    public void AnEndAtTheEndOfTheClockIsAnEndAndNotNoEnd()
    {
        var atTheBottom = Of(DeploymentState.Unknown, DateTimeOffset.MinValue);

        Assert.NotNull(atTheBottom.Finished);
        Assert.Equal(DateTimeOffset.MinValue, atTheBottom.Finished);
        Assert.False(atTheBottom.InFlight);
        Assert.False(Of(DeploymentState.Failed, DateTimeOffset.MinValue).InFlight);

        // The same state, the same start, and no end at all: the nothing is the whole of the difference - and an end
        // of no moment given is the epoch, which is an end.
        Assert.True(Of(DeploymentState.Unknown, null).InFlight);
        Assert.False(Of(DeploymentState.Unknown, new DateTimeOffset()).InFlight);

        // At the bottom of the clock it marks - the asking and the end are one moment - and stops marking when its
        // own ten minutes are full, however far the rest of the clock still runs.
        Assert.True(atTheBottom.IsMarkedAt(DateTimeOffset.MinValue));
        Assert.True(atTheBottom.IsMarkedAt(DateTimeOffset.MinValue + DeploymentsReport.EndedFor - TimeSpan.FromTicks(1)));
        Assert.False(atTheBottom.IsMarkedAt(DateTimeOffset.MinValue + DeploymentsReport.EndedFor));
        Assert.False(atTheBottom.IsMarkedAt(Now));
        Assert.False(atTheBottom.IsMarkedAt(DateTimeOffset.MaxValue));

        var atTheTop = Of(DeploymentState.Unknown, DateTimeOffset.MaxValue);

        Assert.NotNull(atTheTop.Finished);
        Assert.False(atTheTop.InFlight);
        Assert.True(atTheTop.IsMarkedAt(DateTimeOffset.MinValue));
        Assert.True(atTheTop.IsMarkedAt(Now));
        Assert.True(atTheTop.IsMarkedAt(DateTimeOffset.MaxValue));
        Assert.True(Of(DeploymentState.Succeeded, DateTimeOffset.MaxValue).IsMarkedAt(Now));
    }

    /// <summary>The window is counted as a difference between two moments and nothing else: the whole of the clock
    /// lying between the asking and the end - ten thousand years, the most the two can ever lie by and well within
    /// the length it is counted in - says that nothing is marked, and without spilling that length; the same
    /// difference the other way round, an end the page has not reached yet, says marked at both ends of the clock and
    /// in between. Every state that has ended answers the same, and the counting stands in whichever hour the two
    /// moments happen to be spelled in.</summary>
    [Fact]
    public void TheWindowIsCountedAcrossTheWholeClockAndOverflowsNothing()
    {
        Assert.Equal(3_652_058, WholeClock.Days);
        Assert.True(WholeClock < TimeSpan.MaxValue);
        Assert.True(DeploymentsReport.EndedFor < WholeClock);

        Deployment[] behind =
        [
            Of(DeploymentState.Succeeded, DateTimeOffset.MinValue),
            Of(DeploymentState.Failed, DateTimeOffset.MinValue),
            Of(DeploymentState.Canceled, DateTimeOffset.MinValue),
            Of(DeploymentState.Unknown, DateTimeOffset.MinValue),
        ];

        Assert.All(behind, deployment => Assert.False(deployment.InFlight));
        Assert.All(behind, deployment => Assert.False(deployment.IsMarkedAt(DateTimeOffset.MaxValue)));
        Assert.All(behind, deployment => Assert.False(deployment.IsMarkedAt(Now)));
        Assert.All(behind, deployment => Assert.True(deployment.IsMarkedAt(DateTimeOffset.MinValue)));

        Deployment[] ahead =
        [
            Of(DeploymentState.Succeeded, DateTimeOffset.MaxValue),
            Of(DeploymentState.Failed, DateTimeOffset.MaxValue),
            Of(DeploymentState.Canceled, DateTimeOffset.MaxValue),
            Of(DeploymentState.Unknown, DateTimeOffset.MaxValue),
        ];

        Assert.All(ahead, deployment => Assert.False(deployment.InFlight));
        Assert.All(ahead, deployment => Assert.True(deployment.IsMarkedAt(DateTimeOffset.MinValue)));
        Assert.All(ahead, deployment => Assert.True(deployment.IsMarkedAt(Now)));
        Assert.All(ahead, deployment => Assert.True(deployment.IsMarkedAt(DateTimeOffset.MaxValue)));

        // The hour the two moments are spelled in is no part of the difference: the bottom of the clock in the west
        // of the meridian is a whole afternoon after the bottom of the instant, and an end so spelled marks at the
        // bottom of the clock.
        var west = new DateTimeOffset(DateTimeOffset.MinValue.Ticks, TimeSpan.FromHours(-14));

        Assert.Equal(-14d, west.Offset.TotalHours);
        Assert.Equal(TimeSpan.FromHours(14), west.UtcDateTime - DateTimeOffset.MinValue.DateTime);
        Assert.True(Of(DeploymentState.Failed, west).IsMarkedAt(DateTimeOffset.MinValue));
        Assert.False(Of(DeploymentState.Failed, west).IsMarkedAt(west + DeploymentsReport.EndedFor));
        Assert.False(Of(DeploymentState.Failed, west.ToUniversalTime()).InFlight);
    }

    // ----- every number the page has no word for

    /// <summary>Below the first state and above the last, at the two ends of an <see cref="int" /> among them: a
    /// number the page has no word for is none of the three that fly, so it has ended - the same answer whatever the
    /// file's own word beside it says. With an end it stands in the ten minutes like any other state that ended;
    /// without one it has no window to stand in at all. Only the number 7 of these was walked where the flight is
    /// pinned; a number below the first, past the width of the seven and said as its own digits are pinned here.</summary>
    [Theory]
    [InlineData(-1, "State = -1, ")]
    [InlineData(7, "State = 7, ")]
    [InlineData(42, "State = 42, ")]
    [InlineData(int.MinValue, "State = -2147483648, ")]
    [InlineData(int.MaxValue, "State = 2147483647, ")]
    public void EveryNumberThePageDoesNotNameIsADeploymentThatEnded(int number, string spoken)
    {
        var state = (DeploymentState)number;

        Assert.False(Enum.IsDefined(state));
        Assert.Equal(number, (int)state);

        var without = Of(state, null);
        var with = Of(state, Ended);

        Assert.False(without.InFlight);
        Assert.False(with.InFlight);
        Assert.False(without.IsMarkedAt(Now));
        Assert.False(without.IsMarkedAt(Since));
        Assert.True(with.IsMarkedAt(Now));
        Assert.True(with.IsMarkedAt(Ended + DeploymentsReport.EndedFor - TimeSpan.FromTicks(1)));
        Assert.False(with.IsMarkedAt(Ended + DeploymentsReport.EndedFor));

        // The number is said as the number it stands on, and not as a name the page does not have.
        Assert.Contains(spoken, Words(with), StringComparison.Ordinal);
        Assert.DoesNotContain("State = Unknown, ", Words(with), StringComparison.Ordinal);
    }

    /// <summary>The numbers the page does not name are as many deployments apart as they are numbers, and not one of
    /// them is <see cref="DeploymentState.Unknown" />: of all the numbers the page has no word for, zero is the only
    /// one it reads as a state of its own, and so the only one that flies while the file gives it no end.</summary>
    [Fact]
    public void EveryNumberThePageDoesNotNameIsADifferentDeployment()
    {
        int[] beyond = [-1, 7, 42, int.MinValue, int.MaxValue];
        var deployments = beyond.Select(number => Of((DeploymentState)number, null)).ToArray();
        var unknown = Of(DeploymentState.Unknown, null);

        Assert.Equal(5, deployments.Length);
        Assert.Equal(5, new HashSet<Deployment>(deployments).Count);
        Assert.DoesNotContain(unknown, deployments);
        Assert.True(unknown.InFlight);
        Assert.All(deployments, deployment => Assert.False(deployment.InFlight));
        Assert.All(deployments, deployment => Assert.False(deployment.Equals(unknown)));
        Assert.All(deployments, deployment => Assert.False(deployment.Equals((object)unknown)));
        Assert.All(deployments, deployment => Assert.False(deployment.Equals((object)DeploymentState.Unknown)));
    }

    // ----- the address, which the record asks only its own equality

    /// <summary>The task in Octopus Deploy is compared as an address and not as the letters it was written in, since
    /// the record asks it nothing beyond the address's own equality: <c>HTTPS://Octopus.Example.Net:443</c> and
    /// <c>https://octopus.example.net</c> are two instances of one address, and so two ways of building one
    /// deployment - the very same in a set, in a lookup and in the mark's link - while the words the deployment says
    /// of it are the address's own and say it normalized. What the reading of a file lets through is another thing
    /// and pinned in <see cref="JsonReadTests" />; nothing is fetched here.</summary>
    [Fact]
    public void OneTaskAddressSpelledAnotherWayIsOneDeployment()
    {
        var asTheFileWrites = Bare() with { Url = new Uri($"{Tasks}1") };
        var asAPersonTypesIt = Bare() with { Url = new Uri("HTTPS://Octopus.Example.Net:443/app#/Spaces-356/tasks/ServerTasks-1") };

        Assert.NotSame(asTheFileWrites.Url, asAPersonTypesIt.Url);
        Assert.Equal(asTheFileWrites, asAPersonTypesIt);
        Assert.True(asTheFileWrites == asAPersonTypesIt);
        Assert.False(asTheFileWrites != asAPersonTypesIt);
        Assert.Equal(asTheFileWrites.GetHashCode(), asAPersonTypesIt.GetHashCode());
        Assert.Single(new HashSet<Deployment> { asTheFileWrites, asAPersonTypesIt });

        var keyed = new Dictionary<Deployment, string> { [asTheFileWrites] = "the task" };
        keyed[asAPersonTypesIt] = "the same task";

        Assert.Single(keyed);
        Assert.Equal("the same task", keyed[asTheFileWrites]);

        // The address says itself, in the words of the deployment that holds it, in its own normalized shape.
        Assert.Equal("https://octopus.example.net/app#/Spaces-356/tasks/ServerTasks-1", asAPersonTypesIt.Url!.ToString());
        Assert.Contains("Url = https://octopus.example.net/app#/Spaces-356/tasks/ServerTasks-1", Words(asAPersonTypesIt), StringComparison.Ordinal);

        // What hangs after the fragment is no part of the address an equality is counted on; what comes before it is.
        Assert.Equal(asTheFileWrites, Bare() with { Url = new Uri("https://octopus.example.net/app") });
        Assert.NotEqual(asTheFileWrites, Bare() with { Url = new Uri("https://octopus.example.net/app?space=356") });
        Assert.NotEqual(asTheFileWrites, Bare() with { Url = new Uri("https://octopus.other.net/app#/Spaces-356/tasks/ServerTasks-1") });

        // An address that is no address but a path is compared as the path it is: twice written, one deployment, and
        // another deployment than no address at all.
        Assert.Equal(Bare() with { Url = new Uri("tasks/ServerTasks-1", UriKind.Relative) }, Bare() with { Url = new Uri("tasks/ServerTasks-1", UriKind.Relative) });
        Assert.NotEqual(Bare() with { Url = new Uri("tasks/ServerTasks-1", UriKind.Relative) }, Bare());

        // None of it has anything to say about the flight or the marking.
        Assert.True(asTheFileWrites.InFlight);
        Assert.True(asAPersonTypesIt.InFlight);
        Assert.True(asAPersonTypesIt.IsMarkedAt(Now));
    }

    /// <summary>What a deployment waits for, and who started it, are two parts the flight and the marking never look
    /// at: the same deployment with a sign-off, with a question after a failed step, with a wait of no kind and with
    /// a wait of nothing at all is four deployments and one answer each of the two - in flight or ended as the state
    /// and the end say, and marked until the same instant. It is the page's words that turn a wait into somebody's
    /// business, and that is <see cref="DeploymentText.Detail" />.</summary>
    [Fact]
    public void WhatADeploymentWaitsForDecidesNeitherItsFlightNorItsMarking()
    {
        DeploymentWait[] waits =
        [
            SignOff,
            Question,
            new DeploymentWait(string.Empty, null, null, null),
            new DeploymentWait(null!, null, null, null),
        ];

        DeploymentState[] states =
        [
            DeploymentState.Queued,
            DeploymentState.Executing,
            DeploymentState.Waiting,
            DeploymentState.Succeeded,
            DeploymentState.Unknown,
            (DeploymentState)7,
        ];

        DateTimeOffset?[] ends = [Ended, null];

        foreach (var state in states)
        {
            foreach (var end in ends)
            {
                var one = Of(state, end);

                Assert.All(waits, wait => Assert.NotEqual(one, one with { WaitsFor = wait }));
                Assert.All(waits, wait => Assert.Equal(one.InFlight, (one with { WaitsFor = wait }).InFlight));
                Assert.All(waits, wait => Assert.Equal(one.IsMarkedAt(Now), (one with { WaitsFor = wait }).IsMarkedAt(Now)));
                Assert.All(waits, wait => Assert.Equal(one.InFlight, (one with { WaitsFor = wait, StartedBy = "jeffrey" }).InFlight));
            }
        }

        // A waiting deployment flies as the state says and not as the wait does; one that has ended carries a wait
        // and marks on its end alone, to the tick of the window.
        var asking = Of(DeploymentState.Waiting, null) with { WaitsFor = SignOff, StartedBy = "jeffrey" };

        Assert.True(asking.InFlight);
        Assert.True(asking.IsMarkedAt(Ended.AddYears(50)));

        var answered = Of(DeploymentState.Succeeded, Ended) with { WaitsFor = Question, StartedBy = null };

        Assert.False(answered.InFlight);
        Assert.True(answered.IsMarkedAt(Ended + DeploymentsReport.EndedFor - TimeSpan.FromTicks(1)));
        Assert.False(answered.IsMarkedAt(Ended + DeploymentsReport.EndedFor));
    }

    // ----- asking the two answers turns nothing

    /// <summary>The flight is a saying of the parts and not a state the deployment keeps: asked twice it answers
    /// twice the same, and asked at the ends of the clock leaves the deployment holding the very address, the very
    /// wait and the very moments it was built of - its hash, its words and its equals all standing where they stood.
    /// Two deployments that are one answer one at every moment, which is what lets the page mark a deployment it
    /// read from a file and one a test wrote of the same ten parts as the one thing.</summary>
    [Fact]
    public void AskingTheFlightAndTheMarkingTurnsNothing()
    {
        var task = new Uri($"{Tasks}1");
        var wait = new DeploymentWait(DeploymentWait.SignOff, "Sign-off", Ended, "demo approvers");
        var deployment = new Deployment("demo-ui", "uat", "2.4.43", DeploymentState.Waiting, "waiting", Since, null, task, "jeffrey", wait);
        var twin = new Deployment("demo-ui", "uat", "2.4.43", DeploymentState.Waiting, "waiting", Since, null, new Uri($"{Tasks}1"), "jeffrey", new DeploymentWait(DeploymentWait.SignOff, "Sign-off", Ended, "demo approvers"));

        Assert.Equal(deployment, twin);
        Assert.NotSame(deployment.Url, twin.Url);
        Assert.NotSame(deployment.WaitsFor, twin.WaitsFor);

        var words = Words(deployment);
        var hash = deployment.GetHashCode();

        DateTimeOffset[] moments =
        [
            DateTimeOffset.MinValue,
            Ended.AddYears(-50),
            Since,
            Now,
            Ended + DeploymentsReport.EndedFor,
            DateTimeOffset.MaxValue,
        ];

        foreach (var moment in moments)
        {
            Assert.True(deployment.IsMarkedAt(moment));
            Assert.Equal(deployment.IsMarkedAt(moment), twin.IsMarkedAt(moment));
            Assert.True(deployment.InFlight);
        }

        Assert.Equal(hash, deployment.GetHashCode());
        Assert.Equal(hash, twin.GetHashCode());
        Assert.Equal(words, Words(deployment));
        Assert.Equal(words, Words(twin));
        Assert.Same(task, deployment.Url);
        Assert.Same(wait, deployment.WaitsFor);
        Assert.Equal(Since, deployment.Since);
        Assert.Null(deployment.Finished);
        Assert.True(deployment == twin);
        Assert.Single(new HashSet<Deployment> { deployment, twin });

        // A copy that ends the deployment answers another thing; the one asked keeps its flight.
        var turned = deployment with { State = DeploymentState.Failed, StateText = "failed", Finished = Ended };

        Assert.False(turned.InFlight);
        Assert.True(deployment.InFlight);
        Assert.NotEqual(deployment, turned);
        Assert.NotEqual(hash, turned.GetHashCode());
    }

    /// <summary>The smallest deployment the page can be given - the eight parts a file must give, no moment to
    /// either of its two, no address, nobody and nothing to act on - still flies on its state and marks on that
    /// flight alone: names, moments and addresses have nothing to add to either answer, and nothing to take away.</summary>
    [Fact]
    public void ADeploymentOfNothingBesideItsNamesStillFliesAndMarks()
    {
        var bare = new Deployment("demo-ui", "uat", "1", DeploymentState.Executing, "executing", null, null, null);

        Assert.Null(bare.Since);
        Assert.Null(bare.Finished);
        Assert.Null(bare.Url);
        Assert.Null(bare.StartedBy);
        Assert.Null(bare.WaitsFor);
        Assert.True(bare.InFlight);
        Assert.True(bare.IsMarkedAt(Ended));
        Assert.True(bare.IsMarkedAt(Ended.AddYears(50)));
        Assert.True(bare.IsMarkedAt(DateTimeOffset.MinValue));
        Assert.Equal(bare, bare with { });
        Assert.Single(new HashSet<Deployment> { bare, new Deployment("demo-ui", "uat", "1", DeploymentState.Executing, "executing", null, null, null) });

        // The same eight parts with an end they do not belong to: the state still flies, and the end changes nothing
        // of the answer - it is the state that has not ended, and not the moment the file also wrote.
        var told = new Deployment("demo-ui", "uat", "1", DeploymentState.Executing, "executing", null, Ended, null);

        Assert.NotEqual(bare, told);
        Assert.True(told.InFlight);
        Assert.True(told.IsMarkedAt(DateTimeOffset.MaxValue));
        Assert.False(told.Equals(bare));
    }
}
