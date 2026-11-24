namespace Dashboard.Tests;

/// <summary>
/// The order of <see cref="ProbeUrl.Combine" />'s refusals, as its doc says it: what is missing is said before what
/// is wrong. No base address is refused first, then no path, and only then a base address that is relative to a
/// page. The refusals one by one are pinned in <see cref="ProbeUrlEdgeCaseTests" />. These tests pin which one
/// answers when more than one applies.
/// </summary>
public class ProbeUrlRefusalOrderTests
{
    [Theory]
    [InlineData("rel/path")]
    [InlineData("x")]
    public void ANullPathIsRefusedBeforeARelativeBaseAddressIs(string baseAddress)
    {
        var refused = Assert.Throws<ArgumentNullException>(
            () => ProbeUrl.Combine(new Uri(baseAddress, UriKind.Relative), null!));

        Assert.Equal("path", refused.ParamName);
    }

    [Fact]
    public void ANullBaseAddressIsRefusedBeforeANullPathIs()
    {
        var refused = Assert.Throws<ArgumentNullException>(() => ProbeUrl.Combine(null!, null!));

        Assert.Equal("baseAddress", refused.ParamName);
    }
}
