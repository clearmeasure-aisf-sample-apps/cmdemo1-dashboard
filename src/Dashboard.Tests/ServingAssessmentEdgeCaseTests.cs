using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="ServingAssessment" /> - the record
/// <c>src/Dashboard/Health/ServingAssessment.cs</c> declares for which node of a deployable is expected to serve its
/// traffic, and whether the Front Door endpoint agrees: the seven parts the record keeps, the seven words and lists it
/// works out of them, the one decision <see cref="ServingAssessment.Assess" /> makes, and the record the compiler
/// builds around the seven parts.
/// <para>
/// What is pinned where it belongs, and is not repeated here: <see cref="ServingAssessmentTests" /> pins the decision
/// for a primary and a standby - which state a set of nodes lands in, the headline and the failover detail of each,
/// the one node of a deployable without a Front Door endpoint serving or not serving, and the Front Door agreeing or
/// disagreeing over the states a check gives; <see cref="RotationTests" /> pins the same for a rotation - every
/// healthy primary serving, the rotation's headline naming up to three stopped regions of five and the fourth as one
/// more, the standby taking over when no primary is healthy, and what the runtime view draws from all of it;
/// <see cref="ServingStateEdgeCaseTests" /> pins the five states as a vocabulary, the state an assessment keeps even
/// when it is none of the five, and <see cref="ServingState.FailedOver" /> as the only state that has failed over;
/// and <see cref="NodeHealthEdgeCaseTests" /> pins the node every part is made of, label included.
/// </para>
/// <para>
/// What all of that leaves open, and what is pinned here: the argument <c>Assess</c> refuses by name, and the nodes it
/// keeps as the very instances it was handed - which is how a tile is matched back to the node it shows; the rotation
/// as the list it is (the primary nodes alone, in the order of the topology, never the caller's own list, and an empty
/// list still a rotation); the two lists worked out of it, a fresh answer on every read, the rotation's words ignoring
/// the node named as the expected one, and an unchecked or an impossible primary standing out of it; the headline of
/// the one deployable still being checked, the rotation's words winning over a single node's, a rotation with no nodes
/// in it at all, and the naming stopping at <see cref="ServingAssessment.MostNamed" /> - that constant itself, three,
/// and a constant; the failover reason refusing to say anything for a single node that failed over, and saying the
/// state it blames in the words of that state, the unchecked one and the state the enum does not name included; the
/// Front Door sentence reading the agreement first and the state second, and falling back to its own words for a
/// state it was never told and a node it was never given; the seven parts standing where they are handed, the last
/// two of them absent when they are not handed at all; and the record around them - two assessments being one while
/// their parts are, a rotation being one with the list it holds and with no copy of it, the seven parts falling apart,
/// the copy that moves one part and carries six, the words the assessment says of itself, and its surface, so that the
/// day an eighth part is added the list below says so.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: an assessment is worked out of the
/// states a probe left behind and says words about them, it holds no moment and opens no address, and every node,
/// state and agreement below is a value the test brings.
/// </para>
/// </summary>
public class ServingAssessmentEdgeCaseTests
{
    private const HealthState Healthy = HealthState.Healthy;
    private const HealthState Unhealthy = HealthState.Unhealthy;
    private const HealthState Unreachable = HealthState.Unreachable;
    private const HealthState Pending = HealthState.Pending;

    /// <summary>A state the enum does not name, which the record is as capable of holding as any of the four.</summary>
    private const HealthState Unknown = (HealthState)(-1);

    /// <summary>The fourteen members an assessment says of itself, in the order the record declares them.</summary>
    private static readonly string[] Spoken =
    [
        "State = ", "Expected = ", "FailedPrimary = ", "FrontDoor = ", "FrontDoorState = ", "OnlyNode = ", "Rotation = ",
        "IsFailedOver = ", "IsRotation = ", "Serving = ", "OutOfRotation = ", "Headline = ", "FailoverDetail = ", "FrontDoorText = ",
    ];

    private static NodeHealth Primary(HealthState state, string region) => new($"app-{region}", region, true, state);

    private static NodeHealth Standby(HealthState state, string region) => new($"app-{region}", region, false, state);

    private static string[] Labels(IReadOnlyList<NodeHealth> nodes) => [.. nodes.Select(node => node.Label)];

    /// <summary>How often a piece of text says a word.</summary>
    private static int Times(string text, string word)
    {
        var count = 0;

        for (var at = text.IndexOf(word, StringComparison.Ordinal); at >= 0; at = text.IndexOf(word, at + word.Length, StringComparison.Ordinal))
        {
            count++;
        }

        return count;
    }

    // ----- Assess: the argument it refuses, the nodes it keeps

    /// <summary>The nodes are the one part of the decision an assessment cannot be worked out without, and the
    /// refusal says which argument it missed by name, whatever the Front Door endpoint is. A sequence that is there
    /// and holds nothing is no such thing: it is the deployable whose topology names no node, answered with the state
    /// of it and with nothing named in any of the parts that can hold a node.</summary>
    [Theory]
    [InlineData(Pending, FrontDoorAgreement.Pending)]
    [InlineData(Unhealthy, FrontDoorAgreement.Pending)]
    [InlineData(null, FrontDoorAgreement.NotPresent)]
    public void AListOfNoNodesIsAnsweredAndAListOfNothingIsRefusedByName(HealthState? frontDoor, FrontDoorAgreement agreement)
    {
        var error = Assert.Throws<ArgumentNullException>(() => ServingAssessment.Assess(null!, frontDoor));

        Assert.Equal("nodes", error.ParamName);

        var none = ServingAssessment.Assess([], frontDoor);

        Assert.Equal(ServingState.NoNodes, none.State);
        Assert.Equal(agreement, none.FrontDoor);
        Assert.Equal(frontDoor, none.FrontDoorState);
        Assert.Null(none.Expected);
        Assert.Null(none.FailedPrimary);
        Assert.Null(none.OnlyNode);
        Assert.Null(none.Rotation);
        Assert.False(none.IsRotation);
        Assert.Empty(none.Serving);
        Assert.Empty(none.OutOfRotation);
        Assert.Equal("No nodes in the topology", none.Headline);
        Assert.Null(none.FailoverDetail);
    }

    /// <summary>The nodes an assessment names are the very instances it was handed, not copies of them and not other
    /// nodes of the same four parts: <c>Expected</c>, <c>FailedPrimary</c>, <c>OnlyNode</c>, every node of the
    /// rotation and every node of the two lists answer to <see cref="object.ReferenceEquals" />, which is what lets
    /// the page match a serving node back to the tile that draws it.</summary>
    [Fact]
    public void TheAssessmentNamesTheVeryNodesItWasHanded()
    {
        var failed = Primary(Unhealthy, "westus3");
        var serves = Standby(Healthy, "eastus2");

        var assessment = ServingAssessment.Assess([failed, serves], Healthy);

        Assert.Same(failed, assessment.FailedPrimary);
        Assert.Same(serves, assessment.Expected);
        Assert.Same(serves, Assert.Single(assessment.Serving));

        var only = Primary(Healthy, "westus3");
        var single = ServingAssessment.Assess([only], null);

        Assert.Same(only, single.OnlyNode);
        Assert.Same(only, single.Expected);
        Assert.Same(only, Assert.Single(single.Serving));

        var first = Primary(Healthy, "a");
        var second = Primary(Unreachable, "b");
        var rotation = ServingAssessment.Assess([first, second], Healthy);

        Assert.Same(first, rotation.Expected);
        Assert.Same(first, rotation.Rotation![0]);
        Assert.Same(second, rotation.Rotation![1]);
        Assert.Same(first, Assert.Single(rotation.Serving));
        Assert.Same(second, Assert.Single(rotation.OutOfRotation));
    }

