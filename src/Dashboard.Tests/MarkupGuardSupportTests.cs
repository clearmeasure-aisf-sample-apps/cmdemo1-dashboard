using System.Reflection;
using System.Runtime.CompilerServices;

namespace Dashboard.Tests;

/// <summary>
/// The tests of the two shared helpers <see cref="MarkupGuards" /> gathers: <see cref="MarkupGuards.CountOccurrences" />,
/// the count of a word in a file every markup guard was writing for itself, and the path of a component every markup
/// guard was navigating to for itself.
/// <para>
/// Three things are pinned here, and all three are needed before a guard file may call the shared one and keep the
/// assertion it already holds. First the count itself: what it counts, what it walks past, that it is ordinal rather
/// than a culture's idea of two words being the same, and that it refuses an empty word rather than walking on the
/// same letter for ever - the one way the copies this helper retires could be made to hang. Second the paths: every
/// component, every folder and every file the shared helper names is the file or folder that exists, inside this
/// repository's <c>src</c> and nowhere else, and the text it hands back is the text of that file.
/// </para>
/// <para>
/// Third - and this is the pin that lets the hoist be done one call site at a time - the equivalence of the shared
/// helper with every copy still standing: the assembly is walked for the private <c>CountOccurrences(string, string)</c>
/// and the private <c>[CallerFilePath]</c> path helpers the guard files still keep, each is invoked, and each must say
/// exactly what <see cref="MarkupGuards" /> says. A copy that drifts, and a guard that reads a different file from the
/// one the shared helper names, both fail here rather than quietly agreeing with an assertion that no longer means what
/// it says. The counts of the copies are pinned from above as well as below: the sets may only shrink, never grow, so
/// the duplication this change homes cannot spread while the follow-up files retire the copies one file at a time.
/// </para>
/// <para>
/// The counts the guard tests already hold are said again through the shared helper - the two
/// <c>@ReloadTopologyLabel</c> of the header, the one <c>WorkItemsText.Availability</c> of the work items view, the
/// help ids and their <c>aria-describedby</c> pairs, the badge's quiet class, the facts row and its two cards - so the
/// same assertion stands on the shared path and on the copy, and neither may move without the other being seen.
/// Nothing here reaches the network, and nothing asks a clock what time it is: every value is a letter of a file this
/// repository holds.
/// </para>
/// </summary>
public class MarkupGuardSupportTests
{
    private const BindingFlags Private = BindingFlags.NonPublic | BindingFlags.Static;

    /// <summary><c>src</c>: the folder the app's source and the tests' source both stand under, so a shared path can
    /// be pinned to inside it.</summary>
    private static string Sources => Path.GetFullPath(Path.Combine(MarkupGuards.TestsRoot(), ".."));

    // ----- the count

    /// <summary>What the count counts: every occurrence, each walked past by the word's own length, so the letters of
    /// one occurrence are never the letters of the next. Nothing found is nothing; a word longer than the text is
    /// nothing; a word said once at either end and once in the middle is three; the word as long as the text is one;
    /// and a word that could overlap itself is counted by whole steps only - <c>aa</c> in <c>aaa</c> is said once, the
    /// second step starting past the letters of the first.</summary>
    [Theory]
    [InlineData("", "a", 0)]
    [InlineData("a", "b", 0)]
    [InlineData("abc", "abcd", 0)]
    [InlineData("a", "a", 1)]
    [InlineData("aba", "a", 2)]
    [InlineData("ababab", "ab", 3)]
    [InlineData("aa", "aa", 1)]
    [InlineData("aaa", "aa", 1)]
    [InlineData("aaaa", "aa", 2)]
    [InlineData("ababa", "aba", 1)]
    [InlineData("  a  ", "  ", 2)]
    [InlineData("one\none\n", "one\n", 2)]
    [InlineData("<p>x</p><p>y</p>", "<p>", 2)]
    [InlineData("<p>x</p><p>y</p>", "</p><p>", 1)]
    public void TheCountWalksEveryOccurrenceByItsOwnLength(string text, string value, int expected) =>
        Assert.Equal(expected, MarkupGuards.CountOccurrences(text, value));

