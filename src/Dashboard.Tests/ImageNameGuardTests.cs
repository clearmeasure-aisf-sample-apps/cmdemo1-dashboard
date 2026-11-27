using System.Runtime.CompilerServices;

namespace Dashboard.Tests;

/// <summary>
/// A picture says nothing to a reader on its own. An <c>&lt;img&gt;</c> without an <c>alt</c> is read out as the file
/// name of its source, or as nothing at all; an element that takes <c>role="img"</c> - the chart of
/// <c>WeekChart.razor</c>, the sparkline of <c>TrendLine.razor</c>, the language bar of <c>CodeCard.razor</c>, which
/// draw their meaning in geometry rather than in words - says "a picture is here" and not what the picture shows. So
/// every <c>&lt;img&gt;</c> of this app writes an <c>alt</c>, and every tag that carries <c>role="img"</c> names
/// itself with a non-empty <c>aria-label</c> or <c>aria-labelledby</c> (#146).
/// <para>
/// A guard walks every component and refuses what says less. An <c>alt</c> is asked for by presence only, since
/// <c>alt=""</c> is a real answer: it declares the image decoration and tells a reader to walk past it. A name is
/// asked for by content, since an empty <c>aria-label=""</c> is the silence this rule exists to refuse. A name Razor
/// builds at runtime - <c>aria-label="@Chart.Title"</c> - counts as said, because the component's own tests are
/// where its value is read; a <c>role</c> Razor builds is not looked at, because there is no role here to compare.
/// </para>
/// <para>
/// The tag is read to the "&gt;" that closes it, past the Razor expressions, the quoted values that carry a "&gt;" of
/// their own and the line breaks they spread over - the way <c>SvgIconGuardTests</c>, <c>MeterGuardTests</c> and
/// <c>AriaHiddenFocusGuardTests</c> read them: the image of <c>ApplicationView.razor</c> carries an
/// <c>@onerror="() => _imageFailed = true"</c> whose arrow holds a "&gt;" of its own, three attributes before the tag
/// closes.
/// </para>
/// </summary>
public class ImageNameGuardTests
{
    /// <summary>The element this rule reads by name, and the role it also reads: an element that takes this role owes
    /// a name exactly as an image does.</summary>
    private const string ImageElement = "img";

    /// <summary>The attribute an <c>&lt;img&gt;</c> owes - by presence, since an empty one declares decoration.</summary>
    private const string AltAttribute = "alt";

    /// <summary>How a missing name is named in a report: either of the two attributes gives the element its name.</summary>
    private const string NameAttribute = "aria-label or aria-labelledby";

    /// <summary>The attributes that name an element drawn as a picture.</summary>
    private static readonly string[] NameAttributes = ["aria-label", "aria-labelledby"];

    /// <summary>
    /// One start tag of one component: the file to print, the line its tag opens on, its element name in lower case,
    /// and the attributes the tag writes.
    /// </summary>
    private sealed record StartTag(string File, int Line, string Name, IReadOnlyList<TagAttribute> Attributes)
    {
        /// <summary>True of the element HTML calls an image.</summary>
        public bool IsImage => Name.Equals(ImageElement, StringComparison.Ordinal);

        /// <summary>True of a tag that writes <c>role="img"</c> among its roles. A role Razor builds says nothing this
        /// guard can read, and <c>role="image"</c> is another role.</summary>
        public bool CarriesImageRole
        {
            get
            {
                var role = ValueOf("role");

                return role is not null
                       && !role.Contains('@', StringComparison.Ordinal)
                       && role.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)
                           .Contains(ImageElement, StringComparer.OrdinalIgnoreCase);
            }
        }

