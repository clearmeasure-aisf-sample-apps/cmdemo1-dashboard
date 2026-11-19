using System.Globalization;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="ContainerAppAnswer" />, one probe of a container
/// app. <see cref="ContainerAppProbeTests" /> pins the answer the prober makes - the default path it asks, the
/// status and the latency it saw, the unread answer it cannot read, the reason it gives for no answer and
/// <see cref="ContainerAppAnswer.Answered" /> for an unread answer and for none at all - and
/// <see cref="HealthHelperCoverageTests" /> and <see cref="RuntimePayloadHelperTests" /> pin the words the answer
/// becomes in the badge, the facts and the tooltip; neither pins the parts of the answer itself. What is pinned
/// here: that the answer keeps each part it was given - the path named, that names nothing, and the one it did not
/// name, the status at the boundaries of the codes the probe classifies and the extremes of the number, the
/// latency at its own boundaries, the time with the offset it carries, the detail and the three flags, each as
/// given; the parts it did not name, standing unnamed; what <see cref="ContainerAppAnswer.Answered" /> says while
/// the answer has a status, was answered without being read, has both or has neither; and how the answer
/// compares, copies, reads out and says its name.
/// </summary>
public class ContainerAppAnswerEdgeCaseTests
{
    /// <summary>The time <see cref="ContainerAppProbeTests" /> opens the page at: a fixed moment, not the clock.</summary>
    private static readonly DateTimeOffset Checked = new(2026, 10, 4, 22, 0, 0, TimeSpan.Zero);

    // ----- the parts the answer keeps, as given

    /// <summary>
    /// The answer keeps each part it was given, each in its own part: the path it asked, whether that path reads
    /// as a health check, the status the app sent, whether the answer was one the browser could not read, the
    /// time it took to the answer, the moment the probe started and the reason for the no-answer.
    /// </summary>
    [Fact]
    public void TheAnswerKeepsThePartsItWasGiven()
    {
        var answer = new ContainerAppAnswer("/health", true, 200, false, 85, Checked, "No answer within 10 s.", true);

        Assert.Equal("/health", answer.Path);
        Assert.True(answer.ReadsHealth);
        Assert.Equal(200, answer.StatusCode);
        Assert.False(answer.Opaque);
        Assert.Equal(85, answer.LatencyMs);
        Assert.Equal(Checked, answer.CheckedAt);
        Assert.Equal("No answer within 10 s.", answer.Detail);
        Assert.True(answer.HealthyPayload);
    }

