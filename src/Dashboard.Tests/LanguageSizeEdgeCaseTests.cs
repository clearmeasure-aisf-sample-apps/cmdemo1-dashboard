namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="LanguageSize" />: the three parts the record
/// keeps of what it is given, the three parts it falls apart into, the words it says them in, the copies that
/// are it again, and the parts that make two records one record or two.
/// <see cref="BuildInfoTests" /> pins what the page makes of the languages a build reports - the five largest
/// named and the rest summed into <c>Other</c>, the share below one said as <c>&lt;1 %</c>, the language with no
/// name, empty or missing, left out by the reader, and the total added from the languages where the file
/// does not say one - and <see cref="HealthTextHelperTests" /> and <see cref="DeployableFactsTests" /> pin the
/// shares and the words of the bar built from them. What they leave open is pinned here, and only that: the
/// record's own three parts at the ends the numbers run to and the zero that is a number, the word that is
/// empty or padded or no word at all, the parts nobody asks the record to make good, and the equality, the hash,
/// the words and the copy that follow them - with the languages of the shipped sample build file named as the
/// records they stand for, the files of each the part the reader carries down to them.
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: every language is one the test builds, or
/// one the test reads at a fixed file, and <see cref="LanguageSize" /> holds no moment and calls nothing.
/// </para>
/// </summary>
public class LanguageSizeEdgeCaseTests
{
    // ----- The three parts the record keeps -----

    /// <summary>
    /// The record keeps the three parts it is given, each as the value it stands on: the word as the word, the
    /// lines as the long they are, and the files as the number it is given - and the files it was not given
    /// stand the nothing they are, kept rather than made a zero.
    /// </summary>
    [Fact]
    public void ARecordKeepsTheThreePartsItIsGiven()
    {
        var language = new LanguageSize("C#", 61234, 800);

        Assert.Equal("C#", language.Name);
        Assert.Equal(61234L, language.Lines);
        Assert.Equal(800, language.Files);

        var noFiles = new LanguageSize("YAML", 476, null);

        Assert.Equal("YAML", noFiles.Name);
        Assert.Equal(476L, noFiles.Lines);
        Assert.Null(noFiles.Files);
    }

