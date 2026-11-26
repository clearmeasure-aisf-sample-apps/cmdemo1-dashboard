using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Dashboard.Tests;

/// <summary>
/// <see cref="Deployment" /> is one entry of the deployments file's <c>deployments</c>: a task of the system's
/// Octopus space, in ten parts - the project, the environment, the release, the state as the page knows it and as the
/// file wrote it, the moment it started or was queued, the moment it ended, the task in Octopus Deploy, who started
/// it, and what it waits for while it waits. Two of those ten the file may leave out, and the record itself says one
/// more thing of its own: whether the deployment still stands in flight, and whether the page marks it at a moment.
/// <para>
/// What is pinned where it belongs, and is not pinned again here: <see cref="DeploymentsTests" /> reads the sample and
/// a file written for each state - so the eight parts a file must give land on the properties of their names, an entry
/// without one of them falls out, a time that does not parse is no time, an address the page may not link to is no
/// address, the six states and the word for one the page does not know, <c>startedBy</c> and <c>waitsFor</c> missing
/// from a file being nobody and nothing, the ten minutes at both of their whole-second ends and a clock that runs
/// behind, an in-flight deployment marked however long it takes and one that ended at a time the page cannot read not
/// marked at all, and the flight as the marks carry it, state by state; <see cref="DeploymentsReportEdgeCaseTests" />
/// pins the report the entries are handed to, a deployment built by hand with no environment at all, the marks of an
/// environment at either end of the clock and the deployables walked once;
/// <see cref="HealthHelperCoverageTests" /> and <see cref="DeploymentActivityTests" /> pin the words the page makes of
/// the parts, and <see cref="DeploymentWaitEdgeCaseTests" /> and <see cref="RecentDeploymentEdgeCaseTests" /> the wait
/// and the entry of <c>recent</c> beside them.
/// </para>
/// <para>
/// Left open by all of that, and pinned here: the record's own ten parts, which no test has ever built by hand with
/// all ten - each argument landing on the property of its name with the very <see cref="Uri" /> and the very
/// <see cref="DeploymentWait" /> held and asked nothing of; the two parts the constructor gives up on standing in as
/// nothing and never as the empty word; the five words kept letter for letter, padded, capitalized, empty and
/// nothing, with no trimming and no casing of any kind - which is why the marks compare an environment by ignoring
/// case and this record never decides anything; the state and the word of it as the two parts they are, the seven
/// names apart and a state beyond every name kept as the number it is; the two moments as the instants and the hours
/// they are written in, at the ends the clock runs to and to the tick; the ten parts coming back out in the order
/// they went in; two deployments being one while their ten parts are, one instant spelled in another hour being one
/// deployment, and the hash following the equality; the nothing on either side of the members every record is asked
/// with; the copy that turns the part it names; the words the record says - its ten parts and the flight it computes,
/// and none of the page's sentences; and the flight and the marking themselves, said of the record rather than of a
/// mark: that the three states that have not ended fly whether or not the file also gives them an end, that the
/// states that ended do not fly without one, that the ten minutes close one tick after they are full and not one tick
/// before, that a moment asked in another hour is the same moment, and that no name, no release and no address has
/// anything to say about either.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: the record holds a task's address and
/// two moments but calls on neither - every moment below is one the test brought, and the address is held and never
/// fetched. What a mark is made of, and what a page does with it, is <see cref="DeploymentMark" /> and
/// <see cref="DeploymentsReport.Marks" />, pinned there.
/// </para>
/// </summary>
public class DeploymentEdgeCaseTests
{
    /// <summary>Thursday a few minutes past four, UTC: the moment the page is asked.</summary>
    private static readonly DateTimeOffset Now = new(2026, 10, 8, 4, 23, 24, TimeSpan.Zero);

    /// <summary>The moment the sample's <c>demo-ui 2.4.43</c> started: twenty minutes past four, UTC.</summary>
    private static readonly DateTimeOffset Since = new(2026, 10, 8, 4, 20, 7, TimeSpan.Zero);

    /// <summary>Five minutes before the page is asked: the moment a deployment that ended ended.</summary>
    private static readonly DateTimeOffset Ended = new(2026, 10, 8, 4, 18, 0, TimeSpan.Zero);

    /// <summary>The hours five and a half east of the meridian, to spell one instant the other way round.</summary>
    private static readonly TimeSpan East = TimeSpan.FromMinutes(330);

    private const string Tasks = "https://octopus.example.net/app#/Spaces-356/tasks/ServerTasks-";

    /// <summary>A sign-off whose title says no more than the kind does, asked at the minute the sample ended.</summary>
    private static readonly DeploymentWait SignOff = new(DeploymentWait.SignOff, "Sign-off", Ended, "demo approvers");

    /// <summary>Every part a file may give a deployment, brought here without reading one.</summary>
    private static Deployment Full() =>
        Deployments.InState(DeploymentState.Executing, since: Since, stateText: "executing", release: "2.4.43", url: new Uri($"{Tasks}1"), startedBy: "jeffrey", waitsFor: SignOff);

    /// <summary>A deployment that names nobody and nowhere, in a state the page does not know: the eight arguments a
    /// file must give and nothing beside them.</summary>
    private static Deployment Bare() => Deployments.Bare();

    /// <summary>One deployment of a state and an end, of nothing else: the flight and the marking ask no more.</summary>
    private static Deployment Of(DeploymentState state, DateTimeOffset? finished) =>
        Deployments.InState(state, finished, since: Since, stateText: "as the file wrote it");

    // ----- the constructor, handing each argument to the property of its name

    /// <summary>The constructor's ten arguments, position by position, come back as the property of the same name, and
    /// the deployment holds the very address and the very wait it was built of - never a copy, never a question asked
    /// of either. A deployment built again of the same ten parts is the same deployment and no longer the same
    /// reference, which is what lets a test name a deployment read from a file by writing it out.</summary>
    [Fact]
    public void TheConstructorHandsEachArgumentToThePropertyOfItsName()
    {
        var task = new Uri($"{Tasks}1");
        var wait = new DeploymentWait(DeploymentWait.SignOff, "Sign-off", Ended, "demo approvers");
        var deployment = new Deployment("demo-ui", "uat", "2.4.43", DeploymentState.Waiting, "waiting", Since, Ended, task, "jeffrey", wait);

        Assert.Equal("demo-ui", deployment.Project);
        Assert.Equal("uat", deployment.Environment);
        Assert.Equal("2.4.43", deployment.Release);
        Assert.Equal(DeploymentState.Waiting, deployment.State);
        Assert.Equal("waiting", deployment.StateText);
        Assert.Equal(Since, deployment.Since);
        Assert.Equal(Ended, deployment.Finished);
        Assert.Same(task, deployment.Url);
        Assert.Equal("jeffrey", deployment.StartedBy);
        Assert.Same(wait, deployment.WaitsFor);
        Assert.True(deployment.InFlight);

        // The word the file wrote and the state the page makes of it are two parts, and one does not follow the other.
        Assert.NotEqual(deployment.StateText, deployment.State.ToString());

        var again = new Deployment("demo-ui", "uat", "2.4.43", DeploymentState.Waiting, "waiting", Since, Ended, new Uri($"{Tasks}1"), "jeffrey", new DeploymentWait(DeploymentWait.SignOff, "Sign-off", Ended, "demo approvers"));

        Assert.Equal(deployment, again);
        Assert.NotSame(deployment, again);
        Assert.Equal(deployment.GetHashCode(), again.GetHashCode());
    }

