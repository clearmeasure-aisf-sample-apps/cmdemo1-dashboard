namespace Dashboard.Tests;

/// <summary>
/// <see cref="ResourceUse" /> is one resource - CPU in millicores, or memory in bytes - of a node or a pod: four
/// numbers, each of which may be absent, and no rule of its own. The node's and the pod's rows print them through
/// the use cell - the part that says nothing is the dash the cell says for no measurement - and
/// <see cref="ClusterTotals" /> sums them field by field.
/// <para>
/// What the file makes of them is already pinned where the reading happens: <see cref="ClusterParserTests" /> reads
/// the shipped <c>cluster.json</c> and says that a node says its four numbers, that a limit the file does not set is
/// no limit, that a finished pod says no usage, and that a node or a pod the file does not say a resource about is
/// <see cref="ResourceUse.None" />. The app declares no <c>InternalsVisibleTo</c>, so the internal <c>Read</c> is
/// pinned through <see cref="ClusterStatus.Parse" /> there rather than here. <see cref="ClusterTotalsEdgeCaseTests" />
/// pins the sum - a field no node says is not in the sum, a usage of zero is a sum of zero - and the line the record
/// says about itself, which this file therefore does not say again.
/// </para>
/// <para>
/// What follows is the record itself, which all of that walks past: the four arguments landing on the property of
/// the same name (before this file the parts were only ever read as the sums of
/// <see cref="ClusterTotalsEdgeCaseTests" />, so no test could tell two swapped parts of a resource built by hand),
/// an allocatable not given being nothing, and a part not given being nothing rather than zero. Then the four parts
/// as the numbers they are held - the two ends of <see cref="double" />, the smallest number above zero, a whole
/// number a byte count may stand on, a negative, the two infinities, and the number that is no number - with nothing
/// added, subtracted or checked. Then the members every assertion above leans on without ever saying a word about
/// them: <see cref="ResourceUse.None" /> read by the names of its own parts, a part that is said - even said as zero
/// - telling it apart; that two resources are one when all four parts are, with the hash that goes with it, boxed
/// and unboxed, and each part alone telling them apart; the four parts coming back out in the order they went in;
/// and the copy that changes the part it names and nothing else.
/// </para>
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: the record holds no moment and calls nothing,
/// so every number below is one the test brought. And the record holds no word, so the part that is not there is
/// the part that is nothing rather than the part that is empty.
/// </para>
/// </summary>
public class ResourceUseEdgeCaseTests
{
    /// <summary>A resource with every part filled and the four values named apart from one another, so that an
    /// argument landing on the wrong property is seen instead of quietly matching another part of the same record.</summary>
    private static ResourceUse Full => new(312, 2000, 4000, 9300);

    // -------------------------------------------------------------------------- the constructor

    /// <summary>The constructor's four parts, position by position, each come back as the property of the same name
    /// - the order <c>ResourceUse.Read</c> hands the file over in, and the order the total sums them in.</summary>
    [Fact]
    public void TheConstructorHandsEachArgumentToThePropertyOfItsName()
    {
        var use = new ResourceUse(312, 2000, 4000, 9300);

        Assert.Equal(312d, use.Usage);
        Assert.Equal(2000d, use.Requests);
        Assert.Equal(4000d, use.Limits);
        Assert.Equal(9300d, use.Allocatable);
    }

    /// <summary>The part the file may not have is the one the constructor leaves out by name: a resource built with
    /// its three first parts says nothing about what it offers - not zero, not a part of another kind - while the
    /// three parts it was given stand as they were given.</summary>
    [Fact]
    public void AnAllocatableThatIsNotGivenIsNothing()
    {
        var use = new ResourceUse(312, 2000, 4000);

        Assert.Equal(312d, use.Usage);
        Assert.Equal(2000d, use.Requests);
        Assert.Equal(4000d, use.Limits);
        Assert.Null(use.Allocatable);
    }

    /// <summary>A resource that says nothing about any of its four parts is one whose four parts are nothing:
    /// nothing is kept as zero, as a part of another kind or as the resource itself being nothing, which is what
    /// lets the cell say no measurement rather than a zero the file never said.</summary>
    [Fact]
    public void APartThatIsNotGivenIsNothingRatherThanZero()
    {
        var use = new ResourceUse(null, null, null);

        Assert.Null(use.Usage);
        Assert.Null(use.Requests);
        Assert.Null(use.Limits);
        Assert.Null(use.Allocatable);
    }

    // ------------------------------------------------------------------------------ the four parts

