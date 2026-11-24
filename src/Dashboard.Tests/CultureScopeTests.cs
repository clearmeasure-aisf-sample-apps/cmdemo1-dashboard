using System.Globalization;

namespace Dashboard.Tests;

/// <summary>
/// The shared culture scope: a question is asked as a reader of the named culture, and the thread is left holding
/// the culture it had before - after an answer, after nothing, and after a throw.
/// </summary>
public class CultureScopeTests
{
    private static readonly CultureInfo Before = CultureInfo.GetCultureInfo("en-US");

    [Theory]
    [InlineData("de-DE")]
    [InlineData("fr-FR")]
    [InlineData("")]
    public void SaysByNameAsksAsThatCultureAndRestores(string name) =>
        Around(() =>
        {
            Assert.Equal(name, CultureScope.Says(name, () => CultureInfo.CurrentCulture.Name));
            Assert.Equal(Before, CultureInfo.CurrentCulture);
        });

    [Fact]
    public void SaysByCultureAsksAsThatCultureAndRestores() =>
        Around(() =>
        {
            var german = CultureInfo.GetCultureInfo("de-DE");
            Assert.Equal("1,5", CultureScope.Says(german, () => 1.5.ToString(CultureInfo.CurrentCulture)));
            Assert.Equal(Before, CultureInfo.CurrentCulture);
        });

    [Fact]
    public void TheEmptyNameIsTheInvariantCulture() =>
        Around(() => Assert.Equal(CultureInfo.InvariantCulture, CultureScope.Says("", () => CultureInfo.CurrentCulture)));

    [Fact]
    public void RunsByNameAndByCultureRunAsThatCultureAndRestore() =>
        Around(() =>
        {
            string? seen = null;
            CultureScope.Runs("de-DE", () => seen = CultureInfo.CurrentCulture.Name);
            Assert.Equal("de-DE", seen);
            Assert.Equal(Before, CultureInfo.CurrentCulture);

            CultureScope.Runs(CultureInfo.GetCultureInfo("fr-FR"), () => seen = CultureInfo.CurrentCulture.Name);
            Assert.Equal("fr-FR", seen);
            Assert.Equal(Before, CultureInfo.CurrentCulture);
        });

    [Fact]
    public void AThrowInsideStillRestoresTheReader() =>
        Around(() =>
        {
            Assert.Throws<InvalidOperationException>(() =>
                CultureScope.Says<int>("de-DE", () => throw new InvalidOperationException()));
            Assert.Equal(Before, CultureInfo.CurrentCulture);
            Assert.Throws<InvalidOperationException>(() =>
                CultureScope.Runs("fr-FR", () => throw new InvalidOperationException()));
            Assert.Equal(Before, CultureInfo.CurrentCulture);
        });

    [Fact]
    public void AUsingScopeHoldsUntilDisposed()
    {
        Around(() =>
        {
            using (new CultureScope("de-DE"))
            {
                Assert.Equal("de-DE", CultureInfo.CurrentCulture.Name);
            }

            Assert.Equal(Before, CultureInfo.CurrentCulture);
        });
    }

    [Fact]
    public void AnAbsentQuestionOrCultureIsRefused()
    {
        Assert.Throws<ArgumentNullException>(() => CultureScope.Says<int>("de-DE", null!));
        Assert.Throws<ArgumentNullException>(() => CultureScope.Runs("de-DE", null!));
        Assert.Throws<ArgumentNullException>(() => new CultureScope((CultureInfo)null!));
    }

    /// <summary>Runs <paramref name="test" /> with a known culture before it, without leaning on the scope under test.</summary>
    private static void Around(Action test)
    {
        var reader = Thread.CurrentThread.CurrentCulture;
        try
        {
            Thread.CurrentThread.CurrentCulture = Before;
            test();
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = reader;
        }
    }
}
