using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of the <see cref="EnvironmentInfo" /> a
/// <c>topology.json</c> carries, declared in <c>src/Dashboard/Health/Topology.cs</c>: the eight parts
/// each back as the property of its own name, the six a caller may leave out standing absent,
/// the words handed kept exactly as handed, the empty list and the absent list told apart, the
/// record's equality by all eight parts with the links compared as the hold they stand on, the
/// Deconstruct of the eight, the with-copy and the line of words the environment says, the full
/// public surface - and, beside them, the property nullability and init-only setter shape the
/// compiler writes, the record's equality against the things it is not, the hash code standing from
/// call to call and serving as a dictionary key, the line that names every part even the ones a
/// caller leaves out, the words the empty tier and the absent tier say, and the words of awkward
/// parts kept exactly as handed.
/// <para>
/// What the files beside it already pin is left alone. <see cref="TopologyEdgeCaseTests" /> pins the
/// rest of the file on its own: <see cref="Topology" /> itself, its <see cref="SystemInfo" /> and
/// <see cref="DashboardInfo" />, the <see cref="PageLink" /> a page stands on and the
/// <see cref="ClusterInfo" /> a system may name. <see cref="TopologyParserTests" /> pins the
/// validation of the file.
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: every address is named and
/// never fetched, and the record decides nothing of its own.
/// </para>
/// </summary>
public class EnvironmentInfoEdgeCaseTests
{
    private static readonly LinkSet Links = new([new KeyValuePair<string, Uri>(LinkSet.Portal, new Uri("https://portal.example.net/#demo"))]);
    private static readonly NodeInfo West = new("ca-demo-uat-ui", "westus3", NodeInfo.PrimaryRole, new Uri("https://ui.uat.example.net"));
    private static readonly IReadOnlyList<NodeInfo> OneNode = [West];

    private static readonly DeployableInfo Ui =
        new("ui", null, DeployableInfo.DefaultHealthPath, DeployableInfo.DefaultAlivePath, DeployableInfo.DefaultVersionPath, OneNode);
    private static readonly IReadOnlyList<DeployableInfo> OneDeployable = [Ui];

    private static readonly PageLink Scorecard = new("Scorecard", new Uri("https://ca-demo-uat-scorecard.example.net/scorecard"), "scorecard");
    private static readonly PageLink Reports = new("Reports", new Uri("https://reports.example.net/"));
    private static readonly IReadOnlyList<PageLink> TwoPages = new[] { Scorecard, Reports };

    private static readonly Uri FrontDoor = new("https://demo.azurefd.net");
    private static readonly DeployableInfo AtDoor =
        new("ui", FrontDoor, DeployableInfo.DefaultHealthPath, DeployableInfo.DefaultAlivePath, DeployableInfo.DefaultVersionPath, OneNode);
    private static readonly IReadOnlyList<DeployableInfo> TwoDeployables = new[] { Ui, AtDoor };

    private static readonly Uri Versions = new("https://raw.example.net/org/demo-system/main/environments/uat/versions.json");
    private static readonly Uri VersionsHistory = new("https://github.example.net/org/demo-system/commits/main/environments/uat/versions.json");

    /// <summary>A second link set holding the one and the same link as <see cref="Links" />: the two sets
    /// are two holds of the one word, which is what tells two environments apart although their links
    /// say the same thing.</summary>
    private static readonly LinkSet OtherLinks = new([new KeyValuePair<string, Uri>(LinkSet.Portal, new Uri("https://portal.example.net/#demo"))]);
    private static readonly IReadOnlyList<DeployableInfo> NoDeployables = [];
    private static readonly IReadOnlyList<PageLink> NoPages = [];

    private static readonly EnvironmentInfo OneEnvironment = new("uat", "nonprod", OneDeployable, Versions, Versions, Links, "demo-uat", [Scorecard]);
    private static readonly EnvironmentInfo NoEnvironment = new("tdd", null, []);

    /// <summary>An environment with every part named: its tier, its two version addresses, its links, its
    /// namespace, its two pages and its two deployables - the second of them with a Front Door.</summary>
    private static readonly EnvironmentInfo FullEnvironment = new("uat", "nonprod", TwoDeployables, Versions, VersionsHistory, Links, "demo-uat", TwoPages);

