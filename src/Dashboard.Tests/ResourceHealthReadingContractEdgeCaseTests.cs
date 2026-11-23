using System.Globalization;
using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests still open for <see cref="ResourceHealthReading" />, the one line of
/// one resource of <c>containers.json</c>, declared in <c>src/Dashboard/Health/ResourceHealth.cs</c>: the four parts
/// handed the nothing rather than a word, the three fixed words as the compile-time literals they are, the reading
/// weighed as the <see cref="IEquatable{T}" /> it declares and asked through its own members taken by name, the
/// equality and the hash read as letters under readers that would read those letters differently, and the words the
/// record says of a part that is not there at all.
/// <para>
/// The name this issue asked for is held: <see cref="ResourceHealthReadingEdgeCaseTests" /> already pins the four
/// parts held exactly as given at both ends of the calendar and of any length, the default of the words, the copy that
/// turns one part, the equality and the hash of two readings and of readings that differ in one part, the nothing and
/// the empty word and a word of whitespace kept apart as three readings, the saying of the four parts in the order
/// they are named under three readers, and the whole declared surface - and all of them stand over
/// <see cref="ResourceHealthTests" /> (a reading found by id, a source the page does not know, a summary cut to 200)
/// and <see cref="ResourceHealthReadEdgeCaseTests" /> (the letters and the length a status of the file may be). None
/// of it is repeated, weakened or replaced here; only what all of them leave unread is set down, in a file of its own,
/// as <see cref="SummaryLevelSurfaceEdgeCaseTests" /> stands beside <c>SummaryLevelEdgeCaseTests</c>.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: every reading, word, moment, reader, key
/// and method is one the test brings.
/// </para>
/// </summary>
public class ResourceHealthReadingContractEdgeCaseTests
{
    /// <summary>The state the page reads of one resource, written out rather than taken from the app.</summary>
    private const string Status = "available";

    /// <summary>Where that state came from - the page's fixed word for a reading Resource Health answered.</summary>
    private const string Source = "resourceHealth";

    /// <summary>The words Resource Health said beside it.</summary>
    private const string Summary = "No incidents reported.";

    /// <summary>When the pipeline read it: the one moment the test brought, so that no clock is asked.</summary>
    private static readonly DateTimeOffset Read = new(2026, 10, 4, 21, 37, 0, TimeSpan.Zero);

    /// <summary>The one reading of them all: four parts, none of which is another's.</summary>
    private static ResourceHealthReading Full => new(Status, Source, Read, Summary);

    /// <summary>The reader the record's words are measured in here: the one that never moves a number's point.</summary>
    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

    /// <summary>Three readers that spell a moment otherwise, one of which pairs the letters of a word otherwise too -
    /// which is exactly what the equality of a reading is asked not to follow.</summary>
    private static readonly CultureInfo[] Readers = [new CultureInfo("de-DE"), new CultureInfo("tr-TR"), new CultureInfo("en-US")];

    /// <summary>The states and the sources the page names. Any one of them is a word the record holds as its state as
    /// readily as the word <c>available</c>, and as its source, and the two parts are never read as one another.</summary>
    private static readonly string[] Words = ["available", "degraded", "unavailable", "unknown", "succeeded", "failed", "none", "provisioningState"];

    /// <summary>A reading whose three word parts are the nothing: one of the two ends the record is asked about here,
    /// the record refusing neither this nor the reading whose parts are the word of no letters.</summary>
    private static ResourceHealthReading Nothing => new(null!, null!, Read);

    /// <summary>A reading whose three word parts are the word of no letters, the other end of that asking.</summary>
    private static ResourceHealthReading Empty => new("", "", Read, "");

    /// <summary>The three parts of a reading that are a word, and so may be handed the nothing: the moment is the
    /// fourth and holds its own, whatever is handed to it.</summary>
    private static readonly string[] WordParts = ["Status", "Source", "Summary"];

    /// <summary>The three fixed words the record declares, which a reading may be handed as any part of itself.</summary>
    private static readonly string[] FixedWords = [ResourceHealthReading.FromResourceHealth, ResourceHealthReading.FromProvisioningState, ResourceHealthReading.None];

