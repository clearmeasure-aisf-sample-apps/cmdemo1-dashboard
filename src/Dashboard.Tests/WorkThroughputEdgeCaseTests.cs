using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="WorkThroughput" />, the one row of
/// <c>src/Dashboard/Work/WorkItemsReport.cs</c> that says how much a single bot identity finished: its name, how
/// many pull requests merged, how many reached production, and — for the newer file — how many reached Done.
/// <para>
/// What the other files already pin is left alone. The taking of the rows out of <c>workitems.json</c> is
/// <c>WorkThroughput.Read</c>, which is internal to an app that declares no <c>InternalsVisibleTo</c>, and is
/// therefore pinned through <see cref="WorkItemsReport.Parse" /> in
/// <see cref="WorkThroughputAndReleaseTests" />; the words the page makes of a row - the <c>: N done</c> and the
/// <c>N merged, N deployed to prod</c> - are <see cref="WorkBotText.ThroughputLine" />'s and are pinned there too;
/// and the row the items are counted into when the file gives none is <see cref="WorkBotText.Throughput" />'s and
/// is pinned in the same file. This record's own members - the four parts it holds and the words it says itself -
/// are nowhere among those.
/// </para>
/// <para>
/// What is left, and is pinned here, is the record itself: the constructor handing each of its four arguments to
/// the property of its name; the name held as the <see cref="string" /> it is, at the ends a name runs to - the
/// empty name, the whitespace-only name, the single letter, and the name with its own spaces kept - and the three
/// counts held as the <see cref="int" /> they are, at nought, at either end an <see cref="int" /> runs to, and
/// below nought; a part nobody gave standing unsaid, which is no zero and no empty name; the four parts coming back
/// out in the order they went in; the copy that turns one part while it keeps the other three; the two rows being
/// one while their four parts are, with the hash, the set and the operators following that, and the nothing on
/// either side of each; the words the row says its four parts in, and those words following whoever reads a count;
/// and the public surface, so that the day a fifth part is added the list below says so.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: a row is a name and three counts and
/// calls nothing at all, so every value below is one the test brought and every call says the same thing twice.
/// </para>
/// </summary>
public class WorkThroughputEdgeCaseTests
{
    /// <summary>A row with all four parts said: the identity, its merges, its deploys and the done count.</summary>
    private static WorkThroughput Full => new("cm-ai-ops-bot", 4, 2, 7);

    /// <summary>A row whose every count is unsaid and whose name is the empty name: the nothing a file that says
    /// nothing lands on, had the reading of it kept the row.</summary>
    private static WorkThroughput Unsaid => new("", null, null, null);

    /// <summary>A row whose every count is nought and whose name is said: an empty count of each, which is no
    /// unsaid part.</summary>
    private static WorkThroughput Zeroed => new("alpha", 0, 0, 0);

    // --------------------------------- the four parts, by the names of their own properties

    /// <summary>The constructor's four arguments, position by position, come back as the property of the same name:
    /// the name first, the merges beside it, the deploys beside those, the done count last - the order the newer
    /// file's <c>agent</c>, <c>merged</c>, <c>deployedProd</c> and <c>throughputByAgent</c> are read in. A new row of
    /// the same four values is another row, not the one it was built of, and the parts are held in their own types:
    /// the name as the string it is, the counts as the ints they are.</summary>
    [Fact]
    public void TheConstructorHandsEachArgumentToThePropertyOfItsName()
    {
        var row = new WorkThroughput("cluster-bot", 9, 3, 12);

        Assert.Equal("cluster-bot", row.Agent);
        Assert.Equal(9, row.Merged);
        Assert.Equal(3, row.DeployedProd);
        Assert.Equal(12, row.Done);

        // The done count the older file leaves out is left out, and the rest stand where they were put.
        var older = new WorkThroughput("delivery-bot", 2, 1);

        Assert.Equal("delivery-bot", older.Agent);
        Assert.Equal(2, older.Merged);
        Assert.Equal(1, older.DeployedProd);
        Assert.Null(older.Done);

        var again = new WorkThroughput("cluster-bot", 9, 3, 12);

        Assert.Equal(row, again);
        Assert.NotSame(row, again);
    }

