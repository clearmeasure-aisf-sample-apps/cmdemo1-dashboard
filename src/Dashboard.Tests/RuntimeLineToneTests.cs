using System.Text.RegularExpressions;
using Dashboard.Runtime;

namespace Dashboard.Tests;

/// <summary>
/// The tile-line tones (<see cref="RuntimeLineTone"/>) are the exact words <c>js/runtime.js</c> turns into the class
/// <c>rt-line--{tone}</c> and the icon <c>LINE_ICON[tone]</c>; a changed word silently drops a line's styling.
/// </summary>
public sealed class RuntimeLineToneTests
{
    [Theory]
    [InlineData(RuntimeLineTone.Plain, "plain")]
    [InlineData(RuntimeLineTone.Muted, "muted")]
    [InlineData(RuntimeLineTone.Strong, "strong")]
    [InlineData(RuntimeLineTone.Serving, "serving")]
    [InlineData(RuntimeLineTone.Ok, "ok")]
    [InlineData(RuntimeLineTone.Warn, "warn")]
    [InlineData(RuntimeLineTone.InSync, "insync")]
    [InlineData(RuntimeLineTone.Differs, "differs")]
    [InlineData(RuntimeLineTone.Unknown, "unknown")]
    public void EachToneIsTheWordThePageDrawsBy(string tone, string word) => Assert.Equal(word, tone);

    [Fact]
    public void ALineWithoutAToneIsPlain() => Assert.Equal(RuntimeLineTone.Plain, new RuntimeTileLine("x").Tone);

    [Fact]
    public void EveryToneWithAnIconIsAKeyOfTheScriptsLineIcons()
    {
        var script = File.ReadAllText(MarkupGuards.SourcePath("wwwroot/js/runtime.js"));
        var table = Regex.Match(script, @"const LINE_ICON = \{(?<body>.*?)\};", RegexOptions.Singleline);
        Assert.True(table.Success, "runtime.js no longer declares LINE_ICON");
        var keys = Regex.Matches(table.Groups["body"].Value, @"(?<key>[a-z]+)\s*:").Select(match => match.Groups["key"].Value).ToHashSet();

        string[] withIcons = [RuntimeLineTone.Serving, RuntimeLineTone.Ok, RuntimeLineTone.Warn, RuntimeLineTone.InSync, RuntimeLineTone.Differs, RuntimeLineTone.Unknown];
        Assert.All(withIcons, tone => Assert.Contains(tone, keys));
    }

    [Fact]
    public void TheBuilderWritesNoBareLineTone()
    {
        var builder = File.ReadAllText(MarkupGuards.SourcePath("Runtime/RuntimePayloadBuilder.cs"));
        Assert.DoesNotMatch(@"""(muted|plain|ok|warn|strong|insync|differs)""", builder);
    }
}
