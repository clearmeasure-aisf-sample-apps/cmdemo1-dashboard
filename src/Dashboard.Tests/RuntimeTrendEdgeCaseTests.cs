using System.Reflection;
using System.Text.Json;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="RuntimeTrend" />: the sparkline beside a number, as the
/// script is handed it. The record decides nothing of its own - no scale, no rounding, no words. What is pinned here is
/// that <see cref="RuntimeTrend.Of" /> hands over the very parts of the health trend rather than copies of them, that
/// the record refuses nothing it is given, that its heights are weighed as the list they are held in, and that the
/// members every record is asked compare, hash, fall apart, copy and say the two parts.
/// <para>
/// What the other files already pin is left alone. <see cref="RuntimeTrendAndNameTests" /> pins that a trend of the
/// probes becomes the heights and words the script draws, that no trend and a trend of one reading give none, that a
/// trend of no readings is copied with nothing to draw, that heights outside 0 to 1 are kept, that two copies of one
/// trend are one trend while another title or another list is not, the two property names, and the JSON the script
/// reads. <see cref="HealthTextHelperTests" /> pins how <see cref="Trend.Of" /> scales and words the readings.
/// </para>
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: every value below is one the test brought.
/// </para>
/// </summary>
public class RuntimeTrendEdgeCaseTests
{
    private const string Title = "Requests per minute, last 3 checks: 0 to 40, now 20";

    /// <summary>The heights of one trend, held as one list so the fixtures below are copies of one trend.</summary>
    private static readonly IReadOnlyList<double> Points = new List<double> { 0, 1, 0.5 };

    /// <summary>One height held as an array, for the words an array says about itself.</summary>
    private static readonly double[] OneHeight = [0.5];

    private static RuntimeTrend Full => new(Points, Title);

    private static RuntimeTrend Nothing => new(null!, null!);

    // ------------------------------------------------------------------------------------------- Of

    [Fact]
    public void OfHandsOverTheVeryHeightsAndTitleOfTheHealthTrend()
    {
        var source = new Trend(Points, Title);
        var trend = RuntimeTrend.Of(source)!;

        Assert.Same(source.Points, trend.Points);
        Assert.Same(source.Title, trend.Title);
        Assert.Equal(new RuntimeTrend(Points, Title), trend);

        // Two calls on one trend are two records and one trend.
        var again = RuntimeTrend.Of(source)!;

        Assert.NotSame(trend, again);
        Assert.Equal(trend.GetHashCode(), again.GetHashCode());
    }

    [Fact]
    public void OfTakesNoCopySoAListThatGrowsGrowsTheTrend()
    {
        var list = new List<double> { 0, 1 };
        var trend = RuntimeTrend.Of(new Trend(list, Title))!;

        list.Add(0.25);

        Assert.Equal([0, 1, 0.25], trend.Points);
        Assert.Equal(3, trend.Points.Count);
    }

    [Fact]
    public void OfRefusesNothingTheHealthTrendHolds()
    {
        // A health trend built by hand may hold no list, no title, and heights no scale would give: Of passes them on.
        var nothing = RuntimeTrend.Of(new Trend(null!, null!))!;

        Assert.Null(nothing.Points);
        Assert.Null(nothing.Title);
        Assert.Equal(Nothing, nothing);

        var odd = RuntimeTrend.Of(new Trend(new List<double> { double.NaN, double.PositiveInfinity, -0.0, 1e9 }, string.Empty))!;

        Assert.True(double.IsNaN(odd.Points[0]));
        Assert.Equal(double.PositiveInfinity, odd.Points[1]);
        Assert.True(double.IsNegative(odd.Points[2]));
        Assert.Equal(1e9, odd.Points[3]);
        Assert.Equal(string.Empty, odd.Title);
    }

    [Fact]
    public void OfTheLongestTrendANodeKeepsKeepsEveryHeightInOrder()
    {
        var readings = Enumerable.Range(0, Trend.Length).Select(index => (double?)index).ToList();
        var trend = RuntimeTrend.Of(Trend.Of(readings, "Requests per minute"))!;

        Assert.Equal(Trend.Length, trend.Points.Count);
        Assert.Equal(0, trend.Points[0]);
        Assert.Equal(1, trend.Points[^1]);
        Assert.Equal(Math.Round(30d / 59, 3), trend.Points[30]);
        Assert.Equal("Requests per minute, last 60 checks: 0 to 59, now 59", trend.Title);
    }

