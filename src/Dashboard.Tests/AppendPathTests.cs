using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// <see cref="UriExtensions.AppendPath(Uri, string)" /> is the one join the page says of a path that comes after
/// an address, and the places that build an address from a base and a path - the probe's
/// <see cref="ProbeUrl.Combine" />, the release link <see cref="LinkText.Release(Uri?, string?)" />, the diagram
/// file of the runtime view - all say it through the one join the readings of a base share.
/// <see cref="ProbeUrlTests" /> and <see cref="ProbeUrlEdgeCaseTests" /> pin the join as the probe says it;
/// <see cref="LinkTextEdgeCaseTests" /> pins it as the release link says it. This file pins the joiner itself: the
/// join with and without the slashes each side brings, the empty and the all-slash path that come back as the
/// base's own root, the runs of slashes the join collapses at the front of the path and the ones it leaves
/// standing at the back, the base with its userinfo, the base as it stands - its query and its fragment keeping
/// the segment the join adds, as the release under an Octopus project page is pinned - the relative base the
/// address itself refuses in the framework's own words, the refusals by the names of their parameters and in
/// their order, the wire form of a path with whitespace and unicode, the <c>?</c> and the <c>#</c> the join
/// re-parses as query and fragment, where the probe's reading of a base and the link's differ, and the surface of
/// <see cref="UriExtensions" /> itself.
/// </summary>
public class AppendPathTests
{
    private static Uri Base() => new("https://app.example.net");

    // ---------------------------------------------------------------- the join

    /// <summary>The base, without its trailing slash, one slash, and the path, without its leading one: the base
    /// with and without its trailing slash, the path with and without its leading one, a base that already has a
    /// path, a base on a port, a base with its userinfo, and a base of the <c>file</c> scheme the release link
    /// builds the same way as an https one.</summary>
    [Theory]
    [InlineData("https://app.example.net", "/_healthcheck", "https://app.example.net/_healthcheck")]
    [InlineData("https://app.example.net/", "/_healthcheck", "https://app.example.net/_healthcheck")]
    [InlineData("https://app.example.net/", "alive", "https://app.example.net/alive")]
    [InlineData("https://app.example.net/base/", "/_version", "https://app.example.net/base/_version")]
    [InlineData("http://localhost:5000", "/health/ready", "http://localhost:5000/health/ready")]
    [InlineData("https://user:pass@example.com:8443/base/", "/_healthcheck", "https://user:pass@example.com:8443/base/_healthcheck")]
    [InlineData("file:///srv/app", "deployments/releases/2.4.15", "file:///srv/app/deployments/releases/2.4.15")]
    public void BaseAddressAndPathAreJoinedWithOneSlash(string baseAddress, string path, string asked) =>
        Assert.Equal(asked, UriExtensions.AppendPath(new Uri(baseAddress), path).AbsoluteUri);

    /// <summary>The path that is empty and the path that is nothing but slashes come back as the base's own
    /// root, with its trailing slash: there is nothing to join, and the one slash the join adds is the root's
    /// own.</summary>
    [Theory]
    [InlineData("https://app.example.net", "", "https://app.example.net/")]
    [InlineData("https://app.example.net", "/", "https://app.example.net/")]
    [InlineData("https://app.example.net", "///", "https://app.example.net/")]
    [InlineData("https://app.example.net/base/", "///", "https://app.example.net/base/")]
    public void AnEmptyOrAllSlashPathIsTheBaseRootWithItsTrailingSlash(string baseAddress, string path, string asked)
    {
        var joined = UriExtensions.AppendPath(new Uri(baseAddress), path);

        Assert.Equal(asked, joined.AbsoluteUri);
        Assert.True(joined.AbsolutePath.EndsWith('/'), "the root keeps its trailing slash");
    }

    /// <summary>The runs of slashes at the front of the path are collapsed to the one slash the join adds, and
    /// the runs the join does not add stand: the back of the path and the middle of it are the caller's own
    /// letters, and the join edits them not at all.</summary>
    [Theory]
    [InlineData("///_healthcheck", "https://app.example.net/_healthcheck")]
    [InlineData("////", "https://app.example.net/")]
    [InlineData("///x///y", "https://app.example.net/x///y")]
    [InlineData("/healthcheck/", "https://app.example.net/healthcheck/")]
    [InlineData("/x///y", "https://app.example.net/x///y")]
    public void TheJoinCollapsesOnlyTheSlashesItJoinsWith(string path, string asked)
    {
        var joined = UriExtensions.AppendPath(Base(), path);

        Assert.Equal(asked, joined.AbsoluteUri);
    }

    // ---------------------------------------------------------------- the base, as it stands

