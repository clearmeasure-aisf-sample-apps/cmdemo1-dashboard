using System.Globalization;
using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="WorkAudience" />, the three words for whose work a
/// filter shows, declared in <c>src/Dashboard/Work/WorkItemsReport.cs</c>. What a word is worth in the view is
/// pinned where the view uses it: <see cref="WorkItemsTextTests" /> pins the address segment
/// <see cref="WorkItemsText.AudienceHash" /> of every filter, the name <see cref="WorkItemsText.AudienceLabel" /> of
/// every filter on the strip, and the filter <see cref="WorkItemsText.AudienceOf" /> a word in the address settles
/// on, in any case and for any other word; <see cref="WorkItemsTests" /> the items <see cref="WorkItemsText.Select" />
/// shows, the words <see cref="WorkItemsText.NoneOf" /> says for a filter with no items, and the report
/// <see cref="WorkItemsText.Slice" /> hands that filter; <see cref="ViewAddressTests" /> the address a filter sits at
/// and the filter <see cref="ViewAddress" /> an address reads back. What all of that leaves open is pinned here: the
/// enum itself - the three audiences as the numbers, the names and the order they are declared in, the audience a
/// value of the type that was never set stands on, nothing outside the three, the audiences combined as numbers,
/// text that is or is not an audience, the format of every audience by name and by number, and the surface the
/// enum declares.
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: an audience is a word the file wrote, and
/// every word here is one the test brings.
/// </para>
/// </summary>
public class WorkAudienceEdgeCaseTests
{
    // ----- the three audiences, as the numbers they are

    /// <summary>The three audiences in the order the enum declares them, on the numbers 0 to 2, of an
    /// <see cref="int" /> underneath, and a choice and not a set: the enum wears no <see cref="FlagsAttribute" />
    /// and is sealed, so no audience is built on and none on another.</summary>
    [Fact]
    public void TheThreeAudiencesAreTheWholeVocabularyInTheirDeclaredOrder()
    {
        Assert.Equal(["All", "Bot", "Team"], Enum.GetNames<WorkAudience>());
        Assert.Equal([0, 1, 2], Enum.GetValues<WorkAudience>().Select(audience => (int)audience));
        Assert.Equal(3, Enum.GetValues<WorkAudience>().Length);
        Assert.Equal(typeof(int), Enum.GetUnderlyingType(typeof(WorkAudience)));
        Assert.False(typeof(WorkAudience).IsDefined(typeof(FlagsAttribute), false));
    }

    /// <summary>Each audience on its own number and its own word, both ways round: the number is the one the name
    /// stands on, the word is the one that number answers to, and every one of the three is defined however it is
    /// arrived at.</summary>
    [Theory]
    [InlineData(WorkAudience.All, 0, "All")]
    [InlineData(WorkAudience.Bot, 1, "Bot")]
    [InlineData(WorkAudience.Team, 2, "Team")]
    public void EachAudienceStandsOnItsOwnNumberAndItsOwnWord(WorkAudience audience, int number, string name)
    {
        Assert.Equal(number, (int)audience);
        Assert.Equal(audience, (WorkAudience)number);
        Assert.Equal(name, audience.ToString());
        Assert.Equal(name, Enum.GetName(audience));
        Assert.True(Enum.IsDefined(audience));
    }

    /// <summary>The first audience is <see cref="WorkAudience.All" />: the audience a value of the type that was
    /// never set stands on. Outside the three there is nothing: neither one below the first nor one above the last
    /// nor either end of <see cref="int" /> is a defined audience, and a value the enum does not name says the
    /// number it stands on.</summary>
    [Fact]
    public void AnUnsetAudienceIsAllAndNothingOutsideTheThreeIsAnAudience()
    {
        Assert.Equal(WorkAudience.All, default);
        Assert.Equal(WorkAudience.All, (WorkAudience)0);

        Assert.True(Enum.IsDefined(WorkAudience.All));
        Assert.True(Enum.IsDefined(WorkAudience.Team));
        Assert.False(Enum.IsDefined((WorkAudience)(-1)));
        Assert.False(Enum.IsDefined((WorkAudience)3));
        Assert.False(Enum.IsDefined((WorkAudience)int.MinValue));
        Assert.False(Enum.IsDefined((WorkAudience)int.MaxValue));

        Assert.Null(Enum.GetName((WorkAudience)3));
        Assert.Null(Enum.GetName((WorkAudience)(-1)));
        Assert.Equal("3", ((WorkAudience)3).ToString());
        Assert.Equal("-1", ((WorkAudience)(-1)).ToString());
        Assert.Equal("-2147483648", ((WorkAudience)int.MinValue).ToString());
        Assert.Equal("2147483647", ((WorkAudience)int.MaxValue).ToString());
    }

