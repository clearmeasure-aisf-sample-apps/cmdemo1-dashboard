using System.Reflection;
using System.Reflection.Emit;

namespace Dashboard.Tests;

/// <summary>
/// Pins the three words <see cref="Dashboard.Health.VersionText" /> now names as private consts: the build-metadata
/// mark, the endpoint's property word, and the fallback word the header says when a build carries no version.
/// </summary>
public class VersionTextWordsTests
{
    // A dynamic assembly carries no AssemblyInformationalVersionAttribute, so OfAssembly must answer the fallback.
    private static readonly Assembly NoVersionAssembly = AssemblyBuilder.DefineDynamicAssembly(
        new AssemblyName("dashboard-version-words-test"), AssemblyBuilderAccess.Run);

    [Fact]
    public void AnAssemblyWithoutAnInformationalVersionAnswersTheFallbackWord() =>
        Assert.Equal("unknown", Dashboard.Health.VersionText.OfAssembly(NoVersionAssembly));

    [Fact]
    public void TheEndpointWordIsReadCaseInsensitivelyAndCutAtTheMetadataMark() =>
        Assert.Equal("1.2.3", Dashboard.Health.VersionText.FromVersionResponse("{\"VERSION\":\"1.2.3+sha\"}"));

    [Theory]
    [InlineData("+abc", null)]
    [InlineData("1.0+a+b", "1.0")]
    public void DisplayCutsAtTheFirstMetadataMark(string? version, string? expected) =>
        Assert.Equal(expected, Dashboard.Health.VersionText.Display(version));
}
