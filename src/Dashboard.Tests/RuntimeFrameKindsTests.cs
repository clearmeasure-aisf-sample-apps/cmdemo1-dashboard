using Dashboard.Health;
using Dashboard.Runtime;

namespace Dashboard.Tests;

/// <summary>
/// The six words a frame's kind can be: the vocabulary of <see cref="RuntimeFrameKinds"/>, held once and said by
/// <see cref="RuntimeBoxLink"/> as the one place the word is compared. What is pinned here is that the six constants
/// are the six words the deployment writes - distinct, and none of them empty - and that a frame of each of the six,
/// led to the Azure portal, is said by the words of its kind: the five that name a resource in the portal are said
/// by their words and the frame's name, and the sixth, <see cref="RuntimeFrameKinds.Runtime"/>, is said by the
/// frame's name alone, as a word the vocabulary does not know. The words a kind has of its own are the portal's
/// alone, and what the other five keys of the six say of a frame is pinned in <see cref="RuntimeBoxLinkEdgeCaseTests"/>;
/// what a frame of a kind the vocabulary does not know - another casing, a space, the empty word - says of itself
/// is pinned there all the same, so a frame of one of the six spelled another way is linked as an unknown kind, and
/// this file only pins the six as the six words they are.
/// </summary>
public sealed class RuntimeFrameKindsTests
{
    private const string PortalHint = " (opens in a new tab; the Azure portal asks you to sign in)";

    private static readonly Uri Portal = new("https://portal.example.net/#/overview");
    private static readonly Uri Octopus = new("https://octopus.example.net/app#/Spaces-1/projects/demo");
    private static readonly Uri Site = new("https://demo.example.io/");

    /// <summary>The six constants are the six words the deployment writes, as it writes them - the casing is the
    /// word: <c>resourceGroup</c> and <c>frontDoorProfile</c> are camel, and the read compares them ordinal, so a
    /// changed casing here is a changed word, and a frame of that kind is linked as an unknown kind.</summary>
    [Theory]
    [InlineData(RuntimeFrameKinds.Subscription, "subscription")]
    [InlineData(RuntimeFrameKinds.ResourceGroup, "resourceGroup")]
    [InlineData(RuntimeFrameKinds.FrontDoorProfile, "frontDoorProfile")]
    [InlineData(RuntimeFrameKinds.Plan, "plan")]
    [InlineData(RuntimeFrameKinds.Cluster, "cluster")]
    [InlineData(RuntimeFrameKinds.Runtime, "runtime")]
    public void EachConstantIsTheWordTheDeploymentWritesIt(string kind, string word) =>
        Assert.Equal(word, kind);

    /// <summary>The six are six different words, and none of them is the empty word or a word of nothing but
    /// spaces: the empty word is what the parser gives a frame that names no kind, and it must stay apart from the
    /// six, or a frame without a kind would be a frame of one of them.</summary>
    [Fact]
    public void TheSixWordsAreSixDifferentWordsNoneOfThemEmpty()
    {
        var words = new[]
        {
            RuntimeFrameKinds.Subscription,
            RuntimeFrameKinds.ResourceGroup,
            RuntimeFrameKinds.FrontDoorProfile,
            RuntimeFrameKinds.Plan,
            RuntimeFrameKinds.Cluster,
            RuntimeFrameKinds.Runtime,
        };

        Assert.Equal(6, words.Length);
        Assert.Equal(6, words.Distinct(StringComparer.Ordinal).Count());
        Assert.All(words, word => Assert.False(string.IsNullOrWhiteSpace(word)));
    }

    /// <summary>A frame of one of the five resource kinds, led to the Azure portal, is said by the words of its
    /// kind and the frame's name: the subscription's words stand alone, and the other four put the name after
    /// them. These are the five the read knows of its own; the sixth is the next test.</summary>
    [Theory]
    [InlineData(RuntimeFrameKinds.Subscription, "sub-cmdemo1", "The Azure subscription in the Azure portal" + PortalHint)]
    [InlineData(RuntimeFrameKinds.ResourceGroup, "rg-cmdemo1", "The resource group rg-cmdemo1 in the Azure portal" + PortalHint)]
    [InlineData(RuntimeFrameKinds.FrontDoorProfile, "fd-cmdemo1", "The Front Door profile fd-cmdemo1 in the Azure portal" + PortalHint)]
    [InlineData(RuntimeFrameKinds.Plan, "asp-cmdemo1", "The App Service plan asp-cmdemo1 in the Azure portal" + PortalHint)]
    [InlineData(RuntimeFrameKinds.Cluster, "aks-cmdemo1", "The AKS cluster aks-cmdemo1 in the Azure portal" + PortalHint)]
    public void AFrameOfOneOfTheFiveResourceKindsIsSaidByTheWordsOfItsKind(string kind, string name, string title)
    {
        var frame = new RuntimeFrame("demo", kind, name, Links((LinkSet.Portal, Portal)));
        Assert.Equal(title, TitleOf(RuntimeBoxLink.Of(frame)));
    }

    /// <summary>The sixth word, <see cref="RuntimeFrameKinds.Runtime"/>, is one the deployment writes - the boundary
    /// of an application with its own runtime - and the read says it with the frame's name alone, as a word the
    /// vocabulary does not know: no words of its own, the name and the portal, all the same a link. This is the
    /// fall-through the switch has no arm for, pinned so a future arm for it is a conscious change of the title,
    /// not a quiet one.</summary>
    [Fact]
    public void AFrameOfTheSixthWordIsSaidWithItsNameAloneAsAWordTheVocabularyDoesNotKnow()
    {
        var frame = new RuntimeFrame("own_web", RuntimeFrameKinds.Runtime, "web-cmdemo1", Links((LinkSet.Portal, Portal)));
        Assert.Equal("web-cmdemo1 in the Azure portal" + PortalHint, TitleOf(RuntimeBoxLink.Of(frame)));
    }

    /// <summary>A frame of any of the six, led to a key that is not the portal, is said by its name as it is
    /// drawn, as a frame of a kind the vocabulary does not know: the words a kind has of its own are the portal's
    /// alone, and the other five keys of the six say the name. Pinned for the project key, the one a frame of the
    /// sixth word most often leads to, and for the site key, the one that says the host.</summary>
    [Theory]
    [InlineData(RuntimeFrameKinds.Runtime)]
    [InlineData(RuntimeFrameKinds.Subscription)]
    [InlineData(RuntimeFrameKinds.Cluster)]
    public void AFrameOfAnyOfTheSixLedToANonPortalKeyIsSaidByItsNameAlone(string kind)
    {
        var projectFrame = new RuntimeFrame("own_web", kind, "web-cmdemo1", Links((RuntimeBoxLink.Project, Octopus)));
        Assert.Equal(
            "The project that deploys web-cmdemo1 in Octopus Deploy (opens in a new tab; Octopus Deploy asks you to sign in)",
            TitleOf(RuntimeBoxLink.Of(projectFrame)));

        var siteFrame = new RuntimeFrame("own_web", kind, "web-cmdemo1", Links((RuntimeBoxLink.Site, Site)));
        Assert.Equal(
            "web-cmdemo1: demo.example.io, as a browser opens it (opens in a new tab)",
            TitleOf(RuntimeBoxLink.Of(siteFrame)));
    }

    private static LinkSet Links(params (string Key, Uri Address)[] links) =>
        new(links.Select(link => new KeyValuePair<string, Uri>(link.Key, link.Address)));

    private static string TitleOf(RuntimeLink? link)
    {
        var value = Assert.IsType<RuntimeLink>(link);
        return value.Title;
    }
}
