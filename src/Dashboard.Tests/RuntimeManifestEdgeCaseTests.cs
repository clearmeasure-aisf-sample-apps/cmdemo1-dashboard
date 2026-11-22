using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="RuntimeManifest" />: the record of
/// <c>runtime/&lt;env&gt;.json</c>, the environment's name and the four lists of what its diagram draws - nodes,
/// regions, edges, frames - and the fifth of what it lists beneath (<see cref="RuntimeUnplaced" />). The record
/// decides nothing of its own, so what is pinned here is that each part comes back as the part it went in as, that the
/// two parts a caller may leave out stand as nothing (and not as an empty list), that the lists are held and compared
/// as the very lists they are, and the members every record is asked.
/// <para>
/// What the other files already pin is left alone: <see cref="RuntimeManifestParserTests" /> and
/// <see cref="RuntimeManifestParserEdgeCaseTests" /> pin the read out of the file, and
/// <see cref="RuntimeNodeEdgeCaseTests" />, <see cref="RuntimeRegionEdgeCaseTests" />,
/// <see cref="RuntimeEdgeEdgeCaseTests" /> and <see cref="RuntimeFrameEdgeCaseTests" /> pin the records the lists
/// hold. <see cref="RuntimeUnplaced" /> has no file of its own, so its record is pinned here too.
/// </para>
/// <para>Nothing here reaches the network or asks a clock: every value below is one the test brought.</para>
/// </summary>
public class RuntimeManifestEdgeCaseTests
{
    private const string Environment = "uat";

    private static readonly RuntimeNode[] Nodes = [new("web_primary", RuntimeNodeKind.WebApp, "app-uat")];
    private static readonly RuntimeRegion[] Regions = [new("region_primary", "southcentralus", ["primary"])];
    private static readonly RuntimeEdge[] Edges = [new("browser-to-web_primary", "browser", "web_primary", RuntimeEdgeKind.Public)];
    private static readonly RuntimeFrame[] Frames = [new("sub", "subscription", "demo")];
    private static readonly RuntimeUnplaced[] Unplaced = [new("stray", "Storage account", "rg-demo", "no environment tag")];

    /// <summary>A manifest with every part named, each list a different one, so a part landing on another property is
    /// seen instead of quietly matching.</summary>
    private static RuntimeManifest Full => new(Environment, Nodes, Regions, Edges, Frames, Unplaced);

    /// <summary>A manifest of the four parts a caller must name: one from before frames and unplaced were listed.</summary>
    private static RuntimeManifest Bare => new(Environment, Nodes, Regions, Edges);

    // ------------------------------------------------------------------------------------------ RuntimeManifest

    [Fact]
    public void TheConstructorHandsEachPartToThePropertyOfItsName()
    {
        var manifest = Full;

        Assert.Equal(Environment, manifest.Environment);
        Assert.Same(Nodes, manifest.Nodes);
        Assert.Same(Regions, manifest.Regions);
        Assert.Same(Edges, manifest.Edges);
        Assert.Same(Frames, manifest.Frames);
        Assert.Same(Unplaced, manifest.Unplaced);
    }

    [Fact]
    public void FramesAndUnplacedLeftOutAreNothingAndNotAnEmptyList()
    {
        var bare = Bare;

        Assert.Null(bare.Frames);
        Assert.Null(bare.Unplaced);
        Assert.Equal(bare, new RuntimeManifest(Environment, Nodes, Regions, Edges, null, null));

        // An empty list is a list the file gave: another manifest than one without it.
        Assert.NotEqual(bare, bare with { Frames = [] });
        Assert.NotEqual(bare, bare with { Unplaced = [] });
        Assert.Empty((bare with { Frames = [] }).Frames!);
        Assert.Empty((bare with { Unplaced = [] }).Unplaced!);
    }

    [Fact]
    public void AManifestOfNothingDrawnIsAManifest()
    {
        var empty = new RuntimeManifest(string.Empty, [], [], [], [], []);

        Assert.Equal(string.Empty, empty.Environment);
        Assert.Empty(empty.Nodes);
        Assert.Empty(empty.Regions);
        Assert.Empty(empty.Edges);
        Assert.Empty(empty.Frames!);
        Assert.Empty(empty.Unplaced!);

        // The record refuses nothing: the non-nullable parts handed a null are kept as null.
        var nothing = new RuntimeManifest(null!, null!, null!, null!);

        Assert.Null(nothing.Environment);
        Assert.Null(nothing.Nodes);
        Assert.Null(nothing.Regions);
        Assert.Null(nothing.Edges);
        Assert.Equal(nothing, new RuntimeManifest(null!, null!, null!, null!));
        Assert.Equal(nothing.GetHashCode(), new RuntimeManifest(null!, null!, null!, null!).GetHashCode());
    }