    /// <summary>The count is a walk over letters and not over meanings: ordinal, so a word in a case the file does not
    /// spell is a different word, and so a reader of German - or of any culture at all, in any order of asking - is
    /// given the same number the file itself says. A comparison that asked the culture would call a composed
    /// <c>é</c> the two letters <c>e</c> and a combining acute, and would count a word the file never wrote.</summary>
    [Fact]
    public void TheCountIsTheLettersAndNotTheReadersCulture()
    {
        const string Ready = "Ready ready READY";

        Assert.Equal(1, MarkupGuards.CountOccurrences(Ready, "ready"));
        Assert.Equal(1, MarkupGuards.CountOccurrences(Ready, "Ready"));
        Assert.Equal(1, MarkupGuards.CountOccurrences(Ready, "READY"));
        Assert.Equal(0, MarkupGuards.CountOccurrences(Ready, "rEADY"));

        // Ordinal: a composed é is the one letter it is, and no comparison is made to the two letters
        // an e and a combining acute would read as to a culture.
        Assert.Equal(1, MarkupGuards.CountOccurrences("\u00E9", "\u00E9"));
        Assert.Equal(0, MarkupGuards.CountOccurrences("\u00E9", "e\u0301"));
        Assert.Equal(0, MarkupGuards.CountOccurrences("\u00E9", "e"));

        var app = MarkupGuards.AppText();
        var before = MarkupGuards.CountOccurrences(app, "@ReloadTopologyLabel</button>");

        Assert.Equal(2, before);
        Assert.Equal(before, CultureScope.Says("de-DE", () => MarkupGuards.CountOccurrences(app, "@ReloadTopologyLabel</button>")));
        ViewerCulture.German(() => Assert.Equal(before, MarkupGuards.CountOccurrences(app, "@ReloadTopologyLabel</button>")));
    }

    /// <summary>The count refuses what it cannot count rather than answering nonsense or nothing at all: neither
    /// argument is ever nothing, and an empty word is refused outright - the walk steps by the word's length, so an
    /// empty word would step by nothing and the count would never end. A caller that asks how many times nothing is
    /// said is told where it stands, by the name of the argument.</summary>
    [Fact]
    public void TheCountRefusesNothingToCountAndAnEmptyWord()
    {
        Assert.Throws<ArgumentNullException>(() => MarkupGuards.CountOccurrences(null!, "a"));
        Assert.Throws<ArgumentNullException>(() => MarkupGuards.CountOccurrences("a", null!));
        Assert.Throws<ArgumentException>(() => MarkupGuards.CountOccurrences("a", string.Empty));
        Assert.Throws<ArgumentException>(() => MarkupGuards.CountOccurrences(string.Empty, string.Empty));
    }

    /// <summary>Every private <c>CountOccurrences(string, string)</c> still standing in a guard file says exactly what
    /// the shared one says - on hand-written samples and on the real markup of the page and of four components, over
    /// the needles the guards themselves count. This is the pin that lets a file drop its copy and call
    /// <see cref="MarkupGuards" /> without its expected number moving.</summary>
    [Fact]
    public void TheSharedCountSaysWhatEveryFilesOwnCopyOfItSays()
    {
        var copies = MethodsNamed(nameof(MarkupGuards.CountOccurrences), typeof(int), CountSignature);

        Assert.True(
            copies.Count >= 8,
            $"only {copies.Count} private copy(ies) of CountOccurrences were found: the walk no longer reaches the guard files, so this test proves nothing");
        Assert.True(
            copies.Count <= 10,
            $"{copies.Count} private copies of CountOccurrences stand in the guard files: the duplication may only shrink as the files call MarkupGuards.CountOccurrences, never grow");

        foreach (var (text, value) in CountingSamples())
        {
            var expected = MarkupGuards.CountOccurrences(text, value);

            foreach (var copy in copies)
            {
                var said = (int)copy.Invoke(null, [text, value])!;
                Assert.True(
                    expected == said,
                    $"{copy.DeclaringType!.Name}.{copy.Name} says {said} and MarkupGuards.CountOccurrences says {expected} about {value}.");
            }
        }
    }

