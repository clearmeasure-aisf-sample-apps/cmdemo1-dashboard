namespace Dashboard.Tests;

/// <summary>
/// <see cref="CodeSize" /> is what the build measured about the size of its code: the lines of code, the files,
/// and the languages with their lines, largest first. <see cref="BuildInfoTests" /> pins the two numbers and the
/// names of the seven languages read from the sample answer and the words the card says of them;
/// <see cref="BuildInfoEdgeCaseTests" /> pins the record the constructor is built with - the three parts kept as
/// the values and the instance they stand on, the two numbers at the ends they run to, the empty list and the
/// absent list, the falling apart, the words, the copies that turn the two numbers, and the equality that needs
/// the very list the size holds; <see cref="BuildInfoCodeRoundingEdgeCaseTests" /> pins the numbers the reader of
/// the answer makes of what the answer says; <see cref="LanguageSizeEdgeCaseTests" /> pins the languages the size
/// is made of, and <see cref="BuildTextEdgeCaseTests" /> the words the card says of the size.
/// <para>
/// What is pinned here is what those leave open, and only that: the size of the sample build and of the
/// dashboard's own build as the one record they stand for; the list the record is given kept as the instance it
/// stands on, in the kind the caller brings it, down to the list of one language; the copy that turns the list
/// and keeps the two numbers, and the absent list kept absent through the copy; the size that falls apart into
/// the absent list; the nothing on either side of the operators; and the equality asked of the object, the hash
/// that stands from call to call, and the set the two ask of.
/// </para>
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: every size is one the test builds, or one
/// the fixed answer says.
/// </para>
/// </summary>
public class CodeSizeEdgeCaseTests
{
    // ----- the fixed values the answers stand for -----

    /// <summary>
    /// The <c>code</c> section of the sample answer is one size: the lines and the files the answer says, and the
    /// seven languages it names, each with the lines and the files the answer carries down to it, largest first -
    /// the record as the record, once the list it holds is the one the read brought, the rule the equality needs.
    /// </summary>
    [Fact]
    public void TheCodeOfTheSampleBuildIsTheSizeItStandsFor()
    {
        var code = BuildInfo.Parse(Optics.Build)!.Code!;

        var expected = new CodeSize(
            84_210L,
            1_203,
            [
                new LanguageSize("C#", 61_234, 800),
                new LanguageSize("Razor", 9_000, 120),
                new LanguageSize("SQL", 5_000, 60),
                new LanguageSize("JavaScript", 4_000, 20),
                new LanguageSize("CSS", 3_000, 10),
                new LanguageSize("PowerShell", 1_500, 30),
                new LanguageSize("YAML", 476, 12),
            ]);

        Assert.Equal(expected with { Languages = code.Languages }, code);
        Assert.Equal(84_210L, code.LinesOfCode);
        Assert.Equal(1_203, code.Files);
    }

    /// <summary>
    /// The dashboard's own build, as its facts say it: the lines and the files it measured, and the two languages
    /// it names with their lines and their files - the size the card's words are said of, as the record it stands
    /// for.
    /// </summary>
    [Fact]
    public void TheCodeOfADashboardBuildIsTheSizeItsFactsStandFor()
    {
        var code = BuildInfo.Parse(
            """
            { "code": { "linesOfCode": 19200, "files": 133, "languages": [
                { "name": "C#", "lines": 12600, "files": 87 }, { "name": "Razor", "lines": 2733, "files": 33 } ] } }
            """)!.Code!;

        var expected = new CodeSize(
            19_200L,
            133,
            [
                new LanguageSize("C#", 12_600, 87),
                new LanguageSize("Razor", 2_733, 33),
            ]);

        Assert.Equal(expected with { Languages = code.Languages }, code);
    }

    // ----- the list the record holds -----

    /// <summary>
    /// The record keeps the list it is given as the very instance it stands on, in the kind the caller brings it:
    /// the <see cref="List{T}" /> beside the array the answer's read brings, never copied and never made another
    /// kind, down to the list of one language - the edge between the empty list and the list that names - with
    /// the two numbers it was given standing as the values they are, or as the nothing they were not.
    /// </summary>
    [Fact]
    public void TheRecordKeepsTheListItIsGivenAsTheInstanceItStandsOn()
    {
        var asList = new List<LanguageSize>
        {
            new("C#", 61_234, 800),
            new("Razor", 9_000, 120),
        };
        var size = new CodeSize(84_210L, 1_203, asList);

        Assert.Same(asList, size.Languages);
        Assert.Equal(84_210L, size.LinesOfCode);
        Assert.Equal(1_203, size.Files);
        Assert.Equal(2, size.Languages.Count);

        var asArray = new LanguageSize[1] { new("YAML", 476, 12) };
        var ofOne = new CodeSize(null, null, asArray);

        Assert.Same(asArray, ofOne.Languages);
        Assert.Null(ofOne.LinesOfCode);
        Assert.Null(ofOne.Files);
        Assert.Single(ofOne.Languages);
    }