    [Fact]
    public void OfTwoReadingsOfZeroIsAFlatLineAtTheBottom()
    {
        // The smallest input that makes a line at all, and the one whose largest reading is no scale.
        var trend = RuntimeTrend.Of(Trend.Of([0d, 0d], "SQL commands per minute"))!;

        Assert.Equal([0, 0], trend.Points);
        Assert.Equal("SQL commands per minute, last 2 checks: 0 to 0, now 0", trend.Title);
    }

    // ------------------------------------------------------------------------------- the two parts

    [Fact]
    public void TheRecordRefusesNothingItIsGiven()
    {
        Assert.Null(Nothing.Points);
        Assert.Null(Nothing.Title);
        Assert.Equal(Nothing, new RuntimeTrend(null!, null!));
        Assert.Equal(Nothing.GetHashCode(), new RuntimeTrend(null!, null!).GetHashCode());

        var empty = new RuntimeTrend(Array.Empty<double>(), string.Empty);

        Assert.Empty(empty.Points);
        Assert.Equal(string.Empty, empty.Title);
        Assert.NotEqual(Nothing, empty);
        Assert.NotEqual(new RuntimeTrend(Points, string.Empty), new RuntimeTrend(Points, null!));
    }

    [Fact]
    public void TheTitleIsKeptAsItIsWrittenAndWeighedOrdinally()
    {
        Assert.Equal("  spaced  ", new RuntimeTrend(Points, "  spaced  ").Title);
        Assert.NotEqual(Full, Full with { Title = Title.ToUpperInvariant() });
        Assert.NotEqual(Full, Full with { Title = Title + " " });

        ViewerCulture.German(() =>
        {
            Assert.NotEqual(new RuntimeTrend(Points, "ist"), new RuntimeTrend(Points, "ıst"));
            Assert.Equal(Full, new RuntimeTrend(Points, Title));
        });
    }

    [Fact]
    public void TheHeightsAreWeighedAsTheListTheyAreHeldIn()
    {
        var one = new RuntimeTrend(new List<double> { 0, 1 }, Title);
        var two = new RuntimeTrend(new List<double> { 0, 1 }, Title);

        Assert.Equal(one.Points, two.Points);
        Assert.NotEqual(one, two);
        Assert.False(one == two);
        Assert.Equal(2, new HashSet<RuntimeTrend> { one, two }.Count);

        var shared = new RuntimeTrend(one.Points, Title);

        Assert.True(one == shared);
        Assert.Single(new HashSet<RuntimeTrend> { one, shared });

        // A list and an array of the same heights are two parts.
        Assert.NotEqual(one, new RuntimeTrend(new double[] { 0, 1 }, Title));
    }

    // -------------------------------------------------------------------- the record of the two parts

    [Fact]
    public void NoTrendIsOneWithNoTrendAndATrendIsNotItsParts()
    {
        RuntimeTrend? none = null;
        RuntimeTrend? alsoNone = null;

        Assert.True(none == alsoNone);
        Assert.False(none == Full);
        Assert.True(Full != none);
        Assert.False(Full.Equals(null));
        Assert.False(Full.Equals((object?)null));
        Assert.False(Full.Equals(Title));
        Assert.False(Full.Equals(Points));
        Assert.False(Full.Equals(new Trend(Points, Title)));
        Assert.True(Full.Equals((object)new RuntimeTrend(Points, Title)));
    }

    [Fact]
    public void WithChangesOnlyThePartItNames()
    {
        var copy = Full with { };

        Assert.Equal(Full, copy);
        Assert.NotSame(Full, copy);
        Assert.Same(Points, copy.Points);

        var retitled = Full with { Title = "CPU of the process" };

        Assert.Same(Points, retitled.Points);
        Assert.Equal("CPU of the process", retitled.Title);

        var empty = Full with { Points = [] };

        Assert.Empty(empty.Points);
        Assert.Equal(Title, empty.Title);
        Assert.Equal(3, Full.Points.Count);
    }

    [Fact]
    public void TheTwoPartsFallOutInTheOrderTheyWentIn()
    {
        var (points, title) = Full;

        Assert.Same(Points, points);
        Assert.Equal(Title, title);
        Assert.Equal(Full, new RuntimeTrend(points, title));

        var (nothingPoints, nothingTitle) = Nothing;

        Assert.Null(nothingPoints);
        Assert.Null(nothingTitle);
    }

