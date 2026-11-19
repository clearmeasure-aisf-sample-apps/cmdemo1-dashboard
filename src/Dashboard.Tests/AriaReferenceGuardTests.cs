using System.Runtime.CompilerServices;

namespace Dashboard.Tests;

/// <summary>
/// An <c>aria-describedby</c> or an <c>aria-labelledby</c> is a pointer: it says "the text that names me lives at
/// that <c>id</c>". When no element carries that id the pointer leads nowhere and a screen reader falls back to
/// silence - the button reads as "Generate traffic" with no hint, the section reads with no title. A guard walks
/// every component and refuses a pointer that names an <c>id</c> the same file never writes (#131).
/// <para>
/// What is read is what can be read without running Razor: a plain value, which may hold several space-separated
/// ids (<c>aria-describedby="pause-help"</c>), and the double-quoted string literals of a Razor expression
/// (<c>aria-describedby="@(Running ? "traffic-environment-locked-help" : null)"</c>). Values whose ids are built at
/// runtime are skipped, because there is no id to compare here: a bare expression such as
/// <c>aria-labelledby="@LabelledBy"</c> or <c>aria-labelledby="@id"</c>, an interpolated one such as
/// <c>aria-labelledby="@($"{IdPrefix}-delivery")"</c>, and any expression that still reaches for a field outside its
/// string literals. Those components name their own ids from the same field on both sides of the pointer, so the
/// compiler and their bUnit tests are where they are checked.
/// </para>
/// </summary>
public class AriaReferenceGuardTests
{
    private const string IdAttribute = "id=\"";

    private static readonly string[] ReferenceAttributes = ["aria-describedby=\"", "aria-labelledby=\""];

    /// <summary>
    /// Pointers the guard would otherwise refuse because the <c>id</c> stands in another component's markup: a file
    /// that writes <c>aria-labelledby="some-title"</c> while the <c>h2 id="some-title"</c> lives in a child it
    /// renders. Such a pointer is legal HTML - ids are document-wide - but it is fragile, so every one of them is
    /// named here, file and id together, rather than the rule being loosened to "an id written somewhere". The list
    /// is checked for staleness below: an entry nobody points at any more fails, so it cannot quietly collect
    /// permissions for markup that no longer exists.
    /// </summary>
    private static readonly (string File, string Id)[] DefinedByAChildComponent = [];

    /// <summary>A single id one aria pointer names, and what the scan found around it.</summary>
    private sealed record Reference(string File, int Line, string Id, bool DefinedInFile, bool AllowedAsChildsOwn);

    /// <summary>
    /// Every aria pointer of every component resolves to an <c>id="..."</c> the same file writes, or is named by the
    /// allow-list above as one a child component renders - empty today, since every pointer resolves where it stands.
    /// </summary>
    [Fact]
    public void EveryAriaReferencePointsAtAnId()
    {
        var offenders = References()
            .Where(r => !(r.DefinedInFile || r.AllowedAsChildsOwn))
            .Select(r => $"{r.File}:{r.Line} points at id \"{r.Id}\", which the file never writes as id=\"{r.Id}\"")
            .ToList();

        Assert.Empty(offenders);
    }

    /// <summary>
    /// The guard has something to guard. Were the components ever moved or renamed, the walk would find no pointer
    /// and every assertion above would pass by reading nothing; the app holds far more than five of them today.
    /// </summary>
    [Fact]
    public void TheGuardFindsTheReferencesItClaimsToRead()
    {
        var seen = References();

        Assert.True(
            seen.Count >= 5,
            $"only {seen.Count} aria reference(s) were read under {DashboardRoot()}: the guard is checking nothing");
    }

    /// <summary>An allow-list may only cover a pointer that exists; a leftover entry would excuse a future one.</summary>
    [Fact]
    public void EveryAllowedReferenceIsOneTheFilesActuallyWrite()
    {
        var seen = References().Select(r => (r.File, r.Id)).ToHashSet();

        var stale = DefinedByAChildComponent.Where(entry => !seen.Contains(entry)).Select(e => $"{e.File} -> {e.Id}").ToList();

        Assert.Empty(stale);
    }

    private static List<Reference> References()
    {
        var all = new List<Reference>();

        foreach (var (file, text) in RazorMarkup())
        {
            var defined = DefinedIds(text);

            foreach (var (line, id) in ReferencedIds(text))
            {
                all.Add(new Reference(file, line, id, defined.Contains(id), DefinedByAChildComponent.Contains((file, id))));
            }
        }

        return all;
    }

