using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="RuntimeIndexResult" />: the record
/// <see cref="RuntimeLoader.LoadIndexAsync" /> hands over with the <see cref="RuntimeIndex" /> it read and the reasons
/// it could not, and of the <see cref="RuntimeAvailability" /> it stands on, declared in
/// <c>src/Dashboard/Runtime/RuntimeLoader.cs</c> - whether the runtime is served, is not deployed, or failed to be read.
/// The record decides nothing of its own: no words for the page, no moment, no address. What is pinned here is that
/// each part comes back as the very instance it went in as, that the record refuses nothing it is handed, and that
/// the members every record is asked compare, hash, fall apart, copy and say the three parts.
/// <para>
/// What the other files already pin is left alone. <see cref="RuntimeLoaderTests" /> pins the four outcomes of
/// <see cref="RuntimeLoader.LoadIndexAsync" /> over HTTP: the <see cref="RuntimeIndex" /> the deployment with diagrams
/// serves as <see cref="RuntimeAvailability.Available" />, the <see cref="RuntimeAvailability.NotDeployed" /> of a site with no
/// runtime, the <see cref="RuntimeAvailability.NotDeployed" /> of a host that serves the page for every address, and the
/// <see cref="RuntimeAvailability.Failed" /> with the reasons when the file or the JSON cannot be read.
/// <see cref="RuntimeIndexFrameAndParseResultTests" /> pins the <see cref="RuntimeIndex" /> and the
/// <see cref="RuntimeFrame"/>s of a <see cref="RuntimeManifest"/> as values and the <see cref="RuntimeParseResult{T}"/> a read
/// hands over. <see cref="RuntimeIndexEntryEdgeCaseTests" /> pins the <see cref="RuntimeIndexEntry" /> the index holds - the
/// environment and its name, the diagram the caller names and the database the caller does not, and the members every
/// record is asked compare, hash, fall apart and say.
/// </para>
/// <para>
/// What follows is the record on its own, at the ends the tests above leave open: the three parts by the names of their
/// own properties, each held as the very instance it was given and none of them refused - the <see cref="RuntimeIndex" />
/// that is not there, the <see langword="null" /> reasons an availability stands with and the result that is never the
/// same as nothing. The record counted by its three parts - the same three standing one in a set, the copy a <c>with</c>
/// makes and the one part it turns, the <c>==</c> and the <c>!=</c> a caller writes, the three parts read out in the
/// order they are declared and the line the record says about them. The shapes a caller can survive - that every shape
/// the loader builds has the index to draw, answers <see cref="RuntimeAvailability.NotDeployed" /> for itself, or has
/// at least one message to list, and that an index of no environments is an answer and not a failure. The cached <see cref="RuntimeIndexResult.NotDeployed" /> - one instance the loader keeps, and not one built
/// per ask. And the record's and the enum's whole public surface, so that the day a fourth availability or a fourth
/// part is added the lists below say so.
/// </para>
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: no loader is built, no
/// <c>runtime/index.json</c> is fetched, and every result below is one the test brought - the few reads of
/// <see cref="RuntimeManifestParser.ParseIndex" /> included, which are of words the test wrote.
/// </para>
/// </summary>
public class RuntimeIndexResultEdgeCaseTests
{
    private const string PlantUmlVersion = "1.2026.01.01";

    /// <summary>The reason the loader says when the file is there and could not be read - the one word the page lists
    /// under "The runtime diagrams could not be read".</summary>
    private const string Reason = "The file could not be read.";

    /// <summary>The one reason, held once: every result below that stands with one reason stands with this list.</summary>
    private static readonly string[] FileCouldNotBeRead = [Reason];

    /// <summary>The line a result says of itself when the index and the reasons are there: the environment list named
    /// and never walked, and the reasons named as the array they are - the same words serving one reason and none.</summary>
    private const string AvailableWords =
        "RuntimeIndexResult { Availability = Available, Index = RuntimeIndex { Environments = Dashboard.Runtime.RuntimeIndexEntry[], PlantUml = 1.2026.01.01 }, Errors = System.String[] }";

    /// <summary>The line the cached <see cref="RuntimeIndexResult.NotDeployed" /> says of itself: both parts named, the
    /// index blank, the reasons named as the array they are.</summary>
    private const string NotDeployedWords = "RuntimeIndexResult { Availability = NotDeployed, Index = , Errors = System.String[] }";

