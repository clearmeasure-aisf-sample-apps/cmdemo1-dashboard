using System.Globalization;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="ContainerAppStatus" />, a container app's state as
/// the page last saw it: the three parts it holds, and the record the compiler made around them.
/// <para>
/// What the other files already pin is left alone. <see cref="RuntimeWatchEdgeCaseTests" /> pins that two statuses
/// are one while their three parts are - the operator, <c>Equals</c> of the boxed other status and the hash code
/// that follows them - that a difference in any one part is another status, and that a copy naming no part is the
/// same status. <see cref="ContainerAppStateEdgeCaseTests" /> pins the status built of a state alone, for all six
/// states: no answer, no refusal, and the state's own enum name in the record's words. And
/// <see cref="ContainerAppProbeEdgeCaseTests" /> pins the one <see cref="ContainerAppStatus.NotYet" /> instance a
/// probe holds until it records, and the very answer object it then carries.
/// </para>
/// <para>
/// What is pinned here is the rest of the member: that the constructor keeps each of the three parts as it is given
/// them - the answer by the very reference it stands on, the unnamed parts as nothing and as false, and the parts
/// named out of their order; that the state a status says is the state it was given and never one read out of the
/// answer under it, over a contradicted answer of every kind; that a status is not one with anything that is not a
/// status, nor with the nothing a caller holds; that its hash code stands from call to call; that its three parts
/// read out of it in the order they stand in it; that <c>with</c> changes the part it names and carries the very
/// answer along; and how the status reads out when an answer really does stand under it and when a refusal is
/// remembered - the nested answer said in the answer's own words, which
/// <see cref="ContainerAppAnswerEdgeCaseTests" /> pins.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: every moment is one the test brings,
/// and the words the record says of them are checked in the hours every viewer shares. The words the page says of a
/// status are <see cref="ContainerAppTextEdgeCaseTests" />'s and the state an answer lands on is
/// <see cref="ContainerAppProbeEdgeCaseTests" />'s.
/// </para>
/// </summary>
public class ContainerAppStatusEdgeCaseTests
{
    /// <summary>The moment every answer below stands on: a fixed moment, not the clock.</summary>
    private static readonly DateTimeOffset Opened = new(2026, 10, 4, 22, 0, 0, TimeSpan.Zero);

    /// <summary>The health path the answers below stand on, the one the page asks.</summary>
    private const string HealthPath = "/_healthcheck";

    /// <summary>The answer of an app that said nothing, with the page's own word for why.</summary>
    private static ContainerAppAnswer Silent(string? detail = "No answer within 10 s.") =>
        new(HealthPath, true, null, false, null, Opened, detail);

    /// <summary>The answer of an app the page reached at its health path.</summary>
    private static ContainerAppAnswer Heard(int status, bool healthyPayload = false) =>
        new(HealthPath, true, status, false, 85, Opened, HealthyPayload: healthyPayload);

    // ----- the three parts the status keeps

    /// <summary>
    /// The status keeps the three parts it was given, each in its own part and each as it is given: the answer held
    /// by the very reference it stands on, the refusal as it stands, and the parts the caller did not name standing
    /// as the nothing and the <see langword="false" /> they fall to - named out of their order or not.
    /// </summary>
    [Fact]
    public void TheStatusKeepsTheThreePartsItWasGiven()
    {
        var answer = Heard(200, healthyPayload: true);
        var held = new ContainerAppStatus(ContainerAppState.Healthy, answer, true);

        Assert.Equal(ContainerAppState.Healthy, held.State);
        Assert.Same(answer, held.Last);
        Assert.True(held.CorsRefused);

        // Out of their order: the caller may name the parts it gives and leave the answer out.
        var named = new ContainerAppStatus(ContainerAppState.NotResponding, CorsRefused: true);

        Assert.Equal(ContainerAppState.NotResponding, named.State);
        Assert.Null(named.Last);
        Assert.True(named.CorsRefused);

        // Given explicitly, the nothing and the false stand as the nothing and the false they are.
        var empty = new ContainerAppStatus(ContainerAppState.Waking, null, false);

        Assert.Null(empty.Last);
        Assert.False(empty.CorsRefused);

        // The answer the status holds keeps every part of itself: the status borrows nothing from it.
        var silent = Silent();
        Assert.Same(silent, new ContainerAppStatus(ContainerAppState.NotResponding, silent).Last);
        Assert.Equal(silent.Detail, new ContainerAppStatus(ContainerAppState.NotResponding, silent).Last!.Detail);
    }

