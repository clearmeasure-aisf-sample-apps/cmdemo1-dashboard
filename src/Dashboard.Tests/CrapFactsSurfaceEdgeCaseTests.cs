using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Dashboard.Tests;

/// <summary>
/// <see cref="CrapFacts" /> is the build's CRAP score in three numbers, and three files already say almost
/// everything about them: <see cref="CrapFactsEdgeCaseTests" /> pins the record itself (its parts, its words, the
/// copies, the falling apart, the equality and the hash at the numbers a build reports),
/// <see cref="CrapFactsBoundaryEdgeCaseTests" /> pins the ends of every number and the numbers no build reports, and
/// <see cref="CrapFactsReadEdgeCaseTests" /> pins what an answer may say. <see cref="BuildInfoTests" />,
/// <see cref="BuildInfoEdgeCaseTests" />, <see cref="DeployableFactsTests" />,
/// <see cref="HealthHelperCoverageTests" /> and <see cref="BuildTextEdgeCaseTests" /> pin the facts as the build reads
/// them and as the card says them.
/// <para>
/// What is pinned here is only what those leave open, and none of it needs the network or a clock: the surface the
/// record declares, part by part and member by member, with the <c>init</c> that no plain <c>set</c> may stand in for
/// and the shape the falling apart is written in; the record standing as a key of a table the build keeps, where the
/// hash and the equality have to agree with each other and with themselves twice over; the order the record does not
/// have, which a caller has to ask a part for; the pattern a record's parts are written in, which is the falling apart
/// seen from the other end; the score said to the last digit of the number it stands on, which is where the record
/// stops and the card's rounding begins; and the record that is not there at all, which has no part to read.
/// </para>
/// </summary>
public class CrapFactsSurfaceEdgeCaseTests
{
    /// <summary>A build that measured a worst score of <c>28.5</c> against a threshold of <c>30</c> and found nothing
    /// over it - the sample build's own three numbers, and the record every test below starts from.</summary>
    private static readonly CrapFacts Measured = new(28.5, 30, 0);

    // ----- the surface the record declares

    /// <summary>The record's whole public surface, as it stands: the three parts, named in the order the constructor
    /// names them and of the type the constructor says - the worst score and the threshold as numbers that may go
    /// unsaid, the count of the methods over as a count that may - each read and each <c>init</c>able and none of them
    /// <c>set</c>table, which is what keeps a build's measurement from moving under whoever holds the build. Beyond
    /// them, only the members every record is asked: falling apart (in the three types it holds), comparing, hashing,
    /// saying itself and the two operators - and no other interface but the equality of its own kind. A fourth number
    /// added to the facts has to be written here as well, which is the point.</summary>
    [Fact]
    public void TheRecordDeclaresThreePartsAndTheMembersEveryRecordIsAsked()
    {
        Assert.True(typeof(CrapFacts).IsSealed);
        Assert.Equal([typeof(IEquatable<CrapFacts>)], typeof(CrapFacts).GetInterfaces());

        Assert.Equal(
            ["Max", "Threshold", "OverThreshold"],
            [.. typeof(CrapFacts)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Select(property => property.Name)]);

        Assert.Equal(
            ["Deconstruct", "Equals", "GetHashCode", "Max", "OverThreshold", "Threshold", "ToString", "op_Equality", "op_Inequality"],
            PublicSurface());

        var properties = typeof(CrapFacts)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .ToDictionary(property => property.Name, StringComparer.Ordinal);

        Assert.Equal([typeof(double?), typeof(double?), typeof(int?)], [.. properties.Values.Select(property => property.PropertyType)]);
        Assert.All(properties.Values, property =>
        {
            Assert.True(property.CanRead);
            Assert.NotNull(property.GetGetMethod());

            // The write is an init: the accessor is there, and it carries the marker the compiler asks a `with` to
            // stand on - so no one holding a fact can turn it after the record was made.
            var init = property.GetSetMethod()!;
            Assert.NotNull(init);
            Assert.Contains(typeof(IsExternalInit), init.ReturnParameter.GetRequiredCustomModifiers());
        });

        // Three parts, and the one constructor that takes them in that order, with nothing said for anyone.
        var constructors = typeof(CrapFacts).GetConstructors();

        Assert.Single(constructors);
        var parameters = constructors[0].GetParameters();

        Assert.Equal(["Max", "Threshold", "OverThreshold"], [.. parameters.Select(parameter => parameter.Name!)]);
        Assert.Equal([typeof(double?), typeof(double?), typeof(int?)], [.. parameters.Select(parameter => parameter.ParameterType)]);
        Assert.All(parameters, parameter => Assert.False(parameter.HasDefaultValue));

        // The falling apart hands back the three parts in the same order, as the same three types, each written out.
        var apart = typeof(CrapFacts).GetMethod("Deconstruct")!;
        var apartBy = apart.GetParameters();

        Assert.Equal(typeof(void), apart.ReturnType);
        Assert.Equal(["Max", "Threshold", "OverThreshold"], [.. apartBy.Select(parameter => parameter.Name!)]);
        Assert.All(apartBy, parameter => Assert.True(parameter.IsOut && parameter.ParameterType.IsByRef));
        Assert.Equal(
            [typeof(double?), typeof(double?), typeof(int?)],
            [.. apartBy.Select(parameter => parameter.ParameterType.GetElementType()!)]);
    }

