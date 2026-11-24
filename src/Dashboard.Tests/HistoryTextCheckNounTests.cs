namespace Dashboard.Tests;

/// <summary>
/// The noun of the counted history of
/// <see cref="HistoryText.Describe(IReadOnlyCollection{ProbeResult})" />: the word that follows <c>Last</c> is
/// <see cref="Plural.Noun(int, string, string?)" />'s and not a singular/plural pair written again inside the
/// sentence (#1593). The one check is said without its digit - <c>Last check: 1 healthy</c> - and every other count
/// with it, <c>Last 2 checks</c>, <c>Last 1234 checks</c>, the digit written in the invariant culture.
/// <para>
/// What was already pinned elsewhere and is not repeated as a claim here: <see cref="TextTests" />,
/// <see cref="HistoryTextCountedOrderTests" /> and <see cref="HistoryTextEdgeCaseTests" /> pin the sentence and the
/// order its states are counted in, and <see cref="HistoryTextCultureTests" /> pins the counts of a history of 2236
/// checks for four readers. Added here because #1593 moved this one word to <see cref="Plural" /> and moved the
/// digit with it: the words are pinned again at the counts the noun turns on, the prefix and the noun are held to
/// what the rule says for the same count, and the history with no check at all - which names no count and so asks
/// no noun - is pinned unchanged beside them.
/// </para>
/// </summary>
public class HistoryTextCheckNounTests
{
    private static readonly DateTimeOffset Time = new(2026, 10, 4, 22, 0, 5, TimeSpan.Zero);

    private static ProbeResult Check(HealthState state) => new(state, null, null, Time);

    /// <summary>A history of <paramref name="count" /> healthy checks, all answered at the same fixed moment.</summary>
    private static List<ProbeResult> Checks(int count) =>
        Enumerable.Repeat(Check(HealthState.Healthy), count).ToList();

    /// <summary>The one check is said without its digit and every other count with it, the noun agreeing at each -
    /// the same words as before the noun came to <see cref="Plural" />.</summary>
    [Theory]
    [InlineData(1, "Last check: 1 healthy")]
    [InlineData(2, "Last 2 checks: 2 healthy")]
    [InlineData(12, "Last 12 checks: 12 healthy")]
    [InlineData(1234, "Last 1234 checks: 1234 healthy")]
    public void TheHistorySaysTheNounItsCountAgreesWith(int count, string expected) =>
        Assert.Equal(expected, HistoryText.Describe(Checks(count)));

    /// <summary>Prefix and noun together are what the rule says for that very count, so the sentence the history is
    /// announced with holds no second copy of the agreement and cannot part from <see cref="Plural" />.</summary>
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(12)]
    [InlineData(1234)]
    [InlineData(2236)]
    public void TheNounOfTheHistoryIsTheNounPluralGivesThatCount(int count)
    {
        var said = count == 1
            ? $"Last {Plural.Noun(count, "check")}"
            : FormattableString.Invariant($"Last {count} {Plural.Noun(count, "check")}");

        Assert.StartsWith($"{said}: ", HistoryText.Describe(Checks(count)), StringComparison.Ordinal);
    }

    /// <summary>A history of one check names its state the same way whatever that state is, singular all the
    /// while: the noun the count of one asks for is said even when the states listed after it are several.</summary>
    [Theory]
    [InlineData(HealthState.Healthy)]
    [InlineData(HealthState.Unhealthy)]
    [InlineData(HealthState.Unreachable)]
    public void TheOneCheckKeepsItsSingularNounWhateverStateItAnswered(HealthState state) =>
        Assert.Equal(
            $"Last check: 1 {HealthClassifier.Label(state).ToLowerInvariant()}",
            HistoryText.Describe([Check(state)]));

    /// <summary>The history with no check at all keeps its fixed words, which name no count and so ask no noun:
    /// the sentence #1593 moved to <see cref="Plural" /> begins after it, and it is left exactly as it was.</summary>
    [Fact]
    public void TheHistoryWithNoCheckStillSaysItsFixedWords() =>
        Assert.Equal("No checks yet", HistoryText.Describe(Array.Empty<ProbeResult>()));

    /// <summary>The noun and the digit before it are the dashboard's and not the viewer's: a thousand-odd checks are
    /// counted without a group separator, and the one check stays without its digit, for an invariant, a German and
    /// a French reader alike.</summary>
    [Theory]
    [InlineData("")]
    [InlineData("de-DE")]
    [InlineData("fr-FR")]
    public void TheNounAndItsDigitDoNotChangeWithTheViewersCulture(string reader)
    {
        Assert.Equal(
            "Last 1234 checks: 1234 healthy",
            CultureScope.Says(reader, () => HistoryText.Describe(Checks(1234))));
        Assert.Equal(
            "Last check: 1 healthy",
            CultureScope.Says(reader, () => HistoryText.Describe(Checks(1))));
    }

    /// <summary>#1593's acceptance criterion for this site, as a check: the file that announces a history asks
    /// <see cref="Plural.Noun(int, string, string?)" /> for the word after <c>Last</c>, and holds neither the
    /// singular nor the plural of it written out as its own words.</summary>
    [Fact]
    public void TheFileThatAnnouncesTheHistoryAsksPluralForTheWord()
    {
        var source = File.ReadAllText(MarkupGuards.SourcePath(Path.Combine("Health", "HistoryText.cs")));

        Assert.Contains("Plural.Noun(history.Count, \"check\")", source, StringComparison.Ordinal);
        Assert.DoesNotContain("\"Last check\"", source, StringComparison.Ordinal);
        Assert.DoesNotContain("checks\"", source, StringComparison.Ordinal);
    }
}