    [Theory]
    [InlineData("uat")]
    [InlineData("  uat  ")]
    [InlineData("UAT")]
    [InlineData("u")]
    [InlineData("prod-eastus2")]
    [InlineData("démo 日本")]
    public void TheEnvironmentIsKeptExactlyAsGiven(string environment)
    {
        var manifest = Bare with { Environment = environment };

        Assert.Equal(environment, manifest.Environment);
        Assert.Equal(environment.Length, manifest.Environment.Length);
    }

    [Fact]
    public void TheEnvironmentIsComparedOrdinallyWhateverTheReaderSpeaks()
    {
        ViewerCulture.German(() =>
        {
            Assert.NotEqual(Bare, Bare with { Environment = "UAT" });
            Assert.NotEqual(Bare, Bare with { Environment = "uat " });
            Assert.NotEqual(Bare with { Environment = "ist" }, Bare with { Environment = "ıst" });
            Assert.Equal(Bare, Bare with { Environment = "uat" });
        });
    }

    [Fact]
    public void TheListsAreComparedAsTheListsTheyAreAndNotByWhatTheyHold()
    {
        // The same nodes in another list: the record weighs the list, not its items.
        RuntimeNode[] sameNodes = [.. Nodes];

        Assert.Equal(Nodes, sameNodes);
        Assert.NotEqual(Bare, Bare with { Nodes = sameNodes });
        Assert.NotEqual(Bare, Bare with { Regions = [.. Regions] });
        Assert.NotEqual(Bare, Bare with { Edges = [.. Edges] });
        Assert.NotEqual(Full, Full with { Frames = [.. Frames] });
        Assert.NotEqual(Full, Full with { Unplaced = [.. Unplaced] });

        // The very same lists: one manifest, of one hash.
        var again = new RuntimeManifest(Environment, Nodes, Regions, Edges, Frames, Unplaced);

        Assert.Equal(Full, again);
        Assert.True(Full == again);
        Assert.False(Full != again);
        Assert.Equal(Full.GetHashCode(), again.GetHashCode());
        Assert.Single(new HashSet<RuntimeManifest> { Full, again });
    }

    [Fact]
    public void AManifestDiffersInAnyOneOfItsSixParts()
    {
        var full = Full;

        Assert.NotEqual(full, full with { Environment = "prod" });
        Assert.NotEqual(full, full with { Nodes = [] });
        Assert.NotEqual(full, full with { Regions = [] });
        Assert.NotEqual(full, full with { Edges = [] });
        Assert.NotEqual(full, full with { Frames = null });
        Assert.NotEqual(full, full with { Unplaced = null });

        Assert.False(full.Equals(null));
        Assert.False(full.Equals((object?)null));
        Assert.False(full.Equals((object)"uat"));
        Assert.True(full.Equals((object)Full));
        Assert.False(full == null);
        Assert.True(full != null);
        Assert.True((RuntimeManifest?)null == null);
    }

    [Fact]
    public void WithChangesOnlyThePartItNamesAndSharesTheRest()
    {
        var full = Full;
        RuntimeNode[] other = [new("db", RuntimeNodeKind.Sql, "sql-uat")];

        var copy = full with { Nodes = other };

        Assert.Same(other, copy.Nodes);
        Assert.Equal(Environment, copy.Environment);
        Assert.Same(Regions, copy.Regions);
        Assert.Same(Edges, copy.Edges);
        Assert.Same(Frames, copy.Frames);
        Assert.Same(Unplaced, copy.Unplaced);

        // The original is untouched, and a copy with nothing changed is the same manifest but another object.
        Assert.Same(Nodes, full.Nodes);
        var same = full with { };

        Assert.Equal(full, same);
        Assert.NotSame(full, same);
    }

