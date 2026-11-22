using System.Globalization;
using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// Boundary, null/empty and fixed-value tests of <see cref="ResourceHealthReading" />: the one line of one
/// resource in one run of the file of containers - the state the pipeline read, where it came from, when, and the
/// words Resource Health said beside it. Pinned here is the record itself: its four parts held exactly as given
/// and compared by value, the three fixed words its source is compared against, falling apart, the copy that
/// turns one part, its equality, hash and words, and the shape of its public surface. Finding a record by id, a
/// source the page does not know read as <c>none</c> and a summary cut to 200 characters are pinned in
/// <see cref="ResourceHealthTests" />; what the page's badge says, in <see cref="ResourceHealthTextEdgeCaseTests" />
/// and <see cref="ResourceHealthTextTests" />; the box's words, in <see cref="ResourceHealthWordsEdgeCaseTests" />.
/// Nothing here reaches the network and nothing asks a clock.
/// </summary>
public class ResourceHealthReadingEdgeCaseTests
{
    /// <summary>The state the page reads of one well resource, written out here rather than taken from the app.</summary>
    private const string Status = "available";

    /// <summary>Where that state came from - the page's own fixed word for a reading Resource Health answered.</summary>
    private const string Source = "resourceHealth";

    /// <summary>The words Resource Health said beside it.</summary>
    private const string Summary = "No incidents reported.";

    /// <summary>When the pipeline read it: the one moment the test brought, so that no clock is asked.</summary>
    private static readonly DateTimeOffset Read = new(2026, 10, 4, 21, 37, 0, TimeSpan.Zero);

    /// <summary>One reading, well: four parts none of which is another's.</summary>
    private static ResourceHealthReading Full => new(Status, Source, Read, Summary);

    // --------------------------------------- the three words the source is compared against

    /// <summary>The three fixed words a source is compared against, held as letters - not folded, lowered or
    /// trimmed - and the only declared words the type carries, public and static: no fourth fixed word.</summary>
    [Fact]
    public void TheThreeSourceWordsAreTheFixedValuesThePageComparesAgainst()
    {
        Assert.Equal(["none", "provisioningState", "resourceHealth"],
            [ResourceHealthReading.None, ResourceHealthReading.FromProvisioningState, ResourceHealthReading.FromResourceHealth]);

        Assert.Equal(
            ["FromProvisioningState", "FromResourceHealth", "None"],
            typeof(ResourceHealthReading)
                .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Select(field => field.Name)
                .Order(StringComparer.Ordinal));
    }

    // --------------------------------------- the four parts, by the names of their own properties

    /// <summary>The constructor's four arguments, position by position, come back as the property of the same name,
    /// each held as the very string or moment it was given; the one part nobody says defaults to the nothing, not
    /// the empty word - a reading made without its words is not the reading whose words are the word of no letters.</summary>
    [Fact]
    public void TheConstructorHandsEachArgumentToThePropertyOfItsName()
    {
        var status = Own(Status);
        var source = Own(Source);
        var said = Own(Summary);
        var reading = new ResourceHealthReading(status, source, Read, said);

        Assert.Same(status, reading.Status);
        Assert.Same(source, reading.Source);
        Assert.Equal(Read, reading.CheckedAt);
        Assert.Same(said, reading.Summary);
        Assert.Equal((Status, Source, Read, Summary), (reading.Status, reading.Source, reading.CheckedAt, reading.Summary));

        var withoutWords = new ResourceHealthReading(status, source, Read);

        Assert.Null(withoutWords.Summary);
        Assert.Same(status, withoutWords.Status);
        Assert.Same(source, withoutWords.Source);
        Assert.Equal(Read, withoutWords.CheckedAt);
        Assert.NotEqual(reading, withoutWords);
        Assert.NotEqual(withoutWords, new ResourceHealthReading(status, source, Read, ""));
    }