    /// <summary>The three audiences are apart from one another and sort by their numbers, as the declaration
    /// orders them; a boxed <see cref="int" /> on the same number is no audience's equal, and neither is a box of
    /// nothing, nor a name, nor any other type.</summary>
    [Fact]
    public void TheAudiencesAreApartFromOneAnotherAndSortByTheirNumber()
    {
        var audiences = Enum.GetValues<WorkAudience>();

        Assert.True(WorkAudience.All < WorkAudience.Bot);
        Assert.True(WorkAudience.Bot < WorkAudience.Team);
        Assert.Equal(1, WorkAudience.Team.CompareTo(WorkAudience.Bot));
        Assert.Equal(-1, WorkAudience.All.CompareTo(WorkAudience.Team));
        Assert.Equal(0, WorkAudience.Bot.CompareTo((object)WorkAudience.Bot));
        Assert.Equal(1, WorkAudience.Bot.CompareTo(null));

        Assert.Throws<ArgumentException>(() => WorkAudience.Bot.CompareTo(1));
        Assert.Throws<ArgumentException>(() => WorkAudience.Bot.CompareTo("Bot"));

        Assert.False(WorkAudience.Bot.Equals(null));
        Assert.False(WorkAudience.Bot.Equals((object)1));
        Assert.True(WorkAudience.Bot.Equals((object)WorkAudience.Bot));
        Assert.Equal(WorkAudience.Bot.GetHashCode(), ((object)WorkAudience.Bot).GetHashCode());

        Assert.Equal(3, audiences.Distinct().Count());
        Assert.Equal(WorkAudience.All, audiences.Min());
        Assert.Equal(WorkAudience.Team, audiences.Max());
    }

    /// <summary>An audience is a choice and not a set: the enum wears no <see cref="FlagsAttribute" />, so the
    /// operators work on the numbers underneath, and because <see cref="WorkAudience.All" /> stands on 0, it
    /// leaves every audience unchanged when combined, while <c>Bot</c> with <c>Team</c> shares no number either
    /// stands on, so their union and their difference stand on a number the enum does not name.</summary>
    [Fact]
    public void TwoAudiencesCombinedAreAnotherAudienceOrNoAudienceAtAll()
    {
        Assert.Equal(WorkAudience.Bot, WorkAudience.All | WorkAudience.Bot);
        Assert.Equal(WorkAudience.Team, WorkAudience.All | WorkAudience.Team);
        Assert.Equal((WorkAudience)3, WorkAudience.Bot | WorkAudience.Team);
        Assert.False(Enum.IsDefined(WorkAudience.Bot | WorkAudience.Team));

        Assert.Equal(WorkAudience.All, WorkAudience.Bot & WorkAudience.Team);
        Assert.Equal((WorkAudience)3, WorkAudience.Bot ^ WorkAudience.Team);

        Assert.Equal((WorkAudience)3, WorkAudience.Team + 1);
        Assert.False(Enum.IsDefined(WorkAudience.Team + 1));
        Assert.Equal((WorkAudience)(-1), WorkAudience.All - 1);
        Assert.False(Enum.IsDefined(WorkAudience.All - 1));
    }

    // ----- text that is or is not an audience

    /// <summary>Text that names an audience in the enum's case, its surrounding spaces walked past, or holds its
    /// number is that audience; a name is read whole, and a number with a cipher in front of it is still the small
    /// number it says. <see cref="WorkItemsTextTests" /> reads its words in any case, through the file's helper,
    /// and the enum alone is asked here.</summary>
    [Theory]
    [InlineData("All", WorkAudience.All)]
    [InlineData("Bot", WorkAudience.Bot)]
    [InlineData("Team", WorkAudience.Team)]
    [InlineData("  Team  ", WorkAudience.Team)]
    [InlineData("0", WorkAudience.All)]
    [InlineData("2", WorkAudience.Team)]
    [InlineData("01", WorkAudience.Bot)]
    public void AnAudienceTextIsTheAudienceItNamesOrTheAudienceItNumbers(string text, WorkAudience expected)
    {
        Assert.True(Enum.TryParse<WorkAudience>(text, out var audience));
        Assert.Equal(expected, audience);
        Assert.True(Enum.IsDefined(audience));
        Assert.Equal(expected, Enum.Parse<WorkAudience>(text));
    }

