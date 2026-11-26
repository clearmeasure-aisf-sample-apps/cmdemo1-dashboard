namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="EnvironmentStatus" />, one environment of the
/// topology with the state of its deployables and the versions the deployments pinned for it in Git, declared in
/// <c>src/Dashboard/Health/TargetStatus.cs</c>.
/// <para>
/// What is pinned where it belongs is left alone. <see cref="TargetStatusTests" /> and
/// <see cref="TargetStatusEdgeCaseTests" /> pin the tiles of an environment's deployables and everything a check
/// leaves on them; <see cref="DeployableInfoEdgeCaseTests" /> and <see cref="NodeInfoEdgeCaseTests" /> pin the two
/// parts of the file an environment is built from; <see cref="TopologyParserTests" /> pins the reading of the file
/// itself, and <see cref="SampleTopologyTests" /> the topology the app ships. The environment's own round is pinned
/// by the monitor: <see cref="DashboardMonitorTests" /> that an environment of a topology which names no
/// <c>versions.json</c> is <see cref="PinnedVersionsState.NotTracked" />, diffs nothing and assesses nothing, and
/// <see cref="PinnedVersionsMonitorTests" /> and <see cref="KustomizationPinMonitorTests" /> that
/// <see cref="EnvironmentStatus.Pinned" /> is pending before the first round of an environment that names a file,
/// that <see cref="EnvironmentStatus.AssessVersions" /> answers from the deployable's own pin when it names one and
/// from the environment's reading when it names none, that a failed reading replaces a good one, that
/// <see cref="EnvironmentStatus.VersionsDiffer" /> is said once per environment and
/// <see cref="EnvironmentStatus.PinHistoryOf" /> answers the deployable's own history, else the
/// environment's, else nothing. <see cref="ApplicationTextEdgeCaseTests" /> and <see cref="ApplicationViewTests" />
/// pin <see cref="EnvironmentStatus.Record" /> as the way a test hands an environment a pin it read beforehand.
/// </para>
/// <para>
/// What all of that leaves open, and what follows: the environment asked for its entry of the file and only for it,
/// refused by name and nothing half-built; the entry kept as the very instance it was handed, with the two words of
/// it the page reads - the name and the tier - standing as the file wrote them, the empty word and the blank among
/// them and the absent tier as the absence; one deployable status per deployable named, in the order named, the same
/// deployable named twice being two statuses of one entry, and an environment that names none holding none;
/// the two readings of the pin being the shared instances of their state rather than copies made for the
/// environment; a pin that is not there refused by name, before either memory moves; the recording and the
/// remembering being two memories and not one, remembering nothing not being refused at all;
/// <see cref="EnvironmentStatus.AssessVersions" /> and <see cref="EnvironmentStatus.PinHistoryOf" /> refusing a
/// deployable that is not there; both of
/// them answering of a deployable of another environment, the environment's own reading and history being what is
/// used and no membership being asked; and a deployable with no nodes, and one whose name is the empty word, being
/// judged like any other.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: an environment holds no timer and
/// calls nothing - the file that fetches its pin is <see cref="PinnedVersionsReader" />, and the round that runs it
/// is the monitor's, both pinned with a stubbed answer by the tests named above - and the one moment below, the
/// <see cref="ProbeResult.CheckedAt" /> of the check a node is given a version by, is one the test brought.
/// </para>
/// </summary>
public class EnvironmentStatusEdgeCaseTests
{
    /// <summary>The moment every check below was made from, so that no test asks the clock.</summary>
    private static readonly DateTimeOffset Start = new(2026, 10, 4, 22, 0, 0, TimeSpan.Zero);

    /// <summary>The two addresses an environment is named by when the test names no others: its pin file, and the
    /// page of that file's history - the empty word of the pair standing for the file nobody named.</summary>
    private const string Versions = "https://raw.example.net/org/demo-system/main/environments/uat/versions.json";
    private const string VersionsHistory = "https://github.example.net/org/demo-system/commits/main/environments/uat/versions.json";

    private static readonly Uri UiPin = new("https://raw.example.net/org/demo-system/main/gitops/environments/uat/ui/kustomization.yaml");
    private static readonly Uri UiPinHistory = new("https://github.example.net/org/demo-system/commits/main/gitops/environments/uat/ui/kustomization.yaml");

    /// <summary>An environment of the words named: the two of it the page reads, the file its pin is read from and
    /// the page of that file's history, and the deployables named - one of its own, the <c>ui</c> of every sample.</summary>
    private static EnvironmentInfo Entry(
        string name = "uat",
        string? tier = "nonprod",
        IReadOnlyList<DeployableInfo>? deployables = null,
        string? versions = Versions,
        string? history = VersionsHistory) =>
        new(
            name,
            tier,
            deployables ?? [Deployable("ui")],
            versions is null ? null : new Uri(versions),
            history is null ? null : new Uri(history));