    [Fact]
    public void TheSixPartsFallOutInTheOrderTheyWentIn()
    {
        var (environment, nodes, regions, edges, frames, unplaced) = Full;

        Assert.Equal(Environment, environment);
        Assert.Same(Nodes, nodes);
        Assert.Same(Regions, regions);
        Assert.Same(Edges, edges);
        Assert.Same(Frames, frames);
        Assert.Same(Unplaced, unplaced);

        var (_, _, _, _, noFrames, noUnplaced) = Bare;

        Assert.Null(noFrames);
        Assert.Null(noUnplaced);
    }

    [Fact]
    public void AManifestSaysItsPartsInTheirOrder()
    {
        var manifest = new RuntimeManifest("uat", Array.Empty<RuntimeNode>(), Array.Empty<RuntimeRegion>(), Array.Empty<RuntimeEdge>());

        Assert.Equal(
            "RuntimeManifest { Environment = uat, Nodes = Dashboard.Runtime.RuntimeNode[], "
            + "Regions = Dashboard.Runtime.RuntimeRegion[], Edges = Dashboard.Runtime.RuntimeEdge[], Frames = , Unplaced =  }",
            manifest.ToString());
    }

    [Fact]
    public void TheManifestHasSixPartsAndTheMembersEveryRecordIsAsked()
    {
        var type = typeof(RuntimeManifest);
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        Assert.Equal(
            ["Environment", "Nodes", "Regions", "Edges", "Frames", "Unplaced"],
            [.. properties.Select(property => property.Name)]);
        Assert.Equal(
            [typeof(string), typeof(IReadOnlyList<RuntimeNode>), typeof(IReadOnlyList<RuntimeRegion>),
                typeof(IReadOnlyList<RuntimeEdge>), typeof(IReadOnlyList<RuntimeFrame>), typeof(IReadOnlyList<RuntimeUnplaced>)],
            [.. properties.Select(property => property.PropertyType)]);
        Assert.Equal(
            ["Deconstruct", "Edges", "Environment", "Equals", "Frames", "GetHashCode", "Nodes", "Regions", "ToString",
                "Unplaced", "op_Equality", "op_Inequality"],
            PublicSurface(type));

        var constructor = Assert.Single(type.GetConstructors());
        var parameters = constructor.GetParameters();

        Assert.Equal(
            ["Environment", "Nodes", "Regions", "Edges", "Frames", "Unplaced"],
            [.. parameters.Select(parameter => parameter.Name!)]);
        Assert.Equal([false, false, false, false, true, true], [.. parameters.Select(parameter => parameter.HasDefaultValue)]);
        Assert.Null(parameters[4].DefaultValue);
        Assert.Null(parameters[5].DefaultValue);

        var nullability = new NullabilityInfoContext();

        Assert.Equal(
            [false, false, false, false, true, true],
            [.. parameters.Select(parameter => nullability.Create(parameter).WriteState == NullabilityState.Nullable)]);

        Assert.True(type.IsSealed);
        Assert.Equal([typeof(IEquatable<RuntimeManifest>)], type.GetInterfaces());
    }

    // ------------------------------------------------------------------------------------------ RuntimeUnplaced

    [Fact]
    public void AnUnplacedResourceNamedByItsNameAloneHasNothingElse()
    {
        var bare = new RuntimeUnplaced("stray");

        Assert.Equal("stray", bare.Name);
        Assert.Null(bare.Type);
        Assert.Null(bare.Group);
        Assert.Null(bare.Words);
        Assert.Null(bare.Ids);
        Assert.Null(bare.Portal);
        Assert.Null(bare.Site);
        Assert.Equal(bare, new RuntimeUnplaced("stray", null, null, null, null, null, null));
    }