    /// <summary>
    /// The state a status says is the state it was given and never one read out of the answer under it: every one
    /// of the six states stands over an answer that contradicts it - a healthy word over a 503, a 404 the status
    /// calls merely probing - and over no answer at all, with the refusal standing as given beside them. The rule
    /// that makes the state out of the answer is the probe's, and no status of its own holds it.
    /// </summary>
    [Theory]
    [InlineData(ContainerAppState.Probing)]
    [InlineData(ContainerAppState.Waking)]
    [InlineData(ContainerAppState.Responding)]
    [InlineData(ContainerAppState.Healthy)]
    [InlineData(ContainerAppState.Unhealthy)]
    [InlineData(ContainerAppState.NotResponding)]
    public void TheStateStandsAsGivenWhicheverAnswerStandsUnderIt(ContainerAppState state)
    {
        var gone = Heard(404);
        var broken = Heard(503, healthyPayload: true);
        var nothing = Silent();

        foreach (var answer in new ContainerAppAnswer?[] { gone, broken, nothing, null })
        {
            var status = new ContainerAppStatus(state, answer, CorsRefused: answer is null);

            Assert.Equal(state, status.State);
            Assert.Same(answer, status.Last);
            Assert.Equal(answer is null, status.CorsRefused);
            Assert.False(status.Equals(status with { CorsRefused = answer is not null }));
        }
    }

    // ----- the record around them

    /// <summary>
    /// A status is one with nothing that is not a status of the same three parts: another thing that happens to be
    /// a word, the state worn as a boxed value and one answer of an app are all refused - the operator seeing the
    /// nothing a caller holds, and the one <see cref="ContainerAppStatus.NotYet" /> standing where a fresh status of
    /// the same three nothings stands.
    /// </summary>
    [Fact]
    public void AStatusIsNotOneWithAnythingThatIsNotAStatus()
    {
        var one = new ContainerAppStatus(ContainerAppState.Healthy, Heard(200, healthyPayload: true), true);
        ContainerAppStatus? nothing = null;

        Assert.False(one.Equals("ContainerAppStatus"));
        Assert.False(one.Equals(ContainerAppState.Healthy));
        Assert.False(one.Equals(Heard(200, healthyPayload: true)));

        var again = one with { };

        Assert.True(one.Equals(one));
        Assert.True(one == again);
        Assert.False(one != again);
        Assert.False(one == nothing);
        Assert.True(one != nothing);
        Assert.False(ContainerAppStatus.NotYet == nothing);
        Assert.True(ContainerAppStatus.NotYet == new ContainerAppStatus(ContainerAppState.Probing));
        Assert.False(ContainerAppStatus.NotYet == one);
    }

    /// <summary>
    /// The hash code follows the three parts and stands where it stands: the same status says the same number from
    /// call to call, two statuses made of equal answers in separate instances say one number, and the one
    /// <see cref="ContainerAppStatus.NotYet" /> says the number a status of nothing says.
    /// </summary>
    [Fact]
    public void TheHashCodeFollowsTheThreePartsAndStandsFromCallToCall()
    {
        var one = new ContainerAppStatus(ContainerAppState.Healthy, Heard(200, healthyPayload: true), true);
        var again = new ContainerAppStatus(ContainerAppState.Healthy, Heard(200, healthyPayload: true), true);

        Assert.Equal(one.GetHashCode(), one.GetHashCode());
        Assert.Equal(one.GetHashCode(), again.GetHashCode());
        Assert.Equal(ContainerAppStatus.NotYet.GetHashCode(), new ContainerAppStatus(ContainerAppState.Probing).GetHashCode());

        var held = ContainerAppStatus.NotYet with { Last = Silent(string.Empty) };
        Assert.Equal(held.GetHashCode(), new ContainerAppStatus(ContainerAppState.Probing, Silent(string.Empty)).GetHashCode());
        Assert.Equal(held.GetHashCode(), held.GetHashCode());
    }