    /// <summary>Each of the four parts is the number it was given, at every end of <see cref="double" />: zero, a
    /// fraction, a whole number a byte count may stand on, a negative, the smallest number above zero, the top and
    /// the bottom of the type and the two infinities. The record adds nothing to a number, takes nothing from it
    /// and checks nothing of it - a negative usage or a limit below zero stands as it was handed over, since it is
    /// the reading of the file, not the record, that a number may not be.</summary>
    [Theory]
    [InlineData(0)]
    [InlineData(0.25)]
    [InlineData(312)]
    [InlineData(-7)]
    [InlineData(double.Epsilon)]
    [InlineData(4503599627370496d)]
    [InlineData(double.MaxValue)]
    [InlineData(double.MinValue)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void EachPartIsTheNumberItWasGivenAtEveryEndOfDouble(double value)
    {
        var use = new ResourceUse(value, value, value, value);

        Assert.Equal((double?)value, use.Usage);
        Assert.Equal((double?)value, use.Requests);
        Assert.Equal((double?)value, use.Limits);
        Assert.Equal((double?)value, use.Allocatable);
    }

    /// <summary>The one number a part may be that is not a number: a usage the collector says as no number is kept
    /// as that no number. By the rule of values it stands on, it is equal to the other no number and hashed with
    /// it, and it tells the resource apart from the one that says a number.</summary>
    [Fact]
    public void APartThatIsNoNumberIsStillTheValueItWasGiven()
    {
        var notANumber = Full with { Usage = double.NaN };
        var another = Full with { Usage = double.NaN };

        Assert.True(double.IsNaN(notANumber.Usage!.Value));

        Assert.True(notANumber.Equals(another));
        Assert.True(notANumber == another);
        Assert.Equal(notANumber, another);
        Assert.Equal(notANumber.GetHashCode(), another.GetHashCode());

        Assert.NotEqual(notANumber, Full);
        Assert.False(notANumber.Equals(Full));
    }

    // ---------------------------------------------------------------------------------------------- none

    /// <summary>The one resource that says nothing at all, read by the names of its own parts: not a resource that
    /// is nothing, but the resource whose four parts are each nothing. Every read of it gives the same one back -
    /// the parts the sum leaves out and the row shows as no measurement are its parts.</summary>
    [Fact]
    public void NoneIsTheResourceThatSaysNothingAtAll()
    {
        Assert.NotNull(ResourceUse.None);
        Assert.Null(ResourceUse.None.Usage);
        Assert.Null(ResourceUse.None.Requests);
        Assert.Null(ResourceUse.None.Limits);
        Assert.Null(ResourceUse.None.Allocatable);

        Assert.Same(ResourceUse.None, ResourceUse.None);
        Assert.Equal(new ResourceUse(null, null, null), ResourceUse.None);
        Assert.Equal(new ResourceUse(null, null, null, null), ResourceUse.None);
    }

    /// <summary>Saying a number is not saying nothing, even saying zero: a part of zero is a part the file says,
    /// and the one with it is not <see cref="ResourceUse.None" />. It is the reading of the file that a number not
    /// said is nothing, and the total that a number said as zero is still a number.</summary>
    [Fact]
    public void APartThatIsSaidTellsItApartFromNone()
    {
        Assert.NotEqual(ResourceUse.None, new ResourceUse(0, null, null, null));
        Assert.NotEqual(ResourceUse.None, new ResourceUse(null, 0, null, null));
        Assert.NotEqual(ResourceUse.None, new ResourceUse(null, null, 0, null));
        Assert.NotEqual(ResourceUse.None, new ResourceUse(null, null, null, 0));
        Assert.NotEqual(ResourceUse.None, Full);
    }

    // ------------------------------------------------------------------------------ the equality

    /// <summary>Two resources are one when all four parts are, whichever of the four ways of asking is used -
    /// <c>object.Equals</c>, the record's own <c>Equals</c>, <c>==</c> and <c>!=</c> - and a resource read twice
    /// from the same numbers is one resource. What is not equal to it: the same resource with any one part moved
    /// or dropped to nothing, a part of zero, <see langword="null" />, and a thing of another type that happens to
    /// carry the same numbers. The hash that goes with it stands on the value: equal resources hash alike, boxed
    /// or unboxed.</summary>
    [Fact]
    public void TwoResourcesAreOneWhenAllFourPartsAre()
    {
        var one = Full;
        var same = new ResourceUse(312, 2000, 4000, 9300);

        Assert.True(one.Equals(same));
        Assert.True(one == same);
        Assert.False(one != same);
        Assert.True(one.Equals((object)same));
        Assert.Equal(one, same);
        Assert.Equal(one.GetHashCode(), same.GetHashCode());
        Assert.Equal(one.GetHashCode(), ((object)one).GetHashCode());
        Assert.Equal(one, one with { Usage = 312, Requests = 2000, Limits = 4000, Allocatable = 9300 });

        ResourceUse[] different =
        [
            one with { Usage = null },
            one with { Usage = 0 },
            one with { Usage = 313 },
            one with { Requests = null },
            one with { Requests = 1999 },
            one with { Limits = null },
            one with { Limits = 4001 },
            one with { Allocatable = null },
            one with { Allocatable = 9299 },
        ];

        foreach (var other in different)
        {
            Assert.NotEqual(one, other);
            Assert.False(one.Equals(other));
            Assert.False(one.Equals((object)other));
            Assert.True(one != other);
        }

        Assert.False(one.Equals(null));
        Assert.False(one.Equals((ResourceUse?)null));
        Assert.False(one.Equals(312d));
        Assert.False(one.Equals(new { Usage = 312, Requests = 2000, Limits = 4000, Allocatable = 9300 }));
        Assert.False(ResourceUse.None.Equals(null));
        Assert.True(ResourceUse.None.Equals(new ResourceUse(null, null, null, null)));
    }

    /// <summary>As a key, a resource is its value: two equal instances are one entry of a set, a resource that
    /// differs in a single part is another entry, and the one a file's reading hands over is interchangeable with
    /// the same four numbers written out by hand - the reader builds an ordinary record of values, nothing apart.</summary>
    [Fact]
    public void AResourceIsAKeyOfItsOwnValue()
    {
        var uses = new HashSet<ResourceUse> { Full };
        var sameNumbers = new ResourceUse(312, 2000, 4000, 9300);
        var noLimits = Full with { Limits = null };

        Assert.Contains(sameNumbers, uses);
        Assert.DoesNotContain(noLimits, uses);

        uses.Add(sameNumbers);
        uses.Add(ResourceUse.None);
        Assert.Equal(2, uses.Count);

        var keyed = new Dictionary<ResourceUse, string> { [ResourceUse.None] = "nothing" };
        keyed[ResourceUse.None] = "still nothing";
        keyed[Full] = "full";

        Assert.Equal(2, keyed.Count);
        Assert.Equal("still nothing", keyed[new ResourceUse(null, null, null)]);
        Assert.Equal("full", keyed[Full]);
    }

    // ------------------------------------------------------------- the parts come out and the copy stands

    /// <summary>The four parts come back out in the order they went in - the order of the record's declaration,
    /// usage, requests, limits and allocatable - and the parts that are nothing come out as nothing.</summary>
    [Fact]
    public void TheFourPartsComeOutAgainInTheOrderTheyWentIn()
    {
        var (usage, requests, limits, allocatable) = Full;

        Assert.Equal(312d, usage);
        Assert.Equal(2000d, requests);
        Assert.Equal(4000d, limits);
        Assert.Equal(9300d, allocatable);

        var (noUsage, noRequests, noLimits, noAllocatable) = ResourceUse.None;

        Assert.Null(noUsage);
        Assert.Null(noRequests);
        Assert.Null(noLimits);
        Assert.Null(noAllocatable);
    }

    /// <summary>A copy changes the part it names and nothing else: the original stands as it stood while the copy
    /// carries the moved part, and a copy built from <see cref="ResourceUse.None" /> keeps the parts it does not
    /// name nothing. A copy that names every part its own value is the one it was copied from, and not the same
    /// one.</summary>
    [Fact]
    public void ACopyChangesThePartItNamesAndNothingElse()
    {
        var original = Full;
        var copied = original with { Limits = 6000 };

        Assert.Equal(4000d, original.Limits);
        Assert.Equal(6000d, copied.Limits);
        Assert.Equal(312d, copied.Usage);
        Assert.Equal(2000d, copied.Requests);
        Assert.Equal(9300d, copied.Allocatable);
        Assert.NotEqual(original, copied);

        var sameParts = original with { Usage = 312, Requests = 2000, Limits = 4000, Allocatable = 9300 };

        Assert.Equal(original, sameParts);
        Assert.NotSame(original, sameParts);

        var fromNothing = ResourceUse.None with { Requests = 268435456 };

        Assert.Null(fromNothing.Usage);
        Assert.Equal(268435456d, fromNothing.Requests);
        Assert.Null(fromNothing.Limits);
        Assert.Null(fromNothing.Allocatable);
        Assert.NotEqual(ResourceUse.None, fromNothing);
    }

    // --------------------------------------------------------------------- the shared words

    /// <summary>The words the record says about itself when every part is the nothing it was given: the constant
    /// that every edge-case file pins through interpolation, named once in <see cref="RecordWords" /> and read
    /// here from the instance the test builds itself. The bytes are the pinned bytes; the test says so by
    /// comparing the constant to the words of <see cref="ResourceUse.None" />.</summary>
    [Fact]
    public void TheRecordWordsConstantIsTheWordsOfANothingUse()
    {
        Assert.Equal(RecordWords.EmptyResourceUse, ResourceUse.None.ToString());
    }

    /// <summary>The constant is not only the words of <see cref="ResourceUse.None" />: it is the words of any use
    /// built with no parts said, the same string the cell, the sum and the row all stand on. A use that says one
    /// part is not the constant - the constant is for the all-nothing use alone.</summary>
    [Fact]
    public void TheConstantIsTheWordsOfAnyUseWithNoPartsSaid()
    {
        Assert.Equal(RecordWords.EmptyResourceUse, new ResourceUse(null, null, null).ToString());
        Assert.Equal(RecordWords.EmptyResourceUse, new ResourceUse(null, null, null, null).ToString());
        Assert.NotEqual(RecordWords.EmptyResourceUse, new ResourceUse(0, null, null, null).ToString());
        Assert.NotEqual(RecordWords.EmptyResourceUse, Full.ToString());
    }
}