    /// <summary>The two fixed words the page spells with a capital inside, which the record never lowers.</summary>
    private static readonly string[] Camel = [ResourceHealthReading.FromResourceHealth, ResourceHealthReading.FromProvisioningState];

    /// <summary>The four shapes of one letter, which a reader pairs two of and the record keeps four.</summary>
    private static readonly ResourceHealthReading[] Letters =
    [
        new("i", Source, Read, "i"),
        new("I", Source, Read, "i"),
        new("ı", Source, Read, "i"),
        new("İ", Source, Read, "i"),
    ];

    // --------------------------------- the three fixed words, as the literals the compiler folds them into

    /// <summary>The three fixed words of the record, held in the letters the page compares against: fourteen,
    /// seventeen and four of them, with no space inside and none at either end, each beginning on a lower-case letter,
    /// and no two of the three one word - neither read as letters nor careless of their case. They are literals
    /// rather than fields written once at the start of the run: the only public static fields the record declares,
    /// each a literal <see cref="string" /> and none of them <see cref="FieldInfo.IsInitOnly" />, each answering
    /// <see cref="FieldInfo.GetRawConstantValue" /> with its own value. Being literals, they stand as the labels of a
    /// switch - and that switch reads their letters exactly: the page's own word raised, spelled without its capital,
    /// padded or emptied answers as no word the page knows.</summary>
    [Fact]
    public void TheThreeFixedWordsAreTheOnlyConstantsTheReadingDeclares()
    {
        Assert.Equal("resourceHealth", ResourceHealthReading.FromResourceHealth);
        Assert.Equal("provisioningState", ResourceHealthReading.FromProvisioningState);
        Assert.Equal("none", ResourceHealthReading.None);
        Assert.Equal(
            [14, 17, 4],
            [ResourceHealthReading.FromResourceHealth.Length, ResourceHealthReading.FromProvisioningState.Length, ResourceHealthReading.None.Length]);

        Assert.All(FixedWords, word =>
        {
            Assert.Equal(word, word.Trim());
            Assert.DoesNotContain(" ", word, StringComparison.Ordinal);
            Assert.False(char.IsUpper(word[0]));
            Assert.All(word.ToCharArray(), character => Assert.True(char.IsAsciiLetter(character)));
        });

        Assert.Equal(3, FixedWords.Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(3, FixedWords.Distinct(StringComparer.OrdinalIgnoreCase).Count());

        var fields = Fields();

        Assert.Equal(["FromProvisioningState", "FromResourceHealth", "None"], fields.Select(field => field.Name).Order(StringComparer.Ordinal).ToArray());
        Assert.All(fields, field =>
        {
            Assert.Equal(typeof(string), field.FieldType);
            Assert.True(field.IsLiteral);
            Assert.True(field.IsStatic);
            Assert.True(field.IsPublic);
            Assert.False(field.IsInitOnly);
            Assert.Equal(field.GetValue(null), field.GetRawConstantValue());
            Assert.Contains((string)field.GetRawConstantValue()!, FixedWords, StringComparer.Ordinal);
        });

        // A label of a switch takes a constant, so this answers at all only because the three are literals - and it
        // answers by their very letters, as the app's own switch over a reading's source does.
        Assert.Equal(["health", "provisioned", "nothing"], FixedWords.Select(Answered));
        Assert.Equal("nothing", Answered(ResourceHealthReading.FromResourceHealth.ToUpperInvariant()));
        Assert.Equal("nothing", Answered("resourcehealth"));
        Assert.Equal("nothing", Answered("ResourceHealth"));
        Assert.Equal("nothing", Answered(""));
        Assert.Equal("nothing", Answered(" resourceHealth"));
        Assert.Equal("nothing", Answered(ResourceHealthReading.None.ToUpperInvariant()));
    }

    /// <summary>The word the page answers with for a source: the fixed word it knows and every other word it does
    /// not, read off the three constants as the labels of a switch.</summary>
    private static string Answered(string source) =>
        source switch
        {
            ResourceHealthReading.FromResourceHealth => "health",
            ResourceHealthReading.FromProvisioningState => "provisioned",
            _ => "nothing",
        };

    // ------------------------------------------- either word part handed the nothing, and kept as that

    /// <summary>Neither the state, the source nor the words is checked by the record: handed the nothing, each comes
    /// back as the nothing from its property, from the pattern, from <c>Deconstruct</c> and from
    /// <see cref="PropertyInfo.GetValue(object)" /> - never as the empty word, never a dash, never a refusal, and the
    /// moment the reading was made with stands where it was put. A reading of nothing parts is a reading like any
    /// other: it weighs itself, it hashes, it holds its own place in a set apart from the reading whose parts are the
    /// word of no letters, and turning all three words of that one to the nothing makes the reading of nothing parts.</summary>
    [Fact]
    public void AWordPartHandedTheNothingIsHeldAsTheNothingItWasHanded()
    {
        var nothing = Nothing;
        var empty = Empty;

        Assert.Null(nothing.Status);
        Assert.Null(nothing.Source);
        Assert.Null(nothing.Summary);
        Assert.Equal(Read, nothing.CheckedAt);

        var (status, source, at, said) = nothing;

        Assert.Null(status);
        Assert.Null(source);
        Assert.Null(said);
        Assert.Equal(Read, at);

        nothing.Deconstruct(out var outStatus, out var outSource, out var outAt, out var outSaid);

        Assert.Null(outStatus);
        Assert.Null(outSource);
        Assert.Null(outSaid);
        Assert.Equal(Read, outAt);

        Assert.All(WordParts, name =>
            Assert.Null(typeof(ResourceHealthReading).GetProperty(name)!.GetValue(nothing)));
        Assert.Equal(Read, (DateTimeOffset)typeof(ResourceHealthReading).GetProperty("CheckedAt")!.GetValue(nothing)!);

        Assert.Equal(nothing, new ResourceHealthReading(null!, null!, Read, null));
        Assert.Equal(nothing.GetHashCode(), new ResourceHealthReading(null!, null!, Read, null).GetHashCode());
        Assert.True(nothing == nothing with { });
        Assert.False(nothing != nothing with { });
        Assert.NotEqual(nothing, empty);
        Assert.NotEqual(empty, nothing);
        Assert.Equal(2, new HashSet<ResourceHealthReading> { nothing, nothing with { }, empty }.Count);
        Assert.Equal(3, new HashSet<ResourceHealthReading> { nothing, empty, new ResourceHealthReading(" ", " ", Read, " ") }.Count);

        var turned = empty with { Status = null!, Source = null!, Summary = null! };

        Assert.Equal(nothing, turned);
        Assert.NotSame(empty, turned);
        Assert.Null((Full with { Status = null! }).Status);
        Assert.Null((Full with { Source = null! }).Source);
        Assert.Null((Full with { Summary = null! }).Summary);
        Assert.NotEqual(Full with { Summary = null! }, Full with { Summary = "" });
        Assert.Equal(Full.GetHashCode(), (Full with { Status = Status }).GetHashCode());
    }

    /// <summary>A reading of nothing parts says itself in the very words of a reading whose parts are the word of no
    /// letters: the nothing and the empty word are said the same, so the saying is no spelling of the parts and two
    /// readings that say one thing are still two readings. The saying holds the parts as they stand - a brace, a
    /// comma, an equals sign and a line break come out raw, in the order the constructor names them - with the
    /// record's own name at the front and no namespace, no hash and no reference of the object anywhere in it.</summary>
    [Fact]
    public void TheReadingSaysTheNothingExactlyAsItSaysTheEmptyWord()
    {
        var nothing = Under(Invariant, () => Nothing.ToString())!;
        var empty = Under(Invariant, () => Empty.ToString())!;

        Assert.Equal(Said(null, null, null), nothing);
        Assert.Equal(Said("", "", ""), empty);
        Assert.Equal(nothing, empty);
        Assert.Equal(nothing, Under(Invariant, () => new ResourceHealthReading(null!, null!, Read, "").ToString()));
        Assert.NotEqual(Nothing, Empty);

        Assert.Equal(Said("{", "}", ", \n"), Under(Invariant, () => new ResourceHealthReading("{", "}", Read, ", \n").ToString())!);
        Assert.Equal(Said(Status, Source, Summary), Under(Invariant, () => Full.ToString())!);
        Assert.Equal(Said(null, Status, null), Under(Invariant, () => new ResourceHealthReading(null!, Status, Read).ToString())!);

        Assert.StartsWith("ResourceHealthReading { ", nothing, StringComparison.Ordinal);
        Assert.EndsWith(" }", nothing, StringComparison.Ordinal);
        Assert.Equal($"ResourceHealthReading {{ Status = , Source = , CheckedAt = {Said(Read)}, Summary =  }}", nothing);
        Assert.DoesNotContain("Dashboard.Health", nothing, StringComparison.Ordinal);
        Assert.DoesNotContain("@", nothing, StringComparison.Ordinal);
        Assert.DoesNotContain("0x", nothing, StringComparison.Ordinal);
    }

    /// <summary>The moment of the saying, spelled as the record spells it and not as a test of these words guesses:
    /// the one moment the reading holds, in the invariant reader, told to the second and the offset.</summary>
    private static string Said(DateTimeOffset at) => string.Create(Invariant, $"{at:MM/dd/yyyy HH:mm:ss zzz}");

    // -------------------------------- the reading weighed as the IEquatable it declares, and by its own members

    /// <summary>The reading is one with itself and with its copies by every way of asking: by its own typed
    /// <c>Equals</c>, by the one of <see cref="object" />, by <c>==</c>, by the <see cref="IEquatable{T}" />
    /// reference it is declared to be - both ways round, and by three in a row - and its hash stands over five askings
    /// of the same reading, so that the reading is one key of a dictionary whichever of its equal copies opens the
    /// door. The typed <c>Equals</c> is the very body of the interface's, and the one of <see cref="object" /> a
    /// second method beside it: asked by name, each answers what the reading answers to its own - yes to its copy, no
    /// to the nothing, to a word and to a bare number. A dictionary of readings refuses the nothing for a key with an
    /// <see cref="ArgumentNullException" /> rather than answering no, while a reading of nothing parts hashes well
    /// enough to be a key and stands in the same dictionary as the reading of empty parts.</summary>
    [Fact]
    public void AReadingIsOneWithItselfAndWithEveryCopyOfItselfByEveryWayOfAsking()
    {
        var one = Full;
        var two = new ResourceHealthReading(Own(Status), Own(Source), Read, Own(Summary));
        var three = two with { Status = Status };

        Assert.True(one.Equals(one));

        // Asked of the very same reading through a second name, since the compiler refuses to let a variable be
        // compared with itself in place: the operators answer for one reading as they answer for two names of it.
        ResourceHealthReading itself = one;

        Assert.True(one == itself);
        Assert.False(one != itself);
        Assert.True(((IEquatable<ResourceHealthReading>)one).Equals(one));
        Assert.True(one.Equals(two) && two.Equals(one));
        Assert.True(((object)one).Equals(two) && ((object)two).Equals(one));
        Assert.True(one.Equals(three) && two.Equals(three) && one.Equals((object)three));
        Assert.False(one != two);
        Assert.False(two != one);

        var hashes = Enumerable.Range(0, 5).Select(_ => one.GetHashCode()).ToArray();
        Assert.All(hashes, hash => Assert.Equal(two.GetHashCode(), hash));
        Assert.Equal(((object)one).GetHashCode(), ((object)two).GetHashCode());

        var keyed = new Dictionary<ResourceHealthReading, string> { [one] = "the vault" };

        Assert.Equal("the vault", keyed[two]);
        Assert.Equal("the vault", keyed[three with { }]);
        Assert.False(keyed.ContainsKey(new ResourceHealthReading(Status, Source, Read)));
        Assert.Throws<ArgumentNullException>(() => keyed.ContainsKey(null!));
        Assert.Single(keyed);

        keyed[Nothing] = "no word at all";
        keyed[Empty] = "a word of no letters";

        Assert.Equal(3, keyed.Count);
        Assert.Equal("no word at all", keyed[new ResourceHealthReading(null!, null!, Read, null)]);
        Assert.NotEqual(keyed[Nothing], keyed[Empty]);
        Assert.Single(keyed.Keys, key => key.Status is null);
        Assert.Single(keyed.Values, value => value == "the vault");

        var map = typeof(ResourceHealthReading).GetInterfaceMap(typeof(IEquatable<ResourceHealthReading>));

        Assert.Equal("Equals", Assert.Single(map.InterfaceMethods).Name);
        var body = Assert.Single(map.TargetMethods)!;
        var typed = typeof(ResourceHealthReading).GetMethod("Equals", [typeof(ResourceHealthReading)])!;
        var boxed = typeof(ResourceHealthReading).GetMethod("Equals", [typeof(object)])!;

        Assert.Same(typed, body);
        Assert.NotSame(boxed, body);
        Assert.True((bool)typed.Invoke(one, [two])!);
        Assert.True((bool)boxed.Invoke(one, [two])!);
        Assert.False((bool)typed.Invoke(one, [null])!);
        Assert.False((bool)boxed.Invoke(one, [null])!);
        Assert.False((bool)boxed.Invoke(one, [Status])!);
        Assert.False((bool)boxed.Invoke(one, [(object)40])!);
        Assert.Equal(one.GetHashCode(), (int)typeof(ResourceHealthReading).GetMethod("GetHashCode", Type.EmptyTypes)!.Invoke(one, [])!);
        Assert.Equal(Under(Invariant, () => one.ToString()), (string?)typeof(ResourceHealthReading).GetMethod("ToString", Type.EmptyTypes)!.Invoke(one, []));

        var deconstruct = typeof(ResourceHealthReading).GetMethod("Deconstruct")!;
        var parts = new object?[] { null, null, null, null };

        Assert.Null(deconstruct.Invoke(one, parts));
        Assert.Equal(Status, Assert.IsType<string>(parts[0]));
        Assert.Equal(Source, Assert.IsType<string>(parts[1]));
        Assert.Equal(Read, Assert.IsType<DateTimeOffset>(parts[2]));
        Assert.Equal(Summary, Assert.IsType<string>(parts[3]));

        var nothingParts = new object?[] { "turn", "turn", DateTimeOffset.MinValue, "turn" };

        Assert.Null(deconstruct.Invoke(Nothing, nothingParts));
        Assert.Null(nothingParts[0]);
        Assert.Null(nothingParts[1]);
        Assert.Null(nothingParts[3]);
        Assert.Equal(Read, Assert.IsType<DateTimeOffset>(nothingParts[2]));
    }

    /// <summary>The two ways the page asks whether two readings are one, taken by their own names and called with the
    /// nothing on either side: <c>op_Equality</c> answers yes to the nothing and the nothing, to a reading and itself
    /// and to a reading and its copy, and no to the nothing and a reading from either hand; <c>op_Inequality</c>
    /// answers the other way to every one of those six. Both take two readings and answer with a
    /// <see cref="bool" />, and the two are the only public static methods the record declares - the same two
    /// <c>==</c> and <c>!=</c> compile down to, and they answer so for a reading of nothing parts as for any other.</summary>
    [Fact]
    public void TheTwoOperatorsAnswerForThemselvesWithTheNothingOnEitherSide()
    {
        var type = typeof(ResourceHealthReading);
        var equal = type.GetMethod("op_Equality", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)!;
        var notEqual = type.GetMethod("op_Inequality", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)!;
        var there = Full;
        var copy = new ResourceHealthReading(Status, Source, Read, Summary);
        var other = new ResourceHealthReading("degraded", Source, Read, Summary);

        Assert.True((bool)equal.Invoke(null, [null, null])!);
        Assert.False((bool)equal.Invoke(null, [null, there])!);
        Assert.False((bool)equal.Invoke(null, [there, null])!);
        Assert.True((bool)equal.Invoke(null, [there, copy])!);
        Assert.True((bool)equal.Invoke(null, [there, there])!);
        Assert.False((bool)equal.Invoke(null, [there, other])!);

        Assert.False((bool)notEqual.Invoke(null, [null, null])!);
        Assert.True((bool)notEqual.Invoke(null, [null, there])!);
        Assert.True((bool)notEqual.Invoke(null, [there, null])!);
        Assert.False((bool)notEqual.Invoke(null, [there, copy])!);
        Assert.False((bool)notEqual.Invoke(null, [there, there])!);
        Assert.True((bool)notEqual.Invoke(null, [there, other])!);

        Assert.Equal(typeof(bool), equal.ReturnType);
        Assert.Equal(typeof(bool), notEqual.ReturnType);
        Assert.All(new[] { equal, notEqual }, method =>
        {
            Assert.True(method.IsStatic);
            Assert.True(method.IsPublic);
            Assert.Equal(
                [typeof(ResourceHealthReading), typeof(ResourceHealthReading)],
                method.GetParameters().Select(parameter => parameter.ParameterType).ToArray());
        });

        Assert.Equal(
            ["op_Equality", "op_Inequality"],
            type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).Select(method => method.Name).Order(StringComparer.Ordinal).ToArray());

        foreach (var reading in Words.Select(word => new ResourceHealthReading(word, word, Read, word)).Append(Nothing))
        {
            var twin = new ResourceHealthReading(reading.Status!, reading.Source!, reading.CheckedAt, reading.Summary);

            Assert.True((bool)equal.Invoke(null, [reading, twin])!);
            Assert.True((bool)notEqual.Invoke(null, [reading, null])!);
            Assert.True((bool)notEqual.Invoke(null, [reading, Empty])!);
        }
    }