    /// <summary>The counts the guard tests already hold, said through the shared count of the shared text: the header's
    /// two reload buttons and its help ids, the traffic panel's, the badge's quiet class, the facts row and the cards
    /// it puts together, the availability line's three parts, the cluster card's totals, the delivery group's card and
    /// the work items view's one settling and one saying of its state. Each number is the number the guard file pins,
    /// so the shared helper carries the same assertion and not a weaker one.</summary>
    [Theory]
    [InlineData("DeployableFacts", """<div class="facts-row">""", 1)]
    [InlineData("DeployableFacts", "<CodeCard ", 1)]
    [InlineData("DeployableFacts", "<DeliveryCard ", 1)]
    [InlineData("DeployableFacts", "Context=\"@context\"", 2)]
    [InlineData("DeployableFacts", "[Parameter, EditorRequired]", 6)]
    [InlineData("DashboardCode", "<CodeCard ", 2)]
    [InlineData("DashboardCode", """<div class="facts-row">""", 2)]
    [InlineData("DashboardCode", "[Parameter, EditorRequired]", 4)]
    [InlineData("DashboardCode", "[Parameter]", 1)]
    [InlineData("TrafficPanel", "id=\"traffic-stop-help\"", 1)]
    [InlineData("TrafficPanel", "aria-describedby=\"traffic-stop-help\"", 1)]
    [InlineData("ClusterBadge", "state--quiet", 1)]
    [InlineData("ClusterBadge", "state-icon--neutral", 1)]
    [InlineData("SharedCost", "<section", 1)]
    [InlineData("SharedCost", "<h2", 1)]
    [InlineData("SharedCost", "aria-labelledby", 1)]
    [InlineData("AvailabilityLine", "availability__label", 1)]
    [InlineData("AvailabilityLine", "availability__note", 1)]
    [InlineData("AvailabilityLine", "availability__part", 1)]
    [InlineData("ClusterCard", "cluster-card__totals", 1)]
    [InlineData("EnvironmentDelivery", "@if (others.Count > 0)", 1)]
    [InlineData("EnvironmentDelivery", "<DeliveryCard", 1)]
    [InlineData("WorkItemsView", "WorkItemsText.Availability(", 1)]
    [InlineData("WorkItemsView", "WorkItemsText.Unavailable(", 1)]
    [InlineData("RuntimeView", "<DashboardCode ", 1)]
    public void TheCountsTheGuardTestsHoldAreTheCountsTheSharedHelperSays(string component, string value, int expected) =>
        Assert.Equal(expected, MarkupGuards.CountOccurrences(MarkupGuards.ComponentText(component), value));

    /// <summary>The page's own counts, through <see cref="MarkupGuards.AppText" />: the header draws its reload label
    /// twice, once for the quiet controls and once for the problem button, and explains itself in one help span per
    /// control - the pause help once, the reload help once and named by the two buttons that point at it.</summary>
    [Theory]
    [InlineData("@ReloadTopologyLabel</button>", 2)]
    [InlineData("id=\"interval-help\"", 1)]
    [InlineData("id=\"probe-help\"", 1)]
    [InlineData("id=\"check-now-help\"", 1)]
    [InlineData("id=\"pause-help\"", 1)]
    [InlineData("aria-describedby=\"pause-help\"", 1)]
    [InlineData("id=\"reload-topology-help\"", 1)]
    [InlineData("aria-describedby=\"reload-topology-help\"", 2)]
    [InlineData("<DashboardCode ", 1)]
    public void TheCountsTheHeaderTestsHoldAreTheCountsTheSharedHelperSays(string value, int expected) =>
        Assert.Equal(expected, MarkupGuards.CountOccurrences(MarkupGuards.AppText(), value));

    // ----- the paths

    /// <summary>The two folders the guards live between: the tests' own and the app's, one above the other under
    /// <c>src</c>; the components, the health, the runtime and the site folder the app's source holds; and the two
    /// files a guard reads most - the page and the work items view. Every one exists, every one is inside
    /// <c>src</c>, and the text of the two files is the text of the file the path names.</summary>
    [Fact]
    public void EveryPathTheSharedHelperNamesIsAFileOrAFolderOfThisSource()
    {
        AssertDirectories([MarkupGuards.TestsRoot(), MarkupGuards.DashboardRoot(), MarkupGuards.SourcePath("Components"),
            MarkupGuards.SourcePath("Health"), MarkupGuards.SourcePath("Runtime"), MarkupGuards.SourcePath("wwwroot")]);

        AssertFiles([MarkupGuards.AppPath(), MarkupGuards.ComponentPath("WorkItemsView"), MarkupGuards.ComponentPath("StateBadge"),
            MarkupGuards.ComponentPath("DeployableFacts"), MarkupGuards.SourcePath("wwwroot/index.html"),
            MarkupGuards.SourcePath("Runtime/RuntimePayloadBuilder.cs")]);

        Assert.Equal(File.ReadAllText(MarkupGuards.AppPath()), MarkupGuards.AppText());
        Assert.Equal(File.ReadAllText(MarkupGuards.ComponentPath("WorkItemsView")), MarkupGuards.ComponentText("WorkItemsView"));
    }

