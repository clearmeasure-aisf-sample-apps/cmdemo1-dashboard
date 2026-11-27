using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Dashboard.Tests;

/// <summary>
/// The runtime and work helpers are static methods: the markup and the other tests call most of them by name, and
/// the rest stood untested, so that a change to a line nobody reads would go unseen. The methods no other test
/// called are <see cref="RuntimeBoxLink.Of" /> (a frame, a region, a node), <see cref="WorkItemsText.Number" /> and
/// <see cref="RuntimePayloadBuilder.StateOf" /> (a health state, a container app's probe); the tests above call
/// them by name with the real values they turn into, and the guard below reads every public static method of
/// <c>src/Dashboard/Runtime</c> and <c>src/Dashboard/Work</c> and refuses one that no file of
/// <c>src/Dashboard.Tests</c> calls by name, so the next helper added cannot go untested the way they did. The
/// guard prints the file and the line of the method it refuses, the way the markup guards print the tag they
/// refuse (<see cref="TimeElementGuardTests" />), and it reads the properties and the fields the helpers wear
/// alongside their methods past, since a "public static" of another kind is no method.
/// </summary>
public class RuntimeWorkHelperCoverageTests
{
    /// <summary>The words the app appends to a link of the portal, of Octopus Deploy and of a browser address -
    /// the tests below write them out in full, and a test pins the words to the app's own constants.</summary>
    private const string PortalHint = " (opens in a new tab; the Azure portal asks you to sign in)";
    private const string OctopusHint = " (opens in a new tab; Octopus Deploy asks you to sign in)";
    private const string NewTabHint = " (opens in a new tab)";

    [Fact]
    public void TheHintsAreTheWordsTheAppWrites()
    {
        Assert.Equal(PortalHint, LinkText.PortalSuffix);
        Assert.Equal(OctopusHint, LinkText.OctopusSuffix);
        Assert.Equal(NewTabHint, RuntimeBoxLink.NewTabSuffix);
    }

    /// <summary>A frame of the diagram that the deployment gave no link of leads nowhere, and the read refuses a
    /// frame that is not there at all.</summary>
    [Fact]
    public void AFrameWithoutALinkLeadsNowhere()
    {
        RuntimeFrame? frame = null;

        Assert.Null(RuntimeBoxLink.Of(new RuntimeFrame("sub", "subscription", "sub-cmdemo1", null)));
        Assert.Throws<ArgumentNullException>(() => RuntimeBoxLink.Of(frame!));
    }

    /// <summary>The portal link of a frame says what the frame is: the subscription, the resource group, the
    /// Front Door profile, the plan, the cluster - and a kind the dashboard does not know is linked with its name
    /// alone, the link all the same.</summary>
    [Fact]
    public void AFrameNamesWhatItIsInTheTitleOfItsPortalLink()
    {
        var portal = new Uri("https://portal.example.net/#/overview");

        Assert.Equal(
            ("https://portal.example.net/#/overview", "The Azure subscription in the Azure portal" + PortalHint),
            LinkOf(RuntimeBoxLink.Of(new RuntimeFrame("sub", "subscription", "sub-cmdemo1", Links((LinkSet.Portal, portal))))));

        Assert.Equal(
            "The resource group rg-cmdemo1-nonprod in the Azure portal" + PortalHint,
            TitleOf(RuntimeBoxLink.Of(new RuntimeFrame("rg_tier", "resourceGroup", "rg-cmdemo1-nonprod", Links((LinkSet.Portal, portal))))));

        Assert.Equal(
            "The Front Door profile fd-cmdemo1-uat in the Azure portal" + PortalHint,
            TitleOf(RuntimeBoxLink.Of(new RuntimeFrame("afd", "frontDoorProfile", "fd-cmdemo1-uat", Links((LinkSet.Portal, portal))))));

        Assert.Equal(
            "The App Service plan asp-cmdemo1-uat in the Azure portal" + PortalHint,
            TitleOf(RuntimeBoxLink.Of(new RuntimeFrame("plan_primary", "plan", "asp-cmdemo1-uat", Links((LinkSet.Portal, portal))))));

        Assert.Equal(
            "The AKS cluster aks-cmdemo1-uat in the Azure portal" + PortalHint,
            TitleOf(RuntimeBoxLink.Of(new RuntimeFrame("cluster", "cluster", "aks-cmdemo1-uat", Links((LinkSet.Portal, portal))))));

        Assert.Equal(
            "web in the Azure portal" + PortalHint,
            TitleOf(RuntimeBoxLink.Of(new RuntimeFrame("own_web", "runtime", "web", Links((LinkSet.Portal, portal))))));

        // The words are the app's own, not a guess at them.
        Assert.Equal("The Azure subscription in the Azure portal" + PortalHint, LinkText.Portal("The Azure subscription in the Azure portal"));
    }