    // --------------------------------------- letters, weighed as letters under any reader of them

    /// <summary>The equality and the hash of a reading read its words as letters and nothing else: the same asks under
    /// three readers answer as they do under the invariant one, a reading keeps its hash across all four readers, and
    /// the four shapes of the letter <c>i</c> - which a reader pairs two of - stay four readings, four keys of a set
    /// and four keys of a dictionary wherever the thread happens to stand. The nothing, the empty word and a word of
    /// spaces stay three readings under every reader too. Only the saying of the moment follows the reader; the parts
    /// of a reading stay the very parts it was handed, under a reader as under another.</summary>
    [Fact]
    public void TheReadingWeighsItsWordsAsLettersUnderEveryReader()
    {
        var one = Full;
        var two = new ResourceHealthReading(Own(Status), Own(Source), Read, Own(Summary));
        var invariantHash = one.GetHashCode();

        Assert.Equal(4, new HashSet<ResourceHealthReading>(Letters).Count);
        Assert.Equal(4, Letters.Select(reading => reading.Status).Distinct(StringComparer.Ordinal).Count());

        foreach (var reader in Readers)
        {
            Assert.True(Under(reader, () => one.Equals(two)));
            Assert.True(Under(reader, () => one == two));
            Assert.False(Under(reader, () => one != two));
            Assert.Equal(invariantHash, Under(reader, () => one.GetHashCode()));
            Assert.Equal(4, Under(reader, () => new HashSet<ResourceHealthReading>(Letters).Count));
            Assert.Equal(one, Under(reader, () => new ResourceHealthReading(Status, Source, Read, Summary)));
            Assert.Equal(Summary, Under(reader, () => one.Summary));

            var said = Under(reader, () => one.ToString())!;

            Assert.StartsWith($"ResourceHealthReading {{ Status = {Status}, Source = {Source}, CheckedAt = ", said, StringComparison.Ordinal);
            Assert.EndsWith($", Summary = {Summary} }}", said, StringComparison.Ordinal);

            foreach (var (first, second) in FromEachPair())
            {
                Assert.False(Under(reader, () => first.Equals(second)));
                Assert.False(Under(reader, () => second.Equals(first)));
                Assert.True(Under(reader, () => first != second));
                Assert.False(Under(reader, () => first.Equals((object)second)));
            }
        }

        Assert.NotEqual(Under(Invariant, () => one.ToString()), Under(new CultureInfo("de-DE"), () => one.ToString()));
        Assert.Equal(invariantHash, Under(new CultureInfo("de-DE"), () => one.GetHashCode()));
        Assert.Equal(invariantHash, Under(new CultureInfo("tr-TR"), () => two.GetHashCode()));

        var blank = new ResourceHealthReading(" ", " ", Read, " ");

        Assert.All(Readers, reader =>
            Assert.Equal(3, Under(reader, () => new HashSet<ResourceHealthReading> { Nothing, Empty, blank }.Count)));
    }

