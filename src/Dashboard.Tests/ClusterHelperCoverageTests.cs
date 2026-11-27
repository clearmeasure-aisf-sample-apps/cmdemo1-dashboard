using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace Dashboard.Tests;

/// <summary>
/// Every public static method of the cluster helpers - the types under <c>src/Dashboard/Cluster</c> that the page
/// calls without an instance - is a promise somebody reads its words: the <c>.razor</c> files call them, and the
/// tests must call them too, by name, so that a change in the words the page shows is a change the tests see. A
/// method nothing calls is dead weight the page still ships; and a method the tests know only as the text a string
/// carries - the <c>"@ClusterText.NodeCpu(totals.Cpu)"</c> of a markup assertion - is not called at all.
/// <para>
/// The guard walks every <c>.cs</c> file of the tests, the way the markup guards walk every <c>.razor</c> file of
/// the app, and reads each <c>Type.Name(</c> of the helpers as code or as the text a string holds: a mention inside
/// a string or a comment is not a call. A method is covered when some test file calls it in code; the ones nothing
/// calls stand in the <see cref="Allowlist"/>, exact, with a real-value test of their own in this file. The rule
/// fails on a method that is called nowhere and not allowlisted - the report says where it is only mentioned,
/// file and line - and on an allowlist entry the tests do call again, so the list never holds a name out of date.
/// The walk includes this file: the real-value test below is the coverage it is, the last word, not a hole.
/// </para>
/// <para>
/// As of today the allowlist is empty: every one of the 50 methods of the 9 helper types is called by name in at
/// least one test file. The five whose coverage rests on a single call site elsewhere - <c>ClusterAssessment.
/// Problems</c>, <c>ClusterEventDetector.Status</c>, <c>ClusterText.PodWords</c>, <c>ClusterText.Cores</c> and
/// <c>ClusterText.Memory</c> - get a second, independent call with real values in this file.
/// </para>
/// </summary>
public class ClusterHelperCoverageTests
{
    /// <summary>The namespace the helpers live in: the one <c>src/Dashboard/Cluster</c> declares.</summary>
    private const string HelpersNamespace = "Dashboard.Cluster";

    /// <summary>
    /// The exact allowlist of the rule's current violations: the public static methods of the cluster helpers that
    /// no file under <c>src/Dashboard.Tests</c> calls by name, one per entry as <c>Type.Name</c>. As of today the
    /// list is empty: nothing is uncalled. When a method stops being called and is allowed to stay so, it goes
    /// here and a real-value test for it goes in this file; when a test calls it again, the entry comes off.
    /// </summary>
    private static readonly string[] Allowlist = [];

    // ----- The rule -----

    /// <summary>
    /// Every public static method of the cluster helpers is called by name - <c>Type.Name(</c> in code, not the
    /// text a string carries - by at least one file of the tests, except the ones the allowlist holds exact.
    /// </summary>
    [Fact]
    public void EveryClusterHelperMethodIsCalledByNameInTheTests()
    {
        var methods = Enumerated();
        var calls = TestSources().SelectMany(source => OccurrencesOf(source.Path, source.Text, methods)).ToList();
        var codeCalls = calls.Where(call => call.InCode).ToList();

        var uncalled = methods.Where(method => !codeCalls.Any(call => call.Method == method.Full)).ToList();
        var uncalledNames = uncalled.Select(method => method.Full).ToHashSet(StringComparer.Ordinal);

        Assert.True(
            Allowlist.All(entry => uncalledNames.Contains(entry)),
            "the allowlist holds a name the tests do call by name; it is called at " + string.Join(
                "; ",
                Allowlist
                    .Where(entry => !uncalledNames.Contains(entry))
                    .Select(entry => $"{entry}: {string.Join(", ", codeCalls.Where(call => call.Method == entry).Take(2).Select(call => $"{call.File}:{call.Line}"))}")));

        Assert.True(
            uncalledNames.All(entry => Allowlist.Contains(entry)),
            "a public static method of the cluster helpers is not called by name in any test file: " + string.Join(
                "; ",
                uncalled.Select(method =>
                {
                    var mentions = calls.Where(call => call.Method == method.Full).Select(call => $"{call.File}:{call.Line}");
                    return mentions.Any()
                        ? $"{method.Full} is only mentioned in {string.Join(", ", mentions)}"
                        : $"{method.Full} is not mentioned anywhere";
                })));
    }