    /// <summary>The link takes the base as it stands: what the base asked for or pointed at after its path - its
    /// query and its fragment - keeps the segment the join adds after it, and the joined address is the address
    /// the deployment wrote with the segment appended and nothing taken from it. The fragment is where an Octopus
    /// project page keeps its space and its project, so the release the join writes stands under the fragment the
    /// page carried, beside the path that names the app.</summary>
    [Theory]
    [InlineData("https://octopus.example.net/app#/Spaces-1/projects/demo-ui", "/deployments/releases/2.4.15",
        "https://octopus.example.net/app#/Spaces-1/projects/demo-ui/deployments/releases/2.4.15")]
    [InlineData("https://octopus.example.net/app#/Spaces-1/projects/demo-ui/", "/deployments/releases/2.4.15",
        "https://octopus.example.net/app#/Spaces-1/projects/demo-ui/deployments/releases/2.4.15")]
    [InlineData("https://app.example.net/p?q=1", "/alive", "https://app.example.net/p?q=1/alive")]
    [InlineData("https://app.example.net/p?q=1#frag", "/a/b", "https://app.example.net/p?q=1#frag/a/b")]
    public void TheQueryAndTheFragmentOfTheBaseAddressKeepTheSegmentTheJoinAdds(string baseAddress, string path, string asked)
    {
        var joined = UriExtensions.AppendPath(new Uri(baseAddress), path);

        Assert.Equal(asked, joined.AbsoluteUri);
    }

    /// <summary>The fragment the base carried is read back as the fragment of the joined address, the segment the
    /// join adds standing in it after the letters the base wrote - the release page the link leads to.</summary>
    [Fact]
    public void TheFragmentOfTheBaseAddressKeepsTheReleaseTheJoinWritesUnderIt()
    {
        var joined = UriExtensions.AppendPath(
            new Uri("https://octopus.example.net/app#/Spaces-1/projects/demo-ui"),
            "/deployments/releases/2.4.15");

        Assert.Equal("/app", joined.AbsolutePath);
        Assert.Equal("#/Spaces-1/projects/demo-ui/deployments/releases/2.4.15", joined.Fragment);
    }

    // ---------------------------------------------------------------- the refusals, by the names of their parameters and in their order

    /// <summary>The base address that is not there is refused by the name of the parameter it would have
    /// filled, the path that is not there is refused by the name of its own, and with both not there the base is
    /// said first: the join asks an address, and it is refused at the door, not halfway through.</summary>
    [Fact]
    public void ANullBaseAddressIsRefusedByTheNameBaseAddress()
    {
        var refused = Assert.Throws<ArgumentNullException>(() => UriExtensions.AppendPath(null!, "/_healthcheck"));

        Assert.Equal("baseAddress", refused.ParamName);
    }

    [Fact]
    public void ANullPathIsRefusedByTheNamePath()
    {
        var refused = Assert.Throws<ArgumentNullException>(() => UriExtensions.AppendPath(Base(), null!));

        Assert.Equal("path", refused.ParamName);
    }

    [Fact]
    public void ANullBaseAddressIsRefusedBeforeANullPathIs()
    {
        var refused = Assert.Throws<ArgumentNullException>(() => UriExtensions.AppendPath(null!, null!));

        Assert.Equal("baseAddress", refused.ParamName);
    }