    // ----- the record as a key of the build's table

    /// <summary>The facts are a value, and a value a table can be built on: the record answers
    /// <see cref="IEquatable{T}" /> alone, so the default comparer, a second reading of the same three numbers and a
    /// dictionary keyed by the facts all find the one entry - while the count turned by one, the count left unsaid and
    /// the score turned about are no such finding. The hash is the same number again whenever it is asked, the record
    /// copied and the record re-measured alike, and it is the number the default comparer says; the nothing of a key is
    /// hashed to the zero of it rather than asked for its parts, and the score that is no number at all is one key with
    /// itself, which is the equality the record already says it holds.</summary>
    [Fact]
    public void TheFactsAreOneKeyWhicheverWayTheTableAsksThem()
    {
        Assert.Equal(Measured.GetHashCode(), Measured.GetHashCode());
        Assert.Equal(Measured.GetHashCode(), (Measured with { }).GetHashCode());
        Assert.Equal(Measured.GetHashCode(), new CrapFacts(285e-1, 300e-1, 0).GetHashCode());
        Assert.NotEqual(Measured.GetHashCode(), new CrapFacts(28.5, 30, 1).GetHashCode());

        var table = new Dictionary<CrapFacts, string> { [Measured] = "measured" };

        Assert.Equal("measured", table[new CrapFacts(28.5, 30, 0)]);
        Assert.Equal("measured", table[Measured with { }]);
        Assert.Equal("measured", table[new CrapFacts(285e-1, 300e-1, 0)]);
        Assert.False(table.ContainsKey(new CrapFacts(28.5, 30, 1)));
        Assert.False(table.ContainsKey(new CrapFacts(30, 28.5, 0)));
        Assert.False(table.ContainsKey(new CrapFacts(28.5, 30, null)));
        Assert.False(table.ContainsKey(new CrapFacts(null, null, null)));

        // The record that carries the nothing of every part is a key of its own, and one key only.
        var unsaid = new Dictionary<CrapFacts, string> { [new CrapFacts(null, null, null)] = "nothing measured" };

        Assert.Equal("nothing measured", unsaid[new CrapFacts(null, null, null)]);
        Assert.False(unsaid.ContainsKey(Measured));

        // The score that is no number is one record with itself, so it is one key too: hash and equality agree.
        var endless = new Dictionary<CrapFacts, string> { [new CrapFacts(double.NaN, null, null)] = "not a number" };

        Assert.Equal("not a number", endless[new CrapFacts(double.NaN, null, null)]);

        Assert.True(EqualityComparer<CrapFacts>.Default.Equals(null, null));
        Assert.False(EqualityComparer<CrapFacts>.Default.Equals(Measured, null));
        Assert.False(EqualityComparer<CrapFacts>.Default.Equals(null, Measured));
        Assert.Equal(0, EqualityComparer<CrapFacts>.Default.GetHashCode(null!));
        Assert.Equal(Measured.GetHashCode(), EqualityComparer<CrapFacts>.Default.GetHashCode(Measured));
    }

