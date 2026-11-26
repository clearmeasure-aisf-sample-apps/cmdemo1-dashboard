using System.Text.Json;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of the three members of <c>src/Dashboard/Runtime/RuntimePayload.cs</c>
/// that do something with what they are given rather than only hold it: <see cref="RuntimeTileLine.Of" />, which makes
/// a line's words out of its pieces, <see cref="RuntimeLink.To" />, which makes a link's address out of a
/// <see cref="Uri" />, and <see cref="RuntimePayload.ToJson" />, which makes the bytes the script reads.
/// <para>
/// What the other files already pin is left alone. <see cref="RuntimePayloadEdgeCaseTests" /> pins every part of every
/// record, the lists held rather than copied, the members every record is asked and the public surface of each, a
/// line of pieces keeping the very list and trend it is handed, a line of no pieces, the refusal of no list, the
/// address a <see cref="Uri" /> escapes and normalises, and a relative address refused.
/// <see cref="RuntimePayloadBoundaryEdgeCaseTests" /> pins the escapes of every record, the bytes to a German reader,
/// the heights that are no finite number, a list holding nothing in a slot, a thousand boxes, a piece of no words, the
/// address forms a builder never makes, and an update read back. <see cref="LinkTests" /> pins that the pieces are kept
/// only when one leads and that no address is no link.
/// </para>
/// <para>
/// What follows is what those leave open: that a line's words are worked out once, when it is made, and not again when
/// its pieces change; that the words are joined as they stand, with no space, no trimming and no Unicode normalising;
/// that the tone is handed over as it stands, nothing included; that a link of no address and no words is still a link
/// that keeps the pieces; that a trend goes with a line that keeps no pieces; that a list holding nothing in a slot is
/// no line; that a link keeps a port that is not the scheme's own and the case of an escape as it was written; and that
/// two calls of <see cref="RuntimePayload.ToJson" /> are two strings of the same bytes.
/// </para>
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: every value below is one the test brought.
/// </para>
/// </summary>
public class RuntimePayloadFactoryEdgeCaseTests
{
    private const string Portal = "https://portal.azure.com/#@tenant/resource/sub/rg/app";

    /// <summary>A link that leads nowhere and says nothing, which is still a link and not the absence of one.</summary>
    private static readonly RuntimeLink EmptyLink = new(string.Empty, string.Empty);

    private static readonly RuntimeLink ToPortal = new(Portal, "The resource in the Azure portal");

    /// <summary>A line written out, as the script reads it inside the one tile it belongs to.</summary>
    private static string Bytes(RuntimeTileLine line)
    {
        var tile = new RuntimeTile("a", "neutral", "N", null, [line], null, "t");
        using var json = JsonDocument.Parse(new RuntimePayload([tile], [], []).ToJson());
        return json.RootElement.GetProperty("nodes")[0].GetProperty("lines")[0].GetRawText();
    }

    // ------------------------------------------------------------------------------- RuntimeTileLine.Of

    /// <summary>A line's words are what its pieces say, then and later: it holds the very list it was handed, so a list
    /// of pieces that changes afterwards changes the pieces and the words together - the line never says one thing and
    /// carries another.</summary>
    [Fact]
    public void ALinesWordsFollowItsPiecesWhenThePiecesChange()
    {
        var parts = new List<RuntimeTextPart> { new("12 req/min", ToPortal), new(" · p95 85 ms") };
        var line = RuntimeTileLine.Of("plain", parts);

        Assert.Equal("12 req/min · p95 85 ms", line.Text);

        parts.Add(new RuntimeTextPart(" · 3 errors"));
        parts[0] = new RuntimeTextPart("13 req/min");

        Assert.Equal("13 req/min · p95 85 ms · 3 errors", line.Text);
        Assert.Same(parts, line.Parts);
        Assert.Equal(3, line.Parts!.Count);
        Assert.Null(line.Parts[0].Link);
    }