    /// <summary>The name is a <see cref="string" /> and stands at the ends a name runs to - the empty name, the
    /// whitespace-only name, the single letter, the name with its spaces kept and the name of a whole line - and the
    /// three counts are <see cref="int" />s and stand at the ends they run to: nought, the least count there is, the
    /// greatest, and a negative count. The record keeps each as it is given and never trims, never clamps and never
    /// rounds: whoever reads the file decides what a name and a count can be, and this record only holds them.</summary>
    [Theory]
    [InlineData("alpha", 0, 0, 0)]
    [InlineData("a", 1, 1, 1)]
    [InlineData("cm-ai-ops-bot", 4, 2, 7)]
    [InlineData("  spaced  ", -1, -2, -3)]
    [InlineData("", -2147483648, 2147483647, 0)]
    [InlineData("\t\n\r", 2147483647, -2147483648, 2147483647)]
    [InlineData("alpha", null, null, null)]
    [InlineData("alpha", 5, null, null)]
    [InlineData("alpha", null, 5, null)]
    [InlineData("alpha", null, null, 5)]
    public void ThePartsStandAtTheEndsTheyRunTo(string agent, int? merged, int? deployed, int? done)
    {
        var row = new WorkThroughput(agent, merged, deployed, done);

        Assert.Equal(agent, row.Agent);
        Assert.Equal(merged, row.Merged);
        Assert.Equal(deployed, row.DeployedProd);
        Assert.Equal(done, row.Done);
        Assert.Equal(row, new WorkThroughput(agent, merged, deployed, done));
        Assert.Equal(row, row with { });
        Assert.Equal(row.GetHashCode(), new WorkThroughput(agent, merged, deployed, done).GetHashCode());
    }

    /// <summary>A name nobody gave stands as the empty name it is, which is no name and no whitespace - the record
    /// whose name is empty is a record, and it is another record than the one whose name is a single space, told
    /// apart by its equality, its operators and its words. A count nobody gave stands unsaid, which is no nought
    /// count: the record whose three counts are unsaid is another record than the one whose three counts are nought,
    /// and the page reads that difference as nothing to report rather than as an empty one.</summary>
    [Fact]
    public void ANameNobodyGaveStandsUnsaidAndACountNobodyGaveIsNotANought()
    {
        Assert.Equal("", Unsaid.Agent);
        Assert.Null(Unsaid.Merged);
        Assert.Null(Unsaid.DeployedProd);
        Assert.Null(Unsaid.Done);

        Assert.NotEqual(Unsaid, Zeroed);
        Assert.False(Unsaid == Zeroed);
        Assert.True(Unsaid != Zeroed);
        Assert.False(Unsaid.Equals((object)Zeroed));

        // One count unsaid is enough, and the unsaid count is not the nought count either.
        Assert.NotEqual(Unsaid, new WorkThroughput("", null, null, 0));
        Assert.NotEqual(Unsaid, new WorkThroughput("", 0, null, null));
        Assert.NotEqual(Unsaid, new WorkThroughput("", null, 0, null));
        Assert.NotEqual(Unsaid, new WorkThroughput(" ", null, null, null));
        Assert.Equal(
            4,
            new HashSet<WorkThroughput>
            {
                Unsaid,
                Unsaid with { Merged = 0 },
                Unsaid with { DeployedProd = 0 },
                Unsaid with { Done = 0 },
            }.Count);

        var turnedOff = Full with { Merged = null, DeployedProd = null, Done = null };

        Assert.Null(turnedOff.Merged);
        Assert.Null(turnedOff.DeployedProd);
        Assert.Null(turnedOff.Done);
        Assert.Equal("cm-ai-ops-bot", turnedOff.Agent);
    }