    /// <summary>The facts say which two records are one and which are two; they do not say which of two is worse. The
    /// record wears no order - neither of the two <see cref="IComparable" /> shapes, nor the formattable or clonable
    /// faces a caller might mistake it for - so the default comparer has nothing to hold and says so, and a build that
    /// wants its measurements ranked has to rank them by a part. Asked that way, the record keeps its parts whole, and
    /// the nothing of a part sorts before every number of it: the two parts rank the same four records two ways.</summary>
    [Fact]
    public void TheFactsTellOneRecordFromAnotherAndOrderNoneOfThem()
    {
        var mild = new CrapFacts(12, 30, 0);
        var worst = new CrapFacts(41, 30, 3);
        var noCount = new CrapFacts(35, 30, null);
        var unsaid = new CrapFacts(null, null, null);

        Assert.False(typeof(IComparable).IsAssignableFrom(typeof(CrapFacts)));
        Assert.False(typeof(IComparable<CrapFacts>).IsAssignableFrom(typeof(CrapFacts)));
        Assert.False(typeof(IFormattable).IsAssignableFrom(typeof(CrapFacts)));
        Assert.False(typeof(ICloneable).IsAssignableFrom(typeof(CrapFacts)));

        Assert.Throws<ArgumentException>(() =>
        {
            _ = Comparer<CrapFacts>.Default.Compare(worst, mild);
        });

        List<CrapFacts> piled = [worst, noCount, mild, unsaid];

        // A sort asked of a part puts the nothing of it before every number, since the nothing is the least a
        // nullable number sorts at - and the two parts say two different orders of the very same four records.
        Assert.Equal([unsaid, mild, noCount, worst], [.. piled.OrderBy(crap => crap.Max)]);
        Assert.Equal([noCount, unsaid, mild, worst], [.. piled.OrderBy(crap => crap.OverThreshold)]);
    }

    // ----- the parts seen from the pattern end

    /// <summary>The falling apart, seen from the other end: a record stands in the pattern its parts are written in, so
    /// the three numbers name it, a hole among them lets any part through, and the nothing of a part is matched by the
    /// nothing of the pattern and by nothing else - the zero of a count is not the unsaid count, and the unsaid is not
    /// the zero. A part may also be matched as a number to compare against, which is how the card asks whether the
    /// build went over; and a record that is not there stands in no pattern at all, and has no part to read of it -
    /// the questions asked with a <c>?</c> answer the nothing, and the one asked without it falls over.</summary>
    [Fact]
    public void TheFactsStandInThePatternTheirPartsAreWrittenIn()
    {
        Assert.True(Measured is CrapFacts(28.5, 30, 0));
        Assert.True(Measured is CrapFacts(28.5, 30, _));
        Assert.True(Measured is CrapFacts(28.5, _, 0));
        Assert.True(Measured is CrapFacts({ } worst, { } threshold, 0) && worst == 28.5 && threshold == 30);
        Assert.False(Measured is CrapFacts(28.5, 30, 1));
        Assert.False(Measured is CrapFacts(28.5, 31, 0));

        // The nothing of a part is matched by the nothing of it, and not by the zero beside it.
        Assert.True(new CrapFacts(null, 30, null) is CrapFacts(null, 30, null));
        Assert.False(new CrapFacts(null, 30, null) is CrapFacts(0, 30, 0));
        Assert.False(new CrapFacts(28.5, 30, 0) is CrapFacts(null, 30, null));
        Assert.True(new CrapFacts(28.5, 30, 0) is not CrapFacts(null, null, null));

        // A part matched as a number to stand beside, which is what marks a build over its threshold.
        Assert.True(new CrapFacts(41, 30, 3) is CrapFacts(> 30, 30, > 0));
        Assert.False(new CrapFacts(28.5, 30, 0) is CrapFacts(> 30, 30, > 0));
        Assert.Equal("over", Mark(new CrapFacts(41, 30, 3)));
        Assert.Equal("clean", Mark(Measured));
        Assert.Equal("unsaid", Mark(new CrapFacts(28.5, 30, null)));

        // The record that is not there is in no pattern and has no part to read.
        CrapFacts? missing = null;

        Assert.False(missing is CrapFacts(28.5, 30, 0));
        Assert.True(missing is not CrapFacts);
        Assert.Null(missing?.Max);
        Assert.Null(missing?.Threshold);
        Assert.Null(missing?.OverThreshold);
        Assert.Null(missing?.ToString());
        Assert.Null(missing?.GetHashCode());

        Assert.Throws<NullReferenceException>(() =>
        {
            _ = missing!.Max;
        });
    }

