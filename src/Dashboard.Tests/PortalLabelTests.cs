using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The heading-row label of a link to the Azure portal has one home, <c>LinkText.OpenInPortal</c>: the database's view
/// and the app's view of one box both read it. The constant is internal (the app declares no
/// <c>InternalsVisibleTo</c>), so it is found by reflection.
/// </summary>
public class PortalLabelTests
{
    private static string OpenInPortal()
    {
        var field = typeof(LinkText).GetField("OpenInPortal", BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(field);
        Assert.True(field!.IsLiteral);
        Assert.True(field.IsAssembly);
        return (string)field.GetRawConstantValue()!;
    }

    private static LinkSet Links(string key, string address) =>
        new([KeyValuePair.Create(key, new Uri(address))]);

    [Fact]
    public void TheLabelSaysOpenInTheAzurePortal()
    {
        Assert.Equal("Open in the Azure portal", OpenInPortal());
    }

    [Fact]
    public void TheDatabaseViewsPortalLinkReadsTheOneLabel()
    {
        var box = new RuntimeNode("sqldb", RuntimeNodeKind.Sql, "sql-uat");

        var link = Assert.Single(DatabaseText.Links(box, Links(LinkSet.Database, "https://portal.example.net/#sql-uat")));

        Assert.Equal(OpenInPortal(), link.Text);
        Assert.EndsWith(LinkText.PortalSuffix, link.Title, StringComparison.Ordinal);
    }

    [Fact]
    public void TheApplicationViewOfOneBoxReadsTheOneLabelForItsPortalLink()
    {
        var box = new RuntimeNode(
            "ca_api", RuntimeNodeKind.ContainerApp, "ca-api", new Uri("https://ca-api.example.io"), "api",
            Links: Links(LinkSet.Portal, "https://portal.example.net/#ca-api"));

        var portal = Assert.Single(ApplicationText.Links([box], null), link => link.Href == "https://portal.example.net/#ca-api");

        Assert.Equal(OpenInPortal(), portal.Text);
    }

    private static string OpenTheApp()
    {
        var field = typeof(LinkText).GetField("OpenTheApp", BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(field);
        Assert.True(field!.IsLiteral);
        Assert.True(field.IsAssembly);
        return (string)field.GetRawConstantValue()!;
    }

    private static string Open(string subject)
    {
        var method = typeof(LinkText).GetMethod("Open", BindingFlags.NonPublic | BindingFlags.Static, [typeof(string)]);

        Assert.NotNull(method);
        Assert.True(method.IsAssembly);
        return (string)method.Invoke(null, [subject])!;
    }

    [Fact]
    public void TheSiteLinkLabelSaysOpenTheApp()
    {
        Assert.Equal("Open the app", OpenTheApp());
    }

    [Fact]
    public void TheBoxNamingLabelSaysOpenSubject()
    {
        Assert.Equal("Open demo-ui", Open("demo-ui"));
    }

    [Fact]
    public void TheDatabaseViewNonPortalLinkReadsTheBoxNamingLabel()
    {
        // A database box whose only link is a site link (no portal suffix): the label names the box.
        var box = new RuntimeNode("sqldb", RuntimeNodeKind.Sql, "sql-uat", Links: Links(RuntimeBoxLink.Site, "https://sql.example.io"));

        var link = Assert.Single(DatabaseText.Links(box, null));

        Assert.Equal("Open sql-uat", link.Text);
    }

    [Fact]
    public void TheOneBoxApplicationViewSiteLinkReadsOpenTheApp()
    {
        var box = new RuntimeNode(
            "ca_api", RuntimeNodeKind.ContainerApp, "ca-api", new Uri("https://ca-api.example.io"), "api");

        var link = Assert.Single(ApplicationText.Links([box], null));

        Assert.Equal("Open the app", link.Text);
    }

    [Fact]
    public void TheMultiBoxApplicationViewSiteLinksNameEachBox()
    {
        var boxes = new[]
        {
            new RuntimeNode("ca_api", RuntimeNodeKind.ContainerApp, "ca-api", new Uri("https://ca-api.example.io"), "api"),
            new RuntimeNode("ca_ui", RuntimeNodeKind.ContainerApp, "ca-ui", new Uri("https://ca-ui.example.io"), "api", "primary"),
        };

        var links = ApplicationText.Links(boxes, null);

        Assert.Equal(
            boxes.Select(box => $"Open {box.Name}"),
            links.Select(link => link.Text));
    }
}