    /// <summary>The two files that were reading the work items view the long way round -
    /// <c>WorkItemsViewTests</c> and <c>WorkItemsAvailabilityEdgeCaseTests</c> - and the shared helper all three of
    /// them now agree on: one file, the one the page draws, holding the state the monitor settles on and the sentence
    /// it says, each of them said once.</summary>
    [Fact]
    public void TheWorkItemsViewIsOneFileWhicheverGuardAsksForIt()
    {
        var path = MarkupGuards.ComponentPath("WorkItemsView");

        Assert.EndsWith(Path.Combine("Dashboard", "Components", "WorkItemsView.razor"), path, StringComparison.Ordinal);
        Assert.EndsWith(Path.Combine("Dashboard.Tests", "WorkItemsViewTests.cs"), Path.Combine(MarkupGuards.TestsRoot(), "WorkItemsViewTests.cs"), StringComparison.Ordinal);
        Assert.True(File.Exists(Path.Combine(MarkupGuards.TestsRoot(), "WorkItemsViewTests.cs")));
        Assert.True(File.Exists(Path.Combine(MarkupGuards.TestsRoot(), "WorkItemsAvailabilityEdgeCaseTests.cs")));

        var text = MarkupGuards.ComponentText("WorkItemsView");
        Assert.Contains("var availability = WorkItemsText.Availability(", text, StringComparison.Ordinal);
        Assert.Contains("""<p class="empty" role="status">@WorkItemsText.Unavailable(availability)</p>""", text, StringComparison.Ordinal);
    }

    /// <summary>Every private <c>[CallerFilePath]</c> path helper still standing in a guard file resolves to the very
    /// path the shared helper gives for the same target: a component to <see cref="MarkupGuards.ComponentPath" />,
    /// the page to <see cref="MarkupGuards.AppPath" />, the app's root to
    /// <see cref="MarkupGuards.DashboardRoot" />, the tests' folder to <see cref="MarkupGuards.TestsRoot" />, and
    /// anything else of the app - a folder or a file named inside <c>src/Dashboard</c> - to
    /// <see cref="MarkupGuards.SourcePath" />. A helper whose target the shared helper has no name for fails, so the
    /// shapes cannot multiply unnoticed, and neither may the count of the copies.</summary>
    [Fact]
    public void EveryPathHelperTheGuardFilesKeepIsThePathTheSharedHelperResolvesTo()
    {
        var helpers = PathHelpersOfTheGuardFiles();
        var components = 0;
        var pages = 0;
        var roots = 0;
        var tests = 0;
        var sources = 0;

        Assert.True(
            helpers.Count >= 80,
            $"only {helpers.Count} private [CallerFilePath] path helper(s) were found: the walk no longer reaches the guard files, so this test proves nothing");
        Assert.True(
            helpers.Count <= 93,
            $"{helpers.Count} private [CallerFilePath] path helpers stand in the guard files: the navigation may only shrink as the files call MarkupGuards, never grow");

        foreach (var (helper, found) in helpers)
        {
            var relative = Path.GetRelativePath(Sources, found).Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            var where = $"{helper.DeclaringType!.Name}.{helper.Name} -> {found}";

            Assert.True(Directory.Exists(found) || File.Exists(found), $"{where} is neither a file nor a folder.");
            Assert.False(relative.StartsWith("..", StringComparison.Ordinal), $"{where} walks out of {Sources}.");

            if (relative.StartsWith(Path.Combine("Dashboard", "Components"), StringComparison.Ordinal)
                && relative.EndsWith(".razor", StringComparison.Ordinal))
            {
                components++;
                var component = Path.GetFileNameWithoutExtension(relative);
                Assert.True(MarkupGuards.ComponentPath(component) == found, $"{where} is not MarkupGuards.ComponentPath(\"{component}\").");
            }
            else if (relative.Equals(Path.Combine("Dashboard", "App.razor"), StringComparison.Ordinal))
            {
                pages++;
                Assert.True(MarkupGuards.AppPath() == found, $"{where} is not {MarkupGuards.AppPath()}.");
            }
            else if (relative.Equals("Dashboard", StringComparison.Ordinal))
            {
                roots++;
                Assert.True(MarkupGuards.DashboardRoot() == found, $"{where} is not {MarkupGuards.DashboardRoot()}.");
            }
            else if (relative.Equals("Dashboard.Tests", StringComparison.Ordinal))
            {
                tests++;
                Assert.True(MarkupGuards.TestsRoot() == found, $"{where} is not {MarkupGuards.TestsRoot()}.");
            }
            else if (relative.StartsWith("Dashboard", StringComparison.Ordinal))
            {
                sources++;
                var inside = relative[("Dashboard" + Path.DirectorySeparatorChar).Length..];
                Assert.True(MarkupGuards.SourcePath(inside) == found, $"{where} is not MarkupGuards.SourcePath(\"{inside}\").");
            }
            else
            {
                Assert.Fail($"{where} is a shape MarkupGuards has no name for.");
            }
        }

        Assert.True(components >= 40, $"only {components} component helpers were pinned against MarkupGuards.ComponentPath: the walk read the wrong files");
        Assert.True(pages >= 4, $"only {pages} App.razor helpers were pinned against MarkupGuards.AppPath");
        Assert.True(roots >= 25, $"only {roots} Dashboard-root helpers were pinned against MarkupGuards.DashboardRoot");
        Assert.True(tests >= 3, $"only {tests} tests-root helpers were pinned against MarkupGuards.TestsRoot");
        Assert.Equal(helpers.Count, components + pages + roots + tests + sources);
    }

