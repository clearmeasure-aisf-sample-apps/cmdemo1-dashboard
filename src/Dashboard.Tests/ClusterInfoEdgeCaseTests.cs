using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="ClusterInfo" /> - the cluster a system may name
/// in <c>topology.json</c> - covering the four parts the record keeps and the members every record is asked,
/// only where no existing test pins them.
/// <para>
/// What the files beside it already pin is left alone. <see cref="TopologyEdgeCaseTests" /> pins the constructor's
/// four arguments each back as the property of its own name, the three parts a caller may leave out standing
/// absent - the name it must name standing where it names it, even as nothing or as the empty word - the record's
/// equality by all four parts, its falling apart into them, its <c>with</c> copy, the line of words it says with
/// every part named and the one it says when its name is the empty word and the rest stands as nothing, and its
/// whole public surface: the parts' names and types, the constructor's parameters and their defaults, and the
/// nothing else to call. <see cref="TopologyParserTests" /> pins what the file becomes: the absent and the null
/// cluster, and the addresses a file may not name. <see cref="ClusterMonitorEdgeCaseTests" /> pins what a monitor
/// makes of the cluster's two addresses.
/// </para>
/// <para>
/// What is pinned here is the rest: that the four parts say of themselves, as the compiler writes into the get and
/// the set of each, that every one of them may be nothing, and that a part is written only at the making; that a
/// cluster is none of the things it is not, asked both ways where the type knows how to answer; that the hash code
/// it says stands from call to call and serves as a dictionary key; that the word of its name is kept as it was
/// handed - not trimmed, not normalised, not cased, not put through a format; that it says the name of every part
/// even the ones it is not told; that the name standing as nothing and the empty word say the same words while the
/// record holds them two; that an address is compared as an address is - the same site with a default port, a
/// fragment after the <c>#</c> or the scheme and the host in another case is the same address, while a trailing
/// slash that turns the file's path into a directory's path is another, for the status file and the service file
/// alike; the parts read out of a cluster that names none of them; and the copy that names one part carrying the
/// other three. The only fixed values the record carries are the three defaults of the parts a caller may leave
/// out, and those are pinned beside the record's other parts.
/// </para>
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: every address is named and never fetched, and
/// the record decides nothing of its own.
/// </para>
/// </summary>
public class ClusterInfoEdgeCaseTests
{
    private static readonly Uri StatusUrl = new("https://raw.example.net/org/demo-system/status/cluster.json");
    private static readonly Uri ServiceUrl = new("https://raw.example.net/org/demo-system/status/aks.json");
    private static readonly Uri OtherStatusUrl = new("https://raw.example.net/org/demo-system/status/other-cluster.json");
    private static readonly Uri OtherServiceUrl = new("https://raw.example.net/org/demo-system/status/other-aks.json");

    private static readonly LinkSet Links = new([new KeyValuePair<string, Uri>(LinkSet.Portal, new Uri("https://portal.example.net/#aks-demo"))]);
    private static readonly LinkSet OtherLinks = new([new KeyValuePair<string, Uri>(LinkSet.Portal, new Uri("https://portal.example.net/#aks-demo"))]);

    /// <summary>A cluster named by all four parts, the four values standing apart from one another, so that an
    /// argument landing on another property is seen instead of quietly matching a second part of the record.</summary>
    private static readonly ClusterInfo Full = new("aks-demo", StatusUrl, ServiceUrl, Links);

    /// <summary>A cluster named by its name alone, the three parts a caller may leave out standing as they stand
    /// when nobody names them.</summary>
    private static readonly ClusterInfo Bare = new("aks-demo");

    // ------------------------------------------------------------------ the four parts the record keeps

    /// <summary>The four parts say of themselves, as the compiler writes into the get and the set of each, which
    /// may be nothing: the record promises the file no name, no address and no links of its own, and the reader of
    /// it leans on the same four answers the constructor's four parameters give.</summary>
    [Fact]
    public void TheFourPartsSayOfThemselvesWhichMayBeNothing()
    {
        var nullability = new NullabilityInfoContext();

        NullabilityState[] expected =
        [
            NullabilityState.Nullable,
            NullabilityState.Nullable,
            NullabilityState.Nullable,
            NullabilityState.Nullable,
        ];

        var properties = typeof(ClusterInfo)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        Assert.Equal(expected, [.. properties.Select(property => nullability.Create(property).ReadState)]);
        Assert.Equal(expected, [.. properties.Select(property => nullability.Create(property).WriteState)]);
    }

