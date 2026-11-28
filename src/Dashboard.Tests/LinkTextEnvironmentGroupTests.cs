namespace Dashboard.Tests;

/// <summary>
/// Pins the three sentences <see cref="LinkText.EnvironmentGroup"/> says of an environment's resource-links group,
/// character for character, and that the two portal arms are built through <see cref="LinkText.InPortal"/>. The rule
/// that picks a sentence from whether there are links and pages used to live in Razor markup where no unit test could
/// reach it; this file pins it in C#.
/// </summary>
public class LinkTextEnvironmentGroupTests
{
    [Fact]
    public void LinksAndPagesSaysResourcesAndPages()
    {
        Assert.Equal(
            "Resources of tdd in the Azure portal, and pages of the system there",
            LinkText.EnvironmentGroup("tdd", hasLinks: true, hasPages: true));
    }

    [Fact]
    public void PagesOnlySaysPagesOfTheSystem()
    {
        Assert.Equal(
            "Pages of the system in tdd",
            LinkText.EnvironmentGroup("tdd", hasLinks: false, hasPages: true));
    }

    [Fact]
    public void LinksOnlySaysResourcesInPortal()
    {
        Assert.Equal(
            "Resources of tdd in the Azure portal",
            LinkText.EnvironmentGroup("tdd", hasLinks: true, hasPages: false));
    }

    [Fact]
    public void NeitherLinksNorPagesSaysResourcesInPortal()
    {
        // The (false, false) arm never renders behind the @if (HasLinks || HasPages) guard, but it must keep its answer.
        Assert.Equal(
            "Resources of tdd in the Azure portal",
            LinkText.EnvironmentGroup("tdd", hasLinks: false, hasPages: false));
    }

    [Fact]
    public void BothPortalArmsStartWithInPortalOfResourcesOf()
    {
        const string environment = "tdd";
        string expected = LinkText.InPortal("Resources of tdd");

        Assert.Equal(expected, LinkText.EnvironmentGroup(environment, hasLinks: true, hasPages: false));
        Assert.StartsWith(expected, LinkText.EnvironmentGroup(environment, hasLinks: true, hasPages: true));
        Assert.EndsWith(", and pages of the system there", LinkText.EnvironmentGroup(environment, hasLinks: true, hasPages: true));
    }

    [Fact]
    public void NullEnvironmentThrows()
    {
        Assert.Throws<ArgumentNullException>(
            () => LinkText.EnvironmentGroup(null!, hasLinks: true, hasPages: true));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("uat")]
    [InlineData("prod")]
    [InlineData("réseau-à-proximité")]
    public void TheEnvironmentNameIsCopiedWordForWord(string environment)
    {
        string result = LinkText.EnvironmentGroup(environment, hasLinks: true, hasPages: false);
        Assert.StartsWith($"Resources of {environment}", result);
        Assert.EndsWith(" in the Azure portal", result);
    }
}