    /// <summary>The decision is taken out of the nodes as they were handed and kept apart from the caller's own list:
    /// emptying that list afterwards moves nothing the assessment says, and the rotation is not the sequence the
    /// caller passed in - which is what the page needs when it hands the same nodes round after round.</summary>
    [Fact]
    public void TheCallersOwnListIsNeitherTheRotationNorAnythingTheRotationFollows()
    {
        var nodes = new List<NodeHealth> { Primary(Healthy, "a"), Primary(Unhealthy, "b") };

        var assessment = ServingAssessment.Assess(nodes, Healthy);

        nodes.Clear();
        nodes.Add(Primary(Unreachable, "c"));

        Assert.NotSame(nodes, assessment.Rotation);
        Assert.Equal(["a", "b"], Labels(assessment.Rotation!));
        Assert.Equal(["a"], Labels(assessment.Serving));
        Assert.Equal(["b"], Labels(assessment.OutOfRotation));
        Assert.Equal("Front Door rotates over 1 of 2 regions: b is not healthy", assessment.Headline);
    }

    // ----- Rotation: which nodes, in what order, and when there is one at all

    /// <summary>The rotation is the primary nodes and only they, in the order of the topology and not in the order of
    /// their states or their names: a standby between two primaries belongs to neither list, an unhealthy primary
    /// stays in the rotation it is out of serving, and the part read twice is the same list both times.</summary>
    [Fact]
    public void TheRotationHoldsThePrimaryNodesAloneInTheOrderOfTheTopology()
    {
        var west = Primary(Healthy, "a");
        var north = Standby(Unhealthy, "z");
        var east = Primary(Unhealthy, "b");
        var far = Standby(Healthy, "y");

        var assessment = ServingAssessment.Assess([west, north, east, far], Healthy);

        Assert.Equal(["a", "b"], Labels(assessment.Rotation!));
        Assert.Same(assessment.Rotation, assessment.Rotation);
        Assert.Equal(["a"], Labels(assessment.Serving));
        Assert.Equal(["b"], Labels(assessment.OutOfRotation));
        Assert.DoesNotContain(far, assessment.Rotation!);
        Assert.DoesNotContain(north, assessment.OutOfRotation);
    }

    /// <summary>A rotation is a Front Door endpoint and more than one primary: one primary behind an endpoint is no
    /// rotation, several primaries without an endpoint are no rotation, and an endpoint that has not been checked is
    /// an endpoint all the same - so the rotation stands, with all of its primaries in it, while the agreement says
    /// nothing is decided yet.</summary>
    [Theory]
    [InlineData(new HealthState[] { Healthy }, true, null)]
    [InlineData(new HealthState[] { Healthy }, false, null)]
    [InlineData(new HealthState[] { Pending }, true, null)]
    [InlineData(new HealthState[] { Healthy, Unhealthy }, false, null)]
    [InlineData(new HealthState[] { Healthy, Unhealthy }, true, 2)]
    [InlineData(new HealthState[] { Healthy, Healthy, Unreachable }, true, 3)]
    [InlineData(new HealthState[] { Unreachable, Unreachable, Unreachable }, true, 3)]
    public void ARotationIsSeveralPrimariesBehindAFrontDoorEndpoint(HealthState[] primaries, bool frontDoor, int? count)
    {
        var nodes = new List<NodeHealth>();

        nodes.AddRange(primaries.Select((state, index) => Primary(state, $"r{index + 1}")));
        nodes.Add(Standby(Healthy, "standby"));

        var assessment = ServingAssessment.Assess(nodes, frontDoor ? Healthy : null);

        Assert.Equal(count, assessment.Rotation is null ? null : assessment.Rotation.Count);
        Assert.Equal(count is not null, assessment.IsRotation);
    }

    /// <summary>An endpoint that is being checked leaves the rotation standing and the agreement undecided: the
    /// primary nodes are all of them there, the healthy ones of them serve, and the sentence about the endpoint says
    /// the check and nothing of a state.</summary>
    [Fact]
    public void APendingFrontDoorEndpointIsStillAnEndpointAndItsRotationStillARotation()
    {
        var assessment = ServingAssessment.Assess([Primary(Healthy, "a"), Primary(Healthy, "b")], Pending);

        Assert.Equal(2, assessment.Rotation!.Count);
        Assert.True(assessment.IsRotation);
        Assert.Equal(ServingState.Primary, assessment.State);
        Assert.Equal(FrontDoorAgreement.Pending, assessment.FrontDoor);
        Assert.Equal(Pending, assessment.FrontDoorState);
        Assert.Equal("Front Door is being checked.", assessment.FrontDoorText);
        Assert.Equal(["a", "b"], Labels(assessment.Serving));
    }

    /// <summary>The empty rotation is the boundary of the member: <c>Rotation</c> is asked whether it is there, never
    /// whether it holds anything, so a rotation of no primary nodes at all is a rotation - it says so, it names no
    /// node out of itself, and its headline counts a rotation of nothing as all healthy. No decision
    /// <see cref="ServingAssessment.Assess" /> reaches this: the file hands over a rotation of two primaries or more,
    /// and the record keeps whatever else it is given.</summary>
    [Fact]
    public void ARotationOfNoNodesIsStillARotation()
    {
        var empty = new ServingAssessment(ServingState.Primary, null, null, FrontDoorAgreement.Agrees, Healthy, null, []);

        Assert.NotNull(empty.Rotation);
        Assert.Empty(empty.Rotation!);
        Assert.True(empty.IsRotation);
        Assert.Empty(empty.Serving);
        Assert.Empty(empty.OutOfRotation);
        Assert.Equal("Front Door rotates over 0 regions: all healthy", empty.Headline);
        Assert.Null(empty.FailoverDetail);
    }

    // ----- Serving and OutOfRotation: the two lists worked out of the parts

