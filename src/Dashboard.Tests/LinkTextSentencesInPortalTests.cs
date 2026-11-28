namespace Dashboard.Tests;

/// <summary>
/// Pins the exact tooltip of the five portal keys in <see cref="LinkText.For"/>, and of
/// <see cref="ClusterText.PortalTitle"/>, both literally and as the <see cref="LinkText.Portal"/> of
/// <see cref="LinkText.InPortal"/> composition. After #2274 every one of those sentences is built through
/// <see cref="LinkText.InPortal"/>; this file keeps the output character-for-character unchanged.
/// </summary>
public class LinkTextSentencesInPortalTests
{
    // ── LinkText.Sentences portal rows ──────────────────────────────────────────────

    [Fact]
    public void PortalKeySaysTheWebAppInPortal()
    {
        Assert.Equal(
            "The web app tdd in the Azure portal (opens in a new tab; the Azure portal asks you to sign in)",
            LinkText.For(LinkSet.Portal, "tdd"));
        Assert.Equal(LinkText.Portal(LinkText.InPortal("The web app tdd")), LinkText.For(LinkSet.Portal, "tdd"));
    }

    [Fact]
    public void FrontDoorKeySaysTheFrontDoorProfileInPortal()
    {
        Assert.Equal(
            "The Front Door profile of tdd in the Azure portal (opens in a new tab; the Azure portal asks you to sign in)",
            LinkText.For(LinkSet.FrontDoor, "tdd"));
        Assert.Equal(LinkText.Portal(LinkText.InPortal("The Front Door profile of tdd")), LinkText.For(LinkSet.FrontDoor, "tdd"));
    }

    [Fact]
    public void ApplicationInsightsKeySaysApplicationInsightsInPortal()
    {
        Assert.Equal(
            "Application Insights of tdd in the Azure portal (opens in a new tab; the Azure portal asks you to sign in)",
            LinkText.For(LinkSet.ApplicationInsights, "tdd"));
        Assert.Equal(LinkText.Portal(LinkText.InPortal("Application Insights of tdd")), LinkText.For(LinkSet.ApplicationInsights, "tdd"));
    }

    [Fact]
    public void DatabaseKeySaysTheDatabaseInPortal()
    {
        Assert.Equal(
            "The database of tdd in the Azure portal (opens in a new tab; the Azure portal asks you to sign in)",
            LinkText.For(LinkSet.Database, "tdd"));
        Assert.Equal(LinkText.Portal(LinkText.InPortal("The database of tdd")), LinkText.For(LinkSet.Database, "tdd"));
    }

    [Fact]
    public void ResourceGroupKeySaysTheResourceGroupInPortal()
    {
        Assert.Equal(
            "The resource group of tdd in the Azure portal (opens in a new tab; the Azure portal asks you to sign in)",
            LinkText.For(LinkSet.ResourceGroup, "tdd"));
        Assert.Equal(LinkText.Portal(LinkText.InPortal("The resource group of tdd")), LinkText.For(LinkSet.ResourceGroup, "tdd"));
    }

    // ── ClusterText.PortalTitle ─────────────────────────────────────────────────────

    [Fact]
    public void PortalTitleForNullSaysTheAksClusterInPortal()
    {
        Assert.Equal(
            "The AKS cluster in the Azure portal (opens in a new tab; the Azure portal asks you to sign in)",
            ClusterText.PortalTitle(null));
        Assert.Equal(LinkText.Portal(LinkText.InPortal("The AKS cluster")), ClusterText.PortalTitle(null));
    }

    [Fact]
    public void PortalTitleForNamedClusterSaysTheAksClusterNameInPortal()
    {
        Assert.Equal(
            "The AKS cluster aks-x in the Azure portal (opens in a new tab; the Azure portal asks you to sign in)",
            ClusterText.PortalTitle("aks-x"));
        Assert.Equal(LinkText.Portal(LinkText.InPortal("The AKS cluster aks-x")), ClusterText.PortalTitle("aks-x"));
    }
}
