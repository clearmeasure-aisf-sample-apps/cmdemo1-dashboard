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
    [InlineData("""{ "n": 2.5 }""", 3)] // a half away from zero, not to the even neighbour (#1328)
    [InlineData("""{ "n": 3.5 }""", 4)]
    [InlineData("""{ "n": 0.5 }""", 1)]
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
    [InlineData("""{ "n": 2.5 }""", 3L)] // a half away from zero, as a count rounds
    [InlineData("""{ "n": 4.5 }""", 5L)]
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

    /// <summary>A value that is itself an address (#1039): read as <see cref="JsonRead.Text"/> reads a word - so trimmed,
    /// and nothing when empty or no string - and then held to the one rule of an address.</summary>
    [Theory]
    [InlineData("\"https://a.example/x\"", "https://a.example/x")]
    [InlineData("\"  https://a.example/x  \"", "https://a.example/x")] // trimmed
    [InlineData("\"http://a.example\"", "http://a.example/")]
    [InlineData("\"ftp://a.example\"", null)] // another scheme
    [InlineData("\"/relative\"", null)]
    [InlineData("\"\"", null)] // the empty word
    [InlineData("\"   \"", null)]
    [InlineData("5", null)] // no string
    [InlineData("null", null)]
    [InlineData("{ \"url\": \"https://a.example/x\" }", null)]
    public void AValueIsAnAddressWhenItIsAWordThatIsOne(string json, string? expected)
    {
        using var document = JsonDocument.Parse(json);

        Assert.Equal(expected, JsonRead.Address(document.RootElement)?.AbsoluteUri);
        Assert.Equal(expected, JsonRead.Address(Root($$"""{ "n": {{json}} }"""), "n")?.AbsoluteUri);
    }

    /// <summary>The moments <c>topology.json</c> is pinned on (#1039): in Zulu, trimmed, in another hour, and a bare day.</summary>
    [Theory]
    [InlineData("2026-10-04T22:00:00Z", "2026-10-04T22:00:00+00:00")]
    [InlineData(" 2026-10-04T22:00:00Z ", "2026-10-04T22:00:00+00:00")]
    [InlineData("2026-10-04T22:00:00+02:00", "2026-10-04T22:00:00+02:00")]
    [InlineData("2026-10-04", "2026-10-04T00:00:00+00:00")]
    public void ATimeIsReadInZuluInAnotherHourOrAsABareDay(string text, string expected)
    {
        var time = JsonRead.Time(Root($$"""{ "n": "{{text}}" }"""), "n");

        Assert.Equal(DateTimeOffset.Parse(expected, System.Globalization.CultureInfo.InvariantCulture), time);
        Assert.Equal(DateTimeOffset.Parse(expected, System.Globalization.CultureInfo.InvariantCulture).Offset, time?.Offset);
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
    public void NamedRowsAreMadeFromTheirNameAndObjectInTheOrderWritten()
    {
        var rows = JsonRead.Named(
            Root("""{ "rows": [ { "name": " b ", "count": 2 }, { "name": "a" } ] }"""),
            "rows",
            static (name, parts) => new Named(name, JsonRead.Count(parts, "count")));

        Assert.Equal([new Named("b", 2), new Named("a", null)], rows);
    }

    [Theory]
    [InlineData("""{ "rows": [ { "count": 2 }, { "name": "ui" } ] }""")] // no name
    [InlineData("""{ "rows": [ { "name": "  " }, { "name": "ui" } ] }""")] // an empty name
    [InlineData("""{ "rows": [ { "name": 7 }, { "name": "ui" } ] }""")] // a name of another type
    [InlineData("""{ "rows": [ "ui", [ { "name": "x" } ], { "name": "ui" } ] }""")] // a row that is no object
    public void ARowWithoutANameIsLeftOut(string json)
    {
        var rows = JsonRead.Named(Root(json), "rows", static (name, _) => new Named(name, null));

        Assert.Equal([new Named("ui", null)], rows);
    }

    [Theory]
    [InlineData("""{ }""")] // no such array
    [InlineData("""{ "rows": { "name": "ui" } }""")] // an object, not an array
    [InlineData("""{ "rows": [] }""")]
    [InlineData("""[ { "name": "ui" } ]""")] // the parent is no object
    public void ThereAreNoNamedRowsWithoutAnArrayOfThem(string json) =>
        Assert.Empty(JsonRead.Named(Root(json), "rows", static (name, _) => new Named(name, null)));

    [Fact]
    public void ThereAreNoNamedRowsWithoutAParent() =>
        Assert.Empty(JsonRead.Named(null, "rows", static (name, _) => new Named(name, null)));

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

    // ----- The strings of a list -----

    /// <summary>The strings of the list: each is trimmed, the empty ones are left out, and the rest stand in the
    /// order written - and a list of no words at all is no list, one <c>null</c>, not an empty one.</summary>
    [Fact]
    public void TheStringsOfAListAreItsTrimmedWordsInTheOrderWritten()
    {
        var strings = JsonRead.Strings(Root("""{ "ids": [ "  a  ", "", "b", 7, null, true, {}, [], "   ", "c" ] }"""), "ids");

        Assert.NotNull(strings);
        Assert.Equal(["a", "b", "c"], strings);
    }

    [Theory]
    [InlineData("""{ }""")] // no such name
    [InlineData("""{ "ids": [] }""")] // an array with nothing in it
    [InlineData("""{ "ids": [ "" ] }""")] // the empty word only
    [InlineData("""{ "ids": [ "   ", "\t" ] }""")] // words of nothing but room
    [InlineData("""{ "ids": [ "\u00A0" ] }""")] // a word of only no-break spaces is no word
    [InlineData("""{ "ids": [ 7, null, true, {}, [] ] }""")] // nothing that is a string
    [InlineData("""{ "ids": { "a": 1 } }""")] // an object, not an array
    [InlineData("""{ "ids": "a" }""")] // text, not an array
    [InlineData("""{ "ids": 7 }""")] // a number, not an array
    [InlineData("""{ "ids": null }""")]
    public void AnythingElseIsNoStrings(string json) =>
        Assert.Null(JsonRead.Strings(Root(json), "ids"));

    [Theory]
    [InlineData("""[ { "ids": [ "a" ] } ]""")] // the parent is an array: it has no properties
    [InlineData("\"ids\"")]
    [InlineData("3")]
    public void StringsAreTakenFromAnObjectOnly(string json) =>
        Assert.Null(JsonRead.Strings(Root(json), "ids"));

    [Fact]
    public void StringsOnANullParentAreNull() =>
        Assert.Null(JsonRead.Strings(null, "ids"));

    /// <summary>
    /// Every string that stands is kept, in the order written: the helper knows no most, and capping a list is the
    /// business of the reader that reads it - the most ids of one box is the runtime manifest's, and is pinned beside
    /// it.
    /// </summary>
    [Fact]
    public void EveryStringThatStandsIsKeptForTheReaderToCap()
    {
        var written = string.Join(",", Enumerable.Range(0, 300).Select(number => $"""" "i{number}" """"));

        var strings = JsonRead.Strings(Root($$"""{ "ids": [ {{written}} ] }"""), "ids");

        Assert.NotNull(strings);
        Assert.Equal(300, strings.Count);
        Assert.Equal("i199", strings[199]);
    }

    // ----- The names of a list -----

    /// <summary>
    /// The names of the list: kept in the file's own letters - nothing is trimmed and nothing is cased - in the
    /// order written; what is not a word is left out, and the empty word is no name, as in <see cref="JsonRead.Text"/>.
    /// Unlike <see cref="JsonRead.Strings"/>, the reading is the list and not a word of it: without an array, or when
    /// it names nothing, the answer is an empty list the record keeps, not a <c>null</c>.
    /// </summary>
    [Fact]
    public void TheNamesOfAListAreItsWordsInTheOrderWrittenInItsOwnLetters()
    {
        var names = JsonRead.Names(Root("""{ "environments": [ "  prod  ", "", "uat", 7, null, true, {}, [], "PROD" ] }"""), "environments");

        Assert.Equal(["  prod  ", "uat", "PROD"], names);
    }

    /// <summary>
    /// A name is not trimmed and not cased: the padding of a name is its own, and a name in another case is another
    /// name, since the page says the names back in the letters the file wrote them in.
    /// </summary>
    [Fact]
    public void ANameIsKeptInTheLettersTheFileWroteItIn()
    {
        var names = JsonRead.Names(Root("""{ "projects": [ "  demo-ui  ", "Demo-UI" ] }"""), "projects");

        Assert.Equal(["  demo-ui  ", "Demo-UI"], names);
        Assert.Equal(2, names.Count);
    }

    /// <summary>
    /// The answer of the reading is a list, empty when the field is not an array or names nothing - never a
    /// <c>null</c>, which is what the record that keeps it is built on.
    /// </summary>
    [Theory]
    [InlineData("""{ }""")] // no such name
    [InlineData("""{ "environments": [] }""")] // an array with nothing in it
    [InlineData("""{ "environments": [ "" ] }""")] // the empty word only
    [InlineData("""{ "environments": [ 7, null, true, {}, [] ] }""")] // nothing that is a string
    [InlineData("""{ "environments": { "a": 1 } }""")] // an object, not an array
    [InlineData("""{ "environments": "prod" }""")] // text, not an array
    [InlineData("""{ "environments": 7 }""")] // a number, not an array
    [InlineData("""{ "environments": null }""")]
    public void AnythingElseIsNoNames(string json) =>
        Assert.Empty(JsonRead.Names(Root(json), "environments"));

    /// <summary>A null parent is no object, and its reading is an empty list, as the record that keeps it expects.</summary>
    [Fact]
    public void NamesOnANullParentIsEmpty() =>
        Assert.Empty(JsonRead.Names(null, "environments"));

    // ----- Root: the object a file's text stands as -----

    private sealed record Rooted(string? Answer);

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\n")]
    public void RootReturnsEmptyForNullOrBlankText(string? json) =>
        Assert.Equal(FileProblems.Empty, JsonRead.Root(json, _ => new Rooted("read"), problem => new Rooted(problem)).Answer);

    [Theory]
    [InlineData("[]")]
    [InlineData("42")]
    [InlineData("\"x\"")]
    [InlineData("true")]
    [InlineData("null")]
    public void RootReturnsNotAnObjectWhenTheRootIsNotAnObject(string json)
    {
        var read = false;

        var result = JsonRead.Root(json, _ => { read = true; return new Rooted("read"); }, problem => new Rooted(problem));

        Assert.Equal(FileProblems.NotAnObject, result.Answer);
        Assert.False(read);
    }

    [Fact]
    public void RootReturnsBrokenWhenTheTextIsNotValidJson()
    {
        var result = JsonRead.Root("{", _ => new Rooted("read"), problem => new Rooted(problem));

        Assert.Equal(FileProblems.Broken, result.Answer);
    }

    [Fact]
    public void RootReturnsTheAnswerOfReadForAValidObject()
    {
        const string field = "n";

        var result = JsonRead.Root("{\"n\": 7}", root => new Rooted("n=" + JsonRead.Number(root, field)), problem => new Rooted(problem));

        Assert.Equal("n=7", result.Answer);
    }

    [Fact]
    public void RootReturnsBrokenWhenReadThrowsJsonException()
    {
        var result = JsonRead.Root(
            "{}",
            _ => throw new JsonException(),
            problem => new Rooted(problem));

        Assert.Equal(FileProblems.Broken, result.Answer);
    }
}
