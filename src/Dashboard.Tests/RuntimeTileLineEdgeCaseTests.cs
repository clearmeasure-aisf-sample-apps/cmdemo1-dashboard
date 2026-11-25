using System.Text.Json;
using Dashboard.Runtime;

namespace Dashboard.Tests;

/// <summary>
/// Boundary, null/empty and fixed-value tests for <see cref="RuntimeTileLine" /> that the other runtime payload test
/// files leave open. <see cref="RuntimePayloadEdgeCaseTests" />, <see cref="RuntimePayloadFactoryEdgeCaseTests" /> and
/// <see cref="RuntimePayloadBoundaryEdgeCaseTests" /> already pin the constructor part by part, equality, <c>with</c>,
/// deconstruction, <c>ToString</c>, how <see cref="RuntimeTileLine.Of" /> joins words, passes on the tone and trend,
/// keeps or drops the pieces, and refuses no list. This file adds: every named tone through <see cref="RuntimeTileLine.Of" />
/// and into the JSON, the one-argument constructor written out, a line from <see cref="RuntimeTileLine.Of" /> equal to the
/// constructor line it stands for, an array of pieces kept as that same array, one piece used in several slots, and a
/// long line.
/// </summary>
public sealed class RuntimeTileLineEdgeCaseTests
{
    private static readonly RuntimeLink ToPortal = new("https://portal.azure.com/#@tenant/resource/x", "The resource in the Azure portal");

    /// <summary>A line written out the way the script reads it, inside the one tile it belongs to.</summary>
    private static string Written(RuntimeTileLine line)
    {
        var tile = new RuntimeTile("a", "neutral", "N", null, [line], null, "t");
        using var json = JsonDocument.Parse(new RuntimePayload([tile], [], []).ToJson());
        return json.RootElement.GetProperty("nodes")[0].GetProperty("lines")[0].GetRawText();
    }

    [Theory]
    [InlineData(RuntimeLineTone.Plain, "plain")]
    [InlineData(RuntimeLineTone.Muted, "muted")]
    [InlineData(RuntimeLineTone.Strong, "strong")]
    [InlineData(RuntimeLineTone.Serving, "serving")]
    [InlineData(RuntimeLineTone.Ok, "ok")]
    [InlineData(RuntimeLineTone.Warn, "warn")]
    [InlineData(RuntimeLineTone.InSync, "insync")]
    [InlineData(RuntimeLineTone.Differs, "differs")]
    [InlineData(RuntimeLineTone.Unknown, "unknown")]
    public void EveryNamedToneGoesThroughOfAndIntoTheJsonUnchanged(string tone, string word)
    {
        var line = RuntimeTileLine.Of(tone, [new RuntimeTextPart("x")]);

        Assert.Equal(word, line.Tone);
        Assert.Equal($$"""{"text":"x","tone":"{{word}}"}""", Written(line));
    }

    [Fact]
    public void TheOneArgumentConstructorIsAPlainLineWithNothingElse()
    {
        var line = new RuntimeTileLine("x");

        Assert.Equal("x", line.Text);
        Assert.Equal(RuntimeLineTone.Plain, line.Tone);
        Assert.Null(line.Parts);
        Assert.Null(line.Trend);
        Assert.Null(line.Marks);
        Assert.Equal("""{"text":"x","tone":"plain"}""", Written(line));
        Assert.Equal("""{"text":"","tone":"plain"}""", Written(new RuntimeTileLine(string.Empty)));
    }

    [Fact]
    public void ALineOfLinkedPiecesEqualsTheConstructorLineItStandsFor()
    {
        var parts = new List<RuntimeTextPart> { new("12 req/min", ToPortal), new(" · p95 85 ms") };
        var trend = new RuntimeTrend([0, 1], "Requests per minute");

        var line = RuntimeTileLine.Of(RuntimeLineTone.Ok, parts, trend);

        Assert.Equal(new RuntimeTileLine("12 req/min · p95 85 ms", RuntimeLineTone.Ok, parts, trend), line);
        Assert.Equal(new RuntimeTileLine("12 req/min · p95 85 ms", RuntimeLineTone.Ok, parts, trend, null), line);
        Assert.NotEqual(new RuntimeTileLine("12 req/min · p95 85 ms", RuntimeLineTone.Ok, parts, trend, []), line);
    }

    [Fact]
    public void AnArrayOfPiecesIsKeptAsThatSameArray()
    {
        RuntimeTextPart[] parts = [new("a"), new("b", ToPortal)];

        var line = RuntimeTileLine.Of(RuntimeLineTone.Plain, parts);

        Assert.Same(parts, line.Parts);
        Assert.Equal("ab", line.Text);
    }

    [Fact]
    public void OnePieceInSeveralSlotsIsSaidAsManyTimesAsItStands()
    {
        var linked = new RuntimeTextPart("x", ToPortal);
        var plain = new RuntimeTextPart("-");

        var line = RuntimeTileLine.Of(RuntimeLineTone.Plain, [linked, plain, linked, plain, linked]);

        Assert.Equal("x-x-x", line.Text);
        Assert.Equal(5, line.Parts!.Count);
        Assert.Same(linked, line.Parts[0]);
        Assert.Same(linked, line.Parts[4]);

        Assert.Equal("---", RuntimeTileLine.Of(RuntimeLineTone.Plain, [plain, plain, plain]).Text);
        Assert.Null(RuntimeTileLine.Of(RuntimeLineTone.Plain, [plain, plain, plain]).Parts);
    }

    [Fact]
    public void ALongLineIsJoinedWhole()
    {
        var chunk = new string('a', 50_000);

        var line = RuntimeTileLine.Of(RuntimeLineTone.Muted, [new RuntimeTextPart(chunk), new RuntimeTextPart(chunk, ToPortal)]);

        Assert.Equal(100_000, line.Text.Length);
        Assert.Equal(chunk + chunk, line.Text);
        Assert.Equal(2, line.Parts!.Count);
    }

    [Fact]
    public void AWhitespaceOnlyLinkedPieceStillKeepsThePieces()
    {
        var line = RuntimeTileLine.Of(RuntimeLineTone.Plain, [new RuntimeTextPart(" ", ToPortal)]);

        Assert.Equal(" ", line.Text);
        Assert.Single(line.Parts!);
    }
}
