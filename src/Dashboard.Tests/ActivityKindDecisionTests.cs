using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The decisions <see cref="ActivityKind" /> makes about a word of a kind (#926): whether it knows the word, whether
/// the word is a deployment in flight, and which word the last deployment that ended is said in - the collapse of a
/// result the page does not know into <see cref="ActivityKind.Canceled" /> being a decision in one place rather than
/// a switch default. The members are internal to an app that declares no <c>InternalsVisibleTo</c>, so they are
/// reached by name; the public surface stays the eight words and their order, as
/// <see cref="ActivityKindEdgeCaseTests" /> pins.
/// </summary>
public class ActivityKindDecisionTests
{
    private static T Call<T>(string name, object? argument) =>
        (T)typeof(ActivityKind).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static)!.Invoke(null, [argument])!;

    private static bool Knows(string? kind) => Call<bool>("Knows", kind);

    private static bool IsInFlight(string? kind) => Call<bool>("IsInFlight", kind);

    private static string OfResult(DeploymentState result) => Call<string>("OfResult", result);

    [Fact]
    public void EveryOneOfTheEightWordsIsKnown()
    {
        Assert.All(ActivityKind.All, kind => Assert.True(Knows(kind)));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("Waiting")]
    [InlineData("waiting ")]
    [InlineData("succeeded")]
    [InlineData("timed out")]
    public void AWordThatIsNotOneOfTheEightLetterForLetterIsNotKnown(string? kind)
    {
        Assert.False(Knows(kind));
    }

    [Fact]
    public void TheKindsInFlightAreWaitingDeployingAndQueuedAndNothingElse()
    {
        Assert.Equal(
            [ActivityKind.Waiting, ActivityKind.Deploying, ActivityKind.Queued],
            ActivityKind.All.Where(IsInFlight));
        Assert.False(IsInFlight(null));
        Assert.False(IsInFlight("Deploying"));
        Assert.False(IsInFlight("unknown"));
    }

    [Theory]
    [InlineData(DeploymentState.Succeeded, ActivityKind.Deployed)]
    [InlineData(DeploymentState.Failed, ActivityKind.Failed)]
    [InlineData(DeploymentState.Canceled, ActivityKind.Canceled)]
    public void AResultThePageKnowsIsSaidInItsOwnKind(DeploymentState result, string kind)
    {
        Assert.Equal(kind, OfResult(result));
    }

    [Theory]
    [InlineData(DeploymentState.Unknown)]
    [InlineData(DeploymentState.Queued)]
    [InlineData(DeploymentState.Executing)]
    [InlineData(DeploymentState.Waiting)]
    [InlineData((DeploymentState)99)]
    public void AnyOtherResultCollapsesIntoCanceledAKindThePageKnows(DeploymentState result)
    {
        var kind = OfResult(result);

        Assert.Equal(ActivityKind.Canceled, kind);
        Assert.True(Knows(kind));
        Assert.False(IsInFlight(kind));
    }

    [Fact]
    public void EveryResultIsSaidInAKnownKind()
    {
        Assert.All(Enum.GetValues<DeploymentState>(), result => Assert.True(Knows(OfResult(result))));
    }
}
