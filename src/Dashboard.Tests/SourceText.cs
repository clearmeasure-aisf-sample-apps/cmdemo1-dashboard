using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Dashboard.Tests;

/// <summary>
/// The one shared read of a folder of <c>.cs</c> source that the three coverage guards (Health, Cluster,
/// Runtime/Work) read the same way: the files under a root, the <c>public static</c> methods the source
/// declares, the <c>Type.Method(</c> calls the source writes, and the line a character stands on - with the
/// comments and the content of the strings blanked, so a name that only stands in a comment or in the text a
/// string carries is read as nothing.
/// </summary>
/// <para>
/// The three guards each kept their own copy of this read before: three walks over <c>*.cs</c> that walked
/// past the build's copies under <c>bin</c> and <c>obj</c> by the same idiom, three copies of the line-number
/// arithmetic, and three different ways to tell a comment from a call - one that blanked only the lines it
/// recognised, one that carried a full lexer over comments and strings, and one that relied on the shape of
/// the name it was looking for. A declaration shape one read and another did not, a comment convention one
/// stripped and another missed, were the three ways the rule could drift. This read is the one they share
/// now; each guard still keeps its own folder list, its own floors and its own report line, so the guard
/// "read something" tests keep their teeth.
/// </para>
public static partial class SourceText
{
    /// <summary>One file of a folder, as the path the caller prints it (relative to the root) and its text.</summary>
    public sealed record SourceFile(string Path, string Text);

    /// <summary>One <c>public static</c> method the source declares: the file and the line it stands on, the type
    /// that encloses it and its own name - which is all the rule reads, since that is what a call writes. A
    /// declaration read with no enclosing type names its type as the empty string, so the caller can tell a
    /// declaration the scan could not place from one it placed.</summary>
    public sealed record Declaration(string File, int Line, string Type, string Method)
    {
        /// <summary>The name the call writes: <c>Type.Method</c>.</summary>
        public string Key => $"{Type}.{Method}";
    }

    /// <summary>One <c>Type.Method(</c> the source writes: the type and the method, the file and the line it is
    /// written on. Only a call in code writes a name; a mention in a comment or in the text a string carries is
    /// read as nothing, so the read reports only the calls it can stand behind.</summary>
    public sealed record CallSite(string Type, string Method, string File, int Line)
    {
        /// <summary>The name the call writes: <c>Type.Method</c>.</summary>
        public string Key => $"{Type}.{Method}";
    }

    // --------------------------------------------------------------- the files

    /// <summary>
    /// Every <c>.cs</c> file under a root, as the path the assertion prints it relative to the root and its
    /// text, in the order the paths spell. The build's copies under <c>bin</c> and <c>obj</c> hold the same
    /// source twice, so those directories are walked past - the one idiom the three guards kept, now read once.
    /// </summary>
    public static List<SourceFile> Files(string root)
    {
        var files = new List<SourceFile>();

        foreach (var path in Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories).OrderBy(p => p, StringComparer.Ordinal))
        {
            var relative = Path.GetRelativePath(root, path);

            if (IsBuildCopy(relative))
            {
                continue;
            }

            files.Add(new SourceFile(relative, File.ReadAllText(path)));
        }

