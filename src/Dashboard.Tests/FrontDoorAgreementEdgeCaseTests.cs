using System.Globalization;
using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="FrontDoorAgreement" /> - the four words for what the
/// Front Door endpoint shows against what the nodes predict - declared in
/// <c>src/Dashboard/Health/ServingAssessment.cs</c> beside the decision that reaches them.
/// <para>
/// What the four are worth is pinned where the decision is made and said. <see cref="ServingAssessmentTests" /> pins
/// the rule the members stand for (the endpoint healthy exactly when a node is, <c>Agrees</c>, and <c>Disagrees</c>
/// the other way round), the <c>NotPresent</c> of a deployable without an endpoint and the words of every agreement
/// the decision reaches - <c>Front Door agrees: it is healthy.</c>, <c>... it is unhealthy too.</c>,
/// <c>... it is unreachable although westus3 is healthy.</c>, <c>Front Door is being checked.</c>;
/// <see cref="RotationTests" /> pins <c>Agrees</c> and <c>Disagrees</c> of a rotation;
/// <see cref="TargetStatusTests" /> and <see cref="DashboardMonitorTests" /> pin the agreement of a real topology read
/// through its tiles; <see cref="RuntimePayloadBuilderTests" /> pins the line of a Front Door tile that
/// <c>Agrees</c>.
/// </para>
/// <para>
/// Left open by all of that, and pinned here: the four members as the numbers, the names and the order the declaration
/// writes them in, and nothing outside them however it is arrived at - cast, read out of text, or made by combining
/// two members, which this enum neither offers nor forbids; the word <c>Pending</c> shared with the two vocabularies
/// beside it and meaning another thing in each; the cells of the comparison the other files never stand on - no
/// nodes at all against every answer an endpoint may give, a wait against an endpoint unhealthy, unreachable or
/// absent, a failover against an endpoint unreachable or unchecked, a deployable down against an endpoint unchecked or
/// absent, and a rotation whose primaries are not all in yet; the two invariants that tie a member to the endpoint it
/// speaks of, that <c>NotPresent</c> is the absent endpoint and nothing else and that a word is said exactly then; and
/// the words of the two members when the endpoint has a state nobody settled on - <c>not checked</c>, and the node a
/// disagreement names when the assessment names none.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time is: every input is a node, an endpoint state or
/// a member the test brings, and the few assessments that stand on a combination the decision itself never produces
/// are built by hand, which is the only way the fallbacks of those words can be stood on at all.
/// </para>
/// </summary>
public class FrontDoorAgreementEdgeCaseTests
{
    private const HealthState Healthy = HealthState.Healthy;
    private const HealthState Unhealthy = HealthState.Unhealthy;
    private const HealthState Unreachable = HealthState.Unreachable;
    private const HealthState Pending = HealthState.Pending;

    /// <summary>The endpoint's own answer, every shape a probe can bring back and the nothing of no endpoint.</summary>
    private static readonly HealthState?[] Endpoints = [null, Pending, Healthy, Unhealthy, Unreachable];

    /// <summary>The five states a decision lands on, each of them reached by a shape of nodes below.</summary>
    private static readonly ServingState[] States =
        [ServingState.NoNodes, ServingState.Pending, ServingState.Primary, ServingState.FailedOver, ServingState.Down];

    private static NodeHealth Primary(HealthState state, string region = "westus3") => new($"app-{region}", region, true, state);

    private static NodeHealth Standby(HealthState state, string region = "eastus2") => new($"app-{region}", region, false, state);

    /// <summary>Two nodes, in the order of the topology, that leave the decision on the given state.</summary>
    private static NodeHealth[] NodesFor(ServingState state) => state switch
    {
        ServingState.NoNodes => [],
        ServingState.Pending => [Primary(Pending), Standby(Healthy)],
        ServingState.Primary => [Primary(Healthy), Standby(Unreachable)],
        ServingState.FailedOver => [Primary(Unreachable), Standby(Healthy)],
        _ => [Primary(Unhealthy), Standby(Unreachable)],
    };

    /// <summary>The same five states as a rotation: two primaries, so the Front Door rotates over them, and a standby
    /// that stands by until none of the primaries is healthy.</summary>
    private static NodeHealth[] RotationFor(ServingState state) => state switch
    {
        ServingState.Pending => [Primary(Pending), Primary(Healthy, "eastus2")],
        ServingState.Primary => [Primary(Unhealthy), Primary(Healthy, "eastus2")],
        ServingState.FailedOver => [Primary(Unreachable), Primary(Unhealthy, "eastus2"), Standby(Healthy, "japaneast")],
        _ => [Primary(Unhealthy), Primary(Unreachable, "eastus2")],
    };

    /// <summary>An assessment holding the agreement and the endpoint's state exactly as they are given - the shapes the
    /// decision never produces, which are the only way the words of an unsettled endpoint can be read at all.</summary>
    private static ServingAssessment Shaped(
        ServingState state,
        FrontDoorAgreement agreement,
        HealthState? frontDoor,
        NodeHealth? expected = null) => new(state, expected, null, agreement, frontDoor);

    // --------------------------------------------- the four agreements, as the numbers and words they are

