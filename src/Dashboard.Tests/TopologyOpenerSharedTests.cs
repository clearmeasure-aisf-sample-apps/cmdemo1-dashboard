using System.Text.RegularExpressions;

namespace Dashboard.Tests;

/// <summary>
/// The reader of <c>topology.json</c> opens its file through the app's one opener, <c>RuntimeManifestParser.TryOpen</c>,
/// rather than a fourth copy of it (#1784): the same three refusals in the same words, at the same line and position, and
/// the same two licenses (a trailing comma and a comment) as the files of <c>runtime/</c>. The guard at the bottom reads
/// the parser's source, so a hand-rolled open, a second set of options or a reworded sentence there is refused.
/// </summary>
public class TopologyOpenerSharedTests
{
    /// <summary>An empty, a blank and a missing file are refused in the runtime parser's own words.</summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AnEmptyTopologyIsRefusedInTheOpenersWords(string? json)
    {
        Assert.Equal(RuntimeManifestParser.ParseManifest(json).Errors, TopologyParser.Parse(json).Errors);
        Assert.Equal(["The file is empty."], TopologyParser.Parse(json).Errors);
    }

    /// <summary>A file that will not open is named at the very line and position the runtime reader names.</summary>
    [Theory]
    [InlineData("""{ "environments": """)]
    [InlineData("{\n  \"environments\": [\n    { \"name\": ")]
    [InlineData("not json")]
    public void ABrokenTopologyIsNamedWhereTheRuntimeReaderNamesIt(string json)
    {
        var errors = TopologyParser.Parse(json).Errors;

        Assert.Equal(RuntimeManifestParser.ParseManifest(json).Errors, errors);
        Assert.StartsWith("The file is not valid JSON (line ", Assert.Single(errors), StringComparison.Ordinal);
    }

    /// <summary>A root that is no object is refused by the opener, in the runtime reader's sentence.</summary>
    [Theory]
    [InlineData("[]")]
    [InlineData("42")]
    [InlineData("\"tdd\"")]
    [InlineData("null")]
    public void ATopologyOfAnotherRootIsRefusedInTheOpenersWords(string json)
    {
        Assert.Equal(["The file must contain a JSON object."], TopologyParser.Parse(json).Errors);
        Assert.Equal(RuntimeManifestParser.ParseManifest(json).Errors, TopologyParser.Parse(json).Errors);
    }

    /// <summary>The licenses of the shared options reach the topology: a comment and trailing commas are read past.</summary>
    [Fact]
    public void ATopologyWithACommentAndTrailingCommasIsStillRead()
    {
        var result = TopologyParser.Parse("""
            {
              /* written by the deployment */
              "environments": [ { "name": "tdd", }, ],
            }
            """);

        Assert.True(result.IsValid, string.Join(' ', result.Errors));
        Assert.Equal(["tdd"], result.Topology!.Environments.Select(environment => environment.Name));
    }

    /// <summary>The parser's source writes none of the three sentences, no options and no parse of its own, and calls the
    /// shared opener: the open of <c>topology.json</c> is written in one place with the open of <c>runtime/</c>.</summary>
    [Fact]
    public void TheTopologyParserWritesNoOpenOfItsOwn()
    {
        var text = File.ReadAllText(MarkupGuards.SourcePath(Path.Combine("Health", "TopologyParser.cs")));
        var offenders = new List<string>();

        foreach (var sentence in new[] { "The file is empty.", "The file is not valid JSON", "The file must contain a JSON object." })
        {
            if (text.Contains(sentence, StringComparison.Ordinal))
            {
                offenders.Add($"TopologyParser.cs writes \"{sentence}\" (the opener owns it)");
            }
        }

        if (Regex.IsMatch(text, @"\bJsonDocument\s*\.\s*Parse\s*\("))
        {
            offenders.Add("TopologyParser.cs calls JsonDocument.Parse (the opener owns the one)");
        }

        if (Regex.IsMatch(text, @"\bJsonDocumentOptions\b"))
        {
            offenders.Add("TopologyParser.cs mentions JsonDocumentOptions (the app has one set)");
        }

        Assert.Empty(offenders);
        Assert.Contains("RuntimeManifestParser.TryOpen(", text, StringComparison.Ordinal);
    }
}
