using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Dashboard.Tests;

/// <summary>
/// A <see cref="PodRow" /> is one row of a namespace's table: the <see cref="ClusterPod" /> as the status file holds
/// it and the <see cref="PodState" /> the rule has landed it on, and nothing else - two parts, no more.
/// <see cref="PodRowEdgeCaseTests" /> already pins the row's own life: the pod and the state kept as they were given,
/// the pod that says little, the row that is not there, the row of the pod that is not there, two rows being one while
/// the pod and the state are, the row falling apart and being copied, and the ends of the state, of the restart count
/// and of the clock as the row holds them. <see cref="NamespaceGroupTests" /> and <see cref="ClusterGroupsTests" />
/// pin what a row is worth in a namespace's counts and where it stands in the table, and
/// <see cref="PodStateEdgeCaseTests" /> the state it carries.
/// <para>
/// What follows is the residue of the public surface, which none of that reaches. First the row's shape as the running
/// assembly holds it - the two parts, their order and their types, the one way in, the members every record is asked,
/// and nothing else to call: a third part would have to be written down here as well, which is the point. Then the row
/// of no pod in the three members that file holds only rows with a pod in: the words it says itself, the parts it
/// falls apart into, and the copy of it a <c>with</c> makes. Then the row's equality and hash code as a
/// <see cref="HashSet{PodRow}" />, a <c>Distinct</c> and a <see cref="Dictionary{TKey, TValue}" /> use them, which is how a row is one row among many
/// rather than merely equal to another when asked. Then the words again, said by a pod whose name is commas, braces, a
/// line break, a space, an accent or a thousand characters - the row says the pod's words as the pod says them, with
/// nothing escaped, nothing cut and nothing quoted. Last the one fact the fixed strings elsewhere cannot say: the
/// row's words follow whoever reads them where the pod's numbers are concerned and nowhere else, the part's names and
/// the state's name being the page's own vocabulary in any culture.
/// </para>
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: every pod is one the test builds, every moment it
/// says it started is one the test brings, and no row here is worked out of a pod by <c>PodRules.StateOf</c> - the
/// state is handed to the row itself, as <see cref="NamespaceGroupEdgeCaseTests" /> does for the same reason.
/// </para>
/// </summary>
public class PodRowSurfaceEdgeCaseTests
{
    /// <summary>The four states the page knows, in the order the enum declares them.</summary>
    private static readonly PodState[] States = [PodState.Ready, PodState.Starting, PodState.Unhealthy, PodState.Finished];

    // ------------------------------------------------------------------ the row's own shape

    /// <summary>The row's whole public surface, as the running assembly holds it: the pod first and the state after,
    /// each read and each the place a <c>with</c> may turn, the one constructor that takes the two in that order and
    /// never a default for either, and the members every record is asked - comparing, hashing, saying itself, falling
    /// apart, and the two operators kept although the compiler names them specially, since they are the <c>==</c> and
    /// the <c>!=</c> a caller writes - and nothing else to call. A third part on the row, or a rule the row works out
    /// for itself, has to be written here as well: the table cannot be shown a part nobody decided what it means, and
    /// a row that read a clock of its own would be a row whose state moves between two renderings of one file.</summary>
    [Fact]
    public void TheRowHasTwoPartsAndTheMembersEveryRecordIsAsked()
    {
        var type = typeof(PodRow);

        Assert.True(type.IsSealed);
        Assert.False(type.IsValueType);
        Assert.Equal(typeof(object), type.BaseType);
        Assert.Equal([typeof(IEquatable<PodRow>)], type.GetInterfaces());

        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        Assert.Equal(["Pod", "State"], properties.Select(property => property.Name));
        Assert.Equal(typeof(ClusterPod), properties[0].PropertyType);
        Assert.Equal(typeof(PodState), properties[1].PropertyType);
        Assert.All(properties, property =>
        {
            Assert.True(property.CanRead);
            Assert.True(property.CanWrite);
            Assert.NotNull(property.SetMethod);
            Assert.True(property.GetMethod!.IsPublic);

            // Both parts are said once, at the making or at the copy, and never turned afterwards.
            Assert.Contains(typeof(IsExternalInit), property.SetMethod!.ReturnParameter.GetRequiredCustomModifiers());
        });

        Assert.Equal(
            ["Deconstruct", "Equals", "GetHashCode", "Pod", "State", "ToString", "op_Equality", "op_Inequality"],
            PublicSurface());

        var arguments = type.GetConstructors().Single().GetParameters();

        Assert.Equal(2, arguments.Length);
        Assert.Equal(["Pod", "State"], arguments.Select(argument => argument.Name));
        Assert.Equal([typeof(ClusterPod), typeof(PodState)], arguments.Select(argument => argument.ParameterType));
        Assert.All(arguments, argument => Assert.False(argument.HasDefaultValue));

        // The way in is one; the clone the <c>with</c> walks is the compiler's own, and the contract every record
        // compares by is nothing a caller of the row sees.
        Assert.Single(type.GetConstructors(BindingFlags.Public | BindingFlags.Instance));
        var clone = Assert.Single(type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance));
        Assert.Equal(type, clone.GetParameters().Single().ParameterType);
        Assert.Null(type.GetProperty("EqualityContract", BindingFlags.Public | BindingFlags.Instance));
        Assert.NotNull(type.GetProperty("EqualityContract", BindingFlags.NonPublic | BindingFlags.Instance));
        Assert.DoesNotContain("EqualityContract", PublicSurface(), StringComparer.Ordinal);