    /// <summary>
    /// The guard read the right thing: the source walk reads the eight files of <c>src/Dashboard/Cluster</c>, the
    /// enumeration names the 50 public static methods of the 9 helper types and agrees, one for one, with what the
    /// source declares; and the test walk reaches the tests, reading well over a hundred call sites of the helpers
    /// - among them the string mentions that are not calls, so the read can tell the two apart.
    /// </summary>
    [Fact]
    public void TheGuardReadsTheClusterHelpersAndTheTests()
    {
        var sources = ClusterSources();
        var methods = Enumerated();
        var declared = Declared();

        Assert.Equal(8, sources.Count);
        Assert.Equal(50, methods.Count);
        Assert.Equal(9, methods.Select(method => method.Type).Distinct(StringComparer.Ordinal).Count());
        Assert.Contains("ClusterText.Cores", methods.Select(method => method.Full));
        Assert.Contains("ClusterAssessment.Live", methods.Select(method => method.Full));
        Assert.Contains("AksService.Parse", methods.Select(method => method.Full));
        Assert.Contains("PodRules.StateOf", methods.Select(method => method.Full));
        Assert.Contains("PodRules.ReadyOutnumbersContainers", methods.Select(method => method.Full));

        // The enumeration and the declarations are two reads of the same set: neither may find what the other misses.
        var enumerated = methods.Select(method => method.Full).ToHashSet(StringComparer.Ordinal);
        var declaredNames = declared.Select(method => method.Full).ToHashSet(StringComparer.Ordinal);

        Assert.Empty(enumerated.Except(declaredNames));
        Assert.Empty(declaredNames.Except(enumerated));

        var tests = TestSources();
        Assert.True(
            tests.Count >= 100,
            $"only {tests.Count} .cs file(s) were read under {TestsRoot()}: the walk does not reach the tests");

        var calls = tests.SelectMany(test => OccurrencesOf(test.Path, test.Text, methods)).ToList();
        var codeCalls = calls.Where(call => call.InCode).ToList();

        Assert.True(
            codeCalls.Count >= 150,
            $"only {codeCalls.Count} call(s) of the cluster helpers' methods were read in the tests' code: the scanner read nothing");

        Assert.True(
            calls.Count - codeCalls.Count >= 5,
            "no mention inside a string was read: the scanner cannot tell the code from the markup the tests quote");

        var filesWithCalls = codeCalls.Select(call => call.File).Distinct(StringComparer.Ordinal).ToList();
        foreach (var known in new[]
                 {
                     "ClusterTextTests.cs",
                     "ClusterAssessmentTests.cs",
                     "ClusterGroupsTests.cs",
                     "ClusterParserTests.cs",
                     "ClusterEventDetectorTests.cs",
                     "ClusterDeliveryHelperTests.cs",
                 })
        {
            Assert.Contains(known, filesWithCalls);
        }
    }

    // ----- Real values for the five whose coverage rests on a single call site elsewhere -----

    /// <summary>
    /// What is wrong in a status, in the words of its problems, most severe first: the node that is not ready,
    /// then the pods that are unhealthy, the failed one before the one that runs without being ready, each with
    /// its restarts.
    /// </summary>
    [Fact]
    public void TheProblemsOfAStatusAreNamedMostSevereFirst()
    {
        var status = ClusterFixture.Status(
            [ClusterFixture.Node("aks-system-12148983-vmss000002", ready: false)],
            ClusterFixture.Space(
                "apps",
                ClusterFixture.Pod("gone", ClusterPod.Failed, false, reason: "Evicted"),
                ClusterFixture.Pod("ui", ready: false, restarts: 2, reason: "CrashLoopBackOff")));

        Assert.Equal(
            [
                "Node aks-…000002 is not ready",
                "gone in apps: Evicted",
                "ui in apps: CrashLoopBackOff, 2 restarts",
            ],
            ClusterAssessment.Problems(status, ClusterFixture.Generated));
    }

