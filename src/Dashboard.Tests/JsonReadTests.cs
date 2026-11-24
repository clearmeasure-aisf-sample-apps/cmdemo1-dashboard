using System.Text.Json;

namespace Dashboard.Tests;

/// <summary>
/// The readers of the parts of an answer the dashboard does not own: a part that is absent, or of another type, is
/// null, never an error.
/// </summary>
public class JsonReadTests
{
    private const string Answer = """
        {
          "metrics": { "cpu": 3.5 },
          "pools": [ { "name": "system" }, { "name": "user" } ],
          "empty": [],
          "name": "ui",
          "count": 2
        }
        """;

    /// <summary>An answer as a parent: its root element, detached from the document, which is disposed here.</summary>
    private static JsonElement? Root(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }

    [Fact]
    public void ASectionIsTheObjectUnderItsName()
    {
        var section = JsonRead.Section(Root(Answer), "metrics");

        Assert.NotNull(section);
        Assert.Equal(3.5, JsonRead.Number(section, "cpu"));
    }

    [Theory]
    [InlineData("nothere")] // no such name
    [InlineData("pools")] // an array, not an object
    [InlineData("empty")]
    [InlineData("name")] // text, not an object
    [InlineData("count")] // a number, not an object
    public void AnythingElseIsNoSection(string name) =>
        Assert.Null(JsonRead.Section(Root(Answer), name));

    [Theory]
    [InlineData("""{ "metrics": [ { "cpu": 3.5 } ] }""")] // the parent is an array: it has no properties
    [InlineData("""[ { "metrics": { "cpu": 3.5 } } ]""")]
    [InlineData("\"metrics\"")]
    [InlineData("3.5")]
    public void ASectionIsTakenFromAnObjectOnly(string json) =>
        Assert.Null(JsonRead.Section(Root(json), "metrics"));

    [Fact]
    public void ItemsAreTheElementsOfTheArray()
    {
        var items = JsonRead.Items(Root(Answer), "pools");

        Assert.Equal(2, items.Count());
        Assert.Equal(["system", "user"], items.Select(item => JsonRead.Text(item, "name")));
    }

    [Theory]
    [InlineData("nothere")] // no such name
    [InlineData("empty")] // an array with nothing in it
    [InlineData("metrics")] // an object, not an array
    [InlineData("name")]
    [InlineData("count")]
    public void AnythingElseIsNoItems(string name) =>
        Assert.Empty(JsonRead.Items(Root(Answer), name));

    [Theory]
    [InlineData("""{ "n": 3.5 }""", 3.5)]
    [InlineData("""{ "n": 0 }""", 0.0)]
    [InlineData("""{ "n": 12 }""", 12.0)]
    [InlineData("""{ "n": -1 }""", null)] // negative
    [InlineData("""{ "n": "3" }""", null)] // text, not a number
    [InlineData("""{ "n": true }""", null)]
    [InlineData("""{ "n": {} }""", null)]
    [InlineData("""{ "n": null }""", null)]
    [InlineData("""{ }""", null)] // no such name
    public void ANumberIsNotNegative(string json, double? expected) =>
        Assert.Equal(expected, JsonRead.Number(Root(json), "n"));

    [Theory]
    [InlineData("""{ "n": 3.5 }""", 3.5)]
    [InlineData("""{ "n": 0 }""", 0.0)]
    [InlineData("""{ "n": -0.3 }""", -0.3)] // below zero, such as a refund
    [InlineData("""{ "n": -12 }""", -12.0)]
    [InlineData("""{ "n": 1e400 }""", null)] // not finite
    [InlineData("""{ "n": "-3" }""", null)] // text, not a number
    [InlineData("""{ "n": true }""", null)]
    [InlineData("""{ "n": {} }""", null)]
    [InlineData("""{ "n": null }""", null)]
    [InlineData("""{ }""", null)] // no such name
    public void ASignedNumberIsAnyFiniteNumber(string json, double? expected) =>
        Assert.Equal(expected, JsonRead.Signed(Root(json), "n"));

    [Theory]
    [InlineData("""{ "n": 2.6 }""", 3)] // rounded
    [InlineData("""{ "n": 2.4 }""", 2)]
    [InlineData("""{ "n": 0 }""", 0)]
    [InlineData("""{ "n": -1 }""", null)]
    [InlineData("""{ "n": 1e12 }""", null)] // above int.MaxValue
    [InlineData("""{ "n": "3" }""", null)]
    [InlineData("""{ }""", null)]
    public void ACountIsARoundedNumberWithinItsRange(string json, int? expected) =>
        Assert.Equal(expected, JsonRead.Count(Root(json), "n"));

