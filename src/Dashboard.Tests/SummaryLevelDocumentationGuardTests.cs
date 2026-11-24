namespace Dashboard.Tests;

/// <summary>
/// A guard on the words written above the four members of <see cref="SummaryLevel" />, the level the header weighs
/// what it has checked against, declared in <c>src/Dashboard/Health/HealthSummary.cs</c>.
/// <para>
/// The four are a boundary, and the boundary tests - <see cref="SummaryLevelBoundaryEdgeCaseTests" />,
/// <see cref="SummaryLevelEdgeCaseTests" /> and the count triples of
/// <see cref="HealthSummaryEdgeCaseTests" /> - turn on what each of them means: nothing in the topology, nothing known
/// to be wrong while checks are still owed, nothing not healthy, and at least one thing not healthy. A level whose
/// meaning stands only in the reader's head is the meaning a later edit drifts, so the comment is part of the guard
/// and not decoration: every member of the four carries a <c>&lt;summary&gt;</c> of its own, one line of it, saying a
/// sentence that ends, like the other three, where the sentence ends. The same rule already holds of
/// <see cref="HealthState" /> and of the sayings of <see cref="SummaryWords" />, which is why the four that sit between
/// them may not be the exception.
/// </para>
/// <para>
/// The guard reads the source file the way <see cref="TargetStatusFactoryTests" /> reads the source of
/// <c>TargetStatus</c> - walking up to the solution and reading the file whole - and refuses the file, with the member
/// named, on the first summary it cannot find: no comment at all, a comment of two lines where its three neighbours
/// hold to one, a <c>&lt;summary&gt;</c> left unclosed, one that closes on the words of nothing, and a member added to
/// the enum without its name appearing among the four the guard was written for.
/// </para>
/// </summary>
public class SummaryLevelDocumentationGuardTests
{
    /// <summary>What one member of the enum carries above its own name: the name it declares, and the text of every
    /// <c>///</c> line that stands above it, leading slashes cut away.</summary>
    private sealed record Member(string Name, List<string> Docs)
    {
        /// <summary>The one line a member's documentation is allowed to be: a <c>&lt;summary&gt;</c> opened and closed
        /// on the same line. Null when the member carries no such line.</summary>
        public string? Summary => Docs.Count == 1 && Docs[0].StartsWith("<summary>", StringComparison.Ordinal) && Docs[0].EndsWith("</summary>", StringComparison.Ordinal)
            ? Docs[0]
            : null;

        /// <summary>The sentence between the tags - the words the reader of the tooltip gets, spaces cut off either end.</summary>
        public string SummaryText
        {
            get
            {
                var line = Summary ?? string.Empty;
                var opened = line.IndexOf("<summary>", StringComparison.Ordinal) + "<summary>".Length;

                return line[opened..line.LastIndexOf("</summary>", StringComparison.Ordinal)].Trim();
            }
        }
    }

    /// <summary>Every member of the four carries its own one-line <c>&lt;summary&gt;</c>, and the line says a sentence
    /// of its own rather than the shape of one: the tag closed on the line it opened, the words between them not left
    /// bare, and the sentence ending where the other three end.</summary>
    [Fact]
    public void EveryMemberOfSummaryLevelCarriesItsOwnSummaryComment()
    {
        foreach (var member in Members())
        {
            Assert.True(member.Docs.Count > 0, $"{member.Name} carries no /// comment at all");
            Assert.NotNull(member.Summary);
            Assert.True(member.SummaryText.Length > 0, $"{member.Name} carries an empty <summary>");
            Assert.EndsWith(".", member.SummaryText);
        }
    }

    /// <summary>The four are the four: a fifth member added to the enum is a member this guard has never heard of, and
    /// it fails here by name rather than going undocumented because nothing looked for it. The order pinned is the
    /// order declared, which is the order of the numbers 0 to 3
    /// <see cref="SummaryLevelBoundaryEdgeCaseTests" /> pins of the reflection over the type.</summary>
    [Fact]
    public void TheGuardKnowsEveryMemberTheEnumDeclares()
    {
        Assert.Equal(
            ["Empty", "Pending", "AllHealthy", "Problems"],
            Members().Select(member => member.Name));
    }

    /// <summary>The load-bearing part of each comment, which is the meaning the boundary tests turn on and nothing
    /// else: the empty topology, the check still owed on not every endpoint, nothing left not healthy, and at least
    /// one thing not healthy. The words a future rewording picks are its own; the four meanings are not its own to
    /// drop, and each of the four sentences below says one of them.</summary>
    [Fact]
    public void EachSummarySaysTheMeaningTheBoundaryTestsWeighOn()
    {
        var members = Members().ToDictionary(member => member.Name, StringComparer.Ordinal);

        Assert.Contains("no endpoint", members["Empty"].SummaryText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("not every", members["Pending"].SummaryText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("not healthy", members["Pending"].SummaryText, StringComparison.OrdinalIgnoreCase);

        // AllHealthy is every endpoint healthy, and Problems the one that is not: the two say opposite things, and
        // the words that tell them apart stand in the two sentences, not in the reader's memory of the switch.
        Assert.Contains("every", members["AllHealthy"].SummaryText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("healthy", members["AllHealthy"].SummaryText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("not healthy", members["AllHealthy"].SummaryText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("not healthy", members["Problems"].SummaryText, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>The four members of <c>enum SummaryLevel</c>, read out of the file that declares them: the text of every
    /// <c>///</c> line standing above a member, and the name the member declares. Blank lines and the braces are passed
    /// over; the body ends at the brace that closes the enum.</summary>
    private static List<Member> Members()
    {
        var lines = File.ReadAllLines(Path.Combine(RepoRoot(), "src", "Dashboard", "Health", "HealthSummary.cs"));

        var declared = Array.FindIndex(lines, line => line.Contains("enum SummaryLevel", StringComparison.Ordinal));
        Assert.True(declared >= 0, "no enum SummaryLevel was found to read the comments of");

        var open = Array.FindIndex(lines, declared, line => line.Contains('{'));
        Assert.True(open >= 0, $"the enum SummaryLevel found on line {declared + 1} never opens");

        var members = new List<Member>();
        var docs = new List<string>();

        for (var line = open + 1; line < lines.Length; line++)
        {
            var text = lines[line].Trim();

            if (text.Length == 0)
            {
                continue;
            }

            if (text == "}")
            {
                break;
            }

            if (text.StartsWith("///", StringComparison.Ordinal))
            {
                docs.Add(text[3..].Trim());
                continue;
            }

            if (text.StartsWith('['))
            {
                continue;
            }

            var name = text.Split('=')[0].Trim().Split(' ', '\t')[0].TrimEnd(',');
            members.Add(new Member(name, docs));
            docs = [];
        }

        Assert.True(members.Count > 0, $"no member of the enum SummaryLevel was read out of line {open + 1} onwards");

        return members;
    }

    /// <summary>The walk up from the test's own bin to the solution, the route
    /// <see cref="TargetStatusFactoryTests" /> walks to reach the source of the file it guards.</summary>
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