    /// <summary>The four members in the order the enum declares them, on the numbers 0 to 3, of an
    /// <see cref="int" /> underneath, and a choice and not a set: the enum wears no
    /// <see cref="FlagsAttribute" />, so the page holds one word for one deployable and never two at once.</summary>
    [Fact]
    public void TheFourAgreementsAreTheWholeVocabularyInTheirDeclaredOrder()
    {
        Assert.Equal(["NotPresent", "Pending", "Agrees", "Disagrees"], Enum.GetNames<FrontDoorAgreement>());
        Assert.Equal([0, 1, 2, 3], Enum.GetValues<FrontDoorAgreement>().Select(agreement => (int)agreement));
        Assert.Equal(4, Enum.GetValues<FrontDoorAgreement>().Length);
        Assert.Equal(typeof(int), Enum.GetUnderlyingType(typeof(FrontDoorAgreement)));
        Assert.False(typeof(FrontDoorAgreement).IsDefined(typeof(FlagsAttribute), false));
    }

    /// <summary>Each member on its own number and in its own word, both ways round: the number the name stands on, the
    /// word that number answers to, and every one of the four defined however it is arrived at. The four words are the
    /// whole of what a member says of itself - the sentence the page shows is pinned with the decision below.</summary>
    [Theory]
    [InlineData(FrontDoorAgreement.NotPresent, 0, "NotPresent")]
    [InlineData(FrontDoorAgreement.Pending, 1, "Pending")]
    [InlineData(FrontDoorAgreement.Agrees, 2, "Agrees")]
    [InlineData(FrontDoorAgreement.Disagrees, 3, "Disagrees")]
    public void EachAgreementStandsOnItsOwnNumberAndInItsOwnWord(FrontDoorAgreement agreement, int number, string name)
    {
        Assert.Equal(number, (int)agreement);
        Assert.Equal(agreement, (FrontDoorAgreement)number);
        Assert.Equal(name, agreement.ToString());
        Assert.Equal(name, Enum.GetName(agreement));
        Assert.True(Enum.IsDefined(agreement));

        // The word, the number and the member itself are three ways to the same one of the four.
        Assert.Equal(agreement, Enum.Parse<FrontDoorAgreement>(name));
        Assert.Equal(agreement, Enum.Parse<FrontDoorAgreement>(number.ToString(CultureInfo.InvariantCulture)));
        Assert.Equal(agreement, Enum.GetValues<FrontDoorAgreement>()[number]);
    }

    /// <summary>A member said in its own word or in its own number, however the format asks for it - the general
    /// format's word, the decimal and the hexadecimal of the number underneath, padded to the width of the
    /// <see cref="int" /> the enum is of - and a number the enum does not name said as the number it is in every format
    /// alike, since there is no word for it.</summary>
    [Theory]
    [InlineData(FrontDoorAgreement.NotPresent, "NotPresent", "0", "00000000")]
    [InlineData(FrontDoorAgreement.Pending, "Pending", "1", "00000001")]
    [InlineData(FrontDoorAgreement.Agrees, "Agrees", "2", "00000002")]
    [InlineData(FrontDoorAgreement.Disagrees, "Disagrees", "3", "00000003")]
    [InlineData((FrontDoorAgreement)4, "4", "4", "00000004")]
    public void EachAgreementIsSaidInItsWordOrInItsNumber(FrontDoorAgreement agreement, string word, string number, string hex)
    {
        Assert.Equal(word, agreement.ToString());
        Assert.Equal(word, agreement.ToString("G"));
        Assert.Equal(word, agreement.ToString("g"));
        Assert.Equal(number, agreement.ToString("D"));
        Assert.Equal(number, agreement.ToString("d"));
        // The case of the format letter is not looked at: both spell the hexadecimal capitals.
        Assert.Equal(hex, agreement.ToString("X"));
        Assert.Equal(hex, agreement.ToString("x"));
        Assert.Equal(word, Enum.Format(typeof(FrontDoorAgreement), agreement, "G"));
    }

    // --------------------------------------------------------- the boundary: nothing outside the four

    /// <summary>The first member is <see cref="FrontDoorAgreement.NotPresent" />: the word a value of the type that was
    /// never settled on stands on, which is the one the page says nothing about - an agreement nobody reached reads as
    /// no endpoint at all, whether an endpoint was probed or not. Outside the four there is nothing: neither one below
    /// the first nor one above the last nor either end of <see cref="int" /> is a defined agreement, and a value the
    /// enum does not name says the number it stands on.</summary>
    [Fact]
    public void AnUnsettledAgreementIsNotPresentAndNothingOutsideTheFourIsOne()
    {
        Assert.Equal(FrontDoorAgreement.NotPresent, default);
        Assert.Equal(FrontDoorAgreement.NotPresent, (FrontDoorAgreement)0);

        // The nothing of an agreement no one reached says the same words as the nothing of an endpoint there is not.
        var unsettled = Shaped(ServingState.Primary, default, Healthy, Primary(Healthy));

        Assert.Null(unsettled.FrontDoorText);
        Assert.Equal(FrontDoorAgreement.NotPresent, unsettled.FrontDoor);

        Assert.True(Enum.IsDefined(FrontDoorAgreement.NotPresent));
        Assert.True(Enum.IsDefined(FrontDoorAgreement.Disagrees));
        Assert.False(Enum.IsDefined((FrontDoorAgreement)(-1)));
        Assert.False(Enum.IsDefined((FrontDoorAgreement)4));
        Assert.False(Enum.IsDefined((FrontDoorAgreement)int.MinValue));
        Assert.False(Enum.IsDefined((FrontDoorAgreement)int.MaxValue));

        Assert.Null(Enum.GetName((FrontDoorAgreement)4));
        Assert.Null(Enum.GetName((FrontDoorAgreement)(-1)));
        Assert.Equal("4", ((FrontDoorAgreement)4).ToString());
        Assert.Equal("-1", ((FrontDoorAgreement)(-1)).ToString());
        Assert.Equal("-2147483648", ((FrontDoorAgreement)int.MinValue).ToString());
        Assert.Equal("2147483647", ((FrontDoorAgreement)int.MaxValue).ToString());
    }

