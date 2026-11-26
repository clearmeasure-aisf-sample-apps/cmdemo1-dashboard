using System.Globalization;
using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// <see cref="HealthWindow" /> is the count of a window of the hourly health reports in the two numbers the
/// file gives it - how many reports ended in the window and how many of them found every node healthy - and the
/// one verdict drawn from the two: <see cref="HealthWindow.AllHealthy" />, the two counts being one number.
/// Nothing else: no moment of its own, no zone, no words of its own for the page - the words are the page's and
/// are pinned where they are said.
/// <para>
/// What is pinned where it belongs is walked past here on purpose: <see cref="AvailabilityTests" /> pins the
/// reading of the two windows out of a file - the counts in their places, the count that is not there, the count
/// that cannot be, the window that counted nothing - and the line, the parts and the seven percents the windows
/// are said in; <see cref="DeliveryReportEdgeCaseTests" /> pins the verdict while the two counts are the small
/// numbers the file gives - <c>24</c> and <c>24</c>, <c>1</c> and <c>1</c>, <c>0</c> and <c>0</c>, <c>168</c>
/// and <c>168</c> one number, and <c>24</c> and <c>23</c>, <c>1</c> and <c>0</c>, <c>0</c> and <c>1</c>,
/// <c>24</c> and <c>25</c> not - a window the file could not give counted and said as it stands, the words of a
/// count and a percent, and the refusal of what is not there; <see cref="ContainerAppReportsEdgeCaseTests" />
/// and <see cref="HourlyCheckTextEdgeCaseTests" /> pin the windows a container app holds in the words of the
/// box.
/// </para>
/// <para>
/// What follows is the record of the two numbers itself, at the ends the tests of the file leave open. First the
/// two counts by the names of their own properties: the constructor handing each number to the property of its
/// name and holding it exactly - the fixed numbers the page reads, the two numbers at the very ends of
/// <see cref="int" />, the negative numbers no file gives and the two numbers turned about, so that a healthy
/// count landing on the reports' place is seen rather than quietly matching. Then the verdict stepped to the
/// ends the file's small counts never reach: the two counts both at the bottom of <see cref="int" /> and both at
/// the top, apart by one at either end, and the negative counts no file gives. Then the two counts coming out
/// again in the order they went in, the window that is not there and the empty window that counted nothing, the
/// copy that turns one count while it keeps the other, the words the window says itself - its name, the two
/// counts and the verdict, the same for a reader of German as for one of the invariant culture - and the public
/// surface, so that the day a third number is added the list below says so.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: a window holds two numbers, calls
/// nothing at all, and every number below is one the test brought. The counts a window holds are numbers and not
/// words, so there is no empty word for them to refuse; the nulls are the window's own - the reference that is
/// no window at all, which the equality and the inequality each say what it is, and the empty window that
/// counted nothing, which the counts say is a window still.
/// </para>
/// </summary>
public class HealthWindowEdgeCaseTests
{
    // ------------------------------------------------ the two counts, by the names of their own properties

    /// <summary>The constructor's two numbers, position by position, come back as the property of the same name:
    /// the reports first and the healthy ones last - the order the page says them in, the healthy count beside
    /// the whole. The fixed numbers the file gives stand as they are, and the two numbers turned about are
    /// another window, not the same one read the other way.</summary>
    [Fact]
    public void TheConstructorHandsEachCountToThePropertyOfItsName()
    {
        Assert.Equal(24, new HealthWindow(24, 23).Reports);
        Assert.Equal(23, new HealthWindow(24, 23).Healthy);
        Assert.Equal(166, new HealthWindow(166, 164).Reports);
        Assert.Equal(164, new HealthWindow(166, 164).Healthy);
        Assert.Equal(24, new HealthWindow(24, 24).Reports);
        Assert.Equal(24, new HealthWindow(24, 24).Healthy);
        Assert.Equal(1, new HealthWindow(1, 1).Reports);
        Assert.Equal(0, new HealthWindow(0, 0).Reports);
        Assert.Equal(0, new HealthWindow(0, 0).Healthy);

        // The two numbers in the other places: 23 reports of which 24 were healthy, which no file gives and is
        // not the 24 of 23 read the other way.
        var turnedAbout = new HealthWindow(23, 24);

        Assert.Equal(23, turnedAbout.Reports);
        Assert.Equal(24, turnedAbout.Healthy);
        Assert.NotEqual(new HealthWindow(24, 23), turnedAbout);
    }

