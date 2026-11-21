namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="ClusterVolume" />, the one record of
/// <c>ClusterStatus.cs</c> that stands for a volume claim of a namespace: its three parts as the record is given
/// them, the capacity at the ends and the odd shapes of the number, the phase as the word it is, the two volumes
/// that are one, the parts a volume falls apart into and the one line a volume says about itself. What another
/// test already pins is not pinned again: <see cref="ParsedEdgeCaseTests" /> holds the capacity at the absent, the
/// zero and the top of the number, the phase at the nothing and the empty word, the two volumes that are one and
/// the copy that is it again, and the name that is only spaces out of a file; <see cref="ClusterParserTests" />
/// holds the sample's <c>data-db-0</c> and the volume a file says nothing of but its name;
/// <see cref="ClusterTextTests" /> holds the words the page makes of a volume; and
/// <see cref="NamespaceGroupTests" /> and <see cref="NamespaceGroupEdgeCaseTests" /> hold the volumes a namespace's
/// group carries.
/// <para>
/// What is pinned here is the edge those readings sit next to, and only that: the name the record keeps with its
/// spaces about it - no trimming is the record's, the reader's - and the name that is the empty word, the
/// capacity at the bottom of the number, below it, at the smallest number there is and at the biggest whole one
/// the number counts exactly; the three words a claim is bound in, and the word spelled another way; equality
/// through the operators, the object and the hash, at each part on its own and at the numbers that agree with
/// themselves - the not-a-number, the two infinities and the zero with a sign; the three parts in the order the
/// record declares them; and the capacity and the phase a file spells in the wrong kind of word.
/// </para>
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: a volume says no moment, every one of them is
/// built by the test or read from text handed over.
/// </para>
/// </summary>
public class ClusterVolumeEdgeCaseTests
{
    // ------------------------------------------------------------------ the three parts as they are given

    /// <summary>The volume keeps the very three parts it is given, in the shape it is given them: the name with the
    /// spaces about it stands with them, because trimming the word of a file is what the reader does and never what the
    /// record does, the empty name is a name the record is made with, and a capacity and a phase that are absent are
    /// the nothing they are and not the zero and the word they are next to.</summary>
    [Fact]
    public void TheVolumeKeepsItsThreePartsInTheShapeTheyAreGivenIn()
    {
        var claim = new ClusterVolume("data-db-0", 8589934592d, "Bound");

        Assert.Equal("data-db-0", claim.Name);
        Assert.Equal(8589934592d, claim.Capacity);
        Assert.Equal("Bound", claim.Phase);

        var padded = new ClusterVolume("  data  ", 1d, "  Pending  ");

        Assert.Equal("  data  ", padded.Name);
        Assert.Equal("  Pending  ", padded.Phase);
        Assert.NotEqual("data", padded.Name);
        Assert.NotEqual("Pending", padded.Phase);

        var nameless = new ClusterVolume(string.Empty, null, null);

        Assert.Equal(string.Empty, nameless.Name);
        Assert.Null(nameless.Capacity);
        Assert.Null(nameless.Phase);
        Assert.NotEqual(new ClusterVolume(" ", null, null), nameless);
    }

    // --------------------------------------------------------------------------------------------- Capacity

    /// <summary>The capacity at the ends and the odd shapes of the number the record keeps without a word about
    /// them: the bottom of the number, which is a capacity below nothing - the record is no reader and refuses
    /// nothing - the smallest number a double holds, the byte that is the least of a real one, and the biggest whole
    /// number the double counts exactly, two to the fifty-third, past which the number stops being able to say one
    /// more byte at all.</summary>
    [Theory]
    [InlineData(-1d)]
    [InlineData(-9007199254740992d)]
    [InlineData(double.MinValue)]
    [InlineData(double.Epsilon)]
    [InlineData(1d)]
    [InlineData(9007199254740991d)]
    [InlineData(9007199254740992d)]
    public void TheCapacityKeepsTheEndsAndTheEdgesOfTheNumberItIsGiven(double bytes)
    {
        var claim = new ClusterVolume("data", bytes, "Bound");
        var copy = claim with { };

        Assert.Equal(bytes, claim.Capacity);
        Assert.Equal(bytes, copy.Capacity);
        Assert.Equal(new ClusterVolume("data", bytes, "Bound"), claim);
        Assert.NotEqual(claim, claim with { Name = "cache" });
        Assert.NotEqual(claim, claim with { Phase = "Pending" });
    }

    /// <summary>The capacity that is no capacity at all stands beside the numbers it is not: the absent one, the zero
    /// that is a number, and the two ends of the number are four volumes of which no two are one - and the nothing
    /// turns a volume into itself again, which is what the copy that names no part says.</summary>
    [Fact]
    public void ANoCapacityIsNotTheZeroNorEitherEndOfTheNumber()
    {
        ClusterVolume[] apart =
        [
            new ClusterVolume("data", null, "Bound"),
            new ClusterVolume("data", 0d, "Bound"),
            new ClusterVolume("data", double.MinValue, "Bound"),
            new ClusterVolume("data", double.MaxValue, "Bound"),
        ];

        foreach (var claim in apart)
        {
            Assert.Same(claim, Assert.Single(apart, other => claim.Equals(other)));
            Assert.Equal(claim, claim with { });
        }

        Assert.Null((apart[1] with { Capacity = null }).Capacity);
        Assert.Equal(0d, apart[1].Capacity);
        Assert.Equal(double.MinValue, apart[2].Capacity);
        Assert.Equal(double.MaxValue, apart[3].Capacity);
    }