    /// <summary>
    /// The two numbers stand where they are put, at the two ends of the number each stands in and at the zero
    /// that is a number: the most lines a language can say and the least it can say, the one line and the none,
    /// and the lines the reader of a build file would never let through - the record is not that reader, and
    /// keeps the number it is given, the sign and all.
    /// </summary>
    [Theory]
    [InlineData(long.MinValue, int.MinValue)]
    [InlineData(-1, -1)]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(long.MaxValue, int.MaxValue)]
    public void TheTwoNumbersStandAtTheEndsTheyRunToAndTheZeroThatIsANumber(long lines, int? files)
    {
        var language = new LanguageSize("C#", lines, files);

        Assert.Equal(lines, language.Lines);
        Assert.Equal(files, language.Files);
    }

    /// <summary>
    /// The word the record is given is the word it keeps, whether it is empty, padded or none at all: the
    /// record trims nothing, drops nothing and asks nothing what it should make of the nothing - a name of
    /// only spaces stands with the spaces about it, the empty word stands the empty word it is and not the
    /// nothing it is not, and the nothing the word was not given stands the nothing it is.
    /// </summary>
    [Fact]
    public void AWordThatIsEmptyOrPaddedOrNothingStandsAsItIsGiven()
    {
        var empty = new LanguageSize(string.Empty, 0, 0);

        Assert.Equal(string.Empty, empty.Name);
        Assert.Equal(0L, empty.Lines);
        Assert.Equal(0, empty.Files);
        Assert.NotEqual(empty, new LanguageSize(string.Empty, 0, null));

        var padded = new LanguageSize("  C#  ", 5, 1);
        Assert.Equal("  C#  ", padded.Name);

        var nameless = new LanguageSize(null!, 3, null);
        Assert.Null(nameless.Name);
        Assert.Equal(3L, nameless.Lines);
        Assert.Null(nameless.Files);
        Assert.Equal(new LanguageSize(null!, 3, null), nameless);
    }

    /// <summary>
    /// The three parts are said once, as the record is made: each may be turned over the value the record was
    /// given for it, and stands where it is turned to - the word to another word, to the empty word and to the
    /// nothing, the lines to the zero, and the files to the nothing the number is not - while the two parts it
    /// does not turn stand as the record gave them, the record turned being another record than the one it
    /// would have stood as, and the two not the one reference.
    /// </summary>
    [Fact]
    public void APartTurnedAsTheRecordIsMadeIsKeptAndMakesItAnotherRecord()
    {
        var stood = new LanguageSize("C#", 61234, 800);

        var renamed = new LanguageSize("C#", 61234, 800) { Name = "Razor" };
        Assert.Equal("Razor", renamed.Name);
        Assert.Equal(61234L, renamed.Lines);
        Assert.Equal(800, renamed.Files);
        Assert.NotEqual(stood, renamed);
        Assert.NotSame(stood, renamed);

        var emptied = new LanguageSize("C#", 61234, 800) { Name = string.Empty };
        Assert.Equal(string.Empty, emptied.Name);

        var nameless = new LanguageSize("C#", 61234, 800) { Name = null! };
        Assert.Null(nameless.Name);

        var noLines = new LanguageSize("C#", 61234, 800) { Lines = 0 };
        Assert.Equal(0L, noLines.Lines);
        Assert.Equal("C#", noLines.Name);
        Assert.Equal(800, noLines.Files);

        var noFiles = new LanguageSize("C#", 61234, 800) { Files = null };
        Assert.Null(noFiles.Files);
        Assert.Equal("C#", noFiles.Name);
        Assert.Equal(61234L, noFiles.Lines);
    }

    // ----- The parts the record falls apart into -----

    /// <summary>
    /// The record falls apart into its three parts, in the order it names them and each as the value it
    /// stands on - the word, the number and the number or the nothing - and the record that says nothing of
    /// itself falls apart into the nothing, the zero and the nothing.
    /// </summary>
    [Fact]
    public void ARecordFallsApartIntoItsThreeParts()
    {
        var (name, lines, files) = new LanguageSize("C#", 61234, 800);

        Assert.Equal("C#", name);
        Assert.Equal(61234L, lines);
        Assert.Equal(800, files);

        var (noName, noLines, noFiles) = new LanguageSize(null!, 0, null);

        Assert.Null(noName);
        Assert.Equal(0L, noLines);
        Assert.Null(noFiles);

        var (emptyName, oneLine, noFileOfItsOwn) = new LanguageSize(string.Empty, 1, null);

        Assert.Equal(string.Empty, emptyName);
        Assert.Equal(1L, oneLine);
        Assert.Null(noFileOfItsOwn);
    }

    // ----- The words the record says its parts in -----

    /// <summary>
    /// The words the record says its three parts in: its own name at the front, the parts in the order they
    /// are named, each as the value it stands on - the lines said as the plain number they are, at the ends of
    /// the number and with no mark between the digits - and the nothing and the empty word said the same way,
    /// as the empty word: the two records that differ only in the name they do not carry are said as one and
    /// are not one.
    /// </summary>
    [Fact]
    public void ARecordSaysItsPartsInTheWordsTheRecordSaysThem()
    {
        Assert.Equal("LanguageSize { Name = C#, Lines = 61234, Files = 800 }", new LanguageSize("C#", 61234, 800).ToString());
        Assert.Equal("LanguageSize { Name = C#, Lines = 0, Files =  }", new LanguageSize("C#", 0, null).ToString());
        Assert.Equal("LanguageSize { Name = , Lines = 9223372036854775807, Files = 2147483647 }", new LanguageSize(string.Empty, long.MaxValue, int.MaxValue).ToString());
        Assert.Equal("LanguageSize { Name = C#, Lines = -1, Files = 0 }", new LanguageSize("C#", -1, 0).ToString());

        var nameless = new LanguageSize(null!, 3, null);
        var emptyNamed = new LanguageSize(string.Empty, 3, null);

        Assert.Equal(nameless.ToString(), emptyNamed.ToString());
        Assert.NotEqual(nameless, emptyNamed);
    }

    // ----- The copies -----

    /// <summary>
    /// The copy that turns nothing is the record again though not the record itself, and the copy that turns
    /// one of the three parts keeps the two it does not name: the word turned alone, the lines turned alone,
    /// the files turned to the nothing and the files turned from the nothing - each a copy of the rest.
    /// </summary>
    [Fact]
    public void ACopyThatTurnsOnePartKeepsTheOtherTwo()
    {
        var one = new LanguageSize("C#", 61234, 800);

        var copy = one with { };
        Assert.Equal(one, copy);
        Assert.NotSame(one, copy);

        var renamed = one with { Name = "Razor" };
        Assert.Equal("Razor", renamed.Name);
        Assert.Equal(one.Lines, renamed.Lines);
        Assert.Equal(one.Files, renamed.Files);
        Assert.NotEqual(one, renamed);

        var moreLines = one with { Lines = 61235 };
        Assert.Equal(61235L, moreLines.Lines);
        Assert.Equal("C#", moreLines.Name);
        Assert.Equal(800, moreLines.Files);

        var noFiles = one with { Files = null };
        Assert.Null(noFiles.Files);
        Assert.Equal(one.Name, noFiles.Name);
        Assert.Equal(one.Lines, noFiles.Lines);
        Assert.NotEqual(one, noFiles);

        var someFiles = new LanguageSize("YAML", 476, null) with { Files = 12 };
        Assert.Equal(12, someFiles.Files);
        Assert.Equal("YAML", someFiles.Name);
        Assert.Equal(476L, someFiles.Lines);
    }

    // ----- The records that are one -----

    /// <summary>
    /// Two records are one while their three parts are, each part as the value it stands on - the nothing
    /// among them included - and the two are not the one reference they are one with: the equality of the
    /// record, of the object and of the operators, the hash that follows them, and the set that keeps one
    /// entry for the records that are one.
    /// </summary>
    [Fact]
    public void TwoRecordsAreOneWhileTheirThreePartsAre()
    {
        var one = new LanguageSize("C#", 61234, 800);
        var two = new LanguageSize("C#", 61234, 800);

        Assert.Equal(one, two);
        Assert.True(one == two);
        Assert.False(one != two);
        Assert.True(one.Equals(two as object));
        Assert.Equal(one.GetHashCode(), two.GetHashCode());
        Assert.NotSame(one, two);
        Assert.True(one.Equals(one));

        var nameless = new LanguageSize(null!, 0, null);
        var alsoNameless = new LanguageSize(null!, 0, null);
        Assert.Equal(nameless, alsoNameless);
        Assert.Equal(nameless.GetHashCode(), alsoNameless.GetHashCode());

        Assert.Single(new HashSet<LanguageSize> { one, two, new("C#", 61234, 800) });
        Assert.Equal(2, new HashSet<LanguageSize> { one, new("Razor", 61234, 800) }.Count);
    }

    /// <summary>
    /// The nothing on either side of the operators: two records that are no record at all are one, a record
    /// and no record are two - the equality and the inequality each saying it - and the record asked of as an
    /// object says no to the nothing and no to a word it is not.
    /// </summary>
    [Fact]
    public void NoRecordIsOneWithNoRecordAndNotWithARecord()
    {
        LanguageSize? nothing = null;
        LanguageSize? alsoNothing = null;
        LanguageSize? one = new LanguageSize("C#", 1, null);

        Assert.True(nothing == alsoNothing);
        Assert.False(nothing != alsoNothing);
        Assert.True(nothing != one);
        Assert.False(nothing == one);
        Assert.False(one == nothing);
        Assert.True(one != nothing);

        Assert.False(one!.Equals(null));
        Assert.False(one!.Equals("C#"));
        Assert.False(one!.Equals((object)476));
    }

    // ----- The records that are not one -----

    /// <summary>
    /// A difference in any one of the three parts is another record: the word turned about in case, cut short
    /// of its spaces, made the empty word or made no word; the lines turned by one, turned to none, turned
    /// about in sign; the files turned by one, made the nothing the zero is not and the zero the nothing is
    /// not - each another record, the operators saying it and the hash following.</summary>
    [Fact]
    public void APartTurnedIsAnotherRecord()
    {
        var one = new LanguageSize("C#", 100, 2);

        Assert.NotEqual(one, new LanguageSize("c#", 100, 2));
        Assert.NotEqual(one, new LanguageSize("C# ", 100, 2));
        Assert.NotEqual(one, new LanguageSize(string.Empty, 100, 2));
        Assert.NotEqual(one, new LanguageSize(null!, 100, 2));
        Assert.NotEqual(one, new LanguageSize("C#", 101, 2));
        Assert.NotEqual(one, new LanguageSize("C#", 0, 2));
        Assert.NotEqual(one, new LanguageSize("C#", -100, 2));
        Assert.NotEqual(one, new LanguageSize("C#", 100, 3));
        Assert.NotEqual(one, new LanguageSize("C#", 100, 0));
        Assert.NotEqual(one, new LanguageSize("C#", 100, null));

        Assert.True(one != new LanguageSize("C#", 100, null));
        Assert.False(one == new LanguageSize("C#", 100, null));
        Assert.Equal(3, new HashSet<LanguageSize> { one, one with { Files = null }, one with { Files = 0 } }.Count);
    }

    // ----- The languages a build file really reports -----

    /// <summary>
    /// The seven languages of the sample build file the fixture holds, as the very records they stand for: the
    /// name, the lines and the files each language is reported with, in the order the largest first - the
    /// files the reader carries down to the record, and every part a fixed value of the file rather than a
    /// number this test made up.
    /// </summary>
    [Fact]
    public void TheLanguagesOfTheSampleBuildAreTheRecordsTheyStandFor()
    {
        var languages = BuildInfo.Parse(Optics.Build)!.Code!.Languages;

        Assert.Equal(
        [
            new LanguageSize("C#", 61234, 800),
            new LanguageSize("Razor", 9000, 120),
            new LanguageSize("SQL", 5000, 60),
            new LanguageSize("JavaScript", 4000, 20),
            new LanguageSize("CSS", 3000, 10),
            new LanguageSize("PowerShell", 1500, 30),
            new LanguageSize("YAML", 476, 12),
        ],
        languages);
    }

    /// <summary>
    /// The files a build file does not say for a language are the nothing the record keeps, and the zero it
    /// says for a language that holds no file of its own is the number it is - kept as the number, not the
    /// nothing it is not - beside the language whose files stand at the top of the number they are counted in.
    /// </summary>
    [Fact]
    public void ALanguageOfTheFileWithoutFilesOrAtTheTopOfTheirNumberIsTheRecordItSays()
    {
        var code = BuildInfo.Parse("""
            { "code": { "languages": [
                { "name": "C#", "lines": 100 },
                { "name": "Other", "lines": 50, "files": 0 },
                { "name": "YAML", "lines": 25, "files": 2147483647 } ] } }
            """)!.Code!;

        Assert.Equal(
        [
            new LanguageSize("C#", 100, null),
            new LanguageSize("Other", 50, 0),
            new LanguageSize("YAML", 25, int.MaxValue),
        ],
        code.Languages);

        Assert.Null(code.Languages[0].Files);
        Assert.Equal(0, code.Languages[1].Files);
        Assert.Equal(int.MaxValue, code.Languages[2].Files);
    }
}