    /// <summary>Text that is no audience is refused - <see langword="null" />, the empty string, a blank of spaces
    /// or of a tab, a name in a case the enum does not spell, a name with something behind it, a plural, a
    /// fraction, a hexadecimal, a near-word of the file's own - and a refused parse leaves its answer standing on
    /// <see cref="WorkAudience.All" />, the first of the three: the <c>false</c> is the whole answer, and a caller
    /// that reads the value anyway is shown the audience that holds the whole file.</summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData("all")]
    [InlineData("BOT")]
    [InlineData("Bot!")]
    [InlineData("Teams")]
    [InlineData("1.0")]
    [InlineData("0x1")]
    [InlineData("worker")]
    public void TextThatIsNoAudienceIsRefusedAndLeavesAllBehind(string? text)
    {
        Assert.False(Enum.TryParse<WorkAudience>(text, out var audience));
        Assert.Equal(WorkAudience.All, audience);
    }

    /// <summary><see cref="Enum.Parse{TEnum}(string?, bool)" /> says what is wrong with text that is no audience:
    /// <see langword="null" /> is no text at all, the empty string and a blank are text without an audience in
    /// them, and a name in a case the enum does not spell is refused.</summary>
    [Fact]
    public void ParsingTextThatIsNoAudienceThrows()
    {
        Assert.Throws<ArgumentNullException>(() => Enum.Parse<WorkAudience>(null!));
        Assert.Throws<ArgumentException>(() => Enum.Parse<WorkAudience>(string.Empty));
        Assert.Throws<ArgumentException>(() => Enum.Parse<WorkAudience>("   "));
        Assert.Throws<ArgumentException>(() => Enum.Parse<WorkAudience>("bot"));
    }

    /// <summary>A number outside the three is read as an audience that is no audience - the top of
    /// <see cref="int" /> among them - and a name in a case the enum does not spell is an audience as soon as the
    /// parse is told to forgive the case, and nothing until it is.</summary>
    [Fact]
    public void ANumberOutsideTheThreeIsReadAsAnAudienceThatIsNoAudience()
    {
        Assert.True(Enum.TryParse<WorkAudience>("3", out var above));
        Assert.Equal((WorkAudience)3, above);
        Assert.False(Enum.IsDefined(above));
        Assert.Null(Enum.GetName(above));

        Assert.True(Enum.TryParse<WorkAudience>("-1", out var below));
        Assert.Equal((WorkAudience)(-1), below);
        Assert.False(Enum.IsDefined(below));

        Assert.True(Enum.TryParse<WorkAudience>("2147483647", out var top));
        Assert.Equal((WorkAudience)int.MaxValue, top);
        Assert.False(Enum.IsDefined(top));

        Assert.False(Enum.TryParse<WorkAudience>("bot", out _));
        Assert.True(Enum.TryParse<WorkAudience>("bot", ignoreCase: true, out var forgiven));
        Assert.Equal(WorkAudience.Bot, forgiven);
        Assert.True(Enum.TryParse<WorkAudience>("TEAM", ignoreCase: true, out var whole));
        Assert.Equal(WorkAudience.Team, whole);
    }

    // ----- the surface the enum declares

    /// <summary>The whole of what <see cref="WorkAudience" /> declares itself, as the running assembly holds it:
    /// an <see cref="int" /> underneath, sealed and no set, and exactly its three audiences and nothing else -
    /// each one a public static literal of its own type, in the order the declaration writes them, on the number
    /// it stands on; no instance member of its own, so an audience carries nothing beyond the number it is.</summary>
    [Fact]
    public void TheEnumDeclaresItsThreeAudiencesAndNothingElse()
    {
        var type = typeof(WorkAudience);

        Assert.True(type.IsEnum);
        Assert.True(type.IsValueType);
        Assert.True(type.IsSealed);
        Assert.False(type.IsPrimitive);

        // The only instance member an audience carries of its own is the storage the compiler gives every value
        // type: the <c>int</c> the enum is of, and nothing else.
        var instance = type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        Assert.Single(instance);
        Assert.Equal("value__", instance[0].Name);
        Assert.Equal(typeof(int), ((FieldInfo)instance[0]).FieldType);

        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);