        /// <summary>Why the guard refuses this tag, in the words a failure prints, or <c>null</c> when it says what it
        /// shows. A tag is asked about once: an <c>&lt;img role="img"&gt;</c> that names nothing is reported by the
        /// rule its element owes first, the image rule.</summary>
        public string? Offence
        {
            get
            {
                if (IsImage && !Has(AltAttribute))
                {
                    return $"<img> writes no {AltAttribute}: where a picture stands a reader hears nothing";
                }

                if (CarriesImageRole && !HasName)
                {
                    return $"role=\"{ImageElement}\" writes no {NameAttribute}: a reader is told a picture is drawn here and not what it shows";
                }

                return null;
            }
        }

        /// <summary>True when one of the name attributes is there and says something - a word of its own or a value
        /// Razor builds.</summary>
        private bool HasName => NameAttributes.Any(attribute => SaysSomething(ValueOf(attribute)));

        private bool Has(string name) =>
            Attributes.Any(attribute => attribute.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        /// <summary>The value the tag's first attribute of that name holds, unquoted; <c>null</c> when the attribute
        /// is written without a value at all, and the name itself absent when the tag writes no such attribute.</summary>
        private string? ValueOf(string name) =>
            Attributes.FirstOrDefault(attribute => attribute.Name.Equals(name, StringComparison.OrdinalIgnoreCase))?.Value;

        private static bool SaysSomething(string? value) =>
            value is not null && (value.Contains('@', StringComparison.Ordinal) || value.Any(c => !char.IsWhiteSpace(c)));
    }

    /// <summary>One attribute of a tag: its name as written and its value without the quotes around it - <c>null</c>
    /// when the attribute is written bare, which HTML reads as an empty value.</summary>
    private sealed record TagAttribute(string Name, string? Value);

    /// <summary>
    /// Every image of every component carries an <c>alt</c>, and every tag drawn as an image names itself.
    /// </summary>
    [Fact]
    public void EveryImageAndEveryElementDrawnAsAnImageIsNamed()
    {
        Assert.Empty(ReportsOf(Scanned()));
    }

    /// <summary>
    /// A guard that found nothing has seen nothing: the app draws one image today - the assembly diagram of
    /// <c>ApplicationView.razor</c> - and four elements that take the image role - the charts of
    /// <c>HistoryStrip.razor</c>, <c>TrendLine.razor</c> and <c>WeekChart.razor</c>, the language bar of
    /// <c>CodeCard.razor</c> - so a scan that reaches far fewer never read the markup, and the clean verdict above
    /// would mean nothing.
    /// </summary>
    [Fact]
    public void TheGuardFindsTheImagesOfTheApp()
    {
        var tags = Scanned();
        var images = tags.Where(tag => tag.IsImage).ToList();
        var drawn = tags.Where(tag => tag.CarriesImageRole).ToList();

        Assert.True(
            images.Count >= 1,
            $"no <{ImageElement}> was read under {DashboardRoot()}: the guard is checking nothing");

        Assert.True(
            drawn.Count >= 3,
            $"only {drawn.Count} role=\"{ImageElement}\" element(s) were read under {DashboardRoot()}: the guard is checking nothing");

        Assert.Contains(
            images,
            image => image.File.Equals(Path.Combine("Components", "ApplicationView.razor"), StringComparison.Ordinal));

        Assert.Contains(drawn, tag => tag.Name == "svg");
        Assert.Contains(drawn, tag => tag.Name == "div");
        Assert.True(
            drawn.Select(tag => tag.File).Distinct(StringComparer.Ordinal).Count() >= 2,
            $"the {drawn.Count} role=\"{ImageElement}\" element(s) read live in one file only: the walk does not reach the app");
    }

