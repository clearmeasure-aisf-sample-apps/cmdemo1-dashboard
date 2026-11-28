namespace Dashboard.Tests;

/// <summary>
/// The tests of <see cref="HealthDetailText.Failing" /> - the phrase of one failing entry in
/// <see cref="HealthDetailText.Failed" />'s list: its name in brackets with its state and its own words,
/// <c>DataAccess (Unhealthy: Login failed)</c>. The whole sentence is pinned in
/// <see cref="HealthDetailTextEdgeCaseTests" />; what is pinned here is the single entry's phrase apart from
/// the sentence, and that <see cref="HealthDetailText.Failed" /> is the lead words joined to the phrases of its
/// failing entries with the list separator.
/// </summary>
public class HealthDetailTextFailingTests
{
    /// <summary>A fixed moment: none of these words is counted from it, and it is here only to make an answer.</summary>
    private static readonly DateTimeOffset ReadAt = new(2026, 10, 4, 22, 0, 0, TimeSpan.Zero);

    /// <summary>An entry of the four parts a caller names, the last two left out when the caller names none.</summary>
    private static HealthCheckEntry Entry(string name, CheckState state, string? description = null, double? durationMs = null) =>
        new(name, state, description, durationMs);

    /// <summary>An answer of the entries given, in the order given, read at the fixed moment above.</summary>
    private static HealthDetail Detail(params HealthCheckEntry[] entries) => new(entries, ReadAt);

    /// <summary>The phrase of a healthy entry is still its name in brackets with the state and no colon when the
    /// entry says nothing: <c>API (Healthy)</c> - the state's word alone, the way the entry that says no words at
    /// all is named.</summary>
    [Fact]
    public void ThePhraseOfAHalthyEntryIsItsNameAndStateAlone() =>
        Assert.Equal("API (Healthy)", HealthDetailText.Failing(Entry("API", CheckState.Healthy)));

    /// <summary>The phrase of a degraded entry with no words of its own is its name in brackets with the state's
    /// word and nothing after it: <c>LlmGateway (Degraded)</c> - the colon the words are joined by (#1167) stands
    /// only when the words stand.</summary>
    [Fact]
    public void ThePhraseOfADegradedEntryWithNoWordsIsItsNameAndStateAlone() =>
        Assert.Equal("LlmGateway (Degraded)", HealthDetailText.Failing(Entry("LlmGateway", CheckState.Degraded)));

    /// <summary>The phrase of an unhealthy entry with words that end in a full stop is its name in brackets with
    /// the state's word, a colon and the words without their full stop: <c>DataAccess (Unhealthy: Login failed)</c>
    /// - the one trimming the words are joined by (#1167) leaves the stop to the sentence that closes it.</summary>
    [Fact]
    public void ThePhraseOfAnUnhealthyEntryTrimsTheFullStopOfItsWords() =>
        Assert.Equal("DataAccess (Unhealthy: Login failed)", HealthDetailText.Failing(Entry("DataAccess", CheckState.Unhealthy, "Login failed.")));

    /// <summary>The phrase of an entry whose state the enum does not name is its name in brackets with the word
    /// <c>Not known</c> and nothing after it: <c>Odd (Not known)</c> - the same word the marks say, and the same
    /// phrase the list of who failed puts in the sentence.</summary>
    [Fact]
    public void ThePhraseOfAnUnknownEntryIsItsNameAndTheWordNotKnown() =>
        Assert.Equal("Odd (Not known)", HealthDetailText.Failing(Entry("Odd", CheckState.Unknown)));

    /// <summary>The phrase refuses an entry that is not there, by the name the guard keeps.</summary>
    [Fact]
    public void ThePhraseRefusesAnEntryThatIsNotThere() =>
        Assert.Equal("entry", Assert.Throws<ArgumentNullException>(() => HealthDetailText.Failing(null!)).ParamName);

    /// <summary>The sentence of who failed is the lead words, then the phrases of the entries that are not healthy
    /// in the order the answer says them, joined with the list separator: two failing entries of an answer of four
    /// are the lead words, the first phrase, the separator and the second - nothing more, nothing less, and the
    /// healthy entries stand out of the sentence.</summary>
    [Fact]
    public void TheSentenceOfWhoFailedIsTheLeadJoiningThePhrasesOfTheFailingEntries()
    {
        var database = Entry("DataAccess", CheckState.Unhealthy, "Login failed.");
        var gateway = Entry("LlmGateway", CheckState.Degraded);
        var healthy = Entry("API", CheckState.Healthy, "API layer is healthy", 1.5);
        var steady = Entry("Cache", CheckState.Healthy, null, 2.0);
        var detail = Detail(database, gateway, healthy, steady);
        var failing = detail.NotHealthy;

        Assert.Equal(
            "Not healthy: " + string.Join(", ", failing.Select(HealthDetailText.Failing)),
            HealthDetailText.Failed(detail));
        Assert.Equal("Not healthy: DataAccess (Unhealthy: Login failed), LlmGateway (Degraded)", HealthDetailText.Failed(detail));
    }
}