    [Theory]
    [InlineData("""{ "n": 1e12 }""", 1_000_000_000_000L)] // lines of code, seconds
    [InlineData("""{ "n": 2.4 }""", 2L)]
    [InlineData("""{ "n": 0 }""", 0L)]
    [InlineData("""{ "n": -1 }""", null)]
    [InlineData("""{ "n": "1000000000000" }""", null)]
    [InlineData("""{ }""", null)]
    public void ALargeCountIsRoundedToo(string json, long? expected) =>
        Assert.Equal(expected, JsonRead.Long(Root(json), "n"));

    [Theory]
    [InlineData("""{ "n": true }""", true)]
    [InlineData("""{ "n": false }""", false)]
    [InlineData("""{ "n": "true" }""", null)] // text, not a flag
    [InlineData("""{ "n": 1 }""", null)]
    [InlineData("""{ "n": 0 }""", null)]
    [InlineData("""{ }""", null)]
    public void AFlagIsTrueOrFalse(string json, bool? expected) =>
        Assert.Equal(expected, JsonRead.Flag(Root(json), "n"));

    [Theory]
    [InlineData("""{ "n": "  hi  " }""", "hi")] // trimmed
    [InlineData("""{ "n": "hi" }""", "hi")]
    [InlineData("""{ "n": "" }""", null)] // empty
    [InlineData("""{ "n": "   " }""", null)] // blank
    [InlineData("""{ "n": 5 }""", null)] // a number, not text
    [InlineData("""{ "n": true }""", null)]
    [InlineData("""{ "n": null }""", null)]
    [InlineData("""{ }""", null)]
    public void TextIsNotEmptyAndTrimmed(string json, string? expected) =>
        Assert.Equal(expected, JsonRead.Text(Root(json), "n"));

    [Fact]
    public void ATimeKeepsTheOffsetItSays()
    {
        var time = JsonRead.Time(Root("""{ "n": "2026-11-19T06:00:00+02:00" }"""), "n");

        Assert.Equal(new DateTimeOffset(2026, 11, 19, 6, 0, 0, TimeSpan.FromHours(2)), time);
        Assert.Equal(TimeSpan.FromHours(2), time?.Offset);
    }

    [Fact]
    public void ATimeWithoutAnOffsetIsUtc()
    {
        var time = JsonRead.Time(Root("""{ "n": "2026-11-19T06:00:00" }"""), "n");

        Assert.Equal(new DateTimeOffset(2026, 11, 19, 6, 0, 0, TimeSpan.Zero), time);
        Assert.Equal(TimeSpan.Zero, time?.Offset);
    }

    [Theory]
    [InlineData("""{ "n": "not a time" }""")]
    [InlineData("""{ "n": "" }""")]
    [InlineData("""{ "n": 5 }""")]
    [InlineData("""{ }""")]
    public void AnythingElseIsNoTime(string json) =>
        Assert.Null(JsonRead.Time(Root(json), "n"));

    [Theory]
    [InlineData("https://a.example/x", "https://a.example/x")]
    [InlineData("http://a.example", "http://a.example/")] // an address the dashboard links to
    [InlineData("ftp://a.example", null)] // another scheme
    [InlineData("/relative", null)] // not absolute
    [InlineData("javascript:alert(1)", null)]
    [InlineData("garbage", null)]
    [InlineData("", null)]
    [InlineData(null, null)]
    public void AnAddressIsAbsoluteHttpOrHttps(string? text, string? expected) =>
        Assert.Equal(expected, JsonRead.Address(text)?.AbsoluteUri);

    [Fact]
    public void AnAddressIsReadAsTextUnderItsName()
    {
        var answer = Root("""{ "url": "https://a.example/x", "path": "/relative", "count": 2 }""");

        Assert.Equal("https://a.example/x", JsonRead.Address(answer, "url")?.AbsoluteUri);
        Assert.Null(JsonRead.Address(answer, "path"));
        Assert.Null(JsonRead.Address(answer, "count"));
        Assert.Null(JsonRead.Address(answer, "nothere"));
    }

