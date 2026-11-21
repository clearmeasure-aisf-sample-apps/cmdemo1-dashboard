namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="NodeHealth" />, the one regional node as the serving
/// decision needs it - the four parts the record keeps and the label it says them with - declared in
/// <c>src/Dashboard/Health/ServingAssessment.cs</c>.
/// <para>
/// What the decision makes of those parts is pinned where the decision is made. <see cref="ServingAssessmentTests" />
/// pins the role that puts a node first, the state that lets it serve, a node of no region named by its own name in
/// the headline, and the one node of a deployable without a Front Door endpoint read by its <c>Name</c>;
/// <see cref="RotationTests" /> pins the labels of the nodes a rotation lists, in and out of it;
/// <see cref="EventDetectorTests" /> and <see cref="EventDetectorEdgeCaseTests" /> pin the events a node's state
/// turning makes, which are worded from its label.
/// </para>
/// <para>
/// What all of that leaves open is pinned here, and only that: the four parts standing where they are given, the
/// state standing as the state it was given over every one of the four and over a number the enum does not name; a
/// name and a region kept as the words the topology wrote - the empty word a word, the spaces about a word kept, and
/// a name of nothing accepted as it is given; the label taken from the region when there is one - the region of no
/// characters counting as one - from the name when there is none, nothing for the node that says neither, never
/// moved by the role or the state and read again from the parts after a copy; the four parts that make two nodes one
/// and any one of them two, the node not one with what is no node, and the node of nothing at all a node; the hash
/// code following the four parts and standing from call to call; the four read out of the node in the order they
/// stand in it; <c>with</c> changing the part it names and carrying the other three along, equal and not the same;
/// and the words the node says its parts in, the region it has not said as the nothing it is and the label said last
/// of all as the word it stands for.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: a node holds four values the test
/// brings, and no part of it is a moment.
/// </para>
/// </summary>
public class NodeHealthEdgeCaseTests
{
    private const HealthState Healthy = HealthState.Healthy;
    private const HealthState Unhealthy = HealthState.Unhealthy;
    private const HealthState Unreachable = HealthState.Unreachable;
    private const HealthState Pending = HealthState.Pending;

    /// <summary>A primary node of the name, the region and the state the test names.</summary>
    private static NodeHealth Primary(HealthState state, string name = "app-west", string? region = "westus3") =>
        new(name, region, true, state);

    /// <summary>The same node as a standby, so a role can be moved without moving anything else.</summary>
    private static NodeHealth Standby(HealthState state, string name = "app-west", string? region = "westus3") =>
        new(name, region, false, state);

    // ----- the four parts the node keeps

    /// <summary>The four parts stand where they are given, the primary's and the standby's alike: the node asks
    /// nothing of the topology behind them and reads nothing out of anything else - the role is the role it was
    /// handed, not one worked out of the state, and the state is the state and not one inferred from the role.</summary>
    [Fact]
    public void TheNodeKeepsItsFourPartsWhereTheyAreGiven()
    {
        var serves = new NodeHealth("prod-west", "westus3", true, Healthy);
        var waits = new NodeHealth("prod-east", "eastus2", false, Unhealthy);

        Assert.Equal("prod-west", serves.Name);
        Assert.Equal("westus3", serves.Region);
        Assert.True(serves.IsPrimary);
        Assert.Equal(Healthy, serves.State);

        Assert.Equal("prod-east", waits.Name);
        Assert.Equal("eastus2", waits.Region);
        Assert.False(waits.IsPrimary);
        Assert.Equal(Unhealthy, waits.State);

        // A role and a state are the two the node never reads out of the other.
        Assert.True(new NodeHealth("prod-west", "westus3", true, Unreachable).IsPrimary);
        Assert.False(new NodeHealth("prod-east", "eastus2", false, Healthy).IsPrimary);
        Assert.Equal(Healthy, new NodeHealth("prod-east", "eastus2", false, Healthy).State);
        Assert.Equal(Unreachable, new NodeHealth("prod-west", "westus3", true, Unreachable).State);
    }

