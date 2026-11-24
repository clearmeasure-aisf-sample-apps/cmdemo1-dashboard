namespace Dashboard.Tests;

/// <summary>
/// The one place that says what a <see cref="ContainerAppAnswer" /> heard (#593):
/// <see cref="ContainerAppAnswer.SaidShort" /> for the facts next to the badge,
/// <see cref="ContainerAppAnswer.SaidIn" /> for the tooltip's sentence and <see cref="ContainerAppAnswer.CannotTell" />
/// for the follow-up. <see cref="ContainerAppText.Facts" /> and <see cref="ContainerAppText.Title" /> must say exactly
/// what these say, so a new answer shape is taught once.
/// </summary>
public class ContainerAppAnswerWordsTests
{
    private static readonly DateTimeOffset Checked = new(2026, 10, 4, 22, 0, 0, TimeSpan.Zero);

    private static readonly ContainerAppTarget Target = new(new Uri("https://app.example.com"));

    [Theory]
    [InlineData(200, false, 85, false, "HTTP 200 · 85 ms")]
    [InlineData(503, false, null, false, "HTTP 503")]
    [InlineData(200, true, 85, false, "HTTP 200 · 85 ms")]
    [InlineData(null, true, 85, false, "answered · 85 ms")]
    [InlineData(null, true, null, false, "answered")]
    [InlineData(null, false, null, true, "no answer yet")]
    [InlineData(null, false, null, false, "no answer")]
    [InlineData(null, false, 85, false, "no answer")]
    public void SaidShortNamesEachShape(int? code, bool opaque, int? ms, bool waking, string expected)
    {
        var answer = new ContainerAppAnswer("/h", true, code, opaque, ms, Checked);

        Assert.Equal(expected, answer.SaidShort(waking));
    }

    [Theory]
    [InlineData(200, false, null, "Last probe 22:00: /h answered HTTP 200.")]
    [InlineData(404, true, null, "Last probe 22:00: /h answered HTTP 404.")]
    [InlineData(null, false, "No answer within 10 s.", "Last probe 22:00: /h, no answer. No answer within 10 s.")]
    [InlineData(null, false, null, "Last probe 22:00: /h, no answer.")]
    [InlineData(null, false, "", "Last probe 22:00: /h, no answer.")]
    public void SaidInNamesEachShape(int? code, bool opaque, string? detail, string expected)
    {
        var answer = new ContainerAppAnswer("/h", true, code, opaque, null, Checked, detail);

        Assert.Equal(expected, answer.SaidIn("22:00"));
    }

    [Fact]
    public void SaidInSaysTheOpaqueAnswerCannotBeRead()
    {
        var answer = new ContainerAppAnswer("/h", true, null, true, null, Checked);

        Assert.Equal(
            "Last probe 22:00: /h answered. The app does not allow this page's origin (CORS), so the page cannot read its answer or its health; it asks without reading the answer.",
            answer.SaidIn("22:00"));
    }

    [Theory]
    [InlineData(200, false, false, "The page could not classify the answer as a health check.")]
    [InlineData(404, false, false, "The page could not classify the answer as a health check.")]
    [InlineData(null, true, false, "The page could not classify the answer as a health check.")]
    [InlineData(404, false, true, "HTTP 404 at the health path (including Azure's \"Container App - Unavailable\" page for a deleted app) means the app is down, not that it is responding.")]
    [InlineData(200, false, true, null)]
    [InlineData(503, false, true, null)]
    [InlineData(null, true, true, null)]
    [InlineData(null, false, true, null)]
    [InlineData(null, false, false, null)]
    public void CannotTellReturnsTheFollowUpOrNull(int? code, bool opaque, bool readsHealth, string? expected)
    {
        var answer = new ContainerAppAnswer("/h", readsHealth, code, opaque, 85, Checked);

        Assert.Equal(expected, answer.CannotTell());
    }

    [Theory]
    [InlineData(200, false, 85, ContainerAppState.Healthy)]
    [InlineData(null, true, null, ContainerAppState.Responding)]
    [InlineData(null, false, null, ContainerAppState.Waking)]
    [InlineData(null, false, null, ContainerAppState.NotResponding)]
    public void FactsSaysWhatTheAnswerSaysShort(int? code, bool opaque, int? ms, ContainerAppState state)
    {
        var answer = new ContainerAppAnswer("/h", true, code, opaque, ms, Checked);

        Assert.Equal(
            answer.SaidShort(state == ContainerAppState.Waking),
            ContainerAppText.Facts(new ContainerAppStatus(state, answer)));
    }

    [Theory]
    [InlineData(404, false, true)]
    [InlineData(200, false, false)]
    [InlineData(null, true, true)]
    [InlineData(null, false, true)]
    public void TitleCarriesTheAnswerSentenceAndItsFollowUp(int? code, bool opaque, bool readsHealth)
    {
        var answer = new ContainerAppAnswer("/h", readsHealth, code, opaque, null, Checked, "Gone.");
        var title = ContainerAppText.Title("app", Target, new ContainerAppStatus(ContainerAppState.Unhealthy, answer), TimeZoneInfo.Utc);
        var said = answer.SaidIn(TimeText.Clock(Checked, TimeZoneInfo.Utc));
        var followUp = answer.CannotTell();

        Assert.Contains(followUp is null ? said : $"{said} {followUp}", title, StringComparison.Ordinal);
        Assert.EndsWith(followUp ?? said, title, StringComparison.Ordinal);
    }

    [Fact]
    public void TitleWithoutAnAnswerSaysNoProbeSentence()
    {
        var title = ContainerAppText.Title("app", Target, ContainerAppStatus.NotYet, TimeZoneInfo.Utc);

        Assert.DoesNotContain("Last probe", title, StringComparison.Ordinal);
        Assert.StartsWith("app (app.example.com): Probing. This page asks it once a minute", title, StringComparison.Ordinal);
        Assert.EndsWith("before it says Not responding.", title, StringComparison.Ordinal);
    }
}