    /// <summary>Every part is written only at the making: the property reads and its setter stands, and that setter
    /// is the compiler's <c>init</c> - carrying the custom modifier the compiler puts on it - so no caller turns a
    /// part of a cluster it was handed; the <c>with</c> copy reaches the parts from inside the record.</summary>
    [Fact]
    public void EveryPartIsWrittenOnlyAtTheMakingAndNeverTurnedAfter()
    {
        var properties = typeof(ClusterInfo)
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

    /// <summary>A cluster is none of the things it is not, and none of the things it is not says it is a cluster:
    /// the environment the system it names runs in, the node behind that environment's deployable, the address of
    /// its status file, the link set it stands on and the thing that is no cluster at all - asked both ways where
    /// the type knows how to answer. (The topology that holds the cluster, the word it is named by and the nothing
    /// it is asked about are pinned beside it.)</summary>
    [Fact]
    public void AClusterIsNoneOfTheThingsItIsNot()
    {
        var environment = new EnvironmentInfo("uat", null, []);
        var node = new NodeInfo("ca-demo-uat-ui", "westus3", NodeInfo.PrimaryRole, new Uri("https://ui.uat.example.net"));

        Assert.False(Full.Equals(environment));
        Assert.False(environment.Equals(Full));
        Assert.False(Full.Equals(node));
        Assert.False(node.Equals(Full));
        Assert.False(Full.Equals(StatusUrl));
        Assert.False(StatusUrl.Equals(Full));
        Assert.False(Full.Equals(Links));
        Assert.False(Full.Equals(new object()));
    }

    /// <summary>The hash code a cluster says stands where it stood: asked of a second instance built on the very
    /// same four holds, of the copy that names nothing, and of the cluster that names none of them - and it
    /// follows the parts the cluster is compared by, so two clusters that are two are two keys.</summary>
    [Fact]
    public void TheHashCodeOfAClusterStandsFromCallToCall()
    {
        Assert.Equal(Full.GetHashCode(), Full.GetHashCode());
        Assert.Equal(Full.GetHashCode(), (Full with { }).GetHashCode());
        Assert.Equal(Bare.GetHashCode(), new ClusterInfo("aks-demo").GetHashCode());

        var keyed = new Dictionary<ClusterInfo, string> { [Full] = "aks-demo" };
        Assert.Equal("aks-demo", keyed[Full with { }]);
        Assert.False(keyed.ContainsKey(Full with { Name = "aks-other" }));
        Assert.False(keyed.ContainsKey(Bare));
    }

    /// <summary>Every word the reader of the file hands a cluster as its name is kept as-is, not trimmed, not
    /// normalised, not cased, not put through a format: the property returns the very string it was handed, the
    /// same word stands whole in the line of words the cluster says, and a cluster with that word is not one with
    /// the word trimmed or upper-cased - the record weighs no word and decides nothing of its own. (The reader
    /// that trims what the file says is <see cref="TopologyParserTests" />'s.)</summary>
    [Theory]
    [MemberData(nameof(AllNames))]
    public void AWordIsKeptAsItWasHanded(string word)
    {
        var named = new ClusterInfo(word);
        Assert.Equal(word, named.Name);
        Assert.Contains(word, named.ToString(), StringComparison.Ordinal);

        var trimmed = word.Trim();
        if (word != trimmed)
        {
            Assert.NotEqual(named, new ClusterInfo(trimmed));
        }

        var upper = word.ToUpper(CultureInfo.InvariantCulture);
        if (word != upper)
        {
            Assert.NotEqual(named, new ClusterInfo(upper));
        }
    }

    /// <summary>The ten words weighed by the theory above, including the five hundred and twelve-letter word that
    /// stands as a string built at test time.</summary>
    public static IEnumerable<object[]> AllNames => new[]
    {
        new object[] { "" },
        new object[] { " " },
        new object[] { "   " },
        new object[] { "  aks-demo  " },
        new object[] { "a\nb\tc" },
        new object[] { "café – 😀" },
        new object[] { new string('n', 512) },
        new object[] { "a" },
        new object[] { "<b>aks-demo</b>" },
        new object[] { "{0} {1}" },
    };

    // ------------------------------------------------------------------ the words a cluster says

    /// <summary>Each of the four parts, named as nothing one at a time while the other three are named: the line
    /// keeps its four names, in the order Name, StatusUrl, ServiceUrl, Links, and the part that is nothing is said
    /// as an empty value - the word before the <c>=</c> is still there, the word after it is gone. The words before
    /// the absent part and the words after it are untouched, and the count of <c>=</c> is four in every line.
    /// (The line with every part named and the one with the name an empty word and the rest nothing are pinned
    /// beside it.)</summary>
    [Theory]
    [InlineData("Name", "ClusterInfo { Name = , StatusUrl = https://raw.example.net/org/demo-system/status/cluster.json, ServiceUrl = https://raw.example.net/org/demo-system/status/aks.json, Links = Dashboard.Health.LinkSet }")]
    [InlineData("StatusUrl", "ClusterInfo { Name = aks-demo, StatusUrl = , ServiceUrl = https://raw.example.net/org/demo-system/status/aks.json, Links = Dashboard.Health.LinkSet }")]
    [InlineData("ServiceUrl", "ClusterInfo { Name = aks-demo, StatusUrl = https://raw.example.net/org/demo-system/status/cluster.json, ServiceUrl = , Links = Dashboard.Health.LinkSet }")]
    [InlineData("Links", "ClusterInfo { Name = aks-demo, StatusUrl = https://raw.example.net/org/demo-system/status/cluster.json, ServiceUrl = https://raw.example.net/org/demo-system/status/aks.json, Links =  }")]
    public void TheClusterSaysEveryPartNameEvenTheOnesItIsNotTold(string partName, string expected)
    {
        var cluster = partName switch
        {
            "Name" => new ClusterInfo(null, StatusUrl, ServiceUrl, Links),
            "StatusUrl" => new ClusterInfo("aks-demo", null, ServiceUrl, Links),
            "ServiceUrl" => new ClusterInfo("aks-demo", StatusUrl, null, Links),
            "Links" => new ClusterInfo("aks-demo", StatusUrl, ServiceUrl, null),
            _ => throw new ArgumentOutOfRangeException(nameof(partName)),
        };

        var line = cluster.ToString();

        // The full line is pinned: every part name is said, in order, with the absent part as an empty value.
        Assert.Equal(expected, line);

        // The count of the words that name a part is four, even when one part is nothing.
        Assert.Equal(4, MarkupGuards.CountOccurrences(line, " = "));
    }

    /// <summary>The empty word and the nothing that is not a word are two different names to the record's
    /// equality, but the line they say about themselves is one: a cluster whose name is the empty word and a
    /// cluster whose name is nothing say exactly the same words, while <c>Equals</c> and <c>==</c> hold that they
    /// are not one. The words are no answer to whether a name is there.</summary>
    [Fact]
    public void AClusterWithNoNameAndOneWithAnEmptyNameSayTheSameWords()
    {
        var absent = new ClusterInfo(null, StatusUrl, ServiceUrl, Links);
        var empty = new ClusterInfo(string.Empty, StatusUrl, ServiceUrl, Links);

        Assert.NotEqual(absent, empty);
        Assert.True(absent != empty);

        Assert.Equal(absent.ToString(), empty.ToString());
    }

    // ------------------------------------------------------------------ the two addresses the record keeps

    /// <summary>An address is compared as an address is, the cluster saying nothing of its own about it: the same
    /// site written with a default port, with a fragment after the <c>#</c>, or with the scheme and the host in
    /// another case is the same address and makes one cluster of two, while a trailing slash that turns the
    /// file's path into a directory's path, another host, another scheme and another port are four other
    /// addresses - for the status file and the service file alike. (That the file's addresses must be absolute
    /// http or https is <see cref="TopologyParserTests" />'s; nothing here fetches one.)</summary>
    [Theory]
    [InlineData("StatusUrl", "https://raw.example.net:443/org/demo-system/status/cluster.json", true)]
    [InlineData("StatusUrl", "https://raw.example.net/org/demo-system/status/cluster.json#tiles", true)]
    [InlineData("StatusUrl", "HTTPS://RAW.example.net/org/demo-system/status/cluster.json", true)]
    [InlineData("StatusUrl", "https://raw.example.net/org/demo-system/status/cluster.json/", false)]
    [InlineData("StatusUrl", "https://raw.example.org/org/demo-system/status/cluster.json", false)]
    [InlineData("StatusUrl", "http://raw.example.net/org/demo-system/status/cluster.json", false)]
    [InlineData("StatusUrl", "https://raw.example.net:8443/org/demo-system/status/cluster.json", false)]
    [InlineData("ServiceUrl", "https://raw.example.net:443/org/demo-system/status/aks.json", true)]
    [InlineData("ServiceUrl", "https://raw.example.net/org/demo-system/status/aks.json#facts", true)]
    [InlineData("ServiceUrl", "HTTPS://RAW.example.net/org/demo-system/status/aks.json", true)]
    [InlineData("ServiceUrl", "https://raw.example.net/org/demo-system/status/aks.json/", false)]
    [InlineData("ServiceUrl", "https://raw.example.org/org/demo-system/status/aks.json", false)]
    [InlineData("ServiceUrl", "http://raw.example.net/org/demo-system/status/aks.json", false)]
    [InlineData("ServiceUrl", "https://raw.example.net:8443/org/demo-system/status/aks.json", false)]
    public void AnAddressIsComparedAsAnAddressIs(string partName, string url, bool sameCluster)
    {
        var moved = partName == "StatusUrl"
            ? Full with { StatusUrl = new Uri(url) }
            : Full with { ServiceUrl = new Uri(url) };

        Assert.Equal(sameCluster, moved == Full);
    }

    // ------------------------------------------------------------------ the record itself

    /// <summary>The parts read out of a cluster that names none of them: the name it must name reads out as the
    /// word it holds, and the three a caller may leave out read out as the nothing they are, in the order they
    /// stand. (The cluster named by all four reading out is pinned beside it.)</summary>
    [Fact]
    public void ThePartsReadOutOfAClusterThatNamesNoneOfThem()
    {
        var (name, statusUrl, serviceUrl, links) = Bare;

        Assert.Equal("aks-demo", name);
        Assert.Null(statusUrl);
        Assert.Null(serviceUrl);
        Assert.Null(links);
    }

    /// <summary><c>with</c> names the part it changes and carries the other three: a copy that names nothing is
    /// one with its original and not the same object, and the cluster it was copied from is untouched - and a copy
    /// that changes any one of the four carries the other three as the very values they stood.</summary>
    [Fact]
    public void WithChangesOnlyThePartItNames()
    {
        var cluster = Full;
        var copy = cluster with { };

        Assert.Equal(cluster, copy);
        Assert.NotSame(cluster, copy);

        var named = cluster with { Name = "aks-other" };
        Assert.Equal("aks-other", named.Name);
        Assert.Equal(StatusUrl, named.StatusUrl);
        Assert.Equal(ServiceUrl, named.ServiceUrl);
        Assert.Same(Links, named.Links);

        var status = cluster with { StatusUrl = OtherStatusUrl };
        Assert.Equal("aks-demo", status.Name);
        Assert.Equal(OtherStatusUrl, status.StatusUrl);
        Assert.Equal(ServiceUrl, status.ServiceUrl);
        Assert.Same(Links, status.Links);

        var service = cluster with { ServiceUrl = OtherServiceUrl };
        Assert.Equal("aks-demo", service.Name);
        Assert.Equal(StatusUrl, service.StatusUrl);
        Assert.Equal(OtherServiceUrl, service.ServiceUrl);
        Assert.Same(Links, service.Links);

        var linked = cluster with { Links = OtherLinks };
        Assert.Equal("aks-demo", linked.Name);
        Assert.Equal(StatusUrl, linked.StatusUrl);
        Assert.Equal(ServiceUrl, linked.ServiceUrl);
        Assert.Same(OtherLinks, linked.Links);
        Assert.NotSame(Links, linked.Links);

        // The copy reaches the parts from inside the record; the cluster it was copied from is untouched.
        Assert.Equal("aks-demo", cluster.Name);
        Assert.Equal(StatusUrl, cluster.StatusUrl);
        Assert.Equal(ServiceUrl, cluster.ServiceUrl);
        Assert.Same(Links, cluster.Links);
    }
}