    [Fact]
    public void ATrendSaysItsTwoPartsInTheirOrderWhateverTheReaderSpeaks()
    {
        const string full = "RuntimeTrend { Points = System.Collections.Generic.List`1[System.Double], Title = Requests per minute, last 3 checks: 0 to 40, now 20 }";

        Assert.Equal(full, Full.ToString());
        Assert.Equal("RuntimeTrend { Points = , Title =  }", Nothing.ToString());
        Assert.Equal("RuntimeTrend { Points = System.Double[], Title = x }", new RuntimeTrend(OneHeight, "x").ToString());
        ViewerCulture.German(() => Assert.Equal(full, Full.ToString()));
    }

    // ------------------------------------------------------------------------ JSON at the edges

    [Fact]
    public void TheHeightsAreWrittenAsTheyStandAndAnEmptyTitleIsStillWritten()
    {
        var trend = new RuntimeTrend(new List<double> { 0, 0.001, 1, -0.5 }, string.Empty);

        using var json = JsonDocument.Parse(new RuntimePayload([], [], [new RuntimeEdgeMark("e1", "active", null, null, null, "t", null, trend)]).ToJson());
        var drawn = json.RootElement.GetProperty("edges")[0].GetProperty("trend");

        Assert.Equal("""{"points":[0,0.001,1,-0.5],"title":""}""", drawn.GetRawText());
    }

    [Fact]
    public void AHeightThatIsNoNumberCannotBeWritten()
    {
        // The health side never makes one (it drops the readings that are not finite); a trend built by hand that
        // holds one is refused by the writer rather than written as something the script cannot read.
        var trend = new RuntimeTrend(new List<double> { double.NaN, 1 }, Title);
        var payload = new RuntimePayload([], [], [new RuntimeEdgeMark("e1", "active", null, null, null, "t", null, trend)]);

        var refusal = Assert.Throws<ArgumentException>(() => payload.ToJson());

        Assert.Contains("relationship 'e1'", refusal.Message, StringComparison.Ordinal);
        Assert.Contains("(NaN at 0)", refusal.Message, StringComparison.Ordinal);
    }

    // ------------------------------------------------------------------------ the record's own shape

    [Fact]
    public void TheTrendHasTwoPartsOfMethodOfAndTheMembersEveryRecordIsAsked()
    {
        Assert.Equal(
            ["Deconstruct", "Equals", "GetHashCode", "Of", "Points", "Title", "ToString", "op_Equality", "op_Inequality"],
            PublicSurface());

        var of = typeof(RuntimeTrend).GetMethod(nameof(RuntimeTrend.Of))!;

        Assert.True(of.IsStatic);
        Assert.Equal(typeof(RuntimeTrend), of.ReturnType);
        Assert.Equal([typeof(Trend)], [.. of.GetParameters().Select(parameter => parameter.ParameterType)]);

        var nullability = new NullabilityInfoContext();

        Assert.Equal(NullabilityState.Nullable, nullability.Create(of.ReturnParameter).ReadState);
        Assert.Equal(NullabilityState.Nullable, nullability.Create(of.GetParameters()[0]).WriteState);

        var constructor = Assert.Single(typeof(RuntimeTrend).GetConstructors());

        Assert.Equal(["Points", "Title"], [.. constructor.GetParameters().Select(parameter => parameter.Name!)]);
        Assert.Equal([typeof(IReadOnlyList<double>), typeof(string)], [.. constructor.GetParameters().Select(parameter => parameter.ParameterType)]);
        Assert.All(constructor.GetParameters(), parameter => Assert.False(parameter.HasDefaultValue));
        Assert.All(constructor.GetParameters(), parameter => Assert.Equal(NullabilityState.NotNull, nullability.Create(parameter).WriteState));

        Assert.True(typeof(RuntimeTrend).IsSealed);
        Assert.Equal([typeof(IEquatable<RuntimeTrend>)], typeof(RuntimeTrend).GetInterfaces());
    }

    private static List<string> PublicSurface() =>
        [.. typeof(RuntimeTrend)
            .GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(member => member is PropertyInfo
                             || member is MethodInfo method
                             && (!method.IsSpecialName || method.Name.StartsWith("op_", StringComparison.Ordinal)))
            .Where(member => !member.Name.Contains('<', StringComparison.Ordinal))
            .Select(member => member.Name)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];
}
