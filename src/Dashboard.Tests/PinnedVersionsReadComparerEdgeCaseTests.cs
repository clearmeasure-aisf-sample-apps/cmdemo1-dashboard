namespace Dashboard.Tests;

/// <summary>
/// Both pin readers build their version map with one rule for matching a deployable's name: exactly, case and all.
/// These pin that the versions.json reading and the Kustomize reading still match names the same way.
/// </summary>
public class PinnedVersionsReadComparerEdgeCaseTests
{
    private const string Kustomization = """
        images:
          - name: ui
            newTag: "2.4.7"
        """;

    [Fact]
    public void KeysThatDifferOnlyInCaseAreTwoEntriesAndAThirdCasingIsNone()
    {
        var pinned = PinnedVersions.Parse("""{ "ui": "1.0.0", "UI": "2.0.0" }""");

        Assert.Equal(PinnedVersionsState.Read, pinned.State);
        Assert.Equal(2, pinned.Versions.Count);
        Assert.Equal("1.0.0", pinned.Of("ui"));
        Assert.Equal("2.0.0", pinned.Of("UI"));
        Assert.Null(pinned.Of("Ui"));
    }

    [Fact]
    public void AKustomizationPinAnswersItsDeployableOnlyInTheSameCase()
    {
        var pinned = PinnedVersions.ParseKustomization(Kustomization, "ui");

        Assert.Equal(PinnedVersionsState.Read, pinned.State);
        Assert.Equal("2.4.7", pinned.Of("ui"));
        Assert.Null(pinned.Of("UI"));
    }

    [Fact]
    public void AKustomizationPinHoldsExactlyOneEntry()
    {
        var pinned = PinnedVersions.ParseKustomization(Kustomization, "ui");

        var entry = Assert.Single(pinned.Versions);
        Assert.Equal("ui", entry.Key);
        Assert.Equal("2.4.7", entry.Value);
        Assert.Null(pinned.Detail);
    }
}
