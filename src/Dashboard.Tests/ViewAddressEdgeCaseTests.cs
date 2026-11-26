namespace Dashboard.Tests;

/// <summary>
/// Boundary, null/empty and fixed-value tests of <see cref="ViewAddress" /> (#1113), limited to what the suite did not
/// already pin. <see cref="ViewAddressTests" />, <see cref="ViewAddressRuntimeHashTests" />,
/// <see cref="ViewAddressHashWordEdgeCaseTests" />, <see cref="DashboardViewEdgeCaseTests" /> and
/// <see cref="ApplicationViewTests" /> already cover the words of a hash, its frames, the database and application
/// zooms, the work items filter and the hash an address writes. What is left and pinned here: the length and alphabet
/// boundaries of a deployable's name as <see cref="ViewAddress.Parse(string)" /> reads it out of a hash, a zoom word
/// behind a path that is no zoom, the spaces inside an environment's name next to a zoom, and a doubled slash after
/// the runtime word.
/// </summary>
public sealed class ViewAddressEdgeCaseTests
{
    private static ViewAddress Runtime(string environment, bool database = false, string? application = null) =>
        new(DashboardView.Runtime, environment, database, application);

    // ------------------------------------------------------------------ a deployable's name, read from a hash

    /// <summary>
    /// A deployable's name read out of a hash is taken from one character up to sixty-four of ASCII letters, digits,
    /// <c>-</c> and <c>_</c>; the hash it writes back opens the same address.
    /// </summary>
    [Theory]
    [InlineData("a")]
    [InlineData("1")]
    [InlineData("_")]
    [InlineData("123")]
    [InlineData("Z9-_z")]
    public void AShortDeployableNameOfTheAllowedCharactersOpensItsView(string application)
    {
        var address = ViewAddress.Parse($"runtime/uat/app/{application}");

        Assert.Equal(Runtime("uat", application: application), address);
        Assert.Equal($"runtime/uat/app/{application}", address.ToHash());
        Assert.Equal(address, ViewAddress.Parse(address.ToHash()));
    }

    /// <summary>The longest name the address takes is sixty-four characters; one more is refused.</summary>
    [Fact]
    public void ADeployableNameOfSixtyFourIsTakenAndOfSixtyFiveIsRefused()
    {
        var longest = ViewAddress.Parse($"runtime/uat/app/{ApplicationNames.Longest}");
        var tooLong = ViewAddress.Parse($"runtime/uat/app/{ApplicationNames.LongestPlusOne}");

        Assert.Equal(Runtime("uat", application: ApplicationNames.Longest), longest);
        Assert.Equal($"runtime/uat/app/{ApplicationNames.Longest}", longest.ToHash());

        Assert.Equal(Runtime("uat"), tooLong);
        Assert.Null(tooLong.Application);
        Assert.Equal("runtime/uat", tooLong.ToHash());
    }

    /// <summary>
    /// Letters and digits outside ASCII, and marks other than <c>-</c> and <c>_</c>, are refused: the environment's
    /// diagram is shown.
    /// </summary>
    [Theory]
    [InlineData("café")]
    [InlineData("ü")]
    [InlineData("١٢٣")]
    [InlineData("ui+x")]
    [InlineData("ui:x")]
    [InlineData("ui~x")]
    public void ADeployableNameOutsideTheAllowedAlphabetIsRefused(string application)
    {
        var address = ViewAddress.Parse($"runtime/uat/app/{application}");

        Assert.Equal(Runtime("uat"), address);
        Assert.Equal("runtime/uat", address.ToHash());
    }

    // ------------------------------------------------------------------------ a zoom behind a longer path

    /// <summary>
    /// The database word is the last segment of a runtime hash whatever stands before it, so a path that names an
    /// application view or another database view and then <c>database</c> is the database view of an environment
    /// whose name is that whole path.
    /// </summary>
    [Theory]
    [InlineData("runtime/uat/app/ui/database", "uat/app/ui")]
    [InlineData("runtime/uat/database/database", "uat/database")]
    [InlineData("runtime/a/b/c/database", "a/b/c")]
    public void TheDatabaseWordBehindALongerPathZoomsIntoThatPath(string hash, string environment)
    {
        var address = ViewAddress.Parse(hash);

        Assert.Equal(Runtime(environment, database: true), address);
        Assert.Null(address.Application);
        Assert.Equal(hash, address.ToHash());
    }

    /// <summary>
    /// A doubled slash right after the runtime word is walked past with the environment's own slashes, so
    /// <c>runtime//app/ui</c> names the environment <c>app/ui</c> rather than the application view of nothing.
    /// </summary>
    [Fact]
    public void ADoubledSlashAfterTheRuntimeWordLeavesTheMarkerInsideTheName()
    {
        var address = ViewAddress.Parse("runtime//app/ui");

        Assert.Equal(Runtime("app/ui"), address);
        Assert.Null(address.Application);
        Assert.Equal("runtime/app/ui", address.ToHash());
    }

    // ---------------------------------------------------------- spaces inside an environment beside a zoom

    /// <summary>
    /// Only the ends of the whole hash are trimmed of spaces: a space at the end or the start of an environment's name,
    /// beside a zoom, stays in the name, and the hash written back keeps it.
    /// </summary>
    [Theory]
    [InlineData("runtime/uat /app/ui", "uat ", false, "ui")]
    [InlineData("runtime/ uat/app/ui", " uat", false, "ui")]
    [InlineData("runtime/uat /database", "uat ", true, null)]
    [InlineData("runtime/ uat/database", " uat", true, null)]
    public void ASpaceInsideTheEnvironmentsNameBesideAZoomIsKept(string hash, string environment, bool database, string? application)
    {
        var address = ViewAddress.Parse(hash);

        Assert.Equal(Runtime(environment, database, application), address);
        Assert.Equal(hash, address.ToHash());
        Assert.Equal(address, ViewAddress.Parse(address.ToHash()));
    }

    // ------------------------------------------------------------------------------- the work items filter

    /// <summary>
    /// The work items filter is the first segment behind the word, trimmed and in any case; an empty or blank segment
    /// is no filter, and the parts that belong to the runtime view stay empty whatever the filter.
    /// </summary>
    [Theory]
    [InlineData("work-items/", WorkAudience.All)]
    [InlineData("work-items/ ", WorkAudience.All)]
    [InlineData("work-items/\t/bot", WorkAudience.All)]
    [InlineData("work-items/Team", WorkAudience.Team)]
    [InlineData("#work-items/team/app/ui", WorkAudience.Team)]
    public void TheWorkItemsFilterIsTheFirstSegmentAndNothingElse(string hash, WorkAudience audience)
    {
        var address = ViewAddress.Parse(hash);

        Assert.Equal(DashboardView.WorkItems, address.View);
        Assert.Equal(audience, address.Audience);
        Assert.Null(address.Environment);
        Assert.False(address.Database);
        Assert.Null(address.Application);
    }
}