    /// <summary>The nodes expected to serve, and the nodes out of the rotation, are worked out again on every read:
    /// two reads are never the same list, so a caller that keeps one holds a snapshot and not the assessment's own
    /// answer - while the nodes inside it stay the nodes it was handed, and the two lists together are exactly the
    /// rotation.</summary>
    [Fact]
    public void TheTwoListsAreWorkedOutAgainOnEveryRead()
    {
        var west = Primary(Healthy, "a");
        var east = Primary(Unhealthy, "b");
        var rotation = ServingAssessment.Assess([west, east], Healthy);

        Assert.NotSame(rotation.Serving, rotation.Serving);
        Assert.NotSame(rotation.OutOfRotation, rotation.OutOfRotation);
        Assert.Same(west, rotation.Serving[0]);
        Assert.Single(rotation.Serving);
        Assert.Single(rotation.OutOfRotation);
        Assert.All(rotation.Serving, node => Assert.Contains(node, rotation.Rotation!));
        Assert.All(rotation.OutOfRotation, node => Assert.Contains(node, rotation.Rotation!));

        var down = ServingAssessment.Assess([Primary(Unreachable, "a"), Standby(Unhealthy, "b")], Healthy);

        Assert.Empty(down.Serving);
        Assert.Empty(down.OutOfRotation);
        Assert.Null(down.Rotation);

        var single = ServingAssessment.Assess([west], null);

        Assert.NotSame(single.Serving, single.Serving);
        Assert.Same(west, Assert.Single(single.Serving));
    }

    /// <summary>While there is a rotation, the rotation is the whole answer: the node named as the expected one serves
    /// nothing unless the rotation names it too, and a rotation whose primary nodes are none of them healthy has
    /// nothing serving even in the state that says something serves. Without a rotation the expected node is the only
    /// member of the list, and its absence an empty list rather than nothing at all.</summary>
    [Fact]
    public void TheRotationOutranksTheNodeNamedAsTheExpectedOne()
    {
        var stranger = Standby(Healthy, "elsewhere");
        var rotation = new List<NodeHealth> { Primary(Healthy, "a"), Primary(Unhealthy, "b") };

        var assessment = new ServingAssessment(ServingState.Primary, stranger, null, FrontDoorAgreement.Agrees, Healthy, null, rotation);

        Assert.Equal(["a"], Labels(assessment.Serving));
        Assert.DoesNotContain(stranger, assessment.Serving);
        Assert.Equal(["b"], Labels(assessment.OutOfRotation));

        var noneHealthy = new ServingAssessment(
            ServingState.Primary,
            stranger,
            null,
            FrontDoorAgreement.Agrees,
            Healthy,
            null,
            [Primary(Unhealthy, "a"), Primary(Unreachable, "b")]);

        Assert.Empty(noneHealthy.Serving);
        Assert.DoesNotContain(stranger, noneHealthy.Serving);
        Assert.Equal(["a", "b"], Labels(noneHealthy.OutOfRotation));
        Assert.Equal("Front Door rotates over 0 of 2 regions: a and b are not healthy", noneHealthy.Headline);

        var undecided = new ServingAssessment(ServingState.Pending, null, null, FrontDoorAgreement.Pending, Pending);

        Assert.Null(undecided.Expected);
        Assert.Empty(undecided.Serving);
        Assert.Empty(undecided.OutOfRotation);
    }

    /// <summary>Out of a rotation stands every primary node that is not healthy, in the order of the topology: the
    /// unchecked one and the state the enum does not name are as much out as the two words a check leaves behind, and
    /// the healthy one is the only node left in. The list is worked out whether or not the rotation has been decided,
    /// so a rotation still being checked has its stopped regions to count.</summary>
    [Fact]
    public void EveryPrimaryNodeThatIsNotHealthyStandsOutOfTheRotation()
    {
        var assessment = ServingAssessment.Assess(
            [Primary(Unhealthy, "a"), Primary(Pending, "b"), Primary(Unknown, "c"), Primary(Unreachable, "d"), Primary(Healthy, "e")],
            Healthy);

        Assert.Equal(ServingState.Pending, assessment.State);
        Assert.Equal(["a", "b", "c", "d"], Labels(assessment.OutOfRotation));
        Assert.Equal(["e"], Labels(assessment.Rotation!.Where(node => node.State == Healthy).ToList()));
        Assert.Empty(assessment.Serving);

        var all = ServingAssessment.Assess([Primary(Healthy, "a"), Primary(Healthy, "b")], Healthy);

        Assert.Empty(all.OutOfRotation);
        Assert.Equal(2, all.Rotation!.Count);

        var standby = ServingAssessment.Assess([Primary(Healthy, "a"), Primary(Pending, "b"), Standby(Unhealthy, "z")], Healthy);

        Assert.Equal(["b"], Labels(standby.OutOfRotation));
    }

    // ----- Headline: the words the parts are said in

    /// <summary>The deployable that is neither a rotation nor a single node, and has not been decided yet, says it is
    /// checking <em>one</em> region: the plural belongs to the rotation alone, and the two sentences are apart by that
    /// one letter, so a rewording that moves it is caught here rather than read off the page.</summary>
    [Fact]
    public void TheDeployableOfOneRegionStillBeingCheckedSaysItIsCheckingOne()
    {
        var assessment = ServingAssessment.Assess([Primary(Pending, "westus3"), Standby(Healthy, "eastus2")], Healthy);

        Assert.Equal(ServingState.Pending, assessment.State);
        Assert.Null(assessment.Rotation);
        Assert.Null(assessment.OnlyNode);
        Assert.Equal("Checking which region serves traffic", assessment.Headline);
        Assert.DoesNotContain("regions", assessment.Headline, StringComparison.Ordinal);
    }

    /// <summary>A rotation's words outrank a single node's: an assessment handed both says the rotation's headline,
    /// names the regions of the rotation and never the node it was also handed as the only one. The order of the two
    /// questions in the member is what this pins, and no decision of <c>Assess</c> hands over both.</summary>
    [Fact]
    public void TheWordsOfARotationOutrankTheWordsOfASingleNode()
    {
        var rotation = new List<NodeHealth> { Primary(Healthy, "a"), Primary(Unhealthy, "b") };
        var both = new ServingAssessment(ServingState.Primary, rotation[0], null, FrontDoorAgreement.NotPresent, null, rotation[1], rotation);

        Assert.NotNull(both.OnlyNode);
        Assert.True(both.IsRotation);
        Assert.Equal("Front Door rotates over 1 of 2 regions: b is not healthy", both.Headline);
        Assert.DoesNotContain("Serves traffic", both.Headline, StringComparison.Ordinal);

        // The failover of that same assessment is the rotation's failover, told in the rotation's words.
        Assert.Equal("Failed over to b", (both with { State = ServingState.FailedOver, Expected = rotation[1] }).Headline);

        // The state that names no region at all is the rotation's last word, not the single node's.
        Assert.Equal("No region is healthy", (both with { State = ServingState.NoNodes, Expected = null }).Headline);
    }

