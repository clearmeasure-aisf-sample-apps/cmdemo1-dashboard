using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The badge words <c>Healthy</c> and <c>Unhealthy</c> have one owner (#2277): <see cref="HealthClassifier" />'s
/// <c>HealthyWord</c> and <c>UnhealthyWord</c>. The detailed-health badge (<see cref="HealthDetailText.Label" />)
/// and the container-app badge (<see cref="ContainerAppText.Label" />) read the words from there instead of
/// spelling their own copies, so a change to the word in one place cannot quietly split the badges.
/// <para>
/// The constants are internal and the app declares no <c>InternalsVisibleTo</c>, so the words are checked by
/// reflection under their own names, in the style of <c>ContainerAppHealthyWordAgreementTests</c>; the agreement
/// is pinned through the badges' public behaviour.
/// </para>
/// </summary>
public class BadgeWordOwnershipTests
{
    /// <summary><see cref="HealthClassifier" />'s <paramref name="name" />: its name, accessibility and value, reached by reflection.</summary>
    private static (bool IsLiteral, bool IsAssembly, string? Value) ClassifierWord(string name)
    {
        var field = typeof(HealthClassifier).GetField(name, BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(field);
        return (field!.IsLiteral, field.IsAssembly, field.GetRawConstantValue() as string);
    }

    /// <summary>True when <see cref="HealthDetailText" /> still declares a <paramref name="name" /> field of its own.</summary>
    private static bool HealthDetailTextDeclares(string name) =>
        typeof(HealthDetailText).GetField(name, BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public | BindingFlags.Instance) is not null;

    /// <summary>The owner's words are the ones they always were, and they are internal constants, not part of the classifier's public surface.</summary>
    [Fact]
    public void TheClassifierOwnsTheWordsAsInternalConstants()
    {
        var healthy = ClassifierWord("HealthyWord");
        Assert.True(healthy.IsLiteral, "HealthyWord is a const.");
        Assert.True(healthy.IsAssembly, "HealthyWord is internal, so the badges' words can read it.");
        Assert.Equal("Healthy", healthy.Value);
        Assert.Null(typeof(HealthClassifier).GetField("HealthyWord", BindingFlags.Public | BindingFlags.Static));

        var unhealthy = ClassifierWord("UnhealthyWord");
        Assert.True(unhealthy.IsLiteral, "UnhealthyWord is a const.");
        Assert.True(unhealthy.IsAssembly, "UnhealthyWord is internal, so the badges' words can read it.");
        Assert.Equal("Unhealthy", unhealthy.Value);
        Assert.Null(typeof(HealthClassifier).GetField("UnhealthyWord", BindingFlags.Public | BindingFlags.Static));
    }

    /// <summary>The detailed-health badge no longer keeps a private copy: <see cref="HealthDetailText" /> declares neither word.</summary>
    [Fact]
    public void TheDetailTextKeepsNoCopyOfTheBadgeWords()
    {
        Assert.False(HealthDetailTextDeclares("HealthyWord"));
        Assert.False(HealthDetailTextDeclares("UnhealthyWord"));
    }

    /// <summary>
    /// The agreement #2277 asks for: the detailed-health badge and the container-app badge say the owner's word
    /// for the same idea, so the three badges cannot drift.
    /// </summary>
    [Fact]
    public void TheDetailAndContainerBadgesSayTheOwnersWords()
    {
        Assert.Equal(HealthClassifier.Label(HealthState.Healthy), HealthDetailText.Label(CheckState.Healthy));
        Assert.Equal(HealthClassifier.Label(HealthState.Unhealthy), HealthDetailText.Label(CheckState.Unhealthy));
        Assert.Equal(HealthClassifier.Label(HealthState.Unhealthy), ContainerAppText.Label(ContainerAppState.Unhealthy));
    }
}
