namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="ContainerAppTarget" />, a container app as the
/// runtime view probes it. <see cref="ContainerAppProbeTests" /> pins that the page asks the health path of every
/// probe - the default and the declared, for both probe kinds - and <see cref="RuntimePayloadHelperTests" /> pins
/// the target the payload builder makes, asked at the path its deployable names; neither pins the target's own
/// parts. What is pinned here: the word the default path owns; that the target keeps the address and the path it
/// was given - the unnamed, the empty and the full; the path each probe asks when the deployable names a path of
/// its own or names the empty one; when the answer at that path reads as a health check; and how the target
/// compares, copies, reads out and says its name.
/// </summary>
public class ContainerAppTargetEdgeCaseTests
{
    /// <summary>The address <see cref="ContainerAppProbeTests" /> asks: the app its own sample shows.</summary>
    private static readonly Uri App = new("https://ca-demo-tdd-ui.graybush-1.southcentralus.azurecontainerapps.io");

    // ----- the fixed value and the parts the target keeps

    /// <summary>
    /// The default health path is the one word the page asks when no deployable names one: the page asks that
    /// path, never the root, so the word itself stands here where a change of it is a change the tests see.
    /// </summary>
    [Fact]
    public void TheDefaultHealthPathIsThePathAskedWhenNoDeployableNamesOne()
    {
        Assert.Equal("/_healthcheck", ContainerAppTarget.DefaultHealthPath);
    }

    /// <summary>
    /// The target keeps the address and the path it was given, each in its own part: the path the deployable
    /// named, the path it named that names nothing, and the path it did not name at all.
    /// </summary>
    [Fact]
    public void TheTargetKeepsTheAddressAndPathItWasGiven()
    {
        var named = new ContainerAppTarget(App, "/alive");
        var namedNothing = new ContainerAppTarget(App, "");
        var unnamed = new ContainerAppTarget(App);

        Assert.Equal(App, named.Url);
        Assert.Equal("https://ca-demo-tdd-ui.graybush-1.southcentralus.azurecontainerapps.io/", named.Url.AbsoluteUri);
        Assert.Equal("/alive", named.HealthPath);
        Assert.Equal("", namedNothing.HealthPath);
        Assert.Null(unnamed.HealthPath);
        Assert.Equal(App, unnamed.Url);
    }

    /// <summary>
    /// The address stands on the very Uri the target was given: the port the address carries, and the path and
    /// query it carries, each standing as the Uri stands.
    /// </summary>
    [Fact]
    public void TheAddressStandsOnTheUriTheTargetWasGiven()
    {
        var withPort = new Uri("https://app.example.net:8443");
        var withPathAndQuery = new Uri("https://app.example.net/probe?source=dashboard");

        Assert.Equal(withPort, new ContainerAppTarget(withPort).Url);
        Assert.Equal("https://app.example.net:8443/", new ContainerAppTarget(withPort).Url.AbsoluteUri);
        Assert.Equal("https://app.example.net/probe?source=dashboard", new ContainerAppTarget(withPathAndQuery).Url.AbsoluteUri);
    }

    // ----- the path the page asks

    /// <summary>
    /// The effective health path is the path the deployable named: a name of its own, the default spelled again
    /// as a name, and the empty and the whitespace the deployable may still name - the default stands in only
    /// for the unnamed, which <see cref="RuntimePayloadHelperTests" /> pins.
    /// </summary>
    [Theory]
    [InlineData("/alive", "/alive")]
    [InlineData("/_healthcheck", "/_healthcheck")]
    [InlineData("", "")]
    [InlineData(" ", " ")]
    public void ThePathTheDeployableNamedIsThePathThePageAsks(string healthPath, string effective)
    {
        Assert.Equal(effective, new ContainerAppTarget(App, healthPath).EffectiveHealthPath);
    }

    /// <summary>
    /// Every probe asks the effective path, whichever probe it is: the deployable's own name, and the empty name
    /// the deployable may still give - the page asks the empty path rather than the root, and never the default.
    /// The unnamed path, and the name the builder gives, <see cref="ContainerAppProbeTests" /> and
    /// <see cref="RuntimePayloadHelperTests" /> pin.
    /// </summary>
    [Theory]
    [InlineData(ProbeKind.Health, "/alive", "/alive")]
    [InlineData(ProbeKind.Liveness, "/alive", "/alive")]
    [InlineData(ProbeKind.Health, "", "")]
    [InlineData(ProbeKind.Liveness, "", "")]
    public void EveryProbeAsksTheEffectivePathWhichProbeItIs(ProbeKind kind, string healthPath, string asked)
    {
        Assert.Equal(asked, new ContainerAppTarget(App, healthPath).PathFor(kind));
    }