    /// <summary>The rule refuses the picture that says nothing: an image with no <c>alt</c> at all, and an element
    /// drawn as an image whose name is missing, empty or all the whitespace there is.</summary>
    [Fact]
    public void TheRuleRefusesAPictureThatSaysNothing()
    {
        var text = """
            <div class="card">
                <img src="@_logo.AbsoluteUri" width="16" height="16" />
                <svg viewBox="0 0 8 8" role="img"></svg>
                <div class="langbar" role="img" aria-label=""></div>
                <div role="img" aria-label="   " aria-labelledby=""></div>
                <span role="img" aria-label></span>
            </div>
            """;

        var offenders = ReportsOf(Scan("Sample.razor", text));

        // A name of nothing but spaces is read as the spaces there are, and says as little as no name at all.
        var whitespace = Scan("Sample.razor", text).Single(tag => tag.Line == 5);

        Assert.Contains(whitespace.Attributes, attribute => attribute.Name == "aria-label" && attribute.Value == "   ");
        Assert.NotNull(whitespace.Offence);
        Assert.Equal(5, offenders.Count);

        Assert.Contains(":2 ", offenders[0], StringComparison.Ordinal);
        Assert.Contains("no alt", offenders[0], StringComparison.Ordinal);

        Assert.All(offenders.Skip(1), report => Assert.Contains("no aria-label or aria-labelledby", report, StringComparison.Ordinal));

        Assert.Contains(":3 ", offenders[1], StringComparison.Ordinal);
        Assert.Contains(":4 ", offenders[2], StringComparison.Ordinal);
        Assert.Contains(":5 ", offenders[3], StringComparison.Ordinal);
        Assert.Contains(":6 ", offenders[4], StringComparison.Ordinal);
    }

    /// <summary>The rule accepts what the app actually writes: a picture named in words, a picture named by the
    /// markup beside it, a name Razor builds, and the <c>alt=""</c> that declares an image decoration rather than
    /// leaving it unspoken. An element with no image role and no <c>&lt;img&gt;</c> owes this rule nothing.</summary>
    [Fact]
    public void TheRuleAcceptsAPictureThatSaysWhatItShows()
    {
        var text = """
            <div class="card">
                <img src="@image.AbsoluteUri" alt="@ApplicationText.AltText(Deployable, version.Text, architecture)" />
                <img src="/dots.png" alt="" class="swatch" />
                <svg role="img" aria-label="@($"Weeks {first} to {last}")" focusable="false"></svg>
                <div class="langbar" role="img"
                     aria-labelledby="@($"langbar-title-{Id}")"></div>
                <p class="note">No picture here at all.</p>
            </div>
            """;

        var tags = Scan("Sample.razor", text);

        Assert.Equal(6, tags.Count);
        Assert.Equal(2, tags.Count(tag => tag.IsImage));
        Assert.Equal(2, tags.Count(tag => tag.CarriesImageRole));
        Assert.Empty(ReportsOf(tags));
    }

    /// <summary>Only the attribute of that very name counts, and only the element of that very name: a
    /// <c>data-alt</c> is not an <c>alt</c>, a <c>data-aria-label</c> is not a name, an <c>&lt;imgage&gt;</c> is
    /// another element, and <c>role="image"</c> or a role Razor builds at runtime is not the role read here.</summary>
    [Fact]
    public void OnlyTheImagesOwnAttributesAndNameCount()
    {
        var text = """
            <main>
                <img src="/a.png" data-alt="Not an alternative text" />
                <imgage src="/b.png"></imgage>
                <span role="img" data-aria-label="Not a name"></span>
                <span role="image" aria-label="Another role, this rule owes it nothing"></span>
                <span role="@_role" aria-hidden="true"></span>
            </main>
            """;

        var tags = Scan("Sample.razor", text);

        Assert.Equal(6, tags.Count);
        Assert.Equal("main img imgage span span span", string.Join(' ', tags.Select(tag => tag.Name)));

        var offenders = ReportsOf(tags);

        Assert.Equal(2, offenders.Count);
        Assert.Contains(":2 ", offenders[0], StringComparison.Ordinal);
        Assert.Contains("no alt", offenders[0], StringComparison.Ordinal);
        Assert.Contains(":4 ", offenders[1], StringComparison.Ordinal);
    }