    // ----- the copy and the falling apart -----

    /// <summary>
    /// The copy that turns the list keeps the two numbers it does not name, and the copy that turns nothing on the
    /// size whose list is absent keeps the absent list it was given, not an empty one of its own - the size that
    /// holds the list and the size that holds nothing for it are two sizes, and the copy that turns the absent
    /// list to the empty one is the size of the empty list, the two numbers standing where they stood.
    /// </summary>
    [Fact]
    public void ACopyThatTurnsTheListKeepsTheTwoNumbersAndTheAbsentListStaysAbsent()
    {
        var size = new CodeSize(84_210L, 1_203, [new LanguageSize("C#", 61_234, 800)]);

        var retagged = size with { Languages = [new LanguageSize("Razor", 9_000, 120)] };

        Assert.Equal(84_210L, retagged.LinesOfCode);
        Assert.Equal(1_203, retagged.Files);
        Assert.Single(retagged.Languages);
        Assert.NotEqual(size, retagged);

        var absent = new CodeSize(84_210L, 1_203, null!);

        var kept = absent with { };

        Assert.Null(kept.Languages);
        Assert.Equal(84_210L, kept.LinesOfCode);
        Assert.Equal(1_203, kept.Files);
        Assert.NotEqual(size, kept);

        var emptied = absent with { Languages = [] };

        Assert.Empty(emptied.Languages);
        Assert.Equal(84_210L, emptied.LinesOfCode);
        Assert.Equal(1_203, emptied.Files);
    }

    /// <summary>
    /// The size whose list is absent falls apart into the two numbers and the absent list - the nothing it was
    /// given, not the empty list it is not - beside the size of one language and no numbers, whose list falls
    /// apart as the one it holds.
    /// </summary>
    [Fact]
    public void ASizeWithoutAListFallsApartIntoItsNumbersAndTheAbsentList()
    {
        var (lines, files, languages) = new CodeSize(84_210L, 1_203, null!);

        Assert.Equal(84_210L, lines);
        Assert.Equal(1_203, files);
        Assert.Null(languages);

        var (noLines, noFiles, oneLanguage) = new CodeSize(null, null, [new LanguageSize("C#", 61_234, 800)]);

        Assert.Null(noLines);
        Assert.Null(noFiles);
        Assert.Single(oneLanguage);
    }

    // ----- the nothing on either side of the operators -----

    /// <summary>
    /// The nothing on either side of the operators: two sizes that are no size at all are one, a size and no size
    /// are two however the question is asked - the operators, and <see cref="object.Equals" /> asked of the no
    /// size and of a word or a number the size is not.
    /// </summary>
    [Fact]
    public void NoSizeIsOneWithNoSizeAndNotWithASize()
    {
        CodeSize? nothing = null;
        CodeSize? alsoNothing = null;
        var one = new CodeSize(84_210L, 1_203, [new LanguageSize("C#", 61_234, 800)]);

        Assert.True(nothing == alsoNothing);
        Assert.False(nothing != alsoNothing);
        Assert.True(nothing != one);
        Assert.False(nothing == one);
        Assert.False(one == nothing);
        Assert.True(one != nothing);

        Assert.False(one.Equals(null));
        Assert.False(one.Equals("CodeSize"));
        Assert.False(one.Equals((object)84_210L));
    }

    // ----- the equality asked of the object and the hash -----

    /// <summary>
    /// The equality asked of the object says yes to the size it is one with and no to the nothing, the hash stands
    /// from call to call on the one size and follows the equality to the other - and the set the two ask of keeps
    /// one entry for the sizes that are one: the same numbers and the same list.
    /// </summary>
    [Fact]
    public void TheEqualityAskedOfTheObjectAndTheHashThatStandsFromCallToCall()
    {
        var one = new CodeSize(84_210L, 1_203, [new LanguageSize("C#", 61_234, 800)]);
        var two = new CodeSize(84_210L, 1_203, one.Languages);

        Assert.True(one.Equals((object)two));
        Assert.True(two.Equals((object)one));
        Assert.False(one.Equals(null));
        Assert.Equal(one.GetHashCode(), one.GetHashCode());
        Assert.Equal(one.GetHashCode(), two.GetHashCode());
        Assert.Single(new HashSet<CodeSize> { one, two });
    }
}