    /// <summary>Each number is held at either end of what a count may be - the two ends of <see cref="int" />,
    /// one end beside the other, a negative count no file gives and the one beside zero - and a count at one end
    /// is not the count at the other while the other number stands where it does.</summary>
    [Theory]
    [InlineData(int.MinValue, int.MinValue)]
    [InlineData(int.MinValue, int.MaxValue)]
    [InlineData(int.MaxValue, int.MinValue)]
    [InlineData(int.MaxValue, int.MaxValue)]
    [InlineData(-1, 0)]
    [InlineData(0, -1)]
    [InlineData(1, int.MaxValue)]
    [InlineData(int.MaxValue, 1)]
    public void EachCountIsHeldAtEitherEndOfACount(int reports, int healthy)
    {
        var window = new HealthWindow(reports, healthy);

        Assert.Equal(reports, window.Reports);
        Assert.Equal(healthy, window.Healthy);
    }

    // ------------------------------------------------------------------ the verdict, at the ends of a count

    /// <summary>The verdict is the two counts being one number, and it is said the same at the ends of
    /// <see cref="int" /> as at the small numbers the file gives: the two counts both at the very bottom and
    /// both at the very top are all healthy, apart by one at either end are not, and a negative count no file
    /// gives is all healthy only while both are the same negative number - the comparison being of the numbers
    /// and not of what a count may be, which is the file's question and pinned where the file is read.</summary>
    [Theory]
    [InlineData(int.MinValue, int.MinValue, true)]
    [InlineData(int.MinValue, int.MinValue + 1, false)]
    [InlineData(int.MaxValue, int.MaxValue, true)]
    [InlineData(int.MaxValue, int.MaxValue - 1, false)]
    [InlineData(-1, -1, true)]
    [InlineData(-1, 0, false)]
    [InlineData(0, -1, false)]
    [InlineData(2, 2, true)]
    [InlineData(168, 167, false)]
    public void TheVerdictIsTheTwoCountsBeingOneNumberAtTheEndsOfACount(int reports, int healthy, bool expected) =>
        Assert.Equal(expected, new HealthWindow(reports, healthy).AllHealthy);

    // ------------------------------------------------------------ the two counts coming out again

    /// <summary>The two counts come out of a window in the order they went in - the reports, then the healthy
    /// ones - whether the window is asked for them at once or one after the other, and what came out goes back
    /// in the same way: the window the two numbers fall apart into is the window they were. The numbers turned
    /// about fall apart turned about, and the empty window falls apart into nothing and is still a window.</summary>
    [Fact]
    public void TheTwoCountsComeOutOfAWindowInTheOrderTheyWentIn()
    {
        var (reports, healthy) = new HealthWindow(24, 23);

        Assert.Equal(24, reports);
        Assert.Equal(23, healthy);
        Assert.Equal(new HealthWindow(24, 23), new HealthWindow(reports, healthy));

        var week = new HealthWindow(166, 164);
        week.Deconstruct(out var reportsOfTheWeek, out var healthyOfTheWeek);

        Assert.Equal(166, reportsOfTheWeek);
        Assert.Equal(164, healthyOfTheWeek);

        var (turnedReports, turnedHealthy) = new HealthWindow(23, 24);

        Assert.Equal(23, turnedReports);
        Assert.Equal(24, turnedHealthy);

        var (nothing, none) = new HealthWindow(0, 0);

        Assert.Equal(0, nothing);
        Assert.Equal(0, none);

        var (bottom, top) = new HealthWindow(int.MinValue, int.MaxValue);

        Assert.Equal(int.MinValue, bottom);
        Assert.Equal(int.MaxValue, top);
    }

    // ------------------------------------------------------- the window, and the window that is not there

