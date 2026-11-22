using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// <see cref="ProbeUrl" /> joins a node's base address and a path into the one address the prober asks at:
/// the base up to its path, without its trailing slash, one slash, and the path, without its leading slash.
/// <see cref="ProbeUrlTests" /> pins the join as the page uses it: the base with and without its trailing
/// slash, the path with and without its leading one, a base that already has a path, and a base with a
/// port - <c>https://app.example.net</c> and <c>/_healthcheck</c> make <c>https://app.example.net/_healthcheck</c>,
/// and so on.
/// <para>
/// What is pinned here is every edge the join leaves open, and only that: the two refusals, each by the name
/// of the parameter refused - <c>baseAddress</c> and <c>path</c>; the path that is empty and the path that is
/// nothing but slashes, which come back as the base's own root with its trailing slash; the path that comes
/// without its leading slash; the runs of slashes the join collapses at the front of the path and the ones it
/// leaves standing at the back; the base with its userinfo kept in the joined address, beside the port
/// <see cref="ProbeUrlTests" /> pins; the base whose query and fragment the join drops; the base that is
/// relative to a page, which the join refuses; the path with whitespace and unicode, whose wire form the join
/// says in <see cref="Uri.AbsoluteUri" /> and not in the letters <see cref="Uri.ToString" /> keeps; the
/// <c>?</c> and the <c>#</c> inside a path, which the join re-parses as query and fragment rather than letting
/// them stand as path letters; and the public surface of <see cref="ProbeUrl" /> itself, so that the day a
/// second member is added to the holder, the list below says so.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: <see cref="ProbeUrl.Combine" />
/// takes a base and a path and says an address, and every address below is one the test brought.
/// </para>
/// </summary>
public class ProbeUrlEdgeCaseTests
{
    private static Uri Base() => new("https://app.example.net");

    // ---------------------------------------------------------------- the refusals, by the names of their parameters

    /// <summary>The base address that is not there is refused by the name of the parameter it would have
    /// filled, and the path that is not there is refused by the name of its own: the prober asks an address,
    /// and it is refused at the door, not halfway through the join.</summary>
    [Fact]
    public void ANullBaseAddressIsRefusedByTheNameBaseAddress()
    {
        var refused = Assert.Throws<ArgumentNullException>(() => ProbeUrl.Combine(null!, "/_healthcheck"));

        Assert.Equal("baseAddress", refused.ParamName);
    }

    [Fact]
    public void ANullPathIsRefusedByTheNamePath()
    {
        var refused = Assert.Throws<ArgumentNullException>(() => ProbeUrl.Combine(Base(), null!));

        Assert.Equal("path", refused.ParamName);
    }

    // ---------------------------------------------------------------- the path that is nothing, or all slashes

    /// <summary>The path that is empty and the path that is nothing but slashes come back as the base's own
    /// root, with its trailing slash: there is nothing to join, and the one slash the join adds is the root's
    /// own.</summary>
    [Theory]
    [InlineData("https://app.example.net", "", "https://app.example.net/")]
    [InlineData("https://app.example.net", "/", "https://app.example.net/")]
    [InlineData("https://app.example.net", "///", "https://app.example.net/")]
    [InlineData("https://app.example.net/", "", "https://app.example.net/")]
    [InlineData("https://app.example.net/base/", "///", "https://app.example.net/base/")]
    public void AnEmptyOrAllSlashPathIsTheBaseRootWithItsTrailingSlash(string baseAddress, string path, string asked)
    {
        var joined = ProbeUrl.Combine(new Uri(baseAddress), path);

        Assert.Equal(asked, joined.AbsoluteUri);
        Assert.True(joined.AbsolutePath.EndsWith('/'), "the root keeps its trailing slash");
    }

    // ---------------------------------------------------------------- the path without its leading slash