    /// <summary>A tag ends at the "&gt;" that closes it, not at the first "&gt;" of the file: a class may hold a
    /// comparison, a title a "&gt;" of its own inside the quotes Razor carries, and an event handler an arrow whose
    /// "=&gt;" is read as text - the image of <c>ApplicationView.razor</c> writes all three over four lines. Read
    /// short, the <c>alt</c> below them would be outside the tag and the guard would cry over a picture that names
    /// itself.</summary>
    [Fact]
    public void TheTagReadRunsPastRazorAndLineBreaks()
    {
        var text = """
            <main>
                <img class="diagram @(failed ? "diagram--failed" : null)"
                     src="@image.AbsoluteUri" title="@("3 requests > the last minute")"
                     alt="@ApplicationText.AltText(Deployable, version.Text, architecture)"
                     @onerror="() => _imageFailed = true" />
            </main>
            """;

        var tags = Scan("Sample.razor", text);

        Assert.Equal(2, tags.Count);

        var image = tags[1];

        Assert.Equal("img", image.Name);
        Assert.Equal(2, image.Line);
        Assert.Null(image.Offence);
        Assert.Contains(image.Attributes, attribute => attribute.Name == "@onerror");
        Assert.Contains(image.Attributes, attribute => attribute.Name == "alt" && attribute.Value!.Contains("AltText", StringComparison.Ordinal));
        Assert.Contains(image.Attributes, attribute => attribute.Name == "title" && attribute.Value!.Contains('>'));
    }

    /// <summary>The report names the file the way the walk prints it and the line the tag opens on - the first of
    /// them, not the line its missing name would have sat on - and reads every tag of a file on its own.</summary>
    [Fact]
    public void TheReportNamesTheFileAndTheLineOfTheTag()
    {
        var text = """
            <div class="toolbar">
                <img class="logo"
                     src="/logo.svg" />
                <svg role="img"></svg>
            </div>
            """;

        var offenders = ReportsOf(Scan(Path.Combine("Components", "Sample.razor"), text));

        Assert.Equal(2, offenders.Count);
        Assert.StartsWith(Path.Combine("Components", "Sample.razor") + ":2 ", offenders[0], StringComparison.Ordinal);
        Assert.Contains("no alt", offenders[0], StringComparison.Ordinal);
        Assert.Contains(":4 ", offenders[1], StringComparison.Ordinal);
        Assert.Contains("no aria-label or aria-labelledby", offenders[1], StringComparison.Ordinal);
    }

    /// <summary>Every start tag of every component, in the order the files write them.</summary>
    private static List<StartTag> Scanned() =>
        [.. RazorMarkup().SelectMany(markup => Scan(markup.File, markup.Text))];

    /// <summary>The start tags of one file, each with the attributes its tag writes.</summary>
    private static List<StartTag> Scan(string file, string text)
    {
        var tags = new List<StartTag>();

        foreach (var start in MarkupScan.OpeningTags(text))
        {
            var tag = MarkupScan.TagOf(text, start);
            var name = NameOf(tag);

            if (name.Length > 0)
            {
                tags.Add(new StartTag(file, LineOf(text, start), name, AttributesOf(tag)));
            }
        }

        return tags;
    }

    /// <summary>Why the tags are refused, one line each: the file, the line of the tag and the offence.</summary>
    private static List<string> ReportsOf(IEnumerable<StartTag> tags) =>
        [.. tags
            .Where(tag => tag.Offence is not null)
            .Select(tag => $"{tag.File}:{tag.Line} {tag.Offence}")];

    /// <summary>The element name a tag opens, in lower case: HTML leaves an element name case-free, and a
    /// <c>&lt;IMG&gt;</c> owes an <c>alt</c> the same as an <c>&lt;img&gt;</c>.</summary>
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

        while (at < tag.Length && !char.IsWhiteSpace(tag[at]))
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

            while (at < tag.Length && !(char.IsWhiteSpace(tag[at]) || tag[at] == '='))
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
    /// value.</summary>
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
