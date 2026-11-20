using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="ActivityKind" /> - the eight words a line of the
/// page is one of, and the list that names them, at the ends the tests of the lines leave open.
/// <para>
/// What is pinned where it belongs: <see cref="DeploymentActivityTests" /> pins <see cref="ActivityKind.All" /> in
/// the order of the urgency the words are in - the order these tests do not repeat here - and pins the lines that
/// carry the words, word for word; <see cref="RuntimePayloadBuilderTests" /> pins a line of the runtime payload
/// that carries a kind.
/// </para>
/// <para>
/// Left open by all of that, and pinned here: the eight words, as the fixed values they are - each is the word the
/// file says it in, letter for letter; each is one word and nothing else - not the empty word, not a word of
/// nothing but whitespace, nothing before it or after it, no letter in it but a lowercase letter of the alphabet,
/// the word in its upper case or with its first letter capitalized being no word the class says; no two of the
/// eight say the same word, in any case, the two words of a freeze differing by a single letter and standing apart
/// for it; and <see cref="ActivityKind.All" />, the very list it is every time it is asked - eight words, the eight,
/// and no word twice, every one of them a word. And the public surface of the class, the eight words as the fixed
/// values they are and their order and nothing else to call, so that the day a ninth word is added the list says
/// so: the page cannot be shown a kind nobody decided what it means.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: a word is a word, and no moment is
/// in any of the eight.
/// </para>
/// </summary>
public class ActivityKindEdgeCaseTests
{
    /// <summary>The eight words, in the order the class says them: the urgency, most first.</summary>
    private static readonly string[] Kinds =
        [ActivityKind.Waiting, ActivityKind.Deploying, ActivityKind.Queued, ActivityKind.Frozen, ActivityKind.Freeze, ActivityKind.Deployed, ActivityKind.Failed, ActivityKind.Canceled];

    // ------------------------------------------------------- the words, as the fixed values they are

    /// <summary>Each of the eight is the word the file says it in, letter for letter: the words the page reads the
    /// file in, and the words it compares the file's word of a kind against.</summary>
    [Fact]
    public void TheEightWordsAreTheEightWordsTheFileSaysThemIn()
    {
        Assert.Equal("waiting", ActivityKind.Waiting);
        Assert.Equal("deploying", ActivityKind.Deploying);
        Assert.Equal("queued", ActivityKind.Queued);
        Assert.Equal("frozen", ActivityKind.Frozen);
        Assert.Equal("freeze", ActivityKind.Freeze);
        Assert.Equal("deployed", ActivityKind.Deployed);
        Assert.Equal("failed", ActivityKind.Failed);
        Assert.Equal("canceled", ActivityKind.Canceled);
    }

    // ---------------------------------------------------------- null/empty: a word is one word, lowercase

    /// <summary>Every one of the eight is a word: not the empty word, not a word of nothing but whitespace, nothing
    /// before it or after it, one word only and no letter in it but a lowercase letter of the alphabet. The word in
    /// its upper case, or the word with its first letter capitalized, is no word the class says - the case the words
    /// are in is part of the fixed value, since the file says them in it and the page compares in it.</summary>
    [Fact]
    public void EachWordIsOneLowercaseWordAndNoOther()
    {
        Assert.All(Kinds, word =>
        {
            Assert.False(string.IsNullOrEmpty(word));
            Assert.False(string.IsNullOrWhiteSpace(word));
            Assert.Equal(word, word.Trim());
            Assert.Single(word.Split(' ', '-'));
            Assert.All(word, letter => Assert.True(char.IsAsciiLetter(letter) && letter == char.ToLowerInvariant(letter)));
            Assert.NotEqual(word, word.ToUpperInvariant());
            Assert.NotEqual(word, string.Concat(char.ToUpperInvariant(word[0]), word[1..]));
        });
    }

    // ------------------------------------------------------------ the boundary: no two words are one

