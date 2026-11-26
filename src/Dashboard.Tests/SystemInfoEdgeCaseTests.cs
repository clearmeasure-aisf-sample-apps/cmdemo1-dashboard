using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The two files a system derives from its delivery facts, <see cref="SystemInfo.ContainersUrl" /> and
/// <see cref="SystemInfo.WorkItemsUrl" />, asked of the delivery addresses <see cref="TopologyEdgeCaseTests" />
/// leaves out: an address that names nothing but its host, a host with a port after it, a delivery file whose
/// name ends on a slash so the name stands as a directory, a dotted directory, an escaped directory that stays
/// escaped, a path with a dot segment the address machinery resolves away, an address said with the scheme and
/// the host in upper case, and a delivery address that is itself named <c>containers.json</c>.
/// <para>
/// The rest of <see cref="SystemInfo" /> is left alone. <see cref="TopologyEdgeCaseTests" /> pins the seven
/// parts, the equality, the falling apart, the copy and the words of the record, its reflection surface, and the
/// shapes of a delivery address this file leaves out - a path with a file, a bare file at the root, a path
/// ending on a slash, a path with no slash after its last word, a port after a path, a query, a fragment and
/// <c>http://localhost:8080</c> - with the two files derived beside them. <see cref="TopologyParserTests" /> pins
/// what the file becomes when it is read: the system the four parts of a topology stand on, and the addresses a
/// file may not name. <see cref="WorkItemsTests" /> pins the work items address beside the <c>delivery.json</c>
/// the file named and the nothing that stands when it names none. <see cref="DashboardBuildTests" /> and
/// <see cref="DeployableFactsTests" /> pin the dashboard the file names.
/// </para>
/// <para>
/// This file is where the record itself is pinned, as the house keeps a record: <see cref="DeployableInfo" /> and
/// <see cref="NodeInfo" /> have their own boundary tests, <see cref="DashboardInfo" /> has
/// <see cref="DashboardInfoEdgeCaseTests" />, and the section <see cref="TopologyEdgeCaseTests" /> carried for
/// <see cref="SystemInfo" /> stands here now - the constructor's seven parts each back as the property of its own
/// name, the five a caller may leave out standing absent, the words kept as they were given, the two derived files
/// beside the shapes of a delivery address a file really names, the two names apart, the files made anew at every
/// ask, two systems one while their seven parts are, any one part telling them apart, the seven parts read out in
/// their order, <c>with</c>, the words the system says about itself and its whole reflection surface. The surface
/// pin there asks the shared walks of <see cref="RecordSurface" />, as <see cref="TopologyEdgeCaseTests" /> now does
/// too; <see cref="RecordSurfaceMachineryTests" /> pins that the shared walk says what an independent walk of its own
/// says of this record, so the list pinned is the list pinned there. The same
/// thirteen pins stand in that file too until the follow-up of cmdemo1-dashboard#1225 walks past them: landing the
/// move dropped no pin and weakened none.
/// </para>
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: the addresses are named and never fetched,
/// and the two derived files are made of the address the record holds and of the two file words the record keeps
/// to itself.
/// </para>
/// </summary>
public class SystemInfoEdgeCaseTests
{
    private static readonly Uri Repository = new("https://github.example.net/org/demo-system");
    private static readonly Uri Delivery = new("https://raw.example.net/org/demo-system/status/delivery.json");
    private static readonly Uri Cost = new("https://raw.example.net/org/demo-system/status/cost.json");
    private static readonly Uri Deployments = new("https://raw.example.net/org/demo-system/deployments/deployments.json");

    private static readonly DashboardInfo Dashboard = new(DashboardInfo.DefaultName, "/build-facts.json");

    /// <summary>A system with every part named and the seven values apart from one another, so that an argument
    /// landing on another property is seen instead of quietly matching a second part of the record.</summary>
    private static readonly SystemInfo FullSystem = new("cmdemo1", "Demo system", Repository, Delivery, Cost, Dashboard, Deployments);

    /// <summary>A system named by nothing but its two required parts, the five a caller may leave out standing as
    /// they stand when nobody names them - and with no delivery facts to derive a file from.</summary>
    private static readonly SystemInfo BareSystem = new("demo", "Demo");

