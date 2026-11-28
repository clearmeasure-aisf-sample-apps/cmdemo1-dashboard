namespace Dashboard.Tests;

/// <summary>
/// The title of a link to an address a browser opens: one rule, one owner - <see cref="RuntimeBoxLink.SiteTitle" /> -
/// that the box's diagram link and the heading row of its application view both read, so the two tooltips can never
/// disagree.
/// </summary>
public class RuntimeBoxLinkSiteTitleTests
{
    /// <summary>A plain https address says its host and nothing more: the full sentence, word for word.</summary>
    [Fact]
    public void TheTitleSaysTheNameAndTheHostOfAPlainHttpsAddress()
    {
        var address = new Uri("https://ca-web.example.io/");

        Assert.Equal("ca-web: ca-web.example.io, as a browser opens it (opens in a new tab)", RuntimeBoxLink.SiteTitle("ca-web", address));
    }

    /// <summary>Only the host is said, whatever the rest of the address is: a port, a path, a query, a fragment.</summary>
    [Fact]
    public void TheTitleSaysOnlyTheHostWhenTheAddressHasAPortPathQueryOrFragment()
    {
        var address = new Uri("https://ca-web.example.io:8443/a/b?x=1#frag");

        Assert.Equal("ca-web: ca-web.example.io, as a browser opens it (opens in a new tab)", RuntimeBoxLink.SiteTitle("ca-web", address));
    }

    /// <summary>An empty name is interpolated as it is: the sentence stands with no name before the colon.</summary>
    [Fact]
    public void TheTitleOfAnEmptyNameSaysNothingButTheColon()
    {
        var address = new Uri("https://demo.example.io/");

        Assert.Equal(": demo.example.io, as a browser opens it (opens in a new tab)", RuntimeBoxLink.SiteTitle("", address));
    }

    /// <summary>A relative address has no host: <see cref="Uri.Host" /> refuses it with its own exception, and the
    /// helper lets it - no guess, no argument of its own.</summary>
    [Fact]
    public void TheTitleOfARelativeAddressThrowsTheUrisOwnInvalidOperationException()
    {
        var relative = new Uri("/a/b", UriKind.Relative);

        Assert.Throws<InvalidOperationException>(() => RuntimeBoxLink.SiteTitle("ca-web", relative));
    }

    /// <summary>The box's diagram link and the application view's site link read the same title as the helper, so
    /// the diagram and the heading row of the view say one sentence of the same address.</summary>
    [Fact]
    public void TheDiagramsSiteLinkAndTheViewsSiteLinkCarryTheHelpersTitle()
    {
        var site = new Uri("https://ca-web.example.io/");
        var box = new RuntimeNode("app_web", RuntimeNodeKind.WebApp, "ca-web", Url: site, Links: Links((RuntimeBoxLink.Site, site)));

        Assert.Equal(RuntimeBoxLink.SiteTitle("ca-web", site), RuntimeBoxLink.Of(box)!.Title);
        Assert.Equal(RuntimeBoxLink.SiteTitle("ca-web", site), ApplicationText.Links([box], null)[0].Title);
    }

    /// <summary>The links a test gives a box of the diagram, by key.</summary>
    private static LinkSet Links(params (string Key, Uri Address)[] links) =>
        new(links.Select(link => new KeyValuePair<string, Uri>(link.Key, link.Address)));
}