    /// <summary>Either part that is a word is held in its exact letters - the word of no letters, a word of
    /// whitespace, a padded word, a word in another case, a word with a line break, a word of another language's
    /// letters - in the state, the source and the words alike. Nothing here trims, folds, lowers, cuts to 200
    /// characters or refuses: that is the reading of the file's, pinned in <see cref="ResourceHealthTests" />.</summary>
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   \t ")]
    [InlineData("\n")]
    [InlineData("available")]
    [InlineData("AVAILABLE")]
    [InlineData(" available")]
    [InlineData("available ")]
    [InlineData("degraded")]
    [InlineData("unavailable")]
    [InlineData("succeeded")]
    [InlineData("unknown")]
    [InlineData("resourceHealth")]
    [InlineData("resourcehealth")]
    [InlineData("provisioningState")]
    [InlineData("none")]
    [InlineData("guess")]
    [InlineData("—")]
    [InlineData("n/a")]
    [InlineData("İyi günler")]
    [InlineData("a\r\nb")]
    public void APartThatIsAWordIsHeldExactlyAsItIsGiven(string word)
    {
        var reading = new ResourceHealthReading(word, word, Read, word);

        Assert.Same(word, reading.Status);
        Assert.All(new[] { reading.Status, reading.Source, reading.Summary }, part => Assert.Equal(word, part));
        Assert.Equal(word.Length, reading.Status.Length);
    }