    /// <summary>An environment named by nothing but its name and its deployables, the six a caller may leave out
    /// standing absent.</summary>
    private static readonly EnvironmentInfo BareEnvironment = new("tdd", null, NoDeployables);

    /// <summary>The eight properties say of themselves, as the compiler writes it into each of them,
    /// which may be nothing and which may not - read out of the property and written back to it, the
    /// same eight answers the constructor's eight parameters give: the record promises the page a name
    /// and a list of deployables, and nothing of its own for the five a caller may leave out.</summary>
    [Fact]
    public void TheEightPartsSayOfThemselvesWhichMayBeNothing()
    {
        var nullability = new NullabilityInfoContext();
        NullabilityState[] expected =
        [
            NullabilityState.NotNull,
            NullabilityState.Nullable,
            NullabilityState.NotNull,
            NullabilityState.Nullable,
            NullabilityState.Nullable,
            NullabilityState.Nullable,
            NullabilityState.Nullable,
            NullabilityState.Nullable,
        ];

        var properties = typeof(EnvironmentInfo)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        Assert.Equal(expected, [.. properties.Select(property => nullability.Create(property).ReadState)]);
        Assert.Equal(expected, [.. properties.Select(property => nullability.Create(property).WriteState)]);
    }

    /// <summary>Every part is written only at the making: the property reads and its setter stands,
    /// and that setter is the compiler's <c>init</c> - carrying the custom modifier the compiler puts
    /// on it - so no caller turns a part of an environment it was handed; the <c>with</c> copy reaches
    /// the parts from inside the record.</summary>
    [Fact]
    public void EveryPartIsWrittenOnlyAtTheMakingAndNeverTurnedAfter()
    {
        var properties = typeof(EnvironmentInfo)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        Assert.All(properties, property =>
        {
            Assert.True(property.CanRead);
            Assert.True(property.CanWrite);
            Assert.NotNull(property.GetMethod);
            Assert.NotNull(property.SetMethod);
            Assert.True(property.GetMethod!.IsPublic);
            Assert.Contains(typeof(IsExternalInit), property.SetMethod!.ReturnParameter.GetRequiredCustomModifiers());
        });
    }

    /// <summary>An environment is none of the things it is not, and none of the things it is not says it
    /// is an environment: the node behind its deployable, the page its part of the dashboard leads to,
    /// the link set it stands on, the address of its pin file and the thing that is no environment at
    /// all - asked both ways where the type knows how to answer.</summary>
    [Fact]
    public void AnEnvironmentIsNoneOfTheThingsItIsNot()
    {
        Assert.False(Ui.Equals(OneEnvironment));
        Assert.False(OneEnvironment.Equals(West));
        Assert.False(West.Equals(OneEnvironment));
        Assert.False(OneEnvironment.Equals(Scorecard));
        Assert.False(Scorecard.Equals(OneEnvironment));
        Assert.False(OneEnvironment.Equals(Links));
        Assert.False(OneEnvironment.Equals(Versions));
        Assert.False(OneEnvironment.Equals(new object()));
    }

    /// <summary>The hash code an environment says stands where it stood: asked of a second instance built on
    /// the very same eight holds, and asked of the environment that names none of them - and it follows
    /// the parts the environment is compared by, so two environments that are two are two keys.</summary>
    [Fact]
    public void TheHashCodeOfAnEnvironmentStandsFromCallToCall()
    {
        Assert.Equal(OneEnvironment.GetHashCode(), (OneEnvironment with { }).GetHashCode());
        Assert.Equal(NoEnvironment.GetHashCode(), new EnvironmentInfo("tdd", null, []).GetHashCode());

        var keyed = new Dictionary<EnvironmentInfo, string> { [OneEnvironment] = "uat" };
        Assert.Equal("uat", keyed[OneEnvironment with { }]);
        Assert.False(keyed.ContainsKey(OneEnvironment with { Namespace = "demo-prod" }));
    }

