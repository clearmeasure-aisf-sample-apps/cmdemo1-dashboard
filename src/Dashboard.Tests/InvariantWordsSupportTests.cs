using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Dashboard.Tests;

/// <summary>
/// The one invariant helper every edge-case test reads a record's own words through: <see cref="CultureScope.Words" />.
/// More than thirty edge-case files keep a private <c>Words(record)</c> of their own, each a copy of the same save and
/// restore of the thread's culture around the record's <c>ToString()</c>.
/// <para>
/// Three things are pinned here. First, the CrapFacts words that <see cref="CrapFactsEdgeCaseTests" /> asserts under
/// whatever culture the runner starts in are said again through the shared helper, asked from a reader of German
/// whose numbers are written <c>28,5</c>, so the assertion holds on every machine. Second, every private
/// <c>Words</c> copy still standing is invoked under that same German reader and held to exactly what the shared
/// helper says, with the reader's culture left as it was found - a copy that drifts fails here. The number of copies
/// may only shrink, so the duplication cannot spread while the files retire their copies one at a time. Third,
/// <see cref="ViewerCulture.German" /> is the shared <see cref="CultureScope.Runs(string, Action)" /> of <c>de-DE</c>.
/// </para>
/// </summary>
public class InvariantWordsSupportTests
{
    private const BindingFlags Private = BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly;

    /// <summary>The copies standing when the shared helper was pinned against them; the count may only go down.</summary>
    private const int MostCopies = 36;

    private static readonly CultureInfo German = CultureInfo.GetCultureInfo("de-DE");

    // ----- the CrapFacts words, pinned in the invariant culture

    /// <summary>The words of <see cref="CrapFactsEdgeCaseTests.TheFactsKeepThePartsTheyAreGivenAndSayThemInTheirOrder" />
    /// and <see cref="CrapFactsEdgeCaseTests.ARecordSaysItsPartsInTheWordsTheRecordSaysThem" />, said through the
    /// shared helper by a reader of German: the point is the invariant string, not the culture the runner started in.</summary>
    [Theory]
    [InlineData(28.5, 30.0, 0, "CrapFacts { Max = 28.5, Threshold = 30, OverThreshold = 0 }")]
    [InlineData(41.0, 30.0, 3, "CrapFacts { Max = 41, Threshold = 30, OverThreshold = 3 }")]
    [InlineData(null, 30.0, 0, "CrapFacts { Max = , Threshold = 30, OverThreshold = 0 }")]
    [InlineData(28.5, null, 0, "CrapFacts { Max = 28.5, Threshold = , OverThreshold = 0 }")]
    [InlineData(28.5, 30.0, null, "CrapFacts { Max = 28.5, Threshold = 30, OverThreshold =  }")]
    [InlineData(null, null, null, "CrapFacts { Max = , Threshold = , OverThreshold =  }")]
    [InlineData(double.MaxValue, double.MinValue, int.MaxValue, "CrapFacts { Max = 1.7976931348623157E+308, Threshold = -1.7976931348623157E+308, OverThreshold = 2147483647 }")]
    [InlineData(-1.0, 0.0, int.MinValue, "CrapFacts { Max = -1, Threshold = 0, OverThreshold = -2147483648 }")]
    public void CrapFactsSayTheirWordsInTheInvariantCultureWhoeverReadsThem(double? max, double? threshold, int? over, string expected) =>
        CultureScope.Runs(German, () =>
        {
            Assert.Equal(expected, CultureScope.Words(new CrapFacts(max, threshold, over)));
            Assert.Equal(German, Thread.CurrentThread.CurrentCulture);
        });

    /// <summary>The record's own words do follow the reader - which is why the assertions above are pinned: the German
    /// reader writes the decimal comma the invariant words do not.</summary>
    [Fact]
    public void WithoutTheSharedHelperTheWordsFollowTheReader() =>
        Assert.Equal(
            "CrapFacts { Max = 28,5, Threshold = 30, OverThreshold = 0 }",
            CultureScope.Says(German, () => new CrapFacts(28.5, 30, 0).ToString()));

    // ----- the copies

