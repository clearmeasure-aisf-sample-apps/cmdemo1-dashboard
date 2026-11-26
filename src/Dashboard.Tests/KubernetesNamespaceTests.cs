using Dashboard.Cluster;
using Dashboard.Health;

namespace Dashboard.Tests;

/// <summary>
/// A Kubernetes namespace's name: one ordinal comparison rule (equality, hashing, <see cref="KubernetesNamespace.Is"/>
/// and <see cref="KubernetesNamespace.Comparer"/>), a null or default name read as the empty name, and the cluster
/// callers (<see cref="EnvironmentIndex"/>, <see cref="ClusterGroups"/>) matching namespaces by that rule.
/// </summary>
public class KubernetesNamespaceTests
{
    [Fact]
    public void TheSameNameCaseForCaseIsTheSameNamespace()
    {
        var first = new KubernetesNamespace("cmdemo1-uat");
        var second = new KubernetesNamespace("cmdemo1-uat");

        Assert.Equal(first, second);
        Assert.True(first == second);
        Assert.Equal(first.GetHashCode(), second.GetHashCode());
        Assert.True(first.Is("cmdemo1-uat"));
    }

    [Theory]
    [InlineData("cmdemo1-uat", "CMDEMO1-UAT")]
    [InlineData("cmdemo1-uat", "cmdemo1-Uat")]
    [InlineData("cmdemo1-uat", " cmdemo1-uat")]
    [InlineData("cmdemo1-uat", "cmdemo1-uat ")]
    public void NamesThatDifferInCaseOrSpacesAreDifferentNamespaces(string name, string other)
    {
        var space = new KubernetesNamespace(name);

        Assert.NotEqual(space, new KubernetesNamespace(other));
        Assert.True(space != new KubernetesNamespace(other));
        Assert.False(space.Is(other));
        Assert.False(KubernetesNamespace.Comparer.Equals(name, other));
    }

    [Fact]
    public void AMissingNameIsNoNamespaceEvenForTheEmptyName()
    {
        Assert.False(new KubernetesNamespace(string.Empty).Is(null));
        Assert.False(default(KubernetesNamespace).Is(null));
        Assert.False(new KubernetesNamespace("cmdemo1-tdd").Is(null));
    }

    [Fact]
    public void ANullOrDefaultNameIsTheEmptyName()
    {
        Assert.Equal(string.Empty, new KubernetesNamespace(null).Value);
        Assert.Equal(string.Empty, default(KubernetesNamespace).Value);
        Assert.Equal(string.Empty, default(KubernetesNamespace).ToString());
        Assert.Equal(new KubernetesNamespace(null), default);
        Assert.Equal(new KubernetesNamespace(string.Empty), default);
        Assert.True(default(KubernetesNamespace).Is(string.Empty));
    }

    [Fact]
    public void TheNameIsKeptAsWritten()
    {
        Assert.Equal("  Cmdemo1-Tdd  ", new KubernetesNamespace("  Cmdemo1-Tdd  ").Value);
        Assert.Equal("  Cmdemo1-Tdd  ", new KubernetesNamespace("  Cmdemo1-Tdd  ").ToString());
    }

    [Fact]
    public void TheComparerIsOrdinal()
    {
        Assert.Same(StringComparer.Ordinal, KubernetesNamespace.Comparer);
    }

    [Fact]
    public void TheIndexAndTheClusterViewMatchNamespacesCaseForCase()
    {
        var environments = new[] { new EnvironmentInfo("uat", "nonprod", [], Namespace: "cmdemo1-uat") };
        var index = EnvironmentIndex.Of(environments);

        Assert.Contains("cmdemo1-uat", index.Namespaces);
        Assert.DoesNotContain("CMDEMO1-UAT", index.Namespaces);
        Assert.True(index.NameForNamespace.ContainsKey("cmdemo1-uat"));
        Assert.False(index.NameForNamespace.ContainsKey("CMDEMO1-UAT"));

        var status = new ClusterStatus(DateTimeOffset.UnixEpoch, null, null, [], [new ClusterNamespace("CMDEMO1-UAT", [], [])]);
        var groups = ClusterGroups.Of(status, environments, DateTimeOffset.UnixEpoch);

        var group = Assert.Single(groups.Environments);
        Assert.False(group.Found);
        Assert.Equal("cmdemo1-uat", group.Name);
        Assert.Equal("CMDEMO1-UAT", Assert.Single(groups.Platform).Name);
    }
}