    /// <summary>The base that is relative to a page is refused by the address itself, in the framework's own
    /// words and naming no argument: the very sentence the address says when its <see cref="Uri.AbsoluteUri" />
    /// is read, which is the sentence the link builders are held to as well - neither <c>baseAddress</c> nor
    /// <c>path</c> is named anywhere in it.</summary>
    [Theory]
    [InlineData("rel/path")]
    [InlineData("./a")]
    [InlineData("a/b")]
    [InlineData("x")]
    public void ABaseAddressThatIsRelativeToThePageIsRefusedInTheAddresssOwnWords(string baseAddress)
    {
        var relative = new Uri(baseAddress, UriKind.Relative);

        var refused = Assert.Throws<InvalidOperationException>(() => UriExtensions.AppendPath(relative, "/x"));

        Assert.IsType<InvalidOperationException>(refused);
        Assert.Equal("This operation is not supported for a relative URI.", refused.Message);
        Assert.Equal(Record.Exception(() => _ = relative.AbsoluteUri)!.Message, refused.Message);
        Assert.DoesNotContain("baseAddress", refused.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("path", refused.Message, StringComparison.Ordinal);
    }

    // ---------------------------------------------------------------- the wire form of the path, and the letters it does not keep

    /// <summary>The path with whitespace and unicode is said in its wire form, not in the letters it came in
    /// with: the address that goes out on the wire is what <see cref="Uri.AbsoluteUri" /> says, and
    /// <see cref="Uri.ToString" /> keeps the letters the path was brought in with, which are not the same
    /// letters.</summary>
    [Theory]
    [InlineData("/health check", "https://app.example.net/health%20check")]
    [InlineData("/a b/c d", "https://app.example.net/a%20b/c%20d")]
    [InlineData("/héllo", "https://app.example.net/h%C3%A9llo")]
    public void APathWithWhitespaceOrUnicodeIsSaidInItsWireFormNotInItsKeptLetters(string path, string wireForm)
    {
        var joined = UriExtensions.AppendPath(Base(), path);

        Assert.Equal(wireForm, joined.AbsoluteUri);
        Assert.NotEqual(wireForm, joined.ToString());
    }

    /// <summary>The <c>?</c> and the <c>#</c> inside the path are not path letters in the address the join says:
    /// the joined string is parsed as an address, and from there on the <c>?</c> starts the query and the
    /// <c>#</c> starts the fragment, whatever letters the path brought after them.</summary>
    [Theory]
    [InlineData("/health?x=1", "/health", "?x=1", "")]
    [InlineData("/health#frag", "/health", "", "#frag")]
    [InlineData("a?b#c", "/a", "?b", "#c")]
    public void AQuestionMarkOrHashInThePathIsReparsedAsQueryAndFragment(string path, string pathPart, string queryPart, string fragmentPart)
    {
        var joined = UriExtensions.AppendPath(Base(), path);

        Assert.Equal(pathPart, joined.AbsolutePath);
        Assert.Equal(queryPart, joined.Query);
        Assert.Equal(fragmentPart, joined.Fragment);
    }

    // ---------------------------------------------------------------- where the probe's reading of a base and the link's differ

    /// <summary>The two readings of a base say the one join two ways, and differ only where the base carries more
    /// than its path: the probe takes the base up to its path, so the address it asks at has neither the query
    /// nor the fragment the base carried, while the link takes the base as it stands and keeps them. For a base
    /// that is nothing but its path the two say the very same address, and the day one of the readings is folded
    /// into the other, this says which answer is lost.</summary>
    [Theory]
    [InlineData("https://app.example.net/base", "/_version", "https://app.example.net/base/_version", "https://app.example.net/base/_version")]
    [InlineData("https://app.example.net/p?q=1", "/alive", "https://app.example.net/p/alive", "https://app.example.net/p?q=1/alive")]
    [InlineData("https://app.example.net/p#frag", "alive", "https://app.example.net/p/alive", "https://app.example.net/p#frag/alive")]
    [InlineData("https://app.example.net/p?q=1#frag", "/a/b", "https://app.example.net/p/a/b", "https://app.example.net/p?q=1#frag/a/b")]
    public void TheProbeTakesTheBaseUpToItsPathAndTheLinkTakesItAsItStands(string baseAddress, string path, string probed, string linked)
    {
        var baseUri = new Uri(baseAddress);

        Assert.Equal(probed, ProbeUrl.Combine(baseUri, path).AbsoluteUri);
        Assert.Equal(linked, UriExtensions.AppendPath(baseUri, path).AbsoluteUri);
    }

    // ---------------------------------------------------------------- the members

    /// <summary><see cref="UriExtensions" /> is the holder of the join and nothing else public: the only public
    /// member is <c>Uri AppendPath(Uri baseAddress, String path)</c>, the one a caller says on a base address,
    /// and the day a second public member is added to the holder, this says so.</summary>
    [Fact]
    public void UriExtensionsIsAPublicStaticHolderWhoseOnlyPublicMemberIsAppendPath()
    {
        var type = typeof(UriExtensions);

        Assert.True(type.IsAbstract && type.IsSealed, "the holder is static: nothing of it is created");
        Assert.Empty(type.GetConstructors(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic));
        Assert.Empty(type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static));
        Assert.Empty(type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static));
        Assert.Empty(type.GetEvents(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static));
        Assert.Empty(type.GetNestedTypes(BindingFlags.Public));
        Assert.Empty(type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));

        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);

        Assert.Equal(
            ["Uri AppendPath(Uri baseAddress, String path)"],
            [.. methods.Select(Signature).Order(StringComparer.Ordinal)]);

        Assert.All(methods, method => Assert.True(method.IsStatic, $"{method.Name} is called through an instance"));
    }

    /// <summary>The holder declares a second member, the one join the readings of a base share, and this one is
    /// not public: <see cref="ProbeUrl.Combine" />, its only caller of it, is of the same assembly, and the
    /// public surface <see cref="UriExtensionsIsAPublicStaticHolderWhoseOnlyPublicMemberIsAppendPath" /> pins -
    /// the one public member <c>Uri AppendPath(Uri baseAddress, String path)</c> - stands as that test writes it.
    /// It takes the base as a string and not as an address, because the readings that call it have read it
    /// already, up to its path or as it stands, and the join edits nothing but the slashes.</summary>
    [Fact]
    public void TheSharedJoinIsTheHoldersSecondAndIsInternal()
    {
        var member = typeof(UriExtensions).GetMethod("Join", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

        Assert.NotNull(member);
        Assert.False(member.IsPublic, "the second member of the holder is public: the public surface test pins it to AppendPath alone");
        Assert.True(member.IsStatic, "the join is called through an instance");
        Assert.Equal(typeof(Uri), member.ReturnType);
        Assert.Equal(["root", "path"], [.. member.GetParameters().Select(parameter => parameter.Name!)]);
        Assert.Equal(typeof(string), member.GetParameters()[0].ParameterType);
        Assert.Equal(typeof(string), member.GetParameters()[1].ParameterType);
    }

    /// <summary>A member as the surface is written: what it gives back, what it is called, and its arguments
    /// in the order a caller names them, each with its type and its name.</summary>
    private static string Signature(MethodInfo method) =>
        $"{method.ReturnType.Name} {method.Name}({string.Join(", ", method.GetParameters().Select(parameter => $"{parameter.ParameterType.Name} {parameter.Name}"))})";
}
