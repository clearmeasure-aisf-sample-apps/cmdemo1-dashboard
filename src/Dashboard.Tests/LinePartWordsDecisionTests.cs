using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The decision of #630, held in one place: the words of <see cref="AvailabilityPart" /> and <see cref="CostPart" /> are
/// declared never null and are not refused at the constructor, and the record's own doc comment says so - "the words are
/// the caller's" - so the next reader finds the decision written down rather than reading it out of a test. The two
/// records go the same way; when one of them is guarded or annotated <c>string?</c> and the other is not, or the doc
/// comment loses the sentence, this file is the one that notices.
/// </summary>
public class LinePartWordsDecisionTests
{
    [Fact]
    public void TheWordsOfBothPartsAreDeclaredNeverNullAndTheExtrasMayBeNothing()
    {
        Assert.Equal([false, false, false, true, true], Nullable(typeof(AvailabilityPart)));
        Assert.Equal([false, false], Nullable(typeof(CostPart)));
    }

    [Fact]
    public void NeitherPartRefusesANullWordAtTheMaking()
    {
        var availability = new AvailabilityPart(null!, null!, null!);
        var cost = new CostPart(null!, null!);

        Assert.Null(availability.Before);
        Assert.Null(availability.Strong);
        Assert.Null(availability.After);
        Assert.Null(cost.Amount);
        Assert.Null(cost.Period);
    }

    [Theory]
    [InlineData("DeliveryText.cs", "public sealed record AvailabilityPart(", "AvailabilityText.Parts")]
    [InlineData("CostReport.cs", "public sealed record CostPart(", "CostText.Parts")]
    public void EachPartWritesTheDecisionInItsOwnDocComment(string file, string declaration, string producer)
    {
        var source = File.ReadAllText(Path.Combine(RepoRoot(), "src", "Dashboard", "Health", file));
        var at = source.IndexOf(declaration, StringComparison.Ordinal);
        Assert.True(at > 0, $"{declaration} not found in {file}");

        var docs = string.Join('\n', source[..at].TrimEnd().Split('\n').Reverse()
            .TakeWhile(line => line.TrimStart().StartsWith("///", StringComparison.Ordinal)));

        Assert.Contains("The words are the caller's", docs, StringComparison.Ordinal);
        Assert.Contains($"<see cref=\"{producer}\" />", docs, StringComparison.Ordinal);
        Assert.Contains("never null by contract", docs, StringComparison.Ordinal);
        Assert.Contains("does not refuse a null on purpose", docs, StringComparison.Ordinal);
    }

    private static bool[] Nullable(Type record)
    {
        var nullability = new NullabilityInfoContext();
        return [.. Assert.Single(record.GetConstructors()).GetParameters()
            .Select(parameter => nullability.Create(parameter).WriteState == NullabilityState.Nullable)];
    }

    private static string RepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Dashboard.sln")))
        {
            directory = directory.Parent;
        }

        Assert.NotNull(directory);
        return directory.FullName;
    }
}