    /// <summary>The four members are apart from one another and sort by their numbers, as the declaration orders them;
    /// a boxed <see cref="int" /> on the same number is no member's equal, and neither is a box of nothing, a word the
    /// member is spelt in or the same word of another vocabulary.</summary>
    [Fact]
    public void TheFourAgreementsAreApartFromOneAnotherAndSortByTheirNumber()
    {
        FrontDoorAgreement[] agreements = [FrontDoorAgreement.NotPresent, FrontDoorAgreement.Pending, FrontDoorAgreement.Agrees, FrontDoorAgreement.Disagrees];

        Assert.All(agreements, agreement => Assert.Single(agreements, other => other == agreement));

        Assert.True(FrontDoorAgreement.NotPresent < FrontDoorAgreement.Pending);
        Assert.True(FrontDoorAgreement.Pending < FrontDoorAgreement.Agrees);
        Assert.True(FrontDoorAgreement.Agrees < FrontDoorAgreement.Disagrees);
        Assert.False(FrontDoorAgreement.Disagrees < FrontDoorAgreement.Agrees);
        Assert.Equal(1, FrontDoorAgreement.Disagrees.CompareTo(FrontDoorAgreement.Agrees));
        Assert.Equal(-1, FrontDoorAgreement.NotPresent.CompareTo(FrontDoorAgreement.Disagrees));
        Assert.Equal(0, FrontDoorAgreement.Agrees.CompareTo((object)FrontDoorAgreement.Agrees));

        Assert.Equal((object)FrontDoorAgreement.Agrees, (object)FrontDoorAgreement.Agrees);
        Assert.False(((object)FrontDoorAgreement.Agrees).Equals(null));
        Assert.False(FrontDoorAgreement.Agrees.Equals(null));
        Assert.False(FrontDoorAgreement.Agrees.Equals("Agrees"));
        Assert.False(FrontDoorAgreement.Agrees.Equals(HealthState.Healthy));
        Assert.NotEqual((object)FrontDoorAgreement.Agrees, (object)2);
        Assert.Equal(FrontDoorAgreement.Agrees.GetHashCode(), ((object)FrontDoorAgreement.Agrees).GetHashCode());
        Assert.Equal(FrontDoorAgreement.Agrees.GetHashCode(), ((FrontDoorAgreement)2).GetHashCode());
    }

    /// <summary>A member is a choice and not a set: the enum wears no <see cref="FlagsAttribute" />, so the operators
    /// work on the numbers underneath - and because those numbers are the four that stand from 0 to 3, a union of two
    /// members lands on a defined member that is neither of them (<c>Agrees</c> with <c>Pending</c> is
    /// <c>Disagrees</c>, a word no caller would have meant), an intersection lands on a member neither of them was
    /// (<c>NotPresent</c>, off <c>Agrees</c> and <c>Pending</c> together), and one step past either end is a number no
    /// member wears.</summary>
    [Fact]
    public void TwoAgreementsCombinedAreAnotherAgreementOrNoAgreementAtAll()
    {
        Assert.False(typeof(FrontDoorAgreement).IsDefined(typeof(FlagsAttribute), false));

        Assert.Equal(FrontDoorAgreement.Pending, FrontDoorAgreement.NotPresent | FrontDoorAgreement.Pending);
        Assert.Equal(FrontDoorAgreement.Disagrees, FrontDoorAgreement.Agrees | FrontDoorAgreement.Pending);
        Assert.Equal(FrontDoorAgreement.Disagrees, FrontDoorAgreement.Disagrees | FrontDoorAgreement.Agrees);
        Assert.Equal(FrontDoorAgreement.Disagrees, FrontDoorAgreement.Disagrees | FrontDoorAgreement.NotPresent);

        Assert.Equal(FrontDoorAgreement.NotPresent, FrontDoorAgreement.Agrees & FrontDoorAgreement.Pending);
        Assert.Equal(FrontDoorAgreement.NotPresent, FrontDoorAgreement.Pending & FrontDoorAgreement.NotPresent);
        Assert.Equal(FrontDoorAgreement.Agrees, FrontDoorAgreement.Agrees & FrontDoorAgreement.Disagrees);
        Assert.Equal(FrontDoorAgreement.Pending, FrontDoorAgreement.Pending & FrontDoorAgreement.Disagrees);

        Assert.Equal((FrontDoorAgreement)4, FrontDoorAgreement.Disagrees + 1);
        Assert.False(Enum.IsDefined(FrontDoorAgreement.Disagrees + 1));
        Assert.Equal((FrontDoorAgreement)(-1), FrontDoorAgreement.NotPresent - 1);
        Assert.False(Enum.IsDefined(FrontDoorAgreement.NotPresent - 1));
        Assert.Equal((FrontDoorAgreement)(-1), ~FrontDoorAgreement.NotPresent);
        Assert.False(Enum.IsDefined(~FrontDoorAgreement.Disagrees));
    }