    /// <summary>
    /// The answer reads as a health check while the path it stands on is not empty: the named path and the
    /// whitespace name read, and the empty path does not - the page asked it and cannot tell its health. The
    /// unnamed path reads, which <see cref="ContainerAppProbeTests" /> pins.
    /// </summary>
    [Theory]
    [InlineData(ProbeKind.Health, "/alive", true)]
    [InlineData(ProbeKind.Liveness, "/alive", true)]
    [InlineData(ProbeKind.Health, " ", true)]
    [InlineData(ProbeKind.Liveness, " ", true)]
    [InlineData(ProbeKind.Health, "", false)]
    [InlineData(ProbeKind.Liveness, "", false)]
    public void TheAnswerReadsAsAHealthCheckWhileThePathItStandsOnIsNotEmpty(ProbeKind kind, string healthPath, bool reads)
    {
        Assert.Equal(reads, new ContainerAppTarget(App, healthPath).ReadsHealth(kind));
    }

    // ----- the record itself

    /// <summary>Two targets are one while their address and path are: the equality of the operator and of
    /// <c>Equals</c>, and the hash code that follows them, standing where it stands from call to call.</summary>
    [Fact]
    public void TwoTargetsAreOneWhileTheirAddressAndPathAre()
    {
        var one = new ContainerAppTarget(App, "/alive");
        var two = new ContainerAppTarget(new Uri("https://ca-demo-tdd-ui.graybush-1.southcentralus.azurecontainerapps.io"), "/alive");

        Assert.True(one == two);
        Assert.True(one.Equals((object)two));
        Assert.True(two.Equals((object)one));
        Assert.Equal(one, two);
        Assert.Equal(one.GetHashCode(), two.GetHashCode());
        Assert.Equal(one.GetHashCode(), one.GetHashCode());
        Assert.False(one.Equals(null));
        Assert.False(one.Equals("ContainerAppTarget"));
    }

    /// <summary>A target differs in either of its parts: an address on another host, a path in another spelling,
    /// and the unnamed, the empty and the whitespace apart from one another and from the named.</summary>
    [Fact]
    public void ATargetDiffersInEitherOfItsParts()
    {
        var target = new ContainerAppTarget(App, "/alive");

        Assert.True(target != target with { Url = new Uri("https://other.example.net") });
        Assert.NotEqual(target, target with { Url = new Uri("https://other.example.net") });
        Assert.NotEqual(target, target with { HealthPath = null });
        Assert.NotEqual(target, target with { HealthPath = "" });
        Assert.NotEqual(target, target with { HealthPath = " " });
        Assert.NotEqual(target, target with { HealthPath = "/_alive" });

        Assert.NotEqual(new ContainerAppTarget(App), new ContainerAppTarget(App, ""));
        Assert.NotEqual(new ContainerAppTarget(App, ""), new ContainerAppTarget(App, " "));
    }

    /// <summary>The unnamed path and the default spelled out are one target (#585): the page asks the same path of
    /// both, so the operator, <c>Equals</c>, the hash code, a dictionary and <c>Distinct</c> all see one app - and
    /// each keeps the path it was given.</summary>
    [Fact]
    public void TheUnnamedPathAndTheDefaultSpelledOutAreOneTarget()
    {
        var unnamed = new ContainerAppTarget(App);
        var spelled = new ContainerAppTarget(App, ContainerAppTarget.DefaultHealthPath);

        Assert.True(unnamed == spelled);
        Assert.False(unnamed != spelled);
        Assert.True(unnamed.Equals((object)spelled));
        Assert.True(spelled.Equals((object)unnamed));
        Assert.Equal(unnamed.GetHashCode(), spelled.GetHashCode());
        Assert.Single(new[] { unnamed, spelled }.Distinct());

        var byTarget = new Dictionary<ContainerAppTarget, string> { [unnamed] = "first" };
        Assert.Equal("first", byTarget[spelled]);

        Assert.Null(unnamed.HealthPath);
        Assert.Equal(ContainerAppTarget.DefaultHealthPath, spelled.HealthPath);
        Assert.NotEqual(unnamed, new ContainerAppTarget(App, ""));
        Assert.NotEqual(spelled, new ContainerAppTarget(new Uri("https://other.example.net"), ContainerAppTarget.DefaultHealthPath));
    }