    /// <summary>
    /// Both files stand beside the delivery address as the address machinery makes them: an address that names
    /// nothing but its host - or its host with a port after it - puts both files in the site's root, with the
    /// slash the machinery puts there. A delivery file whose name ends on a slash is a directory, and the files
    /// are asked inside it: the name is kept as a word. A dotted directory is a directory, and an escaped
    /// directory stays escaped: the words of the path are not opened and not respelled. A dot segment in the path
    /// is resolved away before the files are asked, and an address said with the scheme and the host in upper
    /// case says the host in lower case while the path keeps its case. Where the delivery address is itself named
    /// <c>containers.json</c>, the containers file is the very same address, and the work items file stands
    /// beside it in its directory. The record asks the address machinery and decides nothing of its own: each
    /// derived address is what <see cref="Uri" /> says of the address it holds and of one of the two file words.
    /// </summary>
    [Theory]
    [InlineData("https://raw.example.net", "https://raw.example.net/containers.json", "https://raw.example.net/workitems.json")]
    [InlineData("https://raw.example.net:8443", "https://raw.example.net:8443/containers.json", "https://raw.example.net:8443/workitems.json")]
    [InlineData("https://h/a/delivery.json/", "https://h/a/delivery.json/containers.json", "https://h/a/delivery.json/workitems.json")]
    [InlineData("https://h/v1.0/delivery.json", "https://h/v1.0/containers.json", "https://h/v1.0/workitems.json")]
    [InlineData("https://h/a%20b/delivery.json", "https://h/a%20b/containers.json", "https://h/a%20b/workitems.json")]
    [InlineData("https://h/a/../b/delivery.json", "https://h/b/containers.json", "https://h/b/workitems.json")]
    [InlineData("https://RAW.example.net/A/Delivery.json", "https://raw.example.net/A/containers.json", "https://raw.example.net/A/workitems.json")]
    [InlineData("https://h/a/containers.json", "https://h/a/containers.json", "https://h/a/workitems.json")]
    public void TheTwoFilesStandBesideAnAddressThatNamesNothingButItsHost(string deliveryUrl, string containersUrl, string workItemsUrl)
    {
        var system = new SystemInfo("demo", "Demo", DeliveryUrl: new Uri(deliveryUrl));

        Assert.Equal(containersUrl, system.ContainersUrl!.AbsoluteUri);
        Assert.Equal(workItemsUrl, system.WorkItemsUrl!.AbsoluteUri);
    }

    /// <summary>
    /// Both files, asked of a delivery address that names no site: a path said with no scheme and no host, which
    /// the address machinery will not stand beside. <see cref="TopologyParser" /> only ever writes absolute
    /// addresses, and the nullable part of <see cref="SystemInfo.DeliveryUrl" /> lets a caller hand a relative one
    /// all the same: each of the two asks throws, with <c>baseUri</c> the name of what the machinery will not take.
    /// The record fills nothing in and repairs nothing - it holds the address as it was handed, and the same
    /// system with an absolute address answers both files, so the refusal is the address's, not a broken pair.
    /// </summary>
    [Fact]
    public void NeitherDerivedFileIsAskedOfADeliveryAddressThatNamesNoSite()
    {
        var deliveryUrl = new Uri("/a/b/delivery.json", UriKind.Relative);
        var system = new SystemInfo("demo", "Demo", DeliveryUrl: deliveryUrl);

        Assert.Equal("baseUri", Assert.Throws<ArgumentOutOfRangeException>(() => system.ContainersUrl).ParamName);
        Assert.Equal("baseUri", Assert.Throws<ArgumentOutOfRangeException>(() => system.WorkItemsUrl).ParamName);

        Assert.Equal(deliveryUrl, system.DeliveryUrl);

        var absolute = new SystemInfo("demo", "Demo", DeliveryUrl: new Uri("https://h/a/b/delivery.json"));
        Assert.Equal("https://h/a/b/containers.json", absolute.ContainersUrl!.AbsoluteUri);
        Assert.Equal("https://h/a/b/workitems.json", absolute.WorkItemsUrl!.AbsoluteUri);
    }

