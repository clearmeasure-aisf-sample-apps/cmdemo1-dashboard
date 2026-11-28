namespace Dashboard.Tests;

/// <summary>
/// The third reader of the word <c>Not known</c> (#2185): <see cref="ClusterAssessment.Service" /> says it when Azure's
/// facts are not there (a reading that could not be read) or when a reading marked read holds no facts. It reads the
/// word from <see cref="HealthDetailText.NotKnown" /> (#2055's single owner) through one private const, so the label
/// of the AKS service card and <see cref="HealthDetailText.Label" /> of <see cref="CheckState.Unknown" /> cannot drift
/// apart. Pinned through public behaviour, because the app declares no <c>InternalsVisibleTo</c>.
/// </summary>
public class ClusterAssessmentNotKnownOwnershipTests
{
    /// <summary>
    /// Both arms of the <see cref="ClusterAssessment.Service" /> fallback say the owner's word: a reading that could
    /// not be read, and a reading marked read that holds no facts, each return the label
    /// <see cref="HealthDetailText.Label" /> of <see cref="CheckState.Unknown" />.
    /// </summary>
    [Fact]
    public void TheServiceCardLabelOfAReadingThatIsNotKnownSaysTheOwnersWord()
    {
        var ownerWord = HealthDetailText.Label(CheckState.Unknown);

        // The reading that could not be read: the address does not answer.
        var unavailable = ClusterAssessment.Service(new SourceReading<AksService>(SourceState.Unavailable));
        Assert.Equal(ownerWord, unavailable.Label);

        // The reading marked read that holds no facts: no value to assess.
        var readWithNothing = ClusterAssessment.Service(new SourceReading<AksService>(SourceState.Read));
        Assert.Equal(ownerWord, readWithNothing.Label);
    }
}