    /// <summary>A window is one with another while its two counts are one, whichever of the four ways of asking
    /// is used: the record's own <c>Equals</c>, the one of <see cref="object" />, <c>==</c> and <c>!=</c>. The
    /// hash follows the equality, boxed or unboxed and from one call to the next, and the numbers are compared
    /// rather than the references - two windows with the same two counts, made by two different calls, at the
    /// ends of <see cref="int" />, or rebuilt out of their own fallen-apart counts, are one window and one entry
    /// of a set.</summary>
    [Fact]
    public void TwoWindowsAreOneWhileTheirTwoCountsAreOne()
    {
        var one = new HealthWindow(24, 23);
        var two = new HealthWindow(24, 23);

        Assert.True(one.Equals(two));
        Assert.True(one.Equals(one));
        Assert.True(one.Equals((object)two));
        Assert.True(one == two);
        Assert.True(two == one);
        Assert.False(one != two);
        Assert.Equal(one, two);
        Assert.NotSame(one, two);
        Assert.Equal(one.GetHashCode(), two.GetHashCode());
        Assert.Equal(one.GetHashCode(), ((object)one).GetHashCode());
        Assert.Equal(one.GetHashCode(), one.GetHashCode());
        Assert.Single(new HashSet<HealthWindow> { one, two });

        // The two counts asked out and put back in make the same window again: the equality is of the numbers,
        // not of the call that gathered them.
        one.Deconstruct(out var reports, out var healthy);

        Assert.Equal(one, new HealthWindow(reports, healthy));
        Assert.Equal(one.GetHashCode(), new HealthWindow(reports, healthy).GetHashCode());

        // The ends of the count are compared as counts too, and a set of them holds them once.
        var top = new HealthWindow(int.MaxValue, int.MaxValue);

        Assert.True(top == new HealthWindow(int.MaxValue, int.MaxValue));
        Assert.Equal(top.GetHashCode(), new HealthWindow(int.MaxValue, int.MaxValue).GetHashCode());
        Assert.Single(new HashSet<HealthWindow> { top, new HealthWindow(int.MaxValue, int.MaxValue) });
    }

    /// <summary>A difference in either count is another window, and so are the two counts told in another
    /// order: one healthy report fewer, one report more, the two counts exchanged, a count no file gives - the
    /// comparison is of the two numbers one after the other, so a window a count apart is another window
    /// whatever the two are, and the four of them stand apart in a set.</summary>
    [Fact]
    public void ACountTurnedOrTurnedAboutIsAnotherWindow()
    {
        var one = new HealthWindow(24, 23);

        Assert.NotEqual(one, new HealthWindow(24, 22));
        Assert.NotEqual(one, new HealthWindow(24, 24));
        Assert.NotEqual(one, new HealthWindow(25, 23));
        Assert.NotEqual(one, new HealthWindow(23, 24));
        Assert.NotEqual(one, new HealthWindow(0, 23));
        Assert.NotEqual(one, new HealthWindow(24, 0));
        Assert.NotEqual(one, new HealthWindow(-1, 23));
        Assert.NotEqual(one, new HealthWindow(24, -1));

        Assert.True(one != new HealthWindow(24, 24));
        Assert.False(one == new HealthWindow(24, 24));
        Assert.False(one.Equals(new HealthWindow(24, 22)));
        Assert.False(one.Equals((object)new HealthWindow(23, 24)));

        // The windows that differ from the one by a count or by its order are four apart.
        Assert.Equal(
            4,
            new HashSet<HealthWindow>
            {
                one,
                one with { Healthy = 24 },
                one with { Reports = 23 },
                new HealthWindow(23, 24),
            }.Count);
    }

    /// <summary>The nothing on either side of the members every record is asked with: two windows that are no
    /// window at all are one, a window and no window are two - the equality and the inequality each saying it,
    /// from either side - and the window asked about itself says no to the nothing, to the count it was made of
    /// and to the report that holds it. The window that counted nothing is still a window: the two of them that
    /// both counted nothing are one and one entry of a set, and neither is the window that counted one report,
    /// the one that found a healthy one of nothing, the one that counted one of one, or the one with a negative
    /// count no file gives.</summary>
    [Fact]
    public void NoWindowIsOneWithNoWindowAndTheEmptyWindowIsAWindow()
    {
        HealthWindow? nothing = null;
        HealthWindow? alsoNothing = null;
        var there = new HealthWindow(24, 23);

        Assert.Null(default(HealthWindow));
        Assert.True(nothing == alsoNothing);
        Assert.False(nothing != alsoNothing);
        Assert.False(nothing == there);
        Assert.True(nothing != there);
        Assert.False(there == nothing);
        Assert.True(there != nothing);
        Assert.False(there.Equals(nothing));
        Assert.True(there.Equals(there));

        Assert.False(there.Equals(null));
        Assert.False(there.Equals((HealthWindow?)null));
        Assert.False(there.Equals((object?)null));
        Assert.False(there.Equals(24));
        Assert.False(there.Equals("23 of 24"));
        Assert.False(there.Equals(new HealthReports(there, there, null)));

        var emptyOne = new HealthWindow(0, 0);
        var emptyTwo = new HealthWindow(0, 0);

        Assert.Equal(emptyOne, emptyTwo);
        Assert.NotSame(emptyOne, emptyTwo);
        Assert.Equal(emptyOne.GetHashCode(), emptyTwo.GetHashCode());
        Assert.Single(new HashSet<HealthWindow> { emptyOne, emptyTwo });

        // Counting nothing is not counting one report, nor finding one healthy of nothing.
        Assert.NotEqual(emptyOne, new HealthWindow(1, 0));
        Assert.NotEqual(emptyOne, new HealthWindow(0, 1));
        Assert.NotEqual(emptyOne, new HealthWindow(1, 1));
        Assert.NotEqual(emptyOne, new HealthWindow(0, -1));
    }

