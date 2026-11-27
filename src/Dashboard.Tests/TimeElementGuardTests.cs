using System.Runtime.CompilerServices;

namespace Dashboard.Tests;

/// <summary>
/// A <c>&lt;time&gt;</c> is the one element of this app whose words are a lie by the next refresh. The markup writes
/// "just now", "3 minutes ago", "as of 2 May" - words a viewer reads as a moment and a machine reads as nothing at
/// all, because they are the words of a sentence rather than the number of a clock. The <c>datetime</c> attribute is
/// what says the moment again in the one form everything understands - an ISO 8601 date and time,
/// <c>2026-05-02T08:00Z</c> - so that a reader hears a real date rather than a phrase, so that a browser may render
/// it in the reader's own zone and format, and so that the words on the page are anchored to a fact that outlives
/// them (#159).
/// <para>
/// Without it the element is only a <c>span</c> wearing a name, and HTML says as much: a <c>&lt;time&gt;</c> that
/// writes no <c>datetime</c> owes a valid date string in its own text, which "3 minutes ago" is not. So every
/// <c>&lt;time&gt;</c> start tag of this app writes a <c>datetime</c>, and writes it with something in it. A literal
/// value of nothing - the attribute left bare, a pair of quotes, nothing between them but the spaces, tabs and line
/// breaks a template drifts in - says no moment any more plainly than no attribute at all, and is refused for the
/// same reason.
/// </para>
/// <para>
/// A value Razor builds is a value present: <c>datetime="@TimeText.Iso(deployed)"</c>,
/// <c>datetime="@CostText.Date(day)"</c> and <c>datetime="@($"{day:yyyy-MM-dd}")"</c> are written by the component's
/// own code, and whether that code returns a well-formed moment is its tests' business, not this guard's. Only the
/// <c>@</c> that opens an expression at the start of the value marks one; a literal that carries an <c>@</c> among
/// its words is read as the literal it is.
/// </para>
/// <para>
/// A guard walks every component and refuses a time that names no moment, listing the file and the line the tag
/// opens on, so the next timestamp added cannot be forgotten. The tag is read to the "&gt;" that closes it, past the
/// Razor expressions, the quoted values that carry a "&gt;" of their own and the line breaks they spread over - the
/// way <see cref="EmptyLabelGuardTests" />, <see cref="TabOrderAndGroupGuardTests" /> and
/// <see cref="MeterGuardTests" /> read them - and its attributes are read as name/value pairs, so the whole name is
/// what counts: <c>data-datetime</c> and <c>datetimefoo</c> are other attributes, while <c>DATETIME</c> is this one,
/// HTML leaving an attribute name case-free. The element is matched whole too, so a <c>&lt;timeline&gt;</c>, a
/// <c>&lt;timespan&gt;</c> or a component named <c>&lt;TimeBox&gt;</c> is never read as a <c>&lt;time&gt;</c>, and
/// the <c>&lt;/time&gt;</c> that closes one writes no attribute at all.
/// </para>
/// </summary>
public class TimeElementGuardTests
{
    /// <summary>The element this rule is about, as the tag spells it in lower case.</summary>
    private const string TimeTag = "time";

    /// <summary>The attribute that says the moment the element's words only point at.</summary>
    private const string DateTimeAttribute = "datetime";

    /// <summary>The one character that says a value is built at Razor runtime and is not this guard's to read.</summary>
    private const char Razor = '@';

    /// <summary>
    /// One <c>&lt;time&gt;</c> of the markup: the file to print, the line its tag opens on, its element name, and
    /// every <c>datetime</c> the tag writes - once per occurrence, so a tag that writes the attribute twice is read
    /// twice and the empty one left beside a real one cannot hide.
    /// </summary>
    private sealed record TimeElement(string File, int Line, string Element, List<string?> Datetimes)
    {
        /// <summary>True of the value this guard passes over: one Razor builds, which the markup cannot say more about.</summary>
        public static bool BuiltByRazor(string? value) =>
            value is not null && value.TrimStart().StartsWith(Razor);

        /// <summary>True when the tag writes no <c>datetime</c> at all.</summary>
        public bool WritesNone => Datetimes.Count == 0;

