using System.Globalization;
using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The nothing-word tests of <see cref="DashboardInfo" />, the dashboard itself as a deployable of the system: the
/// two words it is made of and the one fixed word it carries, asked of a dashboard whose words are both nothing.
/// <para>
/// What the other files already pin is left alone. <see cref="TopologyEdgeCaseTests" /> pins the rest of the record:
/// that <see cref="DashboardInfo.DefaultName" /> is the word <c>dashboard</c> - of letters, of no capitals and of no
/// spaces about it -; that the two parts come back as they are handed, the empty word, the word with its spaces and
/// the nothing handed to a part the record says is there all standing; that two dashboards made of words are one
/// while their two parts are and differ in either, that they fall apart into the two words, copy one word at a time
/// and say themselves with the two parts in their order; and the whole public surface of the record - its two parts,
/// the members every record is asked and the one fixed word. <see cref="TopologyParserTests" /> pins what the file
/// becomes, including the dashboard the reader of <c>topology.json</c> writes and the default name it hands a
/// dashboard that names no name. <see cref="DashboardBuildTests" /> pins the dashboard the file names, the file that
/// names no build facts and so no dashboard at all, and the monitor reading the page's own build facts from the path
/// the record holds; <see cref="DeployableFactsTests" /> pins the Code card the dashboard shows of itself; and
/// <see cref="SystemInfoEdgeCaseTests" /> pins the dashboard part of a system weighed as its two words and not as the
/// object they stand in.
/// </para>
/// <para>
/// What is pinned here is the one shape of the record the other files walk past: a dashboard whose two words are
/// both nothing, and two of them. Both parts are declared non-nullable <see cref="string" />, so such a dashboard is
/// one a caller can make by handing two nothings over; and the record fills nothing in and repairs nothing - a
/// nothing word stands as it was handed, is weighed as the nothing it is, and is said in the line the record says
/// itself as the nothing it is, at the edges of length and of whitespace too.
/// </para>
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: a dashboard holds two words and decides nothing,
/// and the build path is named here and never fetched.
/// </para>
/// </summary>
public class DashboardInfoEdgeCaseTests
{
    /// <summary>
    /// Two dashboards handed nothing are one dashboard. <see cref="TopologyEdgeCaseTests" /> hands a null
    /// <see cref="DashboardInfo.Name" /> and a null <see cref="DashboardInfo.BuildPath" /> to one dashboard and reads
    /// its two parts back, but never asks whether two such dashboards are one dashboard. Both words are declared
    /// non-nullable <see cref="string" />, so <c>new DashboardInfo(null!, null!)</c> is a dashboard a caller can make
    /// and keep: the record fills nothing in and repairs nothing - no default name steps in where a name was left
    /// out, and no path is made of nothing - so a nothing word stands as it was handed. Two of them are then one by
    /// <c>==</c>, by <c>Equals</c> and by <c>!=</c> being false, and their hash codes are equal: the nothing a
    /// dashboard holds weighs the same in either hand. Falling one apart reads two nothings back, the two words as
    /// they were given. And a nothing word is a word of its own and not the absent word: a dashboard of one nothing
    /// word and one real word is a second dashboard from the all-nothing one, whether the real word is the path the
    /// nothing dashboard lacks or the name the record carries as its fixed word.
    /// </summary>
    [Fact]
    public void TwoDashboardsHandedNothingAreOneDashboard()
    {
        var one = new DashboardInfo(null!, null!);
        var two = new DashboardInfo(null!, null!);

        Assert.True(one == two);
        Assert.False(one != two);
        Assert.True(one.Equals(two));
        Assert.Equal(one.GetHashCode(), two.GetHashCode());

        var (name, buildPath) = one;

        Assert.Null(name);
        Assert.Null(buildPath);

        Assert.False(new DashboardInfo(null!, "/x") == new DashboardInfo(null!, null!));
        Assert.False(new DashboardInfo(null!, "/x") == new DashboardInfo("dashboard", "/x"));
    }