    /// <summary>
    /// The path the answer stands on stands as given: a path of the app's own, the default spelled again as the
    /// path asked, the empty path the deployable may still name, the whitespace path, and the path nobody named.
    /// The default path itself, <see cref="ContainerAppProbeTests" /> pins.
    /// </summary>
    [Theory]
    [InlineData("/health")]
    [InlineData("/_healthcheck")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void ThePathTheAnswerStandsOnStandsAsGiven(string? path)
    {
        Assert.Equal(path, new ContainerAppAnswer(path!, true, null, false, null, Checked).Path);
    }

    /// <summary>
    /// The status the answer carries is kept as given, without the answer classifying it: the answer no one sent,
    /// the codes at the boundaries the probe classifies - the two hundreds the probe takes, the four four four it
    /// takes for the app gone, and the five hundreds it takes for the app broken - and the extremes of the
    /// number. The classification itself, <see cref="ContainerAppProbeTests" /> pins.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData(int.MinValue)]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(199)]
    [InlineData(200)]
    [InlineData(299)]
    [InlineData(300)]
    [InlineData(404)]
    [InlineData(499)]
    [InlineData(500)]
    [InlineData(599)]
    [InlineData(600)]
    [InlineData(int.MaxValue)]
    public void TheStatusTheAnswerCarriesIsKeptAsGiven(int? statusCode)
    {
        Assert.Equal(statusCode, new ContainerAppAnswer("/health", true, statusCode, false, null, Checked).StatusCode);
    }

    /// <summary>
    /// The latency the answer carries is kept as given: no time to the answer, the zero and the negative the
    /// number may still stand on, the time the page saw, and the extremes of the number.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData(int.MinValue)]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(85)]
    [InlineData(int.MaxValue)]
    public void TheLatencyTheAnswerCarriesIsKeptAsGiven(int? latencyMs)
    {
        Assert.Equal(latencyMs, new ContainerAppAnswer("/health", true, 200, false, latencyMs, Checked).LatencyMs);
    }

    /// <summary>
    /// The time the answer stands on is kept with the offset it carries: the moment in its own hours, the moment
    /// in the hours of a viewer east and of one west of the meridian, and the ends of the time the number runs to.
    /// </summary>
    [Fact]
    public void TheTimeTheAnswerStandsOnIsKeptWithItsOffset()
    {
        var utc = new DateTimeOffset(2026, 10, 4, 22, 0, 0, TimeSpan.Zero);
        var east = new DateTimeOffset(2026, 10, 5, 3, 30, 0, TimeSpan.FromHours(5.5));
        var west = new DateTimeOffset(2026, 10, 4, 16, 30, 0, TimeSpan.FromMinutes(-330));

        Assert.Equal(utc, new ContainerAppAnswer("/health", true, 200, false, 85, utc).CheckedAt);
        Assert.Equal(east, new ContainerAppAnswer("/health", true, 200, false, 85, east).CheckedAt);
        Assert.Equal(west, new ContainerAppAnswer("/health", true, 200, false, 85, west).CheckedAt);
        Assert.Equal(DateTimeOffset.MinValue, new ContainerAppAnswer("/health", true, 200, false, 85, DateTimeOffset.MinValue).CheckedAt);
        Assert.Equal(DateTimeOffset.MaxValue, new ContainerAppAnswer("/health", true, 200, false, 85, DateTimeOffset.MaxValue).CheckedAt);
    }

    /// <summary>
    /// The detail the answer carries stands as given: the reason the page gives for no answer, the empty detail
    /// and the whitespace detail the caller may still give, and no detail at all. The word the prober gives,
    /// <see cref="ContainerAppProbeTests" /> pins.
    /// </summary>
    [Theory]
    [InlineData("No answer within 10 s.")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void TheDetailTheAnswerCarriesStandsAsGiven(string? detail)
    {
        Assert.Equal(detail, new ContainerAppAnswer("/health", true, null, false, null, Checked, detail).Detail);
    }

    /// <summary>
    /// The flags the answer carries stand as given: whether the path it asked reads as a health check, whether
    /// the answer was one the browser could not read, and whether the body it read is a healthy health-check
    /// answer - every way the three stand together.
    /// </summary>
    [Theory]
    [InlineData(false, false, false)]
    [InlineData(false, false, true)]
    [InlineData(false, true, false)]
    [InlineData(false, true, true)]
    [InlineData(true, false, false)]
    [InlineData(true, false, true)]
    [InlineData(true, true, false)]
    [InlineData(true, true, true)]
    public void TheFlagsTheAnswerCarriesStandAsGiven(bool readsHealth, bool opaque, bool healthyPayload)
    {
        var answer = new ContainerAppAnswer("/health", readsHealth, 200, opaque, 85, Checked, HealthyPayload: healthyPayload);

        Assert.Equal(readsHealth, answer.ReadsHealth);
        Assert.Equal(opaque, answer.Opaque);
        Assert.Equal(healthyPayload, answer.HealthyPayload);
    }

    /// <summary>The parts the answer did not name stand unnamed: the reason for the no-answer stands as nothing,
    /// and the body it read stands as not a healthy one - the six parts the probe gives are enough.</summary>
    [Fact]
    public void ThePartsTheAnswerDidNotNameStandUnnamed()
    {
        var answer = new ContainerAppAnswer("/health", true, 200, false, 85, Checked);

        Assert.Null(answer.Detail);
        Assert.False(answer.HealthyPayload);
    }

    // ----- what answered means

    /// <summary>
    /// The answer says it answered while it has a status, whether the code it stands on is a two hundred the page
    /// may read or a code nobody classifies, while the browser reached the app and could not read the answer, and
    /// while it has both - and says it did not while it has neither. The unread answer, which
    /// <see cref="ContainerAppProbeTests" /> pins, stands here beside the status it did not pin.
    /// </summary>
    [Theory]
    [InlineData(200, false, true)]
    [InlineData(404, false, true)]
    [InlineData(0, false, true)]
    [InlineData(-1, false, true)]
    [InlineData(int.MaxValue, false, true)]
    [InlineData(null, true, true)]
    [InlineData(200, true, true)]
    [InlineData(null, false, false)]
    public void TheAnswerSaysItAnsweredWhileItHasAStatusOrWasAnswered(int? statusCode, bool opaque, bool answered)
    {
        Assert.Equal(answered, new ContainerAppAnswer("/health", true, statusCode, opaque, null, Checked).Answered);
    }

    // ----- the record itself

    /// <summary>Two answers are one while their eight parts are: the equality of the operator and of
    /// <c>Equals</c>, and the hash code that follows them, standing where it stands from call to call.</summary>
    [Fact]
    public void TwoAnswersAreOneWhileTheirPartsAre()
    {
        var one = new ContainerAppAnswer("/health", true, 200, false, 85, new DateTimeOffset(2026, 10, 4, 22, 0, 0, TimeSpan.Zero), "No answer within 10 s.", true);
        var two = new ContainerAppAnswer("/health", true, 200, false, 85, new DateTimeOffset(2026, 10, 4, 22, 0, 0, TimeSpan.Zero), "No answer within 10 s.", true);

        Assert.True(one == two);
        Assert.True(one.Equals((object)two));
        Assert.True(two.Equals((object)one));
        Assert.Equal(one, two);
        Assert.Equal(one.GetHashCode(), two.GetHashCode());
        Assert.Equal(one.GetHashCode(), one.GetHashCode());
        Assert.False(one.Equals(null));
        Assert.False(one.Equals("ContainerAppAnswer"));
    }

    /// <summary>An answer differs in either of its parts: a path in another spelling or the one nobody named, a
    /// flag turned, a status moved to another code or to none, a latency moved to another number or to none, a
    /// moment a second off, and a detail emptied, blanked or named at all.</summary>
    [Fact]
    public void AnAnswerDiffersInEitherOfItsParts()
    {
        var answer = new ContainerAppAnswer("/health", true, 200, false, 85, Checked, "No answer within 10 s.", true);

        Assert.True(answer != answer with { Path = null! });
        Assert.NotEqual(answer, answer with { Path = "" });
        Assert.NotEqual(answer, answer with { Path = " " });
        Assert.NotEqual(answer, answer with { Path = "/_healthcheck" });
        Assert.NotEqual(answer, answer with { ReadsHealth = false });
        Assert.NotEqual(answer, answer with { StatusCode = null });
        Assert.NotEqual(answer, answer with { StatusCode = 201 });
        Assert.NotEqual(answer, answer with { StatusCode = 404 });
        Assert.NotEqual(answer, answer with { Opaque = true });
        Assert.NotEqual(answer, answer with { LatencyMs = null });
        Assert.NotEqual(answer, answer with { LatencyMs = 86 });
        Assert.NotEqual(answer, answer with { CheckedAt = Checked.AddSeconds(1) });
        Assert.NotEqual(answer, answer with { Detail = null });
        Assert.NotEqual(answer, answer with { Detail = "" });
        Assert.NotEqual(answer, answer with { Detail = " " });
        Assert.NotEqual(answer, answer with { HealthyPayload = false });

        Assert.NotEqual(new ContainerAppAnswer("/health", true, 200, false, 85, Checked), new ContainerAppAnswer("/health", true, 200, false, 85, Checked, ""));
        Assert.NotEqual(new ContainerAppAnswer("/health", true, 200, false, 85, Checked, ""), new ContainerAppAnswer("/health", true, 200, false, 85, Checked, " "));
    }

    /// <summary><c>with</c> names the part it changes and leaves the rest: the moved path keeps its status and
    /// its time, the emptied status stands emptied, and the answer that names no part is equal and not the same.</summary>
    [Fact]
    public void WithChangesOnlyThePartItNames()
    {
        var answer = new ContainerAppAnswer("/health", true, 200, false, 85, Checked, "No answer within 10 s.", true);
        var copy = answer with { };

        Assert.Equal(answer, copy);
        Assert.NotSame(answer, copy);

        var restated = answer with { Path = "/_healthcheck" };
        Assert.Equal("/_healthcheck", restated.Path);
        Assert.True(restated.ReadsHealth);
        Assert.Equal(200, restated.StatusCode);
        Assert.False(restated.Opaque);
        Assert.Equal(85, restated.LatencyMs);
        Assert.Equal(Checked, restated.CheckedAt);
        Assert.Equal("No answer within 10 s.", restated.Detail);
        Assert.True(restated.HealthyPayload);

        var unanswered = answer with { StatusCode = null, HealthyPayload = false };
        Assert.Null(unanswered.StatusCode);
        Assert.False(unanswered.HealthyPayload);
        Assert.Equal("/health", unanswered.Path);
        Assert.True(unanswered.ReadsHealth);
        Assert.Equal(85, unanswered.LatencyMs);
    }

    /// <summary>The eight parts read out of the answer in the order they stand in it - the no answer reading out
    /// its parts as they stand: the path asked, the status it did not get, the answer it could not read, and the
    /// detail and the health it did not carry.</summary>
    [Fact]
    public void TheEightPartsReadOutOfTheAnswer()
    {
        var (path, reads, code, opaque, ms, checkedAt, detail, healthy) =
            new ContainerAppAnswer("/health", true, 200, false, 85, Checked, "No answer within 10 s.", true);

        Assert.Equal("/health", path);
        Assert.True(reads);
        Assert.Equal(200, code);
        Assert.False(opaque);
        Assert.Equal(85, ms);
        Assert.Equal(Checked, checkedAt);
        Assert.Equal("No answer within 10 s.", detail);
        Assert.True(healthy);

        var (root, _, noCode, answered, noLatency, _, noDetail, notHealthy) =
            new ContainerAppAnswer("/", false, null, true, null, Checked);

        Assert.Equal("/", root);
        Assert.Null(noCode);
        Assert.True(answered);
        Assert.Null(noLatency);
        Assert.Null(noDetail);
        Assert.False(notHealthy);
    }

    /// <summary>The answer says its parts in their order, the name of each with the value it stands on - the part
    /// it did not carry says its name and nothing after it, the moment said in the hours it stands on, and the
    /// offset said with it, and the word that says it answered last. (The words the number and the moment say are
    /// the viewer's, so the words are checked in the hours every viewer shares.)</summary>
    [Fact]
    public void TheAnswerSaysItsPartsInTheirOrder()
    {
        var culture = Thread.CurrentThread.CurrentCulture;
        try
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            Assert.Equal(
                "ContainerAppAnswer { Path = /health, ReadsHealth = True, StatusCode = 200, Opaque = False, LatencyMs = 85, CheckedAt = 10/04/2026 22:00:00 +00:00, Detail = No answer within 10 s., HealthyPayload = True, Answered = True }",
                new ContainerAppAnswer("/health", true, 200, false, 85, Checked, "No answer within 10 s.", true).ToString());
            Assert.Equal(
                "ContainerAppAnswer { Path = /_healthcheck, ReadsHealth = False, StatusCode = , Opaque = True, LatencyMs = , CheckedAt = 01/01/2026 00:00:00 +05:00, Detail = , HealthyPayload = False, Answered = True }",
                new ContainerAppAnswer("/_healthcheck", false, null, true, null, new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.FromHours(5))).ToString());
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = culture;
        }
    }
}
