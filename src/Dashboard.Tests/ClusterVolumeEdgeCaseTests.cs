namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="ClusterVolume" />: the three parts the volume
/// keeps of what it is given, the three parts it falls apart into, the words it says them in, the copies that
/// turn one of them, and the volumes that are one and the volumes that are not.
/// <see cref="ClusterParserTests" /> pins the sample file's volume at the values it stands on and
/// the volume the reader leaves out where the file names no name; and
/// <see cref="ClusterTextTests" /> and <see cref="ClusterTextEdgeCaseTests" /> pin the line the page makes out of
/// the three parts, not the parts themselves.
/// <para>
/// What is pinned here is what they leave open, and only that: the name the volume keeps as the word it is - at
/// the empty word, at the spaces about a word, at the letter and at the length the word stands on, and at the
/// nothing the volume is handed, which it keeps as the nothing it is since the volume is not the reader that
/// drops a nameless one; the capacity at the bottom of the number, at the negative, at the smallest number the
/// number holds and at the fraction of a byte, and the two numbers the number holds that are no count of bytes
/// at all, kept as the numbers they are and not read as the nothing they are not; the phase as whatever word it
/// is, in another case, with the spaces about it and with a word Kubernetes never uses, none of them refused and
/// none of them rewritten; the three parts the volume falls apart into, each as the value it stands on; the
/// words the record says its parts in, the nothing and the empty word said the way it says them and in the order
/// the parts are named; the copy that turns one part and keeps the other two, and the parts written as the
/// initializer writes them; and the parts that turn two volumes apart - the name turned, the capacity turned by
/// one byte, the phase turned to nothing, to the empty word and to another case - beside the two the record
/// still holds as one: the negative zero and the zero, and the number that is no number, which is one volume
/// with the volume that holds the same number, though it is no capacity at all.
/// </para>
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: every volume is one the test builds.
/// </para>
/// </summary>
public class ClusterVolumeEdgeCaseTests
{
    private const double EightGiB = ClusterFixture.EightGiB;

    // ----- The parts the volume keeps -----

    /// <summary>The volume keeps the capacity it is given, at the zero that is a number and at the top of
    /// the number, and says nothing where none is given: the phase the volume was not given stands the
    /// nothing it is, and the phase that is the empty word stands the empty word it is, kept, not read as
    /// absent.</summary>
    [Fact]
    public void AVolumeKeepsItsCapacityAtTheEndsAndSaysNothingWhereNoneIsGiven()
    {
        var noCapacity = new ClusterVolume("data", null, null);
        Assert.Null(noCapacity.Capacity);
        Assert.Null(noCapacity.Phase);

        var zero = new ClusterVolume("data", 0, "Bound");
        Assert.Equal(0d, zero.Capacity);
        Assert.Equal("Bound", zero.Phase);

        var top = new ClusterVolume("data", double.MaxValue, string.Empty);
        Assert.Equal(double.MaxValue, top.Capacity);
        Assert.Equal(string.Empty, top.Phase);

        Assert.Equal(noCapacity, new ClusterVolume("data", null, null));
        Assert.NotEqual(noCapacity, new ClusterVolume("data", 0, null));
        Assert.NotEqual(zero, new ClusterVolume("data", 0, "Pending"));

        var copy = noCapacity with { };
        Assert.Equal(noCapacity, copy);
        Assert.NotSame(noCapacity, copy);
    }

    /// <summary>
    /// The volume keeps the very parts it is given: the name and the phase as the words they are and the
    /// capacity as the number it is, each coming back as the property of the same name. The nothing it is
    /// given stands the nothing it is and the empty word stands the empty word it is - and the name that is no
    /// word at all is kept as the nothing it is, for the volume guards nothing: it is the reader of the file
    /// that leaves a nameless volume out.
    /// </summary>
    [Fact]
    public void AVolumeKeepsTheVeryPartsItIsGiven()
    {
        var data = new ClusterVolume("data-db-0", EightGiB, "Bound");

        Assert.Equal("data-db-0", data.Name);
        Assert.Equal(EightGiB, data.Capacity);
        Assert.Equal("Bound", data.Phase);

        var silent = new ClusterVolume(string.Empty, null, null);

        Assert.Equal(string.Empty, silent.Name);
        Assert.Null(silent.Capacity);
        Assert.Null(silent.Phase);

        Assert.Null(new ClusterVolume(null!, null, null).Name);
    }

