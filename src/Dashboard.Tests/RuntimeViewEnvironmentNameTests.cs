namespace Dashboard.Tests;

/// <summary>
/// The runtime view keys its diagram cache by <see cref="EnvironmentName"/> and picks the addressed environment through
/// <see cref="EnvironmentName.Matches"/>: the cache keeps names that differ in case apart, the address matches in any case.
/// </summary>
public class RuntimeViewEnvironmentNameTests
{
    [Fact]
    public void TheDiagramCacheIsKeyedByEnvironmentName()
    {
        var markup = RuntimeViewMarkup();

        Assert.Contains("Dictionary<EnvironmentName, RuntimeDiagram> _diagrams", markup, StringComparison.Ordinal);
        Assert.DoesNotContain("Dictionary<string, RuntimeDiagram>", markup, StringComparison.Ordinal);
    }

    [Fact]
    public void TheAddressedEnvironmentIsMatchedThroughEnvironmentNameNotACaseInsensitiveStringCompare()
    {
        var markup = RuntimeViewMarkup();

        Assert.DoesNotContain("StringComparison.OrdinalIgnoreCase", markup, StringComparison.Ordinal);
        Assert.Contains(".Matches(Environment)", markup, StringComparison.Ordinal);
        Assert.Contains(".Matches(diagram.Manifest!.Environment)", markup, StringComparison.Ordinal);
    }

    [Fact]
    public void TheShownDiagramsEnvironmentIsComparedAsAnEnvironmentName()
    {
        var markup = RuntimeViewMarkup();

        Assert.DoesNotContain("_diagram.Environment, StringComparison.Ordinal", markup, StringComparison.Ordinal);
        Assert.DoesNotContain("_mounted.Environment, StringComparison.Ordinal", markup, StringComparison.Ordinal);
        Assert.Contains("== new EnvironmentName(_diagram.Environment)", markup, StringComparison.Ordinal);
        Assert.Contains("!= new EnvironmentName(_mounted.Environment)", markup, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("UAT", "uat")]
    [InlineData("Prod", "PROD")]
    public void ACacheKeyedLikeTheViewsKeepsEnvironmentsThatDifferInCaseApartWhileTheAddressMatchesBoth(string name, string other)
    {
        var first = new RuntimeDiagram(name, null, null, []);
        var second = new RuntimeDiagram(other, null, null, []);
        var cache = new Dictionary<EnvironmentName, RuntimeDiagram>
        {
            [new EnvironmentName(first.Environment)] = first,
            [new EnvironmentName(second.Environment)] = second,
        };

        Assert.Equal(2, cache.Count);
        Assert.Same(first, cache[new EnvironmentName(name)]);
        Assert.Same(second, cache[new EnvironmentName(other)]);
        Assert.True(new EnvironmentName(name).Matches(other));
        Assert.True(new EnvironmentName(other).Matches(name));
    }

    private static string RuntimeViewMarkup() => MarkupGuards.ComponentText("RuntimeView");
}