    /// <summary>A deployable of the name named, at no public address, with no file of its own and the nodes named -
    /// the three paths the deployable falls back to being the ones of the topology it stands in.</summary>
    private static DeployableInfo Deployable(string name, Uri? pin = null, Uri? pinHistory = null, params NodeInfo[] nodes) =>
        new(
            name,
            FrontDoor: null,
            HealthPath: DeployableInfo.DefaultHealthPath,
            AlivePath: DeployableInfo.DefaultAlivePath,
            VersionPath: DeployableInfo.DefaultVersionPath,
            Nodes: nodes,
            PinUrl: pin,
            PinHistoryUrl: pinHistory);

    /// <summary>A node of the name named, in the region named, playing the role named, at an address of its own.</summary>
    private static NodeInfo Node(string name, string? region = "westus3", string role = NodeInfo.PrimaryRole) =>
        new(name, region, role, new Uri($"https://{name}.example.net"));

    /// <summary>A deployable status of the same parts as <see cref="Deployable" /> makes, for the times a status -
    /// and not the entry of the file - is what an environment is asked about.</summary>
    private static DeployableStatus Status(string name, Uri? pin = null, Uri? pinHistory = null, params NodeInfo[] nodes) =>
        new(Deployable(name, pin, pinHistory, nodes));

    /// <summary>A reading of a pin file that holds the one deployable named and the version named for it.</summary>
    private static PinnedVersions Read(string deployable, string version) =>
        PinnedVersions.Parse($$"""{ "{{deployable}}": "{{version}}" }""");

    /// <summary>A check of a node that answered healthy on the version named.</summary>
    private static ProbeResult Answer(string version) => new(HealthState.Healthy, 200, 40, Start, version);

    // ----- the environment asked for its entry of the file

    /// <summary>An environment is asked for its entry of the topology, and only for it: the entry that is not there
    /// is refused with <see cref="ArgumentNullException" /> naming <c>info</c>, at the making and before a single
    /// deployable status is built - and the refusal costs nothing, since the environment the test goes on to make
    /// from a real entry stands as it always did.</summary>
    [Fact]
    public void AnEnvironmentIsAskedForItsEntryOfTheFileAndOnlyForIt()
    {
        var error = Assert.Throws<ArgumentNullException>(() => new EnvironmentStatus(null!));

        Assert.Equal("info", error.ParamName);

        var status = new EnvironmentStatus(Entry());

        Assert.Equal("uat", status.Name);
        Assert.Single(status.Deployables);
        Assert.Same(PinnedVersions.Pending, status.Pinned);
    }

    // ----- the three parts the entry of the file says

    /// <summary>The entry of the file stands on the environment as it was handed - the very instance, not a copy of
    /// it - and the two words the page reads of it, <see cref="EnvironmentStatus.Name" /> and
    /// <see cref="EnvironmentStatus.Tier" />, are the words the
    /// file wrote, said again as they were spelled: the empty word is a name and a tier, a blank is a blank kept
    /// whole, and a tier the file does not write is the absence of one and not the empty word. The environment
    /// validates none of it, because the parser already did.</summary>
    [Fact]
    public void TheEnvironmentKeepsTheEntryItWasGivenAndSaysItsTwoWordsAsTheyWereWritten()
    {
        var info = Entry();
        var status = new EnvironmentStatus(info);

        Assert.Same(info, status.Info);
        Assert.Equal(info, status.Info);
        Assert.Equal("uat", status.Name);
        Assert.Equal(info.Name, status.Name);
        Assert.Equal("nonprod", status.Tier);
        Assert.Equal(info.Tier, status.Tier);

        // The empty word is a word the file wrote, and so is a blank of one.
        var empty = new EnvironmentStatus(Entry(string.Empty, string.Empty));

        Assert.Equal(string.Empty, empty.Name);
        Assert.Equal(string.Empty, empty.Tier);

        var blank = new EnvironmentStatus(Entry(" ", " "));

        Assert.Equal(" ", blank.Name);
        Assert.Equal(" ", blank.Tier);

        // A tier nobody wrote is the absence of one, and the name of nothing is still a name to hold.
        var nothing = new EnvironmentStatus(Entry(tier: null));

        Assert.Null(nothing.Tier);
        Assert.Equal("uat", nothing.Name);
        Assert.NotEqual(nothing.Info, empty.Info);
    }

    // ----- the deployables the entry names