    /// <summary>The one environment of the index the tests below hand over.</summary>
    private static readonly RuntimeIndexEntry[] OneEnvironment = [new RuntimeIndexEntry("tdd", "diagram", null!, null)];

    /// <summary>An index of no environments: the answer of a deployment whose <c>system.json</c> names none.</summary>
    private static readonly RuntimeIndex NoEnvironmentIndex = new([], PlantUmlVersion);

    /// <summary>The deployment rendered its diagrams: the index the loader read, and no reason at all.</summary>
    private static RuntimeIndexResult Available => new(RuntimeAvailability.Available, new RuntimeIndex(OneEnvironment, PlantUmlVersion), []);

    /// <summary>The site has no <c>runtime/</c>: no index and no reason. Built here, so that the instance the loader
    /// keeps stays the one <see cref="RuntimeIndexResult.NotDeployed" /> the test of the cache names itself.</summary>
    private static RuntimeIndexResult NotDeployed => new(RuntimeAvailability.NotDeployed, null, []);

    /// <summary>The file is there and could not be read: no index, and the one reason the page lists.</summary>
    private static RuntimeIndexResult FailedOne => new(RuntimeAvailability.Failed, null, FileCouldNotBeRead);

    /// <summary>A read that had two things to say at once: still no index, and both reasons standing.</summary>
    private static RuntimeIndexResult FailedMany => new(
        RuntimeAvailability.Failed,
        null,
        [Reason, "runtime/index.json: environments[0].name: missing or empty."]);

    /// <summary>The shape the record allows and the loader never builds: a <see cref="RuntimeAvailability.Failed" /> of
    /// no index and no reason either - what the page would open as an alert with nothing in it.</summary>
    private static RuntimeIndexResult EmptyErrors => new(RuntimeAvailability.Failed, null, []);

    /// <summary>An answer of no environments: the index is there, it holds none, and no reason stands with it.</summary>
    private static RuntimeIndexResult NoEnvironments => new(RuntimeAvailability.Available, NoEnvironmentIndex, []);

    /// <summary>The result of nothing at all: no index and no list of reasons either.</summary>
    private static RuntimeIndexResult Nothing => new(RuntimeAvailability.Failed, null, null!);

    // ----------------------------------------------- the three parts, as the names they are

    /// <summary>The one constructor a caller may use hands each part to the property of its own name: the
    /// <see cref="RuntimeIndex" /> the caller named, the list of reasons the caller named and the availability the
    /// caller named, so that a result is read back the way it was written and never as another of the three.</summary>
    [Fact]
    public void TheConstructorHandsEachPartToThePropertyOfItsName()
    {
        var index = NoEnvironmentIndex;
        var reasons = new List<string> { Reason };

        var result = new RuntimeIndexResult(RuntimeAvailability.Available, index, reasons);

        Assert.Same(index, result.Index);
        Assert.Same(reasons, result.Errors);
        Assert.Equal(RuntimeAvailability.Available, result.Availability);
    }

    /// <summary>The record refuses nothing it is given: the index that is not there, the reasons that are not, the
    /// empty list of reasons, the word that is nothing but spaces and the empty word itself - every part is held as
    /// the very instance it was handed and read back the way it stands, and the reference it was handed is the
    /// reference it keeps.</summary>
    [Fact]
    public void TheRecordRefusesNothingItIsGiven()
    {
        Assert.Null(Nothing.Index);
        Assert.Null(Nothing.Errors);

        Assert.Empty(NotDeployed.Errors);

        var words = new List<string> { "  ", string.Empty };
        var padded = new RuntimeIndexResult(RuntimeAvailability.Failed, null, words);

        Assert.Same(words, padded.Errors);
        Assert.Equal("  ", padded.Errors[0]);
        Assert.Equal(string.Empty, padded.Errors[1]);
    }

    /// <summary>Nothing is no result: the <c>==</c> and the <c>!=</c> a caller writes both answer that a result is
    /// something and nothing is not, from either side of the operator, and the <c>Equals</c> asked of a result answers
    /// false to nothing - as the object it is and as the result it is not.</summary>
    [Fact]
    public void AResultIsNeverTheSameAsNothingAndNeverEqualToIt()
    {
        var result = RuntimeIndexResult.NotDeployed;

        Assert.True(result != null);
        Assert.False(result == null);
        Assert.False(null == result);
        Assert.True(null != result);
        Assert.False(result.Equals((object?)null));
        Assert.False(result.Equals((RuntimeIndexResult?)null));
    }

