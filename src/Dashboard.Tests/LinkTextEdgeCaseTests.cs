namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="LinkText" /> - the words a link's tooltip is made of
/// and the one address the page builds for itself - in the parts the tests of the page leave unsaid.
/// <para>
/// <see cref="LinkText" /> holds no moment and calls nothing: every member of it is a string out of the strings it is
/// handed, or a <see cref="Uri" /> out of the <see cref="Uri" /> and the version it is handed, so nothing here needs
/// the network and nothing here asks a clock what time it is. Each case below brings its own value and says the
/// words or the address that value produces today - the awkward ones included, because the day a wording moves is a
/// change a reader of the page hears and has to be seen in a red test rather than in a diff.
/// </para>
/// <para>
/// What is pinned where it belongs, and is therefore not written twice: the three suffixes themselves, word for word,
/// in <see cref="EdgeCaseTextTests" /> and against the hints of <see cref="RuntimeWorkHelperCoverageTests" />;
/// <c>Portal</c> of no name and of a name, and <c>For</c> of the key that names nothing, of <c>portal</c> with its
/// first letter in the wrong place, of <c>workloads</c> - a key <c>LinkSet</c> names and <c>For</c> does not - and of
/// <c>0</c>, all in <see cref="EdgeCaseTextTests" />; the words of <c>For</c> for <c>liveMetrics</c>,
/// <c>dependencies</c>, <c>failures</c> and <c>portal</c>, each written out at length in
/// <see cref="TargetTileClusterCardEnvironmentDeliveryTests" /> and <see cref="EdgeCaseTextTests" />; that every known
/// key is answered, says its subject and ends with the portal's promise, and that no two keys answer alike, in
/// <see cref="LinkTests" />; <c>Release</c> without its project, without its version, with a version of no letters and
/// with one of nothing but spaces, its project's one trailing slash and its version escaped, in
/// <see cref="EdgeCaseTextTests" /> and <see cref="LinkTests" />; <c>ReleaseTitle</c> of two empties and of
/// <c>ui 2.4.15</c>, in <see cref="EdgeCaseTextTests" /> and <see cref="LinkTests" />; and the seven characters of
/// <c>CommitTitle</c> - one under the cut, on it, one over it, the empty SHA, and no string at all refused by its own
/// name - in <see cref="EdgeCaseTextTests" /> and <see cref="LinkTextHelperTests" />. Nothing of that is restated.
/// </para>
/// <para>
/// Left open by all of it, and pinned here: <c>Portal</c> and <c>For</c> of a name that is not there at all rather
/// than merely empty, and of a name of one space; the words of the seven keys no test has ever written out -
/// <c>performance</c>, <c>frontDoor</c>, <c>logs</c>, <c>applicationInsights</c>, <c>applicationMap</c>,
/// <c>database</c> and <c>resourceGroup</c> - and one of those tooltips end to end, suffix and all; that a subject is
/// said as it stands and escaped by nobody; <c>Release</c> from a project address of no path, of one path and a
/// hundred trailing slashes, of a port, of another scheme and of another case, the last segment of its path falling
/// to the dot-dot of a version that is no version but a path; a version that is a tab, a newline or a space no
/// keyboard writes, and one that is already escaped, quoted or padded and is said as the escaped thing it is; and
/// <c>ReleaseTitle</c> of a name and a version that are not there, which the tooltip says in the two spaces they leave
/// behind.
/// </para>
/// </summary>
public class LinkTextEdgeCaseTests
{
    /// <summary>The resource every tooltip below is named for: a name long enough to tell from its own prefix.</summary>
    private const string Subject = "uat-west-eu";

    /// <summary>The project's page a release is cut from, of no trailing slash of its own.</summary>
    private const string Project = "https://octopus.example.net/app";

    // ------------------------------------------------------------------ Portal: the suffix and the name in front of it

    /// <summary>The portal tooltip of a name that is not there at all is the suffix and nothing before it, the same
    /// words the empty name says: <c>Portal</c> guards nothing, because a tooltip of a resource the topology never
    /// named still promises the new tab and the sign-in it promises whoever is named.</summary>
    [Fact]
    public void ThePortalTitleOfANameThatIsNotThereIsTheSuffixAndNothingBeforeIt()
    {
        Assert.Equal(LinkText.PortalSuffix, LinkText.Portal(null!));
        Assert.Equal(LinkText.Portal(string.Empty), LinkText.Portal(null!));

        // A name of one space is a name: it is glued on, and the gap it opens in the sentence stays open.
        Assert.Equal($" {LinkText.PortalSuffix}", LinkText.Portal(" "));
    }

    // ------------------------------------------------------------------ For: the keys no test has written out