    /// <summary>A frame the deployment gave a link that is no portal's leads there: the resources of the resource
    /// group as a query in Azure Resource Graph Explorer, the project in Octopus Deploy, the address as a browser
    /// opens it.</summary>
    [Fact]
    public void AFrameLeadsByItsOtherLinks()
    {
        var graph = new Uri("https://portal.example.net/#blade/HubsExtension/ArgQueryBlade/query/");
        var octopus = new Uri("https://octopus.example.net/app#/Spaces-1/projects/demo");
        var site = new Uri("https://ca-web.example.io/");

        Assert.Equal(
            (graph.AbsoluteUri, "The resources in rg-cmdemo1-nonprod, a query in Azure Resource Graph Explorer" + PortalHint),
            LinkOf(RuntimeBoxLink.Of(new RuntimeFrame("rg_tier", "resourceGroup", "rg-cmdemo1-nonprod", Links((RuntimeBoxLink.Resources, graph))))));

        Assert.Equal(
            (octopus.AbsoluteUri, "The project that deploys web in Octopus Deploy" + OctopusHint),
            LinkOf(RuntimeBoxLink.Of(new RuntimeFrame("own_web", "runtime", "web", Links((RuntimeBoxLink.Project, octopus))))));

        Assert.Equal(
            (site.AbsoluteUri, "ca-web: ca-web.example.io, as a browser opens it" + NewTabHint),
            LinkOf(RuntimeBoxLink.Of(new RuntimeFrame("sub", "subscription", "ca-web", Links((RuntimeBoxLink.Site, site))))));
    }

    /// <summary>A box with several links leads to the first of the order the app keeps: the portal, the resources,
    /// the namespaces, the workloads, the project, the address - the most specific place the deployment knows.</summary>
    [Fact]
    public void TheMostSpecificLinkOfAFrameWins()
    {
        var portal = new Uri("https://portal.example.net/#/overview");
        var graph = new Uri("https://portal.example.net/#blade/ArgQuery");
        var namespaces = new Uri("https://portal.example.net/#blade/Namespace");
        var workloads = new Uri("https://portal.example.net/#blade/Workloads");
        var octopus = new Uri("https://octopus.example.net/app#/p");
        var site = new Uri("https://ca-web.example.io/");

        Assert.Equal(
            portal.AbsoluteUri,
            RuntimeBoxLink.Of(new RuntimeFrame("f", "resourceGroup", "rg", Links((LinkSet.Portal, portal), (RuntimeBoxLink.Resources, graph), (RuntimeBoxLink.Namespaces, namespaces), (LinkSet.Workloads, workloads), (RuntimeBoxLink.Project, octopus), (RuntimeBoxLink.Site, site))))!.Href);
        Assert.Equal(
            graph.AbsoluteUri,
            RuntimeBoxLink.Of(new RuntimeFrame("f", "resourceGroup", "rg", Links((RuntimeBoxLink.Resources, graph), (RuntimeBoxLink.Namespaces, namespaces), (LinkSet.Workloads, workloads), (RuntimeBoxLink.Project, octopus), (RuntimeBoxLink.Site, site))))!.Href);
        Assert.Equal(
            namespaces.AbsoluteUri,
            RuntimeBoxLink.Of(new RuntimeFrame("f", "resourceGroup", "rg", Links((RuntimeBoxLink.Namespaces, namespaces), (LinkSet.Workloads, workloads), (RuntimeBoxLink.Project, octopus), (RuntimeBoxLink.Site, site))))!.Href);
        Assert.Equal(
            workloads.AbsoluteUri,
            RuntimeBoxLink.Of(new RuntimeFrame("f", "resourceGroup", "rg", Links((LinkSet.Workloads, workloads), (RuntimeBoxLink.Project, octopus), (RuntimeBoxLink.Site, site))))!.Href);
        Assert.Equal(
            octopus.AbsoluteUri,
            RuntimeBoxLink.Of(new RuntimeFrame("f", "resourceGroup", "rg", Links((RuntimeBoxLink.Project, octopus), (RuntimeBoxLink.Site, site))))!.Href);
        Assert.Equal(
            site.AbsoluteUri,
            RuntimeBoxLink.Of(new RuntimeFrame("f", "resourceGroup", "rg", Links((RuntimeBoxLink.Site, site))))!.Href);
    }