    /// <summary>One deployable status per deployable the file names, in the order it names them, each holding the
    /// very entry of the file it was made from: an environment that names none holds none - the empty list rather
    /// than nothing - and the same deployable named twice is two statuses, because the environment decides nothing
    /// about who belongs to it, and holds no list of its own to keep them apart.</summary>
    [Fact]
    public void TheEnvironmentHoldsOneStatusPerDeployableTheFileNamesInOrderAndNoneWhenItNamesNoOne()
    {
        var bare = new EnvironmentStatus(Entry(deployables: []));

        Assert.Empty(bare.Deployables);
        Assert.Same(PinnedVersions.Pending, bare.Pinned);

        var (ui, api, job) = (Deployable("ui"), Deployable("api"), Deployable("job"));
        var status = new EnvironmentStatus(Entry(deployables: [ui, api, job]));

        Assert.Equal(["ui", "api", "job"], status.Deployables.Select(deployable => deployable.Info.Name));
        Assert.Same(ui, status.Deployables[0].Info);
        Assert.Same(api, status.Deployables[1].Info);
        Assert.Same(job, status.Deployables[2].Info);

        // Twice the same deployable is twice the status of it, of one and the same entry.
        var twice = new EnvironmentStatus(Entry(deployables: [ui, ui]));

        Assert.Equal(2, twice.Deployables.Count);
        Assert.NotSame(twice.Deployables[0], twice.Deployables[1]);
        Assert.Same(twice.Deployables[0].Info, twice.Deployables[1].Info);
        Assert.Equal(twice.Deployables[0].Info.Name, twice.Deployables[1].Info.Name);
    }

    // ----- the two readings of the pin

    /// <summary>An environment begins on the two shared readings and on no memory of one: the environment whose file
    /// is named starts on <see cref="PinnedVersions.Pending" /> and the one whose file nobody named on
    /// <see cref="PinnedVersions.NotTracked" /> - the one instance each of them is, not a reading made again for the
    /// environment - and the last reading that succeeded, which the events of the next round are compared with, is
    /// nothing before the first round of all.</summary>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void AnEnvironmentStartsOnTheSharedReadingOfItsPinAndOnNoMemoryOfOne(bool tracked)
    {
        var status = new EnvironmentStatus(Entry(versions: tracked ? Versions : null));

        Assert.Same(tracked ? PinnedVersions.Pending : PinnedVersions.NotTracked, status.Pinned);
        Assert.Equal(tracked ? PinnedVersionsState.Pending : PinnedVersionsState.NotTracked, status.Pinned.State);
        Assert.Empty(status.Pinned.Versions);
        Assert.Null(status.Pinned.Detail);
        Assert.Null(status.Pinned.Of("ui"));
        Assert.Null(status.LastReadPinned);
    }

    /// <summary>A pin that is not there is refused by name, and the refusal moves neither memory: the recording
    /// says <c>pinned</c> before it touches the reading of the moment or the last good one it keeps, at either end of
    /// a round - before anything was read, and after a version was - so that an environment is never left without a
    /// pin, only ever with the one it had.</summary>
    [Fact]
    public void APinThatIsNotThereIsRefusedByNameAndMovesNeitherMemory()
    {
        var status = new EnvironmentStatus(Entry());
        var pending = status.Pinned;

        Assert.Equal("pinned", Assert.Throws<ArgumentNullException>(() => status.Record(null!)).ParamName);
        Assert.Same(pending, status.Pinned);
        Assert.Null(status.LastReadPinned);

        status.Record(Read("ui", "2.4.7"));

        Assert.Equal("pinned", Assert.Throws<ArgumentNullException>(() => status.Record(null!)).ParamName);
        Assert.NotSame(pending, status.Pinned);
        Assert.Equal("2.4.7", status.Pinned.Of("ui"));
        Assert.Null(status.LastReadPinned);
    }

