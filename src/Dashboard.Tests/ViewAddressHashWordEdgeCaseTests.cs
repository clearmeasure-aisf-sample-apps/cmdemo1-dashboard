namespace Dashboard.Tests;

/// <summary>
/// The four boundaries of <see cref="ViewAddress.Parse(string)" /> that the tests already in the suite leave open,
/// all of them about one rule: a hash is trimmed at its ends and nowhere else, so every word inside it - the word
/// of the view, the word of a zoom, a deployable's name - is read whole, marks and all.
/// </summary>
/// <remarks>
/// <para>
/// <c>src/Dashboard/Runtime/ViewAddress.cs</c> is already pinned by three files: <see cref="DashboardViewEdgeCaseTests" />
/// pins the four <see cref="DashboardView" />s (their numbers, names, order, words, refusals and declared surface) and
/// the <see cref="ViewAddress" /> record underneath them (its five parts, its three fixed addresses, its equality, its
/// hash, its words and its surface), and it pins the hash that says nothing, the frames that are walked past, the
/// segments of a runtime path read apart, the work items filter and the hash an address writes back;
/// <see cref="ViewAddressTests" /> pins the view each word of a hash opens and the database zoom of one environment;
/// and <see cref="ApplicationViewTests" /> pins the application view, the hostile deployable's name that names no
/// view and the hash that carries one. Measuring that lot against the file - every line and every branch of
/// <c>ViewAddress.cs</c> is hit, and the file's behaviour is walked through with the words of both methods turned one
/// at a time - is what left these four open.
/// </para>
/// <para>
/// What is pinned here, and only this: a word that merely begins or ends like a view's name is no view, in all three
/// of the names a hash is read by - and for <c>work-items</c> that had never been said at all, neither side; the
/// <c>database</c> word is the whole of a runtime hash's last segment or it is part of the environment's name, which
/// is what separates <c>runtime/uat/databases</c> from <c>runtime/uat/database</c>; the <c>#</c> is walked past at the
/// front of a hash and nowhere else, so a <c>#</c> at the end of a word is inside that word - it stops
/// <c>cluster#</c> from opening the cluster view and it stays inside an environment's name; and a deployable's name
/// is read whole too, so a name that begins with a space or a tab is refused and the environment's diagram is shown,
/// while a name that ends with one is taken - the space behind it is walked past with the rest of the hash.
/// </para>
/// <para>
/// Three readings of the file cannot be pinned by any hash, and are recorded here so that they are not mistaken for
/// gaps. The tail of a hash is trimmed of its slashes before either marker is looked for, so the text a zoom is read
/// out of can neither begin with a slash nor end with one: <c>last</c> therefore never falls on the first character,
/// which is the only input where <c>last &gt; 0</c> (the database word) and <c>last &gt; marker.Length</c> (the
/// application marker) would answer differently from <c>&gt;=</c>; and what stands before the database word never ends
/// with a slash either, which is the only input where the <c>TrimEnd('/')</c> after it has anything to trim. Those
/// three answer the same way whatever a hash asks of them, so no test can tell them from their own off-by-one.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: a hash is a word the page put in its
/// bar, and every one below is written out in full.
/// </para>
/// </remarks>
public class ViewAddressHashWordEdgeCaseTests
{
    // --------------------------------------- a word of the view is read whole, on both sides