    // ------------------------------------------------------------------------------------------------ Phase

    /// <summary>The three words a claim is bound in, kept as the record is given them: each stands for itself, a word
    /// spelled with other letters is another word to the record - which knows no case, and no meaning - and a word
    /// with a space on it is not the word, since the record trims nothing.</summary>
    [Theory]
    [InlineData("Bound")]
    [InlineData("Pending")]
    [InlineData("Lost")]
    public void ThePhaseStandsOnTheWordItWasGivenAndOnNoOther(string phase)
    {
        var claim = new ClusterVolume("data", 1073741824d, phase);

        Assert.Equal(phase, claim.Phase);
        Assert.NotEqual(claim, new ClusterVolume("data", 1073741824d, phase.ToLowerInvariant()));
        Assert.NotEqual(claim, new ClusterVolume("data", 1073741824d, phase.ToUpperInvariant()));
        Assert.NotEqual(claim, new ClusterVolume("data", 1073741824d, $" {phase} "));
        Assert.NotEqual(claim, new ClusterVolume("data", 1073741824d, null));
        Assert.NotEqual(claim, new ClusterVolume("data", 1073741824d, string.Empty));
    }

    // ------------------------------------------------------------------------ two volumes that are one volume

    /// <summary>Two volumes are one volume while their three parts are, however the equality is asked - the operator
    /// both ways, the typed and the untyped <c>Equals</c>, the hash the dictionary looks at - and each of the three
    /// parts on its own is enough to make them two: the name, the capacity, the phase. The volume is not one with
    /// nothing, nor with a word, which is the other half of what <c>Equals</c> owes its callers.</summary>
    [Fact]
    public void TwoVolumesAreOneVolumeWhileTheirThreePartsAre()
    {
        var one = new ClusterVolume("data-db-0", 8589934592d, "Bound");
        var two = new ClusterVolume("data-db-0", 8589934592d, "Bound");

        Assert.True(one == two);
        Assert.False(one != two);
        Assert.True(one.Equals(two));
        Assert.True(two.Equals((object)one));
        Assert.Equal(one.GetHashCode(), two.GetHashCode());

        ClusterVolume[] other =
        [
            new ClusterVolume("data-db-1", 8589934592d, "Bound"),
            new ClusterVolume("data-db-0", 8589934593d, "Bound"),
            new ClusterVolume("data-db-0", null, "Bound"),
            new ClusterVolume("data-db-0", 8589934592d, "Pending"),
            new ClusterVolume("data-db-0", 8589934592d, null),
        ];

        foreach (var volume in other)
        {
            Assert.False(one == volume);
            Assert.True(one != volume);
            Assert.False(one.Equals(volume));
            Assert.False(one.Equals((object)volume));
            Assert.NotEqual(one, volume);
        }

        Assert.False(one.Equals(null));
        Assert.False(one.Equals("data-db-0"));
        Assert.False(one.Equals(new ClusterPod("data-db-0", null, null, null, ClusterPod.Running, true, null, null, 0, null, null, ResourceUse.None, ResourceUse.None)));

        // The hash the collections count on: one volume once, and the two that are one take one place in a set.
        Assert.Single(new HashSet<ClusterVolume> { one, two });
        Assert.Equal(2, new HashSet<ClusterVolume> { one, other[0] }.Count);
        Assert.Equal("bound", new Dictionary<ClusterVolume, string> { [two] = "bound" }[one]);
    }

    /// <summary>The volumes at the shapes of the number that disagree with the <c>==</c> of the number itself: a
    /// not-a-number is one with itself, and each of the infinities is one with itself and not with the other, and the
    /// zero with a sign on it is the zero without one. The record asks the number whether it is the same number, not
    /// whether the <c>==</c> says so - which is why it says so of the not-a-number and does not of the two
    /// infinities. No file spells a capacity either way, since the reader holds both off the list of numbers.</summary>
    [Fact]
    public void TheOddShapesOfTheNumberAreOneWithThemselves()
    {
        Assert.Equal(new ClusterVolume("data", double.NaN, "Bound"), new ClusterVolume("data", double.NaN, "Bound"));
        Assert.Equal(
            new ClusterVolume("data", double.NaN, "Bound").GetHashCode(),
            new ClusterVolume("data", double.NaN, "Bound").GetHashCode());

        Assert.Equal(new ClusterVolume("data", double.PositiveInfinity, null), new ClusterVolume("data", double.PositiveInfinity, null));
        Assert.NotEqual(new ClusterVolume("data", double.PositiveInfinity, null), new ClusterVolume("data", double.NegativeInfinity, null));
        Assert.NotEqual(new ClusterVolume("data", double.NaN, null), new ClusterVolume("data", 0d, null));

        Assert.Equal(new ClusterVolume("data", -0d, "Bound"), new ClusterVolume("data", 0d, "Bound"));
        Assert.True(new ClusterVolume("data", -0d, "Bound") == new ClusterVolume("data", 0d, "Bound"));
        Assert.False(new ClusterVolume("data", -0d, "Bound") != new ClusterVolume("data", 0d, "Bound"));
    }

