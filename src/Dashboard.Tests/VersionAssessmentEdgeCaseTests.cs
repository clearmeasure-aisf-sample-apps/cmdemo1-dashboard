using System.Reflection;

using static Dashboard.Tests.HealthStates;
using static Dashboard.Tests.Nodes;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of every public member of the version assessment, declared in
/// <c>src/Dashboard/Health/VersionAssessment.cs</c>: <see cref="NodeVersion" /> with the three parts a node hands the
/// decision, its <see cref="NodeVersion.Running" /> version and its <see cref="NodeVersion.UnknownReason" />,
/// <see cref="VersionState" /> with the six states it names, <see cref="VersionAssessment" /> with the
/// <see cref="VersionAssessment.Assess" /> decision, its seven parts and its two words
/// <see cref="VersionAssessment.Differs" /> and <see cref="VersionAssessment.Text" />, and
/// <see cref="VersionSummary" /> with the one count it is.
/// <para>
/// What is pinned where it belongs is left alone. <see cref="VersionAssessmentTests" /> pins the words of every state
/// the decision reaches, the comparison that drops build metadata and case, the node that does not answer standing out
/// of the comparison, the three counts of the in-sync sentence, the deployable missing from the file, the file that is
/// missing, unreadable and malformed, the name of the file in the words, the reading still pending, and the three
/// counts of the summary; <see cref="VersionTextTests" /> pins <see cref="VersionText.Display" /> the running version
/// is read with, and <see cref="PinnedVersionsTests" /> pins the readings <see cref="VersionAssessment.Assess" /> works from - including
/// <see cref="PinnedVersions.Missing" />, <see cref="PinnedVersions.Pending" /> and
/// <see cref="PinnedVersions.NotTracked" /> pinning nothing at all.
/// <see cref="TargetStatusEdgeCaseTests" /> pins <see cref="NodeVersion.Running" /> for the two states a tile leaves it
/// in, and the monitor tests (<see cref="PinnedVersionsMonitorTests" />, <see cref="KustomizationPinMonitorTests" />,
/// <see cref="EnvironmentStatusEdgeCaseTests" />) pin which state a whole dashboard reaches and how
/// <see cref="VersionSummary.Text" /> counts it.
/// </para>
/// <para>
/// What all of that leaves open, and what follows: the running version at the ends of the two questions it turns on -
/// the state that answered and the version that says nothing - and a state the enum does not name; the reason said of a
/// node that answered but reported no version, and said of a node whatever else it carries; the three parts standing
/// where they are handed, two nodes being one while they are, and the words a node says itself; the six states as the
/// file declares them, in order and on their numbers; <see cref="VersionAssessment.Differs" /> read out of the state
/// alone, over every state and over a number that is none; <see cref="VersionAssessment.Text" /> as the join it is, with an empty second
/// sentence and without one; the three parts <see cref="VersionAssessment.Assess" /> refuses by name, the file it names only when the
/// caller stays silent, a deployable of no name, a pin that says nothing, an unreadable file that gives no reason and
/// gives the empty word for one; the nodes being read once, and not at all until a version is pinned; the three lists
/// holding every node once, in the order it was handed and as the node it was handed as; the seven parts of an
/// assessment standing where they are handed while the two words are worked out of them, two decisions being two
/// assessments because each built its own lists, and a copy being one; the summary counting the ends of the count,
/// below zero and at the top of it; and the surface the four types declare.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: an assessment is worked out of a reading
/// of the file and a list of nodes both brought by the test, and neither of the two asks when anything happened.
/// </para>
/// </summary>
public class VersionAssessmentEdgeCaseTests
{
    /// <summary>A reading of the file that pins <c>ui</c> to this very text, as the record of a reading holds it and
    /// before the decision reads anything out of it - the empty word, a blank, and build metadata alone among them.</summary>
    private static PinnedVersions Read(string version) =>
        new(PinnedVersionsState.Read, new Dictionary<string, string>(StringComparer.Ordinal) { ["ui"] = version });

    /// <summary>The second sentence of the decision a reading and a file name lead to, over no nodes at all.</summary>
    private static string? DetailOf(PinnedVersions reading, string deployable, string? file) =>
        VersionAssessment.Assess(reading, deployable, [], file).Detail;

    // ----- NodeVersion.Running: the version a node is said to run

    /// <summary>The version a node runs is what a check that answered reported: one of the two states a check leaves
    /// behind and nothing else, the number the enum does not name included. What the answer was worth is
    /// <see cref="VersionText.Display" />'s question, and the version the node reported stands where it was handed
    /// whichever way it is read - the dashboard remembering a build is one thing and claiming a node runs it another.</summary>
    [Theory]
    [InlineData(Healthy, "2.4.7", "2.4.7")]
    [InlineData(Unhealthy, "2.4.7", "2.4.7")]
    [InlineData(Healthy, "2.4.7+0a1b2c3", "2.4.7")]
    [InlineData(Unhealthy, " 2.4.7+0a1b2c3 ", "2.4.7")]
    [InlineData(Healthy, "2.4.7-rc1", "2.4.7-rc1")]
    [InlineData(Healthy, "0.0.0-local+abc", "0.0.0-local")]
    [InlineData(Healthy, null, null)]
    [InlineData(Healthy, "", null)]
    [InlineData(Healthy, "   ", null)]
    [InlineData(Healthy, "+0a1b2c3", null)]
    [InlineData(Unhealthy, "+0a1b2c3", null)]
    [InlineData(Unhealthy, null, null)]
    [InlineData(Unreachable, "2.4.7", null)]
    [InlineData(Pending, "2.4.7", null)]
    [InlineData(Unnamed, "2.4.7", null)]
    [InlineData(Unreachable, "+0a1b2c3", null)]
    [InlineData(Pending, "   ", null)]
    public void TheRunningVersionIsWhatACheckThatAnsweredReported(HealthState state, string? version, string? expected)
    {
        var node = West(state, version);

        Assert.Equal(expected, node.Running);

        // The version reported is a part the node keeps whatever it makes of it; the running one is worked out of it.
        Assert.Equal(version, node.Version);
    }