    /// <summary>Every word the reader of the file hands an environment is kept as-is, not trimmed,
    /// not normalised, not escaped, not put through a format: the property returns the very string it
    /// was handed, the same word stands whole in the line of words the environment says, and an
    /// environment with that word is not one with the word trimmed, upper-cased or turned to nothing -
    /// not for the name, not for the tier, not for the namespace.</summary>
    [Theory]
    [MemberData(nameof(AllWords))]
    public void AWordIsKeptAsItWasHanded(string word)
    {
        var named = new EnvironmentInfo(word, null, []);
        Assert.Equal(word, named.Name);
        Assert.Contains(word, named.ToString(), StringComparison.Ordinal);

        var trimmed = word.Trim();
        if (word != trimmed)
        {
            Assert.NotEqual(named, new EnvironmentInfo(trimmed, null, []));
        }

        var upper = word.ToUpper(CultureInfo.InvariantCulture);
        if (word != upper)
        {
            Assert.NotEqual(named, new EnvironmentInfo(upper, null, []));
        }

        var tiered = new EnvironmentInfo("uat", word, []);
        Assert.Equal(word, tiered.Tier);
        Assert.Contains(word, tiered.ToString(), StringComparison.Ordinal);
        Assert.NotEqual(tiered, new EnvironmentInfo("uat", null, []));
        if (word != upper)
        {
            Assert.NotEqual(tiered, new EnvironmentInfo("uat", upper, []));
        }

        var namespaced = new EnvironmentInfo("uat", "nonprod", [], Namespace: word);
        Assert.Equal(word, namespaced.Namespace);
        Assert.Contains(word, namespaced.ToString(), StringComparison.Ordinal);
        Assert.NotEqual(namespaced, new EnvironmentInfo("uat", "nonprod", [], Namespace: null));
        if (word != trimmed)
        {
            Assert.NotEqual(namespaced, new EnvironmentInfo("uat", "nonprod", [], Namespace: trimmed));
        }
        if (word != upper)
        {
            Assert.NotEqual(namespaced, new EnvironmentInfo("uat", "nonprod", [], Namespace: upper));
        }
    }

    /// <summary>The ten words weighed by the theory above, including the five hundred and
    /// twelve-letter word that stands as a string built at test time.</summary>
    public static IEnumerable<object[]> AllWords => new[]
    {
        new object[] { "" },
        new object[] { " " },
        new object[] { "   " },
        new object[] { "  uat  " },
        new object[] { "a\nb\tc" },
        new object[] { "café – 😀" },
        new object[] { new string('n', 512) },
        new object[] { "u" },
        new object[] { "<b>uat</b>" },
        new object[] { "{0} {1}" },
    };