    // ---------------------------------------------------------------------------- Deconstruct and ToString

    /// <summary>The volume falls apart into its three parts, in the order the record declares them - the name, the
    /// capacity, the phase - each as the value it stands on, the nothing coming back as the nothing it is and not as
    /// a word or a zero.</summary>
    [Fact]
    public void TheVolumeFallsApartIntoItsThreePartsInTheOrderTheyAreDeclared()
    {
        var (name, capacity, phase) = new ClusterVolume("data-db-0", 8589934592d, "Bound");

        Assert.Equal("data-db-0", name);
        Assert.Equal(8589934592d, capacity);
        Assert.Equal("Bound", phase);

        var (noName, noCapacity, noPhase) = new ClusterVolume(string.Empty, null, null);

        Assert.Equal(string.Empty, noName);
        Assert.Null(noCapacity);
        Assert.Null(noPhase);

        var claim = new ClusterVolume("data", 1024d, "Lost");
        var turned = claim with { Phase = "Pending" };

        Assert.Equal("data", turned.Name);
        Assert.Equal(1024d, turned.Capacity);
        Assert.Equal("Pending", turned.Phase);
        Assert.Equal("Lost", claim.Phase);
    }

    /// <summary>The one line the record says about itself names the type and then its three parts in the order they
    /// are declared - the line a failing test prints, and the reader of it sees which part moved. A part that is
    /// nothing is said as nothing, not as a word for it; a capacity the page would call GiB is said in the bytes it
    /// is.</summary>
    [Fact]
    public void TheVolumeSaysItsThreePartsInOneLineOfItsOwn()
    {
        Assert.Equal(
            "ClusterVolume { Name = data-db-0, Capacity = 8589934592, Phase = Bound }",
            new ClusterVolume("data-db-0", 8589934592d, "Bound").ToString());

        Assert.Equal(
            "ClusterVolume { Name = , Capacity = , Phase =  }",
            new ClusterVolume(string.Empty, null, null).ToString());

        var text = new ClusterVolume("data", 1073741824d, "Pending").ToString();

        Assert.StartsWith("ClusterVolume { ", text, StringComparison.Ordinal);
        Assert.True(
            text.IndexOf("Name =", StringComparison.Ordinal) < text.IndexOf("Capacity =", StringComparison.Ordinal),
            text);
        Assert.True(
            text.IndexOf("Capacity =", StringComparison.Ordinal) < text.IndexOf("Phase =", StringComparison.Ordinal),
            text);
    }

    // --------------------------------------------------------------------- the parts a file spells otherwise

    /// <summary>A volume the file spells in the wrong kind of word keeps the name and loses the rest: a capacity that
    /// is a word, a number below nothing or a number too big for the number to hold is no capacity, a phase that is a
    /// number, a word or a list is no phase, and the volume itself is still the one its name says - while the capacity
    /// that is a fraction is a capacity, in the bytes the file counts. The parts absent from the file are pinned by
    /// <see cref="ClusterParserTests" />; the parts of the wrong kind, and the element that is no object at all, are
    /// pinned here.</summary>
    [Fact]
    public void AVolumeOfTheWrongKindOfWordKeepsItsNameAndLosesTheRest()
    {
        var status = ClusterStatus.Parse("""
            { "namespaces": [ { "name": "apps", "volumes": [
                    { "name": "slow", "capacity": "8 GiB", "phase": 3 },
                    { "name": "negative", "capacity": -1, "phase": true },
                    { "name": "unholdable", "capacity": 1e400, "phase": [] },
                    { "name": "half", "capacity": 0.5, "phase": "Lost" },
                    "text" ] } ] }
            """).Value!;

        var spaces = Assert.Single(status.Namespaces);
        Assert.Equal(["slow", "negative", "unholdable", "half"], spaces.Volumes.Select(volume => volume.Name));

        var slow = spaces.Volumes[0];
        Assert.Null(slow.Capacity);
        Assert.Null(slow.Phase);
        Assert.Equal(new ClusterVolume("slow", null, null), slow);

        Assert.Null(spaces.Volumes[1].Capacity);
        Assert.Null(spaces.Volumes[1].Phase);
        Assert.Null(spaces.Volumes[2].Capacity);
        Assert.Null(spaces.Volumes[2].Phase);

        Assert.Equal(0.5d, spaces.Volumes[3].Capacity);
        Assert.Equal("Lost", spaces.Volumes[3].Phase);
        Assert.NotEqual(spaces.Volumes[3], spaces.Volumes[0]);
    }
}
