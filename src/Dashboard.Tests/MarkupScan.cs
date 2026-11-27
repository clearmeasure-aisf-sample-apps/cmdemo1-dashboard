namespace Dashboard.Tests;

/// <summary>
/// The one tag scanner of the markup guards: how a start tag is found in a component's text, and how it is read from
/// its <c>&lt;</c> to the <c>&gt;</c> that closes it, past the quoted values and the Razor expressions that may hold a
/// <c>&gt;</c> of their own. Each guard used to carry its own copy of this reading - <see cref="OpeningTags" /> and
/// <see cref="NextTag" /> in fourteen files, <see cref="TagOf" />, <see cref="SkipSpace" />, <see cref="EndOfValue" />
/// and <see cref="EndOfExpression" /> in eleven - letter for letter the same, so a rule changed in one file and missed
/// in another would have made two guards read the same markup two ways (#1382).
/// <para>
/// Only the scanning lives here. Each guard keeps its own rule, its own record of what a tag says, its own
/// <c>Scan</c>, <c>AttributesOf</c> and allow-list, and reaches the source tree through <see cref="MarkupGuards" />.
/// The reading here runs through both kinds of quote, <c>"</c> and <c>'</c>. The seven guards whose own
/// <c>TagOf</c> runs through <c>"</c> alone - and so would cut a tag at a different place - keep their copy until a
/// change of their own moves them, and <see cref="MarkupGuardSupportTests" /> holds the number of those copies from
/// above so no new one joins them.
/// </para>
/// </summary>
internal static class MarkupScan
{
    /// <summary>The character that opens a Razor expression inside an attribute value: <c>@(</c>.</summary>
    private const char Razor = '@';

    /// <summary>Every <c>&lt;</c> that opens a tag, closing tags left out.</summary>
    public static List<int> OpeningTags(string text)
    {
        var starts = new List<int>();

        for (var at = NextTag(text, 0); at >= 0; at = NextTag(text, at + 1))
        {
            if (text[at + 1] != '/')
            {
                starts.Add(at);
            }
        }

        return starts;
    }

    /// <summary>The next <c>&lt;</c> that opens or closes a tag at or after <paramref name="from" />, or -1. A
    /// <c>&lt;</c> of Razor or of prose - <c>@(count &lt; 1)</c>, "if &lt; 2px" - is followed by a space, a digit or
    /// punctuation and opens nothing.</summary>
    public static int NextTag(string text, int from)
    {
        for (var at = text.IndexOf('<', from); at >= 0; at = text.IndexOf('<', at + 1))
        {
            if (at + 1 < text.Length && (text[at + 1] == '/' || char.IsLetter(text[at + 1])))
            {
                return at;
            }
        }

        return -1;
    }

    /// <summary>The first position at or after <paramref name="at" /> that is not whitespace, or the end of the
    /// text.</summary>
    public static int SkipSpace(string text, int at)
    {
        while (at < text.Length && char.IsWhiteSpace(text[at]))
        {
            at++;
        }

        return at;
    }

    /// <summary>The whole opening tag, from its <c>&lt;</c> at <paramref name="start" /> to the <c>&gt;</c> that
    /// closes it - or to the end of the text when nothing does. A value in either kind of quote is run through rather
    /// than read to its first quote, because Razor quotes inside it and a value may hold a <c>&gt;</c> of its own.
    /// The parentheses of an expression outside any value are counted too.</summary>
    public static string TagOf(string text, int start)
    {
        var depth = 0;

        for (var at = start; at < text.Length; at++)
        {
            var c = text[at];

            if (c is '"' or '\'')
            {
                at = EndOfValue(text, at, c);
                continue;
            }

            if (c == '(')
            {
                depth++;
            }
            else if (c == ')')
            {
                depth = Math.Max(0, depth - 1);
            }
            else if (c == '>' && depth == 0)
            {
                return text[start..(at + 1)];
            }
        }

        return text[start..];
    }

    /// <summary>The <paramref name="quote" /> that closes the value opened at <paramref name="open" />, or the end
    /// of the text when the value is never closed. A <c>@(</c> inside the value is read to the <c>)</c> that matches
    /// it, the string literals of the expression and their quotes skipped over.</summary>
    public static int EndOfValue(string text, int open, char quote)
    {
        for (var at = open + 1; at < text.Length; at++)
        {
            var c = text[at];

            if (c == quote)
            {
                return at;
            }

            if (c == Razor && at + 1 < text.Length && text[at + 1] == '(')
            {
                at = EndOfExpression(text, at + 1);
            }
        }

        return text.Length;
    }

    /// <summary>The <c>)</c> that closes the <c>(</c> at <paramref name="open" />, or the end of the text. The quotes
    /// of the expression's own string literals are run through, since a literal may hold anything a plain read would
    /// stop at.</summary>
    public static int EndOfExpression(string text, int open)
    {
        var depth = 0;

        for (var at = open; at < text.Length; at++)
        {
            var c = text[at];

            if (c is '"' or '\'')
            {
                var closing = text.IndexOf(c, at + 1);
                at = closing < 0 ? text.Length : closing;
            }
            else if (c == '(')
            {
                depth++;
            }
            else if (c == ')')
            {
                depth--;

                if (depth == 0)
                {
                    return at;
                }
            }
        }

        return text.Length;
    }
}