    /// <summary>A part of any length is held whole: the record cuts nothing, so a summary past the 200 characters
    /// the reading of the file cuts its to is held as the one word it is, and one more letter is another reading.</summary>
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(40)]
    [InlineData(199)]
    [InlineData(200)]
    [InlineData(201)]
    [InlineData(1_000)]
    public void APartOfAnyLengthIsHeldWholeAndCutByNothing(int length)
    {
        var word = new string('x', length);
        var reading = new ResourceHealthReading(word, word, Read, word);

        Assert.Same(word, reading.Status);
        Assert.All(new[] { reading.Status, reading.Source, reading.Summary }, part => Assert.Equal(length, part!.Length));
        Assert.Equal(reading, new ResourceHealthReading(new string('x', length), word, Read, word));
        Assert.NotEqual(reading, new ResourceHealthReading(new string('x', length + 1), word, Read, word));
    }

    /// <summary>The moment a reading holds stands at either edge of the calendar and at any offset: the earliest
    /// and the latest instant the calendar says, a moment a thousandth of a second short of its second, a moment
    /// told twelve hours behind the one the test brought and fourteen ahead of it - all held as the very values
    /// they are, offset included. The same instant told in another offset is another value to the record, and a
    /// tick newer is another reading.</summary>
    [Fact]
    public void AReadingHoldsItsMomentAtEitherEdgeOfTheCalendar()
    {
        var oldest = DateTimeOffset.MinValue;
        var newest = DateTimeOffset.MaxValue;
        var subSecond = new DateTimeOffset(2026, 10, 4, 21, 37, 0, 999, TimeSpan.Zero);
        var behind = new DateTimeOffset(2026, 10, 4, 21, 37, 0, TimeSpan.FromHours(-12));
        var ahead = new DateTimeOffset(2026, 10, 4, 21, 37, 0, TimeSpan.FromHours(14));

        foreach (var at in new[] { oldest, newest, subSecond, behind, ahead, Read })
        {
            var reading = new ResourceHealthReading(Status, Source, at);

            Assert.Equal(at, reading.CheckedAt);
            Assert.Equal(at.Offset, reading.CheckedAt.Offset);
            Assert.Equal(at.Ticks, reading.CheckedAt.Ticks);
            Assert.Equal(Status, reading.Status);
            Assert.Equal(Source, reading.Source);
        }

        var sameInstant = Read.ToOffset(TimeSpan.FromHours(1));

        Assert.Equal(Read, Read.ToOffset(sameInstant.Offset));
        Assert.Equal(new ResourceHealthReading(Status, Source, Read), new ResourceHealthReading(Status, Source, sameInstant));
        Assert.NotEqual(new ResourceHealthReading(Status, Source, Read), new ResourceHealthReading(Status, Source, Read + TimeSpan.FromTicks(1)));
        Assert.NotEqual(new ResourceHealthReading(Status, Source, oldest), new ResourceHealthReading(Status, Source, newest));
    }

    /// <summary>The one part that may be not there at all is kept as that, and it is not the empty word: nothing,
    /// the empty word and a word of whitespace are three different readings in a set of their own, and the record
    /// refuses nothing - it replaces no absent word with a dash, an <c>n/a</c> or an invented one.</summary>
    [Fact]
    public void AWordsPartThatIsNotGivenIsKeptAsTheNothingItIs()
    {
        var nothing = new ResourceHealthReading(Status, Source, Read);
        var empty = new ResourceHealthReading(Status, Source, Read, "");
        var blank = new ResourceHealthReading(Status, Source, Read, "   ");

        Assert.Null(nothing.Summary);
        Assert.Equal("", empty.Summary);
        Assert.Equal("   ", blank.Summary);
        Assert.NotEqual(nothing, empty);
        Assert.NotEqual(empty, blank);
        Assert.NotEqual(nothing, blank);
        Assert.True(nothing.Equals((object)new ResourceHealthReading(Status, Source, Read)));
        Assert.Equal(3, new HashSet<ResourceHealthReading> { nothing, empty, blank }.Count);
    }

    // ------------------------------------------ the four parts coming out again, and the copy that turns one

    /// <summary>The four parts come back in the order they went in - state, source, moment, words - asked for at
    /// once in a pattern or one after the other, and what came out goes back in as the same reading; the words that
    /// were not said fall apart as the nothing they are.</summary>
    [Fact]
    public void TheFourPartsComeOutAgainInTheOrderTheyWentIn()
    {
        var (status, source, at, said) = Full;

        Assert.Equal((Status, Source, Read, Summary), (status, source, at, said));
        Assert.Equal(Full, new ResourceHealthReading(status, source, at, said));

        Full.Deconstruct(out var outStatus, out var outSource, out var outAt, out var outSaid);

        Assert.Same(Full.Status, outStatus);
        Assert.Same(Full.Source, outSource);
        Assert.Equal(Read, outAt);
        Assert.Same(Full.Summary!, outSaid);

        var (noStatus, noSource, noAt, noSaid) = new ResourceHealthReading(Status, Source, Read);

        Assert.Null(noSaid);
        Assert.Equal(new ResourceHealthReading(Status, Source, Read), new ResourceHealthReading(noStatus, noSource, noAt, noSaid));
    }

    /// <summary>Either of the four parts may be turned as the reading is made; the parts not named stay the very
    /// values the reading was built of, and a copy turned back on is the reading in another reference - never the
    /// same one. A word turned to nothing is the nothing, not the empty word.</summary>
    [Fact]
    public void APartTurnedAsTheReadingIsMadeStandsWhereItIsTurnedTo()
    {
        var said = Own(Summary);
        var reading = new ResourceHealthReading(Status, Source, Read, said);

        var provisioned = reading with { Source = "provisioningState", Status = "succeeded" };

        Assert.Equal("provisioningState", provisioned.Source);
        Assert.Equal("succeeded", provisioned.Status);
        Assert.Same(said, provisioned.Summary);
        Assert.Equal(Read, reading.CheckedAt);
        Assert.NotEqual(reading, provisioned);
        Assert.NotSame(reading, provisioned);

        var aged = reading with { CheckedAt = Read + TimeSpan.FromHours(3) };

        Assert.Equal(Read + TimeSpan.FromHours(3), aged.CheckedAt);
        Assert.Same(said, aged.Summary);
        Assert.NotEqual(reading, aged);

        var emptied = reading with { Summary = null! };

        Assert.Null(emptied.Summary);
        Assert.NotEqual(emptied, new ResourceHealthReading(Status, Source, Read, ""));

        var repacked = reading with { Status = reading.Status, Source = reading.Source, CheckedAt = reading.CheckedAt, Summary = reading.Summary };

        Assert.Equal(reading, repacked);
        Assert.NotSame(reading, repacked);
        Assert.Same(said, repacked.Summary);
    }

    // ----------------------------------------------- the four parts, the equality and the hash

    /// <summary>Two readings are one while their four parts are one, by any of the four ways of asking: the
    /// record's own <c>Equals</c>, the one of <see cref="object" />, <c>==</c> and <c>!=</c>; the hash follows the
    /// equality, boxed or unboxed, and values are compared, not references - two readings made of no shared string
    /// are one reading and one entry of a set.</summary>
    [Fact]
    public void TwoReadingsAreOneWhileTheirFourPartsAre()
    {
        var one = Full;
        var two = new ResourceHealthReading(Status, Source, Read, Summary);

        Assert.True(one.Equals(two));
        Assert.True(one.Equals((object)two));
        Assert.True(one == two);
        Assert.False(one != two);
        Assert.NotSame(one, two);
        Assert.Equal(one.GetHashCode(), two.GetHashCode());
        Assert.Equal(one.GetHashCode(), ((object)one).GetHashCode());
        Assert.Single(new HashSet<ResourceHealthReading> { one, two });

        var writtenAgain = new ResourceHealthReading(Own(Status), Own(Source), Read, Own(Summary));

        Assert.NotSame(one.Status, writtenAgain.Status);
        Assert.Equal(one, writtenAgain);
        Assert.Equal(one.GetHashCode(), writtenAgain.GetHashCode());
        Assert.Single(new HashSet<ResourceHealthReading> { one, two, writtenAgain });
    }

    /// <summary>A difference in any one of the four parts - the state, the source, a tick of the moment, one letter
    /// of a word, the words turned to nothing, the case of a word - or the two parts exchanged, is another reading;
    /// a set of readings that differ one part each holds them all apart.</summary>
    [Fact]
    public void ReadingsThatDifferInOnePartAreTwo()
    {
        var one = Full;

        Assert.NotEqual(one, new ResourceHealthReading("degraded", Source, Read, Summary));
        Assert.NotEqual(one, new ResourceHealthReading("unavailable", Source, Read, Summary));
        Assert.NotEqual(one, new ResourceHealthReading(Status, "provisioningState", Read, Summary));
        Assert.NotEqual(one, new ResourceHealthReading(Status, "NONE", Read, Summary));
        Assert.NotEqual(one, new ResourceHealthReading(Status, Source, Read, Summary + "."));
        Assert.NotEqual(one, new ResourceHealthReading(Status, Source, Read, Summary[..^1]));
        Assert.NotEqual(one, new ResourceHealthReading(Status, Source, Read, null!));
        Assert.NotEqual(one, new ResourceHealthReading(Status, Source, Read + TimeSpan.FromTicks(1), Summary));
        Assert.NotEqual(one, new ResourceHealthReading(Source, Status, Read, Summary));
        Assert.Equal(
            5,
            new HashSet<ResourceHealthReading>
            {
                one,
                one with { Status = "degraded" },
                one with { Source = "provisioningState" },
                one with { CheckedAt = Read + TimeSpan.FromHours(1) },
                one with { Summary = "We're sorry, your key vault is degraded." },
            }.Count);
    }

    /// <summary>No reading and no reading are one; a reading and no reading are two, from either side; and the
    /// reading says no to a word it is not, to a bare number and to the anonymous thing that happens to carry its
    /// four parts - the members every record is asked with, asked with the nothing.</summary>
    [Fact]
    public void NoReadingIsOneWithNoReadingAndNotWithAnyOtherThing()
    {
        ResourceHealthReading? nothing = null;
        var there = Full;

        Assert.True(nothing == (ResourceHealthReading?)null);
        Assert.False(nothing == there);
        Assert.True(nothing != there);
        Assert.True(there != (ResourceHealthReading?)null);
        Assert.False(there.Equals(nothing));
        Assert.False(there.Equals((object?)null));
        Assert.Null(default(ResourceHealthReading));
        Assert.Null(((ResourceHealthReading?)null)?.Summary);

        Assert.False(there.Equals(there.ToString()));
        Assert.False(there.Equals((object)40));
        Assert.False(there.Equals(new { Status = Status, Source = Source, CheckedAt = Read, Summary = Summary }));
        Assert.Equal(2, new HashSet<ResourceHealthReading?> { there, nothing, null }.Count);
    }

    // ------------------------------------------------------------------ the words the record says itself

    /// <summary>The words the reading says its four parts in: its own name at the front, then state, source,
    /// moment and words in the order the constructor names them, each part as the value it is - whitespace, tab
    /// and line break and all - and a part nobody said as the empty place. The moment alone follows a reader: the
    /// day is spelled in the reader's letters, the rest stands, and a reading is said the same under the same
    /// reader - no reference or hash slips into the saying.</summary>
    [Fact]
    public void TheRecordSaysItsFourPartsInTheOrderTheyAreNamed()
    {
        var invariant = Under(CultureInfo.InvariantCulture, () => Full.ToString())!;

        Assert.Equal(
            string.Create(CultureInfo.InvariantCulture, $"ResourceHealthReading {{ Status = {Status}, Source = {Source}, CheckedAt = {Read:MM/dd/yyyy HH:mm:ss zzz}, Summary = {Summary} }}"),
            invariant);
        Assert.Equal(
            string.Create(CultureInfo.InvariantCulture, $"ResourceHealthReading {{ Status = {Status}, Source = {Source}, CheckedAt = {Read:MM/dd/yyyy HH:mm:ss zzz}, Summary =  }}"),
            Under(CultureInfo.InvariantCulture, () => new ResourceHealthReading(Status, Source, Read).ToString())!);
        Assert.Equal(
            string.Create(CultureInfo.InvariantCulture, $"ResourceHealthReading {{ Status =   , Source = \t, CheckedAt = {Read:MM/dd/yyyy HH:mm:ss zzz}, Summary = \n }}"),
            Under(CultureInfo.InvariantCulture, () => new ResourceHealthReading("  ", "\t", Read, "\n").ToString())!);
        Assert.Equal(
            string.Create(CultureInfo.InvariantCulture, $"ResourceHealthReading {{ Status = , Source = , CheckedAt = {Read:MM/dd/yyyy HH:mm:ss zzz}, Summary =  }}"),
            Under(CultureInfo.InvariantCulture, () => new ResourceHealthReading("", "", Read).ToString())!);

        Assert.Equal(invariant, Under(CultureInfo.InvariantCulture, () => new ResourceHealthReading(Status, Source, Read, Summary).ToString()));

        foreach (var reader in new[] { new CultureInfo("de-DE"), new CultureInfo("tr-TR") })
        {
            var said = Under(reader, () => Full.ToString())!;
            Assert.StartsWith($"ResourceHealthReading {{ Status = {Status}, Source = {Source}, CheckedAt = ", said, StringComparison.Ordinal);
            Assert.EndsWith($", Summary = {Summary} }}", said, StringComparison.Ordinal);
            Assert.NotEqual(invariant, said);
        }

        Assert.Contains($"CheckedAt = {Read:dd.MM.yyyy HH:mm:ss zzz}, Summary = {Summary}", Under(new CultureInfo("de-DE"), () => Full.ToString())!, StringComparison.Ordinal);
    }

    // ------------------------------------------------------------------- the reading's declared shape

    /// <summary>The whole public surface, as it stands: the four parts in the order the constructor names them -
    /// state, source, moment, words - the three fixed words, and the members every record is asked, and nothing
    /// else to call; all four parts init-only; the moment the only part that is not a word; no zone, no address,
    /// no wait - the reading of the file is declared but not public. A fifth part added to the record has to be
    /// written here as well, which is the point.</summary>
    [Fact]
    public void TheReadingHasItsFourPartsAndTheMembersEveryRecordIsAsked()
    {
        var type = typeof(ResourceHealthReading);

        Assert.True(type.IsSealed);
        Assert.False(type.IsValueType);
        Assert.Equal([typeof(IEquatable<ResourceHealthReading>)], type.GetInterfaces());
        Assert.Equal(typeof(object), type.BaseType);

        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).ToList();

        Assert.Equal(["Status", "Source", "CheckedAt", "Summary"], properties.Select(property => property.Name));
        Assert.Equal(typeof(DateTimeOffset), type.GetProperty("CheckedAt")!.PropertyType);
        Assert.Equal(3, properties.Count(property => property.PropertyType == typeof(string)));
        Assert.All(properties, property =>
        {
            Assert.True(property.CanRead);
            Assert.True(property.CanWrite);
            Assert.True(property.GetMethod!.IsPublic);
            Assert.Contains(typeof(System.Runtime.CompilerServices.IsExternalInit), property.SetMethod!.ReturnParameter.GetRequiredCustomModifiers());
        });

        Assert.Equal(
            ["CheckedAt", "Deconstruct", "Equals", "FromProvisioningState", "FromResourceHealth", "GetHashCode", "None", "Source", "Status", "Summary", "ToString", "op_Equality", "op_Inequality"],
            PublicSurface());

        var constructor = Assert.Single(type.GetConstructors(BindingFlags.Public | BindingFlags.Instance));
        var arguments = constructor.GetParameters();

        Assert.Equal(["Status", "Source", "CheckedAt", "Summary"], arguments.Select(argument => argument.Name));
        Assert.Equal([typeof(string), typeof(string), typeof(DateTimeOffset), typeof(string)], arguments.Select(argument => argument.ParameterType));
        Assert.True(arguments[3].HasDefaultValue);
        Assert.Null(arguments[3].DefaultValue);

        var copy = Assert.Single(type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance));
        Assert.Equal(type, copy.GetParameters().Single().ParameterType);
        Assert.Equal(2, type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Count(method => method.Name == "Equals"));
        var parts = type.GetMethod("Deconstruct")!;
        Assert.Equal(["Status", "Source", "CheckedAt", "Summary"], parts.GetParameters().Select(parameter => parameter.Name));
        Assert.All(parts.GetParameters(), parameter => Assert.True(parameter.IsOut));
    }

    // -------------------------------------------------------------------------- the machinery

    /// <summary>The public members the reading declares itself: its properties, the three fixed words, and the
    /// methods that are not the get and the set of one - the two operators - the record's compiler-named clone
    /// and the reading of the file walked past.</summary>
    private static List<string> PublicSurface() =>
        [.. typeof(ResourceHealthReading)
            .GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(member => member is PropertyInfo
                             || member is FieldInfo
                             || member is MethodInfo method
                             && (!method.IsSpecialName || method.Name.StartsWith("op_", StringComparison.Ordinal)))
            .Where(member => !member.Name.Contains('<', StringComparison.Ordinal))
            .Select(member => member.Name)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];

    /// <summary>The letters of a word gathered into a string of its own, which no other word in the test shares a
    /// reference with: what the record does with a word it is given is only seen when the two are apart.</summary>
    private static string Own(string word) => new(word.ToCharArray());

    /// <summary>Asking a reading for a part, its words or its hash under another reader, and handing the thread's
    /// own culture back afterwards, whatever the asking did.</summary>
    private static T Under<T>(CultureInfo culture, Func<T> what)
    {
        ArgumentNullException.ThrowIfNull(culture);
        ArgumentNullException.ThrowIfNull(what);

        var reader = Thread.CurrentThread.CurrentCulture;
        try
        {
            Thread.CurrentThread.CurrentCulture = culture;
            return what();
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = reader;
        }
    }
}
