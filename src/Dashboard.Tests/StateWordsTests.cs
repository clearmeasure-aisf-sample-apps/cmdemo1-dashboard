namespace Dashboard.Tests;

/// <summary>
/// The two types that carry the cluster view's state words: <see cref="PodPhase" />, which tells Kubernetes' five
/// phases apart in the exact case Kubernetes writes them, and <see cref="AzureState" />, which compares Azure's words
/// in any case and tells a word that is not there from the empty one. Both keep the word as the file held it, so the
/// label the page shows is the file's own.
/// </summary>
public class StateWordsTests
{
    [Theory]
    [InlineData("Pending", PodPhaseKind.Pending)]
    [InlineData("Running", PodPhaseKind.Running)]
    [InlineData("Succeeded", PodPhaseKind.Succeeded)]
    [InlineData("Failed", PodPhaseKind.Failed)]
    [InlineData("Unknown", PodPhaseKind.Unknown)]
    [InlineData("running", PodPhaseKind.Other)]
    [InlineData("SUCCEEDED", PodPhaseKind.Other)]
    [InlineData("", PodPhaseKind.Other)]
    [InlineData("Terminating", PodPhaseKind.Other)]
    public void APodPhaseKnowsTheFiveWordsOfKubernetesInTheirOwnCaseOnly(string word, PodPhaseKind kind)
    {
        var phase = new PodPhase(word);

        Assert.Equal(kind, phase.Kind);
        Assert.True(phase.Is(kind));
        Assert.Equal(word, phase.Text);
        Assert.Equal(word, phase.ToString());
        Assert.Equal(kind is PodPhaseKind.Other or PodPhaseKind.Unknown, phase.IsUnknown);
    }

    [Theory]
    [InlineData("Stopped", "Stopped", true)]
    [InlineData("StOpPeD", "Stopped", true)]
    [InlineData("SUCCEEDED", "Succeeded", true)]
    [InlineData("NotStopped", "Stopped", false)]
    [InlineData("", "Stopped", false)]
    [InlineData(null, "Stopped", false)]
    public void AnAzureStateIsAWordInAnyCase(string? word, string expected, bool matches)
    {
        var state = new AzureState(word);

        Assert.Equal(matches, state.Is(expected));
        Assert.Equal(word is null, state.IsMissing);
        Assert.Equal(word, state.Text);
        Assert.Equal(word ?? string.Empty, state.ToString());
    }

    [Fact]
    public void TwoAzureStatesAreTheSameOnlyWhenBothSayAWord()
    {
        Assert.True(new AzureState("Running").IsSameAs(new AzureState("RUNNING")));
        Assert.True(new AzureState(string.Empty).IsSameAs(new AzureState(string.Empty)));
        Assert.False(new AzureState("Running").IsSameAs(new AzureState("Stopped")));
        Assert.False(new AzureState(null).IsSameAs(new AzureState(null)));
        Assert.False(new AzureState("Running").IsSameAs(default));
        Assert.True(default(AzureState).IsMissing);
    }

    [Fact]
    public void TheServiceReadsItsPowerAndProvisioningWordsInAnyCase()
    {
        var service = ClusterFixture.SampleService with { PowerState = "STOPPED", ProvisioningState = "succeeded" };

        Assert.True(service.IsStopped);
        Assert.False(service.IsRunning);
        Assert.True(service.IsProvisioned);
        Assert.False((service with { ProvisioningState = string.Empty }).IsProvisioned);
        Assert.False((service with { ProvisioningState = null }).IsProvisioned);
    }
}