    /// <summary>The state a node says is the state it was given and never one the node ought to be in: every one of
    /// the four stands as itself, and so do the numbers the enum does not name - kept as the state they are, apart
    /// from the four, apart from the number one step off them and apart from the other named state, whatever the
    /// record is asked about them.</summary>
    [Theory]
    [InlineData(Pending)]
    [InlineData(Healthy)]
    [InlineData(Unhealthy)]
    [InlineData(Unreachable)]
    [InlineData((HealthState)(-1))]
    [InlineData((HealthState)4)]
    [InlineData((HealthState)int.MaxValue)]
    public void TheStateStandsAsGivenEvenWhenTheEnumDoesNotNameIt(HealthState state)
    {
        var node = Primary(state);

        Assert.Equal(state, node.State);
        Assert.Equal(node, node with { });
        Assert.Equal(node.GetHashCode(), (node with { }).GetHashCode());
        Assert.Equal(node.GetHashCode(), new NodeHealth("app-west", "westus3", true, state).GetHashCode());
        Assert.StartsWith($"NodeHealth {{ Name = app-west, Region = westus3, IsPrimary = True, State = {state}, ", node.ToString(), StringComparison.Ordinal);

        // The other state of the pair is another node, whatever this one is.
        var other = state == Healthy ? Unhealthy : Healthy;
        Assert.NotEqual(node, node with { State = other });
        Assert.NotEqual(node.GetHashCode(), (node with { State = other }).GetHashCode());

        // So is the number one step off this one, which the enum does not name either side of it.
        var oneStepOff = (HealthState)((int)state == int.MaxValue ? (int)state - 1 : (int)state + 1);
        Assert.NotEqual(node, node with { State = oneStepOff });
    }

    /// <summary>A name and a region are kept as the words the topology wrote them in: nothing, the empty word, a
    /// blank, a single character, a region's own word, a padded upper-case word, the same word in another case and a
    /// word with an accented letter are each kept whole, and nothing is trimmed, upper-cased or read again on the way
    /// in.</summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("0")]
    [InlineData("westus3")]
    [InlineData(" WEST US 3 ")]
    [InlineData("WestUs")]
    [InlineData("wését us 3")]
    public void AWordTheTopologyWroteIsKeptAsTheWordItWrote(string? word)
    {
        var named = new NodeHealth(word!, word, true, Healthy);

        Assert.Equal(word, named.Name);
        Assert.Equal(word, named.Region);

        // The same word in both parts, and the two nodes of the two words apart from it are two nodes.
        Assert.Equal(named, new NodeHealth(word!, word, true, Healthy));
        Assert.Equal(named.GetHashCode(), new NodeHealth(word!, word, true, Healthy).GetHashCode());
        Assert.NotEqual(named, new NodeHealth(word!, "eastus2", true, Healthy));
        Assert.NotEqual(named, new NodeHealth("app-west", word, true, Healthy));

        // The word is said as it stands, spaces and all, both in the two parts it is and in the label it makes.
        var spoken = named.ToString();

        Assert.Contains($"Name = {word},", spoken, StringComparison.Ordinal);
        Assert.Contains($"Region = {word},", spoken, StringComparison.Ordinal);
        Assert.Contains($"Label = {word} }}", spoken, StringComparison.Ordinal);
    }

    /// <summary>The node asks nothing of its name: the record is made with a name of nothing, which is the one thing
    /// a topology the page reads never gives it, and the node stands - equal to another of nothing, apart from the
    /// empty word that is not nothing.</summary>
    [Fact]
    public void ANameOfNothingIsKeptAsTheNothingItIs()
    {
        var nameless = new NodeHealth(null!, "westus3", true, Healthy);

        Assert.Null(nameless.Name);
        Assert.Equal("westus3", nameless.Region);
        Assert.Equal("westus3", nameless.Label);

        Assert.Equal(nameless, new NodeHealth(null!, "westus3", true, Healthy));
        Assert.NotEqual(nameless, new NodeHealth(string.Empty, "westus3", true, Healthy));
        Assert.False(nameless.Equals(nameless with { Name = string.Empty }));
    }

    // ----- the label