    /// <summary>
    /// The name stands at the word it is, at the ends the word stands in: the empty word, the word that is
    /// only the space or the tab it is, the spaces about a word kept as the spaces they are, the single letter,
    /// the word that runs to a second line and the word in another case - the volume is not the reader and does
    /// not drop what is empty, does not trim what has spaces about it and does not choose the case it stands in.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData("  data  ")]
    [InlineData("a")]
    [InlineData("data\n")]
    [InlineData("Data-DB-0")]
    public void ANameStandsAtTheWordItIsAndAtTheEndsTheWordStandsIn(string name)
    {
        Assert.Equal(name, new ClusterVolume(name, null, null).Name);
    }

    /// <summary>The name stands at the length it stands on: the long word kept as the word it is, neither
    /// shortened nor wrapped, beside the single letter it also keeps whole.</summary>
    [Fact]
    public void ANameStandsAtTheLengthItStandsOn()
    {
        var longName = new string('d', 128);
        var volume = new ClusterVolume(longName, 1024d, "Bound");

        Assert.Equal(longName, volume.Name);
        Assert.Equal(128, volume.Name.Length);
        Assert.Equal("d", new ClusterVolume("d", 1024d, "Bound").Name);
    }

    /// <summary>
    /// The capacity stands at the ends of the number the number stands in and at the numbers a count of bytes
    /// is made of that no count of bytes is: the bottom of the number, the negative the file could never say,
    /// the smallest number the number holds and the half byte, each kept as the number it is - and the zero
    /// that is a number and the nothing that is not a number are pinned by the reading above, next to this one.
    /// </summary>
    [Theory]
    [InlineData(double.MinValue)]
    [InlineData(-1d)]
    [InlineData(double.Epsilon)]
    [InlineData(0.5d)]
    [InlineData(1024d)]
    [InlineData(EightGiB)]
    public void ACapacityStandsAtTheNumberItIsGiven(double capacity)
    {
        var volume = new ClusterVolume("data", capacity, "Bound");

        Assert.Equal(capacity, volume.Capacity);
        Assert.NotNull(volume.Capacity);
    }

    /// <summary>The two numbers the number holds that are no count of bytes at all stand as the numbers they
    /// are, kept whole and not read as the nothing they are not: the capacity that is no number is a capacity
    /// the volume holds, and so is the capacity past the end of the number.</summary>
    [Fact]
    public void ACapacityThatIsNoCountOfBytesIsStillACapacityTheVolumeKeeps()
    {
        var notAQuantity = new ClusterVolume("data", double.NaN, "Bound");
        var endless = new ClusterVolume("data", double.PositiveInfinity, "Bound");

        Assert.NotNull(notAQuantity.Capacity);
        Assert.True(double.IsNaN(notAQuantity.Capacity!.Value));
        Assert.NotNull(endless.Capacity);
        Assert.True(double.IsPositiveInfinity(endless.Capacity!.Value));
    }

    /// <summary>
    /// The phase stands as the word it is and no word is refused: the three words the claim is ever in, the
    /// same word in another case, the word with the spaces about it, a word of another thing altogether and a
    /// word of nothing but digits - the volume owns no words of its own to answer with, so what it is given is
    /// what it holds.
    /// </summary>
    [Theory]
    [InlineData("Bound")]
    [InlineData("Pending")]
    [InlineData("Lost")]
    [InlineData("bound")]
    [InlineData(" Bound ")]
    [InlineData("Available")]
    [InlineData("0")]
    public void APhaseStandsAsTheWordItIsAndNoWordIsRefused(string phase)
    {
        Assert.Equal(phase, new ClusterVolume("data", 1024d, phase).Phase);
    }

    // ----- The parts the volume falls apart into -----

    /// <summary>
    /// The volume falls apart into its three parts, each as the value it stands on and in the order the volume
    /// names them - the name, the capacity, the phase - and the empty word and the nothing come out as they
    /// stand, the empty word not the nothing it is not and the nothing not the empty word it is not.
    /// </summary>
    [Fact]
    public void AVolumeFallsApartIntoItsThreeParts()
    {
        var (name, capacity, phase) = new ClusterVolume("data-db-0", EightGiB, "Bound");

        Assert.Equal("data-db-0", name);
        Assert.Equal(EightGiB, capacity);
        Assert.Equal("Bound", phase);

        var (noName, noCapacity, noPhase) = new ClusterVolume(string.Empty, null, null);

        Assert.Equal(string.Empty, noName);
        Assert.Null(noCapacity);
        Assert.Null(noPhase);

        var (bareName, bareCapacity, barePhase) = new ClusterVolume(null!, null, string.Empty);

        Assert.Null(bareName);
        Assert.Null(bareCapacity);
        Assert.Equal(string.Empty, barePhase);
    }