    /// <summary>The words are joined exactly as the pieces say them: no space between them, no trimming of the ones at
    /// either end, a letter and the accent after it in another piece left as two characters rather than put together,
    /// and the two halves of one emoji put back into the one emoji.</summary>
    [Fact]
    public void TheWordsAreJoinedAsThePiecesSayThemAndNothingElse()
    {
        Assert.Equal("ab", RuntimeTileLine.Of("plain", [new RuntimeTextPart("a"), new RuntimeTextPart("b")]).Text);
        Assert.Equal("  a  b  ", RuntimeTileLine.Of("plain", [new RuntimeTextPart("  a "), new RuntimeTextPart(" b  ")]).Text);
        Assert.Equal("\n\t", RuntimeTileLine.Of("plain", [new RuntimeTextPart("\n"), new RuntimeTextPart("\t")]).Text);

        var accented = RuntimeTileLine.Of("plain", [new RuntimeTextPart("e"), new RuntimeTextPart("́")]).Text;

        Assert.Equal("é", accented);
        Assert.Equal(2, accented.Length);
        Assert.NotEqual("é", accented);

        var emoji = RuntimeTileLine.Of("plain", [new RuntimeTextPart("\uD83D"), new RuntimeTextPart("\uDE00")]).Text;

        Assert.Equal("\U0001F600", emoji);
        Assert.Equal("\U0001F600 ok", RuntimeTileLine.Of("plain", [new RuntimeTextPart("\U0001F600"), new RuntimeTextPart(" ok")]).Text);
    }

    /// <summary>The tone is handed to the line as it stands: no tone at all is a line of no tone rather than one of the
    /// default <c>plain</c> a caller of the constructor gets, an empty tone is an empty tone, and a tone in capitals is
    /// another tone - and a line of no tone is written without one.</summary>
    [Fact]
    public void TheToneIsHandedOverAsItStandsNothingIncluded()
    {
        var none = RuntimeTileLine.Of(null!, [new RuntimeTextPart("x")]);

        Assert.Null(none.Tone);
        Assert.NotEqual(new RuntimeTileLine("x"), none);
        Assert.Equal(new RuntimeTileLine("x", null!), none);
        Assert.Equal("""{"text":"x"}""", Bytes(none));

        Assert.Equal(string.Empty, RuntimeTileLine.Of(string.Empty, []).Tone);
        Assert.Equal("PLAIN", RuntimeTileLine.Of("PLAIN", []).Tone);
        Assert.NotEqual(RuntimeTileLine.Of("plain", []), RuntimeTileLine.Of("PLAIN", []));
        Assert.Equal(" waiting ", RuntimeTileLine.Of(" waiting ", []).Tone);
    }

    /// <summary>A piece leads somewhere when it has a link, whatever the link says: a link of no address and no words
    /// keeps the pieces as surely as a link to the portal does, while a piece of words alone does not.</summary>
    [Fact]
    public void ALinkOfNoAddressAndNoWordsStillKeepsThePieces()
    {
        var parts = new List<RuntimeTextPart> { new("a"), new("b", EmptyLink) };
        var line = RuntimeTileLine.Of("plain", parts);

        Assert.Same(parts, line.Parts);
        Assert.Equal("ab", line.Text);
        Assert.Equal(
            """{"text":"ab","tone":"plain","parts":[{"text":"a"},{"text":"b","link":{"href":"","title":""}}]}""",
            Bytes(line));
    }

    /// <summary>The trend goes with the line whether or not the pieces do: a line of pieces none of which leads keeps
    /// no pieces and still carries the very trend it was handed, and it never has marks.</summary>
    [Fact]
    public void ATrendGoesWithALineThatKeepsNoPieces()
    {
        var trend = new RuntimeTrend([0, 0.5, 1], "Requests per minute, last 3 checks: 0 to 40, now 40");
        var line = RuntimeTileLine.Of("plain", [new RuntimeTextPart("40 req/min")], trend);

        Assert.Null(line.Parts);
        Assert.Same(trend, line.Trend);
        Assert.Null(line.Marks);
        Assert.Equal(new RuntimeTileLine("40 req/min", "plain", null, trend), line);
        Assert.Equal(
            """{"text":"40 req/min","tone":"plain","trend":{"points":[0,0.5,1],"title":"Requests per minute, last 3 checks: 0 to 40, now 40"}}""",
            Bytes(line));

        Assert.Same(trend, RuntimeTileLine.Of("plain", [], trend).Trend);
    }

    /// <summary>A list of pieces that holds nothing in a slot is no line, and the refusal is the caller's fault named
    /// as the missing list is: <c>parts</c>, with the slot - whichever slot it stands in. The constructor, which is
    /// handed its pieces as they are, holds such a list and says what the other pieces say.</summary>
    [Fact]
    public void AListOfPiecesWithNothingInASlotIsNoLine()
    {
        Assert.Equal("parts", Assert.Throws<ArgumentException>(() => RuntimeTileLine.Of("plain", [null!])).ParamName);
        Assert.Contains("at 1", Assert.Throws<ArgumentException>(() => RuntimeTileLine.Of("plain", [new RuntimeTextPart("a", ToPortal), null!])).Message, StringComparison.Ordinal);
        Assert.Contains("at 0", Assert.Throws<ArgumentException>(() => RuntimeTileLine.Of("plain", [null!, new RuntimeTextPart("a", ToPortal)])).Message, StringComparison.Ordinal);

        var held = new RuntimeTileLine("a", "plain", [null!]);

        Assert.Single(held.Parts!);
        Assert.Null(held.Parts![0]);
        Assert.Equal(string.Empty, held.Text);
    }