    /// <summary>Each of the six parts a caller may leave out, named as nothing one at a time while the other seven
    /// are named: the line keeps its eight names, in the order Name, Tier, Deployables, VersionsUrl,
    /// VersionsHistoryUrl, Links, Namespace, Pages, and the part that is nothing is said as an empty value - the
    /// word before the <c>=</c> is still there, the word after it is gone. The words before the changed part and
    /// the words after it are untouched, and the count of <c>=</c> is eight in every line. (The two list parts
    /// say the concrete type spellings <c>Dashboard.Health.DeployableInfo[]</c> and <c>Dashboard.Health.PageLink[]</c>,
    /// which the expected lines pin; the part names and their order are what the environment says.)</summary>
    [Theory]
    [InlineData("Tier", "EnvironmentInfo { Name = uat, Tier = , Deployables = Dashboard.Health.DeployableInfo[], VersionsUrl = https://raw.example.net/org/demo-system/main/environments/uat/versions.json, VersionsHistoryUrl = https://github.example.net/org/demo-system/commits/main/environments/uat/versions.json, Links = Dashboard.Health.LinkSet, Namespace = demo-uat, Pages = Dashboard.Health.PageLink[] }")]
    [InlineData("VersionsUrl", "EnvironmentInfo { Name = uat, Tier = nonprod, Deployables = Dashboard.Health.DeployableInfo[], VersionsUrl = , VersionsHistoryUrl = https://github.example.net/org/demo-system/commits/main/environments/uat/versions.json, Links = Dashboard.Health.LinkSet, Namespace = demo-uat, Pages = Dashboard.Health.PageLink[] }")]
    [InlineData("VersionsHistoryUrl", "EnvironmentInfo { Name = uat, Tier = nonprod, Deployables = Dashboard.Health.DeployableInfo[], VersionsUrl = https://raw.example.net/org/demo-system/main/environments/uat/versions.json, VersionsHistoryUrl = , Links = Dashboard.Health.LinkSet, Namespace = demo-uat, Pages = Dashboard.Health.PageLink[] }")]
    [InlineData("Links", "EnvironmentInfo { Name = uat, Tier = nonprod, Deployables = Dashboard.Health.DeployableInfo[], VersionsUrl = https://raw.example.net/org/demo-system/main/environments/uat/versions.json, VersionsHistoryUrl = https://github.example.net/org/demo-system/commits/main/environments/uat/versions.json, Links = , Namespace = demo-uat, Pages = Dashboard.Health.PageLink[] }")]
    [InlineData("Namespace", "EnvironmentInfo { Name = uat, Tier = nonprod, Deployables = Dashboard.Health.DeployableInfo[], VersionsUrl = https://raw.example.net/org/demo-system/main/environments/uat/versions.json, VersionsHistoryUrl = https://github.example.net/org/demo-system/commits/main/environments/uat/versions.json, Links = Dashboard.Health.LinkSet, Namespace = , Pages = Dashboard.Health.PageLink[] }")]
    [InlineData("Pages", "EnvironmentInfo { Name = uat, Tier = nonprod, Deployables = Dashboard.Health.DeployableInfo[], VersionsUrl = https://raw.example.net/org/demo-system/main/environments/uat/versions.json, VersionsHistoryUrl = https://github.example.net/org/demo-system/commits/main/environments/uat/versions.json, Links = Dashboard.Health.LinkSet, Namespace = demo-uat, Pages =  }")]
    public void TheEnvironmentSaysEveryPartNameEvenTheOnesItIsNotTold(string partName, string expected)
    {
        var environment = partName switch
        {
            "Tier" => new EnvironmentInfo("uat", null, TwoDeployables, Versions, VersionsHistory, Links, "demo-uat", TwoPages),
            "VersionsUrl" => new EnvironmentInfo("uat", "nonprod", TwoDeployables, null, VersionsHistory, Links, "demo-uat", TwoPages),
            "VersionsHistoryUrl" => new EnvironmentInfo("uat", "nonprod", TwoDeployables, Versions, null, Links, "demo-uat", TwoPages),
            "Links" => new EnvironmentInfo("uat", "nonprod", TwoDeployables, Versions, VersionsHistory, null, "demo-uat", TwoPages),
            "Namespace" => new EnvironmentInfo("uat", "nonprod", TwoDeployables, Versions, VersionsHistory, Links, null, TwoPages),
            "Pages" => new EnvironmentInfo("uat", "nonprod", TwoDeployables, Versions, VersionsHistory, Links, "demo-uat", null),
            _ => throw new ArgumentOutOfRangeException(nameof(partName)),
        };

        var line = environment.ToString();

        // The full line is pinned: every part name is said, in order, with the absent part as an empty value.
        Assert.Equal(expected, line);

        // The count of the words that name a part is eight, even when one part is nothing.
        var occurrences = MarkupGuards.CountOccurrences(line, " = ");
        Assert.Equal(8, occurrences);
    }

    /// <summary>The empty word and the nothing that is not a word are two different tiers to the record's
    /// equality, but the line they say about themselves is one: an environment whose tier is the empty word and an
    /// environment whose tier is nothing say exactly the same words, while <c>Equals</c> and <c>==</c> hold that
    /// they are not one. The words are no answer to whether a tier is there.</summary>
    [Fact]
    public void AnEnvironmentWithNoTierAndOneWithAnEmptyTierSayTheSameWords()
    {
        var absent = new EnvironmentInfo("uat", null, TwoDeployables, Versions, VersionsHistory, Links, "demo-uat", TwoPages);
        var empty = new EnvironmentInfo("uat", string.Empty, TwoDeployables, Versions, VersionsHistory, Links, "demo-uat", TwoPages);

        Assert.NotEqual(absent, empty);
        Assert.True(absent != empty);

        Assert.Equal(absent.ToString(), empty.ToString());
    }