    /// <summary>
    /// The two operators a caller writes - <c>==</c> and <c>!=</c> - asked with the nothing on either side of a
    /// dashboard. <see cref="TopologyEdgeCaseTests" /> pins <see cref="DashboardInfo.Equals" /> against a record of
    /// another type and the two operators between two dashboards, but it never puts the nothing itself on either
    /// side of <c>==</c> or <c>!=</c>, and never asks <see cref="DashboardInfo.Equals" /> with a boxed dashboard on
    /// the other side. Here they are pinned as
    /// <see cref="SystemInfoEdgeCaseTests.TheTwoOperatorsAnswerForTheNothingOnEitherSide" /> pins them of a system:
    /// a dashboard and the nothing are two - <c>==</c> saying false and <c>!=</c> saying true, with the nothing last
    /// as with the nothing first - and <c>Equals</c> answers the nothing false as the operators do. The all-nothing
    /// dashboard of <see cref="TwoDashboardsHandedNothingAreOneDashboard" /> is a dashboard of its own and not the
    /// nothing it might be mistaken for: it is not <c>==</c> to the nothing. A dashboard is one with itself - the
    /// very same reference held under a second name, by <c>==</c>, by <c>Equals</c> and by the hash code it gives,
    /// and one with a second dashboard of its two words handed over boxed as <see cref="object" />, the record
    /// opening the box and weighing the two words; and it is not one with a plain <see cref="object" />, which
    /// is no dashboard at all.
    /// </summary>
    [Fact]
    public void TheTwoOperatorsAnswerForTheNothingOnEitherSide()
    {
        var one = new DashboardInfo(DashboardInfo.DefaultName, "/build-facts.json");

        Assert.False(one == null);
        Assert.False(null == one);
        Assert.True(one != null);
        Assert.True(null != one);
        Assert.False(one.Equals(null));

        var nothing = new DashboardInfo(null!, null!);

        Assert.False(nothing == null);

        var itself = one;

        Assert.True(one == itself);
        Assert.True(one.Equals(one));
        Assert.Equal(one.GetHashCode(), one.GetHashCode());

        var two = new DashboardInfo(DashboardInfo.DefaultName, "/build-facts.json");

        Assert.True(one.Equals((object)two));
        Assert.False(one.Equals(new object()));
    }

    /// <summary>
    /// The hash of one dashboard, asked again and again: <see cref="TopologyEdgeCaseTests" /> pins that two equal
    /// dashboards hash alike, and it never asks of one dashboard that its hash stands still, nor does it ever hash a
    /// dashboard handed nothing - which is all this asks. A named dashboard's
    /// <see cref="DashboardInfo.GetHashCode()" /> says the same number from call to call, the boxed value -
    /// <c>((object)dashboard).GetHashCode()</c> - is that
    /// same number, a copy the record makes with <c>with { }</c> and a dashboard rebuilt from the same two words hash
    /// alike with it, and the all-nothing dashboard of
    /// <see cref="TwoDashboardsHandedNothingAreOneDashboard" /> - <c>new DashboardInfo(null!, null!)</c> - has a hash
    /// that stands still, matches a second such dashboard and asks nothing that throws: nothing about the two words,
    /// absent or present, makes hashing fall over. And the two words really go into the number: a dashboard whose
    /// <see cref="DashboardInfo.Name" /> differs only in its capitals - <c>DASHBOARD</c> where the other says
    /// <c>dashboard</c> - hashes apart from the named one. The number itself is not the point; that it stands still
    /// is, because a caller puts dashboards in a <see cref="HashSet{T}" /> - which is the next test.
    /// </summary>
    [Fact]
    public void TheHashOfADashboardStandsStillFromCallToCall()
    {
        var name = DashboardInfo.DefaultName;
        var buildPath = "/build-facts.json";
        var dashboard = new DashboardInfo(name, buildPath);

        var hash = dashboard.GetHashCode();

        Assert.Equal(hash, dashboard.GetHashCode());
        Assert.Equal(hash, ((object)dashboard).GetHashCode());
        Assert.Equal(hash, (dashboard with { }).GetHashCode());
        Assert.Equal(hash, new DashboardInfo(name, buildPath).GetHashCode());

        var nothing = new DashboardInfo(null!, null!);
        var alsoNothing = new DashboardInfo(null!, null!);

        var nothingHash = nothing.GetHashCode();

        Assert.Equal(nothingHash, nothing.GetHashCode());
        Assert.Equal(nothingHash, alsoNothing.GetHashCode());
        Assert.Null(Record.Exception(() => nothing.GetHashCode()));

        Assert.NotEqual(hash, new DashboardInfo("DASHBOARD", buildPath).GetHashCode());
    }