    [Fact]
    public void AnUnplacedResourceKeepsEachPartAsGiven()
    {
        string[] ids = ["/subscriptions/0/resourceGroups/rg-demo/providers/Microsoft.Storage/storageAccounts/stray"];
        var portal = new Uri("https://portal.azure.com/#resource/stray");
        var site = new Uri("https://stray.example.net/");

        var unplaced = new RuntimeUnplaced("  stray  ", "Storage account", "rg-demo", "tagged zzz: no such environment", ids, portal, site);

        Assert.Equal("  stray  ", unplaced.Name);
        Assert.Equal("Storage account", unplaced.Type);
        Assert.Equal("rg-demo", unplaced.Group);
        Assert.Equal("tagged zzz: no such environment", unplaced.Words);
        Assert.Same(ids, unplaced.Ids);
        Assert.Same(portal, unplaced.Portal);
        Assert.Same(site, unplaced.Site);

        var (name, type, group, words, heldIds, heldPortal, heldSite) = unplaced;

        Assert.Equal("  stray  ", name);
        Assert.Equal("Storage account", type);
        Assert.Equal("rg-demo", group);
        Assert.Equal("tagged zzz: no such environment", words);
        Assert.Same(ids, heldIds);
        Assert.Same(portal, heldPortal);
        Assert.Same(site, heldSite);

        // Untrusted text is held as it stands: the record escapes nothing.
        Assert.Equal("<b>x</b>", new RuntimeUnplaced("<b>x</b>").Name);
        Assert.Equal(string.Empty, new RuntimeUnplaced(string.Empty, string.Empty).Type);
    }

    [Fact]
    public void AnUnplacedResourceComparesAddressesByValueAndIdsByList()
    {
        string[] ids = ["/a"];
        var one = new RuntimeUnplaced("stray", Ids: ids, Portal: new Uri("https://portal.azure.com/#x"));
        var twin = new RuntimeUnplaced("stray", Ids: ids, Portal: new Uri("https://portal.azure.com/#x"));

        // Two Uri objects of one address are one address.
        Assert.Equal(one, twin);
        Assert.Equal(one.GetHashCode(), twin.GetHashCode());

        // The same ids in another list are another resource.
        Assert.NotEqual(one, one with { Ids = ["/a"] });
        Assert.NotEqual(one, one with { Ids = null });
        Assert.NotEqual(one, one with { Ids = [] });

        Assert.NotEqual(one, one with { Name = "Stray" });
        Assert.NotEqual(one, one with { Type = string.Empty });
        Assert.NotEqual(one, one with { Group = "rg" });
        Assert.NotEqual(one, one with { Words = "no environment tag" });
        Assert.NotEqual(one, one with { Portal = null });
        Assert.NotEqual(one, one with { Site = new Uri("https://stray.example.net/") });
        Assert.False(one.Equals(null));
    }

    [Fact]
    public void AnUnplacedResourceSaysItsPartsInTheirOrder()
    {
        Assert.Equal(
            "RuntimeUnplaced { Name = stray, Type = , Group = , Words = , Ids = , Portal = , Site =  }",
            new RuntimeUnplaced("stray").ToString());
        Assert.Equal(
            "RuntimeUnplaced { Name = stray, Type = Disk, Group = rg-demo, Words = no environment tag, Ids = , "
            + "Portal = https://portal.azure.com/#x, Site =  }",
            new RuntimeUnplaced("stray", "Disk", "rg-demo", "no environment tag", Portal: new Uri("https://portal.azure.com/#x")).ToString());
    }

    [Fact]
    public void TheUnplacedResourceHasSevenPartsAndOnlyItsNameIsRequired()
    {
        var type = typeof(RuntimeUnplaced);

        Assert.Equal(
            ["Name", "Type", "Group", "Words", "Ids", "Portal", "Site"],
            [.. type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Select(property => property.Name)]);

        var parameters = Assert.Single(type.GetConstructors()).GetParameters();

        Assert.Equal(
            [typeof(string), typeof(string), typeof(string), typeof(string), typeof(IReadOnlyList<string>), typeof(Uri), typeof(Uri)],
            [.. parameters.Select(parameter => parameter.ParameterType)]);
        Assert.Equal([false, true, true, true, true, true, true], [.. parameters.Select(parameter => parameter.HasDefaultValue)]);
        Assert.All(parameters.Skip(1), parameter => Assert.Null(parameter.DefaultValue));
        Assert.True(type.IsSealed);
    }

    // ------------------------------------------------------------------------------------------ the machinery

    /// <summary>The public members a type declares itself: its properties and its methods that are not a property's
    /// get or set, the two operators kept, the compiler-named clone and print walk past.</summary>
    private static List<string> PublicSurface(Type type) =>
        [.. type
            .GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(member => member is PropertyInfo
                             || member is MethodInfo method
                             && (!method.IsSpecialName || method.Name.StartsWith("op_", StringComparison.Ordinal)))
            .Where(member => !member.Name.Contains('<', StringComparison.Ordinal))
            .Select(member => member.Name)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];
}