    // ------------------------------------------------- a part turned, and the copy that turns it

    /// <summary>Each part may be turned as the row is made or copied - the name to another name, a count to another
    /// count, a said count to the nothing the older file leaves - and stands where it is turned to, while the parts
    /// the turn does not name stay what they were. A copy is another row and never the same reference; a copy that
    /// names no part at all is the row it was copied from, in its parts, in its hash and in its words.</summary>
    [Fact]
    public void ACopyTurnsThePartItNamesAndKeepsTheOtherThree()
    {
        var row = Full;

        var nameTurned = row with { Agent = "runtime-bot" };

        Assert.Equal("runtime-bot", nameTurned.Agent);
        Assert.Equal(4, nameTurned.Merged);
        Assert.Equal(2, nameTurned.DeployedProd);
        Assert.Equal(7, nameTurned.Done);
        Assert.NotSame(row, nameTurned);
        Assert.NotEqual(row, nameTurned);

        var doneTurnedOff = row with { Done = null };

        Assert.Null(doneTurnedOff.Done);
        Assert.Equal(4, doneTurnedOff.Merged);
        Assert.Equal(2, doneTurnedOff.DeployedProd);
        Assert.Equal(new WorkThroughput("cm-ai-ops-bot", 4, 2), doneTurnedOff);

        var mergedTurnedToNought = row with { Merged = 0 };

        Assert.Equal(0, mergedTurnedToNought.Merged);
        Assert.NotEqual(row, mergedTurnedToNought);

        var unchanged = row with { };

        Assert.Equal(row, unchanged);
        Assert.NotSame(row, unchanged);
        Assert.Equal(row.GetHashCode(), unchanged.GetHashCode());
        Assert.Equal(Words(row), Words(unchanged));

        // The written-as-made form turns the same part the copy turns, and the original stands as it stood.
        var asMade = new WorkThroughput("cost-bot", 4, 2, 7) { DeployedProd = 5 };

        Assert.Equal("cost-bot", asMade.Agent);
        Assert.Equal(5, asMade.DeployedProd);
        Assert.Equal(4, row.Merged);
        Assert.Equal(7, row.Done);

        var repacked = row with { Agent = row.Agent, Merged = row.Merged, DeployedProd = row.DeployedProd, Done = row.Done };

        Assert.Equal(row, repacked);
        Assert.NotSame(row, repacked);
    }

    // --------------------------------------------- the row falling apart, and its own declared shape

    /// <summary>The four parts come back out in the order they went in - the name, the merges, the deploys, the
    /// done count - and the parts nobody gave come out as the nothing they are, not as nought. What came out goes
    /// back in the same way: the row the four parts fall apart into is the row they were.</summary>
    [Fact]
    public void TheFourPartsComeOutAgainInTheOrderTheyWentIn()
    {
        var (agent, merged, deployed, done) = Full;

        Assert.Equal("cm-ai-ops-bot", agent);
        Assert.Equal(4, merged);
        Assert.Equal(2, deployed);
        Assert.Equal(7, done);
        Assert.Equal(Full, new WorkThroughput(agent, merged, deployed, done));

        var (noAgent, noMerged, noDeployed, noDone) = new WorkThroughput(" ", null, null, null);

        Assert.Equal(" ", noAgent);
        Assert.Null(noMerged);
        Assert.Null(noDeployed);
        Assert.Null(noDone);
        Assert.Equal(new WorkThroughput(" ", null, null, null), new WorkThroughput(noAgent, noMerged, noDeployed, noDone));

        var (least, greatest, other, count) = new WorkThroughput("x", int.MinValue, int.MaxValue, int.MaxValue);

        Assert.Equal("x", least);
        Assert.Equal(int.MinValue, greatest);
        Assert.Equal(int.MaxValue, other);
        Assert.Equal(int.MaxValue, count);
    }