    /// <summary>The two memories of the pin are moved apart: recording replaces the reading of the moment and
    /// remembers nothing, remembering replaces the last reading that succeeded and leaves the moment where it
    /// stands - and, being the record of a round rather than its answer, remembering is not refused anything:
    /// <see langword="null" /> is accepted and clears the memory, which recording of the same nothing would have
    /// refused.</summary>
    [Fact]
    public void TheRecordingAndTheRememberingOfAPinAreTwoSeparateMemories()
    {
        var status = new EnvironmentStatus(Entry());
        var read = Read("ui", "2.4.7");

        status.Record(read);

        Assert.Same(read, status.Pinned);
        Assert.Null(status.LastReadPinned);

        status.RecordGoodPinned(PinnedVersions.Missing);

        Assert.Same(read, status.Pinned);
        Assert.Same(PinnedVersions.Missing, status.LastReadPinned);

        status.RecordGoodPinned(read);

        Assert.Same(read, status.LastReadPinned);
        Assert.Same(read, status.Pinned);

        // Nothing is refused here, and nothing is what the memory is then left holding.
        status.RecordGoodPinned(null!);

        Assert.Null(status.LastReadPinned);
        Assert.Same(read, status.Pinned);

        // A reading of no version at all is a reading: recorded, and said by the file to be empty.
        var empty = new EnvironmentStatus(Entry());
        var none = PinnedVersions.Parse("{}");

        empty.Record(none);

        Assert.Same(none, empty.Pinned);
        Assert.Equal(PinnedVersionsState.Read, empty.Pinned.State);
        Assert.Empty(empty.Pinned.Versions);
        Assert.Null(empty.Pinned.Of("ui"));
        Assert.Null(empty.LastReadPinned);
    }

    // ----- the versions the environment's deployables run

    /// <summary>The deployable a version is assessed for is asked for and only for: the one that is not there is
    /// refused with <see cref="ArgumentNullException" /> naming <c>deployable</c>, and the refusal costs the
    /// environment nothing - its pin and its memory of the last good reading stand where they stood.</summary>
    [Fact]
    public void AVersionAssessedForNoDeployableIsRefusedByName()
    {
        var status = new EnvironmentStatus(Entry());
        status.Record(Read("ui", "2.4.7"));
        var pinned = status.Pinned;

        Assert.Equal("deployable", Assert.Throws<ArgumentNullException>(() => status.AssessVersions(null!)).ParamName);

        Assert.Same(pinned, status.Pinned);
        Assert.Null(status.LastReadPinned);
    }

    /// <summary>An environment that holds no deployable diffs nothing, whatever its pin says: the question is asked
    /// of the deployables the environment holds and of nothing else, so the environment that holds none has nothing
    /// to answer with - not even after a pin is recorded for a deployable that is not there, and not even when the
    /// file names no <c>versions.json</c> at all.</summary>
    [Fact]
    public void AnEnvironmentWithNoDeployablesDiffsNothingWhateverItsPinSays()
    {
        var status = new EnvironmentStatus(Entry(deployables: []));

        Assert.False(status.VersionsDiffer);

        status.Record(Read("ui", "2.4.7"));

        Assert.False(status.VersionsDiffer);
        Assert.Equal("2.4.7", status.Pinned.Of("ui"));
        Assert.Empty(status.Deployables);

        // The same, for an environment whose pin nobody tracks.
        Assert.False(new EnvironmentStatus(Entry(deployables: [], versions: null)).VersionsDiffer);
    }

    /// <summary>An environment asks nothing about who a deployable belongs to: the status it is handed is judged by
    /// this environment's reading of the pin, in the state this reading stands in at the call - pending before the
    /// file is read, and the version in it afterwards - which is why the monitor reads a deployable's own file into
    /// the deployable rather than leaving it to be judged by the environment it stands in. A node that reported a
    /// version is compared with the pinned one here as it is anywhere else.</summary>
    [Fact]
    public void ADeployableOfAnotherEnvironmentIsJudgedByThisEnvironmentsReading()
    {
        var alien = Status("ui", nodes: [Node("app-alien")]);
        alien.Nodes[0].Record(Answer("2.4.7"));

        var status = new EnvironmentStatus(Entry());

        Assert.Equal(VersionState.Pending, status.AssessVersions(alien)!.State);
        Assert.False(status.VersionsDiffer);

        status.Record(Read("ui", "2.4.7"));

        var inSync = status.AssessVersions(alien)!;

        Assert.NotNull(inSync);
        Assert.Equal(VersionState.InSync, inSync.State);
        Assert.Equal("2.4.7", inSync.Pinned);
        Assert.Equal("Pinned 2.4.7. In sync: westus3 runs 2.4.7.", inSync.Text);
        Assert.Single(inSync.Matching);
        Assert.Empty(inSync.Differing);
        Assert.False(status.VersionsDiffer);

        // ... and a pinned version another environment's node does not run is a difference of that deployable,
        // asked of this environment - but never a difference of this environment, whose own deployables are all it asks.
        var other = Status("ui", pin: UiPin, nodes: [Node("app-other")]);
        other.Nodes[0].Record(Answer("2.4.6"));
        other.RecordPin(Read("ui", "2.4.7"));

        Assert.Equal(VersionState.Differs, status.AssessVersions(other)!.State);
        Assert.False(status.VersionsDiffer);
    }

