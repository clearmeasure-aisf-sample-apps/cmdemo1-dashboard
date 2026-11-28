using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The one rule a freeze's list of names hides in <see cref="FreezeText.Covered" />: the names joined, or the system
/// as a whole when the list names nothing. It is internal to a public class and the app declares no
/// <c>InternalsVisibleTo</c>, so the rule is pinned directly by reflection on <see cref="FreezeText" /> - the same
/// way <see cref="DeploymentStateWordsTests" /> pins an internal method - for an empty list, a single name, a joined
/// list, and a null list it refuses, not only through the full sentences of <see cref="FreezeTextTests" />.
/// </summary>
public class FreezeTextCoveredTests
{
    /// <summary>The internal <see cref="FreezeText.Covered" /> method, reached by reflection under its own name.</summary>
    private static readonly MethodInfo CoveredMethod =
        typeof(FreezeText).GetMethod("Covered", BindingFlags.NonPublic | BindingFlags.Static)!;

    /// <summary>Call the internal <c>Covered</c> with one list of names, the way the page would.</summary>
    private static string Covered(IReadOnlyList<string> names)
    {
        var result = CoveredMethod.Invoke(null, [names]);
        return (string)result!;
    }

    /// <summary><c>Covered</c> is internal and static: the one place the "no names means the system" rule lives.</summary>
    [Fact]
    public void CoveredIsInternalAndStatic()
    {
        Assert.True(CoveredMethod.IsAssembly, "Covered is internal, so only the page and this test name it.");
        Assert.True(CoveredMethod.IsStatic, "Covered is static: a pure wording rule, no state.");
        Assert.Null(typeof(FreezeText).GetMethod("Covered", BindingFlags.Public | BindingFlags.Static));
    }

    /// <summary>A list that names nothing covers the system as a whole: a freeze with no projects or no environments.</summary>
    [Fact]
    public void NoNamesMeansTheSystem()
    {
        Assert.Equal("the system", Covered([]));
    }

    /// <summary>A single name is said alone, with no separator before or after it.</summary>
    [Fact]
    public void OneNameIsSaidAlone()
    {
        Assert.Equal("demo-ui", Covered(["demo-ui"]));
    }

    /// <summary>Two names are joined with the list's separator, in order.</summary>
    [Fact]
    public void TwoNamesAreJoinedInOrder()
    {
        Assert.Equal("demo-ui, demo-system", Covered(["demo-ui", "demo-system"]));
    }

    /// <summary>A null list is refused, not read: the caller holds the list; the rule does not invent one. The refusal
    /// is an <c>ArgumentNullException</c>, surfaced here through the reflection wrapper that <c>Invoke</c> adds.</summary>
    [Fact]
    public void ANullListIsRefused()
    {
        var ex = Assert.Throws<TargetInvocationException>(() => CoveredMethod.Invoke(null, [null!]));
        Assert.IsType<ArgumentNullException>(ex.InnerException);
    }
}
