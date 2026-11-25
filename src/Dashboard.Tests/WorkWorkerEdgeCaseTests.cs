namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="WorkWorker" />, the three words for whose work an item
/// is, declared in <c>src/Dashboard/Work/WorkItemsReport.cs</c>. What the file's <c>worker</c> word reads as is
/// pinned in <see cref="WorkItemEdgeCaseTests" /> and <see cref="WorkItemsTests" />, the unset worker being
/// <see cref="WorkWorker.Unknown" /> on the number 0 among them; the word each worker is said with on a row is pinned
/// in <see cref="WorkItemsTextTests" />. What that leaves open is pinned here: the three workers as the numbers, the
/// names and the order they are declared in, nothing outside the three, and text that is or is not a worker's name
/// as the enum spells it.
/// </summary>
public class WorkWorkerEdgeCaseTests
{
    /// <summary>The three workers in their declared order, on the numbers 0 to 2, of an <see cref="int" />
    /// underneath, and a choice and not a set.</summary>
    [Fact]
    public void TheThreeWorkersAreTheWholeVocabularyInTheirDeclaredOrder()
    {
        Assert.Equal(["Unknown", "Bot", "Team"], Enum.GetNames<WorkWorker>());
        Assert.Equal([0, 1, 2], Enum.GetValues<WorkWorker>().Select(worker => (int)worker));
        Assert.Equal(typeof(int), Enum.GetUnderlyingType(typeof(WorkWorker)));
        Assert.False(typeof(WorkWorker).IsDefined(typeof(FlagsAttribute), false));
    }

    /// <summary>Each worker on its own number and its own name, both ways round.</summary>
    [Theory]
    [InlineData(WorkWorker.Unknown, 0, "Unknown")]
    [InlineData(WorkWorker.Bot, 1, "Bot")]
    [InlineData(WorkWorker.Team, 2, "Team")]
    public void EachWorkerStandsOnItsOwnNumberAndItsOwnName(WorkWorker worker, int number, string name)
    {
        Assert.Equal(number, (int)worker);
        Assert.Equal(worker, (WorkWorker)number);
        Assert.Equal(name, worker.ToString());
        Assert.Equal(name, Enum.GetName(worker));
        Assert.True(Enum.IsDefined(worker));
    }

    /// <summary>Outside the three there is nothing: neither one below the first, one above the last, nor either end
    /// of <see cref="int" /> is a worker, and such a value says the number it stands on.</summary>
    [Theory]
    [InlineData(-1)]
    [InlineData(3)]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    public void NothingOutsideTheThreeIsAWorker(int number)
    {
        var worker = (WorkWorker)number;

        Assert.False(Enum.IsDefined(worker));
        Assert.Null(Enum.GetName(worker));
        Assert.Equal(number.ToString(System.Globalization.CultureInfo.InvariantCulture), worker.ToString());
        Assert.DoesNotContain(worker, Enum.GetValues<WorkWorker>());
    }

    /// <summary>The workers sort by their numbers, the unknown first; each is apart from the others and from a boxed
    /// <see cref="int" /> or a box of nothing.</summary>
    [Fact]
    public void TheWorkersAreApartAndSortByTheirNumber()
    {
        Assert.True(WorkWorker.Unknown < WorkWorker.Bot);
        Assert.True(WorkWorker.Bot < WorkWorker.Team);
        Assert.Equal(-1, WorkWorker.Unknown.CompareTo(WorkWorker.Team));
        Assert.Equal(0, WorkWorker.Bot.CompareTo((object)WorkWorker.Bot));
        Assert.Equal(3, Enum.GetValues<WorkWorker>().Distinct().Count());

        Assert.False(WorkWorker.Bot.Equals(null));
        Assert.NotEqual((object)WorkWorker.Bot, (object)1);
        Assert.Equal(WorkWorker.Team.GetHashCode(), ((object)WorkWorker.Team).GetHashCode());
    }

    /// <summary>The enum's own names parse in their own case; the file's lower case only when case is ignored. Text
    /// that names no worker - nothing, blank, or a word the enum does not have - is no worker.</summary>
    [Theory]
    [InlineData("Unknown", WorkWorker.Unknown)]
    [InlineData("Bot", WorkWorker.Bot)]
    [InlineData("Team", WorkWorker.Team)]
    public void AWorkersNameParsesBackToTheWorker(string name, WorkWorker worker)
    {
        Assert.Equal(worker, Enum.Parse<WorkWorker>(name));
        Assert.True(Enum.TryParse<WorkWorker>(name, out var parsed));
        Assert.Equal(worker, parsed);

        Assert.False(Enum.TryParse<WorkWorker>(name.ToLowerInvariant(), out _));
        Assert.True(Enum.TryParse<WorkWorker>(name.ToLowerInvariant(), ignoreCase: true, out var folded));
        Assert.Equal(worker, folded);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("robot")]
    [InlineData("Bots")]
    [InlineData("Bot,Team")]
    public void TextThatNamesNoWorkerIsNoWorker(string text)
    {
        Assert.False(Enum.TryParse<WorkWorker>(text, out var parsed) && Enum.IsDefined(parsed));
    }

    [Fact]
    public void NoTextIsNoWorker()
    {
        Assert.False(Enum.TryParse<WorkWorker>(null, out var parsed));
        Assert.Equal(default, parsed);
        Assert.Throws<ArgumentNullException>(() => Enum.Parse<WorkWorker>(null!));
    }

    /// <summary>The two named workers share their names with two of the three audiences; the unknown has no audience
    /// of its own name.</summary>
    [Fact]
    public void TheNamedWorkersShareTheirNamesWithAnAudience()
    {
        Assert.Equal(WorkAudience.Bot, Enum.Parse<WorkAudience>(WorkWorker.Bot.ToString()));
        Assert.Equal(WorkAudience.Team, Enum.Parse<WorkAudience>(WorkWorker.Team.ToString()));
        Assert.False(Enum.TryParse<WorkAudience>(WorkWorker.Unknown.ToString(), out _));
    }
}