    /// <summary>Past the three it names, the rotation's headline counts what it left out instead of saying it: the
    /// names are the first three of the topology, the remainder is the stopped count less the three it named, and the
    /// number the sentence opens with is the rotation less everything stopped - the regions left serving, never the
    /// regions named. The four-of-five rows are pinned by <see cref="RotationTests" />; these are the same edges
    /// further out.</summary>
    [Theory]
    [InlineData(12, 10, "Front Door rotates over 2 of 12 regions: r1, r2, r3 and 7 more are not healthy")]
    [InlineData(9, 5, "Front Door rotates over 4 of 9 regions: r1, r2, r3 and 2 more are not healthy")]
    [InlineData(8, 4, "Front Door rotates over 4 of 8 regions: r1, r2, r3 and 1 more are not healthy")]
    public void TheHeadlineNamesThreeStoppedRegionsAndCountsTheRest(int regions, int stopped, string headline)
    {
        var nodes = Enumerable.Range(1, regions)
            .Select(index => Primary(index <= stopped ? Unreachable : Healthy, $"r{index}"))
            .ToList();

        var assessment = ServingAssessment.Assess(nodes, Healthy);

        Assert.Equal(ServingState.Primary, assessment.State);
        Assert.Equal(headline, assessment.Headline);
        Assert.Equal(regions, assessment.Rotation!.Count);
        Assert.Equal(stopped, assessment.OutOfRotation.Count);
        Assert.Equal(regions - stopped, assessment.Serving.Count);
        Assert.Equal(["r1", "r2", "r3"], Labels(assessment.OutOfRotation.Take(ServingAssessment.MostNamed).ToList()));
    }

    // ----- MostNamed, the number the naming stops at

    /// <summary><see cref="ServingAssessment.MostNamed" /> is three, and three as the file wrote it: a public literal
    /// of <see cref="int" /> rather than a field the runtime fills in, which is why a test can hand it to a theory as
    /// a constant, why it is read at compile time above, and why the headline's list and the count behind it are cut
    /// against one number.</summary>
    [Theory]
    [InlineData(ServingAssessment.MostNamed)]
    public void ThreeStoppedRegionsAreNamedAndNoMore(int mostNamed)
    {
        Assert.Equal(3, mostNamed);
        Assert.Equal(3, ServingAssessment.MostNamed);
        Assert.True(mostNamed > 0, "a headline that names no stopped region names nothing");
        Assert.True(mostNamed < 8, $"{mostNamed} names are no list a reader checks");

        var constant = typeof(ServingAssessment).GetField("MostNamed", BindingFlags.Public | BindingFlags.Static)!;

        Assert.Equal(typeof(int), constant.FieldType);
        Assert.True(constant.IsPublic);
        Assert.True(constant.IsStatic);
        Assert.True(constant.IsLiteral);
        Assert.False(constant.IsInitOnly);
        Assert.Equal<object>(mostNamed, constant.GetRawConstantValue()!);
        Assert.DoesNotContain("MostNamed", [.. typeof(ServingAssessment).GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(property => property.Name)], StringComparer.Ordinal);
    }

    // ----- FailoverDetail

    /// <summary>The one node of a deployable has nothing to fail over to, and the member says so even when the state
    /// it was handed is a failover and nodes stand in both of the parts a failover would name: the single node is
    /// asked before the reason is read out of them, so an assessment that carries a node as its only one accounts for
    /// nothing - and the same parts without it do account, in the words of the expected node.</summary>
    [Theory]
    [InlineData(true, true, null)]
    [InlineData(true, false, null)]
    [InlineData(false, true, "Primary b is unhealthy; a is expected to serve traffic.")]
    [InlineData(false, false, "No primary node is healthy; a is expected to serve traffic.")]
    public void ASingleNodeHasNoFailoverToAccountFor(bool onlyNode, bool failedPrimary, string? detail)
    {
        var node = Primary(Healthy, "a");
        var blame = Primary(Unhealthy, "b");

        var assessment = new ServingAssessment(
            ServingState.FailedOver,
            node,
            failedPrimary ? blame : null,
            FrontDoorAgreement.NotPresent,
            null,
            onlyNode ? node : null);

        Assert.True(assessment.IsFailedOver);
        Assert.Equal(onlyNode, assessment.OnlyNode is not null);
        Assert.Equal(detail, assessment.FailoverDetail);
    }

    /// <summary>The single node is asked before either of the two parts a failover reads, so a failover that names
    /// nothing at all beside its one node is answered with the silence rather than with the nothing its parts would
    /// have been read out of.</summary>
    [Fact]
    public void ASingleNodeSaysNoReasonWhateverElseIsMissing()
    {
        var node = Primary(Healthy, "a");
        var assessment = new ServingAssessment(ServingState.FailedOver, null, null, FrontDoorAgreement.NotPresent, null, node);

        Assert.True(assessment.IsFailedOver);
        Assert.Null(assessment.Expected);
        Assert.Null(assessment.FailedPrimary);
        Assert.Null(assessment.FailoverDetail);
        Assert.Equal("Serves traffic: a", assessment.Headline);
    }

    /// <summary>The state of the primary a failover blames is said in the words of that state, and every state that is
    /// none of the three a check leaves behind is said as the one sentence the file holds for it: an unchecked primary
    /// is "not checked", and so is a number the enum does not name, and a healthy one is healthy - the blame being the
    /// node's own state and nothing to do with the state of the node that took over. The two words a check really
    /// gives are pinned by <see cref="ServingAssessmentTests" />.</summary>
    [Theory]
    [InlineData(Healthy, "healthy")]
    [InlineData(Pending, "not checked")]
    [InlineData(Unknown, "not checked")]
    [InlineData((HealthState)4, "not checked")]
    public void ThePrimaryAFailoverBlamesIsSaidInTheWordsOfItsOwnState(HealthState state, string word)
    {
        var assessment = new ServingAssessment(
            ServingState.FailedOver,
            Standby(Healthy, "eastus2"),
            Primary(state, "westus3"),
            FrontDoorAgreement.Agrees,
            Healthy);

        Assert.Equal($"Primary westus3 is {word}; eastus2 is expected to serve traffic.", assessment.FailoverDetail);

        // What the blame is follows the blamed node alone: the standby that took over is only ever named, whatever
        // state it is in, and the label it is named by is its own.
        Assert.Equal(
            $"Primary westus3 is {word}; eastus2 is expected to serve traffic.",
            (assessment with { Expected = Standby(Unhealthy, "eastus2") }).FailoverDetail);
    }

    /// <summary>A failover with no primary to blame is the failover of a deployable whose topology names none, and the
    /// reason says the absence rather than inventing a node: <c>Assess</c> hands over no failed primary for a standby
    /// that took over from standbys, and none at all for a rotation, whose detail always says that no primary is
    /// healthy. Three primaries without a Front Door endpoint are no rotation, and the first of them is the one
    /// blamed, whichever of the two stopped serving last.</summary>
    [Fact]
    public void AFailoverWithNoPrimaryToBlameSaysThatNoneIsHealthy()
    {
        var assessment = ServingAssessment.Assess([Standby(Unhealthy, "a"), Standby(Healthy, "b"), Standby(Healthy, "c")], Healthy);

        Assert.Equal(ServingState.FailedOver, assessment.State);
        Assert.Null(assessment.FailedPrimary);
        Assert.Equal("b", assessment.Expected?.Label);
        Assert.Null(assessment.OnlyNode);
        Assert.Null(assessment.Rotation);
        Assert.Equal("Failed over to b", assessment.Headline);
        Assert.Equal("No primary node is healthy; b is expected to serve traffic.", assessment.FailoverDetail);

        // A rotation names no failed primary either, however many primaries stopped.
        var rotation = ServingAssessment.Assess([Primary(Unreachable, "a"), Primary(Unhealthy, "b"), Standby(Healthy, "c")], Healthy);

        Assert.True(rotation.IsRotation);
        Assert.Null(rotation.FailedPrimary);
        Assert.Equal("No primary node is healthy; c is expected to serve traffic.", rotation.FailoverDetail);

        // The same topology without an endpoint is no rotation, and blames the first primary of the topology.
        var three = ServingAssessment.Assess([Primary(Unreachable, "a"), Primary(Unhealthy, "b"), Standby(Healthy, "c")], null);

        Assert.False(three.IsRotation);
        Assert.Equal("a", three.FailedPrimary?.Label);
        Assert.Equal("Primary a is unreachable; c is expected to serve traffic.", three.FailoverDetail);
    }