    // ------------------------------------------------------------------ the copy, and the verdict it turns

    /// <summary>Either count may be turned as the window is made, and either may be turned in a copy: the copy
    /// that names nothing is the window it started from in another reference, the copy that names one count
    /// keeps the other exactly as it stood, the original is left where it stood, and a count turned onto the
    /// other turns the verdict with it - the window that was not all healthy becoming all healthy while its
    /// reports stand, and the window the file could not give becoming the one it refuses to read.</summary>
    [Fact]
    public void ACopyTurnsTheCountItNamesAndKeepsTheOther()
    {
        var original = new HealthWindow(24, 23);

        var unchanged = original with { };

        Assert.Equal(original, unchanged);
        Assert.NotSame(original, unchanged);
        Assert.False(unchanged.AllHealthy);

        var all = original with { Healthy = 24 };

        Assert.Equal(24, all.Reports);
        Assert.Equal(24, all.Healthy);
        Assert.True(all.AllHealthy);
        Assert.Equal(24, original.Reports);
        Assert.Equal(23, original.Healthy);
        Assert.False(original.AllHealthy);
        Assert.NotEqual(original, all);
        Assert.NotSame(original, all);

        var fewerReports = original with { Reports = 1 };

        Assert.Equal(1, fewerReports.Reports);
        Assert.Equal(23, fewerReports.Healthy);
        Assert.False(fewerReports.AllHealthy);

        var bothTurned = new HealthWindow(24, 24) with { Reports = 0, Healthy = 0 };

        Assert.Equal(0, bothTurned.Reports);
        Assert.Equal(0, bothTurned.Healthy);
        Assert.True(bothTurned.AllHealthy);

        // The window the file could not give is turned into the one it would have read.
        var impossible = new HealthWindow(24, 25) with { Healthy = 24 };

        Assert.True(impossible.AllHealthy);
        Assert.Equal(new HealthWindow(24, 24), impossible);
    }

    // ------------------------------------------------------------ the words the window says itself

    /// <summary>The words the window says its two counts in: its own name at the front, the reports first and
    /// the healthy ones last as they are named, and the verdict the window draws from them at the end - each
    /// number said as it stands, the empty window said as its zeros, the negative counts said with their sign,
    /// and the count that carries no thousands separator. These are the record's words and not the page's: the
    /// page says <c>23 of 24</c> and a percent, and the window says the numbers it holds.</summary>
    [Fact]
    public void TheWindowSaysItsTwoCountsAndItsVerdict()
    {
        Assert.Equal("HealthWindow { Reports = 24, Healthy = 23, AllHealthy = False }", Words(new HealthWindow(24, 23)));
        Assert.Equal("HealthWindow { Reports = 24, Healthy = 24, AllHealthy = True }", Words(new HealthWindow(24, 24)));
        Assert.Equal("HealthWindow { Reports = 0, Healthy = 0, AllHealthy = True }", Words(new HealthWindow(0, 0)));
        Assert.Equal("HealthWindow { Reports = 1, Healthy = 0, AllHealthy = False }", Words(new HealthWindow(1, 0)));
        Assert.Equal("HealthWindow { Reports = 0, Healthy = 1, AllHealthy = False }", Words(new HealthWindow(0, 1)));
        Assert.Equal("HealthWindow { Reports = 23, Healthy = 24, AllHealthy = False }", Words(new HealthWindow(23, 24)));
        Assert.Equal("HealthWindow { Reports = -1, Healthy = 0, AllHealthy = False }", Words(new HealthWindow(-1, 0)));
        Assert.Equal("HealthWindow { Reports = -1, Healthy = -1, AllHealthy = True }", Words(new HealthWindow(-1, -1)));
        Assert.Equal("HealthWindow { Reports = 1000000, Healthy = 999, AllHealthy = False }", Words(new HealthWindow(1_000_000, 999)));
    }