    /// <summary>The hash a result says is the hash it says again: asked twice of the result whose parts are all there
    /// and twice of the result of nothing at all, so that the record neither answers nothing with a word of its own
    /// nor changes its mind between the two asks of one result.</summary>
    [Fact]
    public void TheHashItSaysIsTheHashItSaysAgain()
    {
        var served = Available;

        Assert.Equal(served.GetHashCode(), served.GetHashCode());
        Assert.Equal(Nothing.GetHashCode(), Nothing.GetHashCode());
    }

    // ------------------------------------------------------- the record counted by its parts

    /// <summary>Two results of the same three parts are one result: equal, one hash, one in a set, and the <c>==</c>
    /// and the <c>!=</c> a caller writes say the same. The parts are the references the result was handed, so the
    /// equality of two results is the equality of the three instances standing in them.</summary>
    [Fact]
    public void TwoResultsOfTheSameThreePartsAreOneResult()
    {
        var index = NoEnvironmentIndex;
        var reasons = new List<string> { Reason };

        var result = new RuntimeIndexResult(RuntimeAvailability.Failed, index, reasons);
        var again = new RuntimeIndexResult(RuntimeAvailability.Failed, index, reasons);

        Assert.Equal(result, again);
        Assert.True(result == again);
        Assert.Equal(result.GetHashCode(), again.GetHashCode());
        Assert.Single(new HashSet<RuntimeIndexResult> { result, again });
        Assert.True(result.Equals((object)again));
    }

    /// <summary>A part of another kind is no result's equal: the <c>Equals</c> asked of an object answers false to
    /// the word an availability says, to a number, to the index a result holds and to a thing of no kind at all -
    /// the result counts only the three parts of its own kind, and no word, number or record of another kind is
    /// ever one of them.</summary>
    [Fact]
    public void AResultIsNeverEqualToAnythingOfAnotherKind()
    {
        var index = NoEnvironmentIndex;
        var result = new RuntimeIndexResult(RuntimeAvailability.Failed, index, []);

        Assert.False(result.Equals("Failed"));
        Assert.False(result.Equals(RuntimeAvailability.Failed));
        Assert.False(result.Equals(index));
        Assert.False(result.Equals(0));
        Assert.False(result.Equals(new object()));
    }

    /// <summary>A result differs in any one of its three parts: in the availability, in the index it holds, in the
    /// reference of the list of reasons it holds - the list counted by the very list it holds, and not by the words
    /// in it - and in the <c>null</c> that is not the empty list.</summary>
    [Fact]
    public void AResultDiffersInAnyOneOfItsThreeParts()
    {
        var index = NoEnvironmentIndex;
        var reasons = new List<string> { Reason };
        var result = new RuntimeIndexResult(RuntimeAvailability.Failed, index, reasons);

        Assert.NotEqual(result, new RuntimeIndexResult(RuntimeAvailability.Available, index, reasons));
        Assert.NotEqual(result, new RuntimeIndexResult(RuntimeAvailability.NotDeployed, index, reasons));
        Assert.NotEqual(result, new RuntimeIndexResult(RuntimeAvailability.Failed, new RuntimeIndex([], "1.2026.01.02"), reasons));
        Assert.NotEqual(result, new RuntimeIndexResult(RuntimeAvailability.Failed, null, reasons));
        Assert.NotEqual(result, new RuntimeIndexResult(RuntimeAvailability.Failed, index, null!));
        Assert.NotEqual(result, new RuntimeIndexResult(RuntimeAvailability.Failed, index, new List<string> { Reason }));

        Assert.True(result != new RuntimeIndexResult(RuntimeAvailability.Available, index, reasons));
    }

    /// <summary>The list a result holds is the very list it was given, and the index it holds is the very index: two
    /// results of two lists with the same words are two results, the two built of one list are one, and the index
    /// that is equal by value is the same part of the two results even though it is not the same instance.</summary>
    [Fact]
    public void TheResultCountsItsPartsByTheReferencesItHolds()
    {
        var environments = new RuntimeIndexEntry[] { new RuntimeIndexEntry("tdd", "diagram", null!, null) };
        var index = new RuntimeIndex(environments, PlantUmlVersion);

        var first = new RuntimeIndexResult(RuntimeAvailability.Available, index, new List<string> { Reason });
        var second = new RuntimeIndexResult(RuntimeAvailability.Available, index, new List<string> { Reason });

        Assert.NotSame(first.Errors, second.Errors);
        Assert.NotEqual(first, second);
        Assert.True(first != second);

        var shared = new List<string> { Reason };
        Assert.Equal(new RuntimeIndexResult(RuntimeAvailability.Available, index, shared),
                    new RuntimeIndexResult(RuntimeAvailability.Available, index, shared));

        var indexAgain = new RuntimeIndex(environments, PlantUmlVersion);

        Assert.NotSame(index, indexAgain);
        Assert.Equal(index, indexAgain);

        var reasons = new List<string>();
        Assert.Equal(new RuntimeIndexResult(RuntimeAvailability.Available, index, reasons),
                    new RuntimeIndexResult(RuntimeAvailability.Available, indexAgain, reasons));
    }