    // ----- FrontDoorText

    /// <summary>The agreement is read first and the state of the endpoint second, never the other way round: an
    /// assessment that says there is no endpoint says nothing even with a healthy state standing behind it, and an
    /// assessment that says the check has not run names the check and not the state - the absent one, the unchecked
    /// one or the one a check left behind.</summary>
    [Theory]
    [InlineData(FrontDoorAgreement.NotPresent, Healthy, null)]
    [InlineData(FrontDoorAgreement.NotPresent, Pending, null)]
    [InlineData(FrontDoorAgreement.NotPresent, null, null)]
    [InlineData(FrontDoorAgreement.Pending, Pending, "Front Door is being checked.")]
    [InlineData(FrontDoorAgreement.Pending, null, "Front Door is being checked.")]
    [InlineData(FrontDoorAgreement.Pending, Unhealthy, "Front Door is being checked.")]
    public void TheAgreementIsReadBeforeTheStateItGoesWith(FrontDoorAgreement agreement, HealthState? frontDoor, string? text)
    {
        var assessment = new ServingAssessment(ServingState.Primary, Primary(Healthy, "a"), null, agreement, frontDoor);

        Assert.Equal(agreement, assessment.FrontDoor);
        Assert.Equal(frontDoor, assessment.FrontDoorState);
        Assert.Equal(text, assessment.FrontDoorText);
    }

    /// <summary>A state the endpoint was never checked to is said as the file's own words for it - "not checked" -
    /// whether the agreement is an accord or a contradiction, and whether the state is the absent one, the unchecked
    /// one or a number the enum does not name. And a contradiction with no node named as the expected one says "a
    /// node", which is the sentence's own fallback for the nothing a Pending decision leaves in that part.</summary>
    [Theory]
    [InlineData(null)]
    [InlineData(Pending)]
    [InlineData(Unknown)]
    public void AFrontDoorStateThatIsNoStateIsSaidAsNotChecked(HealthState? frontDoor)
    {
        var west = Primary(Healthy, "westus3");
        var agrees = new ServingAssessment(ServingState.Primary, west, null, FrontDoorAgreement.Agrees, frontDoor);
        var disagrees = agrees with { FrontDoor = FrontDoorAgreement.Disagrees };
        var nameless = disagrees with { Expected = null };

        Assert.Equal(frontDoor, agrees.FrontDoorState);
        Assert.Equal("Front Door agrees: it is not checked too.", agrees.FrontDoorText);
        Assert.Equal("Front Door disagrees: it is not checked although westus3 is healthy.", disagrees.FrontDoorText);
        Assert.Equal("Front Door disagrees: it is not checked although a node is healthy.", nameless.FrontDoorText);

        // The node is named by its label, and a node of no region by the name the topology gave it.
        Assert.Equal(
            "Front Door disagrees: it is not checked although app-west is healthy.",
            (nameless with { Expected = new NodeHealth("app-west", null, true, Unhealthy) }).FrontDoorText);
    }

    /// <summary>What is none of the four agreements is a contradiction as far as the words go - the last arm of the
    /// member answers for everything the file does not name - and it still reads the state of the endpoint before it
    /// reads the node: a healthy endpoint contradicts a page that names no node, anything else contradicts the node it
    /// was given.</summary>
    [Theory]
    [InlineData((FrontDoorAgreement)5, Healthy, "Front Door disagrees: it is healthy although no node is.")]
    [InlineData((FrontDoorAgreement)(-1), Unhealthy, "Front Door disagrees: it is unhealthy although westus3 is healthy.")]
    [InlineData((FrontDoorAgreement)(-1), Unknown, "Front Door disagrees: it is not checked although westus3 is healthy.")]
    public void AnAgreementTheFileDoesNotNameIsSaidAsADisagreement(FrontDoorAgreement agreement, HealthState frontDoor, string text)
    {
        var assessment = new ServingAssessment(ServingState.Primary, Primary(Healthy, "westus3"), null, agreement, frontDoor);

        Assert.False(Enum.IsDefined(agreement));
        Assert.Equal(agreement, assessment.FrontDoor);
        Assert.Equal(text, assessment.FrontDoorText);
    }

    // ----- the seven parts as they are handed over

    /// <summary>The record keeps the seven parts where they are handed and reads nothing out of anything else: the
    /// nodes stand as the very instances, the agreement stands beside the state it was decided from even when the two
    /// contradict each other, a state and an agreement the enums do not name stand as they were given, and the last
    /// two parts are the absence of a part rather than a placeholder for one when the making says no more than the
    /// first five.</summary>
    [Fact]
    public void TheSevenPartsStandWhereTheyAreHanded()
    {
        var expected = Primary(Healthy, "westus3");
        var blame = Standby(Unhealthy, "eastus2");
        var only = Primary(Pending, "northeurope");
        var rotation = new List<NodeHealth> { expected, Primary(Unreachable, "uksouth") };

        var assessment = new ServingAssessment((ServingState)5, expected, blame, (FrontDoorAgreement)7, Unreachable, only, rotation);

        Assert.Equal((ServingState)5, assessment.State);
        Assert.Same(expected, assessment.Expected);
        Assert.Same(blame, assessment.FailedPrimary);
        Assert.Equal((FrontDoorAgreement)7, assessment.FrontDoor);
        Assert.Equal(Unreachable, assessment.FrontDoorState);
        Assert.Same(only, assessment.OnlyNode);
        Assert.Same(rotation, assessment.Rotation);
        Assert.True(assessment.IsRotation);
        Assert.False(assessment.IsFailedOver);
        Assert.Equal(["westus3"], Labels(assessment.Serving));

        // Five parts handed, two absent: the failover is told in full, with no rotation and no single node in it.
        var five = new ServingAssessment(ServingState.FailedOver, expected, blame, FrontDoorAgreement.Agrees, Healthy);

        Assert.Null(five.OnlyNode);
        Assert.Null(five.Rotation);
        Assert.False(five.IsRotation);
        Assert.Equal(["westus3"], Labels(five.Serving));
        Assert.Empty(five.OutOfRotation);
        Assert.Equal("Failed over to westus3", five.Headline);
        Assert.Equal("Primary eastus2 is unhealthy; westus3 is expected to serve traffic.", five.FailoverDetail);
    }