    // ----- the score said to its last digit

    /// <summary>The words the record says of the score carry it to the last digit of the number it stands on, in the
    /// figures the invariant reader writes: the smallest positive number as <c>5E-324</c>, the millionth as
    /// <c>1E-06</c>, a tenth and a fifth in all seventeen of their digits, the neighbour of <c>28.5</c> told from it by
    /// the digit at the end, a third as far as the number runs, and the thousands of methods over without a separator
    /// between them. The card, said the same moment over the same record, rounds the score away, groups the count of
    /// the methods and leaves the threshold plain: the record keeps what the build measured, the card writes what a
    /// reader reads - which is why the record's words are no use on the page, and why nothing on the page is a use for
    /// them either.</summary>
    [Fact]
    public void TheRecordSaysTheScoreToTheLastDigitOfTheNumberItStandsOn()
    {
        Assert.Equal("CrapFacts { Max = 5E-324, Threshold = , OverThreshold =  }", Words(new CrapFacts(double.Epsilon, null, null)));
        Assert.Equal("CrapFacts { Max = 1E-06, Threshold = 30, OverThreshold =  }", Words(new CrapFacts(1e-06, 30, null)));
        Assert.Equal("CrapFacts { Max = 0.30000000000000004, Threshold = 30, OverThreshold = 0 }", Words(new CrapFacts(0.1 + 0.2, 30, 0)));
        Assert.Equal("CrapFacts { Max = 0.3333333333333333, Threshold = , OverThreshold =  }", Words(new CrapFacts(1d / 3d, null, null)));
        Assert.Equal("CrapFacts { Max = 28.500000000000004, Threshold = 30, OverThreshold = 0 }", Words(new CrapFacts(double.BitIncrement(28.5), 30, 0)));
        Assert.Equal("CrapFacts { Max = 28.5, Threshold = 30, OverThreshold = 1000 }", Words(new CrapFacts(28.5, 30, 1_000)));
        Assert.Equal("CrapFacts { Max = 1000000, Threshold = 1000000, OverThreshold = -1000 }", Words(new CrapFacts(1e6, 1_000_000, -1_000)));
        Assert.Equal("CrapFacts { Max = -1234.5, Threshold = -30.25, OverThreshold = -2 }", Words(new CrapFacts(-1234.5, -30.25, -2)));
        Assert.Equal("CrapFacts { Max = 2, Threshold = 2, OverThreshold = 2 }", Words(new CrapFacts(2d, 2, 2)));

        // The same records, said by the card: the digits cut back, the count grouped, the sign and the nothing kept.
        Assert.Equal("worst 28.5, 1,000 methods over 30", BuildText.Crap(new CrapFacts(28.5, 30, 1_000)));
        Assert.Equal("worst 0, none over 30", BuildText.Crap(new CrapFacts(0.04, 30, 0)));
        Assert.Equal("worst 28.5, none over 30", BuildText.Crap(new CrapFacts(double.BitIncrement(28.5), 30, 0)));
        Assert.Equal("worst 1000000, 1,000,000 methods over 1000000", BuildText.Crap(new CrapFacts(1e6, 1_000_000, 1_000_000)));
        Assert.Equal("worst 0.3, 2 methods over 30", BuildText.Crap(new CrapFacts(1d / 3d, 30, 2)));

        // The record keeps what the card cuts away, so the two words are two: the neighbour is the neighbour here.
        Assert.NotEqual(Words(new CrapFacts(double.BitIncrement(28.5), 30, 0)), Words(Measured));
        Assert.Equal(0.04, new CrapFacts(0.04, null, null).Max);
    }

