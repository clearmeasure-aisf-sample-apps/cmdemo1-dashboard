using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Dashboard.Tests;

/// <summary>
/// The boundary and null/empty tests of <see cref="EnvironmentInfo" /> declared in
/// <c>src/Dashboard/Health/Topology.cs</c> that no existing test pins: the property nullability and
/// init-only setter shape the compiler writes, the record's equality against the things it is not,
/// the hash code standing from call to call and serving as a dictionary key, the line that names
/// every part even the ones a caller leaves out, the words the empty tier and the absent tier say,
/// and the words of awkward parts kept exactly as handed.
/// <para>
/// What the files beside it already pin is left alone. <see cref="TopologyEdgeCaseTests" /> pins the
/// eight parts' names, the absent parts a caller may leave out, the empty and absent list
/// boundaries, the record's equality by all eight parts, the links compared as the hold they stand
/// on, the Deconstruct of the eight, the with-copy and the line of words the environment says, and
/// the full public surface. <see cref="TopologyParserTests" /> pins the validation of the file.
/// </para>
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

    private static readonly EnvironmentInfo OneEnvironment = new("uat", "nonprod", OneDeployable, Versions, Versions, Links, "demo-uat", [Scorecard]);
    private static readonly EnvironmentInfo NoEnvironment = new("tdd", null, []);

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
        var occurrences = CountOccurrences(line, " = ");
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

    private static int CountOccurrences(string text, string value)
    {
        var count = 0;
        var index = 0;

        while ((index = text.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += value.Length;
        }

        return count;
    }
}