    /// <summary>The <c>with</c> a caller writes copies the result and turns only the part it names: the copy keeps the
    /// references of the parts it does not turn, the availability the caller names stands in the copy alone, and the
    /// part turned is the reference the caller names - the empty list that is a list and the index that is not.</summary>
    [Fact]
    public void AWithCopiesTheResultAndTurnsOnlyThePartItNames()
    {
        var index = NoEnvironmentIndex;
        var reasons = new List<string> { Reason };
        var result = new RuntimeIndexResult(RuntimeAvailability.Failed, index, reasons);

        var copy = result with { };

        Assert.Equal(copy, result);
        Assert.Same(index, copy.Index);
        Assert.Same(reasons, copy.Errors);

        var served = result with { Availability = RuntimeAvailability.Available };

        Assert.Equal(RuntimeAvailability.Available, served.Availability);
        Assert.Same(index, served.Index);
        Assert.Same(reasons, served.Errors);
        Assert.NotEqual(served, result);

        var withoutReasons = result with { Errors = [] };

        Assert.Empty(withoutReasons.Errors);
        Assert.NotSame(withoutReasons.Errors, reasons);
        Assert.Same(index, withoutReasons.Index);

        var withoutIndex = result with { Index = null };

        Assert.Null(withoutIndex.Index);
        Assert.Equal(RuntimeAvailability.Failed, withoutIndex.Availability);
        Assert.Same(reasons, withoutIndex.Errors);
    }

    /// <summary>The three parts fall out of the result in the order they are declared - the availability, the index
    /// and the list of reasons, each as the very reference the result holds - and the parts read out are the parts of
    /// the one result they came out of, the parts that are not standing out as nothing: the cached
    /// <see cref="RuntimeIndexResult.NotDeployed" /> falls apart into the empty list it holds, and the result of
    /// nothing at all into the <see langword="null" /> it was given.</summary>
    [Fact]
    public void TheThreePartsFallOutOfTheResultInTheirOrder()
    {
        var index = NoEnvironmentIndex;
        var reasons = new List<string> { Reason };
        var result = new RuntimeIndexResult(RuntimeAvailability.Failed, index, reasons);

        var (availability, readIndex, readReasons) = result;

        Assert.Equal(RuntimeAvailability.Failed, availability);
        Assert.Same(index, readIndex);
        Assert.Same(reasons, readReasons);

        var (notAvailability, notIndex, notReasons) = RuntimeIndexResult.NotDeployed;

        Assert.Equal(RuntimeAvailability.NotDeployed, notAvailability);
        Assert.Null(notIndex);
        Assert.Empty(notReasons);

        var (failedAvailability, nothingIndex, nothingReasons) = Nothing;

        Assert.Equal(RuntimeAvailability.Failed, failedAvailability);
        Assert.Null(nothingIndex);
        Assert.Null(nothingReasons);

        Assert.Equal(result, new RuntimeIndexResult(availability, readIndex, readReasons));
    }

    // --------------------------------------------------- what the page can show