    /// <summary>The window's words are numbers and a verdict, so no reader's culture turns them: a reader of
    /// German is told the same words as the reader of the invariant culture - the numbers with no thousands
    /// separator and the verdict in its own letters - which is what the page may count on when it says a window
    /// it holds rather than the words it draws from one.</summary>
    [Fact]
    public void TheWindowsOwnWordsAreTheSameWhateverTheReaderSpeaks()
    {
        var window = new HealthWindow(1_000_000, 999);

        ViewerCulture.German(() => Assert.Equal("HealthWindow { Reports = 1000000, Healthy = 999, AllHealthy = False }", window.ToString()));
        ViewerCulture.German(() => Assert.Equal("HealthWindow { Reports = 24, Healthy = 23, AllHealthy = False }", new HealthWindow(24, 23).ToString()));
        ViewerCulture.German(() => Assert.Equal("HealthWindow { Reports = 24, Healthy = 24, AllHealthy = True }", new HealthWindow(24, 24).ToString()));

        // Under the reader the test happens to run as, the words are the words the invariant reader gets.
        Assert.Equal(Words(window), window.ToString());
        Assert.Equal(Words(new HealthWindow(24, 24)), new HealthWindow(24, 24).ToString());
    }

    // ------------------------------------------------------------------------ the record's own shape

    /// <summary>The window's whole public surface, as it stands: the two counts named in the order the
    /// constructor names them - the reports, then the healthy ones - the verdict drawn from them, and the
    /// members every record is asked, falling apart, comparing, hashing and saying itself, and nothing else to
    /// call. The verdict is read and not written - a window that could be told to be all healthy would be a
    /// verdict no count had drawn - while the two counts are the places a <c>with</c> may turn. A third number
    /// added to the record has to be written here as well, which is the point: the page cannot be shown a count
    /// nobody decided what it means.</summary>
    [Fact]
    public void TheWindowHasTwoCountsAVerdictAndTheMembersEveryRecordIsAsked()
    {
        Assert.Equal(
            ["Reports", "Healthy", "AllHealthy"],
            [.. typeof(HealthWindow)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Select(property => property.Name)]);

        Assert.Equal(
            ["AllHealthy", "Deconstruct", "Equals", "GetHashCode", "Healthy", "Reports", "ToString", "op_Equality", "op_Inequality"],
            PublicSurface());

        var properties = typeof(HealthWindow)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .ToDictionary(property => property.Name, StringComparer.Ordinal);

        Assert.Equal(typeof(int), properties["Reports"].PropertyType);
        Assert.Equal(typeof(int), properties["Healthy"].PropertyType);
        Assert.Equal(typeof(bool), properties["AllHealthy"].PropertyType);

        Assert.True(properties["Reports"].CanRead);
        Assert.True(properties["Reports"].CanWrite);
        Assert.True(properties["Healthy"].CanRead);
        Assert.True(properties["Healthy"].CanWrite);
        Assert.True(properties["AllHealthy"].CanRead);
        Assert.False(properties["AllHealthy"].CanWrite);

        // Two numbers, and the one constructor that takes them in that order.
        var constructors = typeof(HealthWindow).GetConstructors();

        Assert.Single(constructors);
        var parameters = constructors[0].GetParameters();

        Assert.Equal(["Reports", "Healthy"], [.. parameters.Select(parameter => parameter.Name!)]);
        Assert.Equal(typeof(int), parameters[0].ParameterType);
        Assert.Equal(typeof(int), parameters[1].ParameterType);
    }

    // ---------------------------------------------------------------------------------- the machinery

    /// <summary>The public members the window declares itself: its properties and the methods that are not the
    /// get and the set of one - the two operators kept although the compiler names them specially, since they
    /// are the <c>==</c> and the <c>!=</c> a caller writes - with the record's compiler-named clone walked past.
    /// The constructor is no method, so it is named by the tests above rather than by this list.</summary>
    private static List<string> PublicSurface() =>
        [.. typeof(HealthWindow)
            .GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(member => member is PropertyInfo
                             || member is MethodInfo method
                             && (!method.IsSpecialName || method.Name.StartsWith("op_", StringComparison.Ordinal)))
            .Where(member => !member.Name.Contains('<', StringComparison.Ordinal))
            .Select(member => member.Name)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];

    /// <summary>The words a window says its counts in, taken in the invariant culture whatever the reader of
    /// this test happens to be: the words are the numbers the window holds, and the culture is only the way to
    /// say them once.</summary>
    private static string Words(HealthWindow window) => ViewerCulture.Invariant(() => window.ToString()!);
}
