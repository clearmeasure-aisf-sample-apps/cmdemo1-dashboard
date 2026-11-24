using System.Text.RegularExpressions;

namespace Dashboard.Tests;

/// <summary>
/// The guard on the header summary's <c>summary--&lt;level&gt;</c> class modifier (#1003). The modifier used to be the
/// enum member's own name, lowered, an implicit CSS contract nothing checked. Now the word comes from
/// <see cref="SummaryWords.CssClass" />, and these tests tie the three places that know the word to each other: the
/// enum member names, the markup in <c>App.razor</c> and the style sheet <c>app.css</c>.
/// </summary>
public partial class SummaryClassGuardTests
{
    private const string Modifier = "summary--";

    /// <summary>Each level, looked up by its member name, gives its fixed class word. Renaming a member, even with a
    /// tool that renames every use, fails here, so a rename has to decide on purpose whether the markup word changes.</summary>
    [Theory]
    [InlineData("Empty", "empty")]
    [InlineData("Pending", "pending")]
    [InlineData("AllHealthy", "allhealthy")]
    [InlineData("Problems", "problems")]
    public void EachLevelByItsMemberNameGivesItsClassWord(string member, string word) =>
        Assert.Equal(word, SummaryWords.CssClass(Enum.Parse<SummaryLevel>(member)));

    /// <summary>Every level has a word, the words are distinct, and each is a plain lowercase CSS identifier, so
    /// <c>summary--&lt;word&gt;</c> is always a valid class name and no two levels share a style.</summary>
    [Fact]
    public void EveryLevelHasADistinctLowercaseClassWord()
    {
        var words = Enum.GetValues<SummaryLevel>().Select(SummaryWords.CssClass).ToArray();

        Assert.Equal(words.Length, words.Distinct(StringComparer.Ordinal).Count());
        Assert.All(words, word => Assert.Matches(ClassWord(), word));
    }

    /// <summary>The page writes the summary's modifier in one place, and builds it from
    /// <see cref="SummaryWords.CssClass" />, never from the level's own name.</summary>
    [Fact]
    public void ThePageBuildsTheSummaryModifierOnlyFromSummaryWords()
    {
        var app = MarkupGuards.AppText();

        Assert.Equal(1, MarkupGuards.CountOccurrences(app, "class=\"summary summary--@SummaryWords.CssClass(Summary.Level)\""));
        Assert.Equal(1, MarkupGuards.CountOccurrences(app, "\"summary " + Modifier));
        Assert.DoesNotContain("Level.ToString()", app, StringComparison.Ordinal);
    }

    /// <summary>The style sheet styles the summary itself, and any <c>.summary--*</c> rule it has or later adds names
    /// one of the words the page emits. A rule for a word the code never writes, which a level rename would leave
    /// behind, fails here.</summary>
    [Fact]
    public void EverySummaryModifierTheStyleSheetNamesIsAWordThePageEmits()
    {
        var css = File.ReadAllText(MarkupGuards.SourcePath(Path.Combine("wwwroot", "css", "app.css")));
        var emitted = Enum.GetValues<SummaryLevel>().Select(level => Modifier + SummaryWords.CssClass(level)).ToHashSet(StringComparer.Ordinal);

        Assert.Contains(".summary {", css, StringComparison.Ordinal);
        var styled = StyledModifier().Matches(css).Select(match => match.Groups[1].Value).ToArray();
        Assert.All(styled, modifier => Assert.Contains(modifier, emitted));
    }

    [GeneratedRegex("^[a-z][a-z0-9-]*$")]
    private static partial Regex ClassWord();

    // A class selector of the summary's modifier: ".summary--word", not ".summary-versions".
    [GeneratedRegex(@"\.(summary--[A-Za-z0-9_-]+)")]
    private static partial Regex StyledModifier();
}
