using System.Collections;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty, inconsistent and fixed-value tests of <see cref="HealthSummary" />, the counts the
/// header weighs its round on, declared in <c>src/Dashboard/Health/HealthSummary.cs</c>: the record of three counts
/// with <c>NotHealthy</c> subtracted from them, <c>Level</c> fallen out of the three and <c>Text</c> worded from all
/// four, and <c>Of</c>, the one way the counts are counted out of a heap of answers.
/// <para>
/// What the counts say for the answers a round really brings is pinned where the summary is used:
/// <see cref="HealthSummaryTests" /> pins the level, the words and <c>NotHealthy</c> for every combination a round
/// produces - all healthy, a problem among the answers, everything waiting, answers arriving, a problem before every
/// answer arrived - together with the topology of no nodes and the three singular readings;
/// <see cref="SummaryLevelEdgeCaseTests" /> pins <c>NotHealthy</c> and the level of the eight exact count triples the
/// public constructor is asked to weigh - of the counts that add up and of the one triple that cannot be
/// (<c>10, 20, 5</c>) - and the four levels themselves, their numbers, their names, their order and the text that is
/// and is not one of them.
/// </para>
/// <para>
/// What both of those leave open is pinned here, and only that: the words of counts that cannot be and of counts at
/// the ends of <see cref="int" /> - the singular noun a total of one wears under counts that are nothing like a total
/// of one, the empty reading taken on the total alone, negative numbers said as the numbers they are, and a
/// subtraction that wrapped its way into a problem (<c>Text</c>, which neither of those files reads at all);
/// <c>NotHealthy</c> at the ends of <see cref="int" />, where the subtraction is not <c>checked</c> and wraps, so a
/// total of nothing carries the largest count there is; <c>Of</c> refusing nothing as its source - a source walked
/// over before <c>Of</c> answers, the same source counted the same twice over, a source found changed between two
/// calls and counted as the call found it, a source that dies mid-walk or refuses the walk outright with its
/// exception carried out untouched, and a heap that changes under the walk; and the record that the three counts
/// make - the same counts made twice being one summary, one count moved making another, the same words worded from
/// different counts not being one summary, and <c>with</c> moving the one count it names and carrying the other two
/// along out of a copy that is not the summary it was copied from.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: every count and every heap of answers
/// is one the test brings.
/// </para>
/// </summary>
public class HealthSummaryEdgeCaseTests
{
    private const HealthState Healthy = HealthState.Healthy;
    private const HealthState Unhealthy = HealthState.Unhealthy;
    private const HealthState Pending = HealthState.Pending;
    private const HealthState Unreachable = HealthState.Unreachable;

    /// <summary>A summary built of the three counts the header weighs: the total, the healthy count, the pending count.</summary>
    private static HealthSummary Summary(int total, int healthy, int pending) => new(total, healthy, pending);

    /// <summary>A heap of answers built from the answers given, in the order given.</summary>
    private static HealthState[] States(params HealthState[] states) => states;

    /// <summary>A heap of <paramref name="count" /> answers that all say the one <paramref name="state" />.</summary>
    private static HealthState[] States(HealthState state, int count) =>
        [.. Enumerable.Repeat(state, count)];

    // ----- the words of counts that cannot be, and of counts at the ends of int

    /// <summary>The words of counts that cannot be, and of counts at the ends of <see cref="int" />: a healthy count
    /// above the total still reads as everything being well, the reading of an empty topology is taken on the total
    /// alone whatever stands behind it, negative numbers are said as the negative numbers they are, the noun follows
    /// the total and nothing else - the one node of a total of one called <c>node</c> under a problem of three - and
    /// a subtraction that wrapped past the end of <see cref="int" /> comes back as a problem of the largest kind.</summary>
    [Theory]
    // A healthy count above the total leaves NotHealthy negative, which is no problem, and the words are the honest
    // ones of a round with nothing wrong - on counts that could never have produced them.
    [InlineData(2, 3, 0, "All 2 nodes healthy")]
    [InlineData(2, 4, -1, "All 2 nodes healthy")]
    // A total of nothing says the topology is empty whatever the counts behind it are; HealthSummaryTests pins the
    // words of a topology that really has no endpoint at all.
    [InlineData(0, 5, 5, "No nodes in the topology")]
    // A pending count on a total of less than nothing, with nothing healthy behind it and with something behind it.
    [InlineData(-3, 0, 4, "Checking -3 nodes")]
    [InlineData(3, -2, 5, "-2 of 3 nodes healthy, 5 being checked")]
    // A problem is counted, not measured, and is said as the number it is even when it is bigger than the total it
    // stands under.
    [InlineData(3, 1, -1, "3 of 3 nodes not healthy")]
    [InlineData(1, int.MaxValue, int.MaxValue, "3 of 1 node not healthy")]
    // The ends of int: the largest count there is, healthy throughout; a total that is nothing at all and not less,
    // with a pending count that wrapped into the largest problem there is; and a negative reading of every part.
    [InlineData(int.MaxValue, int.MaxValue, 0, "All 2147483647 nodes healthy")]
    [InlineData(int.MinValue, 0, 1, "2147483647 of -2147483648 nodes not healthy")]
    [InlineData(int.MinValue, int.MinValue, 4, "-2147483648 of -2147483648 nodes healthy, 4 being checked")]
    public void TheWordsFollowTheCountsTheyAreGiven(int total, int healthy, int pending, string expected) =>
        Assert.Equal(expected, Summary(total, healthy, pending).Text);