    /// <summary>The three parts read out of the status in the order they stand in it - the status of nothing
    /// reading out its nothing and its false.</summary>
    [Fact]
    public void TheThreePartsReadOutOfTheStatusInTheOrderTheyStand()
    {
        var answer = Silent();
        var (state, last, refused) = new ContainerAppStatus(ContainerAppState.NotResponding, answer, true);

        Assert.Equal(ContainerAppState.NotResponding, state);
        Assert.Same(answer, last);
        Assert.True(refused);

        var (probing, nothing, noRefusal) = ContainerAppStatus.NotYet;

        Assert.Equal(ContainerAppState.Probing, probing);
        Assert.Null(nothing);
        Assert.False(noRefusal);
    }

    /// <summary>
    /// <c>with</c> names the part it changes and leaves the rest, the copy holding the very answer the original
    /// holds rather than a copy of it: the state moved keeps the answer and the refusal, the answer emptied leaves
    /// the state and the refusal standing, the refusal forgiven leaves the answer, and the copy that names no part
    /// is the same status and not the same instance.
    /// </summary>
    [Fact]
    public void WithChangesOnlyThePartItNamesAndCarriesTheVeryAnswerAlong()
    {
        var answer = Heard(200, healthyPayload: true);
        var one = new ContainerAppStatus(ContainerAppState.Healthy, answer, true);
        var copy = one with { };

        Assert.Equal(one, copy);
        Assert.NotSame(one, copy);
        Assert.Same(answer, copy.Last);

        var moved = one with { State = ContainerAppState.Unhealthy };

        Assert.Equal(ContainerAppState.Unhealthy, moved.State);
        Assert.Same(answer, moved.Last);
        Assert.True(moved.CorsRefused);

        var emptied = one with { Last = null };

        Assert.Null(emptied.Last);
        Assert.Equal(ContainerAppState.Healthy, emptied.State);
        Assert.True(emptied.CorsRefused);
        Assert.Equal(new ContainerAppStatus(ContainerAppState.Healthy, null, true), emptied);

        var forgiven = one with { CorsRefused = false };

        Assert.False(forgiven.CorsRefused);
        Assert.Same(answer, forgiven.Last);
        Assert.Equal(ContainerAppState.Healthy, forgiven.State);
    }

    /// <summary>
    /// A status that has an answer under it says all three of its parts in their order, the nested answer said in
    /// the answer's own words - the no answer saying its empty status, its empty latency and its word for why, and
    /// the refusal remembered saying <c>True</c> where <see cref="ContainerAppStateEdgeCaseTests" /> pins it saying
    /// <c>False</c> of a status with no answer at all. (The moment and the number in those words are the viewer's,
    /// so the words are checked in the hours every viewer shares.)
    /// </summary>
    [Fact]
    public void TheStatusSaysItsPartsInTheirOrderWhenAnAnswerStandsUnderIt()
    {
        var culture = Thread.CurrentThread.CurrentCulture;
        try
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            Assert.Equal(
                "ContainerAppStatus { State = Healthy, Last = ContainerAppAnswer { Path = /_healthcheck, ReadsHealth = True, StatusCode = 200, Opaque = False, LatencyMs = 85, CheckedAt = 10/04/2026 22:00:00 +00:00, Detail = , HealthyPayload = True, Answered = True }, CorsRefused = True }",
                new ContainerAppStatus(ContainerAppState.Healthy, Heard(200, healthyPayload: true), true).ToString());

            Assert.Equal(
                "ContainerAppStatus { State = NotResponding, Last = ContainerAppAnswer { Path = /_healthcheck, ReadsHealth = True, StatusCode = , Opaque = False, LatencyMs = , CheckedAt = 10/04/2026 22:00:00 +00:00, Detail = No answer within 10 s., HealthyPayload = False, Answered = False }, CorsRefused = False }",
                new ContainerAppStatus(ContainerAppState.NotResponding, Silent()).ToString());

            // The status of nothing says its nothing, and the status of the empty detail says the detail it is.
            Assert.Equal(
                "ContainerAppStatus { State = Waking, Last = , CorsRefused = True }",
                new ContainerAppStatus(ContainerAppState.Waking, null, true).ToString());
            Assert.Equal(
                "ContainerAppStatus { State = Unhealthy, Last = ContainerAppAnswer { Path = /_healthcheck, ReadsHealth = True, StatusCode = , Opaque = False, LatencyMs = , CheckedAt = 10/04/2026 22:00:00 +00:00, Detail = , HealthyPayload = False, Answered = False }, CorsRefused = False }",
                new ContainerAppStatus(ContainerAppState.Unhealthy, Silent(string.Empty)).ToString());
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = culture;
        }
    }
}
