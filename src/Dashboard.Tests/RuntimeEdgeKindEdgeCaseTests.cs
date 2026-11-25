namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="RuntimeEdgeKind" />, the six kinds of drawn line of
/// an environment's runtime diagram, declared in <c>src/Dashboard/Runtime/RuntimeManifest.cs</c>.
/// <para>
/// What the other files already pin is left alone. <see cref="RuntimeEdgeEdgeCaseTests" /> pins the six names in
/// their declared order, the numbers 0 to 5, the count, the <see cref="int" /> underneath, that the unset kind is
/// <see cref="RuntimeEdgeKind.Other" />, and that an edge holds a number outside the six as it is handed.
/// <see cref="RuntimeManifestParserEdgeCaseTests" /> and <see cref="RuntimeManifestParserJsonEdgeCaseTests" /> pin the
/// word a manifest spells for each kind, in any case, and that any other word is <c>Other</c>.
/// </para>
/// <para>
/// What follows is what those leave open: that the kinds are a choice and not a set of flags, each kind on its own
/// number and its own name both ways round, a value outside the six having no name and saying its number, the kinds
/// apart and sorted by number, two kinds combined as numbers, and text that is or is not a kind when read by
/// <see cref="Enum" /> itself (which, unlike the manifest reader, is strict about case).
/// </para>
/// <para>
/// Nothing here reaches the network or asks a clock what time it is.
/// </para>
/// </summary>
public class RuntimeEdgeKindEdgeCaseTests
{
    // ----- The six kinds, as the numbers and the names they are -----

    /// <summary>A kind is one choice: the enum is not marked <see cref="FlagsAttribute" />, and its names come back
    /// from <see cref="Enum.GetNames{TEnum}" /> as they do from the values.</summary>
    [Fact]
    public void TheEdgeKindsAreAChoiceAndNotASetOfFlags()
    {
        Assert.False(typeof(RuntimeEdgeKind).IsDefined(typeof(FlagsAttribute), false));
        Assert.Equal(["Other", "Public", "Origin", "Sql", "Dashboard", "Dependency"], Enum.GetNames<RuntimeEdgeKind>());
        Assert.True(typeof(RuntimeEdgeKind).IsPublic);
    }

    /// <summary>Each kind on its own number and its own name, both ways round.</summary>
    [Theory]
    [InlineData(RuntimeEdgeKind.Other, 0, "Other")]
    [InlineData(RuntimeEdgeKind.Public, 1, "Public")]
    [InlineData(RuntimeEdgeKind.Origin, 2, "Origin")]
    [InlineData(RuntimeEdgeKind.Sql, 3, "Sql")]
    [InlineData(RuntimeEdgeKind.Dashboard, 4, "Dashboard")]
    [InlineData(RuntimeEdgeKind.Dependency, 5, "Dependency")]
    public void EachEdgeKindStandsOnItsOwnNumberAndItsOwnName(RuntimeEdgeKind kind, int number, string name)
    {
        Assert.Equal(number, (int)kind);
        Assert.Equal(kind, (RuntimeEdgeKind)number);
        Assert.Equal(name, kind.ToString());
        Assert.Equal(name, Enum.GetName(kind));
        Assert.True(Enum.IsDefined(kind));
        Assert.Equal(kind, Enum.Parse<RuntimeEdgeKind>(name));
    }

    /// <summary>One below the first, one above the last and either end of <see cref="int" /> have no name and say
    /// their number.</summary>
    [Fact]
    public void AValueOutsideTheSixHasNoNameAndSaysItsNumber()
    {
        Assert.Null(Enum.GetName((RuntimeEdgeKind)(-1)));
        Assert.Null(Enum.GetName((RuntimeEdgeKind)6));
        Assert.Null(Enum.GetName((RuntimeEdgeKind)int.MinValue));
        Assert.Null(Enum.GetName((RuntimeEdgeKind)int.MaxValue));

        Assert.Equal("-1", ((RuntimeEdgeKind)(-1)).ToString());
        Assert.Equal("6", ((RuntimeEdgeKind)6).ToString());
        Assert.Equal("-2147483648", ((RuntimeEdgeKind)int.MinValue).ToString());
        Assert.Equal("2147483647", ((RuntimeEdgeKind)int.MaxValue).ToString());
    }

    /// <summary>The six kinds are apart from one another and sort by their numbers, as the declaration orders them;
    /// a boxed <see cref="int" /> on the same number is no kind's equal.</summary>
    [Fact]
    public void TheEdgeKindsAreApartFromOneAnotherAndSortByTheirNumber()
    {
        var kinds = Enum.GetValues<RuntimeEdgeKind>();

        Assert.Equal(kinds.Length, kinds.Distinct().Count());
        Assert.Equal(kinds, kinds.Order());
        Assert.Equal(RuntimeEdgeKind.Other, kinds.Min());
        Assert.Equal(RuntimeEdgeKind.Dependency, kinds.Max());
        Assert.True(RuntimeEdgeKind.Other < RuntimeEdgeKind.Public);
        Assert.True(RuntimeEdgeKind.Dashboard < RuntimeEdgeKind.Dependency);
        Assert.Equal(1, RuntimeEdgeKind.Dependency.CompareTo(RuntimeEdgeKind.Other));
        Assert.Equal(-1, RuntimeEdgeKind.Other.CompareTo(RuntimeEdgeKind.Dependency));
        Assert.Equal(0, RuntimeEdgeKind.Sql.CompareTo((object)RuntimeEdgeKind.Sql));

        Assert.False(RuntimeEdgeKind.Sql.Equals(null));
        Assert.False(RuntimeEdgeKind.Sql.Equals((object)3));
        Assert.Equal(RuntimeEdgeKind.Sql.GetHashCode(), ((object)RuntimeEdgeKind.Sql).GetHashCode());
    }