    /// <summary><c>with</c> names the part it changes and leaves the rest: the moved address keeps its path, the
    /// unnamed path falls back to the default, and the emptied path stands emptied - equal and not the same in
    /// the copy that names no part.</summary>
    [Fact]
    public void WithChangesOnlyThePartItNames()
    {
        var target = new ContainerAppTarget(App, "/alive");
        var copy = target with { };

        Assert.Equal(target, copy);
        Assert.NotSame(target, copy);

        var moved = target with { Url = new Uri("https://other.example.net") };
        Assert.Equal("other.example.net", moved.Url.Host);
        Assert.Equal("/alive", moved.HealthPath);

        var unnamed = target with { HealthPath = null };
        Assert.Null(unnamed.HealthPath);
        Assert.Equal(ContainerAppTarget.DefaultHealthPath, unnamed.EffectiveHealthPath);

        var emptied = target with { HealthPath = "" };
        Assert.Equal("", emptied.EffectiveHealthPath);
        Assert.False(emptied.ReadsHealth(ProbeKind.Health));
    }

    /// <summary>The two parts read out of the target in the order they stand in it - the unnamed and the empty
    /// path reading out as they stand.</summary>
    [Fact]
    public void TheTwoPartsReadOutOfTheTarget()
    {
        var (url, path) = new ContainerAppTarget(App, "/alive");
        Assert.Equal(App, url);
        Assert.Equal("/alive", path);

        var (movedUrl, emptyPath) = new ContainerAppTarget(new Uri("https://other.example.net"), "");
        Assert.Equal("other.example.net", movedUrl.Host);
        Assert.Equal("", emptyPath);
    }

    /// <summary>The target says its parts in their order, the name of each with the value it stands on - the
    /// path it did not name says its name and nothing else, and the effective path says what the page asks.
    /// (The values are written out as they are, so a rewording of a part is a change the tests see.)</summary>
    [Fact]
    public void TheTargetSaysItsPartsInTheirOrder()
    {
        Assert.Equal(
            "ContainerAppTarget { Url = https://ca-demo-tdd-ui.graybush-1.southcentralus.azurecontainerapps.io/, HealthPath = /alive, EffectiveHealthPath = /alive }",
            new ContainerAppTarget(App, "/alive").ToString());
        Assert.Equal(
            "ContainerAppTarget { Url = https://ca-demo-tdd-ui.graybush-1.southcentralus.azurecontainerapps.io/, HealthPath = , EffectiveHealthPath = /_healthcheck }",
            new ContainerAppTarget(App).ToString());
    }

    // ----- the ownership this file keeps for the target's members (#586)

    /// <summary>
    /// The unnamed target asks the default path, for every kind of probe, and reads its health: the path and the
    /// reading the other files pin of the target they probe stand here where the target's own parts belong, so the
    /// probe's walk of its answers and the watch's walk of its apps pin only what is theirs - the probe asking,
    /// the watch keeping - not the target answering again.
    /// </summary>
    [Theory]
    [InlineData(ProbeKind.Health)]
    [InlineData(ProbeKind.Liveness)]
    public void TheUnnamedTargetAsksTheDefaultPathAndReadsItsHealth(ProbeKind kind)
    {
        var unnamed = new ContainerAppTarget(App);

        Assert.Equal(ContainerAppTarget.DefaultHealthPath, unnamed.EffectiveHealthPath);
        Assert.Equal(ContainerAppTarget.DefaultHealthPath, unnamed.PathFor(kind));
        Assert.True(unnamed.ReadsHealth(kind));
    }

    /// <summary>
    /// The target's parts stand on any address it is given, not only the one its own sample shows: on another host
    /// the declared path is the effective path, the probe asks it, the answer reads as health, and the unnamed
    /// target on the other host says the default - the spellings the watch's edge cases pin of its targets stand
    /// here, on the target's own address, so a rewording of the record is a change this file alone sees.
    /// </summary>
    [Fact]
    public void TheTargetsPartsStandOnAnyAddressTheyAreGiven()
    {
        var demo = new Uri("https://ca-demo.example.io");
        var named = new ContainerAppTarget(demo, "/health");
        var unnamed = new ContainerAppTarget(demo);

        Assert.Equal("/health", named.EffectiveHealthPath);
        Assert.Equal("/health", named.PathFor(ProbeKind.Health));
        Assert.Equal("/health", named.PathFor(ProbeKind.Liveness));
        Assert.True(named.ReadsHealth(ProbeKind.Health));
        Assert.True(named.ReadsHealth(ProbeKind.Liveness));
        Assert.Equal(ContainerAppTarget.DefaultHealthPath, unnamed.EffectiveHealthPath);
        Assert.Equal(ContainerAppTarget.DefaultHealthPath, unnamed.PathFor(ProbeKind.Health));
        Assert.True(unnamed.ReadsHealth(ProbeKind.Liveness));

        Assert.Equal(
            "ContainerAppTarget { Url = https://ca-demo.example.io/, HealthPath = /health, EffectiveHealthPath = /health }",
            named.ToString());
        Assert.Equal(
            "ContainerAppTarget { Url = https://ca-demo.example.io/, HealthPath = , EffectiveHealthPath = /_healthcheck }",
            unnamed.ToString());
    }
}
