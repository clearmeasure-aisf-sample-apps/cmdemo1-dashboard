namespace Dashboard.Tests;

/// <summary>
/// The rule <see cref="LanguageSize.Read" /> says for a build file's language entries, pinned through the public
/// <see cref="BuildInfo.Parse" /> (the app declares no <c>InternalsVisibleTo</c>): which entries count, the order
/// they come out in, and the section that names nothing readable. What <see cref="BuildInfoEdgeCaseTests" />,
/// <see cref="BuildInfoCodeRoundingEdgeCaseTests" /> and <see cref="LanguageSizeEdgeCaseTests" /> already pin -
/// the sample's seven languages, the rounding of a language's lines, the zero and the negative, the name trimmed
/// or of another kind, and the record's own parts - is left to those files; what is pinned here is the rule where
/// the reading happens, the one place that says whether a language entry counts, and the order the kept entries
/// come out in.
/// </summary>
public class LanguageSizeReadEdgeCaseTests
{
    private static CodeSize Code(string section) => BuildInfo.Parse($$"""{ "code": {{section}} }""")!.Code!;

    /// <summary>
    /// A language whose lines are the zero a number can be is no language of the code: the reader asks for more
    /// than zero lines, and the zero does not qualify - the entry is dropped, the list empty, and the size with
    /// no lines of its own the nothing that is no size.
    /// </summary>
    [Fact]
    public void ALanguageOfZeroLinesIsDropped()
    {
        // A section whose only language is the zero lines is no size at all - and a build of only that is no build.
        Assert.Null(BuildInfo.Parse("""{ "code": { "languages": [ { "name": "C#", "lines": 0 } ] } }""")?.Code);

        // The zero beside another language is dropped while the other is kept.
        var beside = Code("""{ "languages": [ { "name": "C#", "lines": 0 }, { "name": "SQL", "lines": 300 } ] }""");

        Assert.Equal([new LanguageSize("SQL", 300, null)], beside.Languages);
        Assert.Equal(300L, beside.LinesOfCode);
    }

    /// <summary>
    /// A language with no lines at all is no language: the reader cannot ask a missing number whether it is above
    /// zero, so the entry is dropped - not kept with a zero, not made to throw. The size falls back to the lines
    /// of the languages that remain, and the section that names nothing readable is no size at all.
    /// </summary>
    [Fact]
    public void ALanguageWithoutLinesIsDropped()
    {
        var alone = BuildInfo.Parse("""{ "code": { "languages": [ { "name": "C#" } ] } }""");

        Assert.Null(alone?.Code);

        var beside = Code("""{ "languages": [ { "name": "C#" }, { "name": "SQL", "lines": 300 } ] }""");

        Assert.Equal([new LanguageSize("SQL", 300, null)], beside.Languages);
        Assert.Equal(300L, beside.LinesOfCode);
    }

    /// <summary>
    /// A language without a name is no language, the same way: the reader asks for a name as much as for lines,
    /// and the missing one disqualifies the entry - the empty name and the padded name the reader trims to the
    /// empty word are no names, and the entry is dropped rather than kept nameless.
    /// </summary>
    [Fact]
    public void ALanguageWithoutANameIsDropped()
    {
        var alone = BuildInfo.Parse("""{ "code": { "languages": [ { "lines": 61234 } ] } }""");

        Assert.Null(alone?.Code);

        var emptyName = Code("""{ "languages": [ { "name": "", "lines": 300 }, { "name": "SQL", "lines": 300 } ] }""");

        Assert.Equal([new LanguageSize("SQL", 300, null)], emptyName.Languages);
    }

    /// <summary>
    /// The files a language does not say are the nothing the record keeps, not the zero they are not: a language
    /// with lines but no files is kept with <see cref="LanguageSize.Files" /> standing the nothing it was not
    /// given - beside the language that says its files as the zero, which is the number it is, kept as the number.
    /// </summary>
    [Fact]
    public void ALanguageWithoutFilesIsKeptWithNoFiles()
    {
        var code = Code("""
            { "languages": [
                { "name": "C#", "lines": 100 },
                { "name": "SQL", "lines": 50, "files": 0 } ] }
            """);

        Assert.Equal([new LanguageSize("C#", 100, null), new LanguageSize("SQL", 50, 0)], code.Languages);
        Assert.Null(code.Languages[0].Files);
        Assert.Equal(0, code.Languages[1].Files);
        Assert.Equal(150L, code.LinesOfCode);
    }

    /// <summary>
    /// The languages come out largest first, and the equal sizes keep the order the file named them in: the order
    /// is stable, so a tie does not shuffle the two entries, and the size the file does not say is the lines the
    /// kept languages add up to, largest first.
    /// </summary>
    [Fact]
    public void LanguagesComeOutLargestFirstAndEqualSizesKeepFileOrder()
    {
        var code = Code("""
            { "languages": [
                { "name": "B", "lines": 10 },
                { "name": "A", "lines": 10 },
                { "name": "C", "lines": 12 },
                { "name": "D", "lines": 1 },
                { "name": "E", "lines": 10 } ] }
            """);

        Assert.Equal(["C", "B", "A", "E", "D"], code.Languages.Select(language => language.Name));
        Assert.Equal([12L, 10L, 10L, 10L, 1L], code.Languages.Select(language => language.Lines));
        Assert.Equal(43L, code.LinesOfCode);
    }

    /// <summary>
    /// A section whose only language is invalid - no name, no lines, or the zero lines - and that says no
    /// <c>linesOfCode</c> and no <c>files</c> of its own reads as no code at all: the size has nothing to show,
    /// and the build that holds only that section is no build.
    /// </summary>
    [Fact]
    public void ASectionOfOnlyInvalidLanguagesWithNoNumbersIsNoCode()
    {
        Assert.Null(BuildInfo.Parse("""{ "code": { "languages": [ { "name": "C#", "lines": 0 } ] } }""")?.Code);
        Assert.Null(BuildInfo.Parse("""{ "code": { "languages": [ { "lines": 61234 } ] } }""")?.Code);
        Assert.Null(BuildInfo.Parse("""{ "code": { "languages": [ { "name": "C#" } ] } }""")?.Code);

        // The section that names no lines, no files and no language of its own is the nothing that is no size.
        Assert.Null(BuildInfo.Parse("""{ "code": { } }""")?.Code);

        // The same section beside one good language is a size of that language alone.
        var beside = Code("""{ "languages": [ { "name": "C#", "lines": 0 }, { "name": "SQL", "lines": 300 } ] }""");

        Assert.Equal([new LanguageSize("SQL", 300, null)], beside.Languages);
    }
}