    /// <summary>The state of the endpoint an assessment says is the state it was handed and no other: the decision
    /// keeps what the check left, the absent endpoint included, and a number no state of the four answers to is kept
    /// as the number it is.</summary>
    [Theory]
    [InlineData(null)]
    [InlineData(Pending)]
    [InlineData(Healthy)]
    [InlineData(Unhealthy)]
    [InlineData(Unreachable)]
    [InlineData(Unknown)]
    public void TheFrontDoorStateIsTheStateTheProbeLeft(HealthState? frontDoor)
    {
        var assessment = ServingAssessment.Assess([Primary(Healthy, "a"), Standby(Unhealthy, "b")], frontDoor);

        Assert.Equal(frontDoor, assessment.FrontDoorState);
    }

    // ----- the record the compiler builds around the seven parts

    /// <summary>Two assessments are one while their seven parts are - and a rotation is one with the list it holds and
    /// with no copy of it, because a list is compared as the list it is and not node by node. Two decisions of the
    /// very same topology are therefore one assessment when it names no rotation and two when it names one, which is
    /// why the page compares the nodes serving and the headline of a round with the last round's, and never the
    /// records.</summary>
    [Fact]
    public void TwoAssessmentsAreOneWhileTheirPartsAreAndARotationIsOneWithItsOwnList()
    {
        var west = Primary(Healthy, "westus3");
        var east = Primary(Unhealthy, "eastus2");
        var rotation = new List<NodeHealth> { west, east };
        var one = new ServingAssessment(ServingState.Primary, west, null, FrontDoorAgreement.Agrees, Healthy, null, rotation);

        var copy = one with { };

        Assert.True(one.Equals(copy));
        Assert.True(one == copy);
        Assert.False(one != copy);
        Assert.True(one.Equals((object)copy));
        Assert.True(one.Equals(one));
        Assert.NotSame(one, copy);
        Assert.Equal(one.GetHashCode(), copy.GetHashCode());
        Assert.Equal(one.GetHashCode(), one.GetHashCode());
        Assert.Single([.. new HashSet<ServingAssessment> { one, copy }]);

        // A list of the very same two nodes is another rotation, and an assessment made of it another assessment.
        var twin = one with { Rotation = [.. rotation] };

        Assert.Equal(Labels(rotation), Labels(twin.Rotation!));
        Assert.Equal(one.Headline, twin.Headline);
        Assert.False(one.Equals(twin));
        Assert.False(one == twin);
        Assert.True(one != twin);
        Assert.NotEqual(one.GetHashCode(), twin.GetHashCode());

        // Two rotations of no nodes are one, because the runtime hands out one empty array to whoever asks for it.
        Assert.Equal(one with { Rotation = [] }, one with { Rotation = Array.Empty<NodeHealth>() });

        // Every part but one moved is another assessment, the absence of a node included.
        Assert.False(one == one with { State = ServingState.Down });
        Assert.False(one == one with { Expected = null });
        Assert.False(one == one with { FailedPrimary = east });
        Assert.False(one == one with { FrontDoor = FrontDoorAgreement.Disagrees });
        Assert.False(one == one with { FrontDoorState = Unhealthy });
        Assert.False(one == one with { OnlyNode = west });
        Assert.False(one == one with { Rotation = null });

        // Nothing that is no assessment is one, and the nothing a caller holds is nobody's equal.
        ServingAssessment? absent = null;

        Assert.False(one.Equals(null));
        Assert.False(one.Equals((object)"westus3"));
        Assert.False(one.Equals(west));
        Assert.False(one.Equals(ServingState.Primary));
        Assert.False(one == absent);
        Assert.False(absent == one);
        Assert.True(absent == null);

        // Two rounds over the same topology: no rotation is one decision twice over, a rotation never is.
        var nodes = new[] { west, Standby(Unhealthy, "eastus2") };

        Assert.Equal(ServingAssessment.Assess(nodes, null), ServingAssessment.Assess(nodes, null));
        Assert.NotEqual(ServingAssessment.Assess(rotation, Healthy), ServingAssessment.Assess([west, east], Healthy));
    }

    /// <summary>The seven parts fall apart in the order the record declares them, the last two of them the nothing
    /// they were left as: what comes out is what went in, node for node and list for list, and the same for an
    /// assessment a decision made.</summary>
    [Fact]
    public void TheSevenPartsFallApartInTheOrderTheyWentIn()
    {
        var expected = Primary(Healthy, "westus3");
        var blame = Standby(Unhealthy, "eastus2");
        var only = Primary(Pending, "northeurope");
        var rotation = new List<NodeHealth> { expected, Primary(Unreachable, "uksouth") };

        var (state, node, failed, agreement, frontDoor, single, primaries) =
            new ServingAssessment(ServingState.FailedOver, expected, blame, FrontDoorAgreement.Disagrees, Unreachable, only, rotation);

        Assert.Equal(ServingState.FailedOver, state);
        Assert.Same(expected, node);
        Assert.Same(blame, failed);
        Assert.Equal(FrontDoorAgreement.Disagrees, agreement);
        Assert.Equal(Unreachable, frontDoor);
        Assert.Same(only, single);
        Assert.Same(rotation, primaries);

        var (nothing, nowhere, nobody, unchanged, unseen, alone, none) = ServingAssessment.Assess([], null);

        Assert.Equal(ServingState.NoNodes, nothing);
        Assert.Null(nowhere);
        Assert.Null(nobody);
        Assert.Equal(FrontDoorAgreement.NotPresent, unchanged);
        Assert.Null(unseen);
        Assert.Null(alone);
        Assert.Null(none);

        // What the assessment falls apart into is what it was made of: put back in the order they came out, the seven
        // parts are the very assessment again.
        Assert.Equal(
            new ServingAssessment(ServingState.FailedOver, expected, blame, FrontDoorAgreement.Disagrees, Unreachable, only, rotation),
            new ServingAssessment(state, node, failed, agreement, frontDoor, single, primaries));
    }