    /// <summary>The six pairs of the four readings of one letter, each asked of the record twice over: the record
    /// keeps them apart whichever way round they are handed to it, which is what the asks above are for.</summary>
    private static IEnumerable<(ResourceHealthReading First, ResourceHealthReading Second)> FromEachPair() =>
        from first in Letters
        from second in Letters
        where !ReferenceEquals(first, second)
        select (first, second);

    // ------------------------------ a word of the page's own vocabulary held as the state or as the source

    /// <summary>The eight words the page names its states and its sources with are held by the record as they are
    /// handed, in whichever part they are handed to: the same word as the state is not the same reading as the same
    /// word as the source, and the record refuses none of them - one of its own fixed words, held as the state, comes
    /// back as the state, since the record says nothing about what a state may be. Raised, lowered or cut to the word
    /// of no letters beside it, a word makes another reading rather than the same one told differently; eight words
    /// make eight readings. The cutting and the lowering of the file's words are the reading of the file, and are
    /// pinned in <see cref="ResourceHealthReadEdgeCaseTests" /> - this record hands back what it is handed.</summary>
    [Fact]
    public void AWordHeldAsTheStateIsNotTheSameWordHeldAsTheSource()
    {
        Assert.All(Words, word =>
        {
            var asState = new ResourceHealthReading(word, Source, Read);
            var asSource = new ResourceHealthReading(Status, word, Read);

            Assert.Equal(word, asState.Status);
            Assert.Equal(Source, asState.Source);
            Assert.Equal(word, asSource.Source);
            Assert.Equal(Status, asSource.Status);
            Assert.NotEqual(asState, asSource);
            Assert.NotEqual(asSource, asState);

            var raised = new ResourceHealthReading(word.ToUpperInvariant(), word.ToUpperInvariant(), Read, word);

            Assert.NotEqual(asState, raised);
            Assert.NotEqual(asSource, raised);
            Assert.Equal(word, new ResourceHealthReading(word, word, Read, word).Status);
            Assert.NotEqual(asState, new ResourceHealthReading(word, Source, Read, ""));
        });

        Assert.Equal(Words.Length, Words.Select(word => new ResourceHealthReading(word, word, Read, word)).ToHashSet(EqualityComparer<ResourceHealthReading>.Default).Count);

        // The two words the page spells with a capital are held with it: told lower, they are other readings, so the
        // letters a reading of the file lowered and trimmed are the letters this record was handed, and no others.
        Assert.All(Camel, word =>
        {
            Assert.NotEqual(word, word.ToLowerInvariant());
            Assert.NotEqual(
                new ResourceHealthReading(word, word, Read, word),
                new ResourceHealthReading(word.ToLowerInvariant(), word.ToLowerInvariant(), Read, word.ToLowerInvariant()));
            Assert.Equal(word, new ResourceHealthReading(word, word, Read, word).Source);
        });

        // And each of the three fixed words is held as the state, as the source and as the words alike, the record
        // saying nothing about which part a word of its own vocabulary belongs in.
        Assert.All(FixedWords, word =>
        {
            var held = new ResourceHealthReading(word, word, Read, word);

            Assert.Equal(word, held.Status);
            Assert.Equal(word, held.Source);
            Assert.Equal(word, held.Summary);
            Assert.NotEqual(held, new ResourceHealthReading(word, word, Read, ""));
            Assert.NotEqual(held, new ResourceHealthReading(word, word, Read.AddSeconds(1), word));
            Assert.All(FixedWords, other => Assert.Equal(other == word, held.Equals(new ResourceHealthReading(word, other, Read, word))));
        });
    }

    // -------------------------------------------------------------------------- the machinery

    /// <summary>The public literal fields the reading declares, which are its three fixed words and no others.</summary>
    private static FieldInfo[] Fields() =>
        typeof(ResourceHealthReading).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);

    /// <summary>The words the record says of three parts and the one moment the test brought, in the invariant reader:
    /// the nothing and the word of no letters are said the same, which is what the saying of the one is asked here.</summary>
    private static string Said(string? status, string? source, string? summary) =>
        $"ResourceHealthReading {{ Status = {status}, Source = {source}, CheckedAt = {Said(Read)}, Summary = {summary} }}";

    /// <summary>The letters of a word gathered into a string of its own, which no other word in the test shares a
    /// reference with: what the record does with a word it is given is only seen when the two are apart.</summary>
    private static string Own(string word) => new(word.ToCharArray());

    /// <summary>Asking a reading a part, its words or its hash under another reader, and handing the thread's own
    /// culture back afterwards, whatever the asking did.</summary>
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