    /// <summary>The label is the region when the topology gives one - and a region of no characters is a region the
    /// topology gave, so the label is the empty word and not the name - and the name when it gives none. The two
    /// branches are read off the node here; what they say in a headline is pinned in
    /// <see cref="ServingAssessmentTests" />.</summary>
    [Theory]
    [InlineData("app-west", "westus3", "westus3")]
    [InlineData("app-west", "WESTUS3", "WESTUS3")]
    [InlineData("app-west", " ", " ")]
    [InlineData("app-west", "", "")]
    [InlineData("app-west", "  westus3  ", "  westus3  ")]
    [InlineData("app-west", null, "app-west")]
    [InlineData("", null, "")]
    [InlineData("0", null, "0")]
    public void TheLabelIsTheRegionWhenThereIsOneAndTheNameWhenThereIsNone(string name, string? region, string label)
    {
        var node = new NodeHealth(name, region, true, Healthy);

        Assert.Equal(label, node.Label);
        Assert.Equal(label.Length, node.Label.Length);
    }

    /// <summary>A node that says neither a region nor a name has no label at all: the label is the nothing of the
    /// name it was given, which is the one thing a node of a real topology cannot be - the empty word, on the other
    /// hand, is a label, and a label of no characters is not the nothing of a label.</summary>
    [Fact]
    public void ANodeOfNoRegionAndNoNameSaysNoLabel()
    {
        var nothing = new NodeHealth(null!, null, true, Pending);
        var empty = new NodeHealth(string.Empty, null, true, Pending);

        Assert.Null(nothing.Name);
        Assert.Null(nothing.Region);
        Assert.Null(nothing.Label);

        Assert.Equal(string.Empty, empty.Label);
        Assert.Empty(empty.Label);
        Assert.NotEqual(nothing.Label, empty.Label);
        Assert.NotEqual(nothing, empty);

        // The node of nothing at all is still a node, and a node of the last state in the book of it.
        Assert.Equal(Pending, nothing.State);
        Assert.False(Standby(Pending, region: null).Equals(nothing));
        Assert.True(nothing == nothing with { });
    }

    /// <summary>The label says the region and the name and nothing else: a node is one label over every role and
    /// every state - and over the two turned the other way - and the label is read off the parts again when a copy
    /// moves one of them, not remembered from the first read.</summary>
    [Fact]
    public void TheLabelAsksNothingOfTheRoleOrTheStateAndIsReadAgainAfterACopy()
    {
        var labels = new[] { Primary(Healthy), Primary(Unhealthy), Primary(Unreachable), Primary(Pending), Standby(Healthy), Standby(Unhealthy) };

        Assert.All(labels, node => Assert.Equal("westus3", node.Label));
        Assert.Single(labels.Select(node => node.Label).Distinct(StringComparer.Ordinal));

        var nameless = Primary(Healthy, region: null);

        Assert.Equal("app-west", nameless.Label);
        Assert.Equal("eastus2", (nameless with { Region = "eastus2" }).Label);
        Assert.Equal("app-west", (nameless with { Region = null }).Label);
        Assert.Equal("eastus2", (nameless with { Region = "eastus2", Name = "app-east" }).Label);
        Assert.Equal("app-east", (nameless with { Region = null, Name = "app-east" }).Label);
        Assert.Equal("app-west", nameless.Label);
    }

    // ----- the record around the four parts