    /// <summary>The page reads either the index or the messages and never both: <c>RuntimeView.razor</c> lets
    /// <see cref="RuntimeAvailability.NotDeployed" /> answer for itself, shows the index when there is one, and only
    /// then opens the alert that lists the reasons. So every shape <see cref="RuntimeLoader.LoadIndexAsync" /> builds -
    /// the index it read (line 61), the <see cref="RuntimeIndexResult.NotDeployed" /> it keeps (line 51), the
    /// <see cref="RuntimeAvailability.Failed" /> of the one reason a file could not be read (line 56) and the
    /// <see cref="RuntimeAvailability.Failed" /> of the reasons a read of an index collected (line 62) - has the index
    /// to draw, or answers <see cref="RuntimeAvailability.NotDeployed" /> which the page says for itself, or has at
    /// least one message to list; no <see cref="RuntimeAvailability.Failed" /> of the loader stands without one, since
    /// a failure with nothing to say would reach the page as an alert with an empty list under it. The first loop walks
    /// those shapes, the second the two lines of the loader over every kind of answer the reader hands back.</summary>
    [Fact]
    public void ThePageReadsTheIndexOrTheMessagesNeverBoth()
    {
        foreach (var result in new[] { Available, NoEnvironments, NotDeployed, FailedOne, FailedMany })
        {
            // RuntimeView.razor: NotDeployed answers for itself, an index is drawn, and only a read with neither asks
            // for the messages - so what reaches the alert is a list with at least one reason in it.
            var messages = result.Availability == RuntimeAvailability.NotDeployed || result.Index is not null ? null : result.Errors;

            Assert.True(messages is null || messages.Count > 0, $"A failure with nothing to show: {result}");
            Assert.True(result.Index is null || result.Errors.Count == 0, $"A reason the page would never show: {result}");
            Assert.True(result.Availability != RuntimeAvailability.Failed || result.Errors.Count > 0, $"A failure the page cannot explain: {result}");
        }

        // The record itself refuses nothing: a Failed of no index and no reason is one it will hold, and nothing but
        // the loader's own three constructions keeps that shape away from the page.
        Assert.Null(EmptyErrors.Index);
        Assert.Empty(EmptyErrors.Errors);

        foreach (var file in new string?[]
        {
            """{ "environments": [ { "name": "tdd" } ], "plantuml": "1.2026.01.01" }""",
            """{ "environments": [ ] }""",
            """{ "environments": [ { } ] }""",
            """{ }""",
            null,
            "<html>",
            "[]",
            "42",
        })
        {
            var parsed = RuntimeManifestParser.ParseIndex(file);

            // RuntimeLoader.cs:61-62, the two shapes a read of the index lands on.
            var result = parsed.Value is { } index
                ? new RuntimeIndexResult(RuntimeAvailability.Available, index, [])
                : new RuntimeIndexResult(RuntimeAvailability.Failed, null, [.. parsed.Errors.Select(problem => $"{RuntimeLoader.Folder}{RuntimeLoader.IndexFile}: {problem}")]);

            Assert.True(result.Index is not null || result.Errors.Count > 0, $"A read with nothing to show: {file}");
        }
    }

    /// <summary>The boundary the page reads last: an index that rendered no environment is an answer and not a failure.
    /// The availability stands <see cref="RuntimeAvailability.Available" />, the index is there, it holds none and no
    /// reason stands with it - so the page says that the deployment rendered no runtime diagram
    /// (<c>RuntimeView.razor</c>) instead of opening the alert of a read that could not be done. An index of no
    /// environments and no index at all are two answers, and only the second of them leaves the page with the
    /// messages.</summary>
    [Fact]
    public void AnIndexOfNoEnvironmentsIsAnAnswerAndNotAFailure()
    {
        var rendered = NoEnvironments;

        Assert.Equal(RuntimeAvailability.Available, rendered.Availability);
        Assert.NotNull(rendered.Index);
        Assert.Empty(rendered.Index!.Environments);
        Assert.Empty(rendered.Errors);

        // The reader hands it back the same way: an index of no environments is a value, and no reason comes with it.
        var read = RuntimeManifestParser.ParseIndex("""{ "environments": [ ] }""");

        Assert.True(read.IsValid);
        Assert.Empty(read.Value!.Environments);
        Assert.Empty(read.Errors);

        // The other answer of nothing: no index at all, and the page is left with the messages - another shape.
        Assert.NotEqual(rendered, FailedOne);
        Assert.NotEqual(rendered, EmptyErrors);
        Assert.NotEqual(rendered, NotDeployed);
    }

    // ---------------------------------------------------------------- words