    // ----- the line

    /// <summary>
    /// Every private <c>LineOf(string, int)</c> still standing in a guard file says exactly what the shared
    /// <see cref="MarkupGuards.LineOf" /> says - on the real markup the guards read and on a hand-written line of
    /// its own, at the first position, at each line break and past it, and at the text's end. This is the pin that
    /// lets a file drop its copy and call <see cref="MarkupGuards.LineOf" /> without the <c>file:line</c> of its
    /// offender messages moving. As with the count, the set of the copies is pinned from above as well as below:
    /// it may only shrink, never grow, so the duplication this change homes cannot spread while the follow-up
    /// files retire the copies one file at a time.
    /// </summary>
    [Fact]
    public void TheSharedLineSaysWhatEveryFilesOwnCopyOfItSays()
    {
        var copies = MethodsNamed(nameof(MarkupGuards.LineOf), typeof(int), LineSignature);

        Assert.True(
            copies.Count >= 26,
            $"only {copies.Count} private copy(ies) of LineOf were found: the walk no longer reaches the guard files, so this test proves nothing");
        Assert.True(
            copies.Count <= 28,
            $"{copies.Count} private copies of LineOf stand in the guard files: the duplication may only shrink as the files call MarkupGuards.LineOf, never grow");

        foreach (var copy in copies)
        {
            foreach (var (text, at) in LineSamples())
            {
                var expected = MarkupGuards.LineOf(text, at);

                var said = (int)copy.Invoke(null, [text, at])!;
                Assert.True(
                    expected == said,
                    $"{copy.DeclaringType!.Name}.{copy.Name} says {said} and MarkupGuards.LineOf says {expected} about position {at}.");
            }
        }
    }

    // ----- the two walks

    /// <summary>The text and the needles the shared count and its copies are set against: the markup the guards
    /// actually read, and the small words their assertions turn on.</summary>
    private static IEnumerable<(string Text, string Value)> CountingSamples()
    {
        var markup = new[] { MarkupGuards.AppText(), MarkupGuards.ComponentText("WorkItemsView"), MarkupGuards.ComponentText("DeployableFacts"), MarkupGuards.ComponentText("ClusterBadge"), MarkupGuards.ComponentText("SharedCost") };
        var needles = new[] { "@ReloadTopologyLabel</button>", "<p>", "role=\"status\"", "state--", "[Parameter]", "<CodeCard ", "visually-hidden", "\n", "a", "aa", "aaa" };

        return markup.SelectMany(text => needles.Select(value => (text, value)));
    }

    /// <summary>The texts and the positions the shared line and its copies are set against: the markup the guards
    /// actually read, and a hand-written line of its own, each at the first position, at the line break and past it,
    /// and at the text's end - so every line the texts hold is counted and the first line is never dropped.</summary>
    private static IEnumerable<(string Text, int At)> LineSamples()
    {
        var texts = new[] { MarkupGuards.AppText(), MarkupGuards.ComponentText("WorkItemsView"), MarkupGuards.ComponentText("DeployableFacts"), "a\nb\nc\n" };

        return texts.SelectMany(text => new[] { 0, 1, text.Length - 1, text.Length }.Select(at => (text, at)));
    }