        /// <summary>The <c>datetime</c> attributes of the tag that say nothing: left bare, a pair of quotes, or
        /// nothing between them but whitespace. A value Razor builds is a value present.</summary>
        public List<string?> Silent => [.. Datetimes.Where(value => !BuiltByRazor(value) && string.IsNullOrWhiteSpace(value))];

        /// <summary>Why the guard refuses this element, one sentence per fault - empty when it names a moment. Both
        /// faults are reported at once, so a fixer is not sent back for the second one by the next run.</summary>
        public List<string> Faults
        {
            get
            {
                var faults = new List<string>();

                if (WritesNone)
                {
                    faults.Add(
                        $"<{Element}> writes no {DateTimeAttribute}, so the moment it holds stays the words on the page: no reader, browser or calendar can tell when");
                }

                foreach (var value in Silent)
                {
                    faults.Add(
                        $"<{Element}> writes {Written(value)}: a {DateTimeAttribute} of nothing says no moment, which is worse than the attribute left out");
                }

                return faults;
            }
        }

        /// <summary>The offence of the element, or <c>null</c> when it names its moment.</summary>
        public string? Offence => Faults.Count == 0 ? null : string.Join("; ", Faults);

        /// <summary>Where the fault stands: the file as the walk prints it, the line the tag opens on, and why.</summary>
        public string Report => $"{File}:{Line} {Offence}";

        /// <summary>The attribute as the tag wrote it, value and all.</summary>
        private static string Written(string? value) =>
            value is null ? $"{DateTimeAttribute} with no value at all" : $"{DateTimeAttribute}=\"{value}\"";
    }

    /// <summary>One attribute of a tag: its name as written and the value after it, if any - <c>null</c> when the
    /// attribute is written bare, which HTML reads as an empty value.</summary>
    private sealed record TagAttribute(string Name, string? Value);

    /// <summary>
    /// Every <c>&lt;time&gt;</c> of every component writes a <c>datetime</c> that names a moment: the attribute
    /// stands on the tag itself, and it holds a literal of words or one Razor builds.
    /// </summary>
    [Fact]
    public void EveryTimeElementWritesADatetime()
    {
        Assert.Empty(ReportsOf(Times()));
    }

    /// <summary>
    /// A guard that found nothing has seen nothing: the app writes nineteen <c>&lt;time&gt;</c> elements over
    /// thirteen components today - the "ago" of <c>DeliveryCard.razor</c> and <c>WorkRows.razor</c>, the clock of
    /// <c>App.razor</c> and <c>EventsStrip.razor</c>, the "as of" of <c>CostLine.razor</c> and
    /// <c>WorkItemsView.razor</c>, the pod and node ages of <c>PodTable.razor</c> and <c>ClusterNodes.razor</c> -
    /// every one of them building its moment in Razor. A scan that reaches far fewer never read the markup, and the
    /// clean verdict above would mean nothing; the floors are set well below those counts, so the day the app gains
    /// a timestamp the test still says what it reads.
    /// </summary>
    [Fact]
    public void TheGuardReadsTheTimeElementsOfTheApp()
    {
        var times = Times();
        var files = times.Select(time => time.File).Distinct(StringComparer.Ordinal).ToList();

        Assert.True(
            times.Count >= 10,
            $"only {times.Count} <{TimeTag}> element(s) were read under {DashboardRoot()}: the guard is checking nothing");

        Assert.True(
            files.Count >= 5,
            $"the {times.Count} <{TimeTag}> element(s) read live in {files.Count} file(s): the walk does not reach the app");

        Assert.Contains(times, time => time.File.Equals(Path.Combine("Components", "DeliveryCard.razor"), StringComparison.Ordinal));
        Assert.Contains(times, time => time.File.Equals(Path.Combine("Components", "EventsStrip.razor"), StringComparison.Ordinal));
        Assert.Contains(times, time => time.File.Equals("App.razor", StringComparison.Ordinal));

        Assert.All(times, time => Assert.True(time.Offence is null, $"{time.Report}: a <{TimeTag}> of the app names no moment"));

        // What the app writes is read as the app writes it: a moment Razor builds off the parsed feed, which this
        // guard passes over rather than guesses at.
        Assert.Contains(times, time => time.Datetimes.Any(TimeElement.BuiltByRazor));
    }