    /// <summary>The running version is read of the state and the version and of nothing else: the same two parts say
    /// the same thing under every label, and the label of nothing at all is no exception - while the two states that
    /// answer and the two that do not are apart by the answer alone, a build reported standing untouched behind the
    /// version nobody claims.</summary>
    [Fact]
    public void TheRunningVersionIsReadOfTheTwoPartsAndOfNothingElse()
    {
        var answer = new NodeVersion(string.Empty, Healthy, "2.4.7+0a1b2c3");

        Assert.Equal("2.4.7", answer.Running);
        Assert.Equal(West(Healthy, " 2.4.7 ").Running, answer.Running);
        Assert.Equal(West(Healthy, "2.4.7+0a1b2c3").Running, answer.Running);

        // A node of no label at all says the version its two parts make, and the parts themselves are still its own.
        Assert.Equal(string.Empty, answer.Label);
        Assert.Equal("2.4.7+0a1b2c3", answer.Version);

        // A node that stopped answering keeps the build it reported and runs no version the dashboard will name.
        var silent = West(Unreachable, "2.4.7");

        Assert.Null(silent.Running);
        Assert.Equal("2.4.7", silent.Version);
        Assert.NotEqual(silent, West(Unreachable, null));
    }

    // ----- NodeVersion.UnknownReason: why the running version is not known

    /// <summary>The reason is the state said in the words of the end of a sentence, and the state only: the two words
    /// of the checks that never reached an answer, and the one the file holds for everything else - the node that
    /// answered and reported nothing, the node in no state at all, and a number the enum does not name. Neither the
    /// label nor the version reported is a word of it, and the empty version is as unknown as the absent one.</summary>
    [Theory]
    [InlineData(Pending, "is being checked")]
    [InlineData(Unreachable, "is unreachable")]
    [InlineData(Healthy, "reports no version")]
    [InlineData(Unhealthy, "reports no version")]
    [InlineData(Unnamed, "reports no version")]
    public void TheReasonIsTheStateAlone(HealthState state, string reason)
    {
        Assert.Equal(reason, West(state, null).UnknownReason);
        Assert.Equal(reason, West(state, string.Empty).UnknownReason);
        Assert.Equal(reason, West(state, "   ").UnknownReason);
        Assert.Equal(reason, West(state, "2.4.7").UnknownReason);
        Assert.Equal(reason, West(state, "2.4.7+0a1b2c3").UnknownReason);
        Assert.Equal(reason, new NodeVersion(string.Empty, state, null).UnknownReason);
        Assert.Equal(reason, new NodeVersion("north europe", state, "0.0.0-local").UnknownReason);

        // The words end a sentence that starts with the label, so they carry neither a label of their own nor a stop.
        Assert.DoesNotContain("westus3", West(state, "2.4.7").UnknownReason, StringComparison.Ordinal);
        Assert.DoesNotContain(".", West(state, "2.4.7").UnknownReason, StringComparison.Ordinal);
    }

    // ----- NodeVersion: the three parts, the two words, and the record around them

    /// <summary>The three parts stand where they are handed and are read of nothing else: the label kept whole with
    /// the spaces about it and empty as the empty word, the state kept over the four and over the number that is
    /// none of them, and the version kept as it was reported - the blank, the metadata alone and the nothing at all
    /// among them, none of them refused and none of them changed.</summary>
    [Fact]
    public void TheNodeKeepsItsThreePartsAsTheyAreHanded()
    {
        var node = new NodeVersion(" north europe ", Unnamed, " 2.4.7+0a1b2c3 ");

        Assert.Equal(" north europe ", node.Label);
        Assert.Equal(Unnamed, node.State);
        Assert.Equal(" 2.4.7+0a1b2c3 ", node.Version);

        Assert.Equal(string.Empty, new NodeVersion(string.Empty, Healthy, string.Empty).Label);
        Assert.Null(new NodeVersion("westus3", Healthy, null).Version);
        Assert.Equal(string.Empty, new NodeVersion("westus3", Healthy, string.Empty).Version);

        // A node is a node over all three of them being nothing: no part of it is refused, and none is defaulted.
        var nothing = new NodeVersion(string.Empty, default, null);

        Assert.Equal(string.Empty, nothing.Label);
        Assert.Equal(Pending, nothing.State);
        Assert.Null(nothing.Version);
        Assert.Null(nothing.Running);
        Assert.Equal("is being checked", nothing.UnknownReason);
    }

    /// <summary>Two nodes are one while their three parts are, and the parts are the three the record declares: the
    /// running version and the reason are read out of them and are nothing of their own, so a node of one more
    /// spelling of the same build is another node although it runs the very same version, and a number the enum names
    /// no state for is kept as the number it is.</summary>
    [Fact]
    public void TwoNodesAreOneWhileTheirThreePartsAre()
    {
        var west = West(Healthy, "2.4.7");
        var same = new NodeVersion("westus3", Healthy, "2.4.7");

        Assert.True(west == same);
        Assert.False(west != same);
        Assert.True(west.Equals(same));
        Assert.True(west.Equals((object)same));
        Assert.True(west.Equals(west));
        Assert.NotSame(west, same);
        Assert.Equal(west.GetHashCode(), same.GetHashCode());
        Assert.Equal(west.GetHashCode(), west.GetHashCode());
        Assert.Single([.. new HashSet<NodeVersion> { west, same }]);

        // Every part on its own makes two, the label of nothing and the version of nothing included.
        Assert.False(west == west with { Label = string.Empty });
        Assert.False(west == west with { Label = "westus3 " });
        Assert.False(west == west with { State = Unhealthy });
        Assert.False(west == west with { State = Unnamed });
        Assert.False(west == west with { Version = null });
        Assert.False(west == west with { Version = string.Empty });

        // The same build in another spelling is another version reported, and so another node.
        var other = West(Healthy, "2.4.7+0a1b2c3");

        Assert.Equal(west.Running, other.Running);
        Assert.False(west == other);
        Assert.NotEqual(west.GetHashCode(), other.GetHashCode());

        // What is no node is nobody's equal, and the nothing a caller holds is not a node either.
        NodeVersion? absent = null;

        Assert.False(west.Equals(null));
        Assert.False(west.Equals("westus3"));
        Assert.False(west.Equals(Healthy));
        Assert.False(west == absent);
        Assert.False(absent == west);
        Assert.True(absent == null);
    }