    // ----- NotHealthy at the ends of int

    /// <summary><c>NotHealthy</c> is a subtraction and nothing else, and the subtraction is not <c>checked</c>: the
    /// difference stands at the ends of <see cref="int" /> where the counts put it, and a difference that goes past
    /// either end comes back at the other - the total of nothing carrying the largest count there is, the largest
    /// count there is carrying nothing, and the sum of the two ends carrying one less than nothing. The level and
    /// the words of those extremes are pinned elsewhere as far as they are pinned at all:
    /// <see cref="SummaryLevelEdgeCaseTests" /> for the level of exact counts, <see cref="HealthSummaryTests" /> for
    /// the words of the counts a round brings.</summary>
    [Theory]
    [InlineData(int.MaxValue, 0, 0, int.MaxValue)]
    [InlineData(int.MinValue, 0, 0, int.MinValue)]
    [InlineData(int.MinValue, int.MinValue, int.MinValue, int.MinValue)]
    [InlineData(int.MaxValue, int.MinValue, 0, -1)]
    [InlineData(int.MinValue, 1, 0, int.MaxValue)]
    [InlineData(0, int.MaxValue, int.MaxValue, 2)]
    [InlineData(1, int.MaxValue, int.MaxValue, 3)]
    public void NotHealthyStandsWhereTheCountsPutItEvenPastTheEndsOfInt(int total, int healthy, int pending, int expected) =>
        Assert.Equal(expected, Summary(total, healthy, pending).NotHealthy);

    // ----- Of refuses nothing as its source

    /// <summary>Nothing at all is no heap of answers: <c>Of</c> says so before it counts anything, and says which of
    /// its own arguments was nothing.</summary>
    [Fact]
    public void OfRefusesNothingAsItsSource()
    {
        var fault = Assert.Throws<ArgumentNullException>(() => HealthSummary.Of(null!));

        Assert.Equal("states", fault.ParamName);
    }

    // ----- Of over a source that is counted as it is walked

    /// <summary>The source is walked over before <c>Of</c> answers: the summary holds numbers and not a walk still to
    /// make, so whatever the source hands out as it is walked is counted as the walk reached it, and the walk is over
    /// by the time the counts are read - which is also the last time anything is walked.</summary>
    [Fact]
    public void OfWalksItsSourceOverBeforeItAnswers()
    {
        int read = 0;
        IEnumerable<HealthState> Answers()
        {
            foreach (var state in States(Healthy, Unhealthy, Pending, Unreachable))
            {
                read++;
                yield return state;
            }
        }

        var summary = HealthSummary.Of(Answers());

        Assert.Equal(4, read);
        Assert.Equal(4, summary.Total);
        Assert.Equal(1, summary.Healthy);
        Assert.Equal(1, summary.Pending);

        Assert.Equal(4, read);
    }

    /// <summary>The same source asked twice is walked twice and counted the same both times: <c>Of</c> keeps nothing
    /// of one call for the next, and two calls over one heap of answers make two equal summaries rather than one
    /// summary handed back twice.</summary>
    [Fact]
    public void OfCountsTheSameSourceTheSameTwiceOver()
    {
        var source = new WalkCounting(States(Healthy, Pending, Unreachable, Healthy));

        var first = HealthSummary.Of(source);
        var second = HealthSummary.Of(source);

        Assert.Equal(2, source.Walks);
        Assert.Equal(8, source.ItemsRead);
        Assert.Equal(first, second);
        Assert.NotSame(first, second);
    }

