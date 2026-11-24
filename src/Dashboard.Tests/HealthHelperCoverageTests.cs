using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Dashboard.Tests;

/// <summary>
/// The <c>Health</c> folder is where this app keeps the words: <c>23 of 24</c>, <c>Not responding</c>, <c>the
/// sign-off is with demo approvers; asked 2 h ago</c>. A helper of it is the only kind of code in the solution whose
/// output a viewer reads as a fact about a running system, and a number the page never shows is the same kind of
/// lie as a number drawn from the wrong window. So every <c>public static</c> method of <c>src/Dashboard/Health</c>
/// is called by name by a test (#206): not referenced in a comment, not reached on the way through a larger
/// helper - written out in code, with real values, where a reader can find it.
/// </summary>
/// <para>
/// The guard reads the two folders the way <see cref="TimeElementGuardTests" /> and
/// <see cref="NewTabLinkGuardTests" /> read the markup: every declaration of a <c>public static</c> method of the
/// app's <c>Health</c> folder, attributed to the type that encloses it, against every <c>Type.Method(</c> of the
/// test project's own source. The read is by name rather than by signature, which is what a call writes - and is
/// honest for the two overload pairs of the folder (<c>HistoryText.Describe</c> and <c>JsonRead.Address</c>), whose
/// two overloads each are called, in <see cref="TextTests" /> and <see cref="JsonReadTests" />. Comment lines are
/// skipped on both sides, so a doc comment that only mentions a helper names nothing, and a helper nobody calls is
/// reported with the file and the line it stands on.
/// </para>
/// <para>
/// A guard that read nothing would report nothing and pass, so the scan is held to what the app and its tests
/// really hold today: 136 declarations of 134 helper names, of 39 types over 28 files on one side, and over eight
/// thousand calls of some five hundred and thirty distinct <c>Type.Method</c> names over a hundred and twenty test
/// files on the other - <c>TimeText.Clock</c> among them and a name invented for the occasion not. The eight helpers
/// the issue found untested are then held to the letter as well: each of them is declared once in the app and called
/// by name by <c>this</c> file, and every one of the tests below says what the helper answers for values a viewer
/// would recognise from the shipped fixtures.
/// </para>
/// <para>
/// <c>AvailabilityText.Count(HealthWindow)</c>: <c>23 of 24</c>, and the middle of the hourly line of a container
/// app's box. <c>BuildText.Percent(double)</c> and <c>BuildText.Number(double)</c>: <c>81.2 %</c> and <c>1.9</c>,
/// one decimal at most and always the invariant one. <c>ContainerAppText.Label(ContainerAppState)</c> and
/// <c>ContainerAppText.Title(string, ContainerAppTarget, ContainerAppStatus, TimeZoneInfo)</c>: the badge of a probed
/// container app and the tooltip that says what the page asks, how often and what it cannot see.
/// <c>HourlyCheckText.Title(ContainerAppReports?, DateTimeOffset, TimeZoneInfo)</c>: the same for the pipeline's
/// hourly report under that probe. <c>NodeObservation.Of(TargetStatus)</c>: what the page knew of one endpoint at one
/// check, which is the input of every event it lists. <c>DeploymentText.Detail(Deployment, DateTimeOffset)</c>: the
/// second line of a deployment's tooltip, whose sign-off, since when and by whom.
/// </para>
public partial class HealthHelperCoverageTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 7, 0, 0, 0, TimeSpan.Zero);
    private static readonly TimeZoneInfo Utc = TimeZoneInfo.Utc;
    private static readonly TimeZoneInfo MinusFive = TimeZoneInfo.CreateCustomTimeZone("test", TimeSpan.FromHours(-5), "test", "test");

    /// <summary>The name this file is known by to the scan, which is what the tests of the eight helpers below are.</summary>
    private const string Here = "HealthHelperCoverageTests.cs";

    /// <summary>
    /// The helpers of <c>Health</c> that no test is asked to call: none today, since the eight the issue found are
    /// tested below. A helper that really cannot be called from a test is written here as <c>Type.Method</c>, with
    /// the reason in the line above it; the guard compares the whole list with what it reads, so an entry that stops
    /// being a gap fails it just the same as a gap that is not held - the list can neither rot nor grow quietly.
    /// </summary>
    private static readonly string[] Gaps = [];

    // ---------------------------------------------------------------- the guard

    /// <summary>
    /// Every <c>public static</c> method of the app's <c>Health</c> folder is called by name by some file of the test
    /// project - its type and its method written with a <c>(</c> after them, in code rather than in a comment.
    /// </summary>
    [Fact]
    public void EveryHealthHelperIsCalledByNameByATest()
    {
        Assert.Equal(Gaps, Offences());
        Assert.Empty(Gaps);
    }

    /// <summary>
    /// The guard read both folders: 136 declarations of 134 helper names, of 39 types over 28 files on one side -
    /// <c>TimeText.Clock</c>, <c>BuildInfo.Parse</c>, <c>HealthClassifier.Label</c> among them - and eight thousand
    /// nine hundred calls of some five hundred and thirty distinct <c>Type.Method</c> names over a hundred and twenty
    /// test files on the other. A scan that reached far fewer had read the wrong place, and the clean verdict above
    /// would have meant nothing; every floor is set well below what the repo holds today, so the day a helper is
    /// added the test still says what it read.
    /// </summary>
    [Fact]
    public void TheGuardReadsTheHelpersOfTheAppAndTheTestsOfTheProject()
    {
        var helpers = Declared();
        var calls = References();

        Assert.True(helpers.Count >= 100, $"only {helpers.Count} public static helper(s) were read under {HealthRoot()}: the guard is checking nothing");
        Assert.True(
            helpers.Select(helper => helper.File).Distinct(StringComparer.Ordinal).Count() >= 20,
            $"the {helpers.Count} helper(s) read live in {helpers.Select(helper => helper.File).Distinct(StringComparer.Ordinal).Count()} file(s): the walk does not reach the app");
        Assert.True(
            helpers.Select(helper => helper.Type).Distinct(StringComparer.Ordinal).Count() >= 25,
            $"the {helpers.Count} helper(s) belong to {helpers.Select(helper => helper.Type).Distinct(StringComparer.Ordinal).Count()} type(s): the types were not read");

        foreach (var known in new[] { "TimeText.Clock", "TimeText.Ago", "BuildInfo.Parse", "HealthClassifier.Label", "DeploymentText.Sentence" })
        {
            Assert.Contains(helpers, helper => helper.Key == known);
        }

        // Every declaration found a type to belong to, so no name is reported against a type the file never opened.
        Assert.All(helpers, helper => Assert.True(helper.Type.Length > 0, $"{helper.Report} was read with no enclosing type: the scan does not know where it stands"));

        Assert.True(calls.Count >= 1_000, $"only {calls.Count} call site(s) were read under {TestRoot()}: the guard would call anything untested");
        Assert.True(
            calls.Select(call => call.File).Distinct(StringComparer.Ordinal).Count() >= 50,
            $"the {calls.Count} call site(s) were read in {calls.Select(call => call.File).Distinct(StringComparer.Ordinal).Count()} file(s): the walk does not reach the tests");

        List<string> named = [.. calls.Select(call => call.Key).Distinct(StringComparer.Ordinal)];

        Assert.Contains("TimeText.Clock", named);
        Assert.Contains("AvailabilityText.Percent", named);
        Assert.Contains("BuildText.SharePercent", named);
        Assert.DoesNotContain("TimeText.TheMomentThatNeverWas", named);
        Assert.DoesNotContain("HealthClassifier.TheMomentThatNeverWas", named);
    }

    /// <summary>
    /// The eight helpers the issue found are named here, in code, and not by a comment mentioning them: each of them
    /// is declared once in the app and called by name by this file, so the tests below cannot be deleted and leave
    /// the guard above passing on a doc comment.
    /// </summary>
    [Fact]
    public void TheHelpersTheIssueFoundAreCalledByNameByTheTestsBelow()
    {
        var calls = ReferencesOf(Here, SourceOf(Here));
        var helpers = Declared();

        foreach (var (type, method) in HelpersOfTheIssue)
        {
            Assert.Single(helpers, helper => helper.Type == type && helper.Method == method);
            Assert.True(
                Named(calls, type, method),
                $"{type}.{method} is not called by name in {Here}: the test that owes it is gone, and the guard above would pass on a comment");
        }
    }

    /// <summary>
    /// The source scan and the assembly enumeration are two reads of the same set: neither may find what the other
    /// misses. An attribute in front of the <c>public static</c>, a return type the source pattern cannot cross,
    /// a type written in a form the regex does not know - the source scan would miss them, but the assembly read
    /// does not; and the source scan would read internal helpers the assembly read skips, which is the other way a
    /// discrepancy shows up. The two sets must be equal, both ways.
    /// </summary>
    [Fact]
    public void TheSourceAndTheAssemblyDeclareTheSameHealthHelpers()
    {
        var sourceNames = DeclaredPublic().Select(helper => helper.Key).ToHashSet(StringComparer.Ordinal);
        var assemblyNames = SourceScan.DeclaredInAssembly(typeof(TimeText), "Dashboard.Health");

        Assert.Empty(sourceNames.Except(assemblyNames));
        Assert.Empty(assemblyNames.Except(sourceNames));
    }

    /// <summary>
    /// The public static methods the source of <c>src/Dashboard/Health</c> declares, per file: a <c>public
    /// static</c> line whose name stands before the "(" that opens its parameters, read under the type the line
    /// sits in - but only for public types, which is what the assembly read sees. An internal type (like
    /// <c>JsonRead</c>) has its public static methods read by the scan and skipped by the assembly; this read
    /// skips them too, so the two reads can be compared.
    /// </summary>
    private static List<Helper> DeclaredPublic()
    {
        var helpers = new List<Helper>();

        foreach (var (file, text) in SourceFiles(HealthRoot()))
        {
            var line = 0;
            var type = string.Empty;
            var typeIsPublic = false;

            foreach (var raw in text.Split('\n'))
            {
                line++;

                var code = raw.Trim();

                if (IsComment(code))
                {
                    continue;
                }

                if (TypeDeclarationPublic().Match(code) is { Success: true } declared)
                {
                    type = declared.Groups[1].Value;
                    typeIsPublic = true;
                    continue;
                }

                if (TypeDeclarationInternal().Match(code) is { Success: true } declaredInternal)
                {
                    type = declaredInternal.Groups[1].Value;
                    typeIsPublic = false;
                    continue;
                }

                if (MethodDeclaration().Match(code) is { Success: true } method)
                {
                    if (typeIsPublic)
                    {
                        helpers.Add(new Helper(file, line, type, method.Groups[1].Value));
                    }
                }
            }
        }

        return helpers;
    }

    [GeneratedRegex(@"^public\s+(?:(?:sealed|static|partial|readonly|abstract)\s+)*(?:class|record|struct|enum|interface)\s+([A-Za-z_]\w*)")]
    private static partial Regex TypeDeclarationPublic();

    [GeneratedRegex(@"^internal\s+(?:(?:sealed|static|partial|readonly|abstract)\s+)*(?:class|record|struct|enum|interface)\s+([A-Za-z_]\w*)")]
    private static partial Regex TypeDeclarationInternal();

    /// <summary>Only a call writes a name: a comment that quotes one names nothing, a longer name is another member,
    /// and the same method of another type is another helper - <c>AvailabilityText.Count</c> is not
    /// <c>BuildText.Count</c> any more than it is <c>AvailabilityText.Counts</c>.</summary>
    [Fact]
    public void OnlyACallNamesAHelper()
    {
        const string Sample = """
            var line = AvailabilityText.Count(window) + BuildText.Count(rows);
            // AvailabilityText.Count(window) is the number in the middle of the hourly line
            /// <summary>HourlyCheckText.Title(reports, now, zone) says the same thing at length.</summary>
            /* DeploymentText.Detail(deployment, now) is the second line of a mark. */
            var money = CostText.Money(amount, currency);
            """;

        var calls = ReferencesOf("Sample.cs", Sample);

        // The helpers written as calls name themselves, whichever type they belong to.
        Assert.True(Named(calls, "AvailabilityText", "Count"));
        Assert.True(Named(calls, "BuildText", "Count"));
        Assert.True(Named(calls, "CostText", "Money"));

        // A comment quotes a helper and names nothing.
        Assert.False(Named(calls, "HourlyCheckText", "Title"), "a helper named only in a comment is named by nothing");
        Assert.False(Named(calls, "DeploymentText", "Detail"), "a helper named only in a block comment is named by nothing");

        // The type belongs to the name as much as the method does, and a longer name is another member still.
        Assert.False(Named(calls, "AvailabilityText", "Counts"), "the counts of the hourly line is another method");
        Assert.False(Named(calls, "CostText", "Count"), "another type's Count is another helper");
        Assert.False(Names("AvailabilityText.Counter(1);", "AvailabilityText", "Count"), "a longer name is not this name");
        Assert.True(Names("AvailabilityText.Counter(1);", "AvailabilityText", "Counter"));

        // A call a long line wrapped is the call it was: the name and the bracket may sit a line apart.
        Assert.True(Names("AvailabilityText .\n    Count(window);", "AvailabilityText", "Count"));

        // The sample was read as calls at all, rather than this test passing on nothing read.
        Assert.NotEmpty(calls);
    }

    /// <summary>An offence prints the file and the line the helper stands on, which is where the next person has to
    /// look; a helper a test calls prints nothing.</summary>
    [Fact]
    public void TheOffenceNamesTheFileAndTheLineOfTheHelper()
    {
        var named = Keys(References());

        Assert.Null(OffenceOf(new Helper("BuildInfo.cs", 171, "BuildText", "Percent"), named));

        var offence = OffenceOf(new Helper("BuildInfo.cs", 999, "BuildText", "TheHelperNobodyTests"), named);

        Assert.NotNull(offence);
        Assert.StartsWith("BuildInfo.cs:999 BuildText.TheHelperNobodyTests", offence, StringComparison.Ordinal);
        Assert.Contains("no test calls it by name", offence, StringComparison.Ordinal);
    }

    // ---------------------------------------------------------------- AvailabilityText.Count

    /// <summary><c>23 of 24</c>: the healthy reports first, the reports there were second, both of them counted and
    /// never formatted - the order a reader checks against the window they were counted over.</summary>
    [Theory]
    [InlineData(24, 23, "23 of 24")]
    [InlineData(168, 168, "168 of 168")]
    [InlineData(1, 0, "0 of 1")]
    [InlineData(1, 1, "1 of 1")]
    [InlineData(0, 0, "0 of 0")]
    [InlineData(1000, 999, "999 of 1000")]
    public void AWindowCountsTheHealthyReportsOfThoseThatAsked(int reports, int healthy, string expected) =>
        Assert.Equal(expected, AvailabilityText.Count(new HealthWindow(reports, healthy)));

    /// <summary>A window is written as the invariant culture writes it, without the thousands separator a viewer's
    /// own formatting would add and a reader would read as a version.</summary>
    [Fact]
    public void ACountIsCountedAndNotFormatted() =>
        ViewerCulture.German(() => Assert.Equal("1234 of 10000", AvailabilityText.Count(new HealthWindow(10_000, 1_234))));

    /// <summary>The count of a window is the number in the middle of the hourly line of a container app's box - and
    /// the window a report does not hold is said as <c>none</c> rather than as <c>0 of 0</c>, which is what the box
    /// of an app the runbook stopped asking reads.</summary>
    [Fact]
    public void TheCountOfAWindowIsTheNumberInTheHourlyLine()
    {
        var reports = new ContainerAppReports("ui", null, new HealthWindow(24, 23), new HealthWindow(0, 0));

        Assert.Equal("24 h: 23 of 24 · 7 d: none", HourlyCheckText.Counts(reports));
        Assert.Equal($"24 h: {AvailabilityText.Count(new HealthWindow(24, 23))} · 7 d: none", HourlyCheckText.Counts(reports));
        Assert.Throws<ArgumentNullException>(() => AvailabilityText.Count(null!));
    }

    // ---------------------------------------------------------------- BuildText.Percent and BuildText.Number

    /// <summary><c>81.2 %</c>: one decimal at most, a zero kept as <c>0 %</c> and a whole number written without a
    /// point, with the space of the unit that separates it from the words around it.</summary>
    [Theory]
    [InlineData(81.2, "81.2 %")]
    [InlineData(70.1, "70.1 %")]
    [InlineData(0d, "0 %")]
    [InlineData(100d, "100 %")]
    [InlineData(95.8, "95.8 %")]
    [InlineData(12d, "12 %")]
    [InlineData(12.34, "12.3 %")]
    [InlineData(12.36, "12.4 %")]
    [InlineData(-3.5, "-3.5 %")]
    public void APercentKeepsAtMostOneDecimal(double percent, string expected) =>
        Assert.Equal(expected, BuildText.Percent(percent));

    /// <summary><c>1.9</c>: the same one decimal without a unit, for the numbers of the "Code" card that are counts
    /// of something - an average complexity, a worst CRAP score - and not shares of a whole.</summary>
    [Theory]
    [InlineData(1.9, "1.9")]
    [InlineData(28d, "28")]
    [InlineData(0d, "0")]
    [InlineData(1234.56, "1234.6")]
    [InlineData(0.04, "0")]
    [InlineData(-0.5, "-0.5")]
    public void APlainNumberKeepsAtMostOneDecimal(double number, string expected) =>
        Assert.Equal(expected, BuildText.Number(number));

    /// <summary>Neither of them follows the viewer: a reader of German sees <c>81.2 %</c> and <c>1.9</c> with the
    /// point the build facts are published with, not <c>81,2 %</c> and <c>1,9</c>.</summary>
    [Fact]
    public void TheTwoAreWrittenForTheDashboardAndNotForTheViewer() =>
        ViewerCulture.German(() =>
        {
            Assert.Equal("81.2 %", BuildText.Percent(81.2));
            Assert.Equal("1.9", BuildText.Number(1.9));
        });

    /// <summary>What the "Code" card says is built out of these two and nothing else: its coverage line is two
    /// percentages, its complexity and CRAP words plain numbers - so a change to either of them is a change to the
    /// card, and is seen here as well as there.</summary>
    [Fact]
    public void TheCodeCardSaysTheseTwoInItsOwnWords()
    {
        Assert.Equal(
            $"{BuildText.Percent(81.2)} of lines, {BuildText.Percent(70.1)} of branches",
            BuildText.Coverage(new CoverageFacts(81.2, 70.1)));

        Assert.Equal(
            $"average {BuildText.Number(1.9)}, worst 34 (5,210 methods)",
            BuildText.Complexity(new ComplexityFacts(1.9, 34, 5_210)));

        Assert.Equal(
            $"worst {BuildText.Number(28.5)}, none over {BuildText.Number(30)}",
            BuildText.Crap(new CrapFacts(28.5, 30, 0)));
    }

    // ---------------------------------------------------------------- ContainerAppText.Label

    /// <summary>The word of each state a container app's probe can be in - including the two that are not a verdict:
    /// <c>Probing</c> before the first answer and <c>Waking</c> while an app that scaled to zero starts.</summary>
    [Theory]
    [InlineData(ContainerAppState.Probing, "Probing")]
    [InlineData(ContainerAppState.Waking, "Waking")]
    [InlineData(ContainerAppState.Responding, "Responding")]
    [InlineData(ContainerAppState.Healthy, "Healthy")]
    [InlineData(ContainerAppState.Unhealthy, "Unhealthy")]
    [InlineData(ContainerAppState.NotResponding, "Not responding")]
    public void EveryStateOfAProbedContainerAppHasItsWord(ContainerAppState state, string expected) =>
        Assert.Equal(expected, ContainerAppText.Label(state));

    /// <summary>Six states, six words, none of them the empty string and none of them another's: a badge that said
    /// the same as its neighbour would say nothing, and the sixth is the <c>_</c> of the switch, which a state added
    /// to the enum takes until someone gives it a word of its own.</summary>
    [Fact]
    public void TheSixStatesSaySixDifferentThings()
    {
        var states = Enum.GetValues<ContainerAppState>();

        Assert.Equal(6, states.Length);
        Assert.Equal(6, states.Select(state => ContainerAppText.Label(state)).Distinct(StringComparer.Ordinal).Count());
        Assert.All(states, state => Assert.False(string.IsNullOrWhiteSpace(ContainerAppText.Label(state))));
        Assert.Equal("Probing", ContainerAppText.Label((ContainerAppState)(-1)));
    }

    // ---------------------------------------------------------------- ContainerAppText.Title

    private static readonly ContainerAppTarget App = new(new Uri("https://ca-demo-tdd-ui.graybush-1.southcentralus.azurecontainerapps.io"), "/_healthcheck");

    private static ContainerAppAnswer Answer(int status, DateTimeOffset at) =>
        new("/_healthcheck", true, status, false, 85, at, HealthyPayload: status == 200);

    private static ContainerAppAnswer Silent(DateTimeOffset at) => new("/_healthcheck", true, null, false, null, at, "No answer within 10 s.");

    private static ContainerAppAnswer Unreadable(DateTimeOffset at) => new("/_healthcheck", true, null, true, 85, at);

    /// <summary>A healthy answer says its own name, its host, its state, that the page asks once a minute only while
    /// it shows the diagram, and the status of the last probe at the hour it happened: the whole tooltip in four
    /// sentences, in that order.</summary>
    [Fact]
    public void TheTooltipOfAHealthyAppSaysWhatThePageAsksAndWhatItHeard()
    {
        var title = ContainerAppText.Title(
            "ui",
            App,
            new ContainerAppStatus(ContainerAppState.Healthy, Answer(200, new DateTimeOffset(2026, 10, 6, 23, 20, 5, TimeSpan.Zero))),
            Utc);

        Assert.Equal(
            "ui (ca-demo-tdd-ui.graybush-1.southcentralus.azurecontainerapps.io): Healthy. "
            + "This page asks it once a minute while it shows this diagram, and not while it is paused or hidden; "
            + "nothing asks it from here while no page is open, so it can scale to zero. "
            + "Last probe 23:20:05: /_healthcheck answered HTTP 200.",
            title);

        // The badge's own word is the one of the label, and nothing about scaling to zero is promised for an app that answered.
        Assert.Contains(ContainerAppText.Label(ContainerAppState.Healthy), title, StringComparison.Ordinal);
        Assert.DoesNotContain("scaled to zero starts", title, StringComparison.Ordinal);
    }

    /// <summary>The clock of the last probe is the viewer's, not the server's: the same answer is
    /// <c>23:20:05</c> in UTC and <c>18:20:05</c> five hours west, and the state, the path and the status are said
    /// either way.</summary>
    [Fact]
    public void TheProbeOfTheTooltipIsStatedInTheViewersOwnHours()
    {
        var status = new ContainerAppStatus(ContainerAppState.Healthy, Answer(200, new DateTimeOffset(2026, 10, 6, 23, 20, 5, TimeSpan.Zero)));

        Assert.Contains("Last probe 23:20:05:", ContainerAppText.Title("ui", App, status, Utc), StringComparison.Ordinal);
        Assert.Contains("Last probe 18:20:05:", ContainerAppText.Title("ui", App, status, MinusFive), StringComparison.Ordinal);
    }

    /// <summary>A 404 of the health path is said for what it is - the app is down or gone, which is not the same as
    /// "it answers" - and an app the browser could reach but not read (CORS) says so, because a page that shows
    /// <c>Responding</c> owes the viewer the reason it cannot say more.</summary>
    [Fact]
    public void TheTooltipSaysWhatThePageCannotTellFromWhatItHeard()
    {
        var gone = ContainerAppText.Title(
            "ui",
            App,
            new ContainerAppStatus(ContainerAppState.NotResponding, Answer(404, new DateTimeOffset(2026, 10, 6, 23, 20, 5, TimeSpan.Zero))),
            Utc);

        Assert.Contains("ui (ca-demo-tdd-ui.graybush-1.southcentralus.azurecontainerapps.io): Not responding.", gone, StringComparison.Ordinal);
        Assert.Contains("Last probe 23:20:05: /_healthcheck answered HTTP 404.", gone, StringComparison.Ordinal);
        Assert.Contains("HTTP 404 at the health path", gone, StringComparison.Ordinal);
        Assert.Contains("means the app is down, not that it is responding", gone, StringComparison.Ordinal);

        var refused = ContainerAppText.Title(
            "ui",
            App,
            new ContainerAppStatus(ContainerAppState.Responding, Unreadable(new DateTimeOffset(2026, 10, 6, 23, 20, 5, TimeSpan.Zero)), CorsRefused: true),
            Utc);

        Assert.Contains("Last probe 23:20:05: /_healthcheck answered. The app does not allow this page's origin (CORS)", refused, StringComparison.Ordinal);

        // What the page could not read is not invented: no status stands in the tooltip of an answer it never saw.
        Assert.DoesNotContain("HTTP", refused, StringComparison.Ordinal);
    }

    /// <summary>An app that has not answered yet is not said to be broken: the tooltip gives the path it asked, the
    /// app's own reason for staying out of contact, and the ninety seconds the page waits before it says
    /// <c>Not responding</c> - which is what a scaled-to-zero app costs a viewer.</summary>
    [Fact]
    public void TheTooltipOfAnAppThatHasNotAnsweredGivesItsGraceInFull()
    {
        var at = new DateTimeOffset(2026, 10, 6, 23, 20, 5, TimeSpan.Zero);

        var waking = ContainerAppText.Title("ui", App, new ContainerAppStatus(ContainerAppState.Waking, Silent(at)), Utc);

        Assert.Contains("Last probe 23:20:05: /_healthcheck, no answer. No answer within 10 s.", waking, StringComparison.Ordinal);
        Assert.Contains("the page waits up to 90 seconds for its first answer before it says Not responding", waking, StringComparison.Ordinal);

        // Before the first probe there is nothing to report but the wait itself.
        var probing = ContainerAppText.Title("ui", App, ContainerAppStatus.NotYet, Utc);

        Assert.StartsWith("ui (ca-demo-tdd-ui.graybush-1.southcentralus.azurecontainerapps.io): Probing.", probing, StringComparison.Ordinal);
        Assert.DoesNotContain("Last probe", probing, StringComparison.Ordinal);
        Assert.Contains("waits up to 90 seconds", probing, StringComparison.Ordinal);
        Assert.Contains(ContainerAppText.Label(ContainerAppState.Probing), probing, StringComparison.Ordinal);
    }

    /// <summary>A probe of a path that is no health check is refused as an answer about health: the page says so,
    /// because a bare HTTP 200 of a root page is not a system that works.</summary>
    [Fact]
    public void AnAnswerThePageCannotClassifySaysThatItCannot()
    {
        var root = ContainerAppText.Title(
            "ui",
            new ContainerAppTarget(new Uri("https://ca-demo-tdd-ui.example.io"), string.Empty),
            new ContainerAppStatus(ContainerAppState.Responding, new ContainerAppAnswer("/", false, 200, false, 85, Now)),
            Utc);

        Assert.Contains("Last probe 00:00:00: / answered HTTP 200.", root, StringComparison.Ordinal);
        Assert.Contains("The page could not classify the answer as a health check.", root, StringComparison.Ordinal);
    }

    /// <summary>The tooltip is refused for a probe, a target or a zone the caller has not got.</summary>
    [Fact]
    public void TheTooltipRefusesAStatusATargetOrAZoneThatIsNotThere()
    {
        var status = new ContainerAppStatus(ContainerAppState.Healthy, Answer(200, Now));

        Assert.Throws<ArgumentNullException>(() => ContainerAppText.Title("ui", App, null!, Utc));
        Assert.Throws<ArgumentNullException>(() => ContainerAppText.Title("ui", null!, status, Utc));
        Assert.Throws<ArgumentNullException>(() => ContainerAppText.Title("ui", App, status, null!));
    }

    // ---------------------------------------------------------------- HourlyCheckText.Title

    private static readonly ContainerAppReports Reported = new(
        "ui",
        new HourlyCheck(HourlyCheck.Healthy, new DateTimeOffset(2026, 10, 6, 23, 20, 0, TimeSpan.Zero)),
        new HealthWindow(24, 23),
        new HealthWindow(168, 160));

    /// <summary>The tooltip of the hourly line says what the runbook is, what it last found and when, what it counted
    /// of the reports that asked - and, at the end, that these are hourly checks and not monitoring: a sentence the
    /// box cannot leave out, because a viewer who reads <c>23 of 24</c> as a service level is wrong.</summary>
    [Fact]
    public void TheTooltipOfTheHourlyReportSaysWhoAskedAndWhatItFound()
    {
        Assert.Equal(
            "Once an hour the pipeline's runbook \"Health report\" in Octopus Deploy asks the app whether it answers, "
            + "also while no page is open: the history, under the page's own probe. "
            + "The last report that asked it ended 2026-10-06 23:20 +00:00 (40 min ago) and found it healthy. "
            + "Healthy in 23 of the 24 reports of 24 hours that asked it. "
            + "Healthy in 160 of the 168 reports of 7 days that asked it. "
            + "Hourly checks by the pipeline, not continuous monitoring.",
            HourlyCheckText.Title(Reported, Now, Utc));

        // The two numbers of the tooltip are the two windows the hourly line counts, in the same order.
        Assert.Equal("24 h: 23 of 24 · 7 d: 160 of 168", HourlyCheckText.Counts(Reported));
        Assert.Contains("23 of the 24 reports of 24 hours", HourlyCheckText.Title(Reported, Now, Utc), StringComparison.Ordinal);
        Assert.Contains("2026-10-06 18:20 -05:00", HourlyCheckText.Title(Reported, Now, MinusFive), StringComparison.Ordinal);
    }

    /// <summary>A result older than the two hours the runbook needs to catch up is not shown as news: the tooltip
    /// says the box has stopped talking about it, in the very words the box writes.</summary>
    [Fact]
    public void AResultOlderThanTwoHoursIsSaidToBeNoNews()
    {
        var stale = Reported with { Last = new HourlyCheck(HourlyCheck.Unhealthy, Now - TimeSpan.FromHours(3)) };

        Assert.Contains(
            "The last report that asked it ended 2026-10-06 21:00 +00:00 (3 h ago) and found it unhealthy: "
            + "none in the last two hours, so the box says no recent hourly check.",
            HourlyCheckText.Title(stale, Now, Utc),
            StringComparison.Ordinal);

        // Two hours is still said: the runbook and the publisher both take up to an hour of it.
        var edge = Reported with { Last = new HourlyCheck(HourlyCheck.Healthy, Now - HourlyCheckText.Recent) };

        Assert.Contains("and found it healthy.", HourlyCheckText.Title(edge, Now, Utc), StringComparison.Ordinal);
        Assert.DoesNotContain("none in the last two hours", HourlyCheckText.Title(edge, Now, Utc), StringComparison.Ordinal);
    }

    /// <summary>An app the reports never asked, and no reports at all, are said apart: the first has a history that
    /// does not mention it, the second has no history yet. A window the file does not hold writes no sentence rather
    /// than a sentence of nothing.</summary>
    [Fact]
    public void ReportsThatSayNothingSaySoAndNotZeroOfNothing()
    {
        var none = HourlyCheckText.Title(null, Now, Utc);

        Assert.Contains("The delivery facts name no report of it yet.", none, StringComparison.Ordinal);
        Assert.DoesNotContain("Healthy in", none, StringComparison.Ordinal);

        var outside = HourlyCheckText.Title(new ContainerAppReports("ui", null, null, null), Now, Utc);

        Assert.Contains("No report of the last 7 days asked it.", outside, StringComparison.Ordinal);
        Assert.DoesNotContain("Healthy in", outside, StringComparison.Ordinal);
        Assert.EndsWith(AvailabilityText.Note, outside, StringComparison.Ordinal);

        // Only the week counted: a part the file does not have is left out of the tooltip, as it is out of the line.
        var week = HourlyCheckText.Title(new ContainerAppReports("ui", null, null, new HealthWindow(168, 160)), Now, Utc);

        Assert.DoesNotContain("24 hours", week, StringComparison.Ordinal);
        Assert.Contains("Healthy in 160 of the 168 reports of 7 days that asked it.", week, StringComparison.Ordinal);
    }

    /// <summary>The tooltip is refused for a zone the caller has not got.</summary>
    [Fact]
    public void TheHourlyTooltipRefusesAZoneThatIsNotThere() =>
        Assert.Throws<ArgumentNullException>(() => HourlyCheckText.Title(Reported, Now, null!));

    // ---------------------------------------------------------------- NodeObservation.Of

    private static TargetStatus Node(string name, string? role = "primary") =>
        new(TargetKind.Node, name, new Uri($"https://{name}.example.net"), "westeurope", role);

    private static readonly HealthDetail Checks = HealthDetail.Parse(
        """{ "entries": [ { "name": "DataAccess", "status": "Healthy" }, { "name": "LlmGateway", "status": "Degraded", "description": "Slower still: 9 s" } ] }""",
        Now)!;

    private static readonly TelemetrySnapshot Telemetry = TelemetrySnapshot.Parse(
        """{ "requests": { "perMinute": 12, "frontDoor": 10, "direct": 2 }, "sql": { "perMinute": 30 } }""",
        Now)!;

    /// <summary>An observation is what the page knows of one endpoint at one check: its state, the version it last
    /// reported, its telemetry, the result it came from and the entries of its detailed check - each of them taken
    /// from the tile as it stands, so that the next check can be compared with this one and an event named.</summary>
    [Fact]
    public void AnObservationIsEverythingThePageKnowsOfATargetAtThisCheck()
    {
        var target = Node("app_ui_primary");
        target.Record(new ProbeResult(HealthState.Healthy, 200, 85, Now, "2.4.14"));
        target.RecordTelemetry(Telemetry);
        target.RecordHealthDetail(Checks);

        var seen = NodeObservation.Of(target);

        Assert.Equal(new NodeObservation(HealthState.Healthy, "2.4.14", Telemetry, target.Last, Checks), seen);
        Assert.Equal(HealthState.Healthy, seen.State);
        Assert.Equal("2.4.14", seen.Version);
        Assert.Same(Telemetry, seen.Telemetry);
        Assert.Same(Checks, seen.Checks);
        Assert.Equal(85, seen.Last!.LatencyMs);
        Assert.Equal(200, seen.Last.StatusCode);
    }

    /// <summary>Before the first check there is nothing to compare: a tile that has never answered is observed as
    /// <c>Pending</c> with no version, no telemetry, no result and no entries - which is why the first check of the
    /// page writes an event only when something is already wrong.</summary>
    [Fact]
    public void ATargetNobodyHasCheckedYetIsObservedAsPendingWithNothingBehindIt()
    {
        var seen = NodeObservation.Of(new TargetStatus(TargetKind.FrontDoor, "Front Door", new Uri("https://ui.example.net")));

        Assert.Equal(HealthState.Pending, seen.State);
        Assert.Null(seen.Version);
        Assert.Null(seen.Telemetry);
        Assert.Null(seen.Last);
        Assert.Null(seen.Checks);
    }

    /// <summary>An observation is of one check and does not move with the tile: the version a node stops reporting
    /// stays the version it is observed with, while the state of an endpoint that goes on answering follows the
    /// answer - a before and an after that both tracked the live tile would compare a moment with itself and name no
    /// event at all.</summary>
    [Fact]
    public void AnObservationKeepsTheCheckItWasMadeOfAndNotTheNextOne()
    {
        var target = Node("app_ui_standby", role: "standby");
        target.Record(new ProbeResult(HealthState.Healthy, 200, 85, Now, "2.4.14"));
        target.RecordHealthDetail(Checks);

        var before = NodeObservation.Of(target);

        target.Record(new ProbeResult(HealthState.Unhealthy, 503, 40, Now + TimeSpan.FromMinutes(1), null, "HTTP 503"));
        target.RecordHealthDetail(null);

        var after = NodeObservation.Of(target);

        Assert.Equal(HealthState.Healthy, before.State);
        Assert.Equal(HealthState.Unhealthy, after.State);
        Assert.Equal("2.4.14", after.Version);
        Assert.Null(after.Last!.Version);

        // A detailed check that answers without entries does not replace the last one that had them, and this
        // observation keeps the entries of its own check - null - rather than the tile's memory of the last good read.
        Assert.Equal(Checks, before.Checks);
        Assert.Null(after.Checks);
        Assert.Same(Checks, target.LastReadHealthDetail);
    }

    /// <summary>An endpoint the page has not got is refused rather than observed as nothing.</summary>
    [Fact]
    public void ANodeThatIsNotThereIsNotObserved() => Assert.Throws<ArgumentNullException>(() => NodeObservation.Of(null!));

    // ---------------------------------------------------------------- DeploymentText.Detail

    private static readonly Uri OctopusTask = new("https://octopus.example.net/tasks/T123");

    private static Deployment Waiting(DeploymentWait? wait, string? startedBy = null) =>
        new("demo-ui", "prod", "2.4.43", DeploymentState.Waiting, "waiting", Now - TimeSpan.FromHours(2), null, OctopusTask, startedBy, wait);

    private static readonly DeploymentWait SignOff = new(DeploymentWait.SignOff, "Sign-off", Now - TimeSpan.FromHours(2), "demo approvers");

    /// <summary>What a waiting deployment waits for, for how long and who is asked, in the file's own order and
    /// joined with a semicolon: the sign-off names the teams or the person who took it, the asking gives its age in
    /// the page's own units, and the account that started the task comes last.</summary>
    [Fact]
    public void AWaitingDeploymentSaysWhoseDeskItIsOnAndSinceWhen() =>
        Assert.Equal(
            "the sign-off is with demo approvers; asked 2 h ago; started by jeffrey",
            DeploymentText.Detail(Waiting(SignOff, "jeffrey"), Now));

    /// <summary>Octopus Deploy's own title of a question is quoted when it says more than the kind does, and left out
    /// when it does not: <c>Sign-off</c> of a <c>sign-off</c> is the kind in capitals and is not repeated, while a
    /// title that names the release is the only thing a viewer learns about what is being asked. Nobody named and no
    /// time given is nothing to say, and the detail is left out rather than written as an empty sentence.</summary>
    [Theory]
    [InlineData("Sign-off", "pat", true, "the sign-off is with pat; asked 2 h ago")]
    [InlineData("Approve the release to production", "pat", true, "the sign-off \"Approve the release to production\" is with pat; asked 2 h ago")]
    [InlineData("Sign-off", null, true, "asked 2 h ago")]
    [InlineData("Approve the release to production", null, true, "the sign-off \"Approve the release to production\"; asked 2 h ago")]
    [InlineData("Sign-off", "pat", false, "the sign-off is with pat")]
    [InlineData("Approve the release to production", null, false, "the sign-off \"Approve the release to production\"")]
    [InlineData("Sign-off", null, false, null)]
    public void TheTitleOfAQuestionIsQuotedOnlyWhereItSaysMore(string title, string? responsible, bool asked, string? expected)
    {
        var wait = new DeploymentWait(DeploymentWait.SignOff, title, asked ? Now - TimeSpan.FromHours(2) : null, responsible);

        Assert.Equal(expected, DeploymentText.Detail(Waiting(wait), Now));
    }

    /// <summary>The question a failed step asks is named as what it is, a kind the page does not know is said in the
    /// file's own word, and neither is invented a sign-off: what the file says is what the tooltip says.</summary>
    [Fact]
    public void AQuestionThePageDoesNotHaveAWordForIsSaidInTheFilesOwnWords()
    {
        Assert.Equal(
            "the question after a failed step \"Database is unreachable\" is with jeffrey; asked 3 min ago",
            DeploymentText.Detail(
                Waiting(new DeploymentWait(DeploymentWait.GuidedFailure, "Database is unreachable", Now - TimeSpan.FromMinutes(3), "jeffrey")),
                Now));

        Assert.Equal(
            "manual intervention is with ops",
            DeploymentText.Detail(Waiting(new DeploymentWait("manual intervention", null, null, "ops")), Now));
    }

    /// <summary>Only a deployment that is waiting says what it waits for: an executing task with a sign-off left
    /// standing in the file - which Octopus Deploy does, for the question it answered a moment ago - is not stopped
    /// for anyone, and a deployment that has ended says who started it and nothing else.</summary>
    [Fact]
    public void WhatIsNotWaitedForIsNotSaidToBeWaiting()
    {
        var executing = new Deployment("demo-ui", "uat", "2.4.44", DeploymentState.Executing, "executing", Now, null, OctopusTask, "ai-ops", SignOff);

        Assert.Equal("started by ai-ops", DeploymentText.Detail(executing, Now));

        var ended = new Deployment("demo-ui", "tdd", "2.4.45", DeploymentState.Succeeded, "succeeded", Now - TimeSpan.FromMinutes(9), Now, OctopusTask, null);

        Assert.Null(DeploymentText.Detail(ended, Now));
        Assert.Null(DeploymentText.Detail(ended with { StartedBy = null, WaitsFor = null }, Now));
        Assert.Equal("started by cm-ai-ops", DeploymentText.Detail(ended with { StartedBy = "cm-ai-ops" }, Now));

        // A sign-off with nobody named and nothing asked yet says nothing at all.
        Assert.Null(DeploymentText.Detail(Waiting(new DeploymentWait(DeploymentWait.SignOff, null, null, null)), Now));
        Assert.Throws<ArgumentNullException>(() => DeploymentText.Detail(null!, Now));
    }

    /// <summary>How long a deployment has been waiting is the page's own measure of age: whole minutes under an hour
    /// and never less than one, whole hours under two days, whole days after that - truncated, so a sign-off asked
    /// fifty-nine minutes ago is not yet an hour old.</summary>
    [Theory]
    [InlineData(1, "asked 1 min ago")]
    [InlineData(20, "asked 20 min ago")]
    [InlineData(59, "asked 59 min ago")]
    [InlineData(60, "asked 1 h ago")]
    [InlineData(119, "asked 1 h ago")]
    [InlineData(2879, "asked 47 h ago")]
    [InlineData(2880, "asked 2 d ago")]
    public void TheAgeOfAQuestionIsThePagesOwnAge(int minutes, string expected)
    {
        var wait = new DeploymentWait(DeploymentWait.SignOff, null, Now - TimeSpan.FromMinutes(minutes), null);

        Assert.Equal(expected, DeploymentText.Detail(Waiting(wait), Now));
    }

    /// <summary>What the file says beyond the state is the second line of the mark's tooltip: the sentence the page
    /// builds and this detail, on lines of their own, so a title with both reads as two statements and not as one
    /// long one.</summary>
    [Fact]
    public void TheDetailIsTheSecondLineOfTheMarksTooltip()
    {
        var deployment = Waiting(SignOff, "jeffrey");
        var detail = DeploymentText.Detail(deployment, Now);
        var mark = new DeploymentMark(
            deployment.Project,
            "ui",
            deployment.Environment,
            deployment.Release,
            deployment.State,
            DeploymentText.Sentence(deployment, Now),
            (int)(Now - deployment.Since!.Value).TotalMinutes,
            deployment.Url,
            InFlight: true,
            OfSystem: false,
            detail);

        Assert.Equal("the sign-off is with demo approvers; asked 2 h ago; started by jeffrey", detail);
        Assert.Equal($"demo-ui 2.4.43 waits for a sign-off in prod (2 h so far)\n{detail}", DeploymentText.Title(mark));
    }

    // ---------------------------------------------------------------- HistoryText.ResultFacts

    /// <summary>The shared helper that formats the HTTP status and latency parts of a check result: the two callers
    /// (RuntimePayloadBuilder and HistoryText.Describe) pass their own separator, and the helper joins the parts
    /// accordingly.</summary>
    [Fact]
    public void ResultFactsJoinsTheStatusAndLatencyPartsOfAProbe()
    {
        var result = new ProbeResult(HealthState.Healthy, 200, 85, Now);

        Assert.Equal("HTTP 200 · 85 ms", HistoryText.ResultFacts(result, " · "));
        Assert.Equal("HTTP 200, 85 ms", HistoryText.ResultFacts(result, ", "));

        // One part only: the helper writes what is there and leaves out the rest.
        var statusOnly = new ProbeResult(HealthState.Unhealthy, 503, null, Now);
        Assert.Equal("HTTP 503", HistoryText.ResultFacts(statusOnly, " · "));

        var latencyOnly = new ProbeResult(HealthState.Unreachable, null, 12, Now);
        Assert.Equal("12 ms", HistoryText.ResultFacts(latencyOnly, " · "));

        // Neither part: the helper writes nothing, and the caller decides what to say in that case.
        var neither = new ProbeResult(HealthState.Unreachable, null, null, Now);
        Assert.Equal(string.Empty, HistoryText.ResultFacts(neither, " · "));
    }

    // ---------------------------------------------------------------- the machinery

    /// <summary>The eight helpers the issue found untested, as their type and their name.</summary>
    private static (string Type, string Method)[] HelpersOfTheIssue { get; } =
    [
        ("AvailabilityText", "Count"),
        ("BuildText", "Percent"),
        ("BuildText", "Number"),
        ("ContainerAppText", "Label"),
        ("ContainerAppText", "Title"),
        ("DeploymentText", "Detail"),
        ("HourlyCheckText", "Title"),
        ("NodeObservation", "Of"),
    ];

    /// <summary>One <c>public static</c> method of the app's <c>Health</c> folder: where it stands, the type that
    /// encloses it and its own name - which is all the rule reads, since that is what a call writes.</summary>
    private sealed record Helper(string File, int Line, string Type, string Method)
    {
        public string Key => $"{Type}.{Method}";

        public string Report => $"{File}:{Line} {Key}";
    }

    /// <summary>One <c>Type.Method(</c> of the test project's source, with the file and the line it is written on.</summary>
    private sealed record Reference(string Type, string Method, string File, int Line)
    {
        public string Key => $"{Type}.{Method}";
    }

    /// <summary>Why the helpers are refused, one line each: the file and the line of the declaration and the name no
    /// test writes.</summary>
    private static List<string> Offences()
    {
        var named = Keys(References());

        return [.. Declared()
            .Select(helper => OffenceOf(helper, named))
            .OfType<string>()
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];
    }

    /// <summary>The names the test project writes, once each.</summary>
    private static HashSet<string> Keys(IEnumerable<Reference> calls) =>
        [.. calls.Select(call => call.Key).Distinct(StringComparer.Ordinal)];

    /// <summary>Why this helper is refused, or <c>null</c> when a test calls it by name.</summary>
    private static string? OffenceOf(Helper helper, HashSet<string> named) =>
        named.Contains(helper.Key)
            ? null
            : $"{helper.Report}: no test calls it by name - no file of src/Dashboard.Tests writes {helper.Key}(...) outside a comment, so nothing says what the helper answers";

    /// <summary>True when this piece of source calls this helper by name.</summary>
    private static bool Names(string code, string type, string method) => Named(ReferencesOf("Sample.cs", code), type, method);

    /// <summary>True when the calls read name this helper: its own type, its own name, and a <c>(</c> after them.</summary>
    private static bool Named(IReadOnlyCollection<Reference> calls, string type, string method) =>
        calls.Any(call => call.Type == type && call.Method == method);

    /// <summary>Every <c>public static</c> method declared in the app's <c>Health</c> folder, in the order of the
    /// files and the lines, each attributed to the type declared above it.</summary>
    private static List<Helper> Declared()
    {
        var helpers = new List<Helper>();

        foreach (var (file, text) in SourceFiles(HealthRoot()))
        {
            var line = 0;
            var type = string.Empty;

            foreach (var raw in text.Split('\n'))
            {
                line++;

                var code = raw.Trim();

                if (IsComment(code))
                {
                    continue;
                }

                // A type declaration opens the scope of everything under it, and is no helper of its own.
                if (TypeDeclaration().Match(code) is { Success: true } declared)
                {
                    type = declared.Groups[1].Value;
                    continue;
                }

                if (MethodDeclaration().Match(code) is { Success: true } method)
                {
                    // A method above every type declaration of its file keeps no type, is named by nothing that can
                    // be written, and is reported - which is the point: the guard does not walk past what it cannot read.
                    helpers.Add(new Helper(file, line, type, method.Groups[1].Value));
                }
            }
        }

        return helpers;
    }

    /// <summary>Every <c>Type.Method(</c> of the test project's own source, outside the comments.</summary>
    private static List<Reference> References() =>
        [.. SourceFiles(TestRoot()).SelectMany(source => ReferencesOf(source.File, source.Text))];

    /// <summary>The <c>Type.Method(</c> a piece of source writes: its calls, each reported on the line it opens on.
    /// Every comment line is blanked out first, so a doc comment that quotes a helper names nothing while the line
    /// numbers of the calls around it stay the numbers a reader looks for.</summary>
    private static List<Reference> ReferencesOf(string file, string text)
    {
        var code = string.Join('\n', text.Split('\n').Select(line => IsComment(line) ? string.Empty : line));

        return [.. Call().Matches(code).Select(match => new Reference(match.Groups[1].Value, match.Groups[2].Value, file, LineOf(code, match.Index)))];
    }

    private static int LineOf(string text, int at) => text.AsSpan(0, at).Count('\n') + 1;

    private static bool IsComment(string line)
    {
        var code = line.Trim();

        return code.StartsWith("//", StringComparison.Ordinal)
               || code.StartsWith("/*", StringComparison.Ordinal)
               || (code.Length > 0 && code[0] == '*');
    }

    /// <summary>A type that opens its scope: a class, a record, a struct, an enum or an interface.</summary>
    [GeneratedRegex(@"^(?:public|internal)\s+(?:(?:sealed|static|partial|readonly|abstract)\s+)*(?:class|record|struct|enum|interface)\s+([A-Za-z_]\w*)")]
    private static partial Regex TypeDeclaration();

    /// <summary>A <c>public static</c> method: its name, which is what a call writes.</summary>
    [GeneratedRegex(@"^public\s+static\s+[^=;{}()]*?\b([A-Za-z_]\w*)\s*\(")]
    private static partial Regex MethodDeclaration();

    /// <summary>A call of a member of a type: <c>AvailabilityText.Count(</c>, spaces and line breaks between the
    /// name and the dot included, since a long call wraps there.</summary>
    [GeneratedRegex(@"\b([A-Z]\w*)\s*\.\s*([A-Za-z_]\w*)\s*\(")]
    private static partial Regex Call();

    private static string HealthRoot([CallerFilePath] string here = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "Dashboard", "Health"));

    private static string TestRoot([CallerFilePath] string here = "") => Path.GetFullPath(Path.GetDirectoryName(here)!);

    /// <summary>The text of one file of this project, by its name: the tests of the eight helpers are held to this
    /// file, so it is read by the name it is written under.</summary>
    private static string SourceOf(string name) => File.ReadAllText(Path.Combine(TestRoot(), name));

    /// <summary>Every <c>.cs</c> under a folder, as the path the assertion prints relative to it and its text. The
    /// build's copies under <c>bin</c> and <c>obj</c> hold the same source twice, so those directories are walked
    /// past.</summary>
    private static IEnumerable<(string File, string Text)> SourceFiles(string root)
    {
        foreach (var path in Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories).OrderBy(p => p, StringComparer.Ordinal))
        {
            var relative = Path.GetRelativePath(root, path);

            if (relative.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Any(part =>
                    part.Equals("bin", StringComparison.OrdinalIgnoreCase) || part.Equals("obj", StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            yield return (relative, File.ReadAllText(path));
        }
    }
}
