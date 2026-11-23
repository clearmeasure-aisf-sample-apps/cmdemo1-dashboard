namespace Dashboard.Tests;

/// <summary>
/// The edges of <see cref="BuildInfo.Parse" /> that <see cref="BuildInfoTests" />, <see cref="BuildInfoEdgeCaseTests" />,
/// <see cref="BuildInfoParseBoundaryEdgeCaseTests" /> and <see cref="BuildInfoParseWordsEdgeCaseTests" /> leave open:
/// the rounding of the <c>code</c> section's numbers (a language whose lines round to zero is no language, a half
/// goes to the even neighbour), the files of the size at the very end of their range, a language past the reader's
/// end beside one within it, a language's name trimmed or said as another kind, and the moment at the first and the
/// last a <see cref="DateTimeOffset" /> holds. Every input is a fixed string; nothing reaches the network or reads
/// the clock.
/// </summary>
public class BuildInfoCodeRoundingEdgeCaseTests
{
    private static CodeSize Code(string section) => BuildInfo.Parse($$"""{ "code": {{section}} }""")!.Code!;

    /// <summary>A language's lines are rounded before the reader asks whether there are any: below a half, and the
    /// half itself, round to zero and the language is dropped; above it the language is kept with one line.</summary>
    [Theory]
    [InlineData("0.4", null)]
    [InlineData("0.5", null)]
    [InlineData("0.6", 1L)]
    [InlineData("1.5", 2L)]
    [InlineData("2.5", 2L)]
    public void ALanguagesLinesAreRoundedBeforeTheyAreCounted(string lines, long? expected)
    {
        var code = Code($$"""{ "files": 1, "languages": [ { "name": "YAML", "lines": {{lines}} } ] }""");

        if (expected is null)
        {
            Assert.Empty(code.Languages);
            Assert.Null(code.LinesOfCode);
        }
        else
        {
            Assert.Equal([new LanguageSize("YAML", expected.Value, null)], code.Languages);
            Assert.Equal(expected, code.LinesOfCode);
        }
    }

    /// <summary>The lines the file says are rounded too, a half to the even neighbour.</summary>
    [Theory]
    [InlineData("84210.4", 84_210L)]
    [InlineData("84210.5", 84_210L)]
    [InlineData("84211.5", 84_212L)]
    [InlineData("0.5", 0L)]
    public void TheLinesOfCodeAreRounded(string said, long expected) =>
        Assert.Equal(expected, Code($$"""{ "linesOfCode": {{said}} }""").LinesOfCode);

    /// <summary>A language's files are a count: rounded, a half to the even neighbour.</summary>
    [Fact]
    public void ALanguagesFilesAreRounded()
    {
        var code = Code("""{ "languages": [ { "name": "C#", "lines": 10, "files": 2.5 }, { "name": "SQL", "lines": 5, "files": 3.5 } ] }""");

        Assert.Equal([new LanguageSize("C#", 10, 2), new LanguageSize("SQL", 5, 4)], code.Languages);
    }

    /// <summary>The size's files run to the largest <see cref="int" />: one past it is no count, and a section of
    /// nothing but that is no size.</summary>
    [Fact]
    public void TheFilesOfTheSizeEndAtTheLargestInt()
    {
        Assert.Equal(int.MaxValue, Code("""{ "files": 2147483647 }""").Files);

        var past = BuildInfo.Parse("""{ "version": "2.4.15", "code": { "files": 2147483648 } }""")!;

        Assert.Null(past.Code);
        Assert.Null(BuildInfo.Parse("""{ "code": { "files": 2147483648 } }"""));
    }

    /// <summary>A language whose lines are past the reader's end is dropped, and the lines the size adds up are
    /// those of the languages that remain.</summary>
    [Fact]
    public void ALanguagePastTheReadersEndIsDroppedFromTheSum()
    {
        var code = Code("""{ "languages": [ { "name": "Huge", "lines": 1e30 }, { "name": "C#", "lines": 700 }, { "name": "SQL", "lines": 300 } ] }""");

        Assert.Equal(["C#", "SQL"], code.Languages.Select(language => language.Name));
        Assert.Equal(1_000L, code.LinesOfCode);
    }

    /// <summary>A language's name is trimmed; a name said as a number, a flag, a list or an object is no name, and
    /// a language of zero lines among others is dropped while the rest are kept.</summary>
    [Fact]
    public void ALanguagesNameIsTrimmedAndANameOfAnotherKindIsNoLanguage()
    {
        var code = Code("""
            { "languages": [
                { "name": "  C#\t", "lines": 700 },
                { "name": 5, "lines": 50 },
                { "name": true, "lines": 50 },
                { "name": [ "SQL" ], "lines": 50 },
                { "name": { "text": "SQL" }, "lines": 50 },
                { "name": "Razor", "lines": 0 },
                { "name": "SQL", "lines": 300 } ] }
            """);

        Assert.Equal([new LanguageSize("C#", 700, null), new LanguageSize("SQL", 300, null)], code.Languages);
        Assert.Equal(1_000L, code.LinesOfCode);
        Assert.Null(code.Files);
    }

    /// <summary>The moment runs from the first to the last a <see cref="DateTimeOffset" /> holds, each at the
    /// universal time it says.</summary>
    [Fact]
    public void TheMomentRunsFromTheFirstToTheLastADateTimeOffsetHolds()
    {
        Assert.Equal(DateTimeOffset.MinValue, BuildInfo.Parse("""{ "builtAt": "0001-01-01T00:00:00Z" }""")!.BuiltAt);
        Assert.Equal(DateTimeOffset.MaxValue, BuildInfo.Parse("""{ "builtAt": "9999-12-31T23:59:59.9999999Z" }""")!.BuiltAt);
    }

    /// <summary>A moment said at the widest offset there is keeps that offset and the instant it stands for.</summary>
    [Fact]
    public void TheMomentKeepsTheWidestOffset()
    {
        var east = BuildInfo.Parse("""{ "builtAt": "2026-10-05T10:00:00+14:00" }""")!.BuiltAt!.Value;
        var west = BuildInfo.Parse("""{ "builtAt": "2026-10-04T06:00:00-14:00" }""")!.BuiltAt!.Value;

        Assert.Equal(TimeSpan.FromHours(14), east.Offset);
        Assert.Equal(TimeSpan.FromHours(-14), west.Offset);
        Assert.Equal(new DateTimeOffset(2026, 10, 4, 20, 0, 0, TimeSpan.Zero), east);
        Assert.Equal(new DateTimeOffset(2026, 10, 4, 20, 0, 0, TimeSpan.Zero), west);
    }
}