    /// <summary>
    /// The hash of one system, asked again and again: <see cref="TopologyEdgeCaseTests" /> pins that two equal
    /// systems hash alike, and it never asks of one system that its hash stands still - which is all this asks.
    /// A full system's <see cref="SystemInfo.GetHashCode()" /> says the same number from call to call, the boxed
    /// value is that same number, a copy the record makes with <c>with { }</c> and a system rebuilt from the same
    /// seven words hash alike with it, and a system handed nothing at all - <c>new SystemInfo(null!, null!)</c> -
    /// has a hash that stands still and matches a second such system: nothing about the parts, absent or present,
    /// makes hashing fall over on the nothing. The number itself is not the point; that it stands still is.
    /// </summary>
    [Fact]
    public void TheHashOfASystemStandsStillFromCallToCall()
    {
        var slug = "cmdemo1";
        var name = "Demo system";
        var repository = new Uri("https://github.example.net/org/demo-system");
        var delivery = new Uri("https://raw.example.net/org/demo-system/status/delivery.json");
        var cost = new Uri("https://raw.example.net/org/demo-system/status/cost.json");
        var dashboard = new DashboardInfo(DashboardInfo.DefaultName, "/build-facts.json");
        var deployments = new Uri("https://raw.example.net/org/demo-system/deployments/deployments.json");

        var system = new SystemInfo(slug, name, repository, delivery, cost, dashboard, deployments);

        var hash = system.GetHashCode();

        Assert.Equal(hash, system.GetHashCode());
        Assert.Equal(hash, ((object)system).GetHashCode());
        Assert.Equal(hash, (system with { }).GetHashCode());
        Assert.Equal(hash, new SystemInfo(slug, name, repository, delivery, cost, dashboard, deployments).GetHashCode());

        var nothing = new SystemInfo(null!, null!);
        var alsoNothing = new SystemInfo(null!, null!);

        Assert.Equal(nothing.GetHashCode(), nothing.GetHashCode());
        Assert.Equal(nothing.GetHashCode(), alsoNothing.GetHashCode());
    }

    /// <summary>
    /// The two members a caller really asks of a system - <see cref="SystemInfo.Equals" /> and
    /// <see cref="SystemInfo.GetHashCode" /> - asked together, the way a set and a dictionary ask them: two
    /// systems of the same seven words are one entry of a set, and a <see cref="Dictionary{TKey,TValue}" />
    /// written with one system answers the value when it is asked with an equal copy built from the same
    /// words. A copy that tells any one of the seven parts apart - <see cref="SystemInfo.Slug" />,
    /// <see cref="SystemInfo.Name" />, <see cref="SystemInfo.Repository" />, <see cref="SystemInfo.DeliveryUrl" />
    ///, <see cref="SystemInfo.CostUrl" />, <see cref="SystemInfo.Dashboard" /> or
    /// <see cref="SystemInfo.DeploymentsUrl" /> - stands eight apart from the original in the same set, and the
    /// system told nothing at all - <c>new SystemInfo(null!, null!)</c> - is a usable key that collapses with a
    /// second such system. The part-by-part inequality itself is pinned by
    /// <see cref="TopologyEdgeCaseTests.ASystemDiffersInAnyOneOfItsParts" />, and the hash standing still from
    /// call to call by <see cref="TheHashOfASystemStandsStillFromCallToCall" />.
    /// </summary>
    [Fact]
    public void ASystemIsOneKeyOfASetAndOneKeyOfADictionary()
    {
        var slug = "cmdemo1";
        var name = "Demo system";
        var repository = new Uri("https://github.example.net/org/demo-system");
        var delivery = new Uri("https://raw.example.net/org/demo-system/status/delivery.json");
        var cost = new Uri("https://raw.example.net/org/demo-system/status/cost.json");
        var dashboard = new DashboardInfo(DashboardInfo.DefaultName, "/build-facts.json");
        var deployments = new Uri("https://raw.example.net/org/demo-system/deployments/deployments.json");

        var system = new SystemInfo(slug, name, repository, delivery, cost, dashboard, deployments);

        // Two systems of the same seven words are one entry of a set, and a dictionary written with one
        // answers the value when it is asked with an equal copy built from the same words.
        Assert.Single(new HashSet<SystemInfo> { system, new SystemInfo(slug, name, repository, delivery, cost, dashboard, deployments) });

        var keyed = new Dictionary<SystemInfo, string> { [system] = "the system" };

        Assert.Single(keyed);
        Assert.Equal("the system", keyed[new SystemInfo(slug, name, repository, delivery, cost, dashboard, deployments)]);

        // The same set plus a copy of each of the seven parts told apart stands eight apart: the copy is of the
        // words, not of the reference, so the set weighs each part and keeps them apart.
        Assert.Equal(
            8,
            new HashSet<SystemInfo>
            {
                system,
                system with { Slug = "cmdemo2" },
                system with { Name = "Demo" },
                system with { Repository = null },
                system with { DeliveryUrl = new Uri("https://raw.example.net/elsewhere/delivery.json") },
                system with { CostUrl = null },
                system with { Dashboard = null },
                system with { DeploymentsUrl = new Uri("https://raw.example.net/elsewhere/deployments/deployments.json") },
            }.Count);

        // The all-nothing system is a usable key, and it collapses with a second such system.
        var nothing = new SystemInfo(null!, null!);
        var alsoNothing = new SystemInfo(null!, null!);

        Assert.Single(new HashSet<SystemInfo> { nothing, alsoNothing });
    }