    /// <summary>The word <c>Pending</c> belongs to the vocabulary of the agreement and to the two beside it -
    /// <see cref="HealthState" /> and <see cref="ServingState" /> - and is the only word any two of the three share.
    /// It stands on the number 1 in the agreement and in the serving state, where it is the second member of each, and
    /// on 0 in the health state, where it is the first: the same number, 1, says an endpoint not yet asked in one
    /// vocabulary and a deployable not yet decided in another, and the same word says a third thing again. Nothing
    /// converts between the three, and a member of one is no other's equal, whatever its number - which is why each of
    /// the three is read where it stands.</summary>
    [Fact]
    public void TheWordPendingIsSharedWithTheTwoVocabulariesBesideItAndMeansAnotherThingInEach()
    {
        Assert.Equal(["NotPresent", "Pending", "Agrees", "Disagrees"], Enum.GetNames<FrontDoorAgreement>());
        Assert.Equal(["Pending", "Healthy", "Unhealthy", "Unreachable"], Enum.GetNames<HealthState>());
        Assert.Equal(["NoNodes", "Pending", "Primary", "FailedOver", "Down"], Enum.GetNames<ServingState>());

        Assert.Equal(["Pending"], Enum.GetNames<FrontDoorAgreement>().Intersect(Enum.GetNames<HealthState>()).ToArray());
        Assert.Equal(["Pending"], Enum.GetNames<FrontDoorAgreement>().Intersect(Enum.GetNames<ServingState>()).ToArray());
        Assert.Equal(["Pending"], Enum.GetNames<HealthState>().Intersect(Enum.GetNames<ServingState>()).ToArray());

        Assert.Equal(1, (int)FrontDoorAgreement.Pending);
        Assert.Equal(1, (int)ServingState.Pending);
        Assert.Equal(0, (int)HealthState.Pending);

        // The same number, in the vocabulary next door, is another thing entirely.
        Assert.Equal(HealthState.Healthy, (HealthState)(int)FrontDoorAgreement.Pending);
        Assert.Equal(ServingState.NoNodes, (ServingState)(int)FrontDoorAgreement.NotPresent);
        Assert.Equal(FrontDoorAgreement.Pending, (FrontDoorAgreement)(int)ServingState.Pending);

        // And the same word read out of text is a member of whichever vocabulary is asked for it, and no other's equal.
        Assert.Equal(FrontDoorAgreement.Pending, Enum.Parse<FrontDoorAgreement>("Pending"));
        Assert.Equal(HealthState.Pending, Enum.Parse<HealthState>("Pending"));
        Assert.False(FrontDoorAgreement.Pending.Equals((object)HealthState.Pending));
        Assert.False(FrontDoorAgreement.Pending.Equals((object)ServingState.Pending));

        // The number is what hashes, so two vocabularies' members on one number share a hash and are still two.
        Assert.Equal(FrontDoorAgreement.Pending.GetHashCode(), ServingState.Pending.GetHashCode());
        Assert.Equal(3, new HashSet<object>([FrontDoorAgreement.Pending, ServingState.Pending, HealthState.Pending]).Count);
    }

    // ------------------------------------------------------- text that is, and text that is no agreement

    /// <summary>Text that names a member in the enum's case, with its surrounding spaces walked past, or holds its
    /// number is that member; a name is read whole and a number with a cipher in front of it is still the small number
    /// it says.</summary>
    [Theory]
    [InlineData("NotPresent", FrontDoorAgreement.NotPresent)]
    [InlineData("Pending", FrontDoorAgreement.Pending)]
    [InlineData("Agrees", FrontDoorAgreement.Agrees)]
    [InlineData("Disagrees", FrontDoorAgreement.Disagrees)]
    [InlineData("  Agrees  ", FrontDoorAgreement.Agrees)]
    [InlineData("\tDisagrees\n", FrontDoorAgreement.Disagrees)]
    [InlineData("0", FrontDoorAgreement.NotPresent)]
    [InlineData("3", FrontDoorAgreement.Disagrees)]
    [InlineData("01", FrontDoorAgreement.Pending)]
    public void AnAgreementTextIsTheAgreementItNamesOrTheAgreementItNumbers(string text, FrontDoorAgreement expected)
    {
        Assert.True(Enum.TryParse(text, out FrontDoorAgreement agreement));
        Assert.Equal(expected, agreement);
        Assert.True(Enum.IsDefined(agreement));
        Assert.Equal(expected, Enum.Parse<FrontDoorAgreement>(text));
    }

