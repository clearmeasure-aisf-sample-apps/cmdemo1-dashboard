namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="ContainerAppText" />, the words a container app's
/// probe is told in: the badge word, the number line next to it and the tooltip. What is pinned where it belongs,
/// and what this file pins: <see cref="ContainerAppText.Label" /> says its six words for the six states, and the
/// word of a number that is no state, which <see cref="HealthHelperCoverageTests" /> and
/// <see cref="ContainerAppStateEdgeCaseTests" /> pin, so nothing of it is pinned here; the number line next to
/// the badge is pinned in <see cref="RuntimePayloadHelperTests" /> for the codes and the times the page actually
/// saw, and its refusal of a status that is not there in <see cref="RuntimeWatchEdgeCaseTests" /> - what is pinned
/// here are the codes and the times at the ends of the number, the status that stands beside the answer the page
/// may not read, the answer the page may not read in the state the app has not answered yet, and a time to an
/// answer the page never received; the tooltip's sentences are pinned for the healthy answer, the 404 of the
/// health path, the answer the app refused to let the page read, the silence with a word and without one, the
/// name that is not there, and the hours five west of the meridian - what is pinned here are the codes the probe
/// heard at the ends of the number, the 404 asked at a path the page cannot classify, the silence whose word is
/// only whitespace, the name that is only whitespace, the host of an address that carries a port, the hours east
/// of the meridian, the moment that stands on its own offset, and the moments at the ends of the time.
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: every moment is one the test brings, and the
/// zones are UTC and the one the test makes.
/// </para>
/// </summary>
public class ContainerAppTextEdgeCaseTests
{
    private static readonly DateTimeOffset Opened = new(2026, 10, 4, 22, 0, 0, TimeSpan.Zero);
    private static readonly Uri App = new("https://ca-demo-tdd-ui.graybush-1.southcentralus.azurecontainerapps.io");
    private static readonly ContainerAppTarget Declared = new(App, "/_healthcheck");
    private static readonly ContainerAppTarget WithPort = new(new Uri("https://app.example.net:8443"), "/_healthcheck");
    private static readonly TimeZoneInfo Utc = TimeZoneInfo.Utc;
    private static readonly TimeZoneInfo PlusFiveThirty = TimeZoneInfo.CreateCustomTimeZone("east", TimeSpan.FromHours(5.5), "east", "east");

    private static ContainerAppAnswer Heard(int status, DateTimeOffset at, bool readsHealth = true, string path = "/_healthcheck") =>
        new(path, readsHealth, status, false, 85, at);

    private static ContainerAppAnswer Silent(DateTimeOffset at, string? detail) =>
        new("/_healthcheck", true, null, false, null, at, detail);

    private static ContainerAppStatus Seen(ContainerAppState state, int? status = null, int? ms = null, bool opaque = false) =>
        new(state, new ContainerAppAnswer("/_healthcheck", true, status, opaque, ms, Opened));

    /// <summary>The number line as a viewer of another culture would see it: the code asks the invariant one, so
    /// even at the ends of the number the digits and the minus sign never change.</summary>
    private static string Facts(ContainerAppStatus status) =>
        CultureScope.Says("de-DE", () => ContainerAppText.Facts(status));

    // ----- the number line next to the badge, at the ends of the number

    /// <summary>
    /// The codes and the times the number runs to are said as the numbers they are: the zero and the negative the
    /// number may still stand on, the hundred and ninety-nine and the three hundred the probe does not take, the
    /// six hundred the number runs past the five hundreds, and the ends of the number itself - said to the viewer
    /// in a culture that is not the invariant one, so the digits and the minus sign stand as the invariant ones.
    /// </summary>
    [Theory]
    [InlineData(0, 0, "HTTP 0 · 0 ms")]
    [InlineData(-1, -1, "HTTP -1 · -1 ms")]
    [InlineData(199, 1, "HTTP 199 · 1 ms")]
    [InlineData(300, 1, "HTTP 300 · 1 ms")]
    [InlineData(600, 1, "HTTP 600 · 1 ms")]
    [InlineData(int.MinValue, int.MaxValue, "HTTP -2147483648 · 2147483647 ms")]
    [InlineData(int.MinValue, null, "HTTP -2147483648")]
    [InlineData(0, null, "HTTP 0")]
    [InlineData(int.MaxValue, null, "HTTP 2147483647")]
    public void TheFactsSayTheCodeAndTheTimeAtTheEndsOfTheNumber(int? status, int? ms, string facts) =>
        Assert.Equal(facts, Facts(Seen(ContainerAppState.Unhealthy, status, ms)));

    /// <summary>
    /// A status stands beside the answer the page may not read as the status it is: the code says itself, with
    /// the word that says the app answered without being read not said of a status that stands - the four four
    /// four, the code the probe takes for the app gone, among them. The status beside the answer with its time,
    /// <see cref="RuntimePayloadHelperTests" /> pins.
    /// </summary>
    [Theory]
    [InlineData(200, "HTTP 200")]
    [InlineData(404, "HTTP 404")]
    public void AStatusBesideTheUnreadAnswerSaysTheStatusOnly(int status, string facts) =>
        Assert.Equal(facts, Facts(Seen(ContainerAppState.Responding, status, null, opaque: true)));