        return files;
    }

    /// <summary>True when the path the assertion prints walks through a <c>bin</c> or an <c>obj</c>, which is the
    /// build's copy of the source and not the source the app runs.</summary>
    public static bool IsBuildCopy(string relative) =>
        relative.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Any(part =>
            part.Equals("bin", StringComparison.OrdinalIgnoreCase) || part.Equals("obj", StringComparison.OrdinalIgnoreCase));

    // --------------------------------------------------------------- the line

    /// <summary>The line a character of the file stands on, from one: the count of the line breaks before it, plus
    /// one. The one arithmetic the three guards kept, now read once.</summary>
    public static int LineOf(string text, int at) => text.AsSpan(0, at).Count('\n') + 1;

    // --------------------------------------------------------------- the read

    /// <summary>
    /// The <c>public static</c> methods the source of one file declares, in the order the lines stand: a
    /// <c>public static</c> whose name opens the "(" that takes its parameters, read under the type the line sits
    /// in - a type declaration opens the scope of everything under it. The comments and the content of the
    /// strings are blanked first, so the words of a sentence or a markup assertion are not a declaration, and the
    /// line numbers of the declarations around them stay the numbers a reader looks for. A method that stands
    /// above every type of its file is reported with the empty type, which is the point: the read does not walk
    /// past what it cannot place.
    /// </summary>
    public static List<Declaration> Declarations(string file, string text)
    {
        // The blanked text keeps the same length and the same line breaks as the original, so a match of it
        // stands at the same index and on the same line as the declaration it blanks.
        var code = BlankCommentsAndStrings(text);
        var declarations = new List<Declaration>();
        var type = string.Empty;

        foreach (var line in code.Split('\n'))
        {
            if (TypeDeclaration().Match(line) is { Success: true } typeMatch)
            {
                type = typeMatch.Groups[1].Value;
                continue;
            }

            if (MethodDeclaration().Match(line) is { Success: true } methodMatch)
            {
                declarations.Add(new Declaration(file, LineOf(code, methodMatch.Index), type, methodMatch.Groups[1].Value));
            }
        }

        return declarations;
    }

    /// <summary>
    /// The <c>Type.Method(</c> the source of one file writes, each reported on the line it opens on: the name and
    /// the bracket, with the spaces and the line breaks a long call wraps on read as what they spell. The
    /// comments and the content of the strings are blanked first, so a doc comment that quotes a helper names
    /// nothing, a mention in the markup a test carries is not a call, and a longer name that ends in the pattern
    /// is another member still - <c>AvailabilityText.Counts</c> is not <c>AvailabilityText.Count</c>.
    /// </summary>
    public static List<CallSite> Calls(string file, string text)
    {
        var code = BlankCommentsAndStrings(text);
        var calls = new List<CallSite>();

        foreach (Match match in CallPattern().Matches(code))
        {
            var before = match.Index > 0 ? code[match.Index - 1] : '\0';
            if (before != '\0' && (char.IsLetterOrDigit(before) || before == '_'))
            {
                continue;
            }

            calls.Add(new CallSite(match.Groups[1].Value, match.Groups[2].Value, file, LineOf(code, match.Index)));
        }

        return calls;
    }

    // --------------------------------------------------------------- the blanking

    /// <summary>
    /// The text of a file with its comments and the content of its strings blanked to spaces, the line breaks
    /// kept and the length kept, so a character of the original and its stand-in stand at the same index and on
    /// the same line. A quote opens the string it opens - a raw string of three or more quotes runs to the run
    /// that closes it, a verbatim one to its lone quote, a doubled one reads as content, an escaped one as
    /// nothing but an escape - and a <c>//</c> runs to the end of its line, a <c>/*</c> to its <c>*/</c>. What is
    /// blanked is the comment and the string content only; the quote that opens a string and the character that
    /// closes it stand as they did, so a name written in code still stands where it stood.
    /// </summary>
    public static string BlankCommentsAndStrings(string text)
    {
        var blanked = new char[text.Length];
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
                        blanked[i] = '\n';
                        state = LexState.Code;
                    }
                    else
                    {
                        blanked[i] = ' ';
                    }

                    i++;
                    break;

                case LexState.BlockComment:
                    if (c == '*' && i + 1 < text.Length && text[i + 1] == '/')
                    {
                        blanked[i] = ' ';
                        blanked[i + 1] = ' ';
                        state = LexState.Code;
                        i += 2;
                    }
                    else
                    {
                        blanked[i] = c == '\n' ? '\n' : ' ';
                        i++;
                    }

                    break;

                case LexState.String:
                    i = EndOfString(text, blanked, i, kind, rawQuotes, ref state);
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
                        blanked[i] = ' ';
                        blanked[i + 1] = ' ';
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
                            Array.Fill(blanked, ' ', i, run);
                            i += run;
                        }
                        else
                        {
                            kind = StringKind.Regular;
                            state = LexState.String;
                            blanked[i] = '"';
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
                            blanked[i] = ' ';
                            Array.Fill(blanked, ' ', i + 1, run);
                            i += 1 + run;
                        }
                        else
                        {
                            kind = StringKind.Verbatim;
                            state = LexState.String;
                            blanked[i] = '@';
                            blanked[i + 1] = '"';
                            i += 2;
                        }

                        break;
                    }

                    if (c == '\'')
                    {
                        kind = StringKind.Char;
                        state = LexState.String;
                        blanked[i] = '\'';
                        i++;
                        break;
                    }

                    blanked[i] = c;
                    i++;
                    break;
            }
        }

        return new string(blanked);
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

    /// <summary>
    /// Just past the string that opens at <paramref name="at"/>, or the end of the text when nothing closes it.
    /// The content of the string is blanked to spaces in <paramref name="blanked"/>, the quotes that open and
    /// close it stand as they did, and the line breaks inside it stand as line breaks, so a call the string
    /// carries is not read as a call and a name that wraps on a line break inside the string keeps its line. The
    /// state is left in code: the next character is the code's again.
    /// </summary>
    private static int EndOfString(string text, char[] blanked, int at, StringKind kind, int rawQuotes, ref LexState state)
    {
        while (at < text.Length)
        {
            var c = text[at];

            if (kind is StringKind.Regular or StringKind.Char)
            {
                var close = kind == StringKind.Regular ? '"' : '\'';

                if (c == '\\' && at + 1 < text.Length)
                {
                    blanked[at] = ' ';
                    blanked[at + 1] = ' ';
                    at += 2;
                    continue;
                }

                if (c == close)
                {
                    blanked[at] = close;
                    state = LexState.Code;
                    return at + 1;
                }

                blanked[at] = c == '\n' ? '\n' : ' ';
                at++;
                continue;
            }

            if (kind == StringKind.Verbatim)
            {
                if (c == '"')
                {
                    if (at + 1 < text.Length && text[at + 1] == '"')
                    {
                        blanked[at] = ' ';
                        blanked[at + 1] = ' ';
                        at += 2;
                        continue;
                    }

                    blanked[at] = '"';
                    state = LexState.Code;
                    return at + 1;
                }

                blanked[at] = c == '\n' ? '\n' : ' ';
                at++;
                continue;
            }

            // StringKind.Raw
            if (c == '"')
            {
                var run = QuoteRun(text, at);
                if (run >= rawQuotes)
                {
                    // The closing run stands as it did.
                    for (var k = 0; k < run; k++)
                    {
                        blanked[at + k] = '"';
                    }

                    state = LexState.Code;
                    return at + run;
                }
            }

            blanked[at] = c == '\n' ? '\n' : ' ';
            at++;
        }

        state = LexState.Code;
        return text.Length;
    }

    // --------------------------------------------------------------- the patterns

    /// <summary>A type that opens its scope: a class, a record, a struct, an enum or an interface, public or
    /// internal, with the modifiers the language allows before the keyword.</summary>
    [GeneratedRegex(@"^(?:public|internal)\s+(?:(?:sealed|static|partial|readonly|abstract)\s+)*(?:class|record|struct|enum|interface)\s+([A-Za-z_]\w*)")]
    private static partial Regex TypeDeclaration();

    /// <summary>A <c>public static</c> method: its name, which is what a call writes. A <c>public static</c> of
    /// another kind - a property, a field, an event - is not a method the page calls, so the name must stand
    /// before the "(" that opens its parameters, and the return type in between carries no "=" or ";" or "{" of
    /// its own. The return type may carry the "(" of a tuple - <c>(DateTimeOffset? At, bool Live) ReadAt(</c> or
    /// the "(" of a tuple a generic wraps - <c>IReadOnlyList&lt;(string Alias, string Label)&gt; Boxes(</c> - and
    /// the read reads past it rather than walking past the whole method for, because the name that stands before
    /// the "(" that opens the parameters is the name a call writes.</summary>
    [GeneratedRegex(@"^\s*public\s+static\s+[^=;{}]*?\b([A-Za-z_]\w*)\s*\(")]
    private static partial Regex MethodDeclaration();

    /// <summary>A call of a member of a type: <c>AvailabilityText.Count(</c>, the spaces and the line breaks
    /// between the name and the dot included, since a long call wraps there. The type is written in the form a
    /// call writes it - its first letter a capital, the rest of the name as the type spells it.</summary>
    [GeneratedRegex(@"\b([A-Z]\w*)\s*\.\s*([A-Za-z_]\w*)\s*\(")]
    private static partial Regex CallPattern();

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
}
