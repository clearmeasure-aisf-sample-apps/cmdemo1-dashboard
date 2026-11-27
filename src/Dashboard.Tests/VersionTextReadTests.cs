using System.Text.Json;

namespace Dashboard.Tests;

/// <summary>
/// Pins <see cref="VersionText.Read" />: the one read of a record's shown version, built on
/// <see cref="VersionText.Display" /> and the <c>version</c> word. Unlike
/// <see cref="VersionText.FromVersionResponse" />, it stays case-sensitive (it goes through
/// <c>JsonRead.Text</c> / <see cref="JsonElement.TryGetProperty" />), and it answers null for a parent that is
/// not an object or gives no version word.
/// </summary>
public class VersionTextReadTests
{
    [Theory]
    [InlineData("""{"version":"2.4.21+abc"}""", "2.4.21")]
    [InlineData("""{"version":"2.4.21"}""", "2.4.21")]
    public void TheShownVersionIsReadAndCutAtTheMetadataMark(string json, string expected) =>
        Assert.Equal(expected, VersionText.Read(Parse(json)));

    [Theory]
    [InlineData("""{"version":"  "}""")]
    [InlineData("""{"version":"+abc"}""")]
    [InlineData("""{"version":42}""")]
    [InlineData("""{}""")]
    [InlineData("""{"Version":"2.4.21"}""")]
    public void NoVersionWordIsNoShownVersion(string json) =>
        Assert.Null(VersionText.Read(Parse(json)));

    [Fact]
    public void ANullParentGivesNoShownVersion() =>
        Assert.Null(VersionText.Read(null));

    [Theory]
    [InlineData("""[1,2]""")]
    [InlineData("\"2.4.21\"")]
    public void AParentThatIsNotAnObjectGivesNoShownVersion(string json) =>
        Assert.Null(VersionText.Read(Parse(json)));

    private static JsonElement Parse(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }
}