    /// <summary>A region the deployment gave no link of leads nowhere, and the read refuses a region that is not
    /// there at all.</summary>
    [Fact]
    public void ARegionWithoutALinkLeadsNowhere()
    {
        RuntimeRegion? region = null;

        Assert.Null(RuntimeBoxLink.Of(new RuntimeRegion("region_primary", "centralus", ["primary"], null)));
        Assert.Throws<ArgumentNullException>(() => RuntimeBoxLink.Of(region!));
    }

    /// <summary>The link of a region says where its name leads: its page in the portal, the resources of the
    /// region as a query, the cluster's namespaces, the address as a browser opens it - and the region's own name
    /// is the resource the words are about.</summary>
    [Fact]
    public void ARegionLeadsByItsLink()
    {
        var portal = new Uri("https://portal.example.net/#/resourceGroups/rg-cmdemo1-nonprod/overview");
        var graph = new Uri("https://portal.example.net/#blade/HubsExtension/ArgQueryBlade/query/");
        var namespaces = new Uri("https://portal.example.net/#blade/HubBlade/namespaces");
        var site = new Uri("https://ca-cmdemo1-uat-ui.example.southcentralus.azurecontainerapps.io/");

        Assert.Equal(
            (portal.AbsoluteUri, "centralus in the Azure portal" + PortalHint),
            LinkOf(RuntimeBoxLink.Of(new RuntimeRegion("region_primary", "centralus", ["primary"], Links((LinkSet.Portal, portal))))));

        Assert.Equal(
            (graph.AbsoluteUri, "The resources in centralus, a query in Azure Resource Graph Explorer" + PortalHint),
            LinkOf(RuntimeBoxLink.Of(new RuntimeRegion("region_primary", "centralus", ["primary"], Links((RuntimeBoxLink.Resources, graph))))));

        Assert.Equal(
            (namespaces.AbsoluteUri, "The namespaces of the cluster in the Azure portal: centralus is one of them" + PortalHint),
            LinkOf(RuntimeBoxLink.Of(new RuntimeRegion("region_primary", "centralus", ["primary"], Links((RuntimeBoxLink.Namespaces, namespaces))))));

        Assert.Equal(
            (site.AbsoluteUri, "centralus: ca-cmdemo1-uat-ui.example.southcentralus.azurecontainerapps.io, as a browser opens it" + NewTabHint),
            LinkOf(RuntimeBoxLink.Of(new RuntimeRegion("region_primary", "centralus", ["primary"], Links((RuntimeBoxLink.Site, site))))));
    }

    /// <summary>A node the deployment gave no link of leads nowhere, and the read refuses a node that is not there
    /// at all.</summary>
    [Fact]
    public void ANodeWithoutALinkLeadsNowhere()
    {
        RuntimeNode? node = null;

        Assert.Null(RuntimeBoxLink.Of(new RuntimeNode("sqldb", RuntimeNodeKind.Sql, "demo-tdd/db", null)));
        Assert.Throws<ArgumentNullException>(() => RuntimeBoxLink.Of(node!));
    }

