namespace Dashboard.Tests;

/// <summary>
/// The words the page makes of more than one deployment mark at once - <see cref="DeploymentText.Titles"/>, the
/// tooltip of a list of marks, and <see cref="DeploymentText.Sentences"/>, what a screen reader hears of them. The
/// one mark's words are pinned where they belong (<c>DeploymentTextEdgeCaseTests</c>, <c>DeploymentsTests</c>):
/// here they are pinned together, the separator between them, the nothing the list says when it holds nothing,
/// and the refusal of a list that is not there.
/// </summary>
public class DeploymentTextMarkListTests
{
    /// <summary>A mark the test builds from its own words, its count and what the file says more.</summary>
    private static DeploymentMark Mark(string sentence, int? minutes, bool inFlight, string? detail = null) =>
        new("demo-ui", "ui", "uat", "2.4.43", DeploymentState.Executing, sentence, minutes, null, inFlight, OfSystem: false, detail);

    /// <summary>The tooltip of no marks is no tooltip: <c>null</c>, the rule a tooltip with no detail carries.</summary>
    [Fact]
    public void TheTitlesOfNoMarksAreNoTitle() =>
        Assert.Null(DeploymentText.Titles([]));

    /// <summary>
    /// The tooltip of one mark is that mark's own tooltip, word for word and nothing added: no separator, no blank
    /// line of its own - the join that says the words of one is the words.
    /// </summary>
    [Fact]
    public void TheTitlesOfOneMarkAreThatMarksTitle()
    {
        var mark = Mark("deploying demo-ui 2.4.43 to uat", 3, inFlight: true);

        Assert.Equal(DeploymentText.Title(mark), DeploymentText.Titles([mark]));
    }

    /// <summary>
    /// The tooltip of two marks is their titles, one per line, in the order the list gives them: the in-flight mark
    /// with its count says its <c>so far</c> in its line, and the mark that carries what the file says more says it
    /// on the line of its own after its sentence - the two joined with the line break and nothing else between them.
    /// </summary>
    [Fact]
    public void TheTitlesOfTwoMarksAreTheirTitlesOnePerLine()
    {
        var inFlight = Mark("deploying demo-ui 2.4.43 to uat", 3, inFlight: true);
        var withDetail = Mark("demo-ui 2.4.44 waits for a sign-off in uat", null, inFlight: true, detail: "the sign-off is with demo approvers");

        Assert.Equal(
            "deploying demo-ui 2.4.43 to uat (3 min so far)\ndemo-ui 2.4.44 waits for a sign-off in uat\nthe sign-off is with demo approvers",
            DeploymentText.Titles([inFlight, withDetail]));

        Assert.Equal(
            $"{DeploymentText.Title(inFlight)}\n{DeploymentText.Title(withDetail)}",
            DeploymentText.Titles([inFlight, withDetail]));

        Assert.Equal(
            $"{DeploymentText.Title(withDetail)}\n{DeploymentText.Title(inFlight)}",
            DeploymentText.Titles([withDetail, inFlight]));
    }

    /// <summary>
    /// What a screen reader hears of two marks is their sentences, joined with <c>"; "</c> in the order the list
    /// gives them: the counts and what the file says more are the tooltip's words, not the voice's - the sentence
    /// alone is what the voice says, however much the mark carries beyond it.
    /// </summary>
    [Fact]
    public void TheSentencesOfTwoMarksAreTheirSentencesJoined()
    {
        var inFlight = Mark("deploying demo-ui 2.4.43 to uat", 3, inFlight: true);
        var withDetail = Mark("demo-ui 2.4.44 waits for a sign-off in uat", null, inFlight: true, detail: "the sign-off is with demo approvers");

        Assert.Equal("deploying demo-ui 2.4.43 to uat; demo-ui 2.4.44 waits for a sign-off in uat", DeploymentText.Sentences([inFlight, withDetail]));
        Assert.Equal("a; b", DeploymentText.Sentences([Mark("a", null, inFlight: false), Mark("b", null, inFlight: false)]));
    }

    /// <summary>The sentences of no marks is the empty string: no separator to say, and the voice says nothing.</summary>
    [Fact]
    public void TheSentencesOfNoMarksAreEmpty() =>
        Assert.Equal(string.Empty, DeploymentText.Sentences([]));

    /// <summary>A list that is not there is refused by the name the guard keeps, for the tooltip and the voice alike.</summary>
    [Fact]
    public void WithoutMarksThereAreNoTitlesAndNoSentences()
    {
        Assert.Equal("marks", Assert.Throws<ArgumentNullException>(() => DeploymentText.Titles(null!)).ParamName);
        Assert.Equal("marks", Assert.Throws<ArgumentNullException>(() => DeploymentText.Sentences(null!)).ParamName);
    }
}