    /// <summary>A node is one with another while its four parts are, and with nothing else: the operators see the
    /// nothing a caller holds, <c>Equals</c> refuses a word, a state, an assessment and nothing alike, and the node
    /// of nothing at all is one with itself.</summary>
    [Fact]
    public void TwoNodesAreOneWhileTheirFourPartsAre()
    {
        var one = Primary(Healthy);
        var two = new NodeHealth("app-west", "westus3", true, Healthy);
        NodeHealth? nothing = null;

        Assert.True(one.Equals(two));
        Assert.True(two.Equals(one));
        Assert.True(one == two);
        Assert.False(one != two);
        Assert.True(one.Equals((object)two));
        Assert.True(one.Equals(one));
        Assert.True(one.Equals(one with { }));
        Assert.True(one.Equals((object)(one with { })));

        // Every one of the four parts moved is another node, the region made nothing included.
        Assert.False(one == one with { Name = "app-east" });
        Assert.False(one == one with { Name = string.Empty });
        Assert.False(one == one with { Name = null! });
        Assert.False(one == one with { Region = "eastus2" });
        Assert.False(one == one with { Region = null });
        Assert.False(one == one with { Region = string.Empty });
        Assert.False(one == one with { IsPrimary = false });
        Assert.False(one == one with { State = Unhealthy });
        Assert.False(one == one with { State = Unreachable });
        Assert.False(one == one with { State = Pending });

        // A standby of the very same name and region is another node than the primary.
        Assert.False(one.Equals(Standby(Healthy)));
        Assert.False(Standby(Healthy).Equals(nothing));
        Assert.False(one.Equals(nothing));
        Assert.False(one.Equals(null));
        Assert.False(one.Equals("app-west"));
        Assert.False(one.Equals(Healthy));
        Assert.False(one.Equals(one.Label));
        Assert.False(one.Equals(ServingAssessment.Assess([one, Standby(Unhealthy)], frontDoor: Healthy)));
        Assert.False(nothing == one);
        Assert.False(one == nothing);
        Assert.True(nothing == null);
        Assert.False(nothing != null);

        // A node of nothing at all is a node, and one with itself.
        var bare = new NodeHealth(null!, null, false, Pending);

        Assert.True(bare.Equals(bare with { }));
        Assert.True(bare == bare with { });
        Assert.False(bare.Equals(null));
    }

    /// <summary>The hash code follows the four parts and stands where it stands: two nodes of the same parts in two
    /// instances say one number, the same node says it again from call to call, nothing said of the region says
    /// another number than the empty word, and each part moved says another.</summary>
    [Fact]
    public void TheHashCodeFollowsTheFourPartsAndStandsFromCallToCall()
    {
        var one = Primary(Unhealthy);
        var two = new NodeHealth("app-west", "westus3", true, Unhealthy);

        Assert.Equal(one.GetHashCode(), one.GetHashCode());
        Assert.Equal(one.GetHashCode(), two.GetHashCode());
        Assert.Equal(one.GetHashCode(), (one with { }).GetHashCode());

        Assert.NotEqual(one.GetHashCode(), (one with { Name = "app-east" }).GetHashCode());
        Assert.NotEqual(one.GetHashCode(), (one with { Name = string.Empty }).GetHashCode());
        Assert.NotEqual(one.GetHashCode(), (one with { Name = null! }).GetHashCode());
        Assert.NotEqual(one.GetHashCode(), (one with { Region = null }).GetHashCode());
        Assert.NotEqual(one.GetHashCode(), (one with { Region = string.Empty }).GetHashCode());
        Assert.NotEqual(one.GetHashCode(), (one with { Region = "eastus2" }).GetHashCode());
        Assert.NotEqual(one.GetHashCode(), (one with { IsPrimary = false }).GetHashCode());
        Assert.NotEqual(one.GetHashCode(), (one with { State = Healthy }).GetHashCode());
        Assert.NotEqual(one.GetHashCode(), (one with { State = (HealthState)(-1) }).GetHashCode());

        // The node of nothing at all has a number of its own, and the same one twice over.
        var bare = new NodeHealth(null!, null, false, Pending);

        Assert.Equal(bare.GetHashCode(), new NodeHealth(null!, null, false, Pending).GetHashCode());
        Assert.NotEqual(bare.GetHashCode(), one.GetHashCode());
    }

    /// <summary>The four parts read out of the node in the order they stand in it - the name, the region, the role
    /// and the state - the node that says neither a name nor a region falling apart into the two nothings it is
    /// made of.</summary>
    [Fact]
    public void TheFourPartsReadOutOfTheNodeInTheOrderTheyStand()
    {
        var (name, region, isPrimary, state) = new NodeHealth("prod-west", "westus3", true, Unreachable);

        Assert.Equal("prod-west", name);
        Assert.Equal("westus3", region);
        Assert.True(isPrimary);
        Assert.Equal(Unreachable, state);

        var (standbyName, standbyRegion, standby, pending) = Standby(Pending, region: null);

        Assert.Equal("app-west", standbyName);
        Assert.Null(standbyRegion);
        Assert.False(standby);
        Assert.Equal(Pending, pending);

        var (noName, noRegion, isItPrimary, whatever) = new NodeHealth(null!, null, false, (HealthState)(-1));

        Assert.Null(noName);
        Assert.Null(noRegion);
        Assert.False(isItPrimary);
        Assert.Equal((HealthState)(-1), whatever);

        // What the node falls apart into is what it was made of: put back in the order they came out, the four parts
        // are the very node again.
        Assert.Equal(new NodeHealth("prod-west", "westus3", true, Unreachable), new NodeHealth(name, region, isPrimary, state));
    }