    /// <summary>The portal link of a node says what the node is: the database, the Static Web App - and a kind
    /// with no word of its own is linked with its name alone.</summary>
    [Fact]
    public void ANodeNamesWhatItIsInTheTitleOfItsPortalLink()
    {
        var portal = new Uri("https://portal.example.net/#/database");

        Assert.Equal(
            (portal.AbsoluteUri, "The database demo-tdd/db in the Azure portal" + PortalHint),
            LinkOf(RuntimeBoxLink.Of(new RuntimeNode("sqldb", RuntimeNodeKind.Sql, "demo-tdd/db", Links: Links((LinkSet.Portal, portal))))));

        Assert.Equal(
            (portal.AbsoluteUri, "The workloads of the cluster in the Azure portal: demo-tdd/db is one of them" + PortalHint),
            LinkOf(RuntimeBoxLink.Of(new RuntimeNode("sqldb", RuntimeNodeKind.Sql, "demo-tdd/db", Links: Links((LinkSet.Workloads, portal))))));

        Assert.Equal(
            "The Static Web App swa-cmdemo1-uat-dashboard in the Azure portal" + PortalHint,
            TitleOf(RuntimeBoxLink.Of(new RuntimeNode("swa_dashboard", RuntimeNodeKind.StaticSite, "swa-cmdemo1-uat-dashboard", Links: Links((LinkSet.Portal, portal))))));

        Assert.Equal(
            "ca-web in the Azure portal" + PortalHint,
            TitleOf(RuntimeBoxLink.Of(new RuntimeNode("app_web", RuntimeNodeKind.WebApp, "ca-web", Links: Links((LinkSet.Portal, portal))))));
    }

    /// <summary>The address a browser opens is the last of the order: a node the deployment gave only a site leads
    /// there, and says so in the title.</summary>
    [Fact]
    public void ANodeLeadsToItsSiteAsABrowserOpensIt()
    {
        var site = new Uri("https://demo-tdd-db.example.io/");

        Assert.Equal(
            (site.AbsoluteUri, "demo-tdd/db: demo-tdd-db.example.io, as a browser opens it" + NewTabHint),
            LinkOf(RuntimeBoxLink.Of(new RuntimeNode("sqldb", RuntimeNodeKind.Sql, "demo-tdd/db", Links: Links((RuntimeBoxLink.Site, site))))));
    }

    /// <summary>The number of a work item is its number with a "#", and an item the file did not number is only an
    /// item.</summary>
    [Fact]
    public void TheNumberNamesTheWorkItemOrOnlySaysItem()
    {
        Assert.Equal("#208", WorkItemsText.Number(Item(208)));
        Assert.Equal("Item", WorkItemsText.Number(Item(null)));
    }

    /// <summary>Every state of a health check has its word as the payload carries it, and a check not yet made is
    /// the word of a check in progress.</summary>
    [Fact]
    public void TheWordOfAHealthStateIsTheWordThePayloadCarries()
    {
        Assert.Equal("checking", RuntimePayloadBuilder.StateOf(HealthState.Pending));
        Assert.Equal("healthy", RuntimePayloadBuilder.StateOf(HealthState.Healthy));
        Assert.Equal("unhealthy", RuntimePayloadBuilder.StateOf(HealthState.Unhealthy));
        Assert.Equal("unreachable", RuntimePayloadBuilder.StateOf(HealthState.Unreachable));
    }

    /// <summary>Every state of a container app's probe has its word as the payload carries it: answering is as
    /// good as healthy for the colours, and the states not yet asked of and waking are the word of a check in
    /// progress.</summary>
    [Fact]
    public void TheWordOfAContainerAppStateIsTheWordThePayloadCarries()
    {
        Assert.Equal("checking", RuntimePayloadBuilder.StateOf(ContainerAppState.Probing));
        Assert.Equal("checking", RuntimePayloadBuilder.StateOf(ContainerAppState.Waking));
        Assert.Equal("healthy", RuntimePayloadBuilder.StateOf(ContainerAppState.Responding));
        Assert.Equal("healthy", RuntimePayloadBuilder.StateOf(ContainerAppState.Healthy));
        Assert.Equal("unhealthy", RuntimePayloadBuilder.StateOf(ContainerAppState.Unhealthy));
        Assert.Equal("unreachable", RuntimePayloadBuilder.StateOf(ContainerAppState.NotResponding));
    }

