namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="SystemInfo" /> that the files beside it leave open,
/// declared in <c>src/Dashboard/Health/Topology.cs</c>. <see cref="TopologyEdgeCaseTests" /> pins the seven parts,
/// equality, deconstruction, the copy, the words of the record and its reflection surface, and the two derived files
/// beside the common shapes of a delivery address; <see cref="SystemInfoEdgeCaseTests" /> pins a bare host, a port, a
/// directory named like a file, dotted, escaped and dot-segment paths, upper case, a relative address, the hash, the
/// operators and the init-only accessors.
/// <para>
/// What none of those says, and what follows: the two derived files of <see cref="SystemInfo.ContainersUrl" /> and
/// <see cref="SystemInfo.WorkItemsUrl" /> beside a <c>file</c> address, an IPv6 host, a user name before the host, an
/// explicit default port, an empty path segment, a path with letters outside ASCII and a deep path; that the derived
/// files keep the scheme, the host and the port of the delivery address and nothing of its query; and that the empty
/// and blank words of <see cref="SystemInfo.Slug" /> and <see cref="SystemInfo.Name" /> are held as handed and decide
/// nothing of the two files.
/// </para>
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: every address is named and never fetched.
/// </para>
/// </summary>
public class SystemInfoBoundaryEdgeCaseTests
{
    private static SystemInfo Delivering(string deliveryUrl) => new("demo", "Demo", DeliveryUrl: new Uri(deliveryUrl));

    /// <summary>
    /// Both files stand beside delivery addresses of the shapes the other files leave out: a <c>file</c> address, an
    /// IPv6 host, a user name before the host, a default port said out loud (which the address machinery drops), an
    /// empty segment in the path (which is kept), a path segment with letters outside ASCII (which stays escaped) and
    /// a <c>http</c> address on its default port.
    /// </summary>
    [Theory]
    [InlineData("file:///srv/status/delivery.json", "file:///srv/status/containers.json", "file:///srv/status/workitems.json")]
    [InlineData("https://[::1]/status/delivery.json", "https://[::1]/status/containers.json", "https://[::1]/status/workitems.json")]
    [InlineData("https://reader@h/status/delivery.json", "https://reader@h/status/containers.json", "https://reader@h/status/workitems.json")]
    [InlineData("https://h:443/status/delivery.json", "https://h/status/containers.json", "https://h/status/workitems.json")]
    [InlineData("http://h:80/status/delivery.json", "http://h/status/containers.json", "http://h/status/workitems.json")]
    [InlineData("https://h/a//delivery.json", "https://h/a//containers.json", "https://h/a//workitems.json")]
    [InlineData("https://h/caf%C3%A9/delivery.json", "https://h/caf%C3%A9/containers.json", "https://h/caf%C3%A9/workitems.json")]
    public void TheTwoFilesStandBesideAddressesOfRarerShapes(string deliveryUrl, string containersUrl, string workItemsUrl)
    {
        var system = Delivering(deliveryUrl);

        Assert.Equal(containersUrl, system.ContainersUrl!.AbsoluteUri);
        Assert.Equal(workItemsUrl, system.WorkItemsUrl!.AbsoluteUri);
    }

    /// <summary>
    /// The derived files keep the scheme, the host and the port of the delivery address, are absolute like it, and
    /// carry neither its query nor its fragment.
    /// </summary>
    [Fact]
    public void TheTwoFilesKeepTheSiteOfTheDeliveryAddressAndDropItsQuery()
    {
        var delivery = new Uri("http://raw.example.net:8080/org/status/delivery.json?sig=abc#top");
        var system = new SystemInfo("demo", "Demo", DeliveryUrl: delivery);

        foreach (var derived in new[] { system.ContainersUrl!, system.WorkItemsUrl! })
        {
            Assert.True(derived.IsAbsoluteUri);
            Assert.Equal("http", derived.Scheme);
            Assert.Equal("raw.example.net", derived.Host);
            Assert.Equal(8080, derived.Port);
            Assert.Equal(string.Empty, derived.Query);
            Assert.Equal(string.Empty, derived.Fragment);
        }

        Assert.Equal("/org/status/containers.json", system.ContainersUrl!.AbsolutePath);
        Assert.Equal("/org/status/workitems.json", system.WorkItemsUrl!.AbsolutePath);
    }

    /// <summary>
    /// A delivery address forty directories deep: both files stand in the fortieth directory, and the path keeps
    /// every one of the forty.
    /// </summary>
    [Fact]
    public void TheTwoFilesStandBesideADeepDeliveryAddress()
    {
        var directories = string.Join('/', Enumerable.Range(1, 40).Select(index => "d" + index));
        var system = Delivering($"https://h/{directories}/delivery.json");

        Assert.Equal($"https://h/{directories}/containers.json", system.ContainersUrl!.AbsoluteUri);
        Assert.Equal($"https://h/{directories}/workitems.json", system.WorkItemsUrl!.AbsoluteUri);
        Assert.Equal(42, system.ContainersUrl.Segments.Length);
        Assert.Equal(42, system.WorkItemsUrl.Segments.Length);
    }

    /// <summary>
    /// The empty and the blank word for <see cref="SystemInfo.Slug" /> and <see cref="SystemInfo.Name" /> are held as
    /// handed - not trimmed, not turned into the nothing - and the two files are asked of the delivery address alone:
    /// with one they are the same whatever the words, without one they are both the nothing.
    /// </summary>
    [Theory]
    [InlineData("", "")]
    [InlineData(" ", "\t")]
    [InlineData("  cmdemo1  ", "  Demo system  ")]
    public void EmptyAndBlankWordsAreHeldAsHandedAndDecideNothingOfTheTwoFiles(string slug, string name)
    {
        var bare = new SystemInfo(slug, name);

        Assert.Equal(slug, bare.Slug);
        Assert.Equal(name, bare.Name);
        Assert.Null(bare.ContainersUrl);
        Assert.Null(bare.WorkItemsUrl);

        var delivering = bare with { DeliveryUrl = new Uri("https://h/status/delivery.json") };

        Assert.Equal(slug, delivering.Slug);
        Assert.Equal(name, delivering.Name);
        Assert.Equal("https://h/status/containers.json", delivering.ContainersUrl!.AbsoluteUri);
        Assert.Equal("https://h/status/workitems.json", delivering.WorkItemsUrl!.AbsoluteUri);
    }

    /// <summary>
    /// The empty word and the nothing are two systems: a slug of <c>""</c> is not a slug of <see langword="null" />,
    /// and neither is a name - although both say themselves the same way.
    /// </summary>
    [Fact]
    public void TheEmptyWordAndTheNothingAreTwoSystems()
    {
        var empty = new SystemInfo(string.Empty, string.Empty);

        Assert.NotEqual(empty, new SystemInfo(null!, string.Empty));
        Assert.NotEqual(empty, new SystemInfo(string.Empty, null!));
        Assert.Equal(empty.ToString(), new SystemInfo(null!, null!).ToString());
    }
}