    // ----- The words the volume says its parts in -----

    /// <summary>
    /// The words the volume says its parts in - the line a failing test prints: the record's own words, the
    /// volume's name at the front, then its three parts in the order they are declared, each said as the value
    /// it stands on. A part that is nothing is said as nothing, and the nothing and the empty word are said in
    /// the same words although the two volumes they say are not one volume.
    /// </summary>
    [Fact]
    public void AVolumeSaysItsPartsInTheWordsTheRecordSaysThem()
    {
        Assert.Equal(
            "ClusterVolume { Name = data-db-0, Capacity = 8589934592, Phase = Bound }",
            new ClusterVolume("data-db-0", EightGiB, "Bound").ToString());

        Assert.Equal(
            "ClusterVolume { Name = data, Capacity = 1024, Phase = Pending }",
            new ClusterVolume("data", 1024d, "Pending").ToString());

        Assert.Equal(
            "ClusterVolume { Name = , Capacity = , Phase =  }",
            new ClusterVolume(string.Empty, null, null).ToString());

        Assert.Equal(
            "ClusterVolume { Name = , Capacity = 0, Phase =  }",
            new ClusterVolume(null!, 0d, string.Empty).ToString());

        var text = new ClusterVolume("data", 0d, "Lost").ToString();

        Assert.StartsWith("ClusterVolume { ", text, StringComparison.Ordinal);
        Assert.True(
            text.IndexOf("Name =", StringComparison.Ordinal) < text.IndexOf("Capacity =", StringComparison.Ordinal),
            text);
        Assert.True(
            text.IndexOf("Capacity =", StringComparison.Ordinal) < text.IndexOf("Phase =", StringComparison.Ordinal),
            text);

        // The silence and the empty word say the same line, and are still two volumes.
        Assert.Equal(new ClusterVolume("data", null, null).ToString(), new ClusterVolume("data", null, string.Empty).ToString());
        Assert.NotEqual(new ClusterVolume("data", null, null), new ClusterVolume("data", null, string.Empty));
    }

    // ----- The copies -----

    /// <summary>
    /// The copy of a volume that turns one of the parts it keeps keeps the other two: the name turned alone
    /// leaves the capacity and the phase where they stood, and the capacity turned and the phase turned each
    /// keep the rest - and the copy is another volume, never the same one. The parts can be written the other
    /// way round too: an initializer sets the three as the constructor sets them, to the same volume.
    /// </summary>
    [Fact]
    public void ACopyThatTurnsOneOfThePartsItKeepsKeepsTheRest()
    {
        var one = new ClusterVolume("data", EightGiB, "Bound");

        var renamed = one with { Name = "logs" };
        Assert.Equal("logs", renamed.Name);
        Assert.Equal(EightGiB, renamed.Capacity);
        Assert.Equal("Bound", renamed.Phase);
        Assert.NotSame(one, renamed);

        var resized = one with { Capacity = 1073741824d };
        Assert.Equal(1073741824d, resized.Capacity);
        Assert.Equal("data", resized.Name);
        Assert.Equal("Bound", resized.Phase);
        Assert.NotSame(one, resized);

        var released = one with { Capacity = null };
        Assert.Null(released.Capacity);
        Assert.Equal("data", released.Name);
        Assert.Equal("Bound", released.Phase);

        var unbound = one with { Phase = null };
        Assert.Null(unbound.Phase);
        Assert.Equal("data", unbound.Name);
        Assert.Equal(EightGiB, unbound.Capacity);
        Assert.NotSame(one, unbound);

        var written = new ClusterVolume("cache", null, null) { Name = "data", Capacity = EightGiB, Phase = "Bound" };

        Assert.Equal(one, written);
        Assert.NotSame(one, written);
    }

    // ----- The volumes that are one and the volumes that are not -----