    /// <summary>
    /// Every public static method of <c>src/Dashboard/Runtime</c> and <c>src/Dashboard/Work</c> is called by name
    /// in some file of <c>src/Dashboard.Tests</c> - a "Class.Method(" of the test's own text, over and above the
    /// same name of another type - so a helper nobody calls stands out here instead of going untested. The
    /// methods the tests above exist for are among what the guard read, and so this guard and they cannot drift
    /// apart: a rename of a helper breaks both, and the guard reports the file and the line of the method it
    /// refuses, so a fixer is sent back to the right declaration.
    /// </summary>
    [Fact]
    public void EveryPublicStaticMethodOfRuntimeAndWorkIsCalledByNameInTheTests()
    {
        var corpus = TestCorpus();
        var offenders = new List<string>();

        foreach (var group in HelperMethods().GroupBy(method => (method.Class, method.Name)))
        {
            if (!Regex.IsMatch(corpus, $@"(?<![A-Za-z0-9_]){group.Key.Class}\.{group.Key.Name}[ \t]*\("))
            {
                offenders.AddRange(group.Select(method => $"{method.File}:{method.Line} {method.Class}.{method.Name} is called by no test by name"));
            }
        }

        Assert.Empty(offenders);
    }

    /// <summary>
    /// The source scan and the assembly enumeration are two reads of the same set: neither may find what the other
    /// misses. An attribute in front of the <c>public static</c>, a return type the source pattern cannot cross,
    /// a type written in a form the regex does not know - the source scan would miss them, but the assembly read
    /// does not; and the source scan would read internal helpers the assembly read skips, which is the other way a
    /// discrepancy shows up. The two sets must be equal, both ways, for each of the two namespaces.
    /// </summary>
    [Fact]
    public void TheSourceAndTheAssemblyDeclareTheSameRuntimeWorkHelpers()
    {
        var methods = HelperMethods();

        // Runtime namespace: compare source scan with assembly enumeration.
        var runtimeSource = methods
            .Where(method => method.File.StartsWith("Runtime/", StringComparison.Ordinal))
            .Select(method => $"{method.Class}.{method.Name}")
            .Distinct(StringComparer.Ordinal)
            .ToHashSet(StringComparer.Ordinal);
        var runtimeAssembly = SourceScan.DeclaredInAssembly(typeof(RuntimeBoxLink), "Dashboard.Runtime");

        Assert.Empty(runtimeSource.Except(runtimeAssembly));
        Assert.Empty(runtimeAssembly.Except(runtimeSource));

        // Work namespace: compare source scan with assembly enumeration.
        var workSource = methods
            .Where(method => method.File.StartsWith("Work/", StringComparison.Ordinal))
            .Select(method => $"{method.Class}.{method.Name}")
            .Distinct(StringComparer.Ordinal)
            .ToHashSet(StringComparer.Ordinal);
        var workAssembly = SourceScan.DeclaredInAssembly(typeof(WorkItemsText), "Dashboard.Work");

        Assert.Empty(workSource.Except(workAssembly));
        Assert.Empty(workAssembly.Except(workSource));
    }