    /// <summary>
    /// The two operators a caller writes - <c>==</c> and <c>!=</c> - asked of the nothing on either side.
    /// <see cref="TopologyEdgeCaseTests" /> pins <see cref="SystemInfo.Equals" /> against the nothing, against
    /// the word and against another record, and the operators between two systems, but it never puts the
    /// nothing on either side of <c>==</c> or <c>!=</c>. Here the operators are pinned from both sides: a
    /// system and the nothing are two - <c>==</c> saying false and <c>!=</c> saying true, with the nothing
    /// first as with the nothing last - a system is one with itself and with the hash it gives from call to
    /// call, and one with its <c>with { }</c> copy by <c>==</c> and <c>Equals</c> although the copy is not
    /// the same object, and the all-nothing system - <c>new SystemInfo(null!, null!)</c> - is a system and
    /// not the nothing it might be mistaken for: it is not <c>==</c> to the nothing.
    /// </summary>
    [Fact]
    public void TheTwoOperatorsAnswerForTheNothingOnEitherSide()
    {
        var slug = "cmdemo1";
        var name = "Demo system";
        var repository = new Uri("https://github.example.net/org/demo-system");
        var delivery = new Uri("https://raw.example.net/org/demo-system/status/delivery.json");
        var cost = new Uri("https://raw.example.net/org/demo-system/status/cost.json");
        var dashboard = new DashboardInfo(DashboardInfo.DefaultName, "/build-facts.json");
        var deployments = new Uri("https://raw.example.net/org/demo-system/deployments/deployments.json");

        var system = new SystemInfo(slug, name, repository, delivery, cost, dashboard, deployments);

        Assert.False(system == null);
        Assert.False(null == system);
        Assert.True(system != null);
        Assert.True(null != system);

        var hash = system.GetHashCode();
        var itself = system;

        Assert.True(system == itself);
        Assert.True(system.Equals(system));
        Assert.Equal(hash, system.GetHashCode());

        var copy = system with { };

        Assert.True(system == copy);
        Assert.True(system.Equals(copy));
        Assert.NotSame(system, copy);

        var nothing = new SystemInfo(null!, null!);

        Assert.False(nothing == null);
    }

    /// <summary>
    /// The dashboard a system stands on is weighed as its two words and not as the object they stand in. <see
    /// cref="TopologyEdgeCaseTests" /> pins that a dashboard of another BuildPath tells two systems apart, and that
    /// the dashboard comes back as the very instance it was handed - but it never asks of a second <see
    /// cref="DashboardInfo" /> of the one and the same two words that the two systems it stands on are one. Here
    /// that is pinned: a nested record is opened and weighed by what it holds, unlike the <see cref="LinkSet" /> of
    /// an environment, which <see cref="EnvironmentInfoEdgeCaseTests" /> pins as weighed by the hold it stands on. Two
    /// systems whose <see cref="SystemInfo.Dashboard" /> is a second <c>new DashboardInfo</c> of the same name and
    /// path are one - <c>==</c>, <c>Equals</c> and the hash code alike - although the two dashboard objects are not
    /// the same object, and a dashboard of the same name and another path is a second system.
    /// </summary>
    [Fact]
    public void TheDashboardPartIsWeighedAsItsTwoWordsAndNotAsTheObjectItStandsOn()
    {
        var slug = "cmdemo1";
        var name = "Demo system";
        var repository = new Uri("https://github.example.net/org/demo-system");
        var delivery = new Uri("https://raw.example.net/org/demo-system/status/delivery.json");
        var cost = new Uri("https://raw.example.net/org/demo-system/status/cost.json");
        var deployments = new Uri("https://raw.example.net/org/demo-system/deployments/deployments.json");
        var dashboard = new DashboardInfo(DashboardInfo.DefaultName, "/build-facts.json");

        var one = new SystemInfo(slug, name, repository, delivery, cost, dashboard, deployments);
        var two = new SystemInfo(slug, name, repository, delivery, cost, new DashboardInfo(DashboardInfo.DefaultName, "/build-facts.json"), deployments);

        Assert.True(one == two);
        Assert.Equal(one, two);
        Assert.True(one.Equals(two));
        Assert.Equal(one.GetHashCode(), two.GetHashCode());
        Assert.NotSame(one.Dashboard, two.Dashboard);

        Assert.NotEqual(one, one with { Dashboard = new DashboardInfo(DashboardInfo.DefaultName, "/facts.json") });
    }