    /// <summary>
    /// The two words of a dashboard are weighed as words, character against character, and never as a reader of some
    /// other language reads them. Every dashboard <see cref="TheHashOfADashboardStandsStillFromCallToCall" /> and
    /// <see cref="ADashboardIsOneKeyOfASetAndOneKeyOfADictionary" /> weighs is weighed by whatever reader the test
    /// process happens to bring; neither asks a reader whose alphabet turns letters about. Here one does, a Turkish
    /// reader, for which the dotted capital <c>İ</c> is the capital of the plain <c>i</c> and the dotless <c>ı</c>
    /// the lower case of the plain <c>I</c> - so that reader, following its own rules, says the two names below are
    /// one name spelled twice.
    /// <para>
    /// The record asks no such rules, and the two dashboards stay two under that reader: <c>==</c> says false,
    /// <c>!=</c> says true, <see cref="DashboardInfo.Equals" /> says false as the operators do, and the two hash
    /// apart - so a set of them holds two entries and neither spelling is ever forgiven the other. The dashboard the
    /// record names for itself, <see cref="DashboardInfo.DefaultName" /> beside <c>/build-facts.json</c>, is by
    /// contrast one whether it was made under that reader or under any other - one by <c>==</c>, by
    /// <see cref="DashboardInfo.Equals" /> and by the one number both of them hash to - and a dashboard says itself
    /// the same words under the Turkish reader as it does under the invariant one.
    /// </para>
    /// <para>
    /// The two halves of the product rest on this agreeing: a topology is written by a workflow on one machine and
    /// the page that shows it is rendered on another whose reader speaks another language, and the two must agree on
    /// the same two words - the same name standing for one dashboard in the workflow's file and in that browser too,
    /// with the same address beside it.
    /// </para>
    /// </summary>
    [Fact]
    public void TheTwoWordsAreWeighedAsWordsAndNotAsACultureWeighsThem()
    {
        // Two dashboards made before any reader is asked anything: the dashboard the record names for itself, with
        // the hash it gives, and the dashboard named of the plain <c>i</c>.
        var named = new DashboardInfo(DashboardInfo.DefaultName, "/build-facts.json");
        var namedHash = named.GetHashCode();
        var dotted = new DashboardInfo("i", "/b");

        string namedSaidAsTurkish;
        string dottedSaidAsTurkish;

        var reader = Thread.CurrentThread.CurrentCulture;
        try
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("tr-TR");

            // The dotted capital <c>İ</c>, which that reader's own alphabet calls the capital of the plain <c>i</c>
            // above - the two words that reader would say are one word spelled twice. The record weighs letters and
            // not the reader, so the two dashboards stay two, and they hash apart: a set of them holds both.
            var dottedCapital = new DashboardInfo("İ", "/b");

            Assert.False(dotted == dottedCapital);
            Assert.True(dotted != dottedCapital);
            Assert.False(dotted.Equals(dottedCapital));
            Assert.NotEqual(dotted.GetHashCode(), dottedCapital.GetHashCode());
            Assert.Equal(2, new HashSet<DashboardInfo> { dotted, dottedCapital }.Count);

            // The named dashboard is one dashboard whichever reader made it, and the reader turns no letter of it
            // and no digit of its hash.
            var alsoNamed = new DashboardInfo(DashboardInfo.DefaultName, "/build-facts.json");

            Assert.True(named == alsoNamed);
            Assert.False(named != alsoNamed);
            Assert.True(named.Equals(alsoNamed));
            Assert.Equal(namedHash, alsoNamed.GetHashCode());
            Assert.Single(new HashSet<DashboardInfo> { named, alsoNamed });

            namedSaidAsTurkish = named.ToString();
            dottedSaidAsTurkish = dotted.ToString();
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = reader;
        }

        string namedSaidAsInvariant;
        string dottedSaidAsInvariant;