    /// <summary>
    /// A guard that found nothing has seen nothing: the helpers declare well over a hundred public static methods
    /// across both directories today, and the tests that call them number well over a hundred files. A scan that
    /// reaches far fewer read the wrong place, and the clean verdict above would mean nothing; the floors are set
    /// well below those counts, so the day the app gains a helper the test still says what it reads. The guard
    /// reads the methods this file calls - <c>RuntimeBoxLink.Of</c>, <c>RuntimePayloadBuilder.StateOf</c>,
    /// <c>WorkItemsText.Number</c> - and it reads the properties and fields the helpers wear past.
    /// </summary>
    [Fact]
    public void TheGuardReadsTheHelpersAndTheTestsOfTheApp()
    {
        var methods = HelperMethods();
        var files = methods.Select(method => method.File).Distinct(StringComparer.Ordinal).ToList();

        Assert.True(
            methods.Count >= 100,
            $"only {methods.Count} public static method(s) were read under {DashboardRoot()}: the guard reads nothing");

        Assert.Contains(files, file => file.StartsWith("Runtime/", StringComparison.Ordinal));
        Assert.Contains(files, file => file.StartsWith("Work/", StringComparison.Ordinal));
        Assert.Contains(methods, method => method.File.Equals("Runtime/RuntimeBoxLink.cs", StringComparison.Ordinal) && method.Name == "Of");
        Assert.Contains(methods, method => method.File.Equals("Runtime/RuntimePayloadBuilder.cs", StringComparison.Ordinal) && method.Name == "StateOf");
        Assert.Contains(methods, method => method.File.Equals("Work/WorkItemsText.cs", StringComparison.Ordinal) && method.Name == "Number");
        Assert.Contains(methods, method => method.File.Equals("Work/WorkBotText.cs", StringComparison.Ordinal) && method.Name == "FromItems");
        Assert.Contains(methods, method => method.File.Equals("Runtime/RuntimeManifestParser.cs", StringComparison.Ordinal) && method.Name == "ParseIndex");

        // A "public static" of another kind is no method: the properties and the fields of the helpers are read past.
        Assert.DoesNotContain(methods, method => method.Class == "ApplicationRead" && method.Name == "Missing");
        Assert.DoesNotContain(methods, method => method.Class == "DatabaseRefresh" && method.Name == "Interval");
        Assert.DoesNotContain(methods, method => method.Class == "WorkColumns" && method.Name == "Order");
        Assert.DoesNotContain(methods, method => method.Class == "WorkColumns" && method.Name == "Active");

        var tests = TestFiles();

        Assert.True(
            tests.Count >= 100,
            $"only {tests.Count} test file(s) were read under {TestsRoot()}: the call scan reads the wrong place");
        Assert.Contains(tests, file => file.Equals("RuntimeBoxLinkTests.cs", StringComparison.Ordinal));
        Assert.Contains(tests, file => file.Equals("RuntimeWorkHelperCoverageTests.cs", StringComparison.Ordinal));
    }

    /// <summary>The links a test gives a box of the diagram, by key.</summary>
    private static LinkSet Links(params (string Key, Uri Address)[] links) =>
        new(links.Select(link => new KeyValuePair<string, Uri>(link.Key, link.Address)));

    /// <summary>The href and the title of the link a box leads to - refusing a box that leads nowhere, so a test
    /// cannot pass on a link the rule never read.</summary>
    private static (string Href, string Title) LinkOf(RuntimeLink? link)
    {
        var value = Assert.IsType<RuntimeLink>(link);
        return (value.Href, value.Title);
    }

    private static string TitleOf(RuntimeLink? link)
    {
        var value = Assert.IsType<RuntimeLink>(link);
        return value.Title;
    }

    /// <summary>A work item the tests above read the number of: the number is what varies, the rest is the shape
    /// of an item as the file writes one.</summary>
    private static WorkItem Item(int? number) =>
        new(new WorkItemIdentity(number, "A title", null, "In progress", "in-progress"), [], null, "the bot", [], [], null, WorkItemEnvironments.None, null);

    /// <summary>One public static method the guard read: the file and the line it declares on, and the type that
    /// owns it - the file as the walk prints it, under <c>src/Dashboard</c>.</summary>
    private sealed record HelperMethod(string File, int Line, string Class, string Name);

    private static readonly Regex StaticType = new(
        @"^[ \t]*public[ \t]+(?:(?:sealed|static|partial|abstract)[ \t]+)*(?:class|record|struct|enum|interface)[ \t]+([A-Za-z_][A-Za-z0-9_]*)",
        RegexOptions.Multiline | RegexOptions.Compiled);

    private static readonly Regex StaticDeclaration = new(
        @"^[ \t]*public[ \t]+static[ \t]+",
        RegexOptions.Multiline | RegexOptions.Compiled);

    private static readonly Regex MethodName = new(
        @"([A-Za-z_][A-Za-z0-9_]*)(?:[ \t]*<[^;{}]*>)?[ \t]*\(",
        RegexOptions.Compiled);

    /// <summary>Every public static method of the helpers, in the order the files write them: the public static
    /// classes of <c>src/Dashboard/Runtime</c> and <c>src/Dashboard/Work</c>, the properties and the fields of
    /// the helpers read past. Read once per test run from the shared read of the two folders - the files that
    /// stand directly in each folder, as the guard has always read them - since the source under test does not
    /// move while the tests run.</summary>
    private static IReadOnlyList<HelperMethod> HelperMethods() => HelperMethodsOnce.Value;