    /// <summary>The three parts fall out in the order the record declares them - label, state, version - and the two
    /// words are no parts of that falling apart, so what comes out is what went in; the same three parts are what the
    /// node says of itself, the running version and the reason said along with them.</summary>
    [Fact]
    public void TheNodeFallsApartIntoItsThreePartsAndSaysThemInItsOwnWords()
    {
        var (label, state, version) = West(Unreachable, "2.4.7+0a1b2c3");

        Assert.Equal("westus3", label);
        Assert.Equal(Unreachable, state);
        Assert.Equal("2.4.7+0a1b2c3", version);

        var (nothing, unset, nowhere) = new NodeVersion(string.Empty, default, null);

        Assert.Equal(string.Empty, nothing);
        Assert.Equal(Pending, unset);
        Assert.Null(nowhere);

        Assert.Equal(
            "NodeVersion { Label = westus3, State = Healthy, Version = 2.4.7, Running = 2.4.7, UnknownReason = reports no version, "
            + "UnknownClause = westus3 reports no version }",
            West(Healthy, "2.4.7").ToString());

        // The three parts are said as they were handed and the two words along with them: a state no word of the enum
        // names is said as the number it is, and a word with nothing behind it is said as nothing at all.
        Assert.Equal(
            "NodeVersion { Label = , State = 4, Version = , Running = , UnknownReason = reports no version, "
            + "UnknownClause =  reports no version }",
            new NodeVersion(string.Empty, Unnamed, null).ToString());
    }

    // ----- VersionState: the six states the decision names

    /// <summary>The whole of what <see cref="VersionState" /> declares: an <see cref="int" /> underneath, six states
    /// in the order the file writes them and on the numbers they stand on, and no member of its own beyond the storage
    /// every value type is given - so a state carries nothing but the number it is, and the two words the assessment
    /// says are worked out of it elsewhere. A seventh state added to the file has to be written here as well, which is
    /// the point: an assessment cannot be shown in words nobody decided what they mean.</summary>
    [Fact]
    public void TheEnumDeclaresItsSixStatesAndNothingElse()
    {
        var type = typeof(VersionState);

        Assert.True(type.IsEnum);
        Assert.True(type.IsValueType);
        Assert.True(type.IsSealed);
        Assert.False(type.IsPrimitive);
        Assert.Equal(typeof(int), Enum.GetUnderlyingType(type));
        Assert.Equal(typeof(VersionState), VersionState.Differs.GetType());

        var instance = type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        Assert.Single(instance);
        Assert.Equal("value__", instance[0].Name);
        Assert.Equal(typeof(int), ((FieldInfo)instance[0]).FieldType);

        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);

        Assert.Equal(
            [nameof(VersionState.Pending), nameof(VersionState.PinnedUnknown), nameof(VersionState.NotDeployed),
             nameof(VersionState.NodesUnknown), nameof(VersionState.InSync), nameof(VersionState.Differs)],
            fields.Select(field => field.Name));
        Assert.Equal([0, 1, 2, 3, 4, 5], fields.Select(field => (int)field.GetRawConstantValue()!));
        Assert.All(fields, field =>
        {
            Assert.Equal(type, field.FieldType);
            Assert.True(field.IsPublic);
            Assert.True(field.IsLiteral);
            Assert.False(field.IsInitOnly);
        });

