namespace Dashboard.Tests;

/// <summary>
/// The words of a waiting deployment have one home, <see cref="DeploymentWait"/> (#641): the kind as a noun, the
/// question with its title, the sentence of the mark and the line of the node. The callers'
/// (<see cref="DeploymentText"/>, <see cref="DeploymentActivity"/>) words stay pinned where they were.
/// </summary>
public class DeploymentWaitWordsTests
{
    [Theory]
    [InlineData(DeploymentWait.GuidedFailure, "the question after a failed step")]
    [InlineData("Guided Failure", "the question after a failed step")]
    [InlineData(DeploymentWait.SignOff, "the sign-off")]
    [InlineData("SIGN-OFF", "the sign-off")]
    [InlineData("manual intervention", "manual intervention")]
    public void TheKindIsANounOnlyWhereThePageKnowsIt(string kind, string noun) =>
        Assert.Equal(noun, new DeploymentWait(kind, null, null, null).Noun());

    [Fact]
    public void TheQuestionQuotesATitleOnlyWhereItSaysMoreThanTheKind()
    {
        Assert.Equal("the sign-off \"Approve prod\"", new DeploymentWait(DeploymentWait.SignOff, "Approve prod", null, null).Question());
        Assert.Equal("the sign-off", new DeploymentWait(DeploymentWait.SignOff, "Sign off", null, null).Question());
        Assert.Equal("the question after a failed step", new DeploymentWait(DeploymentWait.GuidedFailure, null, null, null).Question());
        Assert.Equal("paused \"Hold on\"", new DeploymentWait("paused", "Hold on", null, null).Question());
    }

    [Fact]
    public void OnlyAGuidedFailureAsksWhatToDoAfterAFailedStep()
    {
        Assert.Equal("a 1 asks what to do after a failed step in uat", DeploymentWait.Asks(new DeploymentWait(DeploymentWait.GuidedFailure, null, null, null), "a 1", "uat"));
        Assert.Equal("a 1 waits for a sign-off in uat", DeploymentWait.Asks(new DeploymentWait(DeploymentWait.SignOff, null, null, null), "a 1", "uat"));
        Assert.Equal("a 1 waits for a sign-off in uat", DeploymentWait.Asks(new DeploymentWait("manual intervention", null, null, null), "a 1", "uat"));
        Assert.Equal("a 1 waits for a sign-off in uat", DeploymentWait.Asks(null, "a 1", "uat"));
    }

    [Theory]
    [InlineData(DeploymentWait.GuidedFailure, "Guided-Failure", true)]
    [InlineData(DeploymentWait.GuidedFailure, "guided-failure", true)]
    [InlineData(DeploymentWait.SignOff, "Sign off", true)]
    [InlineData(DeploymentWait.SignOff, "SIGN-OFF", true)]
    [InlineData(DeploymentWait.SignOff, "Approve prod", false)]
    public void TheTitleIsTheKindWhenItSaysNothingMoreThanTheKind(string kind, string title, bool isTheKind) =>
        Assert.Equal(isTheKind, new DeploymentWait(kind, title, null, null).TitleIsTheKind);

    [Fact]
    public void TheTitleIsTheKindWithNoTitle() =>
        Assert.True(new DeploymentWait(DeploymentWait.SignOff, null, null, null).TitleIsTheKind);

    [Fact]
    public void TheLineSaysTheKindAndWhoDecides()
    {
        Assert.Equal("2.4.43 failed a step: ops decides", DeploymentWait.Line(new DeploymentWait(DeploymentWait.GuidedFailure, null, null, "ops"), "2.4.43", ", 3 min"));
        Assert.Equal("2.4.43 failed a step and waits", DeploymentWait.Line(new DeploymentWait(DeploymentWait.GuidedFailure, null, null, null), "2.4.43", ", 3 min"));
        Assert.Equal("2.4.43 waits for ops", DeploymentWait.Line(new DeploymentWait(DeploymentWait.SignOff, null, null, "ops"), "2.4.43", ", 3 min"));
        Assert.Equal("2.4.43 waits for a sign-off, 3 min", DeploymentWait.Line(new DeploymentWait(DeploymentWait.SignOff, null, null, null), "2.4.43", ", 3 min"));
        Assert.Equal("2.4.43 waits for a sign-off", DeploymentWait.Line(null, "2.4.43", string.Empty));
    }
}