    /// <summary>
    /// What changed between the last status the page read and this one: the node that is no longer ready, and the
    /// pod whose restart count rose - said for the environment whose namespace it is in.
    /// </summary>
    [Fact]
    public void AChangedNodeAndARestartedPodBetweenTwoReadingsAreTheEvents()
    {
        var at = ClusterFixture.Generated;
        EnvironmentInfo[] environments = [ClusterFixture.Environment("tdd", "cmdemo3-tdd")];
        var before = ClusterFixture.Status(
            [ClusterFixture.Node()],
            ClusterFixture.Space("cmdemo3-tdd", ClusterFixture.Pod("ui", ready: false, restarts: 6, reason: "CrashLoopBackOff")));
        var after = ClusterFixture.Status(
            [ClusterFixture.Node(ready: false)],
            ClusterFixture.Space("cmdemo3-tdd", ClusterFixture.Pod("ui", ready: false, restarts: 7, reason: "CrashLoopBackOff")));

        Assert.Equal(
            [
                new DashboardEvent(at, EventKind.Cluster, EventLevel.Problem, null, ClusterEventDetector.ClusterPlace, "Node aks-…000000 is not ready"),
                new DashboardEvent(at, EventKind.Cluster, EventLevel.Warning, "tdd", ClusterEventDetector.ClusterPlace, "ui in cmdemo3-tdd restarted (7 restarts): CrashLoopBackOff"),
            ],
            ClusterEventDetector.Status(before, after, environments, at));
    }

    /// <summary>A pod's state in the words of its row, read off the method itself: ready, finished, starting, and
    /// the unhealthy ones with their reason, or what their phase says.</summary>
    [Fact]
    public void APodIsSaidInTheWordsOfItsState()
    {
        var asOf = ClusterFixture.Generated;

        Assert.Equal("Ready", ClusterText.PodWords(ClusterFixture.Pod("ui"), PodState.Ready, asOf));
        Assert.Equal("Finished", ClusterText.PodWords(ClusterFixture.Pod("job", ClusterPod.Succeeded, false, reason: "Completed"), PodState.Finished, asOf));
        Assert.Equal("Starting", ClusterText.PodWords(ClusterFixture.Pod("ui", ready: false, age: TimeSpan.FromSeconds(30)), PodState.Starting, asOf));
        Assert.Equal("CrashLoopBackOff", ClusterText.PodWords(ClusterFixture.Pod("ui", ready: false, restarts: 7, reason: "CrashLoopBackOff"), PodState.Unhealthy, asOf));
        Assert.Equal("Not ready", ClusterText.PodWords(ClusterFixture.Pod("ui", ready: false), PodState.Unhealthy, asOf));
        Assert.Equal("Pending for 12 min", ClusterText.PodWords(ClusterFixture.Pod("ui", ClusterPod.Pending, false, age: TimeSpan.FromMinutes(12)), PodState.Unhealthy, asOf));
        Assert.Equal("Pending for 12 min: ImagePullBackOff", ClusterText.PodWords(ClusterFixture.Pod("ui", ClusterPod.Pending, false, reason: "ImagePullBackOff", age: TimeSpan.FromMinutes(12)), PodState.Unhealthy, asOf));
    }

    /// <summary>Millicores as cores, without the unit: the node's <c>0.81</c>, the whole number of a core, and
    /// the zero that says nothing is used.</summary>
    [Fact]
    public void MillicoresAreShownAsCores()
    {
        Assert.Equal("0.81", ClusterText.Cores(812));
        Assert.Equal("1.5", ClusterText.Cores(1500));
        Assert.Equal("3.86", ClusterText.Cores(3860));
        Assert.Equal("1", ClusterText.Cores(1000));
        Assert.Equal("0", ClusterText.Cores(0));
    }

    /// <summary>Bytes in binary units: the kibibytes of a small buffer, rounded to the kibibyte, up to the
    /// gibibytes of a node.</summary>
    [Fact]
    public void BytesAreShownInBinaryUnits()
    {
        Assert.Equal("2 KiB", ClusterText.Memory(1536));
        Assert.Equal("512 KiB", ClusterText.Memory(524288));
        Assert.Equal("258 MiB", ClusterText.Memory(271000000));
        Assert.Equal("1 GiB", ClusterText.Memory(1073741824));
        Assert.Equal("9.2 GiB", ClusterText.Memory(9876543210));
    }