        // The order the file writes them in is the order of the numbers, from the reading not taken to the difference.
        Assert.Equal([0, 1, 2, 3, 4, 5], [.. Enum.GetValues<VersionState>().Select(state => (int)state)]);
        Assert.False(Enum.IsDefined((VersionState)6));
        Assert.False(Enum.IsDefined((VersionState)(-1)));
    }

    // ----- VersionAssessment: the two words the seven parts are read into

    /// <summary><see cref="VersionAssessment.Differs" /> is the state asked and nothing else: of the six states one
    /// only, of the numbers that are none of the six none - whatever the seven parts around the state say, so a
    /// difference the record was handed with no differing node in it still differs and an in-sync record full of them
    /// does not.</summary>
    [Theory]
    [InlineData(VersionState.Pending, false)]
    [InlineData(VersionState.PinnedUnknown, false)]
    [InlineData(VersionState.NotDeployed, false)]
    [InlineData(VersionState.NodesUnknown, false)]
    [InlineData(VersionState.InSync, false)]
    [InlineData(VersionState.Differs, true)]
    [InlineData((VersionState)6, false)]
    [InlineData((VersionState)(-1), false)]
    public void TheAnswerThatTheyDifferIsTheStateAlone(VersionState state, bool differs)
    {
        var node = West(Healthy, "2.4.6");

        Assert.Equal(differs, new VersionAssessment(state, "2.4.7", [node], [node], [node], "Pinned 2.4.7", "A second sentence.").Differs);
        Assert.Equal(differs, new VersionAssessment(state, null, [], [], [], "Nothing known", null).Differs);
    }

    /// <summary>The lists are nothing of the answer that they differ, and neither is the pinned version: the state
    /// that says they differ answers true with no node in any of the three lists and no pin to differ from, and the
    /// state that says they do not answers false with the differing nodes standing in the list of them.</summary>
    [Fact]
    public void TheThreeListsAndThePinAreNothingOfTheAnswerThatTheyDiffer()
    {
        var node = West(Healthy, "2.4.6");

        Assert.True(new VersionAssessment(VersionState.Differs, null, [], [], [], "Pinned 2.4.7", null).Differs);
        Assert.False(new VersionAssessment(VersionState.InSync, "2.4.7", [], [node], [node], "Pinned 2.4.7", null).Differs);
        Assert.False(new VersionAssessment(VersionState.NodesUnknown, "2.4.7", [node], [node], [node], "Pinned 2.4.7", null).Differs);

        // The word is read again on every ask, so a copy that moves the state moves the answer with it.
        var still = new VersionAssessment(VersionState.InSync, "2.4.7", [node], [], [], "Pinned 2.4.7", null);

        Assert.False(still.Differs);
        Assert.True((still with { State = VersionState.Differs }).Differs);
        Assert.False((still with { Headline = "Differs: westus3 runs 2.4.6." }).Differs);
    }

    /// <summary><see cref="VersionAssessment.Text" /> is the two sentences joined by a full stop and a space, the
    /// second one left out when there is none to leave out - and nothing more than that join: the empty word is a
    /// second sentence the join does not swallow, a headline of nothing is a headline the join still puts its stop
    /// behind, and neither of the two is trimmed, changed in case, or compared with anything.</summary>
    [Theory]
    [InlineData("Pinned 2.4.7", null, "Pinned 2.4.7")]
    [InlineData("Pinned 2.4.7", "", "Pinned 2.4.7. ")]
    [InlineData("Pinned 2.4.7", "In sync: all 2 nodes run 2.4.7.", "Pinned 2.4.7. In sync: all 2 nodes run 2.4.7.")]
    [InlineData("", null, "")]
    [InlineData("", "", ". ")]
    [InlineData("", "Differs.", ". Differs.")]
    [InlineData(" ", null, " ")]
    [InlineData("  Pinned  ", "  Detail  ", "  Pinned  .   Detail  ")]
    public void TheTwoSentencesAreJoinedByAFullStopAndASpace(string headline, string? detail, string text)
    {
        var assessment = new VersionAssessment(VersionState.InSync, "2.4.7", [], [], [], headline, detail);

        Assert.Equal(text, assessment.Text);
        Assert.Equal(text, $"{assessment.Headline}{(assessment.Detail is null ? string.Empty : $". {assessment.Detail}")}");
    }

    /// <summary>The whole line is read of the two sentences on every ask, so it follows whichever of them a copy
    /// moves; of the assessments the decision makes, only the one still reading the file is left without its second
    /// sentence, and the join says it as the headline alone.</summary>
    [Fact]
    public void TheWholeLineFollowsWhicheverSentenceMoved()
    {
        var assessment = Assess(West(Healthy, "2.4.7"));

        Assert.Equal("Pinned 2.4.7. In sync: westus3 runs 2.4.7.", assessment.Text);
        Assert.Equal("Pinned 2.4.7. Differs.", (assessment with { Detail = "Differs." }).Text);
        Assert.Equal("Pinned 9.9.9. In sync: westus3 runs 2.4.7.", (assessment with { Headline = "Pinned 9.9.9" }).Text);
        Assert.Equal("Pinned 2.4.7", (assessment with { Detail = null }).Text);

        // The second sentence is the decision's own mark of a reading that is still going on.
        var reading = VersionAssessment.Assess(PinnedVersions.Pending, "ui", [West(Healthy, "2.4.7")]);

        Assert.Null(reading.Detail);
        Assert.Equal(reading.Headline, reading.Text);
    }

    // ----- Assess: what it refuses, and the file it names

    /// <summary>The three parts the decision works from are refused by name, in the order the file asks them - the
    /// reading of the pin first, then the deployable it was read for, then the nodes - and refused before anything of
    /// them is read, so a reading that is not there is refused over an empty list of nodes and over a file that names
    /// itself as well. The name of the file is the one part a caller may leave unsaid, and a list of no nodes is a
    /// list rather than a refusal.</summary>
    [Fact]
    public void TheThreePartsTheDecisionWorksFromAreRefusedByName()
    {
        var nodes = new[] { West(Healthy, "2.4.7") };

        Assert.Equal("pinned", Assert.Throws<ArgumentNullException>(() => VersionAssessment.Assess(null!, "ui", nodes)).ParamName);
        Assert.Equal("deployable", Assert.Throws<ArgumentNullException>(() => VersionAssessment.Assess(Pinned, null!, nodes)).ParamName);
        Assert.Equal("nodes", Assert.Throws<ArgumentNullException>(() => VersionAssessment.Assess(Pinned, "ui", null!)).ParamName);

        // The reading is refused before the deployable and the nodes are looked at, and the deployable before the nodes.
        Assert.Equal("pinned", Assert.Throws<ArgumentNullException>(() => VersionAssessment.Assess(null!, "ui", [])).ParamName);
        Assert.Equal("pinned", Assert.Throws<ArgumentNullException>(() => VersionAssessment.Assess(null!, null!, [], null)).ParamName);
        Assert.Equal("deployable", Assert.Throws<ArgumentNullException>(() => VersionAssessment.Assess(PinnedVersions.Missing, null!, [], "kustomization.yaml")).ParamName);
        Assert.Equal("nodes", Assert.Throws<ArgumentNullException>(() => VersionAssessment.Assess(PinnedVersions.Missing, "ui", null!, "kustomization.yaml")).ParamName);

        // Nothing to compare and nothing to name the file by are both answers, not refusals.
        Assert.Equal(VersionState.NotDeployed, VersionAssessment.Assess(PinnedVersions.Missing, "ui", []).State);
        Assert.Equal(VersionState.NodesUnknown, VersionAssessment.Assess(Pinned, "ui", [], null).State);
    }

    /// <summary>The file is named in the three sentences that say no version is pinned, exactly as the caller named
    /// it: the spaces about a name and the empty name are kept, and only the caller saying nothing at all brings the
    /// one name the environment pins its versions in - which is the same answer as naming it outright. Where a version
    /// is pinned, the file is nowhere in the words.</summary>
    [Theory]
    [InlineData(null, PinnedVersions.FileName)]
    [InlineData("", "")]
    [InlineData(" ", " ")]
    [InlineData("kustomization.yaml", "kustomization.yaml")]
    public void TheFileIsNamedAsTheCallerNamesItAndDefaultsToTheEnvironmentFile(string? file, string named)
    {
        Assert.StartsWith($"{named} was not found", DetailOf(PinnedVersions.Missing, "ui", file), StringComparison.Ordinal);
        Assert.StartsWith($"{named} could not be read", DetailOf(PinnedVersions.Unavailable("the server answered HTTP 503"), "ui", file), StringComparison.Ordinal);
        Assert.StartsWith($"{named} has no entry for api", DetailOf(Pinned, "api", file), StringComparison.Ordinal);

        Assert.Equal(DetailOf(PinnedVersions.Missing, "ui", null), DetailOf(PinnedVersions.Missing, "ui", PinnedVersions.FileName));
        Assert.Equal(DetailOf(Pinned, "api", null), DetailOf(Pinned, "api", PinnedVersions.FileName));

        // A pin that was read says the version and the nodes, and never the file it came out of.
        var read = VersionAssessment.Assess(Pinned, "ui", [West(Healthy, "2.4.7")], "kustomization.yaml");

        Assert.Equal("Pinned 2.4.7", read.Headline);
        Assert.DoesNotContain("kustomization", read.Text, StringComparison.Ordinal);
    }

    // ----- Assess: the readings that leave no version pinned

    /// <summary>A reading, a deployable, the state it leads to and the two sentences it is said in: one row of the
    /// table both the theory and the loop below read, public because a theory's data is handed to it as a member of
    /// this class.</summary>
    public sealed record Unpinned(
        PinnedVersions Reading,
        string Deployable,
        VersionState State,
        string Headline,
        string? Detail);

    /// <summary>The sentence the decision says of a deployable a reading holds no version for, named as the caller
    /// named it: one shape of words for the key that is not in the file, the key that is there and says nothing, and
    /// the name of nothing at all.</summary>
    private static string NoEntry(string deployable) =>
        $"versions.json has no entry for {deployable}: it was not deployed here yet.";

    private static readonly IReadOnlyList<Unpinned> NothingPinned =
    [
        new(PinnedVersions.NotTracked, "ui", VersionState.Pending, "Reading the pinned version", null),
        new(PinnedVersions.Pending, "ui", VersionState.Pending, "Reading the pinned version", null),
        new(PinnedVersions.Missing, "ui", VersionState.NotDeployed, "No pinned version", "versions.json was not found: nothing was deployed here yet, or the repository is not public."),
        new(PinnedVersions.Unavailable("the server answered HTTP 503"), "ui", VersionState.PinnedUnknown, "Pinned version not known", "versions.json could not be read: the server answered HTTP 503."),
        new(PinnedVersions.Unavailable(null!), "ui", VersionState.PinnedUnknown, "Pinned version not known", "versions.json could not be read: no reason given."),
        new(PinnedVersions.Unavailable(string.Empty), "ui", VersionState.PinnedUnknown, "Pinned version not known", "versions.json could not be read: ."),
        new(Read(string.Empty), "ui", VersionState.NotDeployed, "No pinned version", NoEntry("ui")),
        new(Read("   "), "ui", VersionState.NotDeployed, "No pinned version", NoEntry("ui")),
        new(Read("+0a1b2c3"), "ui", VersionState.NotDeployed, "No pinned version", NoEntry("ui")),
        new(new PinnedVersions(PinnedVersionsState.Read, new Dictionary<string, string>(StringComparer.Ordinal)), "ui", VersionState.NotDeployed, "No pinned version", NoEntry("ui")),
        new(Pinned, "api", VersionState.NotDeployed, "No pinned version", NoEntry("api")),
        new(Pinned, string.Empty, VersionState.NotDeployed, "No pinned version", NoEntry(string.Empty)),
        new(Pinned, " ", VersionState.NotDeployed, "No pinned version", NoEntry(" ")),
    ];

    public static TheoryData<Unpinned> ReadingsThatPinNothing() => new([.. NothingPinned]);

    /// <summary>Every reading that pins nothing the decision could compare is answered the same way, whichever nodes
    /// stand ready to be compared: the state that says so, no pinned version, no node in any of the three lists - the
    /// nodes are not even read - the headline the state is said by, the sentence that explains it where the file has
    /// one, and the whole line the two make. A reason that was never given is said as <c>no reason given</c> while the
    /// empty word is a reason the file hands over as it stands, and an entry of nothing but spaces or but build
    /// metadata is no entry at all: the decision reads the pin through the same display every version goes by.</summary>
    [Theory]
    [MemberData(nameof(ReadingsThatPinNothing))]
    public void AReadingThatPinsNothingIsAnsweredBeforeTheNodesAreRead(Unpinned unpinned)
    {
        var assessment = VersionAssessment.Assess(unpinned.Reading, unpinned.Deployable, [West(Healthy, "2.4.7"), East(Unreachable, "2.4.6")]);

        Assert.Equal(unpinned.State, assessment.State);
        Assert.Equal(unpinned.State == VersionState.Differs, assessment.Differs);
        Assert.False(assessment.Differs);
        Assert.Null(assessment.Pinned);
        Assert.Equal(unpinned.Headline, assessment.Headline);
        Assert.Equal(unpinned.Detail, assessment.Detail);
        Assert.Equal(unpinned.Detail is null ? unpinned.Headline : $"{unpinned.Headline}. {unpinned.Detail}", assessment.Text);
        Assert.Empty(assessment.Matching);
        Assert.Empty(assessment.Differing);
        Assert.Empty(assessment.Unknown);
    }

    /// <summary>A deployable is looked for by the very name the caller gave it, both ways round: the empty word and a
    /// blank are no names in a file that pins <c>ui</c>, and a file that pins the empty name is read by a caller that
    /// asks for the empty name - while the name of the deployable is never trimmed, so the spaces about it ask for
    /// something the file does not hold.</summary>
    [Fact]
    public void TheDeployableIsLookedForByTheVeryNameTheCallerGivesIt()
    {
        Assert.Equal(VersionState.NotDeployed, VersionAssessment.Assess(Pinned, "ui ", [West(Healthy, "2.4.7")]).State);
        Assert.Equal(VersionState.NotDeployed, VersionAssessment.Assess(Pinned, " UI", [West(Healthy, "2.4.7")]).State);

        var nameless = new PinnedVersions(
            PinnedVersionsState.Read,
            new Dictionary<string, string>(StringComparer.Ordinal) { [string.Empty] = "2.4.7" });

        Assert.Equal(VersionState.InSync, VersionAssessment.Assess(nameless, string.Empty, [West(Healthy, "2.4.7")]).State);
        Assert.Equal(VersionState.NotDeployed, VersionAssessment.Assess(nameless, "ui", [West(Healthy, "2.4.7")]).State);
    }

    // ----- Assess: the nodes it reads and the three lists it makes

    /// <summary>The nodes are read once, into the three lists, and only once the reading has pinned a version: a
    /// sequence that counts the times it is walked is walked a single time by a decision that compares and not at all
    /// by one that has nothing to compare with. The walk happens while the decision is made rather than later, so a
    /// sequence that throws on its first read hands the throw straight back to the caller.</summary>
    [Fact]
    public void TheNodesAreReadOnceAndOnlyOnceThePinIsKnown()
    {
        var walks = 0;

        IEnumerable<NodeVersion> Counted()
        {
            walks++;
            yield return West(Healthy, "2.4.7");
            yield return East(Healthy, "2.4.6");
        }

        var assessment = VersionAssessment.Assess(Pinned, "ui", Counted());

        Assert.Equal(1, walks);
        Assert.Equal(VersionState.Differs, assessment.State);
        Assert.Equal(2, assessment.Matching.Count + assessment.Differing.Count + assessment.Unknown.Count);

        foreach (var unpinned in NothingPinned)
        {
            walks = 0;

            Assert.Null(VersionAssessment.Assess(unpinned.Reading, unpinned.Deployable, Counted()).Pinned);
            Assert.Equal(0, walks);
        }

        Assert.Throws<InvalidOperationException>(() => VersionAssessment.Assess(Pinned, "ui", Throwing()));
    }

    /// <summary>The count the in-sync sentence says is the three lists together: the unknown nodes are counted in the
    /// total although they stand out of the comparison, and a node that differs never reaches the in-sync count.</summary>
    [Fact]
    public void TheTotalInTheWordsIsTheThreeListsTogether()
    {
        var south = new NodeVersion("southcentralus", Unreachable, "2.4.7");
        var north = new NodeVersion("north europe", Pending, null);

        var assessment = Assess(West(Healthy, "2.4.7"), south, East(Unhealthy, "2.4.7+0a1b2c3"), north);

        Assert.Equal(VersionState.InSync, assessment.State);
        Assert.Equal(4, assessment.Matching.Count + assessment.Differing.Count + assessment.Unknown.Count);
        Assert.Equal(
            "In sync: 2 of 4 nodes run 2.4.7; southcentralus is unreachable, north europe is being checked.",
            assessment.Detail);
    }

    private static IEnumerable<NodeVersion> Throwing()
    {
        yield return West(Healthy, "2.4.7");
        throw new InvalidOperationException("the nodes went away while they were being read");
    }

    /// <summary>Every node the caller handed stands in exactly one of the three lists, in the order it was handed and
    /// as the very node it was handed as - the decision copies no node and orders none. A node handed twice stands
    /// twice, and no nodes at all are three empty lists. The three lists together are the nodes: none is left out and
    /// none is counted twice, and a difference next to two unknown nodes says all three of them in one sentence.</summary>
    [Fact]
    public void EveryNodeLandsInOneOfTheThreeListsInItsOwnPlace()
    {
        var west = West(Healthy, "2.4.7");
        var east = East(Healthy, "2.4.6");
        var south = new NodeVersion("southcentralus", Unreachable, "2.4.7");
        var north = new NodeVersion("north europe", Unhealthy, null);
        var again = West(Healthy, "2.4.7+0a1b2c3");

        var assessment = VersionAssessment.Assess(Pinned, "ui", [west, east, south, north, again]);

        Assert.Equal(VersionState.Differs, assessment.State);
        Assert.Equal(["westus3", "westus3"], assessment.Matching.Select(node => node.Label));
        Assert.Same(west, assessment.Matching[0]);
        Assert.Same(again, assessment.Matching[1]);
        Assert.Same(east, Assert.Single(assessment.Differing));
        Assert.Equal(["southcentralus", "north europe"], assessment.Unknown.Select(node => node.Label));
        Assert.Same(south, assessment.Unknown[0]);
        Assert.Same(north, assessment.Unknown[1]);
        Assert.Equal(5, assessment.Matching.Count + assessment.Differing.Count + assessment.Unknown.Count);
        Assert.Equal(
            "Differs: eastus2 runs 2.4.6; southcentralus is unreachable, north europe reports no version.",
            assessment.Detail);

        // Made once, and read from then on: the lists are the decision's own, standing the same from read to read.
        Assert.Same(assessment.Matching, assessment.Matching);
        Assert.Same(assessment.Unknown, assessment.Unknown);
        Assert.NotSame(assessment.Matching, assessment.Differing);

        // A node handed twice is a node twice over, and no nodes at all are three empty lists.
        var twice = Assess(west, west);

        Assert.Equal(2, twice.Matching.Count);
        Assert.Empty(twice.Differing);
        Assert.Empty(twice.Unknown);

        var none = Assess();

        Assert.All([none.Matching, none.Differing, none.Unknown], list => Assert.Empty(list));
    }

    /// <summary>A node that answered with build metadata and nothing else before it runs no version, so it stands in
    /// the unknown list and never in the other two: it neither differs from the pin nor agrees with it, whatever state
    /// the rest of the deployable is in. The pinned version itself is said the same way - the file's text read through
    /// the display every version goes by, so the metadata and the spaces about it are out of the line the page shows.</summary>
    [Fact]
    public void AVersionOfNothingButBuildMetadataIsNoVersionAtEitherEnd()
    {
        var silent = West(Healthy, "+0a1b2c3");
        var unknown = Assess(silent, East(Healthy, "2.4.6"));

        Assert.Equal(VersionState.Differs, unknown.State);
        Assert.Same(silent, Assert.Single(unknown.Unknown));
        Assert.DoesNotContain(silent, unknown.Matching);
        Assert.Equal("Differs: eastus2 runs 2.4.6; westus3 reports no version.", unknown.Detail);

        var alone = Assess(silent);

        Assert.Equal(VersionState.NodesUnknown, alone.State);
        Assert.Equal("Not compared: westus3 reports no version.", alone.Detail);

        // The pin the file wrote with metadata and spaces about it is pinned without either of them.
        var pinned = VersionAssessment.Assess(Read(" 2.4.7+0a1b2c3 "), "ui", [East(Healthy, "2.4.7")]);

        Assert.Equal("2.4.7", pinned.Pinned);
        Assert.Equal(VersionState.InSync, pinned.State);
        Assert.Equal("Pinned 2.4.7. In sync: eastus2 runs 2.4.7.", pinned.Text);
    }

    // ----- VersionAssessment: the seven parts and the record around them

    /// <summary>The record reads nothing out of anything: the seven parts stand where they are handed - a state the
    /// enum does not name as the number it is, a pinned version as the text it was given, and the three lists as the
    /// very lists the caller holds - and only the two words are worked out of them. No part is asked after another:
    /// a difference with no differing node in it differs, and a pin that was never read stands under a headline that
    /// names one.</summary>
    [Fact]
    public void TheAssessmentKeepsItsSevenPartsAsTheyAreHanded()
    {
        var matching = new List<NodeVersion> { West(Healthy, "2.4.6") };
        var differing = new List<NodeVersion> { East(Unhealthy, "2.4.6") };
        var unknown = new List<NodeVersion> { new("southcentralus", Unreachable, "2.4.7") };

        var assessment = new VersionAssessment((VersionState)7, " 2.4.7+0a1b2c3 ", matching, differing, unknown, "Pinned", null);

        Assert.Equal((VersionState)7, assessment.State);
        Assert.Equal(" 2.4.7+0a1b2c3 ", assessment.Pinned);
        Assert.Same(matching, assessment.Matching);
        Assert.Same(differing, assessment.Differing);
        Assert.Same(unknown, assessment.Unknown);
        Assert.Equal("Pinned", assessment.Headline);
        Assert.Null(assessment.Detail);
        Assert.Equal("Pinned", assessment.Text);
        Assert.False(assessment.Differs);
        Assert.True((assessment with { State = VersionState.Differs }).Differs);

        // A state the enum names nothing for is kept as the number it is, and differs no more than the in-sync one.
        Assert.False((assessment with { State = (VersionState)(-1) }).Differs);
    }

    /// <summary>Two decisions over one topology are two assessments: each makes its own three lists, and a list is
    /// compared as the list it is and not node by node, so the same nodes decided twice are never one - which is why
    /// a caller that asks whether this round is the last one compares the words and not the records. Two decisions
    /// that made no list of any node are one, because the runtime hands out one empty list to whoever asks for it.</summary>
    [Fact]
    public void TwoDecisionsAreTwoAssessmentsBecauseEachMadeItsOwnLists()
    {
        var nodes = new[] { West(Healthy, "2.4.7") };

        var one = VersionAssessment.Assess(Pinned, "ui", nodes);
        var twice = VersionAssessment.Assess(Pinned, "ui", nodes);

        Assert.Equal(one.State, twice.State);
        Assert.Equal(one.Text, twice.Text);
        Assert.False(one.Equals(twice));
        Assert.False(one == twice);
        Assert.True(one != twice);
        Assert.NotEqual(one.GetHashCode(), twice.GetHashCode());

        // The copy carries the very lists along, and so is one with the assessment it was made from.
        var copy = one with { };

        Assert.True(one == copy);
        Assert.False(one != copy);
        Assert.NotSame(one, copy);
        Assert.Same(one.Matching, copy.Matching);
        Assert.Equal(one.GetHashCode(), copy.GetHashCode());
        Assert.Single([.. new HashSet<VersionAssessment> { one, copy }]);

        // Every part on its own makes two, the absence of a second sentence included.
        Assert.False(one == one with { State = VersionState.NodesUnknown });
        Assert.False(one == one with { Pinned = null });
        Assert.False(one == one with { Matching = twice.Matching });
        Assert.False(one == one with { Differing = [] });
        Assert.False(one == one with { Unknown = [] });
        Assert.False(one == one with { Headline = "Pinned" });
        Assert.False(one == one with { Detail = null });

        // Two assessments that made no list of any node are one, the empty lists they hold being the one array the
        // runtime hands out to whoever asks for it.
        Assert.Equal(VersionAssessment.Assess(PinnedVersions.Missing, "ui", []), VersionAssessment.Assess(PinnedVersions.Missing, "ui", []));

        // What is no assessment is nobody's equal, and the nothing a caller holds is not one either.
        VersionAssessment? absent = null;

        Assert.False(one.Equals(null));
        Assert.False(one.Equals(nodes[0]));
        Assert.False(one.Equals(VersionState.InSync));
        Assert.False(one == absent);
        Assert.False(absent == one);
        Assert.True(absent == null);
    }

    /// <summary>The seven parts fall out in the order the record declares them - state, pin, the three lists, the two
    /// sentences - as the parts they went in as, list for list and node for node; and the words the assessment says of
    /// itself name the seven parts it was handed and the two worked out of them, in that order.</summary>
    [Fact]
    public void TheSevenPartsFallApartInTheOrderTheyWereHanded()
    {
        var west = West(Healthy, "2.4.7");
        var east = East(Healthy, "2.4.6");
        var south = new NodeVersion("southcentralus", Pending, null);
        var first = VersionAssessment.Assess(Pinned, "ui", [west, east, south]);

        var (state, pinned, matching, differing, unknown, headline, detail) = first;

        Assert.Equal(VersionState.Differs, state);
        Assert.Equal("2.4.7", pinned);
        Assert.Equal(["westus3"], matching.Select(node => node.Label));
        Assert.Same(west, matching[0]);
        Assert.Equal(["eastus2"], differing.Select(node => node.Label));
        Assert.Same(east, differing[0]);
        Assert.Equal(["southcentralus"], unknown.Select(node => node.Label));
        Assert.Same(south, unknown[0]);
        Assert.Equal("Pinned 2.4.7", headline);
        Assert.Equal("Differs: eastus2 runs 2.4.6; southcentralus is being checked.", detail);

        // Put back in the order they came out, the seven parts are the very assessment again: the three lists come
        // back as the references they were, and a record holding them is one with the decision that made them.
        Assert.True(first == new VersionAssessment(state, pinned, matching, differing, unknown, headline, detail));

        Assert.Equal(
            "VersionAssessment { State = InSync, Pinned = 2.4.7, Matching = System.Collections.Generic.List`1[Dashboard.Health.NodeVersion], "
            + "Differing = System.Collections.Generic.List`1[Dashboard.Health.NodeVersion], "
            + "Unknown = System.Collections.Generic.List`1[Dashboard.Health.NodeVersion], "
            + "Headline = Pinned 2.4.7, Detail = In sync: westus3 runs 2.4.7., Differs = False, "
            + "Text = Pinned 2.4.7. In sync: westus3 runs 2.4.7. }",
            VersionAssessment.Assess(Pinned, "ui", [west]).ToString());

        // The lists a decision made are said as lists are said, node for node in nothing of their own; the empty ones
        // of an assessment that compared nothing are said as the array they are, and its absent pin and sentence as
        // nothing at all between their commas.
        Assert.Equal(
            "VersionAssessment { State = Pending, Pinned = , Matching = Dashboard.Health.NodeVersion[], "
            + "Differing = Dashboard.Health.NodeVersion[], Unknown = Dashboard.Health.NodeVersion[], "
            + "Headline = Reading the pinned version, Detail = , Differs = False, Text = Reading the pinned version }",
            VersionAssessment.Assess(PinnedVersions.Pending, "ui", []).ToString());
    }

    // ----- VersionSummary: the one count the header shows

    /// <summary>The count is kept as it was given and said as it is: nothing at all, one environment, and the plural
    /// the day after it, up to the top of the count and back down through the numbers no count of environments is ever
    /// - every one of them under zero answered with the silence of the version that differs nowhere, so no negative
    /// can be shown as a difference.</summary>
    [Theory]
    [InlineData(int.MinValue, false, null)]
    [InlineData(-2, false, null)]
    [InlineData(-1, false, null)]
    [InlineData(0, false, null)]
    [InlineData(1, true, "Versions differ in 1 environment")]
    [InlineData(2, true, "Versions differ in 2 environments")]
    [InlineData(3, true, "Versions differ in 3 environments")]
    [InlineData(int.MaxValue, true, "Versions differ in 2147483647 environments")]
    public void TheSummaryCountsEnvironmentsFromBelowNoneOfThemToAllOfThem(int environments, bool differs, string? text)
    {
        var summary = new VersionSummary(environments);

        Assert.Equal(environments, summary.EnvironmentsThatDiffer);
        Assert.Equal(differs, summary.Differs);
        Assert.Equal(differs, summary.EnvironmentsThatDiffer > 0);
        Assert.Equal(text, summary.Text);
    }

    /// <summary>The one count stands where it is handed, the two words are worked out of it and of nothing else, and
    /// the record around it is the record the compiler builds: two summaries of one count are one, a copy changes the
    /// count it names and carries the rest along - which is all it can carry - and the words it says of itself are the
    /// count and the two words worked out of it.</summary>
    [Fact]
    public void TheSummaryIsOneCountThatSaysItselfInWords()
    {
        var three = new VersionSummary(3);

        Assert.Equal(3, three.EnvironmentsThatDiffer);
        Assert.Equal(new VersionSummary(3), three);
        Assert.Equal(new VersionSummary(3).GetHashCode(), three.GetHashCode());
        Assert.True(new VersionSummary(3) == three);
        Assert.False(new VersionSummary(2) == three);
        Assert.False(new VersionSummary(-1) == three);
        Assert.False(new VersionSummary(0) == new VersionSummary(-1));
        Assert.Single([.. new HashSet<VersionSummary> { three, new VersionSummary(3) }]);

        three.Deconstruct(out var environments);

        Assert.Equal(3, environments);

        var copy = three with { };

        Assert.Equal(three, copy);
        Assert.NotSame(three, copy);
        Assert.Equal("Versions differ in 9 environments", (three with { EnvironmentsThatDiffer = 9 }).Text);
        Assert.False((three with { EnvironmentsThatDiffer = 0 }).Differs);
        Assert.Null((three with { EnvironmentsThatDiffer = 0 }).Text);

        Assert.Equal(
            "VersionSummary { EnvironmentsThatDiffer = 3, Differs = True, Text = Versions differ in 3 environments }",
            three.ToString());
        Assert.Equal(
            "VersionSummary { EnvironmentsThatDiffer = 0, Differs = False, Text =  }",
            new VersionSummary(0).ToString());

        Assert.False(new VersionSummary(3).Equals(null));
        Assert.False(new VersionSummary(3).Equals(3));
        Assert.False(new VersionSummary(3).Equals((object)"Versions differ in 3 environments"));
    }

    // ----- the surfaces the four types declare

    /// <summary>The whole public surface <see cref="VersionAssessment" /> declares, as the running assembly holds it:
    /// the seven parts, the two words worked out of them, the one decision, and everything the compiler adds to a
    /// positional record - and nothing else to call. A member added has to be written here as well, which is the
    /// point: the page and the runtime view read these seven parts and no others.</summary>
    [Fact]
    public void TheAssessmentSurfaceIsItsSevenPartsItsTwoWordsAndTheDecision()
    {
        Assert.Equal(
            ["Assess", "Deconstruct", "Detail", "Differing", "Differs", "Equals", "GetHashCode", "Headline", "Matching", "Pinned", "State", "Text", "ToString", "Unknown", "op_Equality", "op_Inequality"],
            PublicSurface(typeof(VersionAssessment)));

        var constructor = Assert.Single(typeof(VersionAssessment).GetConstructors(BindingFlags.Public | BindingFlags.Instance));

        Assert.Equal(
            ["State", "Pinned", "Matching", "Differing", "Unknown", "Headline", "Detail"],
            [.. constructor.GetParameters().Select(parameter => parameter.Name!)]);
        Assert.All(constructor.GetParameters(), parameter => Assert.False(parameter.HasDefaultValue));

        Assert.Equal(
            ["Detail", "Differing", "Differs", "Headline", "Matching", "Pinned", "State", "Text", "Unknown"],
            [.. typeof(VersionAssessment)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Select(property => property.Name)
                .Order(StringComparer.Ordinal)]);

        // The two words are read only, as the sentences the decision works out; the seven parts are written at the making.
        string[] sentences = ["Differs", "Text"];
        string[] parts = ["State", "Pinned", "Matching", "Differing", "Unknown", "Headline", "Detail"];

        Assert.All(
            sentences.Select(name => typeof(VersionAssessment).GetProperty(name, BindingFlags.Public | BindingFlags.Instance)!),
            word => Assert.False(word.CanWrite));
        Assert.All(
            parts.Select(name => typeof(VersionAssessment).GetProperty(name, BindingFlags.Public | BindingFlags.Instance)!),
            part => Assert.True(part.CanWrite));

        var assess = Assert.Single(typeof(VersionAssessment).GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly), method => method.Name == "Assess");
        var parameters = assess.GetParameters();

        Assert.Equal(["pinned", "deployable", "nodes", "file"], [.. parameters.Select(parameter => parameter.Name!)]);
        Assert.All(parameters.Take(3), parameter => Assert.False(parameter.HasDefaultValue));

        // The name of the file is the one part a caller may leave unsaid, and unsaid it is defaulted to nothing at all:
        // the decision is the place that turns that nothing into versions.json.
        Assert.True(parameters[3].HasDefaultValue);
        Assert.Null(parameters[3].DefaultValue);
    }

    /// <summary>The surface of <see cref="NodeVersion" /> and of <see cref="VersionSummary" />, the two records the
    /// decision is worked out of and worked into: three parts and two words, and one count and two words, each with
    /// the making, the copy, the equality and the operators the compiler builds around them - the nodes and the count
    /// are what the page and the runtime view reach for, and nothing else is there to reach for.</summary>
    [Fact]
    public void TheNodeAndTheSummaryDeclareTheirPartsAndNothingElse()
    {
        Assert.Equal(
            ["Deconstruct", "Equals", "GetHashCode", "Label", "Running", "State", "ToString", "UnknownClause", "UnknownReason", "Version", "op_Equality", "op_Inequality"],
            PublicSurface(typeof(NodeVersion)));
        Assert.Equal(
            ["Deconstruct", "Differs", "EnvironmentsThatDiffer", "Equals", "GetHashCode", "Text", "ToString", "op_Equality", "op_Inequality"],
            PublicSurface(typeof(VersionSummary)));

        var node = Assert.Single(typeof(NodeVersion).GetConstructors(BindingFlags.Public | BindingFlags.Instance));

        Assert.Equal(["Label", "State", "Version"], [.. node.GetParameters().Select(parameter => parameter.Name!)]);
        Assert.All(node.GetParameters(), parameter => Assert.False(parameter.HasDefaultValue));

        var summary = Assert.Single(typeof(VersionSummary).GetConstructors(BindingFlags.Public | BindingFlags.Instance));

        Assert.Equal(["EnvironmentsThatDiffer"], [.. summary.GetParameters().Select(parameter => parameter.Name!)]);
        Assert.Equal(typeof(int), summary.GetParameters()[0].ParameterType);
        Assert.False(summary.GetParameters()[0].HasDefaultValue);
    }

    // ---------------------------------------------------------------------------------- the machinery

    /// <summary>The public members a type declares itself: its properties and the methods that are not the get and the
    /// set of one - the two operators kept although the compiler names them specially, since they are the <c>==</c>
    /// and the <c>!=</c> a caller writes - with the record's compiler-named clone walked past.</summary>
    private static List<string> PublicSurface(Type type) =>
        [.. type
            .GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(member => member is PropertyInfo
                             || member is MethodInfo method
                             && (!method.IsSpecialName || method.Name.StartsWith("op_", StringComparison.Ordinal)))
            .Where(member => !member.Name.Contains('<', StringComparison.Ordinal))
            .Select(member => member.Name)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];
}