    /// <summary>Text that is no member is refused - nothing at all, the empty word, a blank of spaces or of a tab, a
    /// name in the lower case the page's own words are written in, a name in capitals, a name with something behind it,
    /// a fraction, a hexadecimal, the sentence a tile says, and the words of the two vocabularies beside it - and a
    /// refused parse leaves its answer standing on <see cref="FrontDoorAgreement.NotPresent" />, the first of the four:
    /// a word mis-spelt is read as a deployable with no Front Door endpoint at all, and the page says nothing.</summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData("agrees")]
    [InlineData("NOTPRESENT")]
    [InlineData("Disagrees?")]
    [InlineData("Agrees with the web apps")]
    [InlineData("not present")]
    [InlineData("Front Door agrees")]
    [InlineData("Healthy")]
    [InlineData("Primary")]
    [InlineData("1.0")]
    [InlineData("0x2")]
    public void TextThatIsNoAgreementIsRefusedAndLeavesNotPresentBehind(string? text)
    {
        Assert.False(Enum.TryParse<FrontDoorAgreement>(text, out var agreement));
        Assert.Equal(FrontDoorAgreement.NotPresent, agreement);
        Assert.False(Enum.TryParse<FrontDoorAgreement>(text, ignoreCase: false, out _));
    }

    /// <summary>Reading text that is no member says what is wrong with it: nothing at all is no text, the empty word
    /// and a blank are text without a member in them, and a word is a word in the case the enum spells it - the
    /// page's lower-case <c>agrees</c>, the tile's whole sentence and the word of another vocabulary are all
    /// refused.</summary>
    [Fact]
    public void ParsingTextThatIsNoAgreementThrows()
    {
        Assert.Throws<ArgumentNullException>(() => Enum.Parse<FrontDoorAgreement>(null!));
        Assert.Throws<ArgumentException>(() => Enum.Parse<FrontDoorAgreement>(string.Empty));
        Assert.Throws<ArgumentException>(() => Enum.Parse<FrontDoorAgreement>("   "));
        Assert.Throws<ArgumentException>(() => Enum.Parse<FrontDoorAgreement>("agrees"));
        Assert.Throws<ArgumentException>(() => Enum.Parse<FrontDoorAgreement>("Healthy"));
    }

    /// <summary>A number outside the four is read as a member that is no member, however it is arrived at - the
    /// number one past the last, the number below the first and the top of <see cref="int" /> - and the case the page
    /// writes its words in is forgiven only when the parse is told to: a lower-case <c>agrees</c> is no member until it
    /// is asked for as one. Nothing at all, the empty word and a blank stay refused whichever way the case is treated,
    /// each of them leaving <see cref="FrontDoorAgreement.NotPresent" /> behind.</summary>
    [Fact]
    public void ANumberOutsideTheFourIsNoAgreementAndTheCaseIsForgivenOnlyWhenAsked()
    {
        Assert.True(Enum.TryParse<FrontDoorAgreement>("4", out var above));
        Assert.Equal((FrontDoorAgreement)4, above);
        Assert.False(Enum.IsDefined(above));
        Assert.Null(Enum.GetName(above));

        Assert.True(Enum.TryParse<FrontDoorAgreement>("-1", out var below));
        Assert.False(Enum.IsDefined(below));

        Assert.True(Enum.TryParse<FrontDoorAgreement>("2147483647", out var top));
        Assert.Equal((FrontDoorAgreement)int.MaxValue, top);
        Assert.False(Enum.IsDefined(top));

        // The words of the page are its own lower case, and the enum spells them with a capital: forgiven on request
        // and not otherwise, which is why the file that reads an answer compares members rather than parsing text.
        Assert.False(Enum.TryParse<FrontDoorAgreement>("agrees", ignoreCase: false, out _));
        Assert.True(Enum.TryParse<FrontDoorAgreement>("agrees", ignoreCase: true, out var forgiven));
        Assert.Equal(FrontDoorAgreement.Agrees, forgiven);
        Assert.True(Enum.TryParse<FrontDoorAgreement>("DISAGREES", ignoreCase: true, out var shouted));
        Assert.Equal(FrontDoorAgreement.Disagrees, shouted);

        Assert.False(Enum.TryParse<FrontDoorAgreement>(null, ignoreCase: true, out var nothing));
        Assert.Equal(FrontDoorAgreement.NotPresent, nothing);
        Assert.False(Enum.TryParse<FrontDoorAgreement>(string.Empty, ignoreCase: true, out var empty));
        Assert.Equal(FrontDoorAgreement.NotPresent, empty);
        Assert.False(Enum.TryParse<FrontDoorAgreement>(" ", ignoreCase: true, out var blank));
        Assert.Equal(FrontDoorAgreement.NotPresent, blank);
    }

    // ------------------------------------------------------------ the agreement a decision reaches, cell by cell