    /// <summary>The signature of the count the guard files copied.</summary>
    private static readonly Type[] CountSignature = [typeof(string), typeof(string)];

    /// <summary>The signature of the line helper the guard files copied.</summary>
    private static readonly Type[] LineSignature = [typeof(string), typeof(int)];

    /// <summary>The private static methods of the test assembly under the name and the signature named - the copies
    /// the shared helper answers for.</summary>
    private static List<MethodInfo> MethodsNamed(string name, Type returns, Type[] parameters) =>
        [.. TypesOfTheGuardFiles()
            .SelectMany(type => type.GetMethods(Private))
            .Where(method => method.Name == name
                             && method.ReturnType == returns
                             && method.GetParameters().Select(parameter => parameter.ParameterType).SequenceEqual(parameters))];

    /// <summary>Every private static <c>string</c>-taking-<c>[CallerFilePath]</c> helper of the guard files: the
    /// navigation and the two readings of a file's text alike.</summary>
    private static List<MethodInfo> CallersOfTheGuardFiles() =>
        [.. TypesOfTheGuardFiles()
            .SelectMany(type => type.GetMethods(Private))
            .Where(method => method.ReturnType == typeof(string))
            .Where(method => method.GetParameters() is [var single]
                             && single.ParameterType == typeof(string)
                             && single.HasDefaultValue
                             && single.GetCustomAttribute<CallerFilePathAttribute>() is not null)];

    /// <summary>An anchor standing in the tests' own folder. A <c>[CallerFilePath]</c> parameter is filled by the
    /// compiler at the call and its metadata default is the empty string, so reflection hands out no caller's path
    /// of its own; the walk therefore hands this anchor. Every guard file stands in the tests' folder and every copy
    /// reads only the folder of the path it is handed, so the answer a copy gives to the anchor is the answer it
    /// gives to the file that calls it.</summary>
    private static string Anchor => Path.Combine(MarkupGuards.TestsRoot(), $"{nameof(MarkupGuardSupportTests)}.cs");

    /// <summary>The <c>[CallerFilePath]</c> helpers that answer with a path, each with the path it answers: the
    /// navigation this change homes. The helpers that answer with a slice of the app's source rather than with a path
    /// are named by <see cref="NotNavigation" />, so they are said there rather than missed here, and no new shape
    /// joins either set unnoticed.</summary>
    private static List<(MethodInfo Helper, string Found)> PathHelpersOfTheGuardFiles()
    {
        var navigation = new List<(MethodInfo, string)>();
        var theRest = new List<string>();

        foreach (var helper in CallersOfTheGuardFiles())
        {
            var answer = (string)helper.Invoke(null, [Anchor])!;
            if (Path.IsPathRooted(answer))
            {
                navigation.Add((helper, Path.GetFullPath(answer)));
            }
            else
            {
                theRest.Add($"{helper.DeclaringType!.Name}.{helper.Name}");
            }
        }

        Assert.Equal(NotNavigation, theRest.Order(StringComparer.Ordinal).ToArray());
        return navigation;
    }

    /// <summary>The two <c>[CallerFilePath]</c> helpers of the guard files that read a file's text rather than name a
    /// path. They are no navigation the shared helper answers for, and they are these two alone.</summary>
    private static readonly string[] NotNavigation =
    [
        "HourlyCheckTextTemplateTests.HourlyCheckTextSource",
        "HourlyCheckTextTemplateTests.TitleBody",
    ];

    /// <summary>Every type of the running test assembly, the guard files and their fixtures alike.</summary>
    private static List<Type> TypesOfTheGuardFiles() => [.. typeof(MarkupGuards).Assembly.GetTypes()];

    private static void AssertDirectories(params string[] paths) =>
        Assert.All(paths, path =>
        {
            Assert.True(Directory.Exists(path), $"{path} is no folder.");
            AssertInsideSources(path);
        });

    private static void AssertFiles(params string[] paths) =>
        Assert.All(paths, path =>
        {
            Assert.True(File.Exists(path), $"{path} is no file.");
            AssertInsideSources(path);
        });

    private static void AssertInsideSources(string path) =>
        Assert.False(
            Path.GetRelativePath(Sources, path).StartsWith("..", StringComparison.Ordinal),
            $"{path} walks out of {Sources}.");
}
