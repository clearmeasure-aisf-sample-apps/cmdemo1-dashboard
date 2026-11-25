namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="RuntimeNodeKind" />, the nine kinds of box of an
/// environment's runtime diagram, declared in <c>src/Dashboard/Runtime/RuntimeManifest.cs</c>.
/// <para>
/// What the other files already pin is left alone. <see cref="RuntimeNodeEdgeCaseTests" /> pins the nine names in
/// their declared order, the numbers 0 to 8, the count, the <see cref="int" /> underneath, that the unset kind is
/// <see cref="RuntimeNodeKind.Other" />, that 9 is no kind, and that a box holds a number outside the nine as it is
/// handed. <see cref="RuntimeManifestParserEdgeCaseTests" /> pins the word a manifest spells for each kind and that
/// any other word is <c>Other</c>.
/// </para>
/// <para>
/// What follows is what those leave open: that the kinds are a choice and not a set of flags, each kind on its own
/// number and its own name both ways round, a value outside the nine having no name and saying its number, the kinds
/// apart and sorted by number, two kinds combined as numbers, and text that is or is not a kind when read by
/// <see cref="Enum" /> itself (which, unlike the manifest reader, is strict about case).
/// </para>
/// <para>
/// Nothing here reaches the network or asks a clock what time it is.
/// </para>
/// </summary>
public class RuntimeNodeKindEdgeCaseTests
{
    // ----- The nine kinds, as the numbers and the names they are -----

    /// <summary>A kind is one choice: the enum is public and not marked <see cref="FlagsAttribute" />.</summary>
    [Fact]
    public void TheNodeKindsAreAChoiceAndNotASetOfFlags()
    {
        Assert.False(typeof(RuntimeNodeKind).IsDefined(typeof(FlagsAttribute), false));
        Assert.True(typeof(RuntimeNodeKind).IsPublic);
        Assert.True(typeof(RuntimeNodeKind).IsEnum);
    }

    /// <summary>Each kind on its own number and its own name, both ways round.</summary>
    [Theory]
    [InlineData(RuntimeNodeKind.Other, 0, "Other")]
    [InlineData(RuntimeNodeKind.Person, 1, "Person")]
    [InlineData(RuntimeNodeKind.FrontDoor, 2, "FrontDoor")]
    [InlineData(RuntimeNodeKind.WebApp, 3, "WebApp")]
    [InlineData(RuntimeNodeKind.Sql, 4, "Sql")]
    [InlineData(RuntimeNodeKind.StaticSite, 5, "StaticSite")]
    [InlineData(RuntimeNodeKind.Dependency, 6, "Dependency")]
    [InlineData(RuntimeNodeKind.ContainerApp, 7, "ContainerApp")]
    [InlineData(RuntimeNodeKind.Resource, 8, "Resource")]
    public void EachNodeKindStandsOnItsOwnNumberAndItsOwnName(RuntimeNodeKind kind, int number, string name)
    {
        Assert.Equal(number, (int)kind);
        Assert.Equal(kind, (RuntimeNodeKind)number);
        Assert.Equal(name, kind.ToString());
        Assert.Equal(name, Enum.GetName(kind));
        Assert.True(Enum.IsDefined(kind));
        Assert.Equal(kind, Enum.Parse<RuntimeNodeKind>(name));
    }

    /// <summary>One below the first, one above the last and either end of <see cref="int" /> have no name and say
    /// their number.</summary>
    [Fact]
    public void AValueOutsideTheNineHasNoNameAndSaysItsNumber()
    {
        Assert.Null(Enum.GetName((RuntimeNodeKind)(-1)));
        Assert.Null(Enum.GetName((RuntimeNodeKind)9));
        Assert.Null(Enum.GetName((RuntimeNodeKind)int.MinValue));
        Assert.Null(Enum.GetName((RuntimeNodeKind)int.MaxValue));

        Assert.False(Enum.IsDefined((RuntimeNodeKind)(-1)));
        Assert.Equal("-1", ((RuntimeNodeKind)(-1)).ToString());
        Assert.Equal("9", ((RuntimeNodeKind)9).ToString());
        Assert.Equal("-2147483648", ((RuntimeNodeKind)int.MinValue).ToString());
        Assert.Equal("2147483647", ((RuntimeNodeKind)int.MaxValue).ToString());
    }

    /// <summary>The nine kinds are apart from one another and sort by their numbers, as the declaration orders them;
    /// a boxed <see cref="int" /> on the same number is no kind's equal.</summary>
    [Fact]
    public void TheNodeKindsAreApartFromOneAnotherAndSortByTheirNumber()
    {
        var kinds = Enum.GetValues<RuntimeNodeKind>();

        Assert.Equal(kinds.Length, kinds.Distinct().Count());
        Assert.Equal(kinds, kinds.Order());
        Assert.Equal(RuntimeNodeKind.Other, kinds.Min());
        Assert.Equal(RuntimeNodeKind.Resource, kinds.Max());
        Assert.True(RuntimeNodeKind.Other < RuntimeNodeKind.Person);
        Assert.True(RuntimeNodeKind.ContainerApp < RuntimeNodeKind.Resource);
        Assert.Equal(1, RuntimeNodeKind.Resource.CompareTo(RuntimeNodeKind.Other));
        Assert.Equal(-1, RuntimeNodeKind.Other.CompareTo(RuntimeNodeKind.Resource));
        Assert.Equal(0, RuntimeNodeKind.Sql.CompareTo((object)RuntimeNodeKind.Sql));

        Assert.False(RuntimeNodeKind.Sql.Equals(null));
        Assert.False(RuntimeNodeKind.Sql.Equals((object)4));
        Assert.Equal(RuntimeNodeKind.Sql.GetHashCode(), ((object)RuntimeNodeKind.Sql).GetHashCode());
    }