    /// <summary>The rule refuses the <c>&lt;time&gt;</c> that writes no <c>datetime</c>: nothing at all, the element
    /// padded with other attributes, and the same element spelled with the name in capitals, since HTML leaves an
    /// element name case-free.</summary>
    [Fact]
    public void TheRuleRefusesATimeThatWritesNoDatetime()
    {
        Assert.Contains("no datetime", OffenceOf("""<time>just now</time>"""), StringComparison.Ordinal);
        Assert.Contains("no datetime", OffenceOf("""<time class="event__time">@TimeText.Ago(at, Now)</time>"""), StringComparison.Ordinal);
        Assert.Contains("<time>", OffenceOf("""<time class="grid__minor" title="When the item moved">3 minutes ago</time>"""), StringComparison.Ordinal);
        Assert.Contains("no datetime", OffenceOf("""<TIME>2 May</TIME>"""), StringComparison.Ordinal);
        Assert.Contains(":2 ", ReportOf("""
            <div class="card">
                <time>no moment at all</time>
            </div>
            """), StringComparison.Ordinal);
    }

    /// <summary>The rule refuses the <c>datetime</c> that holds nothing: the attribute written bare, a pair of
    /// quotes, and a value of nothing but the whitespace a template drifts between them - the space, the tab, the
    /// line break and the non-breaking space, which a reader hears as silence.</summary>
    [Fact]
    public void TheRuleRefusesADatetimeThatHoldsNothing()
    {
        Assert.Contains($"{DateTimeAttribute}=\"\"", OffenceOf("""<time datetime="">2 May</time>"""), StringComparison.Ordinal);
        Assert.Contains("with no value at all", OffenceOf("<time datetime>2 May</time>"), StringComparison.Ordinal);
        Assert.Contains($"{DateTimeAttribute}=\"   \"", OffenceOf("""<time datetime="   ">2 May</time>"""), StringComparison.Ordinal);
        Assert.Contains($"{DateTimeAttribute}=\"\t\"", OffenceOf("<time datetime=\"\t\">2 May</time>"), StringComparison.Ordinal);
        Assert.Contains($"{DateTimeAttribute}=\"\r\n  \"", OffenceOf("<time datetime=\"\r\n  \">2 May</time>"), StringComparison.Ordinal);
        Assert.Contains($"{DateTimeAttribute}=\"\u00a0\"", OffenceOf("<time datetime=\"\u00a0\">2 May</time>"), StringComparison.Ordinal);
        Assert.Contains("a datetime of nothing", OffenceOf("""<time class="event__time" datetime=" ">@Text</time>"""), StringComparison.Ordinal);

        // A single-quoted value is a value like any other - and, with nothing in it, no value.
        Assert.Contains($"{DateTimeAttribute}=\" \"", OffenceOf("<time datetime=' '>2 May</time>"), StringComparison.Ordinal);
    }

    /// <summary>The rule accepts the moments the app actually writes and the ones it may: an ISO date, a full
    /// date-time with a zone, a bare month and day - which HTML allows as a valid date string - and every value
    /// Razor builds, whether it opens with a bare expression, a call, a parenthesised one or an interpolated string
    /// holding a "&gt;" of its own.</summary>
    [Fact]
    public void TheRuleAcceptsADatetimeThatNamesAMoment()
    {
        var accepted = new[]
        {
            """<time datetime="2026-05-02">2 May</time>""",
            """<time datetime="2026-05-02T08:00Z">2 May, 08:00 UTC</time>""",
            """<time datetime="2026-05">May 2026</time>""",
            """<time class="grid__minor" datetime="2026-05-02T08:00:00+01:00" title="When the item moved">2 May</time>""",
            """<time datetime="@TimeText.Iso(deployed)">@TimeText.Ago(deployed, Now)</time>""",
            """<time datetime="@CostText.Date(day)">@CostText.Date(day)</time>""",
            """<time datetime="@(_when.ToString("o"))">@Text</time>""",
            """<time datetime="@($"{day:yyyy-MM-dd}")">@Text</time>""",
            """<time datetime="@TimeText.Iso(started)" title="@($"Started {TimeText.DateAndTime(started, Zone)}")">@ClusterText.Age(started, AsOf)</time>""",
            """<time datetime="@Iso" />""",
            """<time datetime="@TimeText.Iso(at)" title="@($"3 checks > the last minute")">@Text</time>""",
        };

        foreach (var text in accepted)
        {
            var times = Scan("Sample.razor", text);

            Assert.Single(times);
            Assert.Null(times[0].Offence);

            // The sample was read as the element this rule is about, not skipped past.
            Assert.Equal(TimeTag, times[0].Element);
        }

        // A literal is a literal: the "@" of a word is not the "@" that opens an expression, and a value that begins
        // with a word is read as the words it begins with - here, words that name a moment.
        Assert.False(TimeElement.BuiltByRazor("2026-05-02T08:00Z"));
        Assert.True(TimeElement.BuiltByRazor("@TimeText.Iso(at)"));
        Assert.True(TimeElement.BuiltByRazor(" @(Iso)"));
    }