    /// <summary>
    /// The answer the page may not read is said as answered while the app has not answered yet: the word that
    /// says it answered, with the time and without it, stands before the word that says the app has not answered
    /// yet - the page in fact heard the app, only not from itself. The time to that answer runs to the ends of
    /// the number as well.
    /// </summary>
    [Theory]
    [InlineData(85, "answered · 85 ms")]
    [InlineData(0, "answered · 0 ms")]
    [InlineData(int.MinValue, "answered · -2147483648 ms")]
    [InlineData(int.MaxValue, "answered · 2147483647 ms")]
    [InlineData(null, "answered")]
    public void TheUnreadAnswerIsSaidAnsweredWhileTheAppHasNotAnsweredYet(int? ms, string facts) =>
        Assert.Equal(facts, Facts(Seen(ContainerAppState.Waking, null, ms, opaque: true)));

    /// <summary>
    /// A time to an answer the page never received is no fact: a latency without a status and without the answer
    /// being unread says no answer, in the states the pinning left open - the app the page took for broken and
    /// the one it took for answering. And a status the page received says its status in any state: the answered
    /// code stands before the word that says the app has not answered yet.
    /// </summary>
    [Theory]
    [InlineData(ContainerAppState.Unhealthy, "no answer")]
    [InlineData(ContainerAppState.Responding, "no answer")]
    [InlineData(ContainerAppState.Probing, "no answer")]
    public void ATimeWithoutAnAnswerSaysNoAnswer(ContainerAppState state, string facts) =>
        Assert.Equal(facts, Facts(Seen(state, null, 85)));

    [Fact]
    public void AReceivedStatusSaysItsStatusWhileTheAppHasNotAnsweredYet()
    {
        Assert.Equal("HTTP 200 · 85 ms", Facts(Seen(ContainerAppState.Waking, 200, 85)));
        Assert.Equal("HTTP 200 · 85 ms", Facts(Seen(ContainerAppState.Probing, 200, 85)));
    }

    // ----- the tooltip, at the ends of the number and the time

    /// <summary>
    /// The status the probe heard is said as the number it is, at the ends of the number: the zero and the
    /// negative the number may still stand on, the hundred and ninety-nine and the three hundred the probe does
    /// not take, the six hundred the number runs past the five hundreds, and the ends of the number itself.
    /// </summary>
    [Theory]
    [InlineData(0, "Last probe 22:00:00: /_healthcheck answered HTTP 0.")]
    [InlineData(-1, "Last probe 22:00:00: /_healthcheck answered HTTP -1.")]
    [InlineData(199, "Last probe 22:00:00: /_healthcheck answered HTTP 199.")]
    [InlineData(300, "Last probe 22:00:00: /_healthcheck answered HTTP 300.")]
    [InlineData(600, "Last probe 22:00:00: /_healthcheck answered HTTP 600.")]
    [InlineData(int.MinValue, "Last probe 22:00:00: /_healthcheck answered HTTP -2147483648.")]
    [InlineData(int.MaxValue, "Last probe 22:00:00: /_healthcheck answered HTTP 2147483647.")]
    public void TheTooltipSaysTheStatusTheProbeHeardAtTheEndsOfTheNumber(int status, string said)
    {
        var title = ContainerAppText.Title("ui", Declared, new ContainerAppStatus(ContainerAppState.Responding, Heard(status, Opened)), Utc);

        Assert.Contains(said, title, StringComparison.Ordinal);
    }

    /// <summary>
    /// A four four four asked at a path that is not the health check is said as what it is - the page could not
    /// classify the answer as a health check - and not as what the four four four of the health path is: the app
    /// being down is said of the health path alone.
    /// </summary>
    [Fact]
    public void AFourFourFourAskedWhereThePageCannotClassifyIsNotSaidForTheAppGone()
    {
        var title = ContainerAppText.Title(
            "ui",
            Declared,
            new ContainerAppStatus(ContainerAppState.Responding, Heard(404, Opened, readsHealth: false, path: "/")),
            Utc);

        Assert.Contains("Last probe 22:00:00: / answered HTTP 404.", title, StringComparison.Ordinal);
        Assert.Contains("The page could not classify the answer as a health check.", title, StringComparison.Ordinal);
        Assert.DoesNotContain("HTTP 404 at the health path", title, StringComparison.Ordinal);
    }