    /// <summary>The (line, id) of every id the markup points at with <c>aria-describedby</c> or
    /// <c>aria-labelledby</c>, a value naming several ids once per id.</summary>
    private static IEnumerable<(int Line, string Id)> ReferencedIds(string text)
    {
        foreach (var attribute in ReferenceAttributes)
        {
            for (var at = text.IndexOf(attribute, StringComparison.Ordinal);
                 at >= 0;
                 at = text.IndexOf(attribute, at + attribute.Length, StringComparison.Ordinal))
            {
                var value = ValueOf(text, at + attribute.Length);

                if (value is null)
                {
                    continue;
                }

                foreach (var id in IdsOf(value))
                {
                    yield return (LineOf(text, at), id);
                }
            }
        }
    }

    /// <summary>
    /// The attribute's raw value, from after the opening quote to before the one that closes it. A Razor expression
    /// carries quotes of its own - <c>@(Running ? "traffic-environment-locked-help" : null)</c> - so a value that
    /// opens with <c>@(</c> is read to the ')' that matches it, the way Razor parses it, instead of stopping at the
    /// first quote inside.
    /// </summary>
    private static string? ValueOf(string text, int from)
    {
        if (from >= text.Length)
        {
            return null;
        }

        if (text[from] == '@' && from + 1 < text.Length && text[from + 1] == '(')
        {
            var depth = 0;

            for (var i = from; i < text.Length; i++)
            {
                var c = text[i];

                if (c == '(')
                {
                    depth++;
                }
                else if (c == ')')
                {
                    depth--;

                    if (depth == 0)
                    {
                        return text[from..(i + 1)];
                    }
                }
            }

            return null;
        }

        var close = text.IndexOf('"', from);

        return close < 0 ? null : text[from..close];
    }

    /// <summary>
    /// The literal ids a value points at, or none when its ids are built at runtime and cannot be read here: a bare
    /// expression (<c>@LabelledBy</c>), an interpolated string (<c>$"{IdPrefix}-code"</c>) or an expression that
    /// reaches for a field outside its string literals.
    /// </summary>
    private static IEnumerable<string> IdsOf(string value)
    {
        if (!value.StartsWith('@'))
        {
            // "help" or "title help" - several ids in one plain value. An '@' anywhere would build one at runtime.
            return value.Contains('@')
                ? []
                : value.Split([' ', '\t', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        }

        if (!value.StartsWith("@(", StringComparison.Ordinal))
        {
            return [];
        }

        var ids = new List<string>();
        var inner = value[2..^1];

        for (var i = 0; i < inner.Length; i++)
        {
            var c = inner[i];

            if (c == '@')
            {
                // Outside a literal, and not the "@(" this value already opened with: the expression names a field.
                return [];
            }

            if (c != '"')
            {
                continue;
            }

            if (i > 0 && inner[i - 1] == '$')
            {
                // $"..." - an interpolated id, not one that can be compared by name.
                return [];
            }

            var end = inner.IndexOf('"', i + 1);

            if (end < 0)
            {
                return [];
            }

            ids.AddRange(inner[(i + 1)..end].Split([' ', '\t', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries));
            i = end;
        }

        return ids;
    }

    /// <summary>The ids the file itself writes, as literal <c>id="..."</c> attributes. An <c>id</c> built by Razor
    /// (<c>id="@HeadingId"</c>) is not in the set: nothing that could be checked by name points at it.</summary>
    private static HashSet<string> DefinedIds(string text)
    {
        var ids = new HashSet<string>(StringComparer.Ordinal);

        for (var at = text.IndexOf(IdAttribute, StringComparison.Ordinal);
             at >= 0;
             at = text.IndexOf(IdAttribute, at + IdAttribute.Length, StringComparison.Ordinal))
        {
            // Only a tag's own whitespace opens an attribute: otherwise a data-id or a guid= would count as one.
            if (at > 0 && !(char.IsWhiteSpace(text[at - 1]) || text[at - 1] == '<'))
            {
                continue;
            }

            var from = at + IdAttribute.Length;
            var end = text.IndexOf('"', from);

            if (end < 0)
            {
                continue;
            }

            var value = text[from..end];

            if (!value.Contains('@'))
            {
                ids.Add(value);
            }
        }

        return ids;
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