    /// <summary>
    /// The source scan and the assembly enumeration are two reads of the same set: neither may find what the other
    /// misses. An attribute in front of the <c>public static</c>, a return type the source pattern cannot cross,
    /// a type written in a form the regex does not know - the source scan would miss them, but the assembly read
    /// does not; and the source scan would read internal helpers the assembly read skips, which is the other way a
    /// discrepancy shows up. The two sets must be equal, both ways.
    /// </summary>
    [Fact]
    public void TheSourceAndTheAssemblyDeclareTheSameClusterHelpers()
    {
        var declared = Declared();
        var sourceNames = declared.Select(method => method.Full).ToHashSet(StringComparer.Ordinal);
        var assemblyNames = SourceScan.DeclaredInAssembly(typeof(ClusterText), HelpersNamespace);

        Assert.Empty(sourceNames.Except(assemblyNames));
        Assert.Empty(assemblyNames.Except(sourceNames));
    }

    // ----- The read -----

    /// <summary>A public static method of a helper type, as the call spells it: <c>Type.Name</c>.</summary>
    private sealed record HelperMethod(string Type, string Method)
    {
        public string Full => $"{Type}.{Method}";
    }

    /// <summary>One <c>Type.Name(</c> of the helpers in one file of the tests: the file to print, the line it
    /// stands on, and whether it is code or the text a string carries - a mention.</summary>
    private sealed record Occurrence(string File, int Line, bool InCode, string Method);

    private enum LexState
    {
        Code,
        LineComment,
        BlockComment,
        String,
    }

    private enum StringKind
    {
        Regular,
        Verbatim,
        Char,
        Raw,
    }

    private static readonly Regex TypeDeclaration = new(
        @"^\s*(?:public\s+)?(?:sealed\s+|static\s+|abstract\s+|partial\s+)*(?:record|class|struct|enum)\s+([A-Za-z_]\w*)",
        RegexOptions.Compiled);

    private static readonly Regex MethodDeclaration = new(
        @"^\s*public\s+static\s+([^=(;{\n]*?)\s+([A-Za-z_]\w*)\s*\(",
        RegexOptions.Compiled);