    /// <summary>A deployable is named to the pin file by the word it holds, and by no other: the empty word is such a
    /// name, the file read for it is the file read for any other, and a deployable with no nodes at all - the
    /// topology naming nothing behind the address - is a deployable whose versions are simply not compared, which is
    /// no difference. The environment says nothing about either.</summary>
    [Fact]
    public void ADeployableWithNoNodesAndOneNamedByNoWordAreBothJudgedLikeAnyOther()
    {
        var status = new EnvironmentStatus(Entry(deployables: [Deployable(string.Empty)]));
        status.Record(PinnedVersions.Parse("""{ "": "1.0.0" }"""));

        var assessment = status.AssessVersions(status.Deployables[0])!;

        Assert.NotNull(assessment);
        Assert.Equal(string.Empty, status.Deployables[0].Info.Name);
        Assert.Equal("1.0.0", assessment.Pinned);
        Assert.Equal(VersionState.NodesUnknown, assessment.State);
        Assert.Equal("Pinned 1.0.0. Not compared: the deployable has no nodes.", assessment.Text);
        Assert.Empty(assessment.Matching);
        Assert.Empty(assessment.Differing);
        Assert.Empty(assessment.Unknown);
        Assert.False(assessment.Differs);
        Assert.False(status.VersionsDiffer);

        // The same deployable, with a node that reported that very version, is in sync and no longer uncomparable.
        var running = new EnvironmentStatus(Entry(deployables: [Deployable(string.Empty, nodes: [Node("app-ui")])]));
        running.Deployables[0].Nodes[0].Record(Answer("1.0.0"));
        running.Record(PinnedVersions.Parse("""{ "": "1.0.0" }"""));

        Assert.Equal(VersionState.InSync, running.AssessVersions(running.Deployables[0])!.State);
        Assert.False(running.VersionsDiffer);
    }

    // ----- the page with the history of a deployable's pin

    /// <summary>The history page of a pin is answered without asking who the deployable is: the deployable's own
    /// history wins even from an environment that names none - the environment adding nothing to an answer the
    /// deployable already has - and a deployable naming none of its own is answered with this environment's history,
    /// whatever environment that deployable was built for. Nothing is copied on the way out, and an environment that
    /// names neither history answers with nothing at all.</summary>
    [Fact]
    public void ThePinHistoryOfADeployableIsAnsweredWithoutAskingWhoItBelongsTo()
    {
        var own = new EnvironmentStatus(Entry(
            history: null,
            deployables: [Deployable("ui", pin: UiPin, pinHistory: UiPinHistory)]));

        Assert.Null(own.Info.VersionsHistoryUrl);
        Assert.Same(UiPinHistory, own.PinHistoryOf(own.Deployables[0]));

        // A deployable naming no history of its own, in an environment naming none either, is answered with nothing.
        Assert.Null(own.PinHistoryOf(Status("api")));

        var tracked = new EnvironmentStatus(Entry(deployables: []));

        Assert.Equal(VersionsHistory, tracked.PinHistoryOf(Status("api"))?.AbsoluteUri);

        // The environment that names neither history answers with nothing, whoever is asked.
        var bare = new EnvironmentStatus(Entry(versions: null, history: null));

        Assert.Null(bare.PinHistoryOf(Status("api")));
        Assert.Null(bare.PinHistoryOf(Status("ui", pin: UiPin)));
        Assert.Equal(UiPinHistory, bare.PinHistoryOf(Status("ui", pin: UiPin, pinHistory: UiPinHistory)));
    }

    /// <summary>The history page of no deployable is refused by name, at either end of what the environment holds:
    /// the one that answers with the deployable's own, the one that answers with its own file and the one that
    /// answers with nothing at all all refuse the question the same way, and none of them is moved by it.</summary>
    [Fact]
    public void APinHistoryAskedOfNoDeployableIsRefusedByName()
    {
        var tracked = new EnvironmentStatus(Entry());

        Assert.Equal("deployable", Assert.Throws<ArgumentNullException>(() => tracked.PinHistoryOf(null!)).ParamName);

        var own = new EnvironmentStatus(Entry(deployables: [Deployable("ui", pin: UiPin, pinHistory: UiPinHistory)]));

        Assert.Equal("deployable", Assert.Throws<ArgumentNullException>(() => own.PinHistoryOf(null!)).ParamName);

        var bare = new EnvironmentStatus(Entry(deployables: [], versions: null, history: null));

        Assert.Equal("deployable", Assert.Throws<ArgumentNullException>(() => bare.PinHistoryOf(null!)).ParamName);
        Assert.Empty(bare.Deployables);
        Assert.Same(PinnedVersions.NotTracked, bare.Pinned);
    }
}