    /// <summary>No two of the eight say the same word, in any case: each is its own of a set, the set holding all
    /// eight apart whether the case is looked at or not. The two words of a freeze - the one in force and the one
    /// that comes - differ by a single letter and are still two words, as the two words of a deployment that ended
    /// and the words of the ones that have not.</summary>
    [Fact]
    public void NoTwoWordsAreTheSameWord()
    {
        Assert.Equal(8, Kinds.Length);
        Assert.Equal(8, new HashSet<string>(Kinds, StringComparer.Ordinal).Count);
        Assert.Equal(8, new HashSet<string>(Kinds, StringComparer.OrdinalIgnoreCase).Count);

        for (var first = 0; first < Kinds.Length; first++)
        {
            for (var second = first + 1; second < Kinds.Length; second++)
            {
                Assert.NotEqual(Kinds[first], Kinds[second]);
                Assert.NotEqual(Kinds[first], Kinds[second], StringComparer.OrdinalIgnoreCase);
            }
        }

        Assert.NotEqual(ActivityKind.Frozen, ActivityKind.Freeze);
        Assert.NotEqual(ActivityKind.Deployed, ActivityKind.Deploying);
    }

    // -------------------------------------------------------- All: the same list, the eight, once each

    /// <summary><see cref="ActivityKind.All" /> is the very list it is every time it is asked, and it is the eight
    /// words and no word twice: the list and the eight constants are the same eight, the order the words are in
    /// being pinned where the urgency is pinned. Every word of the list is a word: not the empty word, not a word
    /// of nothing but whitespace.</summary>
    [Fact]
    public void AllIsTheEightWordsOnceEachAndTheSameListEveryTime()
    {
        var all = ActivityKind.All;

        Assert.NotNull(all);
        Assert.Same(all, ActivityKind.All);
        Assert.Equal(8, all.Count);
        Assert.Equal(8, new HashSet<string>(all, StringComparer.Ordinal).Count);
        Assert.Equal(new HashSet<string>(Kinds, StringComparer.Ordinal), new HashSet<string>(all, StringComparer.Ordinal));

        Assert.All(all, word =>
        {
            Assert.False(string.IsNullOrEmpty(word));
            Assert.False(string.IsNullOrWhiteSpace(word));
        });
    }

    // --------------------------------------------------------------- the class's own shape

    /// <summary>The whole public surface of the class, as it stands: the eight words, as the fixed values they are,
    /// and their order - and nothing else to call. The words are the fixed values they are: constants of the type
    /// of a word, given once and not given again. Their order is the list that has the words the page compares
    /// against, with nothing to write to it. A word added to the class has to be written here as well, which is the
    /// point: the page cannot be shown a kind nobody decided what it means.</summary>
    [Fact]
    public void TheSurfaceIsTheEightWordsAndTheirOrder()
    {
        Assert.Equal(
            ["All", "Canceled", "Deployed", "Deploying", "Failed", "Freeze", "Frozen", "Queued", "Waiting"],
            PublicSurface());

        var fields = typeof(ActivityKind).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);

        Assert.Equal(8, fields.Length);
        Assert.All(fields, field =>
        {
            Assert.True(field.IsLiteral);
            Assert.Equal(typeof(string), field.FieldType);
        });

        var all = typeof(ActivityKind).GetProperty("All", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)!;

        Assert.Equal(typeof(IReadOnlyList<string>), all.PropertyType);
        Assert.False(all.CanWrite);
        Assert.True(all.GetMethod!.IsPublic);
    }

    /// <summary>The public members the class declares itself: the eight words as the fixed values they are, and
    /// their order - no method to call and no instance of the class to build, the words standing on their own.</summary>
    private static List<string> PublicSurface() =>
        [.. typeof(ActivityKind)
            .GetMembers(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(member => member is FieldInfo
                             || member is PropertyInfo
                             || member is MethodInfo method && !method.IsSpecialName)
            .Select(member => member.Name)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];
}
