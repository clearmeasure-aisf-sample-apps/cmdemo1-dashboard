using Dashboard.Health;
using Xunit;

namespace Dashboard.Tests;

/// <summary>
/// The dot-shape words of <see cref="DeploymentText"/>: the four letters the dot's class and the runtime payload
/// carry, named once. The letters are a contract the CSS and <c>runtime.js</c> read, so they are pinned here, letter
/// for letter: a rename that would break either fails this test.
/// </summary>
public class DeploymentShapeWordTests
{
    [Fact]
    public void TheShapeWordsCarryTheLettersTheCssAndJsRead()
    {
        Assert.Equal("executing", DeploymentText.ExecutingShape);
        Assert.Equal("queued", DeploymentText.QueuedShape);
        Assert.Equal("waiting", DeploymentText.WaitingShape);
        Assert.Equal("ended", DeploymentText.EndedShape);
    }

    [Fact]
    public void ShapeSaysTheInFlightStatesInTheirOwnWord()
    {
        Assert.Equal(DeploymentText.ExecutingShape, DeploymentText.Shape(DeploymentState.Executing));
        Assert.Equal(DeploymentText.QueuedShape, DeploymentText.Shape(DeploymentState.Queued));
        Assert.Equal(DeploymentText.WaitingShape, DeploymentText.Shape(DeploymentState.Waiting));
    }

    [Theory]
    [InlineData(DeploymentState.Succeeded)]
    [InlineData(DeploymentState.Failed)]
    [InlineData(DeploymentState.Canceled)]
    [InlineData(DeploymentState.Unknown)]
    public void ShapeSaysEndedForAStateThatIsNotInFlight(DeploymentState state)
    {
        Assert.Equal(DeploymentText.EndedShape, DeploymentText.Shape(state));
    }

    [Fact]
    public void ShapeSaysEndedForAStateThePageDoesNotKnow()
    {
        Assert.Equal(DeploymentText.EndedShape, DeploymentText.Shape((DeploymentState)999));
    }
}
