namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="RuntimeManifest" /> and the record its last list
/// holds, <see cref="RuntimeUnplaced" />, that neither <see cref="RuntimeManifestEdgeCaseTests" /> nor
/// <see cref="RuntimeManifestBoundaryEdgeCaseTests" /> pins.
/// <para>
/// Those two pin each part as it goes in and comes out, the two optional parts standing as nothing, the lists held and
/// compared as the lists they are, the environment compared ordinally, <c>with</c>, the six-part deconstruction of a
/// full and a four-part manifest, the words, the hash and the reflection surface. What stands here is the round trip
/// between the older four-part shape and the six-part one, equality as the collections and the interface ask it, the
/// deconstruction of a manifest of no parts, and the open corners of <see cref="RuntimeUnplaced" />: its words with
/// every part named, its hash, its culture, its nothing, and the address equality it inherits from <see cref="Uri" />.
/// </para>
/// <para>Nothing here reaches the network or asks a clock: every value below is one the test brought.</para>
/// </summary>
public class RuntimeManifestSurfaceEdgeCaseTests
{
    private const string Environment = "uat";

    private static readonly RuntimeNode[] Nodes = [new("web_primary", RuntimeNodeKind.WebApp, "app-uat")];
    private static readonly RuntimeRegion[] Regions = [new("region_primary", "southcentralus", ["primary"])];
    private static readonly RuntimeEdge[] Edges = [new("browser-to-web_primary", "browser", "web_primary", RuntimeEdgeKind.Public)];
    private static readonly RuntimeFrame[] Frames = [new("sub", "subscription", "demo")];
    private static readonly RuntimeUnplaced[] Unplaced = [new("stray", "Storage account", "rg-demo", "no environment tag")];

    private static RuntimeManifest Full => new(Environment, Nodes, Regions, Edges, Frames, Unplaced);

    private static RuntimeManifest Bare => new(Environment, Nodes, Regions, Edges);

    // ------------------------------------------------------------------------------------------ RuntimeManifest

    [Fact]
    public void AFullManifestStrippedOfItsTwoLaterPartsIsTheOlderManifestAndBack()
    {
        var stripped = Full with { Frames = null, Unplaced = null };

        Assert.Equal(Bare, stripped);
        Assert.Equal(Bare.GetHashCode(), stripped.GetHashCode());
        Assert.Equal(Bare.ToString(), stripped.ToString());

        var grown = Bare with { Frames = Frames, Unplaced = Unplaced };

        Assert.Equal(Full, grown);
        Assert.Equal(Full.GetHashCode(), grown.GetHashCode());

        // Only one of the two dropped: a third manifest, neither the older nor the full one.
        var framesOnly = Full with { Unplaced = null };

        Assert.NotEqual(Bare, framesOnly);
        Assert.NotEqual(Full, framesOnly);
        Assert.Same(Frames, framesOnly.Frames);
        Assert.Null(framesOnly.Unplaced);
        Assert.Equal(3, new HashSet<RuntimeManifest> { Full, Bare, framesOnly, stripped, grown }.Count);
    }

    [Fact]
    public void EqualityIsTheSameWhicheverDoorTheCallerAsksThrough()
    {
        var one = Full;
        var twin = new RuntimeManifest(Environment, Nodes, Regions, Edges, Frames, Unplaced);
        var other = Full with { Environment = "prod" };

        Assert.True(EqualsAsEquatable(one, twin));
        Assert.False(EqualsAsEquatable(one, other));
        Assert.False(EqualsAsEquatable(one, null));

        var comparer = EqualityComparer<RuntimeManifest>.Default;

        Assert.True(comparer.Equals(one, twin));
        Assert.False(comparer.Equals(one, other));
        Assert.False(comparer.Equals(one, null));
        Assert.True(comparer.Equals(null, null));
        Assert.Equal(one.GetHashCode(), comparer.GetHashCode(twin));

        // Symmetric and transitive across the constructor, the empty copy and the parts read back out.
        var copy = one with { };
        var (environment, nodes, regions, edges, frames, unplaced) = one;
        var rebuilt = new RuntimeManifest(environment, nodes, regions, edges, frames, unplaced);

        Assert.True(one.Equals(copy) && copy.Equals(one));
        Assert.True(copy.Equals(rebuilt) && rebuilt.Equals(copy));
        Assert.True(one.Equals(rebuilt));
    }