    /// <summary>A summary holds its counts and not the source they were counted from: the heap a summary was made out
    /// of may be emptied and filled with other answers afterwards, and the summary still says what the one call found
    /// - while the same source, asked again, is counted as it stands at that second call.</summary>
    [Fact]
    public void ASummaryHoldsItsCountsAndNotTheSourceTheyWereCountedFrom()
    {
        var source = new WalkCounting(States(Healthy, Pending));
        var counted = HealthSummary.Of(source);

        source.Items.Clear();
        source.Items.Add(Unhealthy);

        Assert.Equal(2, counted.Total);
        Assert.Equal(1, counted.Healthy);
        Assert.Equal(1, counted.Pending);

        var again = HealthSummary.Of(source);

        Assert.Equal(1, again.Total);
        Assert.Equal(0, again.Healthy);
        Assert.Equal(0, again.Pending);
        Assert.NotEqual(counted, again);
    }

    /// <summary>A source that dies on the walk is not swallowed and not answered with counts of what was reached
    /// before the death: the exception the source threw is the exception <c>Of</c> throws, that very one. A source
    /// that refuses to be walked at all dies the same way, and a heap that changes under the walk is the source's own
    /// broken promise rather than a summary counted out of half of it.</summary>
    [Fact]
    public void ASourceThatDiesOnTheWalkDiesOutOfOf()
    {
        var raised = new InvalidOperationException("the third answer never came");
        IEnumerable<HealthState> Broken()
        {
            foreach (var state in States(Healthy, 2))
            {
                yield return state;
            }

            throw raised;
        }

        var fault = Assert.Throws<InvalidOperationException>(() => HealthSummary.Of(Broken()));
        Assert.Same(raised, fault);

        Assert.Throws<InvalidOperationException>(() => HealthSummary.Of(new RefusesTheWalk()));

        List<HealthState> answers = [Healthy, Unhealthy];
        IEnumerable<HealthState> GrowsWhileWalked()
        {
            foreach (var state in answers)
            {
                answers.Add(Healthy);
                yield return state;
            }
        }

        Assert.Throws<InvalidOperationException>(() => HealthSummary.Of(GrowsWhileWalked()));
    }

    // ----- the record: the three counts it is, and with

    /// <summary>The same three counts made twice are one summary: equal both ways round, one to the <c>==</c>, not
    /// two to the <c>!=</c>, of one hash code, and of the same hash code from call to call.</summary>
    [Fact]
    public void TheSameThreeCountsMadeTwiceAreOneSummary()
    {
        var first = Summary(7, 5, 2);
        var second = Summary(7, 5, 2);

        Assert.True(first.Equals(second));
        Assert.True(second.Equals(first));
        Assert.True(first == second);
        Assert.False(first != second);
        Assert.Equal(first.GetHashCode(), second.GetHashCode());
        Assert.Equal(first.GetHashCode(), first.GetHashCode());
    }

    /// <summary>The counts a round counted out of its answers are the counts the constructor is given: one summary
    /// either way, before the answers are looked at and after them.</summary>
    [Fact]
    public void TheCountsOfASourceAreTheCountsOfTheRecord()
    {
        Assert.Equal(Summary(4, 1, 2), HealthSummary.Of(States(Healthy, Pending, Pending, Unreachable)));
    }

    /// <summary>One count moved is another summary: the total, the healthy count and the pending count each count on
    /// their own, and counts of nothing - or of less than nothing - are not counts of something.</summary>
    [Theory]
    [InlineData(6, 5, 2)]
    [InlineData(7, 4, 2)]
    [InlineData(7, 5, 1)]
    [InlineData(0, 0, 0)]
    [InlineData(-7, -5, -2)]
    public void OneCountMovedMakesAnotherSummary(int total, int healthy, int pending)
    {
        var summary = Summary(7, 5, 2);
        var moved = Summary(total, healthy, pending);

        Assert.False(summary.Equals(moved));
        Assert.False(moved.Equals(summary));
        Assert.True(summary != moved);
    }