    /// <summary>The words a result says about itself - its name, then its three parts in the order they are declared,
    /// each named and saying its part's own words: the availability's word, the index's own line or nothing, and the
    /// list's own words or nothing - are the record's and not the page's. The index that is not there and the index of
    /// no environments are two different lines, and the empty list says the words of the array it is.</summary>
    [Fact]
    public void TheResultSaysItsThreePartsInTheirOrder()
    {
        Assert.Equal(
            AvailableWords,
            new RuntimeIndexResult(RuntimeAvailability.Available, new RuntimeIndex(OneEnvironment, PlantUmlVersion), FileCouldNotBeRead).ToString());

        Assert.Equal(
            NotDeployedWords,
            RuntimeIndexResult.NotDeployed.ToString());

        Assert.Equal(
            "RuntimeIndexResult { Availability = Failed, Index = , Errors = System.String[] }",
            new RuntimeIndexResult(RuntimeAvailability.Failed, null, Array.Empty<string>()).ToString());

        // The reasons that are not a list at all leave the name of the part and no word after it.
        Assert.Equal(
            "RuntimeIndexResult { Availability = Failed, Index = , Errors =  }",
            Nothing.ToString());

        Assert.Equal(
            "RuntimeIndexResult { Availability = Available, Index = RuntimeIndex { Environments = , PlantUml = 1.2026.01.01 }, Errors = System.String[] }",
            new RuntimeIndexResult(RuntimeAvailability.Available, new RuntimeIndex(null!, PlantUmlVersion), []).ToString());
        Assert.Equal(
            AvailableWords,
            new RuntimeIndexResult(RuntimeAvailability.Available, NoEnvironmentIndex, []).ToString());
    }

    /// <summary>A result's words are nothing but words, so no reader's culture turns them: a viewer of German is told
    /// the same result as the invariant reader - the same availability, the same index and the same list of reasons,
    /// which is what a log of one of these results may count on whatever the reader of the machine happens to be.</summary>
    [Fact]
    public void TheResultSaysTheSameThingWhateverTheReaderSpeaks()
    {
        ViewerCulture.German(() =>
        {
            Assert.Equal(
                AvailableWords,
                new RuntimeIndexResult(RuntimeAvailability.Available, new RuntimeIndex(OneEnvironment, PlantUmlVersion), FileCouldNotBeRead).ToString());
            Assert.Equal(
                NotDeployedWords,
                RuntimeIndexResult.NotDeployed.ToString());
        });
    }

    // --------------------------------------------------------------- the cached value

    /// <summary>The <see cref="RuntimeIndexResult.NotDeployed" /> the loader keeps is one instance and not one built per
    /// ask: it stands on the availability <see cref="RuntimeAvailability.NotDeployed" />, the index that is not there
    /// and the empty list of reasons, and the result built the same way is equal to it - one value, not the same
    /// instance - holding the very list the cache holds. A <c>with</c> over the cached value turns only the part it
    /// names, so the copy of it holds that same list too.</summary>
    [Fact]
    public void TheCachedNotDeployedIsOneInstanceAndItsParts()
    {
        Assert.Same(RuntimeIndexResult.NotDeployed, RuntimeIndexResult.NotDeployed);

        Assert.Equal(RuntimeAvailability.NotDeployed, RuntimeIndexResult.NotDeployed.Availability);
        Assert.Null(RuntimeIndexResult.NotDeployed.Index);
        Assert.Empty(RuntimeIndexResult.NotDeployed.Errors);

        var built = new RuntimeIndexResult(RuntimeAvailability.NotDeployed, null, []);

        Assert.NotSame(built, RuntimeIndexResult.NotDeployed);
        Assert.Equal(built, RuntimeIndexResult.NotDeployed);
        Assert.Same(built.Errors, RuntimeIndexResult.NotDeployed.Errors);
        Assert.Equal(built.GetHashCode(), RuntimeIndexResult.NotDeployed.GetHashCode());

        var served = RuntimeIndexResult.NotDeployed with { Availability = RuntimeAvailability.Available };

        Assert.Same(RuntimeIndexResult.NotDeployed.Errors, served.Errors);
        Assert.Null(served.Index);
        Assert.NotEqual(served, RuntimeIndexResult.NotDeployed);
    }

    // -------------------------------------------------------- the record's own shape