    /// <summary>Only the <c>&lt;time&gt;</c> element is read, and only by its own name: a <c>&lt;timeline&gt;</c>,
    /// a <c>&lt;timespan&gt;</c> or a component named <c>&lt;TimeBox&gt;</c> opens with a longer name and is another
    /// element, the <c>&lt;/time&gt;</c> that closes one writes no attributes, and the <c>&lt;</c> of a Razor
    /// comparison opens nothing at all.</summary>
    [Fact]
    public void OnlyTheTimeElementIsReadAsATime()
    {
        var text = """
            <main>
                <timeline class="strip">no time element</timeline>
                <TimeSpan Days="1" />
                <TimeBox Title="">a component of this app</TimeBox>
                <p class="age @(late ? "age--late" : null)">written @(count < 10) times</p>
                <div class="stamp">3 minutes ago</div>
            </main>
            """;

        Assert.Empty(Scan("Sample.razor", text));

        // The element read whole, the closing tag never: one time element per start tag.
        var times = Scan("Sample.razor", """<time datetime="@Iso">@Text</time>""");

        Assert.Single(times);
        Assert.Equal(TimeTag, times[0].Element);
        Assert.Empty(times[0].Faults);
    }

    /// <summary>Only the attribute of that very name says the moment: a <c>data-datetime</c>, a
    /// <c>Datetime-Foo</c>, an <c>@bind-datetime</c> or a <c>xlink:datetime</c> is another attribute, and none of
    /// them rescues the <c>&lt;time&gt;</c> that writes no <c>datetime</c> of its own - while the same name spelled
    /// in another case is the attribute, since HTML leaves a name case-free.</summary>
    [Fact]
    public void OnlyTheDatetimeOwnNamesCount()
    {
        var text = """
            <div class="events">
                <time data-datetime="2026-05-02" datetime-foo="2026-05-02" @bind-datetime="2026-05-02">ago</time>
                <time DATETIME="2026-05-02T08:00Z">2 May</time>
            </div>
            """;

        var times = Scan("Sample.razor", text);

        Assert.Equal(2, times.Count);
        Assert.Empty(times[0].Datetimes);
        Assert.True(times[0].WritesNone);
        Assert.Contains("no datetime", times[0].Offence!, StringComparison.Ordinal);
        Assert.Null(times[1].Offence);
        Assert.Equal(3, times[1].Line);
    }

    /// <summary>A tag that offends more than once is reported once per <c>datetime</c>, so a fixer is sent back with
    /// the whole of the tag rather than one attribute at a time - even when one of them names a moment and the tag
    /// writes the same attribute twice.</summary>
    [Fact]
    public void EveryFaultOfATagIsReported()
    {
        var times = Scan("Sample.razor", """<time datetime="" datetime="   " class="stamp">ago</time>""");

        Assert.Single(times);
        Assert.Equal(2, times[0].Silent.Count);
        Assert.Equal(2, times[0].Faults.Count);
        Assert.Contains($"{DateTimeAttribute}=\"\"", times[0].Offence!, StringComparison.Ordinal);
        Assert.Contains($"{DateTimeAttribute}=\"   \"", times[0].Offence!, StringComparison.Ordinal);

        // The whole tag sits on one line, so both faults print the same place - the line the tag opened on.
        Assert.All(ReportsOf(times), report => Assert.Contains(":1 ", report, StringComparison.Ordinal));
    }

