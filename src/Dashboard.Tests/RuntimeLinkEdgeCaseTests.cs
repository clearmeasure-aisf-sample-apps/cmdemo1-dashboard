using System.Text.Json;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="RuntimeLink" /> that the other files leave open.
/// <para>
/// What is already pinned is left alone: <see cref="RuntimePayloadEdgeCaseTests" /> pins the two parts, equality,
/// the hash, the falling apart, the words of <c>ToString</c> (also of a link of nothing), the public surface, the JSON
/// of an empty link, the normalising of an address by <see cref="Uri" />, a title in more than one line, a title of
/// null and of nothing, and the refusal of a relative address; <see cref="RuntimePayloadBoundaryEdgeCaseTests" /> pins
/// a host with no path, escapes, an empty fragment, an IPv6 zone, the schemes that are not http, a padded title, two
/// calls over one address, <c>with { }</c>, and <c>==</c>/<c>!=</c> against null; <see cref="RuntimePayloadFactoryEdgeCaseTests" />
/// pins ports and queries; <see cref="LinkTextEdgeCaseTests" /> pins the sentence of the refusal.
/// </para>
/// <para>
/// What follows is the rest: the JSON of a link one of whose parts is null, an address with a user, a host outside
/// the Latin alphabet, a very long address, a <see cref="Uri" /> made with <see cref="UriKind.RelativeOrAbsolute" />,
/// <c>with</c> changing one part and keeping the other, and the hash and equality of a link of nulls. Nothing here
/// reaches the network or asks a clock.
/// </para>
/// </summary>
public class RuntimeLinkEdgeCaseTests
{
    private const string Href = "https://octopus.example.net/app#/Spaces-1/tasks/ServerTasks-1";
    private const string Title = "The deployment task in Octopus Deploy (opens in a new tab; Octopus Deploy asks you to sign in)";

    /// <summary>The link the writer gave back inside a tile, as the script reads it.</summary>
    private static JsonElement Written(RuntimeLink link)
    {
        var tile = new RuntimeTile("a", "neutral", "Neutral", null, [], null, "t", Link: link);
        using var json = JsonDocument.Parse(new RuntimePayload([tile], [], []).ToJson());
        return json.RootElement.GetProperty("nodes")[0].GetProperty("link").Clone();
    }

    private static string HrefOf(Uri address) => Assert.IsType<RuntimeLink>(RuntimeLink.To(address, "a")).Href;

    // ----- The JSON of a part that is null -----

    /// <summary>The writer leaves out nothing at all, and that holds for each part of a link on its own: a link of no
    /// title is an address alone, a link of no address is words alone, and a link of neither is an empty object -
    /// while an empty string is still written (pinned elsewhere), so null and empty are told apart.</summary>
    [Fact]
    public void ThePartOfALinkThatIsNullIsLeftOutOfTheJson()
    {
        Assert.Equal($$"""{"href":"{{Href}}"}""", Written(new RuntimeLink(Href, null!)).GetRawText());
        Assert.Equal("""{"title":"t"}""", Written(new RuntimeLink(null!, "t")).GetRawText());
        Assert.Equal("{}", Written(new RuntimeLink(null!, null!)).GetRawText());
        Assert.Equal("""{"href":"","title":""}""", Written(new RuntimeLink(string.Empty, string.Empty)).GetRawText());
    }

    /// <summary>The two parts are written in the order the record declares them and by their camel-cased names.</summary>
    [Fact]
    public void ALinkIsWrittenAsHrefThenTitle()
    {
        Assert.Equal(["href", "title"], Written(new RuntimeLink(Href, Title)).EnumerateObject().Select(property => property.Name));
        Assert.Equal(Title, Written(new RuntimeLink(Href, Title)).GetProperty("title").GetString());
        Assert.Equal(Href, Written(new RuntimeLink(Href, Title)).GetProperty("href").GetString());
    }

    // ----- Addresses at the ends -----

    /// <summary>The user and the password an address carries are kept: the link is the address the manifest wrote, and
    /// deciding what it may hold is the manifest's business, not this record's.</summary>
    [Fact]
    public void ALinkKeepsTheUserAnAddressCarries() =>
        Assert.Equal("https://user:secret@a.example.net/x", HrefOf(new Uri("https://user:secret@a.example.net/x")));

