namespace Dashboard.Tests;

/// <summary>
/// The three sentences <see cref="LinkText.Group" /> says of an environment's group of links: its resources in the
/// Azure portal, its pages of the system, or both. Pinned here, character for character, so a rewording is seen in a
/// red test rather than in a diff. The <c>(false, false)</c> arm falls back to the resources sentence.
/// </summary>
public class LinkTextGroupTests
{
    [Fact]
    public void LinksAndPagesNamesBoth()
    {
        Assert.Equal(
            "Resources of tdd in the Azure portal, and pages of the system there",
            LinkText.Group("tdd", hasResources: true, hasPages: true));
        Assert.Equal(
            "Resources of uat in the Azure portal, and pages of the system there",
            LinkText.Group("uat", hasResources: true, hasPages: true));
    }

    [Fact]
    public void PagesOnlyNamesThePages()
    {
        Assert.Equal("Pages of the system in tdd", LinkText.Group("tdd", hasResources: false, hasPages: true));
        Assert.Equal("Pages of the system in uat", LinkText.Group("uat", hasResources: false, hasPages: true));
    }

    [Fact]
    public void ResourcesOnlyNamesTheResources()
    {
        Assert.Equal("Resources of tdd in the Azure portal", LinkText.Group("tdd", hasResources: true, hasPages: false));
        Assert.Equal("Resources of uat in the Azure portal", LinkText.Group("uat", hasResources: true, hasPages: false));
    }

    [Fact]
    public void NeitherFallsBackToTheResourcesSentence()
    {
        Assert.Equal("Resources of uat in the Azure portal", LinkText.Group("uat", hasResources: false, hasPages: false));
    }

    [Fact]
    public void TheResourcesArmsEqualInPortalOfTheResourcesSubject()
    {
        Assert.StartsWith(LinkText.InPortal("Resources of uat"), LinkText.Group("uat", hasResources: true, hasPages: true));
        Assert.StartsWith(LinkText.InPortal("Resources of uat"), LinkText.Group("uat", hasResources: true, hasPages: false));
        Assert.StartsWith(LinkText.InPortal("Resources of uat"), LinkText.Group("uat", hasResources: false, hasPages: false));
    }
}