    /// <summary>The cells of the comparison the other files never stand on: a deployable with no nodes at all against
    /// an endpoint unchecked, unhealthy, unreachable or absent (its state is pinned against a healthy one, and the
    /// whole of the rule against nodes there are); a decision not made yet against an endpoint unhealthy, unreachable,
    /// unchecked or absent; a failover against an endpoint unreachable, unchecked or absent; and a deployable down
    /// against an endpoint unchecked or absent. Every row reads the nodes back, so a row cannot quietly move to
    /// another cell of the table.</summary>
    [Theory]
    // No nodes: the endpoint's answer has nothing to be read against, and only its absence says NotPresent.
    [InlineData(ServingState.NoNodes, null, FrontDoorAgreement.NotPresent)]
    [InlineData(ServingState.NoNodes, Pending, FrontDoorAgreement.Pending)]
    [InlineData(ServingState.NoNodes, Unhealthy, FrontDoorAgreement.Pending)]
    [InlineData(ServingState.NoNodes, Unreachable, FrontDoorAgreement.Pending)]
    // A node of higher priority unchecked: no disagreement is settled until it is.
    [InlineData(ServingState.Pending, null, FrontDoorAgreement.NotPresent)]
    [InlineData(ServingState.Pending, Unhealthy, FrontDoorAgreement.Pending)]
    [InlineData(ServingState.Pending, Unreachable, FrontDoorAgreement.Pending)]
    [InlineData(ServingState.Pending, Pending, FrontDoorAgreement.Pending)]
    // A primary healthy and its standby down: an endpoint unhealthy disagrees with the node that serves.
    [InlineData(ServingState.Primary, Unhealthy, FrontDoorAgreement.Disagrees)]
    // Failed over to a standby: an endpoint that answers badly disagrees with the node that serves.
    [InlineData(ServingState.FailedOver, null, FrontDoorAgreement.NotPresent)]
    [InlineData(ServingState.FailedOver, Unreachable, FrontDoorAgreement.Disagrees)]
    [InlineData(ServingState.FailedOver, Pending, FrontDoorAgreement.Pending)]
    // Nothing healthy anywhere: an unchecked endpoint agrees with nothing, and an absent one is not there.
    [InlineData(ServingState.Down, null, FrontDoorAgreement.NotPresent)]
    [InlineData(ServingState.Down, Pending, FrontDoorAgreement.Pending)]
    public void AnAgreementIsReachedForEveryCellTheOtherFilesLeaveOpen(ServingState state, HealthState? frontDoor, FrontDoorAgreement expected)
    {
        var assessment = ServingAssessment.Assess(NodesFor(state), frontDoor);

        Assert.Equal(state, assessment.State);
        Assert.Equal(expected, assessment.FrontDoor);
    }

    /// <summary>The same cells read off a rotation, whose decision is made by another road through the file: while a
    /// primary is unchecked the agreement is <c>Pending</c> whichever answer the endpoint itself brings, because a
    /// rotation is only compared with the Front Door once every primary has been asked.</summary>
    [Theory]
    [InlineData(Pending)]
    [InlineData(Healthy)]
    [InlineData(Unhealthy)]
    [InlineData(Unreachable)]
    public void ARotationWithAPrimaryUncheckedIsPendingHoweverTheEndpointAnswered(HealthState frontDoor)
    {
        var assessment = ServingAssessment.Assess(RotationFor(ServingState.Pending), frontDoor);

        Assert.Equal(ServingState.Pending, assessment.State);
        Assert.True(assessment.IsRotation);
        Assert.Equal(FrontDoorAgreement.Pending, assessment.FrontDoor);
    }

    /// <summary>A rotation whose every primary is down and whose endpoint has not been asked yet is no disagreement
    /// either: the endpoint's silence is the wait, whatever the primaries say among themselves.</summary>
    [Fact]
    public void ARotationWithNoPrimaryHealthyAndAnUncheckedEndpointIsPending()
    {
        var assessment = ServingAssessment.Assess(RotationFor(ServingState.Down), Pending);

        Assert.Equal(ServingState.Down, assessment.State);
        Assert.True(assessment.IsRotation);
        Assert.Equal(FrontDoorAgreement.Pending, assessment.FrontDoor);
    }

    /// <summary>The whole table read at once - the five states, each reached as a deployable of two nodes and as a
    /// rotation, every one of them against every answer an endpoint may give: fifty decisions of fixed inputs and no
    /// clock - holding the two invariants the members stand on and no test yet writes: that <c>NotPresent</c> is the
    /// absent endpoint and nothing else, and that a sentence is said exactly when there is an endpoint to say a sentence
    /// about. Every agreement reached is one of the four, every one of the four is reached by some decision, and the
    /// endpoint's own state is kept as it was given whatever the comparison made of it.</summary>
    [Fact]
    public void EveryDecisionSaysOneOfTheFourAndWordsItExactlyWhenAnEndpointIsThere()
    {
        var reached = new HashSet<FrontDoorAgreement>();

        foreach (var state in States)
        {
            foreach (var shape in new Func<ServingState, NodeHealth[]>[] { NodesFor, RotationFor })
            {
                foreach (var endpoint in Endpoints)
                {
                    var assessment = ServingAssessment.Assess(shape(state), endpoint);

                    Assert.True(
                        Enum.IsDefined(assessment.FrontDoor),
                        $"{state} against {endpoint?.ToString() ?? "nothing"} reached {(int)assessment.FrontDoor}, which is no agreement");
                    Assert.Equal(endpoint is null, assessment.FrontDoor == FrontDoorAgreement.NotPresent);
                    Assert.Equal(endpoint, assessment.FrontDoorState);
                    Assert.Equal(assessment.FrontDoor != FrontDoorAgreement.NotPresent, assessment.FrontDoorText is not null);

                    reached.Add(assessment.FrontDoor);
                }
            }
        }

        Assert.Equal(4, reached.Count);
        Assert.All(Enum.GetValues<FrontDoorAgreement>(), agreement => Assert.Contains(agreement, reached));
    }