    /// <summary>The path that comes without its leading slash is joined the same way as the one that brings it:
    /// the one slash the join adds is the only one between the root and the path, whatever root it joins to.</summary>
    [Theory]
    [InlineData("https://app.example.net", "_healthcheck", "https://app.example.net/_healthcheck")]
    [InlineData("https://app.example.net/base/", "alive", "https://app.example.net/base/alive")]
    [InlineData("http://localhost:5000", "health/ready", "http://localhost:5000/health/ready")]
    [InlineData("https://app.example.net", "a/b/c", "https://app.example.net/a/b/c")]
    public void APathWithoutItsLeadingSlashIsJoinedWithTheOneTheJoinAdds(string baseAddress, string path, string asked)
    {
        var joined = ProbeUrl.Combine(new Uri(baseAddress), path);

        Assert.Equal(asked, joined.AbsoluteUri);
    }

    // ---------------------------------------------------------------- the runs of slashes

    /// <summary>The runs of slashes at the front of the path are collapsed to the one slash the join adds:
    /// three of them standing, or four, or all the path being them, the address has the single slash between
    /// the root and the path.</summary>
    [Theory]
    [InlineData("///_healthcheck", "https://app.example.net/_healthcheck")]
    [InlineData("////", "https://app.example.net/")]
    [InlineData("///x///y", "https://app.example.net/x///y")]
    [InlineData("////x", "https://app.example.net/x")]
    public void RunsOfSlashesAtTheFrontOfThePathCollapseToTheOneTheJoinAdds(string path, string asked)
    {
        var joined = ProbeUrl.Combine(Base(), path);

        Assert.Equal(asked, joined.AbsoluteUri);
    }

    /// <summary>The runs of slashes at the back of the path are not collapsed: the join looks only at the front
    /// of the path, and whatever slashes the path brought at its back, or in its middle, stand in the address
    /// it says.</summary>
    [Theory]
    [InlineData("/healthcheck/", "https://app.example.net/healthcheck/")]
    [InlineData("healthcheck///", "https://app.example.net/healthcheck///")]
    [InlineData("///healthcheck///", "https://app.example.net/healthcheck///")]
    [InlineData("/x///y", "https://app.example.net/x///y")]
    public void RunsOfSlashesAtTheBackOfThePathStandInTheJoinedAddress(string path, string asked)
    {
        var joined = ProbeUrl.Combine(Base(), path);

        Assert.Equal(asked, joined.AbsoluteUri);
    }

    // ---------------------------------------------------------------- the base: what the join keeps and what it drops

    /// <summary>The userinfo of the base address stands in the joined address, beside the port
    /// <see cref="ProbeUrlTests" /> pins: the join takes the base up to its path, and neither the user the
    /// base names nor the port it stands on is left out of the address it says.</summary>
    [Theory]
    [InlineData("https://user:pass@example.com:8443/base/", "/_healthcheck", "https://user:pass@example.com:8443/base/_healthcheck")]
    [InlineData("https://user@example.com:8080", "alive", "https://user@example.com:8080/alive")]
    [InlineData("https://user:pass@example.com", "/health/ready", "https://user:pass@example.com/health/ready")]
    public void TheUserinfoOfTheBaseAddressStandsInTheJoinedAddress(string baseAddress, string path, string asked)
    {
        var joined = ProbeUrl.Combine(new Uri(baseAddress), path);

        Assert.Equal(asked, joined.AbsoluteUri);
    }

    /// <summary>The query and the fragment of the base address do not stand in the joined address: the join
    /// takes the base up to its path, and nothing of what the base asked for or pointed at after the path is
    /// in the address it says.</summary>
    [Theory]
    [InlineData("https://app.example.net/p?q=1", "/alive", "https://app.example.net/p/alive")]
    [InlineData("https://app.example.net/p#frag", "alive", "https://app.example.net/p/alive")]
    [InlineData("https://app.example.net/p?q=1#frag", "/a/b", "https://app.example.net/p/a/b")]
    public void TheQueryAndTheFragmentOfTheBaseAddressDoNotStandInTheJoinedAddress(string baseAddress, string path, string asked)
    {
        var joined = ProbeUrl.Combine(new Uri(baseAddress), path);

        Assert.Equal(asked, joined.AbsoluteUri);
        Assert.Empty(joined.Query);
        Assert.Empty(joined.Fragment);
    }

    // ---------------------------------------------------------------- the base that is no base

