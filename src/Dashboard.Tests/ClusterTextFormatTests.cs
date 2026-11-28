namespace Dashboard.Tests;

/// <summary>
/// The named number formats and unit words <see cref="ClusterText"/> now reads its memory numbers and its
/// percentages through: the whole-number format of the MiB and KiB rows and of the percentage, the one-decimal
/// format of the GiB row, and the unit words beside them. The rounding and the negative-zero edges of the
/// percentage are pinned here against the named constants, so the rewrite cannot change a word it says.
/// </summary>
public class ClusterTextFormatTests
{
    // ----- the percent

    /// <summary>The whole percentage rounds to the even side and says the negative that rounds to no percent
    /// as the negative zero it is: the same output the rewrite must keep.</summary>
    [Theory]
    [InlineData(2.5, "2 %")]
    [InlineData(-0.4, "-0 %")]
    [InlineData(0, "0 %")]
    [InlineData(99.9, "100 %")]
    [InlineData(double.NaN, "NaN %")]
    [InlineData(double.PositiveInfinity, "Infinity %")]
    public void ThePercentIsSaidInItsWholeFormatWithThePercentSign(double percent, string words) =>
        Assert.Equal(words, ClusterText.Percent(percent));

    // ----- the memory

    /// <summary>The bytes at the ends of the named units: the half GiB in its one-decimal format, the byte
    /// below a KiB in the whole format of the KiB row, the half KiB the rounding rounds up, and the 1.6 MiB the
    /// whole format rounds to the MiB it reaches.</summary>
    [Theory]
    [InlineData(1.5 * 1024 * 1024 * 1024, "1.5 GiB")]
    [InlineData(1, "0 KiB")]
    [InlineData(512, "1 KiB")]
    [InlineData(1023, "1 KiB")]
    [InlineData(1024 * 1024 * 1.6, "2 MiB")]
    public void TheMemoryIsSaidInTheFormatOfTheUnitItStandsIn(double bytes, string words) =>
        Assert.Equal(words, ClusterText.Memory(bytes));

    // ----- the source guard

    /// <summary>The named formats and unit words the class says its numbers with stand as constants in its
    /// source, so the literals cannot come back beside the code that says them.</summary>
    [Fact]
    public void TheNamedFormatsAndUnitsAreDeclaredInTheSource()
    {
        var source = File.ReadAllText(Path.Combine(RepoRoot(), "src", "Dashboard", "Cluster", "ClusterText.cs"));

        Assert.Contains("private const string WholeFormat", source, StringComparison.Ordinal);
        Assert.Contains("private const string GibibyteFormat", source, StringComparison.Ordinal);
        Assert.Contains("private const string GibibyteUnit", source, StringComparison.Ordinal);
        Assert.Contains("private const string MebibyteUnit", source, StringComparison.Ordinal);
        Assert.Contains("private const string KibibyteUnit", source, StringComparison.Ordinal);

        // The percent's sign and whole rounding moved to the one Shares.WholePercent rule: the named constant is
        // pinned where it now stands, and the class can neither type the sign nor the rounding again.
        var shares = File.ReadAllText(Path.Combine(RepoRoot(), "src", "Dashboard", "Shares.cs"));
        Assert.Contains("internal const string PercentSign", shares, StringComparison.Ordinal);
        Assert.Contains("Percent(double percent) => Shares.WholePercent(percent);", source, StringComparison.Ordinal);
        Assert.DoesNotContain("\" %\"", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Math.Round(percent).ToString(", source, StringComparison.Ordinal);
    }

    private static string RepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Dashboard.sln")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("Dashboard.sln not found above the test output.");
    }
}