    // ---------------------------------------------------- the words each agreement is said in, at the edges

    /// <summary><see cref="FrontDoorAgreement.NotPresent" /> says nothing at all, whatever state the assessment holds
    /// for the endpoint beside it: the arm of the agreement is read before the state, so an absent endpoint cannot be
    /// described as healthy by a record that was handed one - the decision never pairs the two, and a record built by
    /// hand can.</summary>
    [Theory]
    [InlineData(null)]
    [InlineData(Pending)]
    [InlineData(Healthy)]
    [InlineData(Unhealthy)]
    [InlineData(Unreachable)]
    public void ANotPresentAgreementSaysNothingWhicheverStateItHolds(HealthState? frontDoor) =>
        Assert.Null(Shaped(ServingState.Primary, FrontDoorAgreement.NotPresent, frontDoor, Primary(Healthy)).FrontDoorText);

    /// <summary><see cref="FrontDoorAgreement.Pending" /> says the one sentence of the wait whatever the endpoint's own
    /// state was: an endpoint checked and found unhealthy is still no comparison, while the nodes it would be compared
    /// with are undecided - and the sentence does not read the state at all.</summary>
    [Theory]
    [InlineData(null)]
    [InlineData(Pending)]
    [InlineData(Healthy)]
    [InlineData(Unhealthy)]
    [InlineData(Unreachable)]
    public void APendingAgreementSaysTheWaitWhicheverStateItHolds(HealthState? frontDoor) =>
        Assert.Equal("Front Door is being checked.", Shaped(ServingState.Pending, FrontDoorAgreement.Pending, frontDoor, Primary(Pending)).FrontDoorText);

    /// <summary>The two agreements that read the endpoint's state say <c>not checked</c> of a state that is
    /// <see cref="HealthState.Pending" /> just as they say it of no state at all: the decision lands on neither pairing
    /// - an endpoint unchecked makes the agreement <c>Pending</c> - so the words of the unsettled state are pinned on
    /// the records the page builds its own comparisons into, one letter for one agreement.</summary>
    [Theory]
    [InlineData(Pending)]
    [InlineData(null)]
    public void AnAgreementAboutAnUnsettledEndpointSaysItIsNotChecked(HealthState? frontDoor)
    {
        var expected = Primary(Healthy);

        Assert.Equal("Front Door agrees: it is not checked too.", Shaped(ServingState.Primary, FrontDoorAgreement.Agrees, frontDoor, expected).FrontDoorText);
        Assert.Equal("Front Door disagrees: it is not checked although westus3 is healthy.", Shaped(ServingState.Primary, FrontDoorAgreement.Disagrees, frontDoor, expected).FrontDoorText);
    }

    /// <summary>A disagreement names the node that serves, and where an assessment names none it names <c>a node</c>:
    /// the fallback of the words, read here beside each state the endpoint holds - unhealthy, unreachable and the two
    /// that say <c>not checked</c>. The sentence of a healthy endpoint that has no node to disagree with is the decision
    /// own and is pinned in <see cref="ServingAssessmentTests" />; the rest of these pairings the decision never
    /// produces, since an assessment with no node named is undecided and an undecided agreement is
    /// <see cref="FrontDoorAgreement.Pending" />.</summary>
    [Theory]
    [InlineData(Unhealthy, "Front Door disagrees: it is unhealthy although a node is healthy.")]
    [InlineData(Unreachable, "Front Door disagrees: it is unreachable although a node is healthy.")]
    [InlineData(Pending, "Front Door disagrees: it is not checked although a node is healthy.")]
    [InlineData(null, "Front Door disagrees: it is not checked although a node is healthy.")]
    public void ADisagreementNamesTheNodeItHasOrANodeAtAll(HealthState? frontDoor, string words) =>
        Assert.Equal(words, Shaped(ServingState.Down, FrontDoorAgreement.Disagrees, frontDoor).FrontDoorText);

    // ------------------------------------------------------------------- the agreement as a part of the record

