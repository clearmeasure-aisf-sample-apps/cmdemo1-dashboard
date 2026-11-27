namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="EventKind" />, the eight words for what kind of thing
/// the page observed, declared in <c>src/Dashboard/Health/DashboardEvents.cs</c>. Which kind each detector hands out
/// is pinned where the detectors are - <see cref="EventDetectorTests" />, <see cref="ClusterEventDetectorTests" />,
/// <see cref="RotationTests" /> and <see cref="KustomizationPinMonitorTests" /> - and what they leave open is pinned
/// here: the eight kinds as the numbers, the names and the order they are declared in, the unset kind, nothing outside
/// the eight, the kinds combined as numbers, and text that is or is not a kind.
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: every moment is one the test brings.
/// </para>
/// </summary>
public class EventKindEdgeCaseTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 19, 12, 0, 0, TimeSpan.Zero);

    // ----- The eight kinds, as the numbers they are -----

    /// <summary>The eight kinds in the order the enum declares them, on the numbers 0 to 7, of an <see cref="int" />
    /// underneath.</summary>
    [Fact]
    public void TheEightEventKindsAreTheWholeVocabularyInTheirDeclaredOrder()
    {
        Assert.Equal(
            ["Health", "HealthCheck", "Restart", "Version", "Serving", "Pinned", "Traffic", "Cluster"],
            EnumVocabulary<EventKind>.Names);
        Assert.Equal([0, 1, 2, 3, 4, 5, 6, 7], EnumVocabulary<EventKind>.Numbers);
        Assert.Equal(8, EnumVocabulary<EventKind>.Count);
        Assert.Equal(typeof(int), EnumVocabulary<EventKind>.UnderlyingType);
        Assert.False(EnumVocabulary<EventKind>.IsFlags);
    }

    /// <summary>Each kind on its own number and its own word, both ways round.</summary>
    [Theory]
    [InlineData(EventKind.Health, 0, "Health")]
    [InlineData(EventKind.HealthCheck, 1, "HealthCheck")]
    [InlineData(EventKind.Restart, 2, "Restart")]
    [InlineData(EventKind.Version, 3, "Version")]
    [InlineData(EventKind.Serving, 4, "Serving")]
    [InlineData(EventKind.Pinned, 5, "Pinned")]
    [InlineData(EventKind.Traffic, 6, "Traffic")]
    [InlineData(EventKind.Cluster, 7, "Cluster")]
    public void EachEventKindStandsOnItsOwnNumberAndItsOwnWord(EventKind kind, int number, string name)
    {
        Assert.Equal(number, (int)kind);
        Assert.Equal(kind, (EventKind)number);
        Assert.Equal(name, kind.ToString());
        Assert.Equal(name, Enum.GetName(kind));
        Assert.True(Enum.IsDefined(kind));
    }

    /// <summary>An unset kind is the first one, <c>Health</c>; neither one below the first nor one above the last nor
    /// either end of <see cref="int" /> is a kind, and a value the enum does not name says its number.</summary>
    [Fact]
    public void AnUnsetEventKindIsHealthAndNothingOutsideTheEightIsAKind()
    {
        var outside = EnumVocabulary<EventKind>.OutsideOf(8);

        Assert.Equal(EventKind.Health, default);
        Assert.Equal(EventKind.Health, EnumVocabulary<EventKind>.Of(0));

        Assert.True(Enum.IsDefined(EventKind.Health));
        Assert.True(Enum.IsDefined(EventKind.Cluster));
        Assert.False(Enum.IsDefined(outside.Below));
        Assert.False(Enum.IsDefined(outside.Above));
        Assert.False(Enum.IsDefined(outside.Lowest));
        Assert.False(Enum.IsDefined(outside.Highest));

        Assert.Null(Enum.GetName(outside.Above));
        Assert.Null(Enum.GetName(outside.Below));
        Assert.Null(Enum.GetName(outside.Lowest));
        Assert.Null(Enum.GetName(outside.Highest));
        Assert.Equal("8", outside.Above.ToString());
        Assert.Equal("-1", outside.Below.ToString());
        Assert.Equal("-2147483648", outside.Lowest.ToString());
        Assert.Equal("2147483647", outside.Highest.ToString());
    }

    /// <summary>The eight kinds are apart from one another and sort by their numbers, as the declaration orders
    /// them; a boxed <see cref="int" /> on the same number is no kind's equal.</summary>
    [Fact]
    public void TheEventKindsAreApartFromOneAnotherAndSortByTheirNumber()
    {
        var kinds = Enum.GetValues<EventKind>();

        Assert.Equal(kinds.Length, kinds.Distinct().Count());
        Assert.Equal(kinds, kinds.Order());
        Assert.True(EventKind.Health < EventKind.HealthCheck);
        Assert.True(EventKind.Traffic < EventKind.Cluster);
        Assert.Equal(EventKind.Health, kinds.Min());
        Assert.Equal(EventKind.Cluster, kinds.Max());
        Assert.Equal(1, EventKind.Cluster.CompareTo(EventKind.Health));
        Assert.Equal(-1, EventKind.Health.CompareTo(EventKind.Cluster));
        Assert.Equal(0, EventKind.Serving.CompareTo((object)EventKind.Serving));

        Assert.False(EventKind.Cluster.Equals(null));
        Assert.False(EventKind.Cluster.Equals((object)7));
        Assert.Equal(EventKind.Cluster.GetHashCode(), ((object)EventKind.Cluster).GetHashCode());
    }

    /// <summary>A kind is a choice and not a set: the operators work on the numbers underneath, so two kinds can
    /// combine into a third - or, past <c>Cluster</c>, into no kind at all.</summary>
    [Fact]
    public void TwoEventKindsCombinedAreAnotherKindOrNoKindAtAll()
    {
        Assert.Equal(EventKind.Version, EventKind.HealthCheck | EventKind.Restart);
        Assert.Equal(EventKind.Traffic, EventKind.Restart | EventKind.Serving);
        Assert.Equal(EventKind.Cluster, EventKind.Version | EventKind.Serving);
        Assert.Equal(EventKind.Pinned, EventKind.Pinned | EventKind.Health);
        Assert.Equal(EventKind.Health, EventKind.HealthCheck & EventKind.Restart);
        Assert.Equal(EventKind.Restart, EventKind.Version & EventKind.Traffic);

        Assert.Equal((EventKind)8, EventKind.Cluster + 1);
        Assert.False(Enum.IsDefined(EventKind.Cluster + 1));
        Assert.Equal((EventKind)(-1), EventKind.Health - 1);
        Assert.False(Enum.IsDefined(EventKind.Health - 1));
    }

    // ----- Text that is or is not a kind -----

    /// <summary>Text that names a kind in the enum's case, surrounding spaces walked past, or holds its number is
    /// that kind; <c>Health</c> is read whole and never as the start of <c>HealthCheck</c>.</summary>
    [Theory]
    [InlineData("Health", EventKind.Health)]
    [InlineData("HealthCheck", EventKind.HealthCheck)]
    [InlineData("Restart", EventKind.Restart)]
    [InlineData("Version", EventKind.Version)]
    [InlineData("Serving", EventKind.Serving)]
    [InlineData("Pinned", EventKind.Pinned)]
    [InlineData("Traffic", EventKind.Traffic)]
    [InlineData("Cluster", EventKind.Cluster)]
    [InlineData("  Cluster  ", EventKind.Cluster)]
    [InlineData("0", EventKind.Health)]
    [InlineData("7", EventKind.Cluster)]
    [InlineData("01", EventKind.HealthCheck)]
    public void AnEventKindTextIsTheKindItNamesOrTheKindItNumbers(string text, EventKind expected)
    {
        Assert.True(Enum.TryParse<EventKind>(text, out var kind));
        Assert.Equal(expected, kind);
        Assert.True(Enum.IsDefined(kind));
        Assert.Equal(expected, Enum.Parse<EventKind>(text));
    }

    /// <summary>Text that is no kind is refused - null, empty, blank, another case, a name with something behind it,
    /// a fraction, a hexadecimal, a near-word - and a refused parse leaves <c>Health</c> behind.</summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData("health")]
    [InlineData("CLUSTER")]
    [InlineData("Health Check")]
    [InlineData("HealthChecks")]
    [InlineData("Restart!")]
    [InlineData("3.0")]
    [InlineData("0x3")]
    [InlineData("Deployment")]
    public void TextThatIsNoEventKindIsRefusedAndLeavesHealthBehind(string? text)
    {
        Assert.False(Enum.TryParse<EventKind>(text, out var kind));
        Assert.Equal(EventKind.Health, kind);
        Assert.False(Enum.TryParse<EventKind>(text, ignoreCase: false, out _));
    }

    /// <summary><c>Enum.Parse</c> says what is wrong with text that is no kind: null is no text at all, the empty
    /// string and a blank are text without a kind in it, and an unknown word is an unknown word.</summary>
    [Fact]
    public void ParsingTextThatIsNoEventKindThrows()
    {
        Assert.Throws<ArgumentNullException>(() => Enum.Parse<EventKind>(null!));
        Assert.Throws<ArgumentException>(() => Enum.Parse<EventKind>(string.Empty));
        Assert.Throws<ArgumentException>(() => Enum.Parse<EventKind>("   "));
        Assert.Throws<ArgumentException>(() => Enum.Parse<EventKind>("Deployment"));
    }

    /// <summary>A number outside the eight is read as a kind that is no kind, and a name in another case is a kind
    /// only once the parse is told to forgive the case.</summary>
    [Fact]
    public void ANumberOutsideTheEightIsReadAsAKindThatIsNoKind()
    {
        Assert.True(Enum.TryParse<EventKind>("8", out var above));
        Assert.Equal((EventKind)8, above);
        Assert.False(Enum.IsDefined(above));

        Assert.True(Enum.TryParse<EventKind>("-1", out var below));
        Assert.Equal((EventKind)(-1), below);
        Assert.False(Enum.IsDefined(below));

        Assert.False(Enum.TryParse<EventKind>("healthcheck", out _));
        Assert.True(Enum.TryParse<EventKind>("healthcheck", ignoreCase: true, out var forgiven));
        Assert.Equal(EventKind.HealthCheck, forgiven);
    }

    // ----- The kind an event carries -----

    /// <summary>An event carries the kind it is given, whatever it is: a kind no detector hands out, a number
    /// outside the eight, and the default <c>Health</c>, each kept and each making another event.</summary>
    [Fact]
    public void AnEventKeepsTheKindItIsGivenEvenOneOutsideTheEight()
    {
        var health = new DashboardEvent(Now, default, EventLevel.Info, null, null, string.Empty);
        var outside = health with { Kind = (EventKind)99 };

        Assert.Equal(EventKind.Health, health.Kind);
        Assert.Equal((EventKind)99, outside.Kind);
        Assert.NotEqual(health, outside);
        Assert.Equal(health, outside with { Kind = EventKind.Health });
        Assert.EndsWith(", Kind = 99, Level = Info, Environment = , Node = , Text =  }", outside.ToString(), StringComparison.Ordinal);
    }
}
