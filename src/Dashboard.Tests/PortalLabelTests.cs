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
}