    // ------------------------------------------------------------------ the eight parts of an environment

    /// <summary>The constructor's eight parts, each back as the property of its own name - the order the reader of
    /// the file hands an environment over in. The two lists and the link set come back as the very holds they were
    /// handed, never copied.</summary>
    [Fact]
    public void TheConstructorHandsEachEnvironmentArgumentToThePropertyOfItsName()
    {
        var environment = FullEnvironment;

        Assert.Equal("uat", environment.Name);
        Assert.Equal("nonprod", environment.Tier);
        Assert.Same(TwoDeployables, environment.Deployables);
        Assert.Equal(Versions, environment.VersionsUrl);
        Assert.Equal(VersionsHistory, environment.VersionsHistoryUrl);
        Assert.Same(Links, environment.Links);
        Assert.Equal("demo-uat", environment.Namespace);
        Assert.Same(TwoPages, environment.Pages);
    }

    /// <summary>The six parts a caller may leave out stand absent when it does: no tier, no version address, no
    /// links, no namespace and no page - and the deployables it must name stand where it names them, an empty list
    /// of them being an environment the page has nothing to check.</summary>
    [Fact]
    public void TheEnvironmentPartsTheCallerDoesNotNameStandAbsent()
    {
        var environment = BareEnvironment;

        Assert.Equal("tdd", environment.Name);
        Assert.Null(environment.Tier);
        Assert.Empty(environment.Deployables);
        Assert.Null(environment.VersionsUrl);
        Assert.Null(environment.VersionsHistoryUrl);
        Assert.Null(environment.Links);
        Assert.Null(environment.Namespace);
        Assert.Null(environment.Pages);

        Assert.Null(new EnvironmentInfo("tdd", null, NoDeployables, Links: null, Namespace: null, Pages: null).Links);
    }

    /// <summary>The environment keeps the words it is given and changes none of them: an empty tier is a tier and
    /// not the nothing a tier of no word is, a namespace of spaces is a namespace, a name of no letters is still a
    /// name, and no part is trimmed, lower-cased or filled in - the reader of the file does all of that
    /// (<see cref="TopologyParserTests" />).</summary>
    [Fact]
    public void AnEnvironmentKeepsItsWordsAsItIsGivenThem()
    {
        var empty = new EnvironmentInfo(string.Empty, string.Empty, NoDeployables, Links: null, Namespace: string.Empty, Pages: NoPages);
        var spaced = new EnvironmentInfo("  uat  ", "  nonprod  ", NoDeployables, Namespace: "  demo-uat  ");
        var nothing = new EnvironmentInfo(null!, null, null!);

        Assert.Equal(string.Empty, empty.Name);
        Assert.Equal(string.Empty, empty.Tier);
        Assert.Equal(string.Empty, empty.Namespace);
        Assert.Same(NoPages, empty.Pages);
        Assert.Empty(empty.Pages!);

        Assert.Equal("  uat  ", spaced.Name);
        Assert.Equal("  nonprod  ", spaced.Tier);
        Assert.Equal("  demo-uat  ", spaced.Namespace);

        Assert.Null(nothing.Name);
        Assert.Null(nothing.Tier);
        Assert.Null(nothing.Deployables);
        Assert.Null(nothing.Pages);
    }