    /// <summary>Who started a deployment and what it waits for are the two parts it does without: left to the
    /// constructor they are nothing, and never the empty word or a wait of no kind - which is why the page says
    /// nothing of who started what and leaves a mark's second line out altogether.</summary>
    [Fact]
    public void TheTwoPartsADeploymentMayDoWithoutAreNothing()
    {
        var without = Of(DeploymentState.Waiting, null);

        Assert.Null(without.StartedBy);
        Assert.Null(without.WaitsFor);
        Assert.Null(Bare().StartedBy);
        Assert.Null(Bare().WaitsFor);
        Assert.Equal(Bare(), new Deployment("demo-ui", "uat", "1", DeploymentState.Unknown, "paused", null, null, null));
        Assert.Equal(Bare().GetHashCode(), new Deployment("demo-ui", "uat", "1", DeploymentState.Unknown, "paused", null, null, null, null, null).GetHashCode());

        Assert.NotEqual(without, without with { StartedBy = string.Empty });
        Assert.NotEqual(without, without with { StartedBy = " " });
        Assert.NotEqual(without, without with { WaitsFor = new DeploymentWait(string.Empty, null, null, null) });
        Assert.NotEqual(Bare(), Bare() with { WaitsFor = new DeploymentWait(DeploymentWait.SignOff, null, null, null) });

        var withBoth = Full();

        Assert.NotEqual(withBoth, withBoth with { StartedBy = null });
        Assert.NotEqual(withBoth, withBoth with { StartedBy = "jeffrey " });
        Assert.NotEqual(withBoth, withBoth with { WaitsFor = null });
    }

    // ----- the five words, kept exactly as they are given