    [Fact]
    public void AManifestFindsItsEntryWhenAskedByAnEqualManifest()
    {
        var svgByManifest = new Dictionary<RuntimeManifest, string> { [Full] = "uat.svg", [Bare] = "uat-old.svg" };

        Assert.Equal("uat.svg", svgByManifest[new RuntimeManifest(Environment, Nodes, Regions, Edges, Frames, Unplaced)]);
        Assert.Equal("uat-old.svg", svgByManifest[new RuntimeManifest(Environment, Nodes, Regions, Edges)]);
        Assert.False(svgByManifest.ContainsKey(Full with { Environment = "UAT" }));
        Assert.False(svgByManifest.ContainsKey(Full with { Nodes = [.. Nodes] }));
    }

    [Fact]
    public void AManifestOfNoPartsFallsOutAsSixNothings()
    {
        var (environment, nodes, regions, edges, frames, unplaced) = new RuntimeManifest(null!, null!, null!, null!);

        Assert.Null(environment);
        Assert.Null(nodes);
        Assert.Null(regions);
        Assert.Null(edges);
        Assert.Null(frames);
        Assert.Null(unplaced);
    }

    [Fact]
    public void AManifestOfEmptyListsFallsOutAsTheVeryEmptyListsItWasHanded()
    {
        RuntimeNode[] noNodes = [];
        RuntimeRegion[] noRegions = [];
        RuntimeEdge[] noEdges = [];
        RuntimeFrame[] noFrames = [];
        RuntimeUnplaced[] noUnplaced = [];

        var (environment, nodes, regions, edges, frames, unplaced) =
            new RuntimeManifest(string.Empty, noNodes, noRegions, noEdges, noFrames, noUnplaced);

        Assert.Equal(string.Empty, environment);
        Assert.Same(noNodes, nodes);
        Assert.Same(noRegions, regions);
        Assert.Same(noEdges, edges);
        Assert.Same(noFrames, frames);
        Assert.Same(noUnplaced, unplaced);
    }

    // ------------------------------------------------------------------------------------------ RuntimeUnplaced

    [Fact]
    public void AnUnplacedResourceWithEveryPartNamedSaysItsListAndBothAddresses()
    {
        string[] ids = ["/subscriptions/0/x"];
        var unplaced = new RuntimeUnplaced(
            "stray",
            "Disk",
            "rg-demo",
            "tagged zzz: no such environment",
            ids,
            new Uri("https://portal.azure.com/#x"),
            new Uri("https://stray.example.net/"));

        const string Words =
            "RuntimeUnplaced { Name = stray, Type = Disk, Group = rg-demo, Words = tagged zzz: no such environment, "
            + "Ids = System.String[], Portal = https://portal.azure.com/#x, Site = https://stray.example.net/ }";

        Assert.Equal(Words, unplaced.ToString());

        // The words are the words to a reader of German too: no part is a number or a date.
        ViewerCulture.German(() => Assert.Equal(Words, unplaced.ToString()));
    }

    [Fact]
    public void AnUnplacedResourceOfNoNameIsHeldAndComparedAsOne()
    {
        var nameless = new RuntimeUnplaced(null!);

        Assert.Null(nameless.Name);
        Assert.Equal(nameless, new RuntimeUnplaced(null!));
        Assert.Equal(nameless.GetHashCode(), new RuntimeUnplaced(null!).GetHashCode());
        Assert.NotEqual(nameless, new RuntimeUnplaced(string.Empty));
        Assert.Equal(
            "RuntimeUnplaced { Name = , Type = , Group = , Words = , Ids = , Portal = , Site =  }",
            nameless.ToString());

        var (name, type, group, words, ids, portal, site) = nameless;

        Assert.Null(name);
        Assert.Null(type);
        Assert.Null(group);
        Assert.Null(words);
        Assert.Null(ids);
        Assert.Null(portal);
        Assert.Null(site);
    }

