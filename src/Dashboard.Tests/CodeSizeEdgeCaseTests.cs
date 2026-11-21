namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="CodeSize" />: the two numbers the size stands
/// on, at the ends the numbers run to and at the zero that is a number, and the nothing where no number is;
/// the languages the size was given, kept as the very list it was given in the order it stood on, the single
/// language that stands alone, and the list that names no language; two sizes are one while their three parts
/// are, a difference in any of them another one, the languages compared as the list they stand in - the
/// other list that lists the same languages another size; the copy that is the size again, the parts it
/// keeps and the part it turns; and the words the size says of its parts, in the order the record declares
/// them.
/// <see cref="BuildInfoTests" /> pins the size the sample build's answer reads into - the two numbers it
/// stands on and the seven languages it names - and the sizes the card's words are said for: the files without
/// the lines, the lines without the files, and the languages whose lines are added into the total;
/// <see cref="HealthTextHelperTests" /> pins the shares the bar takes from a size's languages. What they leave
/// open is pinned here: the numbers at the ends they run to and the nothing they are not, the list the size
/// was given as the very list it is and in the order it was given, the one language and the none, the equality
/// and the copy the sample's size was never said to have, and the words.
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: every size is one the test builds.
/// </para>
/// </summary>
public class CodeSizeEdgeCaseTests
{
    // ----- LinesOfCode: the number and the nothing the number is not -----

    /// <summary>The lines stand where they were given, without being added to, taken from or made nothing of:
    /// the nothing where they are not, the zero that is a number, the one that is the first real number, and
    /// the long number at the two ends it runs to.</summary>
    [Theory]
    [InlineData(null)]
    [InlineData(0L)]
    [InlineData(1L)]
    [InlineData(long.MaxValue)]
    [InlineData(long.MinValue)]
    public void TheLinesOfCodeStandWhereTheyWereGiven(long? lines)
    {
        var code = new CodeSize(lines, 1203, []);

        Assert.Equal(lines, code.LinesOfCode);
    }

    // ----- Files: the number and the nothing the number is not -----