    /// <summary>The empty list and no list at all are two different things, twice over: an environment of no
    /// deployable still holds the list it was given, an environment that names no page holds nothing, and a second
    /// list of the two very pages is another list to a record that opens neither.</summary>
    [Fact]
    public void AnEmptyListOfAnEnvironmentOwnIsNotTheListThatIsAbsent()
    {
        var emptied = FullEnvironment with { Deployables = NoDeployables, Pages = NoPages };
        var unnamed = FullEnvironment with { Pages = null };

        Assert.Empty(emptied.Deployables);
        Assert.Empty(emptied.Pages!);
        Assert.Same(NoDeployables, emptied.Deployables);
        Assert.Same(NoPages, emptied.Pages);

        Assert.Null(unnamed.Pages);
        Assert.NotEqual(emptied, unnamed);

        var copied = FullEnvironment with { Pages = [Scorecard, Reports] };

        Assert.Equal(FullEnvironment.Pages, copied.Pages);
        Assert.NotSame(FullEnvironment.Pages, copied.Pages);
        Assert.NotEqual(FullEnvironment, copied);
    }

    /// <summary>Two environments are one while their eight parts are - the operators following <c>Equals</c> and the
    /// hash code following them - and any one of the eight tells two apart: the empty tier and the absent one, the
    /// absent link set and the empty one, the empty page list and the absent one, a second list of the two very
    /// deployables, and a version address told in another spelling.</summary>
    [Fact]
    public void TwoEnvironmentsAreOneWhileTheirEightPartsAre()
    {
        var one = FullEnvironment;
        var two = new EnvironmentInfo("uat", "nonprod", TwoDeployables, Versions, VersionsHistory, Links, "demo-uat", TwoPages);

        Assert.True(one == two);
        Assert.False(one != two);
        Assert.Equal(one, two);
        Assert.True(one.Equals(two));
        Assert.True(two.Equals((object)one));
        Assert.Equal(one.GetHashCode(), two.GetHashCode());

        Assert.False(one.Equals(null));
        Assert.False(one.Equals("uat"));
        Assert.False(one.Equals(BareEnvironment));
        Assert.False(one.Equals(AtDoor));

        Assert.NotEqual(one, one with { Name = string.Empty });
        Assert.NotEqual(one, one with { Name = "UAT" });
        Assert.NotEqual(one, one with { Tier = null });
        Assert.NotEqual(one, one with { Tier = string.Empty });
        Assert.NotEqual(one, one with { Deployables = NoDeployables });
        Assert.NotEqual(one, one with { Deployables = [.. TwoDeployables] });
        Assert.NotEqual(one, one with { VersionsUrl = null });
        Assert.NotEqual(one, one with { VersionsUrl = new Uri("https://raw.example.net/org/demo-system/main/environments/tdd/versions.json") });
        Assert.NotEqual(one, one with { VersionsHistoryUrl = null });
        Assert.NotEqual(one, one with { Links = null });
        Assert.NotEqual(one, one with { Links = LinkSet.None });
        Assert.NotEqual(one, one with { Links = OtherLinks });
        Assert.NotEqual(one, one with { Namespace = string.Empty });
        Assert.NotEqual(one, one with { Pages = null });
        Assert.NotEqual(one, one with { Pages = NoPages });
    }

    /// <summary>The links an environment holds are compared as the hold they stand on and not as their keys: the
    /// very same link set makes two environments one, and a second set of the one and the same link makes two
    /// environments of them - the record opening neither to look inside it.</summary>
    [Fact]
    public void TheLinksAnEnvironmentHoldsAreComparedAsTheHoldTheyStandOn()
    {
        var one = FullEnvironment;
        var holdingTheSameLinks = new EnvironmentInfo("uat", "nonprod", TwoDeployables, Versions, VersionsHistory, Links, "demo-uat", TwoPages);
        var holdingOtherLinks = one with { Links = OtherLinks };

        Assert.Equal(one, holdingTheSameLinks);
        Assert.Equal(one.GetHashCode(), holdingTheSameLinks.GetHashCode());

        Assert.Equal(one.Links!.Keys, holdingOtherLinks.Links!.Keys);
        Assert.Equal(one.Links[LinkSet.Portal], holdingOtherLinks.Links[LinkSet.Portal]);
        Assert.NotSame(one.Links, holdingOtherLinks.Links);
        Assert.True(one != holdingOtherLinks);
    }