    /// <summary>Every word a deployment carries - the project, the environment, the release, the state in the file's
    /// own word and who started it - is kept as letters and nothing more: the empty word is a word, a word of nothing
    /// but whitespace is a word of its own, and two of the same letters in the same places are one deployment.</summary>
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("  ")]
    [InlineData(" demo-ui ")]
    [InlineData("DEMO-UI")]
    [InlineData("demo-ui")]
    public void EachWordIsKeptExactlyAsItIsGiven(string word)
    {
        var deployment = new Deployment(word, word, word, DeploymentState.Executing, word, Since, null, null, word, null);

        Assert.Equal(word, deployment.Project);
        Assert.Equal(word, deployment.Environment);
        Assert.Equal(word, deployment.Release);
        Assert.Equal(word, deployment.StateText);
        Assert.Equal(word, deployment.StartedBy);

        var same = new Deployment(word, word, word, DeploymentState.Executing, word, Since, null, null, word, null);

        Assert.Equal(deployment, same);
        Assert.Equal(deployment.GetHashCode(), same.GetHashCode());
        Assert.True(deployment == same);
        Assert.False(deployment != same);
        Assert.Single(new HashSet<Deployment> { deployment, same });
    }

    /// <summary>Trimming, casing and leaving a word out are the reading's and not the record's: a deployment padded is
    /// no deployment trimmed, one in another case is no deployment in this, and a word said empty is a word said -
    /// which is why the marks compare an environment by ignoring case while this record keeps the letters it is
    /// given. The names a file cannot leave out the record does not ask for either: nothing, the empty word and a word
    /// of spaces are three deployments, and the record faults nobody for them.</summary>
    [Fact]
    public void TheDeploymentTrimsNothingCasesNothingAndRefusesNoName()
    {
        var padded = new Deployment(" demo-ui ", " uat ", " 2.4.43 ", DeploymentState.Succeeded, " Succeeded ", Since, Ended, null, " ai-ops ", null);

        Assert.NotEqual(padded, padded with { Project = "demo-ui" });
        Assert.NotEqual(padded, padded with { Environment = "uat" });
        Assert.NotEqual(padded, padded with { Release = "2.4.43" });
        Assert.NotEqual(padded, padded with { StateText = "Succeeded" });
        Assert.NotEqual(padded, padded with { StateText = " Succeeded" });
        Assert.NotEqual(padded, padded with { StartedBy = "ai-ops " });
        Assert.Equal(padded, padded with { Project = " demo-ui " });

        var nowhere = new Deployment(null!, null!, null!, DeploymentState.Unknown, null!, null, null, null);

        Assert.Null(nowhere.Project);
        Assert.Null(nowhere.Environment);
        Assert.Null(nowhere.Release);
        Assert.Null(nowhere.StateText);
        Assert.Null(nowhere.StartedBy);
        Assert.True(nowhere.InFlight);

        Assert.Equal(nowhere, new Deployment(null!, null!, null!, DeploymentState.Unknown, null!, null, null, null));
        Assert.NotEqual(nowhere, nowhere with { Project = string.Empty });
        Assert.NotEqual(nowhere with { Project = string.Empty }, nowhere with { Project = " " });

        // Nothing a file must give is the same deployment as that very name written out: only the moments, the task,
        // the person and the wait differ, and the record has nothing to say about which of the two came from a file.
        Assert.Equal(Bare(), nowhere with { Project = "demo-ui", Environment = "uat", Release = "1", StateText = "paused" });
        Assert.NotEqual(Bare(), nowhere with { Project = "demo-ui", Environment = "uat", Release = "1" });
    }

    // ----- the state, and the word the file wrote beside it

    /// <summary>The state stands as it is given and the word of it stays a part of its own: a deployment waiting under
    /// a word that says <c>executing</c> waits, and one failed under a word capitalized is failed - the record ties
    /// nothing to nothing, and it is the reading that settles a word the page does not know on
    /// <see cref="DeploymentState.Unknown" />.</summary>
    [Theory]
    [InlineData(DeploymentState.Unknown, "paused")]
    [InlineData(DeploymentState.Queued, "waiting")]
    [InlineData(DeploymentState.Executing, "queued")]
    [InlineData(DeploymentState.Waiting, "succeeded")]
    [InlineData(DeploymentState.Succeeded, "canceled")]
    [InlineData(DeploymentState.Failed, "abandoned")]
    [InlineData(DeploymentState.Canceled, "Failed")]
    public void TheStateAndTheWordOfItAreTwoParts(DeploymentState state, string word)
    {
        var deployment = Full() with { State = state, StateText = word };

        Assert.Equal(state, deployment.State);
        Assert.Equal(word, deployment.StateText);
        Assert.Equal("2.4.43", deployment.Release);
        Assert.Equal(deployment, Full() with { State = state, StateText = word });
        Assert.Equal(deployment.GetHashCode(), (Full() with { State = state, StateText = word }).GetHashCode());

        // The word is no state and the state no word: neither is read out of the other, and a word that says another
        // thing than the state does is kept as the letters it was given.
        Assert.NotEqual(deployment, Full() with { State = state, StateText = state.ToString().ToLowerInvariant() });
        Assert.NotEqual(word, state.ToString().ToLowerInvariant());

        // It is the state the flight turns on and nothing else: the same state under the word of its own flies the
        // same as under the word of another, which is what lets a task Octopus Deploy renames stay marked.
        Assert.Equal(deployment.InFlight, (Full() with { State = state, StateText = state.ToString().ToLowerInvariant() }).InFlight);
    }

    /// <summary>The seven states are seven deployments and the record does not stop at the last of them: a state beyond
    /// every name the page has is kept as the number it is and said as that number, told apart from its neighbours -
    /// and not in flight, since the flight is decided on the names and everything the record does not name has
    /// ended.</summary>
    [Fact]
    public void EveryStateIsADifferentDeployment()
    {
        var states = new List<DeploymentState>(Enum.GetValues<DeploymentState>()) { (DeploymentState)7 };
        var deployments = states.Select(state => Full() with { State = state }).ToArray();

        Assert.Equal(8, states.Count);
        Assert.Equal(8, deployments.Length);
        Assert.Equal(8, new HashSet<Deployment>(deployments).Count);

        for (var first = 0; first < deployments.Length; first++)
        {
            for (var second = first + 1; second < deployments.Length; second++)
            {
                Assert.NotEqual(deployments[first], deployments[second]);
                Assert.True(deployments[first] != deployments[second]);
            }
        }

        var beyond = Full() with { State = (DeploymentState)7 };

        Assert.Equal((DeploymentState)7, beyond.State);
        Assert.False(Enum.IsDefined(beyond.State));
        Assert.False(beyond.InFlight);
        Assert.Contains("State = 7, ", Words(beyond), StringComparison.Ordinal);
        Assert.NotEqual(Full() with { State = DeploymentState.Unknown }, beyond);
    }

    // ----- the two moments, kept as the instants and the hours they are written in

    /// <summary>The moment it started and the moment it ended are kept whole, at the ends the clock runs to and to the
    /// tick: neither end is the other, a millisecond is no rounding, and a moment the file does not give is nothing
    /// rather than the moment of the reading - which is what leaves a mark without an age and a deployment that ended
    /// unmarked.</summary>
    [Fact]
    public void TheTwoMomentsAreKeptAtTheEndsTheClockRunsTo()
    {
        var ends = Full() with { Since = DateTimeOffset.MinValue, Finished = DateTimeOffset.MaxValue };

        Assert.Equal(DateTimeOffset.MinValue, ends.Since);
        Assert.Equal(DateTimeOffset.MaxValue, ends.Finished);
        Assert.NotEqual(Full(), ends);
        Assert.True(ends.Since!.Value < ends.Finished!.Value);

        var spelled = Full() with { Since = Since.ToOffset(East) };

        Assert.Equal(East, spelled.Since!.Value.Offset);
        Assert.Equal(Since, spelled.Since!.Value.ToUniversalTime());
        Assert.NotEqual(Since.DateTime, spelled.Since!.Value.DateTime);
        Assert.Equal(Full(), spelled);

        var exact = Full() with { Finished = new DateTimeOffset(2026, 10, 8, 4, 18, 0, 123, TimeSpan.Zero) };

        Assert.Equal(123, exact.Finished!.Value.Millisecond);
        Assert.NotEqual(Full() with { Finished = Ended }, exact);

        // One tick on is another deployment, in either of the two moments.
        Assert.NotEqual(Full(), Full() with { Since = Since.AddTicks(1) });
        Assert.NotEqual(Full() with { Finished = Ended }, Full() with { Finished = Ended.AddTicks(1) });
        Assert.Equal(3, new HashSet<Deployment> { Full(), Full() with { Since = Since.AddTicks(1) }, Full() with { Finished = Ended } }.Count);

        // A moment given as nothing is nothing, and not the instant the record was built at.
        Assert.Null(Bare().Since);
        Assert.Null(Bare().Finished);
        Assert.NotNull(Full().Since);
    }

    // ----- the address and the wait, held and never asked

    /// <summary>The task in Octopus Deploy is held as the very instance it was given and asked nothing of: no lookup,
    /// no request, and no refusal of an address the reading would never let through - a scheme other than http or
    /// https, and an address that is no address but a path - since it is the reading that decides what the page may
    /// link to and not the record. One address spelled out again is one deployment; another shape, another place and
    /// no address at all are each another.</summary>
    [Fact]
    public void TheAddressIsHeldAsTheAddressItWasGivenAndAskedNothingOfItsShape()
    {
        var task = new Uri($"{Tasks}1");
        var deployment = Bare() with { Url = task };

        Assert.Same(task, deployment.Url);
        Assert.True(deployment.Url!.IsAbsoluteUri);
        Assert.Equal("https", deployment.Url.Scheme);
        Assert.Equal(deployment, Bare() with { Url = new Uri($"{Tasks}1") });
        Assert.Equal(deployment.GetHashCode(), (Bare() with { Url = new Uri($"{Tasks}1") }).GetHashCode());
        Assert.NotEqual(deployment, Bare());

        // Another address is another deployment, whatever its shape: another path in another space of the same
        // instance, a scheme the page would never link to, and an address that is no address but a path.
        Assert.NotEqual(deployment, Bare() with { Url = new Uri("https://octopus.example.net/spaces/356/tasks/ServerTasks-1") });
        Assert.NotEqual(deployment, Bare() with { Url = new Uri("ftp://octopus.example.net/task/1") });
        Assert.NotEqual(deployment, Bare() with { Url = new Uri("ServerTasks-1", UriKind.Relative) });

        // A host nothing could ever answer for is held host and all, and said as it was given.
        var far = Bare() with { Url = new Uri("https://octopus.invalid/app#/task/1") };

        Assert.Equal("octopus.invalid", far.Url!.DnsSafeHost);
        Assert.Contains("Url = https://octopus.invalid/app#/task/1", Words(far), StringComparison.Ordinal);
    }

    /// <summary>The wait is held as the instance it was given and compared as the wait it is: one held by two
    /// deployments is one pair of deployments, a wait spelled out again part for part is the same wait, and a
    /// deployment that waits for nobody is another deployment than one that waits for someone. The record asks the
    /// wait nothing - no kind, no title and no age is looked at here - which is the saying of what a person has to act
    /// on and is <see cref="DeploymentText.Detail" />.</summary>
    [Fact]
    public void TheWaitIsHeldAndAskedNothing()
    {
        var waiting = Bare() with { State = DeploymentState.Waiting, StateText = "waiting", WaitsFor = SignOff };

        Assert.Same(SignOff, waiting.WaitsFor);
        Assert.Equal(waiting, Bare() with { State = DeploymentState.Waiting, StateText = "waiting", WaitsFor = new DeploymentWait(DeploymentWait.SignOff, "Sign-off", Ended, "demo approvers") });
        Assert.Equal(waiting.GetHashCode(), (Bare() with { State = DeploymentState.Waiting, StateText = "waiting", WaitsFor = new DeploymentWait(DeploymentWait.SignOff, "Sign-off", Ended, "demo approvers") }).GetHashCode());
        Assert.Single(new HashSet<Deployment> { waiting, Bare() with { State = DeploymentState.Waiting, StateText = "waiting", WaitsFor = SignOff } });

        Assert.NotEqual(waiting, waiting with { WaitsFor = new DeploymentWait(DeploymentWait.SignOff, "Sign-off", Ended, "platform") });
        Assert.NotEqual(waiting, waiting with { WaitsFor = new DeploymentWait(DeploymentWait.GuidedFailure, "Sign-off", Ended, "demo approvers") });
        Assert.NotEqual(waiting, waiting with { WaitsFor = null });
        Assert.Equal(2, new HashSet<Deployment> { waiting, waiting with { WaitsFor = null } }.Count);

        // A wait of nothing at all is still a wait, and another deployment than no wait.
        var empty = Bare() with { WaitsFor = new DeploymentWait(null!, null, null, null) };

        Assert.NotNull(empty.WaitsFor);
        Assert.Null(empty.WaitsFor!.Kind);
        Assert.NotEqual(Bare(), empty);
        Assert.Contains("WaitsFor = DeploymentWait { Kind = , Title = , Since = , Responsible = , ", Words(empty), StringComparison.Ordinal);
    }

    // ----- the flight, which is the record's own saying

    /// <summary>Whether a deployment still stands in flight is the state and the end the file gives it, and nothing
    /// else: the three states that have not ended fly whether or not the file also hands them an end - a task Octopus
    /// Deploy had stopped a moment ago keeps its <c>finished</c> while it queues again - while the three that ended do
    /// not fly even with no end to show for it, a state the page does not know flies only while the file gives it no
    /// end, and a state beyond every name the page has is none of the three that fly. The marks carry this saying
    /// part for part, which is where the file is read; what is pinned here is the property itself, on a deployment
    /// nobody read from a file.</summary>
    [Theory]
    [InlineData(DeploymentState.Queued, false, true)]
    [InlineData(DeploymentState.Queued, true, true)]
    [InlineData(DeploymentState.Executing, false, true)]
    [InlineData(DeploymentState.Executing, true, true)]
    [InlineData(DeploymentState.Waiting, false, true)]
    [InlineData(DeploymentState.Waiting, true, true)]
    [InlineData(DeploymentState.Succeeded, false, false)]
    [InlineData(DeploymentState.Succeeded, true, false)]
    [InlineData(DeploymentState.Failed, false, false)]
    [InlineData(DeploymentState.Failed, true, false)]
    [InlineData(DeploymentState.Canceled, false, false)]
    [InlineData(DeploymentState.Canceled, true, false)]
    [InlineData(DeploymentState.Unknown, false, true)]
    [InlineData(DeploymentState.Unknown, true, false)]
    [InlineData((DeploymentState)7, false, false)]
    [InlineData((DeploymentState)7, true, false)]
    public void TheFlightIsTheStateAndTheEndTheFileGives(DeploymentState state, bool withAnEnd, bool inFlight)
    {
        var deployment = Of(state, withAnEnd ? Ended : null);

        Assert.Equal(withAnEnd, deployment.Finished is not null);
        Assert.Equal(inFlight, deployment.InFlight);

        // The flight says nothing of the names, of the two moments or of who started what.
        var renamed = deployment with { Project = "other-system", Environment = "prod", Release = "9.9", Since = null, StartedBy = "jeffrey" };

        Assert.Equal(inFlight, renamed.InFlight);
    }

    /// <summary>The names a deployment carries decide nothing of whether it is marked: the same end under another
    /// project, another environment, another release, another word of a state and another address marks the same,
    /// and a deployment in flight is marked at every moment the page is asked - at the two ends of the clock, fifty
    /// years after its own start, and in whichever hour the moment is spelled.</summary>
    [Fact]
    public void WhatIsInFlightIsMarkedAtEveryMomentThePageIsAsked()
    {
        Deployment[] flying =
        [
            Of(DeploymentState.Queued, null),
            Of(DeploymentState.Executing, null),
            Of(DeploymentState.Waiting, Ended),
            Of(DeploymentState.Unknown, null),
        ];

        Assert.All(flying, deployment => Assert.True(deployment.InFlight));
        Assert.All(flying, deployment => Assert.True(deployment.IsMarkedAt(Now)));
        Assert.All(flying, deployment => Assert.True(deployment.IsMarkedAt(DateTimeOffset.MinValue)));
        Assert.All(flying, deployment => Assert.True(deployment.IsMarkedAt(DateTimeOffset.MaxValue)));
        Assert.All(flying, deployment => Assert.True(deployment.IsMarkedAt(Ended.AddYears(50))));
        Assert.All(flying, deployment => Assert.True(deployment.IsMarkedAt(Ended.AddYears(-50))));
        Assert.All(flying, deployment => Assert.True(deployment.IsMarkedAt(Now.ToOffset(East))));

        // Nothing of the marking turns on the names: the very same end, differently named, marks the same.
        var named = new Deployment("DEMO-UI", "UAT", "9.9", DeploymentState.Executing, "Executing", null, null, new Uri($"{Tasks}9"), "jeffrey", SignOff);

        Assert.True(named.IsMarkedAt(Ended.AddYears(-50)));
        Assert.True(named.IsMarkedAt(DateTimeOffset.MinValue));
    }

    /// <summary>The ten minutes an ended deployment stays marked close on the instant the tenth is full: a tick before
    /// the end of the window it is still marked, at the end of it it is not, and the same stands a millisecond either
    /// side. The window is one length for every state that has ended, the page's own six and a state beyond them
    /// alike, and it is the length <see cref="DeploymentsReport.EndedFor" /> names - pinned as the fixed value it is
    /// where the report is read, and walked here to the tick, which the reading of a file never gives.</summary>
    [Fact]
    public void TheTenMinutesAreFullOneTickAfterTheyAreAndNotOneTickBefore()
    {
        Deployment[] ended =
        [
            Of(DeploymentState.Succeeded, Ended),
            Of(DeploymentState.Failed, Ended),
            Of(DeploymentState.Canceled, Ended),
            Of(DeploymentState.Unknown, Ended),
            Of((DeploymentState)7, Ended),
        ];

        Assert.All(ended, deployment => Assert.False(deployment.InFlight));
        Assert.All(ended, deployment => Assert.True(deployment.IsMarkedAt(Ended + DeploymentsReport.EndedFor - TimeSpan.FromTicks(1))));
        Assert.All(ended, deployment => Assert.False(deployment.IsMarkedAt(Ended + DeploymentsReport.EndedFor)));
        Assert.All(ended, deployment => Assert.True(deployment.IsMarkedAt(Ended + DeploymentsReport.EndedFor - TimeSpan.FromMilliseconds(1))));
        Assert.All(ended, deployment => Assert.False(deployment.IsMarkedAt(Ended + DeploymentsReport.EndedFor + TimeSpan.FromMilliseconds(1))));
        Assert.All(ended, deployment => Assert.True(deployment.IsMarkedAt(Ended)));
        Assert.All(ended, deployment => Assert.True(deployment.IsMarkedAt(Ended.AddMilliseconds(-1))));
    }

    /// <summary>The seconds the window is counted in, at its ends and past them: nine minutes on it marks, ten minutes
    /// and a second does not, and a moment behind the end - a browser whose clock runs behind the system's, so that
    /// the task ends "ahead" of now - marks, since the window is counted as a difference and nothing refuses an end
    /// the page has not reached yet.</summary>
    [Theory]
    [InlineData(-86_400, true)]
    [InlineData(-1, true)]
    [InlineData(0, true)]
    [InlineData(1, true)]
    [InlineData(540, true)]
    [InlineData(60 * 10 - 1, true)]
    [InlineData(60 * 10, false)]
    [InlineData(60 * 10 + 1, false)]
    [InlineData(3_600, false)]
    [InlineData(86_400, false)]
    public void WhatEndedIsMarkedUntilTheTenMinutesAreFull(int seconds, bool marked) =>
        Assert.Equal(marked, Of(DeploymentState.Failed, Ended).IsMarkedAt(Ended.AddSeconds(seconds)));

    /// <summary>The window stands on the instant of the end and not on the hours it was spelled in: a deployment whose
    /// end reads <c>09:48:00 +05:30</c> is the deployment of <c>04:18:00 +00:00</c>, marks at the same moments and
    /// stops marking at the same one - while the hours it was written in stay its own, for whoever prints them.</summary>
    [Fact]
    public void TheWindowIsCountedFromTheInstantOfTheEndAndNotFromTheHoursItIsSpelledIn()
    {
        var utc = Of(DeploymentState.Failed, Ended);
        var east = utc with { Since = Since.ToOffset(East), Finished = Ended.ToOffset(East) };

        Assert.Equal(East, east.Finished!.Value.Offset);
        Assert.Equal(Ended, east.Finished!.Value.ToUniversalTime());
        Assert.Equal(utc, east);
        Assert.True(utc == east);
        Assert.Equal(utc.GetHashCode(), east.GetHashCode());

        Assert.True(east.IsMarkedAt(Ended + TimeSpan.FromMinutes(9)));
        Assert.True(east.IsMarkedAt(Ended + DeploymentsReport.EndedFor - TimeSpan.FromTicks(1)));
        Assert.False(east.IsMarkedAt(Ended + DeploymentsReport.EndedFor));
        Assert.True(east.IsMarkedAt(Ended.AddSeconds(-1).ToOffset(East)));
        Assert.Equal(utc.IsMarkedAt(Ended + DeploymentsReport.EndedFor), east.IsMarkedAt(Ended + DeploymentsReport.EndedFor));

        Assert.Contains("Finished = 10/08/2026 09:48:00 +05:30", Words(east), StringComparison.Ordinal);
        Assert.DoesNotContain("+05:30", Words(utc), StringComparison.Ordinal);
    }

    /// <summary>What the file gives no end at all marks nothing, however the state is named and whenever the page is
    /// asked: of the states that ended, and of a state beyond every name, a deployment with no end has no window to
    /// stand in, since the length is counted from an end that is not there - and the moment it started does not stand
    /// in for it, which is why the page says no age rather than guessing one.</summary>
    [Fact]
    public void WhatEndedAtNoTimeIsMarkedAtNoTime()
    {
        Deployment[] stranded =
        [
            Of(DeploymentState.Failed, null),
            Of(DeploymentState.Canceled, null),
            Of((DeploymentState)7, null),
        ];

        Assert.All(stranded, deployment => Assert.False(deployment.InFlight));
        Assert.All(stranded, deployment => Assert.False(deployment.IsMarkedAt(Now)));
        Assert.All(stranded, deployment => Assert.False(deployment.IsMarkedAt(Since)));
        Assert.All(stranded, deployment => Assert.False(deployment.IsMarkedAt(DateTimeOffset.MinValue)));
        Assert.All(stranded, deployment => Assert.False(deployment.IsMarkedAt(DateTimeOffset.MaxValue)));

        // With an end the same three deployments mark: the missing moment is the whole of the difference.
        Assert.True(Of(DeploymentState.Failed, Ended).IsMarkedAt(Now));
        Assert.True(Of(DeploymentState.Canceled, Ended).IsMarkedAt(Now));
        Assert.True(Of((DeploymentState)7, Ended).IsMarkedAt(Now));

        // A deployment of no moment at all, in a state the page does not know, flies and so marks.
        var nowhere = Of(DeploymentState.Unknown, null) with { Since = null };

        Assert.Null(nowhere.Since);
        Assert.Null(nowhere.Finished);
        Assert.True(nowhere.InFlight);
        Assert.True(nowhere.IsMarkedAt(Now));
    }

    // ----- the ten parts coming back out, in the order they went in

    /// <summary>The ten parts of a deployment fall out in the order the constructor names them - the project, the
    /// environment, the release, the state, the word of it, the two moments, the task, who started it and what it
    /// waits for - as the very address and the very wait it was, and what came out goes back in the same way, to the
    /// same deployment. A deployment of nothing optional falls apart to the five parts a file must give.</summary>
    [Fact]
    public void TheDeploymentFallsApartInTheOrderItWasMade()
    {
        var task = new Uri($"{Tasks}1");
        var wait = new DeploymentWait(DeploymentWait.GuidedFailure, "Database is unreachable", Ended, "jeffrey");
        var deployment = new Deployment("demo-ui", "uat", "2.4.43", DeploymentState.Waiting, "waiting", Since, null, task, "jeffrey", wait);

        var (project, environment, release, state, stateText, since, finished, url, startedBy, waitsFor) = deployment;

        Assert.Equal("demo-ui", project);
        Assert.Equal("uat", environment);
        Assert.Equal("2.4.43", release);
        Assert.Equal(DeploymentState.Waiting, state);
        Assert.Equal("waiting", stateText);
        Assert.Equal(Since, since);
        Assert.Null(finished);
        Assert.Same(task, url);
        Assert.Equal("jeffrey", startedBy);
        Assert.Same(wait, waitsFor);
        Assert.Equal(deployment, new Deployment(project, environment, release, state, stateText, since, finished, url, startedBy, waitsFor));

        var (bareProject, bareEnvironment, bareRelease, bareState, bareStateText, bareSince, bareFinished, bareUrl, bareStartedBy, bareWaitsFor) = Bare();

        Assert.Equal("demo-ui", bareProject);
        Assert.Equal("uat", bareEnvironment);
        Assert.Equal("1", bareRelease);
        Assert.Equal(DeploymentState.Unknown, bareState);
        Assert.Equal("paused", bareStateText);
        Assert.Null(bareSince);
        Assert.Null(bareFinished);
        Assert.Null(bareUrl);
        Assert.Null(bareStartedBy);
        Assert.Null(bareWaitsFor);
        Assert.Equal(Bare(), new Deployment(bareProject, bareEnvironment, bareRelease, bareState, bareStateText, bareSince, bareFinished, bareUrl, bareStartedBy, bareWaitsFor));
    }

    // ----- the equality, the hash and the two operators

    /// <summary>Two deployments are one while their ten parts are - the five words by their letters, the state as the
    /// number it is, the two moments as the instants they stand on and not the hours they are spelled in, the address
    /// as an address and the wait as the wait it is - and the hash follows the equality, boxed or unboxed and from one
    /// call to the next, so that the deployments that are one are one deployment of a set and a lookup meets them.</summary>
    [Fact]
    public void TwoDeploymentsAreOneWhileTheirTenPartsAre()
    {
        var one = Full();
        var two = Full();

        Assert.True(one.Equals(two));
        Assert.True(one.Equals((object)two));
        Assert.True(one == two);
        Assert.True(two == one);
        Assert.False(one != two);
        Assert.Equal(one, two);
        Assert.NotSame(one, two);
        Assert.Equal(one.GetHashCode(), two.GetHashCode());
        Assert.Equal(one.GetHashCode(), ((object)one).GetHashCode());
        Assert.Equal(one.GetHashCode(), one.GetHashCode());
        Assert.Single(new HashSet<Deployment> { one, two });
        Assert.Contains(one, new HashSet<Deployment> { two });

        // The wait is compared as the wait it is, and not as the reference it stands in.
        var waiting = Bare() with { State = DeploymentState.Waiting, StateText = "waiting", WaitsFor = new DeploymentWait(DeploymentWait.SignOff, "Sign-off", Ended, "demo approvers") };

        Assert.True(waiting.Equals(Bare() with { State = DeploymentState.Waiting, StateText = "waiting", WaitsFor = SignOff }));
        Assert.Single(new HashSet<Deployment> { waiting, Bare() with { State = DeploymentState.Waiting, StateText = "waiting", WaitsFor = SignOff } });

        var keyed = new Dictionary<Deployment, string> { [one] = "in flight" };
        keyed[two] = "still in flight";

        Assert.Single(keyed);
        Assert.Equal("still in flight", keyed[Full()]);
    }

    /// <summary>A difference in one part is another deployment, and so is the same part in another place: the project
    /// of another deployable, the environment in another case, a release one number on, the state in another name, the
    /// word of that state capitalized, a moment one tick on, no moment where one was, another person, no person, a
    /// task of another instance and no task, another wait and no wait. A set of the deployments that differ in one
    /// part each holds them all apart, and the ten parts handed over in another order are ten other parts.</summary>
    [Fact]
    public void ADeploymentThatDiffersInOnePartIsAnotherDeployment()
    {
        var one = Full();

        Deployment[] different =
        [
            one with { Project = "demo-system" },
            one with { Project = "DEMO-UI" },
            one with { Project = string.Empty },
            one with { Environment = "prod" },
            one with { Environment = "UAT" },
            one with { Release = "2.4.44" },
            one with { State = DeploymentState.Queued },
            one with { State = DeploymentState.Unknown },
            one with { StateText = "Executing" },
            one with { StateText = "executing " },
            one with { Since = Since.AddTicks(1) },
            one with { Since = null },
            one with { Finished = Ended },
            one with { StartedBy = "ai-ops" },
            one with { StartedBy = null },
            one with { Url = new Uri("https://octopus.example.net/spaces/356/tasks/ServerTasks-1") },
            one with { Url = null },
            one with { WaitsFor = null },
            one with { WaitsFor = new DeploymentWait(DeploymentWait.GuidedFailure, "Sign-off", Ended, "demo approvers") },
        ];

        foreach (var other in different)
        {
            Assert.NotEqual(one, other);
            Assert.False(one.Equals(other));
            Assert.False(one.Equals((object)other));
            Assert.False(one == other);
            Assert.True(one != other);
        }

        Assert.Equal(1 + different.Length, new HashSet<Deployment>([one, .. different]).Count);

        // The environment is no project and the word of a state is no release: the parts stand in their places.
        Assert.NotEqual(one, new Deployment("uat", "demo-ui", "2.4.43", one.State, one.StateText, one.Since, one.Finished, one.Url, one.StartedBy, one.WaitsFor));
        Assert.NotEqual(one, new Deployment(one.Project, one.Environment, one.StateText, one.State, one.Release, one.Since, one.Finished, one.Url, one.StartedBy, one.WaitsFor));
    }

    /// <summary>The nothing on either side of the members every record is asked with: two deployments that are no
    /// deployment at all are one, a deployment and no deployment are two - the equality and the inequality each saying
    /// it, from either side - and the deployment asked about itself says no to the things it is not: a word, a state,
    /// a moment, an address, a wait of the same release, the entry of <c>recent</c> that carries the very same parts,
    /// a mark of this very deployment and the anonymous thing that happens to carry two of them.</summary>
    [Fact]
    public void NoDeploymentIsOneWithNoDeploymentAndNotWithAnyOtherThing()
    {
        Deployment? nothing = null;
        Deployment? alsoNothing = null;
        var there = Bare();

        Assert.Null(default(Deployment));
        Assert.True(nothing == alsoNothing);
        Assert.False(nothing != alsoNothing);
        Assert.False(nothing == there);
        Assert.True(nothing != there);
        Assert.False(there == (Deployment?)null);
        Assert.True(there != (Deployment?)null);
        Assert.False(there.Equals(nothing));
        Assert.False(there.Equals((object?)null));
        Assert.True(there.Equals(there));

        Assert.False(there.Equals("demo-ui"));
        Assert.False(there.Equals(DeploymentState.Unknown));
        Assert.False(there.Equals(Ended));
        Assert.False(there.Equals(new Uri($"{Tasks}1")));
        Assert.False(there.Equals(SignOff));
        Assert.False(there.Equals(new { Project = "demo-ui", Environment = "uat" }));
        Assert.False(there.Equals(Bare() with { Release = "2" }));
        Assert.False(there.Equals(Full()));
        Assert.False(there.Equals(new RecentDeployment("demo-ui", "uat", "1", DeploymentState.Unknown, "paused", Ended, null, null)));
        Assert.False(there.Equals(new DeploymentMark("demo-ui", null, "uat", "1", DeploymentState.Unknown, "demo-ui 1 in uat: paused", null, null, InFlight: true, OfSystem: false)));
    }

    // ----- the copy, turning the part it names

    /// <summary>Any of the ten parts may be turned as the deployment is copied - the environment to another one, the
    /// state to another name, a moment to the other end of the clock, the person, the task and the wait to nothing -
    /// and stands where it is turned to, while the parts the turn does not name stay the very values the deployment
    /// was built of, the address and the wait among them. The copy is another deployment and never the same reference,
    /// the original stands as it stood, and a copy that names nothing its own is the deployment it was copied from.</summary>
    [Fact]
    public void ADeploymentCopiedTurnsThePartItNamesAndKeepsTheRest()
    {
        var task = new Uri($"{Tasks}1");
        var deployment = new Deployment("demo-ui", "uat", "2.4.43", DeploymentState.Waiting, "waiting", Since, null, task, "jeffrey", SignOff);

        var moved = deployment with { Environment = "prod" };

        Assert.Equal("prod", moved.Environment);
        Assert.Equal("demo-ui", moved.Project);
        Assert.Equal("2.4.43", moved.Release);
        Assert.Equal(DeploymentState.Waiting, moved.State);
        Assert.Equal("waiting", moved.StateText);
        Assert.Equal(Since, moved.Since);
        Assert.Null(moved.Finished);
        Assert.Same(task, moved.Url);
        Assert.Equal("jeffrey", moved.StartedBy);
        Assert.Same(SignOff, moved.WaitsFor);
        Assert.NotSame(deployment, moved);
        Assert.NotEqual(deployment, moved);

        var ended = deployment with { State = DeploymentState.Failed, StateText = "failed", Finished = Ended };

        Assert.Equal(DeploymentState.Failed, ended.State);
        Assert.Equal("failed", ended.StateText);
        Assert.Equal(Ended, ended.Finished);
        Assert.False(ended.InFlight);
        Assert.True(deployment.InFlight);
        Assert.Null(deployment.Finished);

        var silent = deployment with { StartedBy = null, WaitsFor = null };

        Assert.Null(silent.StartedBy);
        Assert.Null(silent.WaitsFor);
        Assert.Equal(Bare() with { State = DeploymentState.Waiting, StateText = "waiting", Release = "2.4.43", Since = Since, Url = task }, silent);
        Assert.NotEqual(deployment, silent);

        var unchanged = deployment with { };

        Assert.Equal(deployment, unchanged);
        Assert.NotSame(deployment, unchanged);
        Assert.Equal(deployment.GetHashCode(), unchanged.GetHashCode());
        Assert.Same(task, unchanged.Url);
        Assert.Same(SignOff, unchanged.WaitsFor);

        Assert.Equal("uat", deployment.Environment);
        Assert.Same(task, deployment.Url);
    }

    // ----- the words the record says itself

    /// <summary>The words the deployment says its ten parts in: its own name at the front, the parts in the order the
    /// constructor names them, the state as the name the page gives it, the two moments as the clock of whoever reads
    /// them says them, the wait opened into its own words - and the flight it computes said last of all, as the
    /// eleventh part of the record's words although no part of a file. The part nobody gave is the empty place between
    /// the mark and the comma. These are the record's words and not the page's: no <c>deploying</c>, no
    /// <c>reached</c>, no <c>ago</c> and no sentence, which are what <see cref="DeploymentText" /> makes of the
    /// parts.</summary>
    [Fact]
    public void TheDeploymentSaysItsTenPartsAndTheFlightItComputes()
    {
        const string Spoken =
            "Deployment { Project = demo-ui, Environment = uat, Release = 2.4.43, State = Executing, "
                + "StateText = executing, Since = 10/08/2026 04:20:07 +00:00, Finished = , "
                + "Url = https://octopus.example.net/app#/Spaces-356/tasks/ServerTasks-1, StartedBy = jeffrey, "
                + "WaitsFor = DeploymentWait { Kind = sign-off, Title = Sign-off, Since = 10/08/2026 04:18:00 +00:00, "
                + "Responsible = demo approvers, IsSignOff = True, IsGuidedFailure = False, TitleIsTheKind = True }, "
                + "InFlight = True }";

        Assert.Equal(Spoken, Words(Full()));
        Assert.StartsWith("Deployment { Project = demo-ui, ", Words(Full()), StringComparison.Ordinal);
        Assert.EndsWith(", InFlight = True }", Words(Full()), StringComparison.Ordinal);

        // The two moments at the ends of the clock, and the five parts a file leaves out as the empty places.
        Assert.Equal(
            "Deployment { Project = demo-ui, Environment = uat, Release = 1, State = Unknown, StateText = paused, "
                + "Since = , Finished = , Url = , StartedBy = , WaitsFor = , InFlight = True }",
            Words(Bare()));
        Assert.Equal(
            "Deployment { Project = , Environment = , Release = , State = Unknown, StateText = , Since = , "
                + "Finished = , Url = , StartedBy = , WaitsFor = , InFlight = True }",
            Words(new Deployment(null!, null!, null!, DeploymentState.Unknown, null!, null, null, null)));
        Assert.Equal(
            "Deployment { Project = demo-ui, Environment = uat, Release = 1, State = Succeeded, StateText = succeeded, "
                + "Since = 01/01/0001 00:00:00 +00:00, Finished = 12/31/9999 23:59:59 +00:00, Url = , StartedBy = , "
                + "WaitsFor = , InFlight = False }",
            Words(Bare() with { State = DeploymentState.Succeeded, StateText = "succeeded", Since = DateTimeOffset.MinValue, Finished = DateTimeOffset.MaxValue }));

        foreach (var word in new[] { "deploying", "reached", " ago", "min", "queued for", "waits for", "Not live", " is " })
        {
            Assert.DoesNotContain(word, Words(Bare()), StringComparison.Ordinal);
        }

        // The flight is said as the record computes it and not as the word beside it says it: a deployment the file
        // also gives an end keeps flying under a word of its own, and a state the page does not know flies only while
        // the end is not there.
        Assert.Contains("StateText = as the file wrote it", Words(Of(DeploymentState.Queued, Ended)), StringComparison.Ordinal);
        Assert.Contains("InFlight = True }", Words(Of(DeploymentState.Queued, Ended)), StringComparison.Ordinal);
        Assert.Contains("InFlight = False }", Words(Of(DeploymentState.Failed, Ended)), StringComparison.Ordinal);
        Assert.Contains("InFlight = False }", Words(Of(DeploymentState.Failed, null)), StringComparison.Ordinal);
        Assert.Contains("InFlight = True }", Words(Of(DeploymentState.Unknown, null)), StringComparison.Ordinal);
        Assert.Contains("InFlight = False }", Words(Of(DeploymentState.Unknown, Ended)), StringComparison.Ordinal);
    }

    /// <summary>The moment in these words follows whoever reads them, which is what sets the record's own words apart
    /// from the page's: a reader of German is told <c>08.10.2026 04:20:07 +00:00</c> by the deployment and
    /// <c>2026-10-08 04:20 +00:00</c> by <see cref="TimeText" />, whose words are written for the dashboard. The
    /// names, the state and the flight are the same letters to both readers.</summary>
    [Fact]
    public void TheDeploymentOwnWordsFollowTheReaderWhileThePagesWordsDoNot()
    {
        var running = Full();

        ViewerCulture.German(() =>
        {
            Assert.Contains("Since = 08.10.2026 04:20:07 +00:00", running.ToString(), StringComparison.Ordinal);
            Assert.Contains("State = Executing, StateText = executing", running.ToString(), StringComparison.Ordinal);
            Assert.Contains("InFlight = True }", running.ToString(), StringComparison.Ordinal);
            Assert.Equal("2026-10-08 04:20 +00:00", TimeText.DateAndTime(running.Since!.Value, TimeZoneInfo.Utc));
        });

        Assert.Contains("Since = 10/08/2026 04:20:07 +00:00", Words(running), StringComparison.Ordinal);
        Assert.DoesNotContain("08.10.2026", Words(running), StringComparison.Ordinal);
    }

    // ----- the record's own shape

    /// <summary>The deployment's whole public surface, as it stands: its ten parts and the flight it computes, named
    /// in the order the constructor names them and of the type the constructor says, the two moments and the address
    /// and the wait each as nullable as the file may leave them, the deployment falling apart into the ten, the moment
    /// it is marked at, and the members every record is asked - comparing, hashing and saying itself - and nothing
    /// else to call. The two parts a deployment may do without are the last two arguments and the only two with a
    /// default, which is what lets a file from before them be read at all; the internal reading and the internal word
    /// of a state are no public members and are pinned through <see cref="DeploymentsReport.Parse" />. An eleventh
    /// part added to the deployment has to be written here as well, which is the point: the page cannot be shown a
    /// part nobody decided what it means.</summary>
    [Fact]
    public void TheDeploymentHasTenPartsTheFlightAndTheMembersEveryRecordIsAsked()
    {
        Assert.Equal(
            ["Project", "Environment", "Release", "State", "StateText", "Since", "Finished", "Url", "StartedBy", "WaitsFor", "InFlight"],
            [.. typeof(Deployment)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Select(property => property.Name)]);

        Assert.Equal(
            ["Deconstruct", "Environment", "Equals", "Finished", "GetHashCode", "InFlight", "IsMarkedAt", "Project", "Release", "Since", "StartedBy", "State", "StateText", "ToString", "Url", "WaitsFor", "op_Equality", "op_Inequality"],
            PublicSurface());

        Assert.Equal([typeof(IEquatable<Deployment>)], typeof(Deployment).GetInterfaces());
        Assert.True(typeof(Deployment).IsSealed);

        var properties = typeof(Deployment)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .ToDictionary(property => property.Name, StringComparer.Ordinal);

        Assert.Equal(
            [typeof(string), typeof(string), typeof(string), typeof(DeploymentState), typeof(string), typeof(DateTimeOffset?),
                typeof(DateTimeOffset?), typeof(Uri), typeof(string), typeof(DeploymentWait), typeof(bool)],
            [.. properties.Values.Select(property => property.PropertyType)]);

        Assert.All(properties.Values, property => Assert.True(property.CanRead));
        Assert.All(properties.Values, property => Assert.True(property.GetMethod!.IsPublic));
        Assert.All(properties.Values, property => Assert.False(property.GetMethod!.IsStatic));
        Assert.All(properties.Values, property => Assert.Equal($"get_{property.Name}", property.GetMethod!.Name));
        Assert.All(properties.Values, property => Assert.Empty(property.GetIndexParameters()));

        // The ten parts may be turned as the deployment is copied; the flight is computed and set by nobody.
        List<PropertyInfo> parts = [.. properties.Values.Where(property => property.Name != nameof(Deployment.InFlight))];

        Assert.Equal(10, parts.Count);
        Assert.All(parts, property => Assert.True(property.CanWrite));
        Assert.All(parts, property => Assert.NotNull(property.SetMethod));
        Assert.All(parts, property => Assert.True(property.SetMethod!.IsPublic));
        Assert.All(parts, property => Assert.Equal($"set_{property.Name}", property.SetMethod!.Name));

        // Each of the ten is the record's to be given at the copy and nowhere else: the setter is the init one the
        // positional parameters come with, so a deployment already handed over stands as it was handed.
        Assert.All(parts, property => Assert.Contains(typeof(IsExternalInit), property.SetMethod!.ReturnParameter.GetRequiredCustomModifiers()));

        Assert.False(properties[nameof(Deployment.InFlight)].CanWrite);
        Assert.Null(properties[nameof(Deployment.InFlight)].SetMethod);
        Assert.True(properties[nameof(Deployment.InFlight)].GetMethod!.IsPublic);

        var arguments = typeof(Deployment).GetConstructors().Single().GetParameters();

        Assert.Equal(10, arguments.Length);
        Assert.Equal(
            ["Project", "Environment", "Release", "State", "StateText", "Since", "Finished", "Url", "StartedBy", "WaitsFor"],
            [.. arguments.Select(argument => argument.Name!)]);
        Assert.Equal(
            [typeof(string), typeof(string), typeof(string), typeof(DeploymentState), typeof(string), typeof(DateTimeOffset?),
                typeof(DateTimeOffset?), typeof(Uri), typeof(string), typeof(DeploymentWait)],
            [.. arguments.Select(argument => argument.ParameterType)]);
        Assert.Equal(
            [false, false, false, false, false, false, false, false, true, true],
            [.. arguments.Select(argument => argument.HasDefaultValue)]);
        Assert.Equal(["Project", "Environment", "Release", "State", "StateText", "Since", "Finished", "Url"], [.. arguments.Where(argument => !argument.HasDefaultValue).Select(argument => argument.Name!)]);
        Assert.All(arguments.Where(argument => argument.HasDefaultValue), argument => Assert.Null(argument.DefaultValue));

        var marked = typeof(Deployment).GetMethod(nameof(Deployment.IsMarkedAt))!;

        Assert.False(marked.IsStatic);
        Assert.True(marked.IsPublic);
        Assert.Equal(typeof(bool), marked.ReturnType);
        Assert.Equal(["now"], [.. marked.GetParameters().Select(parameter => parameter.Name!)]);
        Assert.Equal([typeof(DateTimeOffset)], [.. marked.GetParameters().Select(parameter => parameter.ParameterType)]);

        var deconstruct = typeof(Deployment).GetMethod(nameof(Deployment.Deconstruct))!;

        Assert.Equal(
            [typeof(string).MakeByRefType(), typeof(string).MakeByRefType(), typeof(string).MakeByRefType(), typeof(DeploymentState).MakeByRefType(),
                typeof(string).MakeByRefType(), typeof(DateTimeOffset?).MakeByRefType(), typeof(DateTimeOffset?).MakeByRefType(),
                typeof(Uri).MakeByRefType(), typeof(string).MakeByRefType(), typeof(DeploymentWait).MakeByRefType()],
            [.. deconstruct.GetParameters().Select(parameter => parameter.ParameterType)]);
    }

    /// <summary>The deployment's non-public declared instance members as it stands: the one it declares itself,
    /// <c>CountsFrom</c>, and no other. The compiler's <c>EqualityContract</c> and <c>PrintMembers</c> are walked past
    /// as the record's private machinery that every record carries, not the record's own declaration. A second
    /// internal member added to the deployment has to be written down here as well, which is the point: the
    /// record's declared non-public surface is pinned exactly as it is, the way
    /// <see cref="TheDeploymentHasTenPartsTheFlightAndTheMembersEveryRecordIsAsked" /> pins the public surface.</summary>
    [Fact]
    public void TheDeploymentHasOneNonPublicDeclaredInstanceMemberAndItIsCountsFrom()
    {
        Assert.Equal(["CountsFrom"], NonPublicSurface());

        // The one member is the read-only internal moment the mark counts from, not in the public list.
        var member = typeof(Deployment)
            .GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
            .Single(property => property.Name == "CountsFrom");

        Assert.Equal(typeof(DateTimeOffset?), member.PropertyType);
        Assert.True(member.CanRead);
        Assert.False(member.CanWrite);
        Assert.True(member.GetMethod!.IsAssembly);
        Assert.False(member.GetMethod!.IsPublic);

        // And it is not in the public list, which is what keeps the two pins saying the same thing.
        Assert.DoesNotContain("CountsFrom", PublicSurface());
    }

    /// <summary>The non-public instance members the deployment declares itself: the read-only internal properties and
    /// methods, the compiler's private ones walked past. The <c>Read</c> and <c>Of</c> methods are static, so they
    /// stand in the file's own test and not here.</summary>
    private static List<string> NonPublicSurface() =>
        [.. typeof(Deployment)
            .GetMembers(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
            .Where(member => member is PropertyInfo
                             || member is MethodInfo method && !method.IsSpecialName)
            .Where(member => member.Name != "EqualityContract" && member.Name != "PrintMembers")
            .Select(member => member.Name)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];

    /// <summary>The public members the deployment declares itself: its properties and the methods that are not the get
    /// and the set of one - the two operators kept although the compiler names them specially, since they are the
    /// <c>==</c> and the <c>!=</c> a caller writes - with the record's compiler-named clone walked past. The
    /// constructor is no method, so it is named by the test above rather than by this list, and the reading of an
    /// entry is no public member at all.</summary>
    private static List<string> PublicSurface() =>
        [.. typeof(Deployment)
            .GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(member => member is PropertyInfo
                             || member is MethodInfo method
                             && (!method.IsSpecialName || method.Name.StartsWith("op_", StringComparison.Ordinal)))
            .Where(member => !member.Name.Contains('<', StringComparison.Ordinal))
            .Select(member => member.Name)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];

    /// <summary>The words a deployment says its parts in, taken in the invariant culture whatever the reader of this
    /// test happens to be: the record's own words follow whoever reads them, which is pinned on its own further up
    /// this file, so the strings there are the point and the culture only the way to say them once.</summary>
    private static string Words(Deployment deployment) => ViewerCulture.Invariant(() => deployment.ToString()!);
}