    /// <summary>
    /// The volumes that are one are one: the three parts standing the same way make the operators, the
    /// <see cref="object.Equals(object)" /> of a volume asked as an object, the hash code and the set that
    /// holds them as one volume - and the volume asked whether it is the nothing, a word or another thing
    /// altogether says no, whichever way the question is put - and the same answer holds of the volume and of
    /// another volume built part for part like it, which is what the readers of a set and of a dictionary lean on.
    /// </summary>
    [Fact]
    public void TheVolumesThatAreOneAreOneAndTheOperatorsAndTheHashFollow()
    {
        var one = new ClusterVolume("data", EightGiB, "Bound");
        var two = new ClusterVolume("data", EightGiB, "Bound");

        AreOneVolume(one, two);
        AreOneVolume(one, one with { });

        ClusterVolume? absent = null;

        Assert.False(one == absent);
        Assert.True(one != absent);
        Assert.False(one.Equals(absent));
        Assert.False(one.Equals(null));
        Assert.False(one.Equals("not a volume"));
        Assert.False(one.Equals((object)ResourceUse.None));
    }

    /// <summary>
    /// The parts that turn two volumes apart: the name turned - in its case as in its letters - the capacity
    /// turned by the one byte and turned to the nothing, and the phase turned to nothing, to the empty word and
    /// to another case, each is another volume. What the record does <em>not</em> tell apart are two ways of
    /// standing on one number: the negative zero and the zero are one volume with one hash code, and the number
    /// that is no number is one volume with the volume that is no number too, though it is no capacity at all and
    /// not the nothing a capacity that is not there is - the record compares each part as the value it stands on.
    /// </summary>
    [Fact]
    public void APartTurnedIsAnotherVolumeAndTheNumbersThatAreOneNumberMakeOneVolume()
    {
        var one = new ClusterVolume("data", EightGiB, "Bound");

        ClusterVolume[] different =
        [
            one with { Name = "cache" },
            one with { Name = "data " },
            one with { Name = "Data" },
            one with { Capacity = 8589934591d },
            one with { Capacity = null },
            one with { Capacity = double.MinValue },
            one with { Phase = null },
            one with { Phase = string.Empty },
            one with { Phase = "bound" },
            one with { Phase = "Pending" },
        ];

        foreach (var other in different)
        {
            AreApart(one, other);
        }

        var zero = new ClusterVolume("data", 0d, "Bound");
        var negativeZero = new ClusterVolume("data", -0d, "Bound");

        AreOneVolume(zero, negativeZero);
        AreApart(zero, zero with { Capacity = null });

        var noQuantity = new ClusterVolume("data", double.NaN, "Bound");
        var alsoNoQuantity = new ClusterVolume("data", double.NaN, "Bound");

        AreOneVolume(noQuantity, alsoNoQuantity);
        Assert.True(double.IsNaN(noQuantity.Capacity!.Value));
        AreApart(noQuantity, noQuantity with { Capacity = null });
    }

    // ----------------------------------------------------------------------------------------- helpers

    /// <summary>Two volumes the record holds as one volume: equal both ways round, the two operators, the
    /// <see cref="object.Equals(object)" /> of a volume asked as an object, the hash code that follows them and
    /// the set that keeps them as its one entry.</summary>
    private static void AreOneVolume(ClusterVolume one, ClusterVolume other)
    {
        Assert.Equal(one, other);
        Assert.Equal(other, one);
        Assert.True(one.Equals(other));
        Assert.True(one == other);
        Assert.False(one != other);
        Assert.True(one.Equals((object)other));
        Assert.True(other.Equals((object)one));
        Assert.Equal(one.GetHashCode(), other.GetHashCode());

        HashSet<ClusterVolume> theSame = [one, other];

        Assert.Single(theSame);
    }

    /// <summary>Two volumes the record holds apart: neither the equality nor either operator, asked either way
    /// round, makes them one. Their hash codes are left alone - two volumes apart may still fall in one bucket.</summary>
    private static void AreApart(ClusterVolume one, ClusterVolume other)
    {
        Assert.NotEqual(one, other);
        Assert.NotEqual(other, one);
        Assert.False(one.Equals(other));
        Assert.False(one == other);
        Assert.False(other == one);
        Assert.True(one != other);
        Assert.False(one.Equals((object)other));
        Assert.False(other.Equals((object)one));
    }
}