    /// <summary>The result's whole public surface as it stands: its three parts, named in the order the constructor
    /// names them and of the type the constructor says, the three a caller must name and none defaulted, the cached
    /// <see cref="RuntimeIndexResult.NotDeployed" />, and the members every record is asked - falling apart, comparing,
    /// hashing and saying itself - and nothing else to call. A fourth part added to the record has to be written down
    /// here as well, which is the point: a result cannot carry a part nobody decided what it means.</summary>
    [Fact]
    public void TheResultHasThreePartsAndTheMembersEveryRecordIsAsked()
    {
        Assert.True(typeof(RuntimeIndexResult).IsSealed);
        Assert.Same(typeof(object), typeof(RuntimeIndexResult).BaseType);

        Assert.Equal(
            ["Availability", "Index", "Errors"],
            [.. typeof(RuntimeIndexResult)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Select(property => property.Name)]);

        var surface = PublicSurface(typeof(RuntimeIndexResult));

        Assert.Equal(
            ["Availability", "Deconstruct", "Equals", "Errors", "GetHashCode", "Index", "NotDeployed", "ToString", "op_Equality", "op_Inequality"],
            surface);
        Assert.DoesNotContain("EqualityContract", surface, StringComparer.Ordinal);

        var properties = typeof(RuntimeIndexResult)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .ToDictionary(property => property.Name, StringComparer.Ordinal);

        Assert.Equal(typeof(RuntimeAvailability), properties["Availability"].PropertyType);
        Assert.Equal(typeof(RuntimeIndex), properties["Index"].PropertyType);
        Assert.Equal(typeof(IReadOnlyList<string>), properties["Errors"].PropertyType);

        // The one constructor a caller may use: one parameter per part, in the order the record names them, of the
        // type of the part, and none of them defaulted.
        var constructor = typeof(RuntimeIndexResult).GetConstructors().Single();
        Assert.Equal(["Availability", "Index", "Errors"], [.. constructor.GetParameters().Select(parameter => parameter.Name!)]);
        Assert.Equal(
            [typeof(RuntimeAvailability), typeof(RuntimeIndex), typeof(IReadOnlyList<string>)],
            [.. constructor.GetParameters().Select(parameter => parameter.ParameterType)]);
        Assert.All(constructor.GetParameters(), parameter => Assert.False(parameter.HasDefaultValue));

        // The copy constructor a <c>with</c> turns through is not a member a caller may call.
        var copyConstructor = typeof(RuntimeIndexResult).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance).Single();
        Assert.Equal(typeof(RuntimeIndexResult), copyConstructor.GetParameters().Single().ParameterType);

        // The parts a caller sets are init-only, the record is its own type and the parts are weighed as a record
        // weighs them: two Equals overloads, one interface, one contract hidden from the surface.
        Assert.All(properties.Values, property =>
        {
            Assert.NotNull(property.SetMethod);
            var modifiers = property.SetMethod!.ReturnParameter.GetRequiredCustomModifiers();
            Assert.Contains(typeof(System.Runtime.CompilerServices.IsExternalInit), modifiers);
        });
        Assert.Single(typeof(RuntimeIndexResult).GetInterfaces(), interfaceType => interfaceType == typeof(IEquatable<RuntimeIndexResult>));
        Assert.Equal(
            2,
            typeof(RuntimeIndexResult)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Count(method => method.Name == "Equals"));