    /// <summary>The eight parts read out of the environment in the order they stand in it, each as the value it
    /// holds, and the absent ones reading out as the nothing they are.</summary>
    [Fact]
    public void TheEightPartsReadOutOfTheEnvironmentInTheOrderTheyStand()
    {
        var (name, tier, deployables, versionsUrl, versionsHistoryUrl, links, @namespace, pages) = FullEnvironment;

        Assert.Equal("uat", name);
        Assert.Equal("nonprod", tier);
        Assert.Same(TwoDeployables, deployables);
        Assert.Equal(Versions, versionsUrl);
        Assert.Equal(VersionsHistory, versionsHistoryUrl);
        Assert.Same(Links, links);
        Assert.Equal("demo-uat", @namespace);
        Assert.Same(TwoPages, pages);

        var (bareName, bareTier, bareDeployables, bareVersions, bareHistory, bareLinks, bareNamespace, barePages) = BareEnvironment;

        Assert.Equal("tdd", bareName);
        Assert.Null(bareTier);
        Assert.Same(NoDeployables, bareDeployables);
        Assert.Null(bareVersions);
        Assert.Null(bareHistory);
        Assert.Null(bareLinks);
        Assert.Null(bareNamespace);
        Assert.Null(barePages);
    }

    /// <summary><c>with</c> names the part it changes and carries the other seven: a copy that names nothing is one
    /// with its original and not the same object, and the environment it was copied from is untouched.</summary>
    [Fact]
    public void WithChangesOnlyTheEnvironmentPartItNames()
    {
        var environment = FullEnvironment;
        var copy = environment with { };

        Assert.Equal(environment, copy);
        Assert.NotSame(environment, copy);

        var moved = environment with { Namespace = "demo-prod", Tier = "prod" };

        Assert.Equal("demo-prod", moved.Namespace);
        Assert.Equal("prod", moved.Tier);
        Assert.Equal("uat", moved.Name);
        Assert.Same(TwoDeployables, moved.Deployables);
        Assert.Same(TwoPages, moved.Pages);
        Assert.Equal("demo-uat", environment.Namespace);
        Assert.Equal("nonprod", environment.Tier);
    }

    /// <summary>The words the environment says about itself: its type, then its eight parts in their order, each
    /// named and an absent part named and left blank - a list saying the collection it stands on, a link set saying
    /// the type it is, and neither of them opened. (The whole line stands here, so a part moved about in the record
    /// is a change the tests see.)</summary>
    [Fact]
    public void TheEnvironmentSaysItsPartsInTheirOrder()
    {
        Assert.Equal(
            "EnvironmentInfo { Name = uat, Tier = nonprod, Deployables = Dashboard.Health.DeployableInfo[], "
                + "VersionsUrl = https://raw.example.net/org/demo-system/main/environments/uat/versions.json, "
                + "VersionsHistoryUrl = https://github.example.net/org/demo-system/commits/main/environments/uat/versions.json, "
                + "Links = Dashboard.Health.LinkSet, Namespace = demo-uat, Pages = Dashboard.Health.PageLink[] }",
            FullEnvironment.ToString());

        Assert.Equal(
            "EnvironmentInfo { Name = tdd, Tier = , Deployables = Dashboard.Health.DeployableInfo[], VersionsUrl = , "
                + "VersionsHistoryUrl = , Links = , Namespace = , Pages =  }",
            BareEnvironment.ToString());
    }