        Assert.Equal(["All", "Bot", "Team"], fields.Select(field => field.Name));
        Assert.Equal([0, 1, 2], fields.Select(field => (int)field.GetRawConstantValue()!));
        Assert.All(fields, field =>
        {
            Assert.Equal(type, field.FieldType);
            Assert.True(field.IsPublic);
            Assert.True(field.IsLiteral);
            Assert.False(field.IsInitOnly);
        });
    }

    // ----- the format and the flags of an audience

    /// <summary>Every audience says its own word, by name or by number, in the format letters the enum answers to
    /// - the single and the full name, the number in decimal and in hexadecimal - and a value the enum does not
    /// name says its number and its bits, <c>Bot</c> with <c>Team</c>, even though the enum is not a set and the
    /// value is no audience; a letter the enum answers to no one is refused. The two format letters the enum no
    /// longer spells are asked by reflection only, so the build stays quiet about them.</summary>
    [Fact]
    public void EachAudienceFormatsByNameOrByNumber()
    {
        Assert.Equal("Bot", WorkAudience.Bot.ToString());
        Assert.Equal("Bot", WorkAudience.Bot.ToString("G"));
        Assert.Equal("Bot", WorkAudience.Bot.ToString("g"));
        Assert.Equal("Bot", WorkAudience.Bot.ToString("F"));
        Assert.Equal("Bot", WorkAudience.Bot.ToString("f"));
        Assert.Equal("Bot", WorkAudience.Bot.ToString((string?)null));
        Assert.Equal("Bot", WorkAudience.Bot.ToString(string.Empty));
        Assert.Equal("1", WorkAudience.Bot.ToString("D"));
        Assert.Equal("1", WorkAudience.Bot.ToString("d"));
        Assert.Equal("00000001", WorkAudience.Bot.ToString("X"));
        Assert.Equal("00000001", WorkAudience.Bot.ToString("x"));

        Assert.Equal("0", WorkAudience.All.ToString("D"));
        Assert.Equal("00000000", WorkAudience.All.ToString("X"));
        Assert.Equal("2", WorkAudience.Team.ToString("D"));
        Assert.Equal("00000002", WorkAudience.Team.ToString("X"));

        var combined = (WorkAudience)3;
        Assert.Equal("3", combined.ToString());
        Assert.Equal("3", combined.ToString("G"));
        Assert.Equal("3", combined.ToString("D"));
        Assert.Equal("00000003", combined.ToString("X"));
        Assert.Equal("Bot, Team", combined.ToString("F"));
        Assert.Throws<FormatException>(() => WorkAudience.Bot.ToString("Q"));

        Assert.Equal("Bot", typeof(WorkAudience).GetMethod("ToString", [typeof(IFormatProvider)])!
            .Invoke(WorkAudience.Bot, [CultureInfo.InvariantCulture]));
        Assert.Equal("Bot", typeof(WorkAudience).GetMethod("ToString", [typeof(string), typeof(IFormatProvider)])!
            .Invoke(WorkAudience.Bot, ["G", CultureInfo.InvariantCulture]));
    }

    /// <summary>An audience's code and its flags follow its number: it is of <see cref="TypeCode.Int32" />, and
    /// its flag tests ask what the number it stands on has, so <see cref="WorkAudience.All" />, standing on 0, is a
    /// flag of every audience, and the combined number of <c>Bot</c> and <c>Team</c> carries every flag; a flag
    /// that is no audience at all is refused with <see cref="ArgumentNullException" />, and a flag of no
    /// audience's type is refused with <see cref="ArgumentException" />. The two refusals are asked through
    /// reflection, because a direct call with a mismatched flag type is CA2248, and the invocation's
    /// <see cref="TargetInvocationException" /> wraps the exception the refusal really throws.</summary>
    [Fact]
    public void HasFlagAndGetTypeCodeFollowTheNumber()
    {
        Assert.Equal(TypeCode.Int32, WorkAudience.All.GetTypeCode());
        Assert.Equal(TypeCode.Int32, WorkAudience.Bot.GetTypeCode());
        Assert.Equal(TypeCode.Int32, WorkAudience.Team.GetTypeCode());

        Assert.True(WorkAudience.Bot.HasFlag(WorkAudience.Bot));
        Assert.False(WorkAudience.Bot.HasFlag(WorkAudience.Team));
        Assert.False(WorkAudience.All.HasFlag(WorkAudience.Bot));
        Assert.True(WorkAudience.Bot.HasFlag(WorkAudience.All));
        Assert.True(WorkAudience.Team.HasFlag(WorkAudience.All));

        var combined = (WorkAudience)3;
        Assert.True(combined.HasFlag(WorkAudience.Bot));
        Assert.True(combined.HasFlag(WorkAudience.Team));
        Assert.True(combined.HasFlag(WorkAudience.All));

        var nullFlag = Assert.Throws<TargetInvocationException>(() => typeof(WorkAudience).GetMethod(nameof(Enum.HasFlag))!.Invoke(WorkAudience.Bot, [null]));
        Assert.IsType<ArgumentNullException>(nullFlag.InnerException);

        var otherFlag = Assert.Throws<TargetInvocationException>(() => typeof(WorkAudience).GetMethod(nameof(Enum.HasFlag))!.Invoke(WorkAudience.Bot, [DayOfWeek.Monday]));
        Assert.IsType<ArgumentException>(otherFlag.InnerException);
    }
}