    /// <summary>A host outside the Latin alphabet is handed on as <see cref="Uri.AbsoluteUri" /> says it.</summary>
    [Fact]
    public void AHostOutsideTheLatinAlphabetIsTheOneTheUriSays()
    {
        var address = new Uri("https://bücher.example/x");

        Assert.Equal(address.AbsoluteUri, HrefOf(address));
        Assert.EndsWith("/x", HrefOf(address), StringComparison.Ordinal);
    }

    /// <summary>An address of thousands of letters is kept whole: there is no length the record cuts it at.</summary>
    [Fact]
    public void AVeryLongAddressIsKeptWhole()
    {
        var path = new string('p', 4_000);

        Assert.Equal($"https://a.example.net/{path}", HrefOf(new Uri($"https://a.example.net/{path}")));
    }

    /// <summary>What decides is whether the <see cref="Uri" /> is absolute, not the kind it was asked to be: an absolute
    /// address made with <see cref="UriKind.RelativeOrAbsolute" /> is a link, and a relative one made the same way is
    /// refused exactly as one made with <see cref="UriKind.Relative" />.</summary>
    [Fact]
    public void TheKindTheUriWasAskedToBeDoesNotDecide()
    {
        Assert.Equal("https://a.example.net/x", HrefOf(new Uri("https://a.example.net/x", UriKind.RelativeOrAbsolute)));
        Assert.Equal("https://a.example.net/x", HrefOf(new Uri("https://a.example.net/x", UriKind.Absolute)));
        Assert.Throws<InvalidOperationException>(() => RuntimeLink.To(new Uri("x/y", UriKind.RelativeOrAbsolute), "a"));
        Assert.Throws<InvalidOperationException>(() => RuntimeLink.To(new Uri(string.Empty, UriKind.Relative), "a"));
    }

    // ----- with, and a link of nulls -----

    /// <summary><c>with</c> changes the part it names and keeps the other, and changing a part back gives the link it
    /// came from.</summary>
    [Fact]
    public void WithChangesThePartItNamesAndKeepsTheOther()
    {
        var link = new RuntimeLink(Href, Title);

        var moved = link with { Href = "https://a.example.net/" };
        var retitled = link with { Title = "t" };

        Assert.Equal(Title, moved.Title);
        Assert.Equal("https://a.example.net/", moved.Href);
        Assert.Equal(Href, retitled.Href);
        Assert.Equal("t", retitled.Title);
        Assert.Equal(link, moved with { Href = Href });
        Assert.Equal(link, retitled with { Title = Title });
        Assert.Equal(Href, link.Href);
        Assert.Equal(Title, link.Title);
    }

    /// <summary>A link of nulls is hashed and weighed without a <see cref="NullReferenceException" />: two of them are
    /// one link, one slot in a set, and unlike a link of empty strings.</summary>
    [Fact]
    public void ALinkOfNullsIsOneLinkAndNotALinkOfEmptyStrings()
    {
        var one = new RuntimeLink(null!, null!);
        var two = new RuntimeLink(null!, null!);
        var empty = new RuntimeLink(string.Empty, string.Empty);

        Assert.Equal(one, two);
        Assert.Equal(one.GetHashCode(), two.GetHashCode());
        Assert.Single(new HashSet<RuntimeLink> { one, two });
        Assert.NotEqual(one, empty);
        Assert.Equal(2, new HashSet<RuntimeLink> { one, empty }.Count);
        Assert.NotEqual(new RuntimeLink(Href, null!), new RuntimeLink(null!, Href));
    }

    /// <summary><see cref="RuntimeLink.To" /> with a title of null and an address of no path makes a link whose
    /// address gets the root and whose words stay null - and writes only the address.</summary>
    [Fact]
    public void ToOfNoWordsWritesTheAddressAlone()
    {
        var link = Assert.IsType<RuntimeLink>(RuntimeLink.To(new Uri("https://a.example.net"), null!));

        Assert.Equal(new RuntimeLink("https://a.example.net/", null!), link);
        Assert.Equal("""{"href":"https://a.example.net/"}""", Written(link).GetRawText());
    }
}