    /// <summary>The row's whole public surface, as the running assembly holds it: its four parts named in the order
    /// the constructor names them, and the members every record is asked - falling apart, comparing, hashing and
    /// saying itself - and nothing else to call. The way in is one: the four parts, none of them left out, for a
    /// row with no parts at all is the nothing the reading of a file calls for and is had from
    /// <c>WorkThroughput.Read</c>, not from a shorter way in. A fifth part added to the record has to be written
    /// here as well, which is the point: the page cannot be shown a part nobody decided what it means. And nothing
    /// on it is a moment, an address or a length of time the record would have to go and look up: a name and three
    /// counts, and no clock among them - which is why no test in this file needs one.</summary>
    [Fact]
    public void TheRecordHasFourPartsAndTheMembersEveryRecordIsAsked()
    {
        var type = typeof(WorkThroughput);

        Assert.True(type.IsSealed);
        Assert.False(type.IsValueType);
        Assert.Equal(typeof(object), type.BaseType);
        Assert.Equal([typeof(IEquatable<WorkThroughput>)], type.GetInterfaces());

        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        Assert.Equal(["Agent", "Merged", "DeployedProd", "Done"], properties.Select(property => property.Name));
        Assert.Equal(typeof(string), properties[0].PropertyType);
        Assert.Equal(typeof(int?), properties[1].PropertyType);
        Assert.Equal(typeof(int?), properties[2].PropertyType);
        Assert.Equal(typeof(int?), properties[3].PropertyType);
        Assert.All(properties, property =>
        {
            Assert.True(property.CanRead);
            Assert.True(property.CanWrite);
            Assert.NotNull(property.SetMethod);
            Assert.True(property.GetMethod!.IsPublic);

            // The parts are init-only: they are said once, at the making or at the copy, and never turned after.
            Assert.Contains(typeof(IsExternalInit), property.SetMethod!.ReturnParameter.GetRequiredCustomModifiers());

            // And each of the four is a name or a count that may be unsaid, never a moment and never a reference to
            // follow.
            Assert.DoesNotContain(
                property.PropertyType,
                new[] { typeof(DateTimeOffset), typeof(DateTime), typeof(TimeSpan), typeof(Uri) });
        });

        // The contract every record compares by is the compiler's private one: no public part of the row.
        Assert.Null(type.GetProperty("EqualityContract", BindingFlags.Public | BindingFlags.Instance));
        Assert.NotNull(type.GetProperty("EqualityContract", BindingFlags.NonPublic | BindingFlags.Instance));
        Assert.DoesNotContain("EqualityContract", PublicSurface(), StringComparer.Ordinal);

        Assert.Equal(
            ["Agent", "Deconstruct", "DeployedProd", "Done", "Equals", "GetHashCode", "Merged", "ToString", "op_Equality", "op_Inequality"],
            PublicSurface());

        var arguments = type.GetConstructors().Single().GetParameters();

        Assert.Single(type.GetConstructors(BindingFlags.Public | BindingFlags.Instance));
        Assert.Equal(4, arguments.Length);
        Assert.Equal(["Agent", "Merged", "DeployedProd", "Done"], arguments.Select(argument => argument.Name));
        Assert.Equal([typeof(string), typeof(int?), typeof(int?), typeof(int?)], arguments.Select(argument => argument.ParameterType));

        // The first three parts are said at the making; the done count the newer file gives is the only one with a
        // way in left out, which stands as the nothing an older file has not got.
        Assert.False(arguments[0].HasDefaultValue);
        Assert.False(arguments[1].HasDefaultValue);
        Assert.False(arguments[2].HasDefaultValue);
        Assert.True(arguments[3].HasDefaultValue);
        Assert.Null(arguments[3].DefaultValue);

        // The copy the with walks is the compiler's private one, and the reading of the file is no public member.
        var copy = Assert.Single(type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance));
        Assert.Equal(type, copy.GetParameters().Single().ParameterType);
        Assert.Null(type.GetMethod("Read", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly));