        // The three parts fall out in their order, as the out parameters the record names them.
        var deconstruct = typeof(RuntimeIndexResult).GetMethod("Deconstruct")!;
        Assert.Equal(["Availability", "Index", "Errors"], [.. deconstruct.GetParameters().Select(parameter => parameter.Name!)]);
        Assert.All(deconstruct.GetParameters(), parameter =>
        {
            Assert.True(parameter.IsOut);
            Assert.Equal(typeof(void), deconstruct.ReturnType);
        });
    }

    // ------------------------------------- the three availabilities, as the numbers they are

    /// <summary>The three availabilities are the whole vocabulary the record stands on, in the order declared in the
    /// enum: <see cref="RuntimeAvailability.Available" />, <see cref="RuntimeAvailability.NotDeployed" /> and
    /// <see cref="RuntimeAvailability.Failed" /> - one value and one word each, standing on the numbers 0, 1 and 2 of
    /// the <see cref="int"/> the enum carries, no combination of the three, and nothing else to name.</summary>
    [Fact]
    public void TheThreeAvailabilitiesAreTheWholeVocabularyInTheirDeclaredOrder()
    {
        var availabilities = Enum.GetValues<RuntimeAvailability>();

        Assert.Equal(3, availabilities.Length);
        Assert.Equal(["Available", "NotDeployed", "Failed"], Enum.GetNames<RuntimeAvailability>());
        Assert.Equal([0, 1, 2], availabilities.Select(kind => (int)kind));
        Assert.Equal(typeof(int), Enum.GetUnderlyingType(typeof(RuntimeAvailability)));
        Assert.False(typeof(RuntimeAvailability).IsDefined(typeof(FlagsAttribute), false));

        var firstAsk = Enum.GetValues<RuntimeAvailability>();
        var secondAsk = Enum.GetValues<RuntimeAvailability>();

        Assert.NotSame(firstAsk, secondAsk);
        Assert.Equal(firstAsk, secondAsk);
    }

    /// <summary>Each availability stands on its own number and says its own word: the number the enum declares, the
    /// word the enum names and the word the value says - the three words of the record's one part, and nothing
    /// between them or outside them.</summary>
    [Theory]
    [InlineData(RuntimeAvailability.Available, 0, "Available")]
    [InlineData(RuntimeAvailability.NotDeployed, 1, "NotDeployed")]
    [InlineData(RuntimeAvailability.Failed, 2, "Failed")]
    public void EachAvailabilityStandsOnItsOwnNumberAndItsOwnWord(RuntimeAvailability availability, int number, string word)
    {
        Assert.Equal(number, (int)availability);
        Assert.Equal(availability, (RuntimeAvailability)number);
        Assert.Equal(word, availability.ToString());
        Assert.Equal(word, Enum.GetName(availability));
        Assert.True(Enum.IsDefined(availability));
    }

    /// <summary>The availability not set is <see cref="RuntimeAvailability.Available" />, the first of the three, and
    /// nothing outside the three is one: the numbers one above and one below the vocabulary, and the lowest and the
    /// highest of the <see cref="int"/>, are not availabilities - the enum names none of them, and the words they
    /// say are their numbers and not a word of the three.</summary>
    [Fact]
    public void AnUnsetAvailabilityIsAvailableAndNothingOutsideTheThreeIsOne()
    {
        Assert.Equal(RuntimeAvailability.Available, default);
        Assert.Equal(RuntimeAvailability.Available, (RuntimeAvailability)0);

        var oneAbove = (RuntimeAvailability)3;
        var oneBelow = (RuntimeAvailability)(-1);
        var lowest = (RuntimeAvailability)int.MinValue;
        var highest = (RuntimeAvailability)int.MaxValue;

        Assert.False(Enum.IsDefined(oneAbove));
        Assert.False(Enum.IsDefined(oneBelow));
        Assert.False(Enum.IsDefined(lowest));
        Assert.False(Enum.IsDefined(highest));

        Assert.Null(Enum.GetName(oneAbove));
        Assert.Null(Enum.GetName(oneBelow));

        Assert.Equal("3", oneAbove.ToString());
        Assert.Equal("-1", oneBelow.ToString());
        Assert.Equal("-2147483648", lowest.ToString());
        Assert.Equal("2147483647", highest.ToString());
    }

    /// <summary>The enum is the closed vocabulary of the record and nothing more: a sealed value type on
    /// <see cref="object"/>, no primitive, the three declared fields and nothing else public to stand on - so the
    /// day a fourth availability is declared, the fields the enum says say so, and the numbers and the words above
    /// must be written down again for it.</summary>
    [Fact]
    public void TheEnumIsTheClosedVocabularyOfTheRecordAndNothingMore()
    {
        Assert.True(typeof(RuntimeAvailability).IsEnum);
        Assert.True(typeof(RuntimeAvailability).IsValueType);
        Assert.True(typeof(RuntimeAvailability).IsSealed);
        Assert.False(typeof(RuntimeAvailability).IsPrimitive);
        Assert.Same(typeof(Enum), typeof(RuntimeAvailability).BaseType);

        var fields = typeof(RuntimeAvailability)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .OrderBy(field => field.Name, StringComparer.Ordinal);

        Assert.Equal(["Available", "Failed", "NotDeployed"], [.. fields.Select(field => field.Name)]);
        Assert.All(fields, field =>
        {
            Assert.True(field.IsLiteral);
            Assert.True(field.IsPublic);
            Assert.Equal(typeof(RuntimeAvailability), field.FieldType);
        });
    }

    // ------------------------------------------------------------------------------------------ the machinery

    /// <summary>The public members a type declares itself: its properties and its methods that are not a property's
    /// get or set, the two operators kept although the compiler names them specially since they are the <c>==</c> and
    /// the <c>!=</c> a caller writes, the record's compiler-named clone and its print walk past.</summary>
    private static List<string> PublicSurface(Type type) =>
        [.. type
            .GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(member => member is PropertyInfo
                             || member is MethodInfo method
                             && (!method.IsSpecialName || method.Name.StartsWith("op_", StringComparison.Ordinal)))
            .Where(member => !member.Name.Contains('<', StringComparison.Ordinal))
            .Select(member => member.Name)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];
}
