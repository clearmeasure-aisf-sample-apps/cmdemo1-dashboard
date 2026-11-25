using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The decision of #1710: the badge's word for a healthy container app is the word a healthy health-check body
/// says, so <see cref="ContainerAppText.Label" /> reads <c>ContainerAppProbe</c>'s <c>HealthyWord</c> rather than
/// spelling the word a second time. The app is healthy exactly when the body says the word, so the badge is
/// healthy exactly when the body says the word, and the agreement stands by construction.
/// <para>
/// The constant is internal and the app declares no <c>InternalsVisibleTo</c>, so the word is read by reflection
/// under its own name; the agreement is pinned against the badge's public word and the probe's public reading of
/// a body, so a re-spelled literal on either side is seen here.
/// </para>
/// </summary>
public class ContainerAppHealthyWordAgreementTests
{
    /// <summary>The probe's <c>HealthyWord</c>, reached by reflection under its own name.</summary>
    private static string HealthyWord()
    {
        var field = typeof(ContainerAppProbe).GetField("HealthyWord", BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(field);
        Assert.True(field!.IsLiteral);
        Assert.True(field.IsAssembly, "HealthyWord is internal, so the badge's word can read it.");
        Assert.Null(typeof(ContainerAppProbe).GetField("HealthyWord", BindingFlags.Public | BindingFlags.Static));
        return (string)field.GetRawConstantValue()!;
    }

    /// <summary>The constant the probe compares the body against is the word it always was: an internal literal,
    /// not a public member of the probe's surface.</summary>
    [Fact]
    public void TheHealthyWordIsTheInternalConstantOfTheProbe() =>
        Assert.Equal("Healthy", HealthyWord());

    /// <summary>The agreement #1710 asks for: the badge's word for a healthy app is the probe's payload word, read
    /// from the constant rather than spelled a second time, so the two cannot drift.</summary>
    [Fact]
    public void TheBadgeSaysTheWordTheHealthyBodySays() =>
        Assert.Equal(HealthyWord(), ContainerAppText.Label(ContainerAppState.Healthy));

    /// <summary>
    /// The other direction of the agreement, through the probe's public reading: a body that says the badge's own
    /// word - plain, in the case the page does not read, or in its JSON <c>status</c> - is the healthy body, and a
    /// word the badge does not say is not.
    /// </summary>
    [Fact]
    public void TheProbeReadsTheBadgeWordAsAHealthyBody()
    {
        var word = ContainerAppText.Label(ContainerAppState.Healthy);

        Assert.True(ContainerAppProbe.IsHealthyPayload(200, word));
        Assert.True(ContainerAppProbe.IsHealthyPayload(200, word.ToUpperInvariant()));
        Assert.True(ContainerAppProbe.IsHealthyPayload(200, $$"""{ "status": "{{word}}" }"""));

        Assert.False(ContainerAppProbe.IsHealthyPayload(200, word + "y"));
        Assert.False(ContainerAppProbe.IsHealthyPayload(200, $$"""{ "status": "Ok" }"""));
    }
}