    /// <summary>The seven keys of <see cref="LinkSet" /> whose words no other test says at length, written out here as
    /// the sentence the page shows: each names its subject, each ends with the portal's promise, and none of them is
    /// the fallback the unknown key gets. A trailing space, a doubled space or a missing word in any of these is what
    /// a reader of the tooltip sees, so the whole stem is held and not only its end.</summary>
    [Theory]
    [InlineData(LinkSet.Performance, "Performance in Application Insights for uat-west-eu: requests and their duration")]
    [InlineData(LinkSet.FrontDoor, "The Front Door profile of uat-west-eu in the Azure portal")]
    [InlineData(LinkSet.Logs, "Requests of uat-west-eu of the last hour, a query in Application Insights Logs")]
    [InlineData(LinkSet.ApplicationInsights, "Application Insights of uat-west-eu in the Azure portal")]
    [InlineData(LinkSet.ApplicationMap, "The application map of uat-west-eu in Application Insights")]
    [InlineData(LinkSet.Database, "The database of uat-west-eu in the Azure portal")]
    [InlineData(LinkSet.ResourceGroup, "The resource group of uat-west-eu in the Azure portal")]
    public void TheWordsOfAKeyOfTheSetAreSaidInFull(string key, string stem) =>
        Assert.Equal($"{stem}{LinkText.PortalSuffix}", LinkText.For(key, Subject));

    /// <summary>One of those tooltips, the one the environment's links carry, written whole - suffix and all - so the
    /// order of the two halves of the sentence is on the page and not only in a concatenation: what the viewer reads
    /// over a link is this string, whoever changes the constant or the stem.</summary>
    [Fact]
    public void OneTooltipIsTheWordsInTheOrderThePageShowsThem() =>
        Assert.Equal(
            "The resource group of uat-west-eu in the Azure portal (opens in a new tab; the Azure portal asks you to sign in)",
            LinkText.For(LinkSet.ResourceGroup, Subject));

    /// <summary>A key the set knows, of a subject that is not there: the sentence is said around the hole, with the two
    /// spaces the missing name leaves behind it, and nothing is refused - the subject comes from the topology, and a
    /// render that threw at a name the file never wrote would take the whole card with it. A subject of one space is
    /// said as the space it is, which widens the same hole to three.</summary>
    [Fact]
    public void AKeyItKnowsSaysItsWordsAroundASubjectThatIsNotThere()
    {
        Assert.Equal($"The web app  in the Azure portal{LinkText.PortalSuffix}", LinkText.For(LinkSet.Portal, null!));
        Assert.Equal($"The application map of  in Application Insights{LinkText.PortalSuffix}", LinkText.For(LinkSet.ApplicationMap, string.Empty));
        Assert.Equal($"The resource group of   in the Azure portal{LinkText.PortalSuffix}", LinkText.For(LinkSet.ResourceGroup, " "));
    }

    /// <summary>The subject is put in the sentence as it stands and escaped by nobody: an angle bracket, an ampersand
    /// and a quote come back out of the tooltip whole, because escaping is the job of the markup that writes the
    /// <c>title</c> attribute. That the words hold the raw characters is pinned so that the day someone escapes them
    /// twice - and shows a reader <c>&amp;lt;</c> in a tooltip - it is seen.</summary>
    [Fact]
    public void TheSubjectOfATooltipIsSaidAsItStandsAndEscapedByNobody() =>
        Assert.Equal(
            $"The database of uat<west> & \"eu\" in the Azure portal{LinkText.PortalSuffix}",
            LinkText.For(LinkSet.Database, "uat<west> & \"eu\""));

    // ------------------------------------------------------------------ Release: the one address the page builds

    /// <summary>The project's page is taken as an address, not as a prefix: its root is written with a slash by
    /// <see cref="Uri" /> and gets one path segment back, however many trailing slashes the file wrote - the cut takes
    /// all of them, so a project of <c>app//</c> and a project of <c>app</c> lead to the very same release - and the
    /// scheme, the port and the case of the host are the address's own doing rather than this code's. A <c>file</c>
    /// address is built the same way as an https one: the helper asks for nothing but an absolute address.</summary>
    [Theory]
    [InlineData("https://a.example.net", "https://a.example.net/deployments/releases/2.4.15")]
    [InlineData("https://a.example.net/", "https://a.example.net/deployments/releases/2.4.15")]
    [InlineData("https://a.example.net//", "https://a.example.net/deployments/releases/2.4.15")]
    [InlineData("https://a.example.net/app//", "https://a.example.net/app/deployments/releases/2.4.15")]
    [InlineData("https://a.example.net:8443/app", "https://a.example.net:8443/app/deployments/releases/2.4.15")]
    [InlineData("http://a.example.net/app", "http://a.example.net/app/deployments/releases/2.4.15")]
    [InlineData("HTTPS://A.Example.NET/App", "https://a.example.net/App/deployments/releases/2.4.15")]
    [InlineData("file:///srv/app", "file:///srv/app/deployments/releases/2.4.15")]
    public void TheReleasePageTakesTheProjectAsAnAddressAndNotAsAPrefix(string project, string expected) =>
        Assert.Equal(expected, LinkText.Release(new Uri(project), "2.4.15")!.AbsoluteUri);