    /// <summary>A copy names the parts it moves and carries the rest along: the copy that names nothing is the same
    /// assessment and not the same one, the assessment it was made from stands where it stood, and the lists and
    /// nodes a copy carries are carried as the references they are - a copy never copies a rotation.</summary>
    [Fact]
    public void WithChangesOnlyThePartsItNamesAndCarriesTheRestAlong()
    {
        var west = Primary(Healthy, "westus3");
        var rotation = new List<NodeHealth> { west, Primary(Unhealthy, "eastus2") };
        var one = new ServingAssessment(ServingState.Primary, west, null, FrontDoorAgreement.Agrees, Healthy, null, rotation);

        var copy = one with { };

        Assert.Equal(one, copy);
        Assert.NotSame(one, copy);
        Assert.Same(one.Rotation, copy.Rotation);
        Assert.Same(one.Expected, copy.Expected);

        var moved = one with { State = ServingState.FailedOver, FailedPrimary = rotation[1] };

        Assert.Equal(ServingState.FailedOver, moved.State);
        Assert.True(moved.IsFailedOver);
        Assert.Same(rotation[1], moved.FailedPrimary);
        Assert.Equal(FrontDoorAgreement.Agrees, moved.FrontDoor);
        Assert.Equal(Healthy, moved.FrontDoorState);
        Assert.Same(rotation, moved.Rotation);
        Assert.True(moved.IsRotation);
        Assert.Null(moved.OnlyNode);
        Assert.Equal("Failed over to westus3", moved.Headline);

        // The assessment the copy was made from is where it stood, parts and words alike.
        Assert.Equal(ServingState.Primary, one.State);
        Assert.Null(one.FailedPrimary);
        Assert.Equal("Front Door rotates over 1 of 2 regions: eastus2 is not healthy", one.Headline);
    }

    /// <summary>The words an assessment says of itself are its fourteen public members in the order the record
    /// declares them - the seven parts first, then the seven worked out of them - each said in its own words: a node
    /// as the node says itself, an absent part as nothing at all between two commas, and a list as a list says
    /// itself, which is its type and not one word of the nodes inside it.</summary>
    [Fact]
    public void TheAssessmentSaysItsMembersInTheirOrderAndItsListsAsLists()
    {
        Assert.Equal(
            "ServingAssessment { State = NoNodes, Expected = , FailedPrimary = , FrontDoor = NotPresent, FrontDoorState = , OnlyNode = , Rotation = , IsFailedOver = False, IsRotation = False, Serving = Dashboard.Health.NodeHealth[], OutOfRotation = Dashboard.Health.NodeHealth[], Headline = No nodes in the topology, FailoverDetail = , FrontDoorText =  }",
            new ServingAssessment(ServingState.NoNodes, null, null, FrontDoorAgreement.NotPresent, null).ToString());

        var west = Primary(Healthy, "westus3");
        var rotation = new List<NodeHealth> { west, Primary(Unhealthy, "eastus2") };
        var words = new ServingAssessment(ServingState.Primary, west, null, FrontDoorAgreement.Agrees, Healthy, null, rotation).ToString();

        var at = -1;

        foreach (var member in Spoken)
        {
            var found = words.IndexOf(member, StringComparison.Ordinal);

            Assert.True(found > at, $"'{words}' says '{member}' out of its place");
            at = found;
        }

        Assert.StartsWith($"ServingAssessment {{ State = Primary, Expected = {west}, FailedPrimary = , FrontDoor = Agrees, ", words, StringComparison.Ordinal);
        Assert.Contains("Headline = Front Door rotates over 1 of 2 regions: eastus2 is not healthy", words, StringComparison.Ordinal);
        Assert.EndsWith(" }", words, StringComparison.Ordinal);

        // The rotation is said as the list it is: no node of it stands between its own part and the next.
        Assert.Equal(
            "Rotation = System.Collections.Generic.List`1[Dashboard.Health.NodeHealth]",
            words[words.IndexOf("Rotation = ", StringComparison.Ordinal)..words.IndexOf(", IsFailedOver", StringComparison.Ordinal)]);

        // The nodes are said once each, in the one part that names them; the single node of a deployable of one is
        // said twice, once for the part it serves as and once for the part it is on its own as.
        Assert.Equal(1, Times(words, "NodeHealth {"));
        Assert.Equal(2, Times(ServingAssessment.Assess([west], null).ToString(), "NodeHealth {"));
    }

    /// <summary>The whole public surface the record declares, as the running assembly holds it: the seven parts, the
    /// seven it works out, the constant, the one decision, the making of an assessment by hand and everything the
    /// compiler adds to a positional record - and nothing else to call. A member added has to be written here as
    /// well, which is the point.</summary>
    [Fact]
    public void TheSurfaceIsTheSevenPartsTheSevenWordsTheConstantTheDecisionAndTheRecord()
    {
        Assert.Equal(
            [
                ".ctor", "<Clone>$", "Assess", "Deconstruct", "Equals", "Expected", "FailedPrimary", "FailoverDetail", "FrontDoor", "FrontDoorState",
                "FrontDoorText", "GetHashCode", "Headline", "IsFailedOver", "IsRotation", "MostNamed", "OnlyNode", "OutOfRotation", "Rotation", "Serving",
                "State", "ToString", "get_Expected", "get_FailedPrimary", "get_FailoverDetail", "get_FrontDoor", "get_FrontDoorState", "get_FrontDoorText",
                "get_Headline", "get_IsFailedOver", "get_IsRotation", "get_OnlyNode", "get_OutOfRotation", "get_Rotation", "get_Serving", "get_State",
                "op_Equality", "op_Inequality", "set_Expected", "set_FailedPrimary", "set_FrontDoor", "set_FrontDoorState", "set_OnlyNode", "set_Rotation",
                "set_State",
            ],
            [.. typeof(ServingAssessment)
                .GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Select(member => member.Name)
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)]);

        Assert.Equal(
            ["State", "Expected", "FailedPrimary", "FrontDoor", "FrontDoorState", "OnlyNode", "Rotation", "IsFailedOver", "IsRotation", "Serving", "OutOfRotation", "Headline", "FailoverDetail", "FrontDoorText"],
            [.. typeof(ServingAssessment).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Select(property => property.Name)]);

        Assert.Equal(
            ["Assess"],
            [.. typeof(ServingAssessment).GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Select(method => method.Name)
                .Where(name => !name.StartsWith("op_", StringComparison.Ordinal))]);