    /// <summary>The files stand where they were given, without being added to, taken from or made nothing of:
    /// the nothing where they are not, the zero that is a number, the one that is the first real number, and
    /// the int at the two ends it runs to.</summary>
    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(int.MaxValue)]
    [InlineData(int.MinValue)]
    public void TheFilesStandWhereTheyWereGiven(int? files)
    {
        var code = new CodeSize(84210, files, []);

        Assert.Equal(files, code.Files);
    }

    // ----- Languages: the list, its order, the one and the none -----

    /// <summary>The languages are the very list the size was given, not a copy of it: the list the size stands
    /// on is the same reference the size was built with, and the size's words for it are the words of the list
    /// it is.</summary>
    [Fact]
    public void TheLanguagesAreTheVeryListTheSizeWasGiven()
    {
        IReadOnlyList<LanguageSize> languages = [new LanguageSize("C#", 61234, 800), new LanguageSize("Razor", 9000, 120)];

        var code = new CodeSize(84210, 1203, languages);

        Assert.Same(languages, code.Languages);
        Assert.Equal(2, code.Languages.Count);
    }

    /// <summary>The languages stand in the order the size was given them - not the order the build measured
    /// them in, which the size does not re-sort: the list it was built with in any order is the order it
    /// stands on, the language without a number of files included as the nothing it is.</summary>
    [Fact]
    public void TheLanguagesStandInTheOrderTheSizeWasGivenThem()
    {
        var code = new CodeSize(15000, 150, [new LanguageSize("Razor", 9000, 120), new LanguageSize("C#", 6000, null)]);

        Assert.Equal("Razor", code.Languages[0].Name);
        Assert.Equal(9000, code.Languages[0].Lines);
        Assert.Equal(120, code.Languages[0].Files);
        Assert.Equal("C#", code.Languages[1].Name);
        Assert.Equal(6000, code.Languages[1].Lines);
        Assert.Null(code.Languages[1].Files);
    }

    /// <summary>The size with one language names the one language it was given, whole and as itself, and the
    /// size whose list names no language lists no language: the empty list the size was given stands the
    /// empty list it is.</summary>
    [Fact]
    public void ASingleLanguageStandsAloneAndANoLanguageListNamesNoLanguage()
    {
        var one = new CodeSize(61234, 800, [new LanguageSize("C#", 61234, 800)]);
        Assert.Equal(new LanguageSize("C#", 61234, 800), Assert.Single(one.Languages));

        var none = new CodeSize(null, null, []);
        Assert.Empty(none.Languages);
    }

    // ----- Two sizes are one while their three parts are -----

    /// <summary>Two sizes are one while their three parts are - the numbers the same and the languages the
    /// same list - and a difference in any of the parts, the number turned and the number made nothing
    /// included, is another one: the equality of the operators and of <c>Equals</c>, and the hash code that
    /// follows them. The sample's size the other tests pin says its numbers and its languages, but the
    /// equality it has with its own is said here.</summary>
    [Fact]
    public void TwoSizesAreOneWhileTheirPartsAre()
    {
        IReadOnlyList<LanguageSize> languages = [new LanguageSize("C#", 61234, 800), new LanguageSize("Razor", 9000, 120)];
        var one = new CodeSize(84210, 1203, languages);
        var two = new CodeSize(84210, 1203, languages);

        Assert.Equal(one, two);
        Assert.True(one == two);
        Assert.False(one != two);
        Assert.True(one.Equals((object)two));
        Assert.True(two.Equals((object)one));
        Assert.Equal(one.GetHashCode(), two.GetHashCode());
        Assert.False(one.Equals(null));

        Assert.NotEqual(one, new CodeSize(84211, 1203, languages));
        Assert.NotEqual(one, new CodeSize(null, 1203, languages));
        Assert.NotEqual(one, new CodeSize(84210, 1204, languages));
        Assert.NotEqual(one, new CodeSize(84210, null, languages));
    }

    /// <summary>The languages are compared as the list they stand in, not language by language: the size on
    /// the same list is the size again, and the other list that lists the same languages - the one turned,
    /// the one grown, the one that lists no language - is another size, each as the list it is.</summary>
    [Fact]
    public void TheLanguagesAreComparedAsTheListTheyStandIn()
    {
        var one = new CodeSize(84210, 1203, [new LanguageSize("C#", 61234, 800), new LanguageSize("Razor", 9000, 120)]);
        var sameList = new CodeSize(84210, 1203, one.Languages);

        Assert.Equal(one, sameList);
        Assert.NotEqual(one, new CodeSize(84210, 1203, [new LanguageSize("C#", 61234, 800), new LanguageSize("Razor", 9000, 120)]));
        Assert.NotEqual(one, new CodeSize(84210, 1203, [new LanguageSize("Razor", 9000, 120), new LanguageSize("C#", 61234, 800)]));
        Assert.NotEqual(one, new CodeSize(84210, 1203, [new LanguageSize("C#", 61234, 800), new LanguageSize("Razor", 9000, 120), new LanguageSize("YAML", 476, 12)]));
        Assert.NotEqual(one, new CodeSize(84210, 1203, []));
        Assert.NotEqual(one, one with { Languages = [new LanguageSize("C#", 61234, 800), new LanguageSize("Razor", 9000, 120)] });
    }

    // ----- The copy that is the size again -----

    /// <summary>The copy of a size is the size again: the <c>with</c> that names no part keeps all three -
    /// the list the size was given, as the same reference - and not the same reference; the <c>with</c> that
    /// turns one of the parts, the number made nothing and the list made none included, keeps the two it does
    /// not name.</summary>
    [Fact]
    public void ACopyOfASizeIsTheSizeAgainAndTheCopyKeepsThePartsItDoesNotTurn()
    {
        IReadOnlyList<LanguageSize> languages = [new LanguageSize("C#", 61234, 800)];
        var code = new CodeSize(84210, 1203, languages);

        var copy = code with { };
        Assert.Equal(code, copy);
        Assert.NotSame(code, copy);
        Assert.Same(languages, copy.Languages);

        var noLines = code with { LinesOfCode = null };
        Assert.Null(noLines.LinesOfCode);
        Assert.Equal(1203, noLines.Files);
        Assert.Same(languages, noLines.Languages);

        var noFiles = code with { Files = null };
        Assert.Equal(84210, noFiles.LinesOfCode);
        Assert.Null(noFiles.Files);
        Assert.Same(languages, noFiles.Languages);

        var noLanguages = code with { Languages = [] };
        Assert.Equal(84210, noLanguages.LinesOfCode);
        Assert.Equal(1203, noLanguages.Files);
        Assert.Empty(noLanguages.Languages);

        Assert.Equal(code, code with { LinesOfCode = code.LinesOfCode, Files = code.Files, Languages = code.Languages });
    }

    // ----- The size's own words -----

    /// <summary>The size says its three parts in the order the record declares them - the lines first, the
    /// files after, the languages last - each by the word the type says, the nothing the part is said as the
    /// nothing it is, and the words end where the record's words end.</summary>
    [Fact]
    public void TheSizeSaysItsPartsInTheOrderTheRecordDeclaresThem()
    {
        var code = new CodeSize(84210, 1203, [new LanguageSize("C#", 61234, 800)]);
        var words = code.ToString();

        Assert.StartsWith("CodeSize { LinesOfCode = 84210, Files = 1203, Languages = ", words, StringComparison.Ordinal);
        Assert.EndsWith(" }", words, StringComparison.Ordinal);

        Assert.StartsWith(
            "CodeSize { LinesOfCode = , Files = , Languages = ",
            new CodeSize(null, null, []).ToString(),
            StringComparison.Ordinal);
    }
}
