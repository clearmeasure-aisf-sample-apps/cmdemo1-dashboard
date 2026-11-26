using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The one named rule the pair's three records ask (#1842): a view needs both parts of its pair - its
/// manifest and its SVG - and is never drawn from half a pair. <c>RuntimeFilesRead&lt;TManifest&gt;.Of</c>
/// (<see cref="RuntimeFilesReadEdgeCaseTests" />), <c>RuntimeDiagram.IsValid</c>
/// (<see cref="RuntimeDiagramEdgeCaseTests" />) and <c>DatabaseDiagram.IsValid</c>
/// (<see cref="DatabaseDiagramEdgeCaseTests" />) each pinned their own corners of the question; these tests
/// pin the question itself, in its one name, so the rule cannot be hand-rolled back into the records without a
/// test failing.
/// <para>
/// The rule is internal and the app declares no <c>InternalsVisibleTo</c>, so it is found by its name in the
/// namespace where <see cref="RuntimeLoader" /> lives and asked through reflection, as
/// <see cref="RuntimeFilesReadEdgeCaseTests" /> finds the pair. Nothing here reaches the network.
/// </para>
/// </summary>
public class PairPartsEdgeCaseTests
{
    private const string Picture = "<svg xmlns=\"http://www.w3.org/2000/svg\"></svg>";

    private static readonly RuntimeManifest Diagram = new("uat", [], [], []);

    private static readonly DatabaseManifest Database = new("tdd", "sqldb-cmdemo1-tdd", null, null, false, [], 0);

    /// <summary>The one internal static type that owns the pair's question.</summary>
    private static Type TheRule() =>
        typeof(RuntimeLoader).Assembly
            .GetTypes()
            .Single(type => type.Namespace == "Dashboard.Runtime" && type.Name == "PairParts");

    /// <summary>Asks <c>PairParts.Both&lt;TManifest&gt;(manifest, svg)</c> for what it says.</summary>
    private static bool Both<TManifest>(TManifest? manifest, string? svg) where TManifest : class
    {
        var both = TheRule().GetMethod("Both", BindingFlags.Static | BindingFlags.NonPublic)!;

        return (bool)both.MakeGenericMethod(typeof(TManifest)).Invoke(null, [manifest, svg])!;
    }

    // ----------------------------------------------------------------------------- the four corners

    /// <summary>Both parts stand: the rule says true, for the manifest of a diagram and the manifest of a
    /// database view alike, and of an empty picture as of one with something in it - the rule asks whether the
    /// parts stand, not what they hold (what counts as a picture is the loader's question, pinned in
    /// <see cref="RuntimeLoaderTests" />).</summary>
    [Theory]
    [InlineData("")]
    [InlineData("not a picture")]
    public void BothPartsStandingAreBoth(string svg)
    {
        Assert.True(Both(Diagram, svg));
        Assert.True(Both(Database, Picture));
        Assert.True(Both(Diagram, Picture));
    }

    /// <summary>Either part missing is not both: the rule says false of a manifest without its picture, of a
    /// picture without its manifest, and of neither part at all - for both manifests the loader reads.</summary>
    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public void HalfAPairIsNotBoth(bool hasManifest, bool hasSvg)
    {
        RuntimeManifest? manifest = hasManifest ? Diagram : null;
        string? svg = hasSvg ? Picture : null;

        Assert.Equal(hasManifest && hasSvg, Both(manifest, svg));

        DatabaseManifest? database = hasManifest ? Database : null;
        Assert.Equal(hasManifest && hasSvg, Both(database, svg));
    }

    // ----------------------------------------------------------------------------- the shape of the rule

    /// <summary>The rule is one internal static class of the runtime's namespace, with one method: generic over
    /// the manifest, closed over both manifests the loader reads, asking for the manifest and the picture and
    /// answering a bool - the same one generic of one type parameter the pair itself is, so the rule and the
    /// pair cannot drift apart in what they hold.</summary>
    [Fact]
    public void TheRuleIsOneInternalStaticOfOneGenericMethod()
    {
        var rule = TheRule();

        Assert.True(rule.IsSealed);
        Assert.True(rule.IsAbstract);
        Assert.False(rule.IsPublic);
        Assert.Equal("Dashboard.Runtime", rule.Namespace);

        var both = Assert.Single(
            rule.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly),
            method => method.IsStatic && !method.IsSpecialName);

        Assert.Equal("Both", both.Name);
        Assert.True(both.IsGenericMethod);
        Assert.Equal(typeof(bool), both.ReturnType);

        var argument = Assert.Single(both.GetGenericArguments());

        var parameters = both.GetParameters();
        Assert.Equal(2, parameters.Length);
        Assert.True(parameters[0].ParameterType.IsGenericParameter);
        Assert.Equal(typeof(string), Nullable.GetUnderlyingType(parameters[1].ParameterType) ?? parameters[1].ParameterType);

        // Closed over the two manifests the loader reads, the rule is the pair's question for both of them.
        Assert.True((bool)both.MakeGenericMethod(typeof(RuntimeManifest)).Invoke(null, [Diagram, Picture])!);
        Assert.False((bool)both.MakeGenericMethod(typeof(DatabaseManifest)).Invoke(null, [Database, null])!);
    }

    /// <summary>The three records that once asked the question of their own all ask the one rule: the pair's
    /// verdict refuses a pair the rule would call not both, and each diagram record's verdict holds of exactly
    /// the pairs the rule calls both - the corners the records' own edge-case files pin, read through the rule
    /// that now owns them.</summary>
    [Fact]
    public void TheRecordsAskTheRuleAndNotTheirOwnCopy()
    {
        // Both parts, no errors: the pair keeps both, as the rule says both stand.
        Assert.True(Both(Diagram, Picture));
        Assert.True(new RuntimeDiagram("uat", Diagram, Picture, []).IsValid);
        Assert.True(new DatabaseDiagram(Database, Picture, []).IsValid);

        // Half a pair: the rule says not both, and so does each record's verdict.
        Assert.False(Both(Diagram, null));
        Assert.False(new RuntimeDiagram("uat", Diagram, null, []).IsValid);
        Assert.False(new DatabaseDiagram(Database, null, []).IsValid);

        Assert.False(Both((RuntimeManifest?)null, Picture));
        Assert.False(new RuntimeDiagram("uat", null, Picture, []).IsValid);
        Assert.False(new DatabaseDiagram(null, Picture, []).IsValid);

        Assert.False(Both((RuntimeManifest?)null, (string?)null));
        Assert.False(new RuntimeDiagram("uat", null, null, []).IsValid);
        Assert.False(new DatabaseDiagram(null, null, []).IsValid);
    }
}
