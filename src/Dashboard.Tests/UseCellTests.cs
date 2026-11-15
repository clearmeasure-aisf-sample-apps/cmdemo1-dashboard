using System.Runtime.CompilerServices;

namespace Dashboard.Tests;

/// <summary>
/// When the cluster has no measurement, usage cells show a dash. Screen readers would announce that dash (or nothing);
/// the meaning lives in title, which most of them do not announce. The dash is aria-hidden and a visually-hidden
/// "No measurement" carries the accessible name instead.
/// </summary>
public class UseCellTests
{
    private static string Components([CallerFilePath] string here = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "Dashboard", "Components"));

    private const string None =
        """<span class="use-cell__none" title="No measurement"><span aria-hidden="true">—</span><span class="visually-hidden">No measurement</span></span>""";

    [Theory]
    [InlineData("UseCell.razor")]
    [InlineData("UseRow.razor")]
    public void TheNoneSpanHidesTheDashAndSaysNoMeasurement(string file)
    {
        var text = File.ReadAllText(Path.Combine(Components(), file));

        Assert.Contains(None, text, StringComparison.Ordinal);
        Assert.Contains("""<span aria-hidden="true">—</span>""", text, StringComparison.Ordinal);
        Assert.Contains("""<span class="visually-hidden">No measurement</span>""", text, StringComparison.Ordinal);
    }
}