        var invariantReader = Thread.CurrentThread.CurrentCulture;
        try
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            namedSaidAsInvariant = named.ToString();
            dottedSaidAsInvariant = dotted.ToString();
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = invariantReader;
        }

        // The words a dashboard says itself are the two words it is made of, and no reader says them differently.
        Assert.Equal(namedSaidAsInvariant, namedSaidAsTurkish);
        Assert.Equal(dottedSaidAsInvariant, dottedSaidAsTurkish);
    }

    /// <summary>
    /// The two members a caller really asks of a dashboard - <see cref="DashboardInfo.Equals" /> and
    /// <see cref="DashboardInfo.GetHashCode" /> - asked together, the way a set and a dictionary ask them, as
    /// <see cref="SystemInfoEdgeCaseTests.ASystemIsOneKeyOfASetAndOneKeyOfADictionary" /> asks them of a system: a
    /// named dashboard, a second dashboard of the same two words and a <c>with { }</c> copy of the first are one
    /// entry of a <see cref="HashSet{T}" />, which the set says of each of the three; a
    /// <see cref="Dictionary{TKey,TValue}" /> written with the named dashboard answers its value when it is asked
    /// with the second dashboard of the same two words; and the all-nothing dashboard of
    /// <see cref="TwoDashboardsHandedNothingAreOneDashboard" /> is a usable key that collapses with a second such
    /// dashboard. A copy that tells either of the two words apart stands one further entry apart from the three that
    /// collapse - the name in another case, the name empty, the name nothing, the
    /// <see cref="DashboardInfo.BuildPath" /> without its leading slash and the
    /// <see cref="DashboardInfo.BuildPath" /> nothing - so the set weighs both words, in their case and with their
    /// slashes, and grows to six. The part-by-part inequality itself is pinned by
    /// <see cref="TopologyEdgeCaseTests" /> and by <see cref="TwoDashboardsHandedNothingAreOneDashboard" />, and the
    /// hash standing still from call to call by <see cref="TheHashOfADashboardStandsStillFromCallToCall" />.
    /// </summary>
    [Fact]
    public void ADashboardIsOneKeyOfASetAndOneKeyOfADictionary()
    {
        var name = DashboardInfo.DefaultName;
        var buildPath = "/build-facts.json";
        var dashboard = new DashboardInfo(name, buildPath);
        var second = new DashboardInfo(name, buildPath);
        var copy = dashboard with { };

        // Two dashboards of the same two words and a copy the record makes of itself are one entry of a set, and the
        // set answers true of each of the three.
        var set = new HashSet<DashboardInfo> { dashboard, second, copy };

        Assert.Single(set);
        Assert.Contains(dashboard, set);
        Assert.Contains(second, set);
        Assert.Contains(copy, set);

        // A copy that tells one of the two words apart is another dashboard to the set: each of the five variants
        // grows the set by one, the three that were one still being one - six in all.
        Assert.True(set.Add(new DashboardInfo("DASHBOARD", buildPath)));
        Assert.True(set.Add(new DashboardInfo("", buildPath)));
        Assert.True(set.Add(new DashboardInfo(null!, buildPath)));
        Assert.True(set.Add(new DashboardInfo(name, "build-facts.json")));
        Assert.True(set.Add(new DashboardInfo(name, null!)));

        Assert.Equal(6, set.Count);

        // A dictionary written with one dashboard answers the value when it is asked with an equal copy built from
        // the same two words.
        var keyed = new Dictionary<DashboardInfo, string> { [dashboard] = "the dashboard" };

        Assert.Single(keyed);
        Assert.Equal("the dashboard", keyed[second]);

        // The all-nothing dashboard is a usable key, and it collapses with a second such dashboard.
        var nothing = new DashboardInfo(null!, null!);
        var alsoNothing = new DashboardInfo(null!, null!);

        Assert.Single(new HashSet<DashboardInfo> { nothing, alsoNothing });
    }

    /// <summary>
    /// A copy takes the whole pair of words with it, the nothing included: the one word it is named for changes, and
    /// the other word comes over exactly as it stood - even when the other word is nothing.
    /// <see cref="TopologyEdgeCaseTests" /> copies a dashboard that has two words and names one of them -
    /// <c>with { Name = ... }</c> and <c>with { BuildPath = ... }</c> - but never takes a copy that names nothing
    /// (<c>with { }</c>) and never copies a dashboard handed nothing. Both are asked here, the copy being made of the
    /// all-nothing dashboard of <see cref="TwoDashboardsHandedNothingAreOneDashboard" />. A <c>with { }</c> copy of a
    /// named dashboard is that dashboard - one by <c>==</c>, by <see cref="DashboardInfo.Equals" /> and by the hash
    /// both give - and yet not the same object in a second hand (<c>Assert.NotSame</c>): a copy is a dashboard made
    /// anew out of the same two words.
    /// A copy of the all-nothing dashboard that names the name gives a dashboard of that name whose
    /// <see cref="DashboardInfo.BuildPath" /> is still nothing, the nothing of the part not named standing as it
    /// stood; and the dashboard copied is untouched by it - its two words still both nothing and itself still one
    /// with a second all-nothing dashboard, since a copy turns nothing of what it was copied from. Named the other
    /// way, the same holds: <c>with { BuildPath = "/facts.json" }</c> of the all-nothing dashboard leaves its
    /// <see cref="DashboardInfo.Name" /> nothing.</summary>
    [Fact]
    public void ACopyNamesTheWordItChangesAndKeepsTheOtherEvenAtTheNothing()
    {
        var dashboard = new DashboardInfo(DashboardInfo.DefaultName, "/build-facts.json");

        // A copy that names nothing is the very same two words - by ==, by Equals and by the hash - and yet a
        // dashboard of its own, not the same object held under a second name.
        var copy = dashboard with { };

        Assert.True(copy == dashboard);
        Assert.True(copy.Equals(dashboard));
        Assert.Equal(dashboard.GetHashCode(), copy.GetHashCode());
        Assert.NotSame(dashboard, copy);

        var nothing = new DashboardInfo(null!, null!);
        var alsoNothing = new DashboardInfo(null!, null!);

        // The copy that names the name keeps the path as it was: nothing, and standing where it stood.
        var named = nothing with { Name = "dashboard" };

        Assert.Equal("dashboard", named.Name);
        Assert.Null(named.BuildPath);
        Assert.Null(nothing.Name);
        Assert.Null(nothing.BuildPath);
        Assert.True(nothing == alsoNothing);

        // The copy that names the path keeps the name as it was: nothing.
        var pathed = nothing with { BuildPath = "/facts.json" };

        Assert.Equal("/facts.json", pathed.BuildPath);
        Assert.Null(pathed.Name);
    }

    /// <summary>
    /// The shape of the record itself, asked of the running assembly with <see cref="System.Reflection" />: that the
    /// dashboard is sealed, that its two words are written only as it is made, and that its one fixed word is a
    /// literal - the three things a caller cannot see of a dashboard from the words it hands over.
    /// <para>
    /// <see cref="TopologyEdgeCaseTests" /> pins the names and the types of the two parts, the parameter names, the
    /// defaults and the nullability of the constructor, the interfaces the record carries (the one
    /// <see cref="IEquatable{T}" /> of itself) and that the raw constant value of
    /// <see cref="DashboardInfo.DefaultName" /> is <c>dashboard</c>; it never asks the shape, and nothing here
    /// repeats what it pins.
    /// </para>
    /// <para>
    /// The type is the sealed record of its two words, of <see cref="object" /> and nothing else to inherit - no
    /// second kind of dashboard stands under it, and no caller inherits its equality away. The way in is one -
    /// <c>GetConstructors()</c> holds exactly one constructor - and it asks both words, there being no parameterless
    /// constructor (<c>GetConstructor(Type.EmptyTypes)</c>) to leave them out: a dashboard cannot be made of one word
    /// or of none. Each of the two words may be read, and may be written only as the dashboard is made or copied -
    /// <see cref="DashboardInfo.Name" /> and <see cref="DashboardInfo.BuildPath" /> can each be read and written, and
    /// each has a public set accessor (<c>GetSetMethod()</c>) carrying the <c>IsExternalInit</c> custom modifier, the
    /// mark of the <c>init</c> accessor - so a word is written only as the dashboard is made or by <c>with</c>, and
    /// never turned after. The one fixed word is a literal of the type - public, static,
    /// <see cref="FieldInfo.IsLiteral" /> - which is what lets a caller write it as a <c>const</c> of its own, as the
    /// local below does, rather than hold it as a field that could be found holding another telling; and it is no
    /// part of a dashboard - no instance field of the record - standing alone as the one public static field the type
    /// has.
    /// </para>
    /// </summary>
    [Fact]
    public void TheDashboardIsSealedItsTwoWordsAreWrittenOnlyAsItIsMadeAndItsOneFixedWordIsALiteral()
    {
        var type = typeof(DashboardInfo);

        // The dashboard is the sealed record of its two words, with object and nothing else to inherit.
        Assert.True(type.IsSealed);
        Assert.Equal(typeof(object), type.BaseType);

        // The way in is one, and it asks both words: no dashboard is made of one word or of none, since there is no
        // way in that leaves the two of them out.
        var waysIn = type.GetConstructors();

        Assert.Single(waysIn);
        Assert.Null(type.GetConstructor(Type.EmptyTypes));

        // The two words may be read, and may be written only as the dashboard is made or copied: each set accessor
        // is public and carries the mark of the init accessor, so neither word is turned after the making.
        var properties = type
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .ToDictionary(property => property.Name, StringComparer.Ordinal);

        Assert.All(
            ["Name", "BuildPath"],
            name =>
            {
                var part = properties[name];

                Assert.True(part.CanRead);
                Assert.True(part.CanWrite);
                Assert.NotNull(part.GetSetMethod());
                Assert.Contains(
                    typeof(System.Runtime.CompilerServices.IsExternalInit),
                    part.SetMethod!.ReturnParameter.GetRequiredCustomModifiers());
            });

        // The one fixed word is a constant of the type and not a field that could hold another telling: public,
        // static and literal - which is what lets the caller's const local below compile at all.
        var fixedWord = type.GetField("DefaultName", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)!;

        Assert.True(fixedWord.IsPublic);
        Assert.True(fixedWord.IsStatic);
        Assert.True(fixedWord.IsLiteral);

        const string alias = DashboardInfo.DefaultName;

        Assert.Equal("dashboard", alias);

        // And the fixed word is no part of a dashboard: no public field of an instance carries it, and it stands
        // alone as the one public static field the record has.
        Assert.Empty(PublicFieldNames(BindingFlags.Instance));
        Assert.Equal(["DefaultName"], PublicFieldNames(BindingFlags.Static));
    }

    /// <summary>One row per pair of words the dashboard is made of, each word written out on its own by
    /// <see cref="Own" /> so that no two of them stand in the same interned string and the test can tell the holding
    /// of a word from a copying of it.</summary>
    public static TheoryData<string, string> WordsHeldAsGiven() => new()
    {
        { Own(new string('x', 512)), Own("/build-facts.json") },
        { Own(DashboardInfo.DefaultName), Own("/" + new string('y', 4_000)) },
        { Own("  \t\n "), Own("/") },
        { Own("/"), Own("/") },
        { Own(DashboardInfo.DefaultName), Own("café/ü/\\x?q=1#f") },
    };

    /// <summary>
    /// A word comes back out of a dashboard as the very string it was handed, long words and awkward ones included.
    /// <see cref="TopologyEdgeCaseTests" /> pins that the two parts come back as they are given - the empty word, the
    /// word with its spaces about it and the nothing handed to a part the record says is there - but the edges it
    /// never visits are the length of a word, whitespace inside one, letters outside the ASCII, and that the record
    /// keeps the very string it was handed rather than a copy of it. That last question can only be asked of a word
    /// written on its own: two callers writing <c>"/build-facts.json"</c> hold one and the same interned string, so
    /// every word here is made by <see cref="Own" />, out of characters of its own. A name of 512 <c>x</c>, a path of
    /// 4001 letters, a name of spaces with a tab and a newline between them which nothing trims, the single letter
    /// <c>/</c> handed to both parts at once and a path of letters outside the ASCII carrying a backslash, a query and
    /// a fragment are each read back whole - the same letters (<see cref="Assert.Equal(string?, string?)" />) in the
    /// same string (<see cref="Assert.Same" />), neither part copied, trimmed, shortened nor turned. What a build path
    /// means belongs to the caller that fetches it: the record asks nothing of a path - no slash, no escaping, no
    /// length - while the reader of the file gives a path its leading slash and the page then reads its own build
    /// facts out of that address (<see cref="DashboardBuildTests" />).
    /// </summary>
    [Theory]
    [MemberData(nameof(WordsHeldAsGiven))]
    public void AWordIsHeldAsItIsGivenAndByTheSameHold(string name, string buildPath)
    {
        var dashboard = new DashboardInfo(name, buildPath);

        // The whole word back, letter for letter: long, short, spaced inside, and outside the ASCII.
        Assert.Equal(name, dashboard.Name);
        Assert.Equal(buildPath, dashboard.BuildPath);

        // And the word itself, not a copy of it written of the same letters - the record holds what it was handed.
        Assert.Same(name, dashboard.Name);
        Assert.Same(buildPath, dashboard.BuildPath);
    }

    /// <summary>
    /// A long word is still one word: neither of the two parts is cut short, weighed by its first letters or hashed
    /// by a prefix of it. Two dashboards each built of its own characters - the name of 512 <c>x</c> and the path of
    /// <c>/</c> followed by 4000 <c>y</c>, which stays 4001 characters long, each word written out twice over so that
    /// the two dashboards share no reference - are one dashboard by <c>==</c>, by
    /// <see cref="DashboardInfo.Equals" /> and by the hash both give, and they are one entry of a set. The same name
    /// with one letter added at the end is a second dashboard, and so is the dashboard whose
    /// <see cref="DashboardInfo.Name" /> is the 512 <c>x</c> and whose <see cref="DashboardInfo.BuildPath" /> is the
    /// 4000 <c>y</c> without the leading slash: the whole word is weighed, and no prefix of it stands in for the
    /// whole.
    /// <para>
    /// The record copies no word and shortens no word, which is what the page rests on: it reads its own build facts
    /// out of exactly the address the file wrote, however long that address is
    /// (<see cref="DashboardBuildTests.ThePageReadsItsOwnBuildFactsFromItsOwnAddressOnce" />).
    /// </para>
    /// </summary>
    [Fact]
    public void ALongWordIsStillOneWordToTheDashboard()
    {
        var name = Own(new string('x', 512));
        var buildPath = Own("/" + new string('y', 4_000));

        Assert.Equal(512, name.Length);
        Assert.Equal(4_001, buildPath.Length);

        var one = new DashboardInfo(name, buildPath);

        // The two words written out again, character by character, into a second dashboard: one dashboard, though no
        // string of the two is held in common.
        var again = new DashboardInfo(Own(new string('x', 512)), Own("/" + new string('y', 4_000)));

        Assert.True(one == again);
        Assert.False(one != again);
        Assert.True(one.Equals(again));
        Assert.Equal(one.GetHashCode(), again.GetHashCode());

        var set = new HashSet<DashboardInfo> { one, again };

        Assert.Single(set);

        // One letter more at the end of the name is a second dashboard: the 512 and the 513 are not one word.
        var longerName = new DashboardInfo(Own(name + "x"), buildPath);

        Assert.False(one == longerName);
        Assert.True(one != longerName);
        Assert.False(one.Equals(longerName));
        Assert.True(set.Add(longerName));

        // The path without its leading slash - the same 4000 <c>y</c>, one character shorter - is a second dashboard
        // too: the record neither adds a slash nor forgives the lack of one.
        var unslashed = new DashboardInfo(name, Own(new string('y', 4_000)));

        Assert.False(one == unslashed);
        Assert.True(one != unslashed);
        Assert.False(one.Equals(unslashed));
        Assert.True(set.Add(unslashed));

        Assert.Equal(3, set.Count);
    }

    /// <summary>
    /// The one line a dashboard says itself, at the nothing of its two words and at their edges. The whole line is
    /// pinned by <see cref="TopologyEdgeCaseTests.TheDashboardIsARecordOfItsTwoParts" /> for a dashboard of two
    /// words - <c>DashboardInfo { Name = dashboard, BuildPath = /build-facts.json }</c> - and for a dashboard whose
    /// two words are empty strings - <c>DashboardInfo { Name = , BuildPath =  }</c> - but the line is never asked
    /// there of the all-nothing dashboard of <see cref="TwoDashboardsHandedNothingAreOneDashboard" />, nor of a word
    /// of a single character, nor of a word of thousands of them, nor of a word made of whitespace. Those are the
    /// words <see cref="AWordIsHeldAsItIsGivenAndByTheSameHold" /> hands in and reads back; here they are said.
    /// <para>
    /// A dashboard handed nothing is said by the very line a dashboard of two empty words is said by -
    /// <c>DashboardInfo { Name = , BuildPath =  }</c>: a nothing and an empty word leave the same blank where they
    /// stand, and saying either asks nothing that throws. A dashboard of the single letter <c>/</c> for both parts
    /// says those two letters; a dashboard whose <see cref="DashboardInfo.BuildPath" /> is <c>/</c> followed by 4000
    /// <c>y</c> - the 4001-character path of <see cref="ALongWordIsStillOneWordToTheDashboard" /> - is said whole, the
    /// line being the 48 characters about the two words plus the length of the word itself, nothing of that word
    /// shortened, escaped or cut away; and a <see cref="DashboardInfo.Name" /> of spaces with a tab and a newline
    /// between them is said inside the line exactly as it was written, the record escaping nothing and trimming
    /// nothing of the word it holds.
    /// </para>
    /// <para>
    /// The line is what a log and a test report hold of a dashboard, and the page's own address stands in it: the
    /// whole address must be readable there, however long it is
    /// (<see cref="DashboardBuildTests.ThePageReadsItsOwnBuildFactsFromItsOwnAddressOnce" />).
    /// </para>
    /// </summary>
    [Fact]
    public void TheDashboardSaysItsTwoWordsAtTheNothingAndAtTheEdges()
    {
        // The all-nothing dashboard is said by the line the empty words say: a nothing leaves the same blank an empty
        // word leaves, and nothing about saying it falls over.
        var nothing = new DashboardInfo(null!, null!);
        var text = nothing.ToString();

        Assert.Equal("DashboardInfo { Name = , BuildPath =  }", text);
        Assert.Equal(new DashboardInfo(string.Empty, string.Empty).ToString(), text);

        // A word of one character - the slash handed to both parts at once - is said as it stands.
        Assert.Equal("DashboardInfo { Name = /, BuildPath = / }", new DashboardInfo("/", "/").ToString());

        // A word of 4001 characters is said whole: the line is the fixed 48 characters about the two words plus the
        // word's own length, the word standing there unshortened, unescaped and uncut.
        var path = "/" + new string('y', 4_000);
        var longWord = new DashboardInfo(DashboardInfo.DefaultName, path);
        var longText = longWord.ToString();

        Assert.Equal($"DashboardInfo {{ Name = dashboard, BuildPath = {path} }}", longText);
        Assert.Equal(48 + path.Length, longText.Length);

        // Whitespace inside a word is said as it was written - the record escapes no letter of it and trims none.
        var name = "  \t\n ";
        var spaced = new DashboardInfo(name, "/").ToString();

        Assert.Contains(name, spaced, StringComparison.Ordinal);
    }

    /// <summary>A word of its own, written out of characters of its own, so that no two words the tests hand over
    /// stand in one interned string and <c>Assert.Same</c> can tell the holding of a word from a copying of it.</summary>
    private static string Own(string word) => new(word.ToCharArray());

    /// <summary>The public field names the dashboard declares itself, in the given scope -
    /// <see cref="BindingFlags.Static" /> for the fixed word it carries, <see cref="BindingFlags.Instance" /> for
    /// the words each dashboard is made of. The field helpers of <see cref="TopologyEdgeCaseTests" /> are private to
    /// that file, so this one stands here.</summary>
    private static string[] PublicFieldNames(BindingFlags scope) =>
        [.. typeof(DashboardInfo)
            .GetFields(BindingFlags.Public | BindingFlags.DeclaredOnly | scope)
            .Select(field => field.Name)
            .Order(StringComparer.Ordinal)];
}