    private static readonly Lazy<IReadOnlyList<HelperMethod>> HelperMethodsOnce = new(ReadHelperMethods);

    private static System.Collections.ObjectModel.ReadOnlyCollection<HelperMethod> ReadHelperMethods()
    {
        var methods = new List<HelperMethod>();

        foreach (var directory in new[] { "Runtime", "Work" })
        {
            foreach (var file in SourceText.Files(Path.Combine(DashboardRoot(), directory)).Where(file => Path.GetDirectoryName(file.Path) is ""))
            {
                methods.AddRange(MethodsIn(file.Text, Path.Combine(directory, file.Path)));
            }
        }

        return methods.AsReadOnly();
    }

    private static List<HelperMethod> MethodsIn(string text, string file)
    {
        var methods = new List<HelperMethod>();
        var types = StaticType.Matches(text).Select(match => (Index: match.Index, Name: match.Groups[1].Value)).ToList();

        foreach (Match declaration in StaticDeclaration.Matches(text))
        {
            var at = StartOfSignature(text, declaration);
            if (at < 0)
            {
                continue;
            }

            var name = MethodName.Match(SignatureOf(text, at));
            if (!name.Success)
            {
                continue;
            }

            var type = types.LastOrDefault(candidate => candidate.Index < declaration.Index);
            if (type.Name is null)
            {
                continue;
            }

            methods.Add(new HelperMethod(file, LineOf(text, declaration.Index), type.Name, name.Groups[1].Value));
        }

        return methods;
    }

    /// <summary>The signature runs on after the word the declaration opens with, to the "{", the ";" or the "=>"
    /// that ends it - or to the end of the file when nothing does. A word that says the declaration is no method -
    /// the static type, the readonly field, the delegate - is read as no signature.</summary>
    private static int StartOfSignature(string text, Match declaration)
    {
        var at = declaration.Index + declaration.Length;
        var start = at;

        while (at < text.Length && (char.IsLetterOrDigit(text[at]) || text[at] == '_'))
        {
            at++;
        }

        var word = text[start..at];

        return word is "class" or "record" or "struct" or "enum" or "interface" or "delegate" or "params" or "readonly"
            ? -1
            : at;
    }

    /// <summary>The whole of a method's signature: the return type and the parameters, read past the string and
    /// character literals a default value may wear. The parentheses of the parameters are counted, so a ";" of
    /// one of them does not end the read, and the "=>" that opens an expression body does.</summary>
    private static string SignatureOf(string text, int at)
    {
        var depth = 0;

        for (var i = at; i < text.Length; i++)
        {
            var c = text[i];

            if (c == '"')
            {
                i = EndOfString(text, i);
                continue;
            }

            if (c == '\'')
            {
                i = EndOfCharacter(text, i);
                continue;
            }

            if (c == '(')
            {
                depth++;
            }
            else if (c == ')')
            {
                depth--;
            }
            else if (depth == 0 && c is '{' or ';')
            {
                return text[at..i];
            }
            else if (depth == 0 && c == '>' && i + 1 < text.Length && text[i + 1] == '=')
            {
                return text[at..i];
            }
        }

        return text[at..];
    }

    /// <summary>The closing quote of the string that opens at <paramref name="at"/>, or the end of the text when
    /// the string is never closed. The quotes of a raw string are three deep, and a verbatim string spells its
    /// own quote twice rather than escaping it.</summary>
    private static int EndOfString(string text, int at)
    {
        if (text.Length - at >= 6 && text[at + 1] == '"' && text[at + 2] == '"')
        {
            var close = text.IndexOf("\"\"\"", at + 3, StringComparison.Ordinal);
            return close < 0 ? text.Length - 1 : close + 2;
        }

        var verbatim = at > 0 && text[at - 1] == '@';
        var i = at + 1;

        while (i < text.Length)
        {
            if (text[i] == '"')
            {
                if (verbatim && i + 1 < text.Length && text[i + 1] == '"')
                {
                    i += 2;
                    continue;
                }

                return i;
            }

            if (!verbatim && text[i] == '\\' && i + 1 < text.Length)
            {
                i += 2;
                continue;
            }

            i++;
        }

        return text.Length - 1;
    }