    /// <summary>A summary is one thing: itself, and nothing else - not a null, not the words it says itself in, not a
    /// shape of the same three numbers under another name.</summary>
    [Fact]
    public void ASummaryIsOneSummaryAndNothingElse()
    {
        var summary = Summary(7, 5, 2);

        Assert.True(summary.Equals(summary));
        Assert.False(summary.Equals(null));
        Assert.False(summary.Equals("7 of 7 nodes not healthy"));
        Assert.False(summary.Equals(new { Total = 7, Healthy = 5, Pending = 2 }));
    }

    /// <summary>Equality is of the three counts and not of what they are made to say: two summaries of different
    /// counts word the very same sentence and are not one summary, because the record keeps the counts it was given
    /// and not the reading taken out of them.</summary>
    [Fact]
    public void TwoSummariesThatSayTheSameWordsAreNotOneSummary()
    {
        var impossible = Summary(2, 3, 0);
        var alsoImpossible = Summary(2, 4, -1);

        Assert.Equal(impossible.Text, alsoImpossible.Text);
        Assert.False(impossible.Equals(alsoImpossible));
        Assert.True(impossible != alsoImpossible);

        var problem = Summary(3, 0, 0);
        var anotherProblem = Summary(3, 1, -1);

        Assert.Equal(problem.Text, anotherProblem.Text);
        Assert.NotEqual(problem, anotherProblem);
    }

    /// <summary>A copy with nothing moved is the summary it was copied from in every way but one: equal to it, one to
    /// the <c>==</c>, of one hash code, and not that same summary in memory.</summary>
    [Fact]
    public void ACopyWithNothingMovedIsThatSummaryAndNotIt()
    {
        var original = Summary(4, 3, 1);
        var copy = original with { };

        Assert.Equal(original, copy);
        Assert.True(original == copy);
        Assert.False(original != copy);
        Assert.NotSame(original, copy);
        Assert.Equal(original.GetHashCode(), copy.GetHashCode());
    }

    /// <summary><c>with</c> moves the one count it names and carries the other two along, and the copy's
    /// <c>NotHealthy</c> is subtracted from the copy's counts rather than from the counts it came from - while the
    /// summary it was copied from stands exactly where it stood.</summary>
    [Theory]
    [InlineData(1)]
    [InlineData(0)]
    [InlineData(-3)]
    public void WithMovesTheCountItNamesAndCarriesTheOtherTwoAlong(int pending)
    {
        var original = Summary(9, 6, 3);
        var moved = original with { Pending = pending };

        Assert.Equal(pending, moved.Pending);
        Assert.Equal(9, moved.Total);
        Assert.Equal(6, moved.Healthy);
        Assert.Equal(3 - pending, moved.NotHealthy);

        Assert.NotEqual(original, moved);
        Assert.Equal(3, original.Pending);
        Assert.Equal(0, original.NotHealthy);
    }

    /// <summary>A copy that moves the total moves what is subtracted from it as well: the count the copy was not
    /// given carries along, and the summary the copy was made from keeps its own total, its own healthy count and
    /// its own difference.</summary>
    [Fact]
    public void ACopysDifferenceIsSubtractedFromItsOwnCounts()
    {
        var original = Summary(9, 6, 3);
        var moved = original with { Total = 8, Healthy = 2 };

        Assert.Equal(8, moved.Total);
        Assert.Equal(2, moved.Healthy);
        Assert.Equal(3, moved.Pending);
        Assert.Equal(3, moved.NotHealthy);

        Assert.Equal(9, original.Total);
        Assert.Equal(6, original.Healthy);
        Assert.Equal(0, original.NotHealthy);
    }

    /// <summary>A heap of answers that says, from its own side, how many times it was walked and how many answers it
    /// handed out over those walks, so "the same source twice" and "the source found changed between two calls" are
    /// both seen from the source rather than from the summary.</summary>
    private sealed class WalkCounting(HealthState[] answers) : IEnumerable<HealthState>
    {
        public List<HealthState> Items { get; } = [.. answers];

        public int Walks { get; private set; }

        public int ItemsRead { get; private set; }

        public IEnumerator<HealthState> GetEnumerator()
        {
            Walks++;
            return Walk();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private IEnumerator<HealthState> Walk()
        {
            foreach (var state in Items)
            {
                ItemsRead++;
                yield return state;
            }
        }
    }

    /// <summary>A heap of answers that will not be walked at all.</summary>
    private sealed class RefusesTheWalk : IEnumerable<HealthState>
    {
        public IEnumerator<HealthState> GetEnumerator() => throw new InvalidOperationException("no walk at all");

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
