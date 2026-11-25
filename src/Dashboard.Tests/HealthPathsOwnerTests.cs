namespace Dashboard.Tests;

/// <summary>
/// The single owner of the default health path (#583): <see cref="HealthPaths.DefaultHealthPath" /> is the one word
/// the page asks when a deployable or a container app names no health path of its own, and
/// <see cref="DeployableInfo.DefaultHealthPath" /> and <see cref="ContainerAppTarget.DefaultHealthPath" /> both stand
/// on it. The two records used to each declare the word for themselves, so rewording one left the other asking a
/// path the app does not serve; these tests pin the word on the owner and pin that the two records' constants and
/// the parser's fallback all read the same word by construction.
/// </summary>
public class HealthPathsOwnerTests
{
    private static readonly Uri App = new("https://ca-demo.example.io");
    private static readonly Uri Door = new("https://demo.azurefd.net");

    /// <summary>The owner says the one word the page asks: <c>/_healthcheck</c> - the underscore path the app serves,
    /// not the root and not another spelling of it.</summary>
    [Fact]
    public void TheOwnerSaysTheUnderscoreHealthcheckWord()
    {
        Assert.Equal("/_healthcheck", HealthPaths.DefaultHealthPath);
        Assert.StartsWith("/", HealthPaths.DefaultHealthPath, StringComparison.Ordinal);
        Assert.Equal(HealthPaths.DefaultHealthPath, HealthPaths.DefaultHealthPath.Trim());
    }

    /// <summary>The two records stand on the owner: each of their <c>DefaultHealthPath</c> constants is the owner's
    /// word, and the two records therefore say one word, not two that happen to agree today.</summary>
    [Fact]
    public void TheTwoRecordsReadTheDefaultHealthPathFromTheOwner()
    {
        Assert.Equal(HealthPaths.DefaultHealthPath, DeployableInfo.DefaultHealthPath);
        Assert.Equal(HealthPaths.DefaultHealthPath, ContainerAppTarget.DefaultHealthPath);
        Assert.Equal(DeployableInfo.DefaultHealthPath, ContainerAppTarget.DefaultHealthPath);
    }

    /// <summary>The paths the page actually asks stand on the owner's word: a deployable the file gives its default
    /// health path to, and a container app nobody declared, are both asked at <see cref="HealthPaths.DefaultHealthPath" />.</summary>
    [Fact]
    public void ThePageAsksTheOwnersWordForADeployableAndForAnUndeclaredContainerApp()
    {
        var deployable = new DeployableInfo(
            "ui",
            Door,
            DeployableInfo.DefaultHealthPath,
            DeployableInfo.DefaultAlivePath,
            DeployableInfo.DefaultVersionPath,
            []);
        var target = new ContainerAppTarget(App);

        Assert.Equal(HealthPaths.DefaultHealthPath, deployable.PathFor(ProbeKind.Health));
        Assert.Equal(HealthPaths.DefaultHealthPath, target.EffectiveHealthPath);
        Assert.Equal(HealthPaths.DefaultHealthPath, target.PathFor(ProbeKind.Health));
        Assert.Equal(HealthPaths.DefaultHealthPath, target.PathFor(ProbeKind.Liveness));
    }

    /// <summary>The owner is one static class with the one constant and nothing else to call: a second path added
    /// to it is a change the tests see here.</summary>
    [Fact]
    public void TheOwnerHoldsTheOneConstantAndNoMethods()
    {
        var type = typeof(HealthPaths);

        Assert.True(type.IsAbstract && type.IsSealed, "static classes are sealed and abstract to reflection");
        Assert.Equal(
            ["DefaultHealthPath"],
            [.. type.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.DeclaredOnly)
                .Select(field => field.Name)
                .Order(StringComparer.Ordinal)]);
        Assert.Empty(type.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.DeclaredOnly));
    }
}