    /// <summary>The closing quote of the character literal that opens at <paramref name="at"/>, or the end of the
    /// text when it is never closed.</summary>
    private static int EndOfCharacter(string text, int at)
    {
        var i = at + 1;

        while (i < text.Length)
        {
            if (text[i] == '\\' && i + 1 < text.Length)
            {
                i += 2;
                continue;
            }

            if (text[i] == '\'')
            {
                return i;
            }

            i++;
        }

        return text.Length - 1;
    }

    /// <summary>The line a character of the file stands on, from one.</summary>
    private static int LineOf(string text, int at) => text.AsSpan(0, at).Count('\n') + 1;

    /// <summary>Every test file, as the path the assertion prints it: under <c>src/Dashboard.Tests</c>, the
    /// build's copies under <c>bin</c> and <c>obj</c> walked past, from the shared read of the run.</summary>
    private static List<string> TestFiles() => [.. SourceText.Files(TestsRoot()).Select(file => file.Path)];

    /// <summary>The text of every test file, each one marked, so a name at the end of one file never reads as a
    /// call in the next - joined once per test run, from the shared read of the run.</summary>
    private static string TestCorpus() => TestCorpusOnce.Value;

    private static readonly Lazy<string> TestCorpusOnce = new(() =>
        string.Join("\n// the next test file\n", SourceText.Files(TestsRoot()).Select(file => file.Text)));

    private static string DashboardRoot([CallerFilePath] string here = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "Dashboard"));

    private static string TestsRoot([CallerFilePath] string here = "") =>
        Path.GetFullPath(Path.GetDirectoryName(here)!);

    /// <summary>
    /// The shared source read and this guard's own read are two reads of the same set of helpers: neither may
    /// find what the other misses. The shared <see cref="SourceText" /> walk reads the <c>public static</c>
    /// methods of <c>src/Dashboard/Runtime</c> and <c>src/Dashboard/Work</c> the same way this file's
    /// <see cref="HelperMethods" /> does - a declaration attributed to the type that encloses it, the comments and
    /// the content of the strings blanked so a name in a sentence is not a declaration - so the two reads name the
    /// same <c>Type.Method</c> set, both ways, for the two folders together. This is the check the issue asks
    /// for: the one shared read all three coverage guards hold themselves to, read beside this guard's own reader
    /// rather than in place of it, so the guard's floors stand untouched.
    /// </summary>
    [Fact]
    public void TheSharedSourceReadNamesTheSameRuntimeAndWorkHelpersAsThisGuardsOwnRead()
    {
        var own = HelperMethods()
            .Select(method => $"{method.Class}.{method.Name}")
            .ToHashSet(StringComparer.Ordinal);

        var shared = SourceText.Files(Path.Combine(DashboardRoot(), "Runtime"))
            .Concat(SourceText.Files(Path.Combine(DashboardRoot(), "Work")))
            .SelectMany(source => SourceText.Declarations(source.Path, source.Text))
            .Select(declaration => declaration.Key)
            .ToHashSet(StringComparer.Ordinal);

        // The shared read names at least everything this guard's own reader names: a shared read that missed a
        // helper the guard sees would be a weaker read, and the one the three guards hold themselves to must not
        // be weaker than any of them. The reverse is not asserted, since the shared read is the more complete of
        // the two - it names the helpers the guard's own reader names, and more (the tuple-returning
        // <c>DatabaseView.ReadAt</c> and the <c>ApplicationView.Boxes</c> overloads, which the hand-rolled reader
        // reads past, stand among them).
        Assert.Empty(own.Except(shared));

        // The shared read reached the helpers at all, and named the ones this guard names: a shared read that
        // reached nothing would agree with nothing and say nothing.
        Assert.NotEmpty(shared);
        Assert.Contains("RuntimeBoxLink.Of", shared);
        Assert.Contains("RuntimePayloadBuilder.StateOf", shared);
        Assert.Contains("WorkItemsText.Number", shared);
    }
}