        // The two overloads of the equality every record is asked with, and the one Deconstruct.
        Assert.Equal(2, type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Count(method => method.Name == "Equals"));
        var parts = type.GetMethod("Deconstruct")!;
        Assert.Equal(["Agent", "Merged", "DeployedProd", "Done"], parts.GetParameters().Select(parameter => parameter.Name));
        Assert.All(parts.GetParameters(), parameter => Assert.True(parameter.IsOut));

        // And no member of it is given a moment, an address or a length of time to go and ask about.
        var other = type
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .SelectMany(method => method.GetParameters())
            .Select(parameter => parameter.ParameterType)
            .Where(parameter => parameter != type)
            .ToList();
        Assert.DoesNotContain(typeof(DateTimeOffset), other);
        Assert.DoesNotContain(typeof(DateTime), other);
        Assert.DoesNotContain(typeof(TimeSpan), other);
        Assert.DoesNotContain(typeof(Uri), other);
    }

    // -------------------------------------------------------------- the equality, the hash, and the two

    /// <summary>Two rows are one while their four parts are - the name as the name it stands on and the counts as
    /// the counts they stand on, the nothing among them included - whichever of the ways of asking is used: the
    /// record's own <c>Equals</c>, the one of <see cref="object" />, <c>==</c> and <c>!=</c>, from either side. The
    /// hash follows the equality, boxed or unboxed and from one call to the next, so the rows that are one are one
    /// row of a set: which is how a row the test writes is found again among the ones the file was read into.</summary>
    [Fact]
    public void TwoRowsAreOneWhileTheirFourPartsAre()
    {
        var one = Full;
        var two = new WorkThroughput("cm-ai-ops-bot", 4, 2, 7);

        Assert.True(one.Equals(two));
        Assert.True(one.Equals(one));
        Assert.True(one.Equals((object)two));
        Assert.True(one == two);
        Assert.True(two == one);
        Assert.False(one != two);
        Assert.Equal(one, two);
        Assert.NotSame(one, two);
        Assert.Equal(one.GetHashCode(), two.GetHashCode());
        Assert.Equal(one.GetHashCode(), ((object)one).GetHashCode());
        Assert.Equal(one.GetHashCode(), one.GetHashCode());
        Assert.Single(new HashSet<WorkThroughput> { one, two });
        Assert.Contains(two, new HashSet<WorkThroughput> { one });

        var byAgent = new Dictionary<WorkThroughput, string> { [one] = "done" };
        Assert.Equal("done", byAgent[two]);

        // The name is compared as a string: a name with its own spaces is another name, and a name of a different
        // case is another name still, for the record asks the string's own equality.
        Assert.NotEqual(one, new WorkThroughput(" cm-ai-ops-bot ", 4, 2, 7));
        Assert.NotEqual(one, new WorkThroughput("CM-AI-OPS-BOT", 4, 2, 7));
        Assert.Equal(Unsaid, new WorkThroughput("", null, null, null));
        Assert.True(Unsaid == Unsaid);
        Assert.False(Unsaid != Unsaid);
        Assert.Equal(Unsaid.GetHashCode(), ((object)Unsaid).GetHashCode());
    }

    /// <summary>A difference in one part is another row: the name turned by a single letter, a count turned by one,
    /// a said count turned to the nothing the older file leaves, the name and a count swapped into each other's
    /// places, and the whole row stood in another order of its own making. A set of the rows that differ in one part
    /// each holds them all apart.</summary>
    [Fact]
    public void ARowThatDiffersInOnePartIsAnotherRow()
    {
        var one = Full;

        Assert.NotEqual(one, one with { Agent = "cm-ai-ops-bot " });
        Assert.NotEqual(one, one with { Agent = "" });
        Assert.NotEqual(one, one with { Merged = null });
        Assert.NotEqual(one, one with { DeployedProd = null });
        Assert.NotEqual(one, one with { Done = null });
        Assert.NotEqual(one, one with { Merged = 5 });
        Assert.NotEqual(one, one with { DeployedProd = 3 });
        Assert.NotEqual(one, one with { Done = 8 });
        Assert.NotEqual(one, new WorkThroughput("cm-ai-ops-bot", 2, 4, 7));
        Assert.NotEqual(Zeroed, new WorkThroughput("alpha", 1, 0, 0));

        Assert.True(one != (one with { Done = 8 }));
        Assert.False(one == (one with { Merged = null }));
        Assert.False(one.Equals((object)(one with { DeployedProd = null })));
        Assert.False(one.Equals(one with { Agent = "beta" }));

        // The two ends of a count, and the empty and the said name, are four rows apart.
        Assert.Equal(
            4,
            new HashSet<WorkThroughput>
            {
                one,
                new WorkThroughput("cm-ai-ops-bot", int.MinValue, 2, 7),
                new WorkThroughput("cm-ai-ops-bot", 4, int.MaxValue, 7),
                new WorkThroughput("cm-ai-ops-bot", 4, 2, int.MaxValue),
            }.Count);
    }

    /// <summary>The nothing on either side of the members every record is asked with: two rows that are no row at
    /// all are one, a row and no row are two - the equality and the inequality each saying it, from either side -
    /// and the row asked about itself says no to the nothing, to its own words as a name, to a bare count, to the
    /// anonymous thing that happens to carry its four parts, and to the week the chart is made of.</summary>
    [Fact]
    public void NoRowIsOneWithNoRowAndNotWithAnyOtherThing()
    {
        WorkThroughput? nothing = null;
        WorkThroughput? alsoNothing = null;
        var there = Full;

        Assert.Null(default(WorkThroughput));
        Assert.True(nothing == alsoNothing);
        Assert.False(nothing != alsoNothing);
        Assert.False(nothing == there);
        Assert.True(nothing != there);
        Assert.False(there == (WorkThroughput?)null);
        Assert.True(there != (WorkThroughput?)null);
        Assert.True((WorkThroughput?)null == nothing);
        Assert.False(there.Equals(nothing));
        Assert.False(there.Equals((object?)null));
        Assert.True(there.Equals(there));

        // A row whose every count is unsaid is still a row: what turns it into no row at all is the reading of the
        // file, pinned through WorkItemsReport.Parse, and not the record.
        Assert.NotNull(Unsaid);
        Assert.False(there.Equals(Unsaid));

        Assert.False(there.Equals("WorkThroughput { Agent = cm-ai-ops-bot, Merged = 4, DeployedProd = 2, Done = 7 }"));
        Assert.False(there.Equals((object)4));
        Assert.False(there.Equals((object)7));
        Assert.False(there.Equals(new { Agent = "cm-ai-ops-bot", Merged = 4, DeployedProd = 2, Done = 7 }));
        Assert.False(there.Equals(new WorkWeek(null, null, null)));
        Assert.False(there.Equals(new WorkDuration(null, null, null)));
        Assert.Equal(2, new HashSet<WorkThroughput?> { there, nothing, alsoNothing }.Count);
    }

    /// <summary>The hash says the four parts and nothing else: the same parts hashed again are the same hash, a part
    /// turned is another hash, and the ends of a count are hashed with the count and not instead of it. The three
    /// rows of nothing, of nought and of every part said stand apart in a set of their own even though the two whose
    /// counts are all unsaid and all nought share a hash - which is what the set, and not the hash, is there to
    /// settle.</summary>
    [Fact]
    public void TheHashFollowsThePartsAndStandsStillFromOneCallToTheNext()
    {
        var full = Full;
        var least = new WorkThroughput("x", int.MinValue, int.MinValue, int.MinValue);
        var greatest = new WorkThroughput("x", int.MaxValue, int.MaxValue, int.MaxValue);

        Assert.Equal(full.GetHashCode(), new WorkThroughput("cm-ai-ops-bot", 4, 2, 7).GetHashCode());
        Assert.Equal(Unsaid.GetHashCode(), Unsaid.GetHashCode());
        Assert.Equal(least.GetHashCode(), new WorkThroughput("x", int.MinValue, int.MinValue, int.MinValue).GetHashCode());
        Assert.NotEqual(full.GetHashCode(), least.GetHashCode());
        Assert.NotEqual(least.GetHashCode(), greatest.GetHashCode());
        Assert.NotEqual(full.GetHashCode(), (full with { Merged = 5 }).GetHashCode());
        Assert.NotEqual(full.GetHashCode(), (full with { DeployedProd = 3 }).GetHashCode());
        Assert.NotEqual(full.GetHashCode(), (full with { Done = 8 }).GetHashCode());
        Assert.NotEqual(full.GetHashCode(), (full with { Agent = "beta" }).GetHashCode());
        Assert.Equal(3, new HashSet<WorkThroughput> { full, Unsaid, Zeroed }.Count);
        Assert.Equal(2, new HashSet<WorkThroughput> { least, greatest }.Count);
    }

    // ------------------------------------------------------------ the words the record says itself

    /// <summary>The words the row says its four parts in: its own name at the front, the parts in the order the
    /// constructor names them, each count as the plain count it stands on and each part nobody said as the empty
    /// place between the mark and the comma or the closing brace. These are the record's words and not the page's:
    /// no <c>: N done</c>, no <c>merged</c>, no <c>deployed to prod</c> and no <c>unnamed</c> - all of those are
    /// <see cref="WorkBotText.ThroughputLine" />'s and are pinned there.</summary>
    [Fact]
    public void TheRecordSaysItsFourPartsInTheOrderTheyAreNamed()
    {
        Assert.Equal("WorkThroughput { Agent = cm-ai-ops-bot, Merged = 4, DeployedProd = 2, Done = 7 }", Words(Full));
        Assert.Equal("WorkThroughput { Agent = , Merged = , DeployedProd = , Done =  }", Words(Unsaid));
        Assert.Equal("WorkThroughput { Agent = alpha, Merged = 0, DeployedProd = 0, Done = 0 }", Words(Zeroed));
        Assert.Equal("WorkThroughput { Agent = zeta, Merged = 1, DeployedProd = 4, Done =  }", Words(new WorkThroughput("zeta", 1, 4)));
        Assert.Equal("WorkThroughput { Agent = beta, Merged = , DeployedProd = 2, Done = 3 }", Words(new WorkThroughput("beta", null, 2, 3)));
        Assert.Equal("WorkThroughput { Agent =   spaced  , Merged = -1, DeployedProd = -2, Done = -3 }", Words(new WorkThroughput("  spaced  ", -1, -2, -3)));
        Assert.Equal("WorkThroughput { Agent = x, Merged = -2147483648, DeployedProd = 2147483647, Done = 2147483647 }", Words(new WorkThroughput("x", int.MinValue, int.MaxValue, int.MaxValue)));

        // The words are no measure of the row: an unsaid count and a count of nought are said apart, and the page's
        // word for a row that is nothing at all is a word this record has never heard of.
        Assert.NotEqual(Words(Unsaid), Words(Zeroed));
        foreach (var word in new[] { ": ", " merged", "merged", "deployed", "to prod", "done", "unnamed", "—", "<" })
        {
            Assert.DoesNotContain(word, Words(Full), StringComparison.Ordinal);
        }
    }

    /// <summary>The words of the counts at either end of an <see cref="int" />, and of a name that carries its own
    /// spaces: the record says each count as the count it stands on, with no unit and no dash, and the name as the
    /// string it is - the spaces it carries kept and not trimmed, and no word of the page's among them.</summary>
    [Fact]
    public void TheWordsSayTheCountsAtTheirEndsAndTheNameAsItStands()
    {
        Assert.Equal(
            "WorkThroughput { Agent = alpha, Merged = 2147483647, DeployedProd = -2147483648, Done =  }",
            Words(new WorkThroughput("alpha", int.MaxValue, int.MinValue)));
        Assert.Equal(
            "WorkThroughput { Agent =   spaced  , Merged = 0, DeployedProd = 0, Done = 0 }",
            Words(new WorkThroughput("  spaced  ", 0, 0, 0)));

        // A name with spaces and a name without them are the same string to the equality only when they are the same
        // string: the words say the difference the equality settles, and a count of nought is said apart from a count
        // nobody gave.
        Assert.NotEqual(Words(new WorkThroughput("spaced", 0, 0, 0)), Words(new WorkThroughput("  spaced  ", 0, 0, 0)));
        Assert.NotEqual(Words(new WorkThroughput("alpha", 0, 0, 0)), Words(new WorkThroughput("alpha", null, null, null)));
    }

    /// <summary>The counts in the row's own words follow whoever reads them, which is what sets them apart from the
    /// page's words and from the counts themselves: a German reader of the record is told the counts with a comma for
    /// the decimal and the name as the same letters to whichever reader - and two rows the two readers see differently
    /// worded are one row still, for the words are no part of the equality. The three counts are whole <see cref="int" />s,
    /// so the only difference a reader brings is the sign and the digit grouping of a number that is no whole
    /// <see cref="int" /> - which none of them is - and the name stays the same letters to every reader.</summary>
    [Fact]
    public void TheRecordsOwnWordsFollowTheReaderWhileThePartsThemselvesDoNot()
    {
        var row = Full;

        Assert.Equal("WorkThroughput { Agent = cm-ai-ops-bot, Merged = 4, DeployedProd = 2, Done = 7 }", Words(row));

        ViewerCulture.German(() =>
        {
            // Whole counts carry no decimal point, so a German reader says the same four parts as the invariant
            // reader; the name is the same letters to either.
            Assert.Equal("WorkThroughput { Agent = cm-ai-ops-bot, Merged = 4, DeployedProd = 2, Done = 7 }", row.ToString());
        });

        // The words apart by whatever reader says them, the row the same: whoever reads the file, and whoever shows
        // it, decide what a row means; the record only holds the four parts, and hashes them the same whichever
        // reader is asked.
        Assert.Equal(row, new WorkThroughput("cm-ai-ops-bot", 4, 2, 7));
        ViewerCulture.German(() => Assert.Equal(row.GetHashCode(), new WorkThroughput("cm-ai-ops-bot", 4, 2, 7).GetHashCode()));
    }

    // --------------------------------------------------------------------------- the machinery

    /// <summary>The public members the row declares itself: its four properties and the methods that are not the get
    /// and the set of one - the two operators kept although the compiler names them specially, since they are the
    /// <c>==</c> and the <c>!=</c> a caller writes - with the record's compiler-named clone walked past. The
    /// constructor is no method, so it is named by the test of the surface rather than by this list.</summary>
    private static List<string> PublicSurface() =>
        [.. typeof(WorkThroughput)
            .GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(member => member is PropertyInfo
                             || member is MethodInfo method
                             && (!method.IsSpecialName || method.Name.StartsWith("op_", StringComparison.Ordinal)))
            .Where(member => !member.Name.Contains('<', StringComparison.Ordinal))
            .Select(member => member.Name)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];

    /// <summary>The words a row says its four parts in, taken in the invariant culture whatever the reader of this
    /// test happens to be: the record's own words follow whoever reads them, which is pinned on its own further up
    /// this file, so the strings there are the point and the culture only the way to say them once.</summary>
    private static string Words(WorkThroughput row) => ViewerCulture.Invariant(() => row.ToString()!);
}
