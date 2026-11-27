using static Dashboard.Tests.ClusterFixture;

namespace Dashboard.Tests;

/// <summary>
/// The labels <see cref="ClusterAssessment.Service" /> reads from the <see cref="AksAvailability" /> consts: each
/// verdict the page matches on is labelled with the very const it matched, so the page's words for the four
/// verdicts stand in one place, in the consts, and a rename of a const cannot leave the label behind. The words
/// the file writes in any case are judged the same, and so is the label they get: the canonical capitalised word,
/// not the file's own casing.
/// <para>
/// The literal-string pins the other test files hold stay independent of these: they pin the visible words, these
/// pin that the visible words are the consts themselves.
/// </para>
/// </summary>
public class ClusterAssessmentVerdictLabelTests
{
    /// <summary>
    /// Each of the four verdicts Azure can write is labelled with the <see cref="AksAvailability" /> const of the
    /// arm that matched it: Unavailable and Degraded stand on their own, Unknown stands for a verdict that says
    /// Unknown, and Available stands for the healthy one.
    /// </summary>
    [Theory]
    [InlineData(AksAvailability.Unavailable, ClusterState.Unhealthy)]
    [InlineData(AksAvailability.Degraded, ClusterState.Warning)]
    [InlineData(AksAvailability.Unknown, ClusterState.Neutral)]
    [InlineData(AksAvailability.Available, ClusterState.Healthy)]
    public void EachVerdictIsLabelledWithTheConstItsOwnConditionMatches(string verdict, ClusterState expectedState)
    {
        var service = ClusterFixture.SampleService with { Availability = new AksAvailability(verdict, null, null, null) };

        var assessment = Service(service);

        Assert.Equal(expectedState, assessment.State);
        Assert.Equal(verdict, assessment.Label);
    }

    /// <summary>
    /// A verdict the file writes in another case is still matched in any case, and the label it gets is the
    /// canonical capitalised const, not the file's own casing: <c>degraded</c> is Degraded, <c>UNAVAILABLE</c> is
    /// Unavailable, and <c>available</c> is Available.
    /// </summary>
    [Theory]
    [InlineData("degraded", AksAvailability.Degraded, ClusterState.Warning)]
    [InlineData("UNAVAILABLE", AksAvailability.Unavailable, ClusterState.Unhealthy)]
    [InlineData("available", AksAvailability.Available, ClusterState.Healthy)]
    [InlineData("uNkNoWn", AksAvailability.Unknown, ClusterState.Neutral)]
    public void AVerdictWrittenInAnotherCaseIsLabelledWithTheCanonicalConst(string fileWord, string canonical, ClusterState expectedState)
    {
        var service = ClusterFixture.SampleService with { Availability = new AksAvailability(fileWord, null, null, null) };

        var assessment = Service(service);

        Assert.Equal(expectedState, assessment.State);
        Assert.Equal(canonical, assessment.Label);
    }

    /// <summary>
    /// The availability that is not there is labelled with the const of the arm it falls into: a missing
    /// <c>Availability</c> stands for <see cref="AksAvailability.Unknown" />, exactly as the literal pins of
    /// <see cref="ClusterAssessmentTests" /> say of the visible word.
    /// </summary>
    [Fact]
    public void AMissingAvailabilityIsLabelledWithTheUnknownConst()
    {
        var assessment = Service(ClusterFixture.SampleService with { Availability = null });

        Assert.Equal(ClusterState.Neutral, assessment.State);
        Assert.Equal(AksAvailability.Unknown, assessment.Label);
    }

    /// <summary>
    /// The verdict that does not say is labelled the same: an availability whose state the file does not write is
    /// missing a word, and it stands for <see cref="AksAvailability.Unknown" /> too, not for the empty word.
    /// </summary>
    [Fact]
    public void AVerdictThatDoesNotSayItsStateIsLabelledWithTheUnknownConst()
    {
        var assessment = Service(ClusterFixture.SampleService with { Availability = new AksAvailability(null!, null, null, null) });

        Assert.Equal(ClusterState.Neutral, assessment.State);
        Assert.Equal(AksAvailability.Unknown, assessment.Label);
    }
}