    /// <summary>A tag ends at the "&gt;" that closes it, not at the first "&gt;" of the file: a class may hold a
    /// comparison, a title a "&gt;" of its own inside the quotes Razor carries, an event handler an arrow whose
    /// "=&gt;" is read as text, and the attributes may spread over lines of their own. Read short, the
    /// <c>datetime</c> below them would be outside the tag and the guard would cry over a component that says quite
    /// clearly when - and read short the other way, a tag of several lines is reported where it opened.</summary>
    [Fact]
    public void TheTagReadRunsPastRazorAndLineBreaks()
    {
        var text = """
            <main>
                <time class="event__time @(fresh ? "event__time--new" : null)"
                      title="@($"3 checks > the last minute")"
                      datetime="@TimeText.Iso(entry.At)">@TimeText.Clock(entry.At, Zone)</time>
                <time class="stamp"
                      datetime="">@Text</time>
            </main>
            """;

        var times = Scan("Sample.razor", text);

        Assert.Equal(2, times.Count);

        Assert.Equal(2, times[0].Line);
        Assert.Equal(TimeTag, times[0].Element);
        Assert.Single(times[0].Datetimes);
        Assert.Null(times[0].Offence);

        Assert.Equal(5, times[1].Line);
        Assert.Contains($"{DateTimeAttribute}=\"\"", times[1].Offence!, StringComparison.Ordinal);

        // The report prints the line the tag opened on, not the line its datetime sits on.
        Assert.Contains(":2 ", ReportOf("""
            <div>
                <time class="a"
                      datetime="">@Text</time>
            </div>
            """), StringComparison.Ordinal);
    }

    /// <summary>The report names the file the way the walk prints it and the line the tag opens on - the first of
    /// them - and reads every element of a file on its own.</summary>
    [Fact]
    public void TheReportNamesTheFileAndTheLineOfTheTag()
    {
        var text = """
            <div class="events">
                <h2 id="events-title">Events</h2>
                <time class="event__time">@TimeText.Clock(entry.At, Zone)</time>
                <span>between the two</span>
                <TIME datetime=" ">ago</TIME>
            </div>
            """;

        var offenders = ReportsOf(Scan(Path.Combine("Components", "Sample.razor"), text));

        Assert.Equal(2, offenders.Count);
        Assert.StartsWith(Path.Combine("Components", "Sample.razor") + ":3 ", offenders[0], StringComparison.Ordinal);
        Assert.Contains("no datetime", offenders[0], StringComparison.Ordinal);
        Assert.Contains(":5 ", offenders[1], StringComparison.Ordinal);
        Assert.Contains("<time>", offenders[1], StringComparison.Ordinal);
        Assert.Contains($"{DateTimeAttribute}=\" \"", offenders[1], StringComparison.Ordinal);
    }

    /// <summary>Every <c>&lt;time&gt;</c> of every component, in the order the files write them.</summary>
    private static List<TimeElement> Times() =>
        [.. RazorMarkup().SelectMany(markup => Scan(markup.File, markup.Text))];

    /// <summary>The <c>&lt;time&gt;</c> elements a piece of markup opens: every start tag whose element name is
    /// <c>time</c>, with the <c>datetime</c> attributes that tag writes. A whole component and a sample of markup
    /// the tests above read the rule through are scanned the same way.</summary>
    private static List<TimeElement> Scan(string file, string text)
    {
        var times = new List<TimeElement>();

        foreach (var start in MarkupScan.OpeningTags(text))
        {
            var tag = MarkupScan.TagOf(text, start);
            var element = NameOf(tag);

            if (element != TimeTag)
            {
                continue;
            }

            List<string?> datetimes = [.. AttributesOf(tag)
                .Where(attribute => attribute.Name.Equals(DateTimeAttribute, StringComparison.OrdinalIgnoreCase))
                .Select(attribute => attribute.Value)];

            times.Add(new TimeElement(file, LineOf(text, start), element, datetimes));
        }

        return times;
    }

    /// <summary>Why the times are refused, one line each: the file, the line of the tag and the attribute it lacks.</summary>
    private static List<string> ReportsOf(IEnumerable<TimeElement> times) =>
        [.. times
            .Where(time => time.Offence is not null)
            .Select(time => time.Report)];

