namespace Dashboard.Tests;

/// <summary>
/// The three sentences <see cref="LinkText.EnvironmentGroup" /> says of an environment's group of links: its
/// resources in the Azure portal, its pages of the system, or both. Pinned here, character for character, so a
/// rewording is seen in a red test rather than in a diff. The <c>(false, false)</c> arm falls back to the resources
/// sentence.
/// </summary>
public class LinkTextEnvironmentGroupTests
{
    [Fact]
    public void LinksAndPagesNamesBoth()
    {
        Assert.Equal(
            "Resources of tdd in the Azure portal, and pages of the system there",
            LinkText.EnvironmentGroup("tdd", hasLinks: true, hasPages: true));
        Assert.Equal(
            "Resources of uat in the Azure portal, and pages of the system there",
            LinkText.EnvironmentGroup("uat", hasLinks: true, hasPages: true));
    }

    [Fact]
    public void PagesOnlyNamesThePages()
    {
        Assert.Equal("Pages of the system in tdd", LinkText.EnvironmentGroup("tdd", hasLinks: false, hasPages: true));
        Assert.Equal("Pages of the system in uat", LinkText.EnvironmentGroup("uat", hasLinks: false, hasPages: true));
    }

    [Fact]
    public void ResourcesOnlyNamesTheResources()
    {
        Assert.Equal("Resources of tdd in the Azure portal", LinkText.EnvironmentGroup("tdd", hasLinks: true, hasPages: false));
        Assert.Equal("Resources of uat in the Azure portal", LinkText.EnvironmentGroup("uat", hasLinks: true, hasPages: false));
    }

    [Fact]
    public void NeitherFallsBackToTheResourcesSentence()
    {
        Assert.Equal("Resources of uat in the Azure portal", LinkText.EnvironmentGroup("uat", hasLinks: false, hasPages: false));
    }

    [Fact]
    public void TheResourcesArmsStartWithInPortalOfTheResourcesSubject()
    {
        Assert.StartsWith(LinkText.InPortal("Resources of uat"), LinkText.EnvironmentGroup("uat", hasLinks: true, hasPages: true));
        Assert.StartsWith(LinkText.InPortal("Resources of uat"), LinkText.EnvironmentGroup("uat", hasLinks: true, hasPages: false));
        Assert.StartsWith(LinkText.InPortal("Resources of uat"), LinkText.EnvironmentGroup("uat", hasLinks: false, hasPages: false));
    }

    [Fact]
    public void NullEnvironmentThrows()
    {
        Assert.Throws<ArgumentNullException>(() => LinkText.EnvironmentGroup(null!, hasLinks: true, hasPages: true));
        Assert.Throws<ArgumentNullException>(() => LinkText.EnvironmentGroup(null!, hasLinks: false, hasPages: false));
    }
}