    /// <summary>The environment's whole public surface as it stands: its eight parts, named in the order the
    /// constructor names them and of the type the constructor says, and the members every record is asked - falling
    /// apart, comparing, hashing and saying itself - and nothing else to call: an environment decides nothing of its
    /// own. A ninth part added here has to be written down here too, which is the point.</summary>
    [Fact]
    public void TheEnvironmentHasEightPartsAndDecidesNothingOfItsOwn()
    {
        Assert.Equal(
            ["Name", "Tier", "Deployables", "VersionsUrl", "VersionsHistoryUrl", "Links", "Namespace", "Pages"],
            NamesOf<EnvironmentInfo>());

        Assert.Equal(
            [
                "Deconstruct", "Deployables", "Equals", "GetHashCode", "Links", "Name", "Namespace", "Pages",
                "Tier", "ToString", "VersionsHistoryUrl", "VersionsUrl", "op_Equality", "op_Inequality",
            ],
            PublicSurfaceOf<EnvironmentInfo>());

        var properties = PropertiesOf<EnvironmentInfo>();

        Assert.Equal(typeof(string), properties["Name"].PropertyType);
        Assert.Equal(typeof(string), properties["Tier"].PropertyType);
        Assert.Equal(typeof(IReadOnlyList<DeployableInfo>), properties["Deployables"].PropertyType);
        Assert.Equal(typeof(Uri), properties["VersionsUrl"].PropertyType);
        Assert.Equal(typeof(Uri), properties["VersionsHistoryUrl"].PropertyType);
        Assert.Equal(typeof(LinkSet), properties["Links"].PropertyType);
        Assert.Equal(typeof(string), properties["Namespace"].PropertyType);
        Assert.Equal(typeof(IReadOnlyList<PageLink>), properties["Pages"].PropertyType);

        var parameters = ParametersOf<EnvironmentInfo>();

        Assert.Equal(
            ["Name", "Tier", "Deployables", "VersionsUrl", "VersionsHistoryUrl", "Links", "Namespace", "Pages"],
            [.. parameters.Select(parameter => parameter.Name!)]);
        // The name and the deployables are what an environment must be told - the tier too, although it may be
        // named as nothing - and the five after them may be left out, each standing as nothing when it is.
        Assert.Equal([false, false, false, true, true, true, true, true], [.. parameters.Select(parameter => parameter.HasDefaultValue)]);
        Assert.All(parameters.Take(3), parameter => Assert.Equal(DBNull.Value, parameter.DefaultValue));
        Assert.All(parameters.Skip(3), parameter => Assert.Null(parameter.DefaultValue));
        Assert.Equal(
            [false, true, false, true, true, true, true, true],
            [.. parameters.Select(IsNullable)]);

        Assert.Equal([typeof(IEquatable<EnvironmentInfo>)], typeof(EnvironmentInfo).GetInterfaces());
        Assert.Empty(NamesOfFields<EnvironmentInfo>());
    }

    // ------------------------------------------------------------------ the machinery

    /// <summary>The public properties a record declares itself, in the order they stand in it: its parts as the
    /// constructor names them, and the decisions it makes of them after.</summary>
    private static string[] NamesOf<T>() =>
        [.. typeof(T)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Select(property => property.Name)];

    /// <summary>The public members a record declares itself: its properties and the methods that are not the get
    /// and the set of one - the two operators kept although the compiler names them specially, since they are the
    /// <c>==</c> and the <c>!=</c> a caller writes - with the record's compiler-named clone walked past. The
    /// constructor is no method, so it is named by each record's own test rather than by this list.</summary>
    private static string[] PublicSurfaceOf<T>() =>
        [.. typeof(T)
            .GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(member => member is PropertyInfo
                             || member is MethodInfo method
                             && (!method.IsSpecialName || method.Name.StartsWith("op_", StringComparison.Ordinal)))
            .Where(member => !member.Name.Contains('<', StringComparison.Ordinal))
            .Select(member => member.Name)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];

    /// <summary>The public properties of a record, by name, so that a test can ask one of them what type it
    /// says it is.</summary>
    private static Dictionary<string, PropertyInfo> PropertiesOf<T>() =>
        NamesOf<T>()
            .ToDictionary(
                name => name,
                name => typeof(T).GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)!,
                StringComparer.Ordinal);

    /// <summary>The parts a record's one constructor names, in the order it names them.</summary>
    private static ParameterInfo[] ParametersOf<T>() => typeof(T).GetConstructors().Single().GetParameters();

    /// <summary>True when the record says a part may be nothing: what the compiler writes into the signature, and
    /// what the reader of the file leans on.</summary>
    private static bool IsNullable(ParameterInfo parameter) =>
        new NullabilityInfoContext().Create(parameter).WriteState == NullabilityState.Nullable;

    /// <summary>The fixed words a record carries, by name - the environment carries none at all.</summary>
    private static string[] NamesOfFields<T>() =>
        [.. typeof(T)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Select(field => field.Name)
            .Order(StringComparer.Ordinal)];
}