    /// <summary>The operators work on the numbers underneath, so two kinds can combine into a third - or, past
    /// <c>Resource</c> or below <c>Other</c>, into no kind at all.</summary>
    [Fact]
    public void TwoNodeKindsCombinedAreAnotherKindOrNoKindAtAll()
    {
        Assert.Equal(RuntimeNodeKind.WebApp, RuntimeNodeKind.Person | RuntimeNodeKind.FrontDoor);
        Assert.Equal(RuntimeNodeKind.ContainerApp, RuntimeNodeKind.WebApp | RuntimeNodeKind.Sql);
        Assert.Equal(RuntimeNodeKind.Sql, RuntimeNodeKind.Sql | RuntimeNodeKind.Other);
        Assert.Equal(RuntimeNodeKind.Other, RuntimeNodeKind.Resource & RuntimeNodeKind.ContainerApp);
        Assert.Equal(RuntimeNodeKind.Sql, RuntimeNodeKind.StaticSite & RuntimeNodeKind.Dependency);

        Assert.Equal((RuntimeNodeKind)9, RuntimeNodeKind.Resource + 1);
        Assert.False(Enum.IsDefined(RuntimeNodeKind.Resource + 1));
        Assert.Equal((RuntimeNodeKind)(-1), RuntimeNodeKind.Other - 1);
        Assert.False(Enum.IsDefined(RuntimeNodeKind.Other - 1));
    }

    // ----- Text that is or is not a kind -----

    /// <summary>Text that names a kind in the enum's case, surrounding spaces walked past, or holds its number is
    /// that kind.</summary>
    [Theory]
    [InlineData("Other", RuntimeNodeKind.Other)]
    [InlineData("Person", RuntimeNodeKind.Person)]
    [InlineData("FrontDoor", RuntimeNodeKind.FrontDoor)]
    [InlineData("WebApp", RuntimeNodeKind.WebApp)]
    [InlineData("Sql", RuntimeNodeKind.Sql)]
    [InlineData("StaticSite", RuntimeNodeKind.StaticSite)]
    [InlineData("Dependency", RuntimeNodeKind.Dependency)]
    [InlineData("ContainerApp", RuntimeNodeKind.ContainerApp)]
    [InlineData("Resource", RuntimeNodeKind.Resource)]
    [InlineData("  WebApp  ", RuntimeNodeKind.WebApp)]
    [InlineData("0", RuntimeNodeKind.Other)]
    [InlineData("8", RuntimeNodeKind.Resource)]
    [InlineData("07", RuntimeNodeKind.ContainerApp)]
    public void ANodeKindTextIsTheKindItNamesOrTheKindItNumbers(string text, RuntimeNodeKind expected)
    {
        Assert.True(Enum.TryParse<RuntimeNodeKind>(text, out var kind));
        Assert.Equal(expected, kind);
        Assert.True(Enum.IsDefined(kind));
        Assert.Equal(expected, Enum.Parse<RuntimeNodeKind>(text));
    }

    /// <summary>Text that is no kind is refused - null, empty, blank, another case (the words a manifest spells, which
    /// only the manifest reader forgives), a near-word, two words, a fraction, a hexadecimal - and a refused parse
    /// leaves <c>Other</c> behind.</summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData("webapp")]
    [InlineData("containerapp")]
    [InlineData("frontdoor")]
    [InlineData("SQL")]
    [InlineData("Front Door")]
    [InlineData("WebApps")]
    [InlineData("Resource!")]
    [InlineData("WebApp Sql")]
    [InlineData("3.0")]
    [InlineData("0x3")]
    [InlineData("Public")]
    public void TextThatIsNoNodeKindIsRefusedAndLeavesOtherBehind(string? text)
    {
        Assert.False(Enum.TryParse<RuntimeNodeKind>(text, out var kind));
        Assert.Equal(RuntimeNodeKind.Other, kind);
        Assert.False(Enum.TryParse<RuntimeNodeKind>(text, ignoreCase: false, out _));
    }

    /// <summary><c>Enum.Parse</c> says what is wrong with text that is no kind: null is no text at all, and the empty
    /// string, a blank and an unknown word are text without a kind in it.</summary>
    [Fact]
    public void ParsingTextThatIsNoNodeKindThrows()
    {
        Assert.Throws<ArgumentNullException>(() => Enum.Parse<RuntimeNodeKind>(null!));
        Assert.Throws<ArgumentException>(() => Enum.Parse<RuntimeNodeKind>(string.Empty));
        Assert.Throws<ArgumentException>(() => Enum.Parse<RuntimeNodeKind>("   "));
        Assert.Throws<ArgumentException>(() => Enum.Parse<RuntimeNodeKind>("keyvault"));
    }

    /// <summary>A number outside the nine is read as a kind that is no kind, and a name in another case is a kind
    /// only once the parse is told to forgive the case.</summary>
    [Fact]
    public void ANumberOutsideTheNineIsReadAsAKindThatIsNoKind()
    {
        Assert.True(Enum.TryParse<RuntimeNodeKind>("9", out var above));
        Assert.Equal((RuntimeNodeKind)9, above);
        Assert.False(Enum.IsDefined(above));

        Assert.True(Enum.TryParse<RuntimeNodeKind>("-1", out var below));
        Assert.Equal((RuntimeNodeKind)(-1), below);
        Assert.False(Enum.IsDefined(below));

        Assert.False(Enum.TryParse<RuntimeNodeKind>("containerapp", out _));
        Assert.True(Enum.TryParse<RuntimeNodeKind>("containerapp", ignoreCase: true, out var forgiven));
        Assert.Equal(RuntimeNodeKind.ContainerApp, forgiven);
    }
}