    /// <summary><c>with</c> names the part it changes and carries the other three along: the copy that names no part
    /// is the same node and not the same instance, and the node left standing does not move when its copy is
    /// made.</summary>
    [Fact]
    public void WithChangesOnlyThePartItNamesAndCarriesTheOtherThreeAlong()
    {
        var one = Primary(Healthy);

        var copy = one with { };

        Assert.Equal(one, copy);
        Assert.NotSame(one, copy);
        Assert.Equal(one.Label, copy.Label);

        var moved = one with { Name = "app-east", State = Unhealthy };

        Assert.Equal("app-east", moved.Name);
        Assert.Equal("westus3", moved.Region);
        Assert.True(moved.IsPrimary);
        Assert.Equal(Unhealthy, moved.State);
        Assert.Equal("westus3", moved.Label);

        // The node the copy was made from stands where it stood.
        Assert.Equal("app-west", one.Name);
        Assert.Equal(Healthy, one.State);
        Assert.NotEqual(one, moved);

        var demoted = one with { IsPrimary = false };

        Assert.False(demoted.IsPrimary);
        Assert.Equal(one.Label, demoted.Label);
        Assert.Equal(one.Name, demoted.Name);
        Assert.Equal(one.State, demoted.State);
        Assert.NotEqual(one.GetHashCode(), demoted.GetHashCode());
    }

    /// <summary>The node says its four parts in the order the record declares them, each named by the property it
    /// stands on, and says the label last of all as the word it stands for - not as a fifth part the node keeps: the
    /// region it has not said as the nothing it is, the empty word as a word of no characters, the role as the word a
    /// bool says, a state the enum does not name as its number where the four say their word, and a padded region
    /// said with the spaces it keeps in both the part and the label.</summary>
    [Fact]
    public void TheNodeSaysItsFourPartsInTheirOrderAndItsLabelLast()
    {
        Assert.Equal(
            "NodeHealth { Name = prod-west, Region = westus3, IsPrimary = True, State = Healthy, Label = westus3 }",
            new NodeHealth("prod-west", "westus3", true, Healthy).ToString());

        Assert.Equal(
            "NodeHealth { Name = prod-east, Region = eastus2, IsPrimary = False, State = Unhealthy, Label = eastus2 }",
            new NodeHealth("prod-east", "eastus2", false, Unhealthy).ToString());

        // A node with no region says the nothing it has and names itself by its name, in its words as in its label.
        Assert.Equal(
            "NodeHealth { Name = app-one, Region = , IsPrimary = True, State = Unreachable, Label = app-one }",
            new NodeHealth("app-one", null, true, Unreachable).ToString());

        Assert.Equal(
            "NodeHealth { Name = app-one, Region =  west 3 , IsPrimary = False, State = 4, Label =  west 3  }",
            new NodeHealth("app-one", " west 3 ", false, (HealthState)4).ToString());

        // The two words of no characters and the two of nothing are said apart, the label as the empty word the name
        // it stands on is.
        Assert.Equal(
            "NodeHealth { Name = , Region = , IsPrimary = False, State = Pending, Label =  }",
            new NodeHealth(string.Empty, string.Empty, false, Pending).ToString());

        Assert.Equal(
            "NodeHealth { Name = , Region = , IsPrimary = True, State = Pending, Label =  }",
            new NodeHealth(null!, null, true, Pending).ToString());

        // The label is the last word the node says, and the parts before it stand in the order the record has them.
        Assert.EndsWith(", Label = eastus2 }", Standby(Healthy, region: "eastus2").ToString(), StringComparison.Ordinal);
        var words = Primary(Pending).ToString();
        Assert.True(
            words.IndexOf("Name =", StringComparison.Ordinal) < words.IndexOf("Label =", StringComparison.Ordinal),
            $"The node says its label before its name: {words}");
    }
}