    /// <summary>Every private <c>Words(record)</c> copy still in the assembly says exactly what the shared helper says,
    /// for a record whose numbers a German reader would write differently, and leaves the reader's culture as it
    /// found it.</summary>
    [Fact]
    public void EveryCopyOfWordsSaysWhatTheSharedHelperSays()
    {
        var copies = Copies();
        Assert.NotEmpty(copies);

        var drifted = new List<string>();
        foreach (var copy in copies)
        {
            var record = Filled(copy.GetParameters()[0].ParameterType);
            CultureScope.Runs(German, () =>
            {
                var shared = Answer(() => CultureScope.Words(record));
                var own = Answer(() => (string)copy.Invoke(null, [record])!);
                if (shared != own || !Equals(Thread.CurrentThread.CurrentCulture, German))
                {
                    drifted.Add($"{copy.DeclaringType!.Name}.{copy.Name}: shared '{shared}', copy '{own}', reader {Thread.CurrentThread.CurrentCulture.Name}");
                }
            });
        }

        Assert.True(drifted.Count == 0, string.Join(Environment.NewLine, drifted));
    }

    /// <summary>The copies may only be retired, never added: a new edge-case file calls <see cref="CultureScope.Words" />.</summary>
    [Fact]
    public void TheCopiesOfWordsOnlyShrink() =>
        Assert.InRange(Copies().Count, 0, MostCopies);

    // ----- the German reader

    /// <summary><see cref="ViewerCulture.German" /> runs its question as a reader of <c>de-DE</c>, exactly as the
    /// shared scope does, and puts the reader back - after an answer and after a throw.</summary>
    [Fact]
    public void TheGermanViewerIsTheSharedScopeOfGerman()
    {
        var before = Thread.CurrentThread.CurrentCulture;
        string? viewer = null;
        string? shared = null;

        ViewerCulture.German(() => viewer = $"{CultureInfo.CurrentCulture.Name} {28.5}");
        CultureScope.Runs("de-DE", () => shared = $"{CultureInfo.CurrentCulture.Name} {28.5}");

        Assert.Equal("de-DE 28,5", viewer);
        Assert.Equal(shared, viewer);
        Assert.Equal(before, Thread.CurrentThread.CurrentCulture);

        Assert.Throws<InvalidOperationException>(() => ViewerCulture.German(() => throw new InvalidOperationException()));
        Assert.Equal(before, Thread.CurrentThread.CurrentCulture);
    }

    // ----- the machinery

    /// <summary>The private static <c>Words</c> methods of the test assembly that read a record: one parameter that is
    /// not a string (the <c>Words(string?)</c> of a moment is a different helper that shares the name), a string back,
    /// and not the shared helper itself.</summary>
    private static List<MethodInfo> Copies() =>
        [.. typeof(InvariantWordsSupportTests).Assembly.GetTypes()
            .Where(type => type != typeof(CultureScope))
            .SelectMany(type => type.GetMethods(Private))
            .Where(method => method.Name == "Words"
                && method.ReturnType == typeof(string)
                && method.GetParameters() is [{ ParameterType: var parameter }]
                && parameter != typeof(string)
                && parameter != typeof(object))];

    /// <summary>A record of the type named whose every number is one a German reader writes with a comma, and whose
    /// every other part is the default it starts as.</summary>
    private static object Filled(Type type)
    {
        var record = RuntimeHelpers.GetUninitializedObject(type);
        foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            var number = Nullable.GetUnderlyingType(field.FieldType) ?? field.FieldType;
            object? value = number == typeof(double) ? 28.5
                : number == typeof(float) ? 28.5f
                : number == typeof(decimal) ? 28.5m
                : null;
            if (value is not null)
            {
                field.SetValue(record, value);
            }
        }

        return record;
    }

    /// <summary>What a question says, or the kind of failure it ends in: a record left at its defaults may not be able
    /// to say itself at all, and then the copy and the shared helper must fail alike.</summary>
    private static string Answer(Func<string> question)
    {
        try
        {
            return question();
        }
        catch (Exception failure)
        {
            var inner = failure is TargetInvocationException { InnerException: { } cause } ? cause : failure;
            return $"<{inner.GetType().Name}>";
        }
    }
}