    /// <summary>One score written two ways is one record, and the record a hair off it is another: the sum of a tenth
    /// and a fifth and the seventeen digits that name it are one record with one hash and one word, while the tenth of
    /// them rounded is a hair below and therefore two records; <c>285e-1</c> beside <c>28.5</c> and <c>300e-1</c>
    /// beside <c>30</c> is the one measurement again, while the double a hair above it and the double a hair below it
    /// are records of their own. The set counts the records apart only where the numbers are apart, which is all a
    /// build needs: two runs that measured the same thing have to be one fact on the card.</summary>
    [Fact]
    public void OneScoreWrittenTwoWaysIsOneRecordAndItsNeighbourIsAnother()
    {
        var summed = new CrapFacts(0.1 + 0.2, 30, 0);
        var written = new CrapFacts(0.30000000000000004, 30, 0);
        var rounded = new CrapFacts(0.3, 30, 0);

        Assert.Equal(summed, written);
        Assert.True(summed == written);
        Assert.Equal(summed.GetHashCode(), written.GetHashCode());
        Assert.Equal(Words(summed), Words(written));
        Assert.NotEqual(summed, rounded);
        Assert.True(summed != rounded);
        Assert.Equal(2, new HashSet<CrapFacts> { summed, written, rounded }.Count);

        var measured = new CrapFacts(28.5, 30, 0);
        var inTenths = new CrapFacts(285e-1, 300e-1, 0);
        var above = new CrapFacts(double.BitIncrement(28.5), 30, 0);
        var below = new CrapFacts(double.BitDecrement(28.5), 30, 0);
        var thresholdMoved = new CrapFacts(28.5, double.BitIncrement(30), 0);

        Assert.Equal(measured, inTenths);
        Assert.True(measured == inTenths);
        Assert.NotEqual(measured, above);
        Assert.NotEqual(measured, below);
        Assert.NotEqual(measured, thresholdMoved);
        Assert.NotEqual(Words(measured), Words(above));
        Assert.NotEqual(Words(measured), Words(below));
        Assert.Equal(4, new HashSet<CrapFacts> { measured, inTenths, above, below, thresholdMoved }.Count);

        // The score and the threshold a hair apart are still the two numbers the build measured, in their places.
        Assert.Equal(28.5, measured.Max);
        Assert.Equal(30.0, measured.Threshold);
        Assert.Equal(28.5, double.BitDecrement(double.BitIncrement(28.5)));
    }

    // ----- the machinery

    /// <summary>The public members the facts declare themselves: its properties and the methods that are not the get
    /// and the set of one - the two operators kept although the compiler names them specially, since they are the
    /// <c>==</c> and the <c>!=</c> a caller writes - with the record's compiler-named clone walked past. The
    /// constructor is no method, so it is named by the test above rather than by this list.</summary>
    private static List<string> PublicSurface() =>
        [.. typeof(CrapFacts)
            .GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(member => member is PropertyInfo
                             || member is MethodInfo method
                             && (!method.IsSpecialName || method.Name.StartsWith("op_", StringComparison.Ordinal)))
            .Where(member => !member.Name.Contains('<', StringComparison.Ordinal))
            .Select(member => member.Name)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];

    /// <summary>The words a record says its three parts in, taken in the invariant culture whatever the reader of this
    /// test happens to be: the record's own words follow whoever reads them, which
    /// <see cref="CrapFactsBoundaryEdgeCaseTests" /> pins on its own, so the strings above are the point and the
    /// culture only the way to say them once.</summary>
    private static string Words(CrapFacts crap) => ViewerCulture.Invariant(() => crap.ToString()!);

    /// <summary>What three numbers say about a build, asked through a pattern rather than through the words of
    /// <see cref="BuildText" />: some methods over, none over, or the count left unsaid.</summary>
    private static string Mark(CrapFacts crap) =>
        crap switch
        {
            CrapFacts(_, _, > 0) => "over",
            CrapFacts(_, _, 0) => "clean",
            _ => "unsaid",
        };
}