    [Fact]
    public void AParentThatIsThereNotReadsNothing()
    {
        Assert.Null(JsonRead.Section(null, "metrics"));
        Assert.Empty(JsonRead.Items(null, "pools"));
        Assert.Null(JsonRead.Number(null, "cpu"));
        Assert.Null(JsonRead.Signed(null, "cpu"));
        Assert.Null(JsonRead.Count(null, "cpu"));
        Assert.Null(JsonRead.Long(null, "cpu"));
        Assert.Null(JsonRead.Flag(null, "ready"));
        Assert.Null(JsonRead.Text(null, "name"));
        Assert.Null(JsonRead.Time(null, "generated"));
        Assert.Null(JsonRead.Address(null, "url"));
    }

    private sealed record Named(string Name, int? Count);

    [Fact]
    public void AnIdentifiedRecordIsMadeFromItsIdentityAndItsObject()
    {
        var named = JsonRead.Identified(Root(Answer), "name", static (name, parts) => new Named(name, JsonRead.Count(parts, "count")));

        Assert.Equal(new Named("ui", 2), named);
    }

    [Theory]
    [InlineData("""{ "count": 2 }""")] // no identity
    [InlineData("""{ "name": "  ", "count": 2 }""")] // an empty identity
    [InlineData("""{ "name": 7 }""")] // an identity of another type
    [InlineData("""[ { "name": "ui" } ]""")] // not an object
    public void ThereIsNoIdentifiedRecordWithoutItsIdentity(string json)
    {
        var made = false;

        var named = JsonRead.Identified(Root(json), "name", (name, _) => { made = true; return new Named(name, null); });

        Assert.Null(named);
        Assert.False(made);
    }

    [Fact]
    public void ThereIsNoIdentifiedRecordWithoutAnObject() =>
        Assert.Null(JsonRead.Identified(null, "name", static (name, _) => new Named(name, null)));

    [Fact]
    public void APresentRecordIsMadeFromAnyObjectEvenAnEmptyOne()
    {
        Assert.Equal(new Named("x", 2), JsonRead.Present(Root(Answer), static parts => new Named("x", JsonRead.Count(parts, "count"))));
        Assert.Equal(new Named("x", null), JsonRead.Present(Root("{}"), static parts => new Named("x", JsonRead.Count(parts, "count"))));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("[]")]
    [InlineData("3")]
    public void ThereIsNoPresentRecordWithoutAnObject(string? json) =>
        Assert.Null(JsonRead.Present(json is null ? null : Root(json), static _ => new Named("x", null)));

    [Theory]
    [InlineData("pools", true)]
    [InlineData("empty", true)]
    [InlineData("metrics", false)] // an object, not an array
    [InlineData("name", false)]   // text
    [InlineData("count", false)]  // a number
    [InlineData("nothere", false)]
    public void HasArrayRecognisesAnArrayByItsName(string name, bool expected) =>
        Assert.Equal(expected, JsonRead.HasArray(Root(Answer), name));

    [Theory]
    [InlineData("[]")]
    [InlineData("3")]
    [InlineData("\"text\"")]
    [InlineData("{ }")]
    public void HasArrayOnANonObjectParentIsFalse(string json) =>
        Assert.False(JsonRead.HasArray(Root(json), "x"));

    [Fact]
    public void HasArrayOnANullParentIsFalse() =>
        Assert.False(JsonRead.HasArray(null, "x"));

    [Theory]
    [InlineData("""{ "n": 2.6 }""", 3)] // rounded
    [InlineData("""{ "n": 2.5 }""", 2)] // a half to the even neighbour
    [InlineData("""{ "n": 3.5 }""", 4)]
    [InlineData("""{ "n": 0.4 }""", 0)]
    [InlineData("""{ "n": 0 }""", 0)]
    [InlineData("""{ "n": 2147483647.4 }""", int.MaxValue)]
    [InlineData("""{ "n": 3000000000 }""", int.MaxValue)] // past int.MaxValue: held at the top, not refused
    [InlineData("""{ "n": 1e12 }""", int.MaxValue)]
    [InlineData("""{ "n": 1e21 }""", int.MaxValue)]
    [InlineData("""{ "n": 1e400 }""", int.MaxValue)] // too large for a double: held at the top too
    [InlineData("""{ "n": -1 }""", null)]
    [InlineData("""{ "n": -0.5 }""", null)]
    [InlineData("""{ "n": "3" }""", null)]
    [InlineData("""{ "n": null }""", null)]
    [InlineData("""{ }""", null)]
    public void ASaturatedCountHoldsANumberPastAnIntAtItsTop(string json, int? expected) =>
        Assert.Equal(expected, JsonRead.SaturatedCount(Root(json), "n"));

    [Fact]
    public void ASaturatedCountOnANullParentIsNull() =>
        Assert.Null(JsonRead.SaturatedCount(null, "n"));
}