    [Fact]
    public void AnUnplacedResourceComparesItsTextOrdinallyWhateverTheReaderSpeaks()
    {
        var one = new RuntimeUnplaced("stray", "Disk", "rg-demo", "no environment tag");

        ViewerCulture.German(() =>
        {
            Assert.Equal(one, new RuntimeUnplaced("stray", "Disk", "rg-demo", "no environment tag"));
            Assert.NotEqual(one, one with { Name = "STRAY" });
            Assert.NotEqual(one, one with { Group = "RG-DEMO" });
            Assert.NotEqual(one, one with { Type = "disk" });
            Assert.NotEqual(one, one with { Words = "no environment tag " });
            Assert.NotEqual(new RuntimeUnplaced("ist"), new RuntimeUnplaced("ıst"));
        });
    }

    [Fact]
    public void TheHashOfAnUnplacedResourceStandsAndFollowsItsParts()
    {
        string[] ids = ["/a"];
        var one = new RuntimeUnplaced("stray", "Disk", "rg-demo", "no environment tag", ids, new Uri("https://portal.azure.com/#x"));
        var hash = one.GetHashCode();

        Assert.Equal(hash, one.GetHashCode());
        Assert.Equal(hash, (one with { }).GetHashCode());
        Assert.Equal(
            hash,
            new RuntimeUnplaced("stray", "Disk", "rg-demo", "no environment tag", ids, new Uri("https://portal.azure.com/#x")).GetHashCode());

        RuntimeUnplaced[] variants =
        [
            one,
            one with { },
            one with { Name = "other" },
            one with { Type = null },
            one with { Group = null },
            one with { Words = null },
            one with { Ids = null },
            one with { Portal = null },
            one with { Site = new Uri("https://stray.example.net/") },
        ];

        Assert.Equal(8, new HashSet<RuntimeUnplaced>(variants).Count);
    }

    [Fact]
    public void TheAddressesOfAnUnplacedResourceAreComparedAsUriComparesThem()
    {
        var one = new RuntimeUnplaced("stray", Portal: new Uri("https://portal.azure.com/#resource/a"), Site: new Uri("https://stray.example.net/"));

        // The host's case is no difference: one address.
        var shouted = one with { Site = new Uri("HTTPS://STRAY.EXAMPLE.NET/") };

        Assert.Equal(one, shouted);
        Assert.Equal(one.GetHashCode(), shouted.GetHashCode());

        // Uri weighs no fragment, so two portal pages that differ only after the '#' are one address to the record.
        var otherFragment = one with { Portal = new Uri("https://portal.azure.com/#resource/b") };

        Assert.Equal(one, otherFragment);
        Assert.Equal(one.GetHashCode(), otherFragment.GetHashCode());
        Assert.NotEqual(one.Portal!.ToString(), otherFragment.Portal!.ToString());

        // The path's case and a trailing slash are differences.
        Assert.NotEqual(one, one with { Site = new Uri("https://stray.example.net/App") });
        Assert.NotEqual(one with { Site = new Uri("https://stray.example.net/app") }, one with { Site = new Uri("https://stray.example.net/App") });
        Assert.NotEqual(one with { Site = new Uri("https://stray.example.net/app") }, one with { Site = new Uri("https://stray.example.net/app/") });
    }

    // ------------------------------------------------------------------------------------------ the machinery

    /// <summary>Equality as a generic collection asks it: through <see cref="IEquatable{T}" />, not the record's own
    /// overloads.</summary>
    private static bool EqualsAsEquatable<T>(T one, T? other)
        where T : IEquatable<T> => one.Equals(other);
}