    /// <summary>
    /// The writing side of the record the compiler made around the seven parts and the two files: <see
    /// cref="TopologyEdgeCaseTests" /> pins the names, the types, the constructor, the interfaces and the absence
    /// of a public static field, and this test pins the rest of them. The seven parts may be read, and may be
    /// written only as the system is made - each set accessor carries the <c>IsExternalInit</c> custom modifier,
    /// the mark of the <c>init</c> accessor, the only writing a record's <c>with</c> and its one constructor do -
    /// and the two files it derives may be read and not written: they have no set accessor at all, since nothing
    /// can be written into them. The type itself is the sealed record of values it holds, of <see cref="object" />
    /// and nothing else to inherit.
    /// </summary>
    [Fact]
    public void TheSevenPartsAreWrittenOnlyAsTheSystemIsMadeAndTheTwoFilesNever()
    {
        var properties = typeof(SystemInfo)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .ToDictionary(property => property.Name, StringComparer.Ordinal);

        Assert.All(
            ["Slug", "Name", "Repository", "DeliveryUrl", "CostUrl", "Dashboard", "DeploymentsUrl"],
            name =>
            {
                Assert.True(properties[name].CanRead);
                Assert.True(properties[name].CanWrite);
                Assert.Contains(
                    typeof(global::System.Runtime.CompilerServices.IsExternalInit),
                    properties[name].SetMethod!.ReturnParameter.GetRequiredCustomModifiers());
            });

        Assert.False(properties["ContainersUrl"].CanWrite);
        Assert.Null(properties["ContainersUrl"].GetSetMethod());
        Assert.False(properties["WorkItemsUrl"].CanWrite);
        Assert.Null(properties["WorkItemsUrl"].GetSetMethod());

        Assert.True(typeof(SystemInfo).IsSealed);
        Assert.Equal(typeof(object), typeof(SystemInfo).BaseType);
    }

    /// <summary>
    /// The system handed no words at all - <c>new SystemInfo(null!, null!)</c> - which <see cref="TopologyEdgeCaseTests" />
    /// leaves out: that file pins the line a system of the empty word says, with its two derived files named and
    /// left blank, and its seven parts falling apart as the nothings they are, but it never speaks of the system of
    /// nothing beside it. Here the nothing is pinned: it says itself exactly as the empty word does - the same
    /// line, the two derived files among its named parts since no delivery facts are derived from no delivery
    /// address - and it falls apart into its seven parts, each reading out as the nothing it is, so a nothing and
    /// an empty word are said the same way and neither crashes the record. The same line, with the two derived
    /// files said beside the delivery address and the rest left blank, stands for a system of nothing but a
    /// delivery address.
    /// </summary>
    [Fact]
    public void ASystemHandedNothingSaysItselfAndFallsApartAsTheNothingItIs()
    {
        var nothing = new SystemInfo(null!, null!);

        Assert.Equal(
            "SystemInfo { Slug = , Name = , Repository = , DeliveryUrl = , CostUrl = , Dashboard = , "
                + "DeploymentsUrl = , ContainersUrl = , WorkItemsUrl =  }",
            nothing.ToString());

        var (slug, name, repository, deliveryUrl, costUrl, dashboard, deploymentsUrl) = nothing;

        Assert.Null(slug);
        Assert.Null(name);
        Assert.Null(repository);
        Assert.Null(deliveryUrl);
        Assert.Null(costUrl);
        Assert.Null(dashboard);
        Assert.Null(deploymentsUrl);

        var onlyDelivery = new SystemInfo(null!, null!, DeliveryUrl: new Uri("https://h/a/delivery.json"));

        Assert.Equal(
            "SystemInfo { Slug = , Name = , Repository = , DeliveryUrl = https://h/a/delivery.json, CostUrl = , Dashboard = , "
                + "DeploymentsUrl = , ContainersUrl = https://h/a/containers.json, WorkItemsUrl = https://h/a/workitems.json }",
            onlyDelivery.ToString());
    }
}