    /// <summary>The operators work on the numbers underneath, so two kinds can combine into a third - or, past
    /// <c>Dependency</c> or below <c>Other</c>, into no kind at all.</summary>
    [Fact]
    public void TwoEdgeKindsCombinedAreAnotherKindOrNoKindAtAll()
    {
        Assert.Equal(RuntimeEdgeKind.Sql, RuntimeEdgeKind.Public | RuntimeEdgeKind.Origin);
        Assert.Equal(RuntimeEdgeKind.Dependency, RuntimeEdgeKind.Public | RuntimeEdgeKind.Dashboard);
        Assert.Equal(RuntimeEdgeKind.Origin, RuntimeEdgeKind.Origin | RuntimeEdgeKind.Other);
        Assert.Equal(RuntimeEdgeKind.Other, RuntimeEdgeKind.Sql & RuntimeEdgeKind.Dashboard);
        Assert.Equal(RuntimeEdgeKind.Public, RuntimeEdgeKind.Sql & RuntimeEdgeKind.Dependency);

        Assert.Equal((RuntimeEdgeKind)6, RuntimeEdgeKind.Dependency + 1);
        Assert.False(Enum.IsDefined(RuntimeEdgeKind.Dependency + 1));
        Assert.Equal((RuntimeEdgeKind)(-1), RuntimeEdgeKind.Other - 1);
        Assert.False(Enum.IsDefined(RuntimeEdgeKind.Other - 1));
    }

    // ----- Text that is or is not a kind -----

    /// <summary>Text that names a kind in the enum's case, surrounding spaces walked past, or holds its number is
    /// that kind.</summary>
    [Theory]
    [InlineData("Other", RuntimeEdgeKind.Other)]
    [InlineData("Public", RuntimeEdgeKind.Public)]
    [InlineData("Origin", RuntimeEdgeKind.Origin)]
    [InlineData("Sql", RuntimeEdgeKind.Sql)]
    [InlineData("Dashboard", RuntimeEdgeKind.Dashboard)]
    [InlineData("Dependency", RuntimeEdgeKind.Dependency)]
    [InlineData("  Sql  ", RuntimeEdgeKind.Sql)]
    [InlineData("0", RuntimeEdgeKind.Other)]
    [InlineData("5", RuntimeEdgeKind.Dependency)]
    [InlineData("03", RuntimeEdgeKind.Sql)]
    public void AnEdgeKindTextIsTheKindItNamesOrTheKindItNumbers(string text, RuntimeEdgeKind expected)
    {
        Assert.True(Enum.TryParse<RuntimeEdgeKind>(text, out var kind));
        Assert.Equal(expected, kind);
        Assert.True(Enum.IsDefined(kind));
        Assert.Equal(expected, Enum.Parse<RuntimeEdgeKind>(text));
    }

    /// <summary>Text that is no kind is refused - null, empty, blank, another case (the words a manifest spells, which
    /// only the manifest reader forgives), a near-word, a fraction, a hexadecimal - and a refused parse leaves
    /// <c>Other</c> behind.</summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData("sql")]
    [InlineData("SQL")]
    [InlineData("public")]
    [InlineData("origins")]
    [InlineData("Sql!")]
    [InlineData("Public Origin")]
    [InlineData("3.0")]
    [InlineData("0x3")]
    [InlineData("webapp")]
    public void TextThatIsNoEdgeKindIsRefusedAndLeavesOtherBehind(string? text)
    {
        Assert.False(Enum.TryParse<RuntimeEdgeKind>(text, out var kind));
        Assert.Equal(RuntimeEdgeKind.Other, kind);
        Assert.False(Enum.TryParse<RuntimeEdgeKind>(text, ignoreCase: false, out _));
    }

    /// <summary><c>Enum.Parse</c> says what is wrong with text that is no kind: null is no text at all, and the empty
    /// string, a blank and an unknown word are text without a kind in it.</summary>
    [Fact]
    public void ParsingTextThatIsNoEdgeKindThrows()
    {
        Assert.Throws<ArgumentNullException>(() => Enum.Parse<RuntimeEdgeKind>(null!));
        Assert.Throws<ArgumentException>(() => Enum.Parse<RuntimeEdgeKind>(string.Empty));
        Assert.Throws<ArgumentException>(() => Enum.Parse<RuntimeEdgeKind>("   "));
        Assert.Throws<ArgumentException>(() => Enum.Parse<RuntimeEdgeKind>("amqp"));
    }

    /// <summary>A number outside the six is read as a kind that is no kind, and a name in another case is a kind
    /// only once the parse is told to forgive the case.</summary>
    [Fact]
    public void ANumberOutsideTheSixIsReadAsAKindThatIsNoKind()
    {
        Assert.True(Enum.TryParse<RuntimeEdgeKind>("6", out var above));
        Assert.Equal((RuntimeEdgeKind)6, above);
        Assert.False(Enum.IsDefined(above));

        Assert.True(Enum.TryParse<RuntimeEdgeKind>("-1", out var below));
        Assert.Equal((RuntimeEdgeKind)(-1), below);
        Assert.False(Enum.IsDefined(below));

        Assert.False(Enum.TryParse<RuntimeEdgeKind>("dependency", out _));
        Assert.True(Enum.TryParse<RuntimeEdgeKind>("dependency", ignoreCase: true, out var forgiven));
        Assert.Equal(RuntimeEdgeKind.Dependency, forgiven);
    }
}