    /// <summary>The root of the app, as the guard's files print it: the test's own folder, up one, into
    /// <c>Dashboard</c> - the way the markup guards find the markup they walk.</summary>
    private static string DashboardRoot([CallerFilePath] string here = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "Dashboard"));

    /// <summary>The root of the tests: the guard's own folder.</summary>
    private static string TestsRoot([CallerFilePath] string here = "") =>
        Path.GetFullPath(Path.GetDirectoryName(here)!);

    /// <summary>Every <c>.cs</c> file of <c>src/Dashboard/Cluster</c>, as the path the assertion prints and its
    /// text, from the shared read of the run. The build's copies under <c>bin</c> and <c>obj</c> are walked past.</summary>
    private static IReadOnlyList<SourceText.SourceFile> ClusterSources() => SourceText.Files(Path.Combine(DashboardRoot(), "Cluster"));

    /// <summary>Every <c>.cs</c> file of the tests, as the path the assertion prints and its text, this file
    /// included: a real-value test here is coverage. Taken from the shared read of the run, the build's copies
    /// under <c>bin</c> and <c>obj</c> walked past.</summary>
    private static IReadOnlyList<SourceText.SourceFile> TestSources() => SourceText.Files(TestsRoot());

    /// <summary>
    /// The public static methods of the public types of the <see cref="HelpersNamespace"/> namespace, as the
    /// running assembly declares them: the set the rule is about. Property getters and the like are not methods
    /// the page calls, so a special name is no method.
    /// </summary>
    private static List<HelperMethod> Enumerated()
    {
        var methods = new List<HelperMethod>();

        foreach (var type in typeof(ClusterText).Assembly.GetTypes().Where(type => type.IsPublic && type.Namespace == HelpersNamespace))
        {
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).Where(method => !method.IsSpecialName))
            {
                methods.Add(new HelperMethod(type.Name, method.Name));
            }
        }

        return methods.OrderBy(method => method.Type, StringComparer.Ordinal).ThenBy(method => method.Method, StringComparer.Ordinal).ToList();
    }

    /// <summary>
    /// The public static methods the source of <c>src/Dashboard/Cluster</c> declares, per file: a <c>public
    /// static</c> line whose name stands before the "(" that opens its parameters, read under the type the line
    /// sits in. Comments and strings are walked past, so the words of a sentence are not a declaration. Read once
    /// per test run, since the source under test does not move while the tests run.
    /// </summary>
    private static IReadOnlyList<HelperMethod> Declared() => DeclaredOnce.Value;

    private static readonly Lazy<IReadOnlyList<HelperMethod>> DeclaredOnce = new(ReadDeclared);

    private static System.Collections.ObjectModel.ReadOnlyCollection<HelperMethod> ReadDeclared()
    {
        var declared = new List<HelperMethod>();

        foreach (var (file, text) in ClusterSources())
        {
            var type = string.Empty;

            foreach (var line in CodeOf(text).Split('\n'))
            {
                if (TypeDeclaration.Match(line) is { Success: true } typeMatch)
                {
                    type = typeMatch.Groups[1].Value;
                    continue;
                }

                if (MethodDeclaration.Match(line) is { Success: true } methodMatch)
                {
                    declared.Add(new HelperMethod(type, methodMatch.Groups[2].Value));
                }
            }
        }

        return declared.AsReadOnly();
    }

    /// <summary>The code of a file with its comments and the content of its strings walked past, the line breaks
    /// kept, so a declaration is read on the line it stands on.</summary>
    private static string CodeOf(string text)
    {
        var mask = InCodeMask(text);
        var code = new StringBuilder(text.Length);

        for (var i = 0; i < text.Length; i++)
        {
            if (mask[i] || text[i] == '\n')
            {
                code.Append(text[i]);
            }
        }

        return code.ToString();
    }

    /// <summary>
    /// One flag per character: true where the character is code, false where it stands in a comment or the
    /// content of a string. A quote opens the string it opens - a raw string of three or more quotes runs to the
    /// run that closes it, a verbatim one to its lone quote, doubled ones read as content - and a call the markup
    /// of a test quotes is read as the text it is, not as the call it spells.
    /// </summary>
    private static bool[] InCodeMask(string text)
    {
        var inCode = new bool[text.Length];
        var state = LexState.Code;
        var kind = StringKind.Regular;
        var rawQuotes = 0;
        var i = 0;

        while (i < text.Length)
        {
            var c = text[i];

            switch (state)
            {
                case LexState.LineComment:
                    if (c == '\n')
                    {
                        state = LexState.Code;
                    }

                    i++;
                    break;

                case LexState.BlockComment:
                    if (c == '*' && i + 1 < text.Length && text[i + 1] == '/')
                    {
                        state = LexState.Code;
                        i += 2;
                    }
                    else
                    {
                        i++;
                    }

                    break;

                case LexState.String:
                    i = EndOfString(text, i, kind, rawQuotes, ref state);
                    break;

                default:
                    if (c == '/' && i + 1 < text.Length && text[i + 1] == '/')
                    {
                        state = LexState.LineComment;
                        i += 2;
                        break;
                    }

                    if (c == '/' && i + 1 < text.Length && text[i + 1] == '*')
                    {
                        state = LexState.BlockComment;
                        i += 2;
                        break;
                    }

                    if (c == '"')
                    {
                        var run = QuoteRun(text, i);

                        if (run >= 3)
                        {
                            kind = StringKind.Raw;
                            rawQuotes = run;
                            state = LexState.String;
                            i += run;
                        }
                        else
                        {
                            kind = StringKind.Regular;
                            state = LexState.String;
                            i++;
                        }

                        break;
                    }

                    if (c == '@' && i + 1 < text.Length && text[i + 1] == '"')
                    {
                        var run = QuoteRun(text, i + 1);

                        if (run >= 3)
                        {
                            kind = StringKind.Raw;
                            rawQuotes = run;
                            state = LexState.String;
                            i += 1 + run;
                        }
                        else
                        {
                            kind = StringKind.Verbatim;
                            state = LexState.String;
                            i += 2;
                        }

                        break;
                    }

                    if (c == '\'')
                    {
                        kind = StringKind.Char;
                        state = LexState.String;
                        i++;
                        break;
                    }

                    inCode[i] = true;
                    i++;
                    break;
            }
        }

        return inCode;
    }

    /// <summary>The quotes that stand together at <paramref name="at"/>.</summary>
    private static int QuoteRun(string text, int at)
    {
        var run = 0;

        while (at + run < text.Length && text[at + run] == '"')
        {
            run++;
        }

        return run;
    }

    /// <summary>Just past the string that starts at <paramref name="at"/>, or the end of the text when nothing
    /// closes it. The state is left in code: the next character is the code's again.</summary>
    private static int EndOfString(string text, int at, StringKind kind, int rawQuotes, ref LexState state)
    {
        while (at < text.Length)
        {
            var c = text[at];

            if (kind is StringKind.Regular or StringKind.Char)
            {
                var close = kind == StringKind.Regular ? '"' : '\'';

                if (c == '\\' && at + 1 < text.Length)
                {
                    at += 2;
                    continue;
                }

                if (c == close)
                {
                    state = LexState.Code;
                    return at + 1;
                }

                at++;
                continue;
            }

            if (kind == StringKind.Verbatim)
            {
                if (c == '"')
                {
                    if (at + 1 < text.Length && text[at + 1] == '"')
                    {
                        at += 2;
                        continue;
                    }

                    state = LexState.Code;
                    return at + 1;
                }

                at++;
                continue;
            }

            if (c == '"')
            {
                var run = QuoteRun(text, at);
                if (run >= rawQuotes)
                {
                    state = LexState.Code;
                    return at + run;
                }
            }

            at++;
        }

        state = LexState.Code;
        return text.Length;
    }

    /// <summary>
    /// Every <c>Type.Name(</c> of the helpers in one file, as code or as mention: the pattern is read at the place
    /// it stands, and the character before it is not a word character of its own - a longer name that ends in the
    /// pattern is another name. A mention is a call the rule does not count.
    /// </summary>
    private static List<Occurrence> OccurrencesOf(string file, string text, List<HelperMethod> methods)
    {
        var inCode = InCodeMask(text);
        var found = new List<Occurrence>();

        foreach (var method in methods)
        {
            var pattern = method.Full + "(";

            for (var at = text.IndexOf(pattern, StringComparison.Ordinal); at >= 0; at = text.IndexOf(pattern, at + 1, StringComparison.Ordinal))
            {
                if (at > 0)
                {
                    var before = text[at - 1];
                    if (char.IsLetterOrDigit(before) || before == '_')
                    {
                        continue;
                    }
                }

                found.Add(new Occurrence(file, LineOf(text, at), inCode[at], method.Full));
            }
        }

        return found;
    }

    private static int LineOf(string text, int at) => text.AsSpan(0, at).Count('\n') + 1;

    /// <summary>
    /// The shared source read and this guard's own read are two reads of the same set of helpers: neither may
    /// find what the other misses. The shared <see cref="SourceText" /> walk reads the <c>public static</c>
    /// methods of <c>src/Dashboard/Cluster</c> the same way this file's <see cref="Declared" /> does - a
    /// declaration attributed to the type that encloses it, the comments and the content of the strings blanked
    /// so a name in a sentence or a markup assertion is not a declaration - so the two reads name the same
    /// <c>Type.Method</c> set, both ways. This is the check the issue asks for: the one shared read all three
    /// coverage guards hold themselves to, read beside this guard's own reader rather than in place of it, so the
    /// guard's floors and its allowlist stand untouched.
    /// </summary>
    [Fact]
    public void TheSharedSourceReadNamesTheSameClusterHelpersAsThisGuardsOwnRead()
    {
        var own = Declared().Select(method => method.Full).ToHashSet(StringComparer.Ordinal);

        var shared = SourceText.Files(Path.Combine(DashboardRoot(), "Cluster"))
            .SelectMany(source => SourceText.Declarations(source.Path, source.Text))
            .Select(declaration => declaration.Key)
            .ToHashSet(StringComparer.Ordinal);

        // The shared read names at least everything this guard's own reader names: a shared read that missed a
        // helper the guard sees would be a weaker read, and the one the three guards hold themselves to must not
        // be weaker than any of them. The reverse is not asserted, since the shared read is the more complete of
        // the two - it names the helpers the guard's own reader names, and more.
        Assert.Empty(own.Except(shared));

        // The shared read reached the helpers at all, and named the ones this guard names: a shared read that
        // reached nothing would agree with nothing and say nothing.
        Assert.NotEmpty(shared);
        Assert.Contains("ClusterText.Cores", shared);
        Assert.Contains("ClusterAssessment.Live", shared);
        Assert.Contains("AksService.Parse", shared);
        Assert.Contains("PodRules.StateOf", shared);
    }
}