    // ---------------------------------------------------------------------------------- RuntimeLink.To

    /// <summary>A link keeps a port that is not the scheme's own and drops the one that is, keeps the case of an
    /// escape as the address wrote it, and keeps a query of no value and a query of no key as they stand.</summary>
    [Fact]
    public void ALinkKeepsWhatTheUriKeepsOfAnAddress()
    {
        Assert.Equal("https://a.example.net:8443/x", RuntimeLink.To(new Uri("https://a.example.net:8443/x"), "a")!.Href);
        Assert.Equal("http://a.example.net:443/x", RuntimeLink.To(new Uri("http://a.example.net:443/x"), "a")!.Href);
        Assert.Equal("http://a.example.net/x", RuntimeLink.To(new Uri("http://a.example.net:80/x"), "a")!.Href);
        Assert.Equal("https://a.example.net/x?q=a%2bb", RuntimeLink.To(new Uri("https://a.example.net/x?q=a%2bb"), "a")!.Href);
        Assert.Equal("https://a.example.net/x?flag", RuntimeLink.To(new Uri("https://a.example.net/x?flag"), "a")!.Href);
        Assert.Equal("https://a.example.net/x?=1&", RuntimeLink.To(new Uri("https://a.example.net/x?=1&"), "a")!.Href);
        Assert.Equal("https://a.example.net/x?", RuntimeLink.To(new Uri("https://a.example.net/x?"), "a")!.Href);
    }

    /// <summary>The address is the one the <see cref="Uri" /> says at the moment the link is made, and the link is a
    /// new one each time: the words of one address handed to two calls make two links that are one, and the words
    /// of two addresses that say the same thing make one link.</summary>
    [Fact]
    public void TwoAddressesThatSayTheSameThingMakeOneLink()
    {
        var spelled = RuntimeLink.To(new Uri("HTTPS://PORTAL.AZURE.COM:443/#@tenant/resource/sub/rg/app"), "portal");
        var plain = RuntimeLink.To(new Uri(Portal), "portal");

        Assert.Equal(plain, spelled);
        Assert.Equal(Portal, spelled!.Href);
        Assert.NotEqual(plain, RuntimeLink.To(new Uri(Portal), "Portal"));
    }

    // ------------------------------------------------------------------------------- RuntimePayload.ToJson

    /// <summary>Each call of <see cref="RuntimePayload.ToJson" /> writes the update anew: two calls are two strings of
    /// the same bytes, and the update of nothing at all is three empty lists.</summary>
    [Fact]
    public void EachCallWritesTheUpdateAnew()
    {
        var update = new RuntimePayload([], [], []);
        var first = update.ToJson();
        var second = update.ToJson();

        Assert.Equal("""{"nodes":[],"regions":[],"edges":[]}""", first);
        Assert.Equal(first, second);
        Assert.NotSame(first, second);
    }

    /// <summary>The bytes are written in one line with no space between a key and its value, nor after a comma - the
    /// smallest form, which the script parses the same as an indented one.</summary>
    [Fact]
    public void TheBytesAreWrittenInOneLineWithNoSpaces()
    {
        var json = new RuntimePayload(
            [new RuntimeTile("a", "healthy", "Healthy", null, [], ["healthy", "healthy"], "t")],
            [new RuntimeRegionMark("r", "serving", "serving")],
            [],
            [new RuntimeName("n", ToPortal)]).ToJson();

        Assert.Equal(
            """{"nodes":[{"alias":"a","state":"healthy","label":"Healthy","lines":[],"history":["healthy","healthy"],"title":"t"}],"regions":[{"alias":"r","state":"serving","label":"serving"}],"edges":[],"names":[{"alias":"n","nameLink":{"href":"https://portal.azure.com/#@tenant/resource/sub/rg/app","title":"The resource in the Azure portal"}}]}""",
            json);
        Assert.DoesNotContain('\n', json);
        Assert.DoesNotContain(": ", json, StringComparison.Ordinal);
        Assert.DoesNotContain(", ", json, StringComparison.Ordinal);
    }
}