    /// <summary>
    /// The head of a hash is the whole of what stands before its first slash, and a head that merely touches one of
    /// the three words a view is opened by - in front of it, behind it, or with one of them buried in it - opens the
    /// health view instead: <c>work-itemsx</c> and <c>xwork-items</c> are no work items view (the whole of that word
    /// had never been pinned, on either side, although <c>workitems</c> and <c>work-items/board</c> have been), and so
    /// are <c>mycluster</c> and <c>myruntime</c> - where the front of the two other words is pinned by
    /// <c>clusters</c> and <c>runtimes/uat</c> in <see cref="ViewAddressTests" /> and
    /// <see cref="DashboardViewEdgeCaseTests.AWordInsideAHashIsPartOfWhatItStandsIn" />, and their back by nothing.
    /// A dash, a cipher or a mark where a word should end makes it another word, and the words that touch a view's
    /// name still say nothing: no environment, no zoom, no filter, and the empty hash back out again.
    /// </summary>
    [Theory]
    [InlineData("work-itemsx")]
    [InlineData("Work-Itemsx")]
    [InlineData("work-item")]
    [InlineData("my-work-items")]
    [InlineData("xwork-items")]
    [InlineData("mywork-itemsbot")]
    [InlineData("work-items!")]
    [InlineData("work-items2/bot")]
    [InlineData("xwork-items/bot")]
    [InlineData("mycluster")]
    [InlineData("xcluster")]
    [InlineData("cluster-x")]
    [InlineData("cluster!")]
    [InlineData("myruntime")]
    [InlineData("xruntime/uat")]
    [InlineData("runtime-x/uat")]
    [InlineData("runtime!")]
    public void AWordThatOnlyTouchesAViewsNameOpensNoView(string hash)
    {
        var address = ViewAddress.Parse(hash);

        Assert.Equal(ViewAddress.Health, address);
        Assert.Equal(DashboardView.Health, address.View);
        Assert.Null(address.Environment);
        Assert.False(address.Database);
        Assert.Null(address.Application);
        Assert.Equal(WorkAudience.All, address.Audience);
        Assert.Equal(string.Empty, address.ToHash());
    }

    // ------------------------------------------------- the database word, the whole last segment or nothing

    /// <summary>
    /// The database zoom is the whole of a runtime hash's last segment spelled <c>database</c> - in any case, and
    /// nothing else: a word that runs on past it (<c>databases</c>, <c>database2</c>), a word that runs up to it
    /// (<c>mydatabase</c>) or a word marked at its end (<c>database!</c>) is part of the environment's name, which
    /// comes back out of the hash exactly as it went in. The hash that asks for the zoom itself is pinned in
    /// <see cref="ViewAddressTests.TheHashNamesTheDatabaseViewOfAnEnvironment" />; what is pinned here is the width of
    /// the word - the <c>database</c> the reader takes is neither longer nor shorter.
    /// </summary>
    [Theory]
    [InlineData("runtime/uat/databases", "uat/databases")]
    [InlineData("runtime/uat/DATABASES", "uat/DATABASES")]
    [InlineData("runtime/uat/mydatabase", "uat/mydatabase")]
    [InlineData("runtime/uat/database2", "uat/database2")]
    [InlineData("runtime/uat/database!", "uat/database!")]
    [InlineData("runtime/uat/database-x", "uat/database-x")]
    [InlineData("runtime/databases", "databases")]
    public void TheDatabaseWordIsTheWholeLastSegmentOrPartOfTheNames(string hash, string environment)
    {
        var address = ViewAddress.Parse(hash);

        Assert.Equal(new ViewAddress(DashboardView.Runtime, environment), address);
        Assert.Equal(DashboardView.Runtime, address.View);
        Assert.Equal(environment, address.Environment);
        Assert.False(address.Database);
        Assert.Null(address.Application);
        Assert.Equal(WorkAudience.All, address.Audience);
        Assert.Equal(hash, address.ToHash());
    }

    // --------------------------------------------------------------- the # at the front and nowhere else

    /// <summary>
    /// A <c>#</c> is walked past at the front of a hash and stands where it is at its end: a word of the view with a
    /// <c>#</c> behind it is no word of the view, so the cluster view, the work items view and the runtime view are
    /// all out of reach of <c>cluster#</c>, <c>work-items#</c> and <c>runtime#</c> - however many marks are stacked
    /// there, and whoever opens the hash - and the health view is what is left, with nothing said and the empty hash
    /// written back. The <c>#</c> in front of a hash is pinned in
    /// <see cref="DashboardViewEdgeCaseTests.TheSpacesAndTheMarkersThatOnlyFrameAHashAreWalkedPast" />.
    /// </summary>
    [Theory]
    [InlineData("cluster#")]
    [InlineData("cluster##")]
    [InlineData("#cluster#")]
    [InlineData("work-items#")]
    [InlineData("runtime#")]
    [InlineData("runtime##")]
    public void AHashMarkBehindTheWordOfAViewOpensNoView(string hash)
    {
        var address = ViewAddress.Parse(hash);

        Assert.Equal(ViewAddress.Health, address);
        Assert.Equal(string.Empty, address.ToHash());
    }