        Assert.DoesNotContain(typeof(ServingAssessment).GetFields(BindingFlags.Instance | BindingFlags.DeclaredOnly), field => !field.IsPrivate);
    }

    /// <summary>The seven parts are written at the making and read ever after, and the seven sentences are read only:
    /// each part a public property with a public reader no one may extend and a writer only the making and a
    /// <c>with</c> reach, the last two of them defaulted to the absence of a node and of a rotation rather than to an
    /// empty one, and each sentence without a writer at all - the two lists and the two questions never saying
    /// nothing.</summary>
    [Fact]
    public void TheSevenPartsAreWrittenAtTheMakingAndTheSevenWordsAreReadOnly()
    {
        var type = typeof(ServingAssessment);
        var nullability = new NullabilityInfoContext();
        string[] positional = ["State", "Expected", "FailedPrimary", "FrontDoor", "FrontDoorState", "OnlyNode", "Rotation"];

        foreach (var name in positional)
        {
            var part = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)!;

            Assert.True(part.GetMethod!.IsPublic);
            Assert.False(part.GetMethod.IsVirtual);
            Assert.True(part.CanWrite);
            Assert.True(part.SetMethod!.IsPublic);
            Assert.False(part.SetMethod.IsVirtual);
            Assert.Contains(typeof(IsExternalInit), part.SetMethod.ReturnParameter.GetRequiredCustomModifiers());
        }

        foreach (var name in new[] { "Expected", "FailedPrimary", "FrontDoorState", "OnlyNode", "Rotation" })
        {
            Assert.Equal(NullabilityState.Nullable, nullability.Create(type.GetProperty(name)!).WriteState);
        }

        foreach (var name in new[] { "State", "FrontDoor" })
        {
            Assert.Equal(NullabilityState.NotNull, nullability.Create(type.GetProperty(name)!).WriteState);
        }

        Assert.Equal(typeof(ServingState), type.GetProperty("State")!.PropertyType);
        Assert.Equal(typeof(FrontDoorAgreement), type.GetProperty("FrontDoor")!.PropertyType);
        Assert.Equal(typeof(NodeHealth), type.GetProperty("Expected")!.PropertyType);
        Assert.Equal(typeof(NodeHealth), type.GetProperty("FailedPrimary")!.PropertyType);
        Assert.Equal(typeof(HealthState?), type.GetProperty("FrontDoorState")!.PropertyType);
        Assert.Equal(typeof(NodeHealth), type.GetProperty("OnlyNode")!.PropertyType);
        Assert.Equal(typeof(IReadOnlyList<NodeHealth>), type.GetProperty("Rotation")!.PropertyType);

        foreach (var name in new[] { "IsFailedOver", "IsRotation", "Serving", "OutOfRotation", "Headline", "FailoverDetail", "FrontDoorText" })
        {
            var said = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)!;

            Assert.False(said.CanWrite);
            Assert.Null(said.SetMethod);
            Assert.True(said.GetMethod!.IsPublic);
            Assert.False(said.GetMethod.IsVirtual);
        }

        foreach (var name in new[] { "IsFailedOver", "IsRotation", "Serving", "OutOfRotation", "Headline" })
        {
            Assert.Equal(NullabilityState.NotNull, nullability.Create(type.GetProperty(name)!).ReadState);
        }

        foreach (var name in new[] { "FailoverDetail", "FrontDoorText" })
        {
            Assert.Equal(NullabilityState.Nullable, nullability.Create(type.GetProperty(name)!).ReadState);
        }

        Assert.Equal(typeof(bool), type.GetProperty("IsRotation")!.PropertyType);
        Assert.Equal(typeof(IReadOnlyList<NodeHealth>), type.GetProperty("Serving")!.PropertyType);
        Assert.Equal(typeof(IReadOnlyList<NodeHealth>), type.GetProperty("OutOfRotation")!.PropertyType);
        Assert.Equal(typeof(string), type.GetProperty("Headline")!.PropertyType);

        var made = Assert.Single(type.GetConstructors(BindingFlags.Public | BindingFlags.Instance));
        var arguments = made.GetParameters();

        Assert.Equal(
            ["State", "Expected", "FailedPrimary", "FrontDoor", "FrontDoorState", "OnlyNode", "Rotation"],
            [.. arguments.Select(argument => argument.Name!)]);
        Assert.All(arguments.Take(5), argument => Assert.False(argument.HasDefaultValue));

        foreach (var argument in arguments.Skip(5))
        {
            Assert.True(argument.HasDefaultValue);
            Assert.Null(argument.DefaultValue);
            Assert.Equal(NullabilityState.Nullable, nullability.Create(argument).WriteState);
        }
    }

    /// <summary>The assessment is sealed, so nothing is an assessment on top of it: it stands on
    /// <see cref="object" /> alone and wears one interface, the typed equality of its own kind. The words it says of
    /// itself are private to it, the copy a <c>with</c> calls is the private constructor of the assessment it copies,
    /// and the decision is the one static thing the type offers besides the two operators.</summary>
    [Fact]
    public void TheAssessmentIsSealedItsEqualityIsTypedAndItsWordsAreItsOwn()
    {
        var type = typeof(ServingAssessment);

        Assert.True(type.IsSealed);
        Assert.False(type.IsAbstract);
        Assert.Equal(typeof(object), type.BaseType);
        Assert.Same(typeof(IEquatable<ServingAssessment>), Assert.Single(type.GetInterfaces()));
        Assert.False(type.IsDefined(typeof(CompilerGeneratedAttribute), false));

        var typed = type.GetMethod("Equals", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, [type], null)!;

        Assert.True(typed.IsVirtual);
        Assert.True(typed.IsFinal);
        Assert.Equal(typeof(bool), typed.ReturnType);

        var words = type.GetMethod("PrintMembers", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly, null, [typeof(StringBuilder)], null)!;

        Assert.True(words.IsPrivate);
        Assert.Null(type.GetMethod("PrintMembers", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));

        var contract = type.GetProperty("EqualityContract", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

        Assert.True(contract is null || contract.GetMethod is { IsPublic: false });
        Assert.DoesNotContain("EqualityContract", [.. type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Select(member => member.Name)], StringComparer.Ordinal);

        var copy = Assert.Single(
            type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance),
            constructor => constructor.GetParameters().Length == 1);

        Assert.True(copy.IsPrivate);
        Assert.Equal(type, copy.GetParameters()[0].ParameterType);
        Assert.True(copy.IsDefined(typeof(CompilerGeneratedAttribute), false));

        var clone = type.GetMethod("<Clone>$", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)!;

        Assert.Equal(type, clone.ReturnType);
        Assert.Empty(clone.GetParameters());

        Assert.Equal(7, type.GetMethod("Deconstruct", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)!.GetParameters().Length);

        Assert.Equal(
            ["Equals", "GetHashCode", "ToString"],
            [.. type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(method => !method.IsSpecialName && !method.Name.StartsWith("op_", StringComparison.Ordinal) && method.Name is not ("<Clone>$" or "Deconstruct"))
                .Select(method => method.Name)
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)]);

        Assert.Equal(
            ["Assess", "op_Equality", "op_Inequality"],
            [.. type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Select(method => method.Name)
                .Order(StringComparer.Ordinal)]);
    }

    /// <summary>
    /// The rotation headline's count-against-1 verb choice is routed through Plural, not written by hand:
    /// one stopped region says "is not healthy", any other count says "are not healthy", and zero stopped
    /// regions uses a different sentence altogether.
    /// </summary>
    [Theory]
    [InlineData(2, 0, "Front Door rotates over 2 regions: all healthy")]
    [InlineData(2, 1, "Front Door rotates over 1 of 2 regions: a is not healthy")]
    [InlineData(3, 2, "Front Door rotates over 1 of 3 regions: a and b are not healthy")]
    [InlineData(10, 5, "Front Door rotates over 5 of 10 regions: a, b, c and 2 more are not healthy")]
    public void TheRotationHeadlineRoutesThroughPlural(int regions, int stopped, string expected)
    {
        var nodes = Enumerable.Range(1, regions)
            .Select(index => Primary(index <= stopped ? Unreachable : Healthy, $"{(char)('a' + index - 1)}"))
            .ToList();

        var assessment = ServingAssessment.Assess(nodes, Healthy);

        Assert.Equal(ServingState.Primary, assessment.State);
        Assert.Equal(stopped, assessment.OutOfRotation.Count);
        Assert.Equal(expected, assessment.Headline);
    }
}