    /// <summary>The base that is relative to a page is no base at all: the address the prober asks at must be
    /// absolute, and a base that is relative is refused, with the refusal saying it is a relative URI.</summary>
    [Theory]
    [InlineData("rel/path")]
    [InlineData("./a")]
    [InlineData("a/b")]
    [InlineData("x")]
    public void ABaseAddressThatIsRelativeToThePageIsRefused(string baseAddress)
    {
        var refused = Assert.Throws<InvalidOperationException>(() => ProbeUrl.Combine(new Uri(baseAddress, UriKind.Relative), "/x"));

        Assert.Equal("This operation is not supported for a relative URI.", refused.Message);
    }

    // ---------------------------------------------------------------- the wire form of the path

    /// <summary>The path with whitespace and unicode is said in its wire form, not in the letters it came in
    /// with: the address that goes out on the wire is what <see cref="Uri.AbsoluteUri" /> says, and
    /// <see cref="Uri.ToString" /> keeps the letters the path was brought in with, which are not the same
    /// letters.</summary>
    [Theory]
    [InlineData("/health check", "https://app.example.net/health%20check")]
    [InlineData("/a b/c d", "https://app.example.net/a%20b/c%20d")]
    [InlineData("/héllo", "https://app.example.net/h%C3%A9llo")]
    [InlineData("/naïve café", "https://app.example.net/na%C3%AFve%20caf%C3%A9")]
    public void APathWithWhitespaceOrUnicodeIsSaidInItsWireFormNotInItsKeptLetters(string path, string wireForm)
    {
        var joined = ProbeUrl.Combine(Base(), path);

        Assert.Equal(wireForm, joined.AbsoluteUri);
        Assert.NotEqual(wireForm, joined.ToString());
    }

    // ---------------------------------------------------------------- the letters the path does not keep

    /// <summary>The <c>?</c> and the <c>#</c> inside a path are not path letters in the address the join says:
    /// the joined string is parsed as an address, and from there on the <c>?</c> starts the query and the
    /// <c>#</c> starts the fragment, whatever letters the path brought after them.</summary>
    [Theory]
    [InlineData("/health?x=1", "/health", "?x=1", "")]
    [InlineData("/health#frag", "/health", "", "#frag")]
    [InlineData("a?b#c", "/a", "?b", "#c")]
    [InlineData("/health?", "/health", "?", "")]
    [InlineData("/health#", "/health", "", "#")]
    [InlineData("/health?#frag", "/health", "?", "#frag")]
    public void AQuestionMarkOrHashInThePathIsReparsedAsQueryAndFragment(string path, string pathPart, string queryPart, string fragmentPart)
    {
        var joined = ProbeUrl.Combine(Base(), path);

        Assert.Equal(pathPart, joined.AbsolutePath);
        Assert.Equal(queryPart, joined.Query);
        Assert.Equal(fragmentPart, joined.Fragment);
    }

    // ---------------------------------------------------------------- the members

    /// <summary><see cref="ProbeUrl" /> is the holder of the join and nothing else: the only public member is
    /// <c>Uri Combine(Uri baseAddress, string path)</c>, the one the prober calls, and the day a second member
    /// is added to the holder, this says so.</summary>
    [Fact]
    public void ProbeUrlIsAPublicStaticHolderWhoseOnlyPublicMemberIsCombine()
    {
        var type = typeof(ProbeUrl);

        Assert.True(type.IsAbstract && type.IsSealed, "the holder is static: nothing of it is created");
        Assert.Empty(type.GetConstructors(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic));
        Assert.Empty(type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static));
        Assert.Empty(type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static));
        Assert.Empty(type.GetEvents(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static));
        Assert.Empty(type.GetNestedTypes(BindingFlags.Public));
        Assert.Empty(type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));

        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);

        Assert.Equal(
            ["Uri Combine(Uri baseAddress, String path)"],
            [.. methods.Select(Signature).Order(StringComparer.Ordinal)]);

        Assert.All(methods, method => Assert.True(method.IsStatic, $"{method.Name} is called through an instance"));
    }

    /// <summary>A member as the surface is written: what it gives back, what it is called, and its arguments
    /// in the order a caller names them, each with its type and its name.</summary>
    private static string Signature(MethodInfo method) =>
        $"{method.ReturnType.Name} {method.Name}({string.Join(", ", method.GetParameters().Select(parameter => $"{parameter.ParameterType.Name} {parameter.Name}"))})";
}