    /// <summary>
    /// Inside a runtime hash the <c>#</c> is a character like any other, so the name it sits in is the name with the
    /// mark in it: an environment called <c>uat#</c> is an environment (and writes its mark straight back), the
    /// database word marked at its end is part of the environment's name rather than a zoom, and a deployable named
    /// <c>ui#</c> is refused a view of its own - the mark is no more part of the letters, digits, dash and understore
    /// a name may hold than a point or a space is, so the environment's diagram is shown instead, and the hash that
    /// goes with it is the environment's own two words, the zoom and its mark dropped together.
    /// </summary>
    [Theory]
    [InlineData("runtime/uat#", "uat#", false, null, "runtime/uat#")]
    [InlineData("runtime/uat/database#", "uat/database#", false, null, "runtime/uat/database#")]
    [InlineData("runtime/uat/app/ui#", "uat", false, null, "runtime/uat")]
    public void AHashMarkInsideANameIsPartOfTheName(string hash, string environment, bool database, string? application, string written)
    {
        var address = ViewAddress.Parse(hash);

        Assert.Equal(new ViewAddress(DashboardView.Runtime, environment, database, application), address);
        Assert.Equal(DashboardView.Runtime, address.View);
        Assert.Equal(environment, address.Environment);
        Assert.Equal(database, address.Database);
        Assert.Equal(application, address.Application);
        Assert.Equal(WorkAudience.All, address.Audience);

        // The hash that refuses the mark writes the name it settled on, and opens that same address again.
        Assert.Equal(written, address.ToHash());
        Assert.Equal(address, ViewAddress.Parse(written));
    }

    /// <summary>
    /// Behind the one segment a work items hash is filtered by, a <c>#</c> names no filter either:
    /// <c>work-items/bot#</c> is the whole file rather than the bots' half of it. The filter that is looked for is
    /// trimmed of its spaces and read in any case, which <see cref="DashboardViewEdgeCaseTests" /> and
    /// <see cref="WorkItemsTextTests" /> pin; a mark is neither a space nor a case, and is a word of its own.
    /// </summary>
    [Fact]
    public void AHashMarkBehindAFiltersWordNamesNoFilter()
    {
        var address = ViewAddress.Parse("work-items/bot#");

        Assert.Equal(new ViewAddress(DashboardView.WorkItems), address);
        Assert.Equal(DashboardView.WorkItems, address.View);
        Assert.Equal(WorkAudience.All, address.Audience);
        Assert.Equal("work-items", address.ToHash());
    }

    // ------------------------------------------------------------------ a deployable's name is not trimmed

    /// <summary>
    /// The spaces the address walks past are the ones at the ends of the whole hash, so a deployable's name that
    /// <em>begins</em> with a space or a tab is refused - it is no name of letters, digits, dash and understore, and
    /// an address that refuses the name still shows the environment's diagram, the zoom dropped with it - while a
    /// name that merely <em>ends</em> with a space has already lost it to that first trim and is taken for the name it
    /// is left as: <c>runtime/uat/app/ui </c> opens <c>ui</c>, and <c>-_</c> keeps its two marks. A name with a space
    /// in its middle, and a path that runs on past the name, are no names at all either - the whole tail goes into the
    /// environment's name, mark and space included. What the address writes back is what a hash then reads again.
    /// </summary>
    [Theory]
    [InlineData("runtime/uat/app/ ui", "uat", null)]
    [InlineData("runtime/uat/app/ ui ", "uat", null)]
    [InlineData("runtime/uat/app/\tui", "uat", null)]
    [InlineData("runtime/uat/app/u i", "uat", null)]
    [InlineData("runtime/uat/app/ ui/x", "uat/app/ ui/x", null)]
    [InlineData("runtime/uat/app/ui ", "uat", "ui")]
    [InlineData("runtime/uat/app/-_ ", "uat", "-_")]
    public void ADeployablesNameIsReadWholeAndIsTrimmedOnlyWithTheHash(string hash, string environment, string? application)
    {
        var address = ViewAddress.Parse(hash);

        Assert.Equal(new ViewAddress(DashboardView.Runtime, environment, Application: application), address);
        Assert.Equal(DashboardView.Runtime, address.View);
        Assert.Equal(environment, address.Environment);
        Assert.False(address.Database);
        Assert.Equal(application, address.Application);
        Assert.Equal(WorkAudience.All, address.Audience);

        // The hash an address of a name writes is one with the address of that name, trimmed or refused as it was.
        var written = ViewAddress.Parse(address.ToHash());
        Assert.Equal(address, written);
        Assert.Equal(application, written.Application);
    }
}