        // The equality every record is asked with, in both of its shapes, and the one falling apart.
        Assert.Equal(
            2,
            type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Count(method => method.Name == nameof(Equals)));

        var parts = type.GetMethod("Deconstruct")!;
        Assert.Equal(["Pod", "State"], parts.GetParameters().Select(parameter => parameter.Name));
        Assert.All(parts.GetParameters(), parameter => Assert.True(parameter.IsOut));

        // And no part of the row is a moment, a length, an address or a cancellation: nothing the row could ask a
        // clock or a network about, only the two facts it is handed.
        List<Type> kinds =
            [.. type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .SelectMany(method => method.GetParameters())
                .Concat(type.GetConstructors().SelectMany(constructor => constructor.GetParameters()))
                .Select(parameter => parameter.ParameterType)];

        Assert.DoesNotContain(typeof(DateTimeOffset), kinds);
        Assert.DoesNotContain(typeof(DateTime), kinds);
        Assert.DoesNotContain(typeof(TimeSpan), kinds);
        Assert.DoesNotContain(typeof(Uri), kinds);
        Assert.DoesNotContain(typeof(CancellationToken), kinds);
    }

    // --------------------------------------------------------- the row of no pod, in what is left of it

    /// <summary>The row of no pod says itself with nothing for the pod: the word for the part, the space where its
    /// words would stand, and the state said whole - the empty between them being how every record of this app says a
    /// part that is not there, as <see cref="NamespaceGroupEdgeCaseTests" /> pins for a group's absent environment.
    /// Whatever the state is - one of the four, one the enum does not name, or the end of the count - the row says it
    /// as the state says itself, and never reaches into the pod it does not have to find out.</summary>
    [Theory]
    [InlineData(PodState.Ready, "PodRow { Pod = , State = Ready }")]
    [InlineData(PodState.Starting, "PodRow { Pod = , State = Starting }")]
    [InlineData(PodState.Unhealthy, "PodRow { Pod = , State = Unhealthy }")]
    [InlineData(PodState.Finished, "PodRow { Pod = , State = Finished }")]
    [InlineData((PodState)4, "PodRow { Pod = , State = 4 }")]
    [InlineData((PodState)(-1), "PodRow { Pod = , State = -1 }")]
    [InlineData((PodState)int.MaxValue, "PodRow { Pod = , State = 2147483647 }")]
    public void ARowOfNoPodSaysNothingForThePodAndItsStateWhole(PodState state, string words) =>
        Assert.Equal(words, Words(new PodRow(null!, state)));

    /// <summary>The row of no pod falls apart into nothing and a state, and a copy of it is that row again: the
    /// <c>with</c> that names no part, the <c>with</c> that turns the state and the <c>with</c> that hands the empty
    /// part over again all keep the pod absent, and the <c>with</c> that gives the row a pod at last makes an ordinary
    /// row - equal to the row built the other way round, from the pod and the state together. The absence is a value
    /// the record carries, not a hole it repairs.</summary>
    [Fact]
    public void ARowOfNoPodFallsApartAndIsCopiedWithNothingForAPod()
    {
        var row = new PodRow(null!, PodState.Unhealthy);

        var (pod, state) = row;

        Assert.Null(pod);
        Assert.Equal(PodState.Unhealthy, state);

        var copy = row with { };
        Assert.Equal(row, copy);
        Assert.NotSame(row, copy);
        Assert.Null(copy.Pod);
        Assert.Equal(PodState.Unhealthy, copy.State);

        var turned = row with { State = PodState.Finished };
        Assert.NotEqual(row, turned);
        Assert.Null(turned.Pod);
        Assert.Equal("PodRow { Pod = , State = Finished }", Words(turned));

        var emptied = row with { Pod = null! };
        Assert.Equal(row, emptied);
        Assert.True(row == emptied);
        Assert.False(row != emptied);

        var filled = turned with { Pod = Silent("ui-0") };
        Assert.Equal(new PodRow(Silent("ui-0"), PodState.Finished), filled);
        Assert.NotEqual(row, filled);
        Assert.NotNull(filled.Pod);
    }

    // ------------------------------------------------------------------- the row among other rows

    /// <summary>The row's equality and hash code as the collections of a page use them: a row twice over is one row in
    /// a set and one entry in a lookup, however the two were built - the pod being equal, not the same instance - and
    /// the four states of one pod are four rows, a different pod being a fifth. A row of no pod is one row among
    /// itself and apart from every row that has one. Nothing here is a fact about hashing alone: a set puts two rows
    /// together only when the hash agrees <em>and</em> the equality does, which is what <c>Distinct</c> of a table's
    /// rows and a dictionary keyed by a row both lean on.</summary>
    [Fact]
    public void RowsThatAreOneAreOneRowToASetAndToALookup()
    {
        var ui = ClusterFixture.Pod("ui");
        var ready = new PodRow(ui, PodState.Ready);
        var twin = new PodRow(ClusterFixture.Pod("ui"), PodState.Ready);
        var broken = new PodRow(ui, PodState.Unhealthy);
        var other = new PodRow(ClusterFixture.Pod("api"), PodState.Ready);

        var set = new HashSet<PodRow> { ready, twin, broken, other };

        Assert.Equal(3, set.Count);
        Assert.Contains(new PodRow(ClusterFixture.Pod("ui"), PodState.Ready), set);
        Assert.Contains(new PodRow(ui with { }, PodState.Ready), set);
        Assert.DoesNotContain(new PodRow(ui, PodState.Starting), set);
        Assert.DoesNotContain(new PodRow(ui with { Restarts = 9 }, PodState.Ready), set);
        Assert.Contains(twin, set);

        PodRow[] table = [ready, twin, ready, broken, other];

        Assert.Equal(3, table.Distinct().Count());
        Assert.Equal(4, table.Append(new PodRow(ui, PodState.Finished)).Distinct().Count());
        // The rows that are left stand in the order the table gave them, the first of a row being the one kept.
        Assert.Equal(["ui-0", "ui-0", "api-0"], [.. table.Distinct().Select(row => row.Pod.Name)]);
        Assert.Equal(["api-0", "ui-0", "ui-0"], [.. table.Distinct().Select(row => row.Pod.Name).Order(StringComparer.Ordinal)]);

        var looked = new Dictionary<PodRow, string> { [ready] = "the row of ui-0", [broken] = "the row that worries" };

        Assert.Equal(2, looked.Count);
        Assert.Equal("the row of ui-0", looked[twin]);
        Assert.Equal("the row that worries", looked[new PodRow(ui, PodState.Unhealthy)]);
        Assert.False(looked.ContainsKey(other));

        var none = new PodRow(null!, PodState.Ready);

        Assert.Single(new HashSet<PodRow> { none, new(null!, PodState.Ready) });
        Assert.Equal(2, new HashSet<PodRow> { none, ready }.Count);
        Assert.Equal(2, new HashSet<PodRow> { none, new(null!, PodState.Finished) }.Count);
    }

    /// <summary>A row's hash code is its own, at every asking and from every angle: the same row asked twice, the
    /// boxed row and the row itself, and a row built again from the same pod and the same state - each of the four
    /// states, a state the enum does not name, both ends of the count, and the row of no pod. The hash is the promise
    /// the set above keeps its side of, and it is never a number the page prints.</summary>
    [Fact]
    public void EveryRowHashesTheSameAtEveryAskingAndWhereItIsOne()
    {
        var ui = ClusterFixture.Pod("ui");
        PodState[] every = [.. States, (PodState)4, (PodState)(-1), (PodState)int.MinValue, (PodState)int.MaxValue];

        foreach (var state in every)
        {
            var row = new PodRow(ui, state);
            var hash = row.GetHashCode();

            Assert.Equal(hash, row.GetHashCode());
            Assert.Equal(hash, ((object)row).GetHashCode());
            Assert.Equal(hash, new PodRow(ui, state).GetHashCode());
            Assert.Equal(hash, (row with { Pod = ui }).GetHashCode());
            Assert.Equal(hash, row.GetHashCode());
        }

        var none = new PodRow(null!, PodState.Ready);

        Assert.Equal(none.GetHashCode(), new PodRow(null!, PodState.Ready).GetHashCode());
        Assert.Equal(none.GetHashCode(), ((object)none).GetHashCode());
    }

    // --------------------------------------------------------------------- the row's words again

    /// <summary>The row says a pod's name as the pod says it, with nothing escaped, cut or quoted: a name of commas
    /// and an equals sign, a name of braces, a name of a line break, the empty name, a name of spaces, a name with an
    /// accent and a name of nothing but a quote are all of them the name between <c>Name = </c> and the first comma,
    /// and the label the row says last is that same name - the pod having no workload to be called by. The line a
    /// failing test prints is therefore one line only while the pod's own words are: a name that carries a break
    /// carries it into the row's words untouched, which is what makes those words a fact to read and not a format to
    /// parse.</summary>
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("ui-0")]
    [InlineData("ui, api = 0")]
    [InlineData("{ Pod = }")]
    [InlineData("a\nb")]
    [InlineData("\"ui\"")]
    [InlineData("uber-0")]
    public void TheRowSaysThePodsNameAsItStands(string name)
    {
        var row = new PodRow(Silent(name), PodState.Starting);

        Assert.Equal(name, row.Pod.Name);
        Assert.Equal(name, row.Pod.Label);
        Assert.Equal(PodState.Starting, row.State);
        Assert.Equal(Said(name, PodState.Starting), Words(row));
    }

    /// <summary>A name of a thousand characters, and a name of the workload that owns the pod: the row cuts nothing
    /// and prefers the workload where there is one, so the words it says are the pod's words whole - the same
    /// <c>Label</c> the table's order is built on, standing last among the pod's parts and the pod's own name among
    /// them both.</summary>
    [Fact]
    public void TheRowSaysALongNameWholeAndTheWorkloadRatherThanTheName()
    {
        var longName = new string('x', 1_000);
        var row = new PodRow(Silent(longName), PodState.Starting);

        Assert.Equal(Said(longName, PodState.Starting), Words(row));
        Assert.Equal(1_000, row.Pod.Name.Length);

        var owned = new PodRow(ClusterFixture.Pod("ui", name: "ui-7f9c-abcde"), PodState.Ready);

        Assert.Equal("ui-7f9c-abcde", owned.Pod.Name);
        Assert.Equal("ui", owned.Pod.Label);
        Assert.Contains("Name = ui-7f9c-abcde, Workload = ui, ", Words(owned), StringComparison.Ordinal);
        Assert.EndsWith("Label = ui }, State = Ready }", Words(owned), StringComparison.Ordinal);
    }

    /// <summary>The row's words follow whoever reads them - but only where the pod's numbers are said. A reader whose
    /// culture writes a decimal point as a comma is shown <c>Usage = 10,5</c> by the same row that shows
    /// <c>Usage = 10.5</c> to the invariant one, for the row says the pod by the pod's own words and puts nothing
    /// between them. The names of the two parts, the state's word and the row's brackets are the page's vocabulary and
    /// stand as they stand in either culture - which is why <see cref="PodRowEdgeCaseTests" /> may pin the whole line
    /// for the numbers the file writes, all of them whole.</summary>
    [Fact]
    public void TheRowsWordsFollowTheReaderForThePodsNumbersAlone()
    {
        var row = new PodRow(
            ClusterFixture.Pod("ui") with { Cpu = new ResourceUse(10.5, 100, 500), Restarts = 3 },
            PodState.Unhealthy);

        Assert.Equal(10.5, row.Pod.Cpu.Usage);

        var reader = Thread.CurrentThread.CurrentCulture;
        try
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Assert.Contains("Cpu = ResourceUse { Usage = 10.5, Requests = 100, Limits = 500, Allocatable =  }", row.ToString(), StringComparison.Ordinal);
            Assert.Contains("Usage = 104857600", row.ToString(), StringComparison.Ordinal);

            Thread.CurrentThread.CurrentCulture = new CultureInfo("de-DE");
            Assert.Contains("Usage = 10,5", row.ToString(), StringComparison.Ordinal);

            // The parts' names, the state's word and the row's own brackets: the same to both readers.
            Assert.StartsWith("PodRow { Pod = ClusterPod { Name = ui-0, ", row.ToString(), StringComparison.Ordinal);
            Assert.Contains(", Label = ui }, State = Unhealthy }", row.ToString(), StringComparison.Ordinal);
            Assert.Contains("Restarts = 3, ", row.ToString(), StringComparison.Ordinal);
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = reader;
        }

        // And the same row read back in the invariant culture says what it said first: the reader changes the words
        // and not the row.
        Assert.Equal(Words(row), Words(row with { }));
    }

    // ------------------------------------------------------------------------------ helpers

    /// <summary>A pod that says as little as the record allows - no workload, no kind, no node, no containers, no
    /// reason, no start and no measurement - so that the row's words stand on the one part the test names and on
    /// nothing the file could have filled in differently.</summary>
    private static ClusterPod Silent(string name) =>
        new(name, null, null, null, ClusterPod.Running, false, null, null, 0, null, null, ResourceUse.None, ResourceUse.None);

    /// <summary>The words a pod of nothing but a name says itself in, with the state the row carries after them - the
    /// line the row is expected to say, written out once rather than once per case.</summary>
    private static string Said(string name, PodState state) =>
        "PodRow { Pod = ClusterPod { Name = " + name +
        ", Workload = , Kind = , Node = , Phase = Running, Ready = False, Containers = , ContainersReady = , " +
        "Restarts = 0, Reason = , StartedAt = , Cpu = ResourceUse { Usage = , Requests = , Limits = , Allocatable =  }, " +
        "Memory = ResourceUse { Usage = , Requests = , Limits = , Allocatable =  }, Label = " + name +
        " }, State = " + state + " }";

    /// <summary>The words a row says itself in the invariant culture whatever the reader of this test happens to be:
    /// the record's words follow whoever reads them, which is pinned on its own further up this file, so the strings
    /// below are the point and the culture only the way to say them once.</summary>
    private static string Words(PodRow row)
    {
        ArgumentNullException.ThrowIfNull(row);

        var reader = Thread.CurrentThread.CurrentCulture;
        try
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            return row.ToString();
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = reader;
        }
    }

    /// <summary>The public members the row declares itself: its two properties and the methods that are not the get
    /// and the set of one - the two operators kept although the compiler names them specially, since they are the
    /// <c>==</c> and the <c>!=</c> a caller writes - with the record's compiler-named clone walked past. The
    /// constructor is no method, so it is named by the test above rather than by this list.</summary>
    private static List<string> PublicSurface() =>
        [.. typeof(PodRow)
            .GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(member => member is PropertyInfo
                             || member is MethodInfo method
                             && (!method.IsSpecialName || method.Name.StartsWith("op_", StringComparison.Ordinal)))
            .Where(member => !member.Name.Contains('<', StringComparison.Ordinal))
            .Select(member => member.Name)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];
}