    /// <summary>The agreement is a part of the assessment like any other: it stands where it is given, <c>with</c>
    /// changes it and nothing else of the record - the state, the nodes, the rotation, the headline, the failover and
    /// the endpoint's own state all read the same with any of the four worn - and the words about the endpoint are the
    /// one thing it moves, said exactly for the three agreements that are about an endpoint and not for
    /// <see cref="FrontDoorAgreement.NotPresent" />.</summary>
    [Fact]
    public void TheAgreementIsOnePartOfTheAssessmentAndMovesNoOther()
    {
        var node = Primary(Healthy);
        var one = Shaped(ServingState.Primary, FrontDoorAgreement.Agrees, Healthy, node);

        foreach (var agreement in Enum.GetValues<FrontDoorAgreement>())
        {
            var moved = one with { FrontDoor = agreement };

            Assert.Equal(agreement, moved.FrontDoor);
            Assert.Equal(agreement == FrontDoorAgreement.NotPresent, moved.FrontDoorText is null);
            Assert.Equal(one.State, moved.State);
            Assert.Equal(one.Expected, moved.Expected);
            Assert.Equal(one.FailedPrimary, moved.FailedPrimary);
            Assert.Equal(one.FrontDoorState, moved.FrontDoorState);
            Assert.Equal(one.OnlyNode, moved.OnlyNode);
            Assert.Equal(one.Rotation, moved.Rotation);
            Assert.Equal(one.IsFailedOver, moved.IsFailedOver);
            Assert.Equal(one.IsRotation, moved.IsRotation);
            Assert.Equal(one.Serving, moved.Serving);
            Assert.Equal(one.OutOfRotation, moved.OutOfRotation);
            Assert.Equal(one.Headline, moved.Headline);
            Assert.Equal(one.FailoverDetail, moved.FailoverDetail);

            Assert.NotSame(one, moved);
        }

        // The copy that names no part is the same assessment and not the same instance.
        Assert.Equal(one, one with { });
        Assert.NotSame(one, one with { });

        Assert.Equal(one, one with { FrontDoor = FrontDoorAgreement.Agrees });
        Assert.NotEqual(one, one with { FrontDoor = FrontDoorAgreement.Disagrees });
        Assert.NotEqual(one.GetHashCode(), (one with { FrontDoor = FrontDoorAgreement.Disagrees }).GetHashCode());
        Assert.True(one != Shaped(ServingState.Primary, FrontDoorAgreement.Pending, Healthy, node));

        // The one part it does move: the words about the endpoint, said for three of the four and not for the fourth.
        Assert.NotEqual(one.FrontDoorText, (one with { FrontDoor = FrontDoorAgreement.Disagrees }).FrontDoorText);
        Assert.NotEqual(one.FrontDoorText, (one with { FrontDoor = FrontDoorAgreement.NotPresent }).FrontDoorText);
    }

    /// <summary>Every one of the four worn by an assessment of the same nodes and the same endpoint state is another
    /// assessment, and none of them is one with the nothing of an assessment: the agreement is compared with the other
    /// parts and not read out of them, so two records that differ only in it are two records.</summary>
    [Theory]
    [InlineData(FrontDoorAgreement.NotPresent)]
    [InlineData(FrontDoorAgreement.Pending)]
    [InlineData(FrontDoorAgreement.Agrees)]
    [InlineData(FrontDoorAgreement.Disagrees)]
    public void EachAgreementMakesAnAssessmentOfItsOwn(FrontDoorAgreement agreement)
    {
        var node = Primary(Healthy);
        var one = Shaped(ServingState.Primary, agreement, Healthy, node);

        Assert.Equal(agreement, one.FrontDoor);
        Assert.Equal(one, Shaped(ServingState.Primary, agreement, Healthy, node));
        Assert.Equal(one.GetHashCode(), Shaped(ServingState.Primary, agreement, Healthy, node).GetHashCode());
        Assert.False(one.Equals(null));
        Assert.False(one.Equals(agreement));

        // The assessment of nothing at all is no assessment, whichever member it would have worn.
        ServingAssessment? nothing = null;

        Assert.False(one.Equals(nothing));
        Assert.False(one == nothing);

        foreach (var other in Enum.GetValues<FrontDoorAgreement>())
        {
            Assert.Equal(other == agreement, one.Equals(Shaped(ServingState.Primary, other, Healthy, node)));
        }
    }

    // ------------------------------------------------------------------- the surface the enum declares

    /// <summary>The whole of what <see cref="FrontDoorAgreement" /> declares itself, as the running assembly holds it:
    /// an <see cref="int" /> underneath, sealed and no set, and exactly its four members and nothing else - each one a
    /// public static literal of its own type, in the order the declaration writes them, on the number it stands on; no
    /// instance member of its own, so a member carries nothing beyond the number it is, and no method either: the words
    /// a member is said in belong to the assessment that holds it, pinned above.</summary>
    [Fact]
    public void TheEnumDeclaresItsFourMembersAndNothingElse()
    {
        var type = typeof(FrontDoorAgreement);

        Assert.True(type.IsEnum);
        Assert.True(type.IsValueType);
        Assert.True(type.IsSealed);
        Assert.False(type.IsPrimitive);
        Assert.False(type.IsDefined(typeof(FlagsAttribute), false));

        // The only instance member a value carries of its own is the storage the compiler gives every value type.
        var instance = type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        Assert.Single(instance);
        Assert.Equal("value__", instance[0].Name);
        Assert.Equal(typeof(int), ((FieldInfo)instance[0]).FieldType);

        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);

        Assert.Equal(["NotPresent", "Pending", "Agrees", "Disagrees"], fields.Select(field => field.Name));
        Assert.Equal([0, 1, 2, 3], fields.Select(field => (int)field.GetRawConstantValue()!));
        Assert.All(fields, field =>
        {
            Assert.Equal(type, field.FieldType);
            Assert.True(field.IsPublic);
            Assert.True(field.IsLiteral);
            Assert.False(field.IsInitOnly);
        });

        // Nothing to call, nothing to build and nothing to write: the four members are the whole public surface.
        Assert.Empty(type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly));
        Assert.Empty(type.GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly));
        Assert.Empty(type.GetConstructors(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
    }
}