    /// <summary>The offence of the sample's first <c>&lt;time&gt;</c> - <c>null</c> when it names its moment -
    /// refusing a sample that writes no time element at all, so a test cannot pass on markup the rule never read.</summary>
    private static string? OffenceOf(string text)
    {
        var times = Scan("Sample.razor", text);

        Assert.NotEmpty(times);

        return times[0].Offence;
    }

    /// <summary>The report of the sample's first offending <c>&lt;time&gt;</c>, refusing a sample the rule finds
    /// nothing to say about.</summary>
    private static string ReportOf(string text)
    {
        var offenders = ReportsOf(Scan("Sample.razor", text));

        Assert.Single(offenders);

        return offenders[0];
    }

    /// <summary>The element a tag opens, in lower case: HTML spells an element name the same whichever way its
    /// letters go, and a <c>&lt;TIME&gt;</c> owes a moment just the same.</summary>
    private static string NameOf(string tag)
    {
        var end = 1;

        while (end < tag.Length && (char.IsLetterOrDigit(tag[end]) || tag[end] is '-' or '_' or ':'))
        {
            end++;
        }

        return tag[1..end].ToLowerInvariant();
    }

    /// <summary>The attributes a tag writes, past its element name: each name as written and the value after it, if
    /// any. A quoted value is run through to the quote that closes it, Razor's own quotes included, and the "/" that
    /// opens a self-closing tag ends the tag rather than naming an attribute.</summary>
    private static List<TagAttribute> AttributesOf(string tag)
    {
        var attributes = new List<TagAttribute>();
        var at = 1;

        while (at < tag.Length && !(char.IsWhiteSpace(tag[at]) || tag[at] == '>'))
        {
            at++;
        }

        while (true)
        {
            at = MarkupScan.SkipSpace(tag, at);

            if (at >= tag.Length || tag[at] is '>' or '/')
            {
                break;
            }

            var name = at;

            while (at < tag.Length && !(char.IsWhiteSpace(tag[at]) || tag[at] is '=' or '>' or '/'))
            {
                at++;
            }

            var attributeName = tag[name..at];

            at = MarkupScan.SkipSpace(tag, at);

            string? value = null;

            if (at < tag.Length && tag[at] == '=')
            {
                at = MarkupScan.SkipSpace(tag, at + 1);
                value = ValueAt(tag, ref at);
            }

            attributes.Add(new TagAttribute(attributeName, value));
        }

        return attributes;
    }

    /// <summary>The value that stands at <paramref name="at"/>, read from its opening quote to the closing one that
    /// Razor does not trick the read with, or to the whitespace that ends a bare one. The index is left after the
    /// value. A pair of quotes with nothing between them is read as the empty string, and an attribute written
    /// without a value at all as <c>null</c>.</summary>
    private static string? ValueAt(string tag, ref int at)
    {
        var quote = at < tag.Length && (tag[at] == '"' || tag[at] == '\'') ? tag[at] : '\0';

        if (quote == '\0')
        {
            var bare = at;

            while (at < tag.Length && !(char.IsWhiteSpace(tag[at]) || tag[at] == '>'))
            {
                at++;
            }

            return tag[bare..at];
        }

        var open = at;
        var close = MarkupScan.EndOfValue(tag, open, quote);

        at = close < tag.Length ? close + 1 : close;

        return close <= open + 1 ? string.Empty : tag[(open + 1)..close];
    }

    private static int LineOf(string text, int at) => text.AsSpan(0, at).Count('\n') + 1;

    private static string DashboardRoot([CallerFilePath] string here = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "Dashboard"));

    /// <summary>Every <c>.razor</c> file of the app, as the path the assertion prints and its text. The build's
    /// copies under <c>bin</c> and <c>obj</c> hold the same markup twice, so those directories are walked past.</summary>
    private static IEnumerable<(string File, string Text)> RazorMarkup()
    {
        var root = DashboardRoot();

        foreach (var path in Directory.EnumerateFiles(root, "*.razor", SearchOption.AllDirectories).OrderBy(p => p, StringComparer.Ordinal))
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
