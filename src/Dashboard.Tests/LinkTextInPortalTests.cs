namespace Dashboard.Tests;

/// <summary>
/// The words <see cref="LinkText.InPortal" /> says of a subject: the plain phrase that names a resource by its page
/// in the Azure portal, with no suffix. The phrase is the one the heading-row label and the diagram's box link share;
/// this file pins it so a rewording is seen in a red test rather than in a diff.
/// </summary>
public class LinkTextInPortalTests
{
    [Fact]
    public void InPortalNamesTheSubjectInTheAzurePortal()
    {
        Assert.Equal("ca-api in the Azure portal", LinkText.InPortal("ca-api"));
    }

    [Fact]
    public void PortalOfInPortalMatchesTheTooltipViewLinksTestsPins()
    {
        // ViewLinksTests pins LinkText.Portal("ca-api in the Azure portal") for the heading-row tooltip.
        Assert.Equal(
            "ca-api in the Azure portal (opens in a new tab; the Azure portal asks you to sign in)",
            LinkText.Portal(LinkText.InPortal("ca-api")));
        Assert.Equal(LinkText.Portal("ca-api in the Azure portal"), LinkText.Portal(LinkText.InPortal("ca-api")));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("my app")]
    [InlineData("ca-api")]
    [InlineData("réseau-à-proximité")]
    [InlineData("The resource group rg-cmdemo1-nonprod")]
    public void TheSubjectIsCopiedWordForWord(string subject)
    {
        Assert.Equal($"{subject} in the Azure portal", LinkText.InPortal(subject));
    }

    [Fact]
    public void NoCultureSensitiveFormattingIsInvolved()
    {
        // InPortal is a plain interpolation: no Convert, no IFormattable, no culture.
        // A subject that would be culture-sensitive if formatted must come through unchanged.
        const string subject = "1,234.56";
        Assert.Equal("1,234.56 in the Azure portal", LinkText.InPortal(subject));
    }

    [Fact]
    public void InPortalHasNoSuffix()
    {
        // The phrase is the bare name, not the tooltip: no " (opens in a new tab; …)" suffix.
        Assert.DoesNotContain(LinkText.PortalSuffix, LinkText.InPortal("ca-api"));
        Assert.Equal("ca-api in the Azure portal", LinkText.InPortal("ca-api"));
    }
}