    /// <summary>
    /// A silence whose word is only whitespace is the silence whose word is not there: the sentence ends where
    /// the word is, the whitespace trimmed from the end rather than standing in the title - the whole tooltip
    /// standing as the one the pinning says for the silence without a word.
    /// </summary>
    [Fact]
    public void ASilenceWhoseDetailIsOnlyWhitespaceIsSaidAsTheSilenceWithoutAWord()
    {
        const string title =
            "ui (ca-demo-tdd-ui.graybush-1.southcentralus.azurecontainerapps.io): Not responding. "
            + "This page asks it once a minute while it shows this diagram, and not while it is paused or hidden; "
            + "nothing asks it from here while no page is open, so it can scale to zero. "
            + "Last probe 22:00:00: /_healthcheck, no answer.";

        Assert.Equal(title, ContainerAppText.Title("ui", Declared, new ContainerAppStatus(ContainerAppState.NotResponding, Silent(Opened, " ")), Utc));
        Assert.Equal(title, ContainerAppText.Title("ui", Declared, new ContainerAppStatus(ContainerAppState.NotResponding, Silent(Opened, "  ")), Utc));
        Assert.Equal(title, ContainerAppText.Title("ui", Declared, new ContainerAppStatus(ContainerAppState.NotResponding, Silent(Opened, null)), Utc));
    }

    /// <summary>
    /// A name that is only whitespace is not a name that is not there: the whitespace stands where the name was,
    /// and the title is not the one the empty name gets, which <see cref="RuntimeWatchEdgeCaseTests" /> pins -
    /// the host stands after the whitespace, and the wait the page gives a scaled-to-zero app is said to the
    /// second.
    /// </summary>
    [Fact]
    public void ANameThatIsOnlyWhitespaceStandsWhereTheNameWas()
    {
        const string title =
            "  (ca-demo-tdd-ui.graybush-1.southcentralus.azurecontainerapps.io): Probing. "
            + "This page asks it once a minute while it shows this diagram, and not while it is paused or hidden; "
            + "nothing asks it from here while no page is open, so it can scale to zero. "
            + "An app that scaled to zero starts on the first request: the page waits up to 90 seconds for its first answer before it says Not responding.";

        Assert.Equal(title, ContainerAppText.Title(" ", Declared, ContainerAppStatus.NotYet, Utc));
        Assert.NotEqual(title, ContainerAppText.Title(string.Empty, Declared, ContainerAppStatus.NotYet, Utc));
    }

    /// <summary>
    /// The tooltip says the host of the address, and only the host: the port the address carries is said nowhere
    /// in the tooltip, the host standing alone where the name was.
    /// </summary>
    [Fact]
    public void TheTooltipSaysTheHostOfTheAddressAndNotItsPort()
    {
        var title = ContainerAppText.Title("ui", WithPort, ContainerAppStatus.NotYet, Utc);

        Assert.StartsWith("ui (app.example.net): Probing.", title, StringComparison.Ordinal);
        Assert.DoesNotContain(":8443", title, StringComparison.Ordinal);
    }

    /// <summary>
    /// The clock of the last probe is the viewer's, east of the meridian as well as west, which
    /// <see cref="HealthHelperCoverageTests" /> pins: the same moment is said in the hours of the zone the viewer
    /// stands in, and a moment that stands on an offset of its own is said in the hours it stands on in each of
    /// them.
    /// </summary>
    [Fact]
    public void TheTooltipSaysTheMomentInTheZoneItIsViewedFrom()
    {
        var utc = new ContainerAppStatus(ContainerAppState.Healthy, Heard(200, new DateTimeOffset(2026, 10, 6, 23, 20, 5, TimeSpan.Zero)));

        Assert.Contains("Last probe 04:50:05:", ContainerAppText.Title("ui", Declared, utc, PlusFiveThirty), StringComparison.Ordinal);

        var own = new ContainerAppStatus(ContainerAppState.Healthy, Heard(200, new DateTimeOffset(2026, 10, 7, 4, 50, 5, TimeSpan.FromHours(5.5))));

        Assert.Contains("Last probe 23:20:05:", ContainerAppText.Title("ui", Declared, own, Utc), StringComparison.Ordinal);
        Assert.Contains("Last probe 04:50:05:", ContainerAppText.Title("ui", Declared, own, PlusFiveThirty), StringComparison.Ordinal);
    }

    /// <summary>
    /// The moments at the ends of the time are said as the hours they are: the first moment the time has and the
    /// last the time runs to, in the zone with no offset of its own.
    /// </summary>
    [Fact]
    public void TheTooltipSaysTheMomentsAtTheEndsOfTheTime()
    {
        var first = ContainerAppText.Title("ui", Declared, new ContainerAppStatus(ContainerAppState.Waking, Silent(DateTimeOffset.MinValue, null)), Utc);
        var last = ContainerAppText.Title("ui", Declared, new ContainerAppStatus(ContainerAppState.Waking, Silent(DateTimeOffset.MaxValue, null)), Utc);

        Assert.Contains("Last probe 00:00:00: /_healthcheck, no answer.", first, StringComparison.Ordinal);
        Assert.Contains("Last probe 23:59:59: /_healthcheck, no answer.", last, StringComparison.Ordinal);
    }
}