    /// <summary>A version is a word, not a path: it is escaped, so the padding a JSON field drifts into survives as
    /// <c>%20</c> rather than being trimmed away, an escape already written in it is escaped a second time, the accent
    /// is cut to the bytes it is sent in, the percent of <c>100%</c> becomes <c>%25</c> and the slash of a path is no
    /// path at all. The one exception is the version that is a dot-dot, because <see cref="Uri" /> folds the segment
    /// before it away once the escaping has left the two characters alone: the release loses its <c>releases</c> and
    /// the page leads to the deployments of it - the address a caller is handed, awkward as a reader finds it, and
    /// pinned so that the day it is answered with <c>UriBuilder</c> instead (follow-up #665) the difference is a red
    /// test rather than a link nobody checks.</summary>
    [Theory]
    [InlineData(" 2.4.15 ", "https://a.example.net/app/deployments/releases/%202.4.15%20")]
    [InlineData("2.4.15%2B1", "https://a.example.net/app/deployments/releases/2.4.15%252B1")]
    [InlineData("2.4.15\u00e9", "https://a.example.net/app/deployments/releases/2.4.15%C3%A9")]
    [InlineData("100%", "https://a.example.net/app/deployments/releases/100%25")]
    [InlineData("../../x", "https://a.example.net/app/deployments/releases/..%2F..%2Fx")]
    [InlineData("..", "https://a.example.net/app/deployments/")]
    public void TheVersionOfAReleaseIsEscapedAndNeverTrimmed(string version, string expected) =>
        Assert.Equal(expected, LinkText.Release(new Uri("https://a.example.net/app"), version)!.AbsoluteUri);

    /// <summary>Of no version there is no release, and a version is no version when it holds nothing but the spaces a
    /// file can write in it - the tab, the newline, and the unbreakable space a browser copies out of a page - the
    /// blank being read by <c>IsNullOrWhiteSpace</c> and not by a length. The empty version and the run of ordinary
    /// spaces are pinned where the other half of the rule stands.</summary>
    [Theory]
    [InlineData("\t")]
    [InlineData("\n")]
    [InlineData("\u00a0")]
    [InlineData("\t \n")]
    public void AVersionOfNothingButTheGapBetweenWordsIsNoRelease(string version) =>
        Assert.Null(LinkText.Release(new Uri(Project), version));

    /// <summary>An address that is no address is refused by the address itself rather than answered with a guess: the
    /// helper reads the project's absolute string and a relative <see cref="Uri" /> has none, so the
    /// <see cref="InvalidOperationException" /> the caller meets is <c>Uri</c>'s own and not a guard this class could
    /// have written - it holds no <see cref="UriKind" /> of its own, and inventing a page under a host nobody named is
    /// the thing a link on this page must never do.</summary>
    [Fact]
    public void AProjectThatIsNoAddressBuildsNoRelease() =>
        Assert.Throws<InvalidOperationException>(() => LinkText.Release(new Uri("app/releases", UriKind.Relative), "2.4.15"));

    // ------------------------------------------------------------------ ReleaseTitle: the two names, as they stand

    /// <summary>The title of a release says a deployable and a version it was given as they come, spaces and all: the
    /// two that are not there at all leave the two holes of the sentence, which is the same string the two empties
    /// leave - and a name of one space widens both holes rather than closing either. Nothing here trims what a
    /// deployment omitted, because the words are a sentence about a release the caller already decided there is.</summary>
    [Fact]
    public void TheTitleOfAReleaseSaysTheNamesItWasGivenAsTheyCome()
    {
        Assert.Equal($"Release  of  in Octopus Deploy{LinkText.OctopusSuffix}", LinkText.ReleaseTitle(null!, null!));
        Assert.Equal($"Release  of  in Octopus Deploy{LinkText.OctopusSuffix}", LinkText.ReleaseTitle(string.Empty, string.Empty));
        Assert.Equal($"Release   of   in Octopus Deploy{LinkText.OctopusSuffix}", LinkText.ReleaseTitle(" ", " "));
        Assert.Equal($"Release 2.4.15 of  in Octopus Deploy{LinkText.OctopusSuffix}", LinkText.ReleaseTitle(null!, "2.4.15"));
    }
}
