using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// <see cref="ProbeUrl" /> holds the two ways the page turns a path into an address (#1142):
/// <c>ProbeUrl.Combine</c> joins a path onto another node's base address, which
/// <see cref="ProbeUrlTests" /> and <see cref="ProbeUrlEdgeCaseTests" /> pin;
/// <see cref="ProbeUrl.ResolveRelative" /> resolves a path of the dashboard's own site against the address the
/// page is served from, which is how the page reads <c>topology.json</c> and its own build facts. This file
/// pins the second of the two: that the path is taken from the site's root whatever it starts with, that the
/// address the page is served from contributes its scheme, its host, its port and its own path but neither its
/// query nor its fragment, that a path naming another site or carrying a scheme of its own is no address at all
/// (the refusal the member says out loud, where the call site used to swallow it), that the empty path is the
/// page's own address, that a page with no address of its own leaves the path relative for the client to answer
/// for, and that the member's own surface is what the holder declares today.
/// <para>
/// The member is internal, and the app declares no <c>InternalsVisibleTo</c> (as
/// <c>Dashboard.Tests.csproj</c> notes of <c>JsonRead</c>): <c>NodeProber</c> is its only caller and lives in the
/// same assembly, so nothing outside the app needs it, and the holder's public surface stands as
/// <see cref="ProbeUrlEdgeCaseTests" /> writes it - the only public member still <c>Combine</c>. The test
/// therefore cannot name the member and finds it by its name on the holder, the way
/// <see cref="MarkPlaceOrderEdgeCaseTests" /> reads the internal <c>MarkPlace</c>.
/// <see cref="NodeProberTests" /> pins the same rules through the call that uses them.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: the member takes an address and a
/// path and says an address, and every address below is one the test brought.
/// </para>
/// </summary>
public class ProbeUrlResolveRelativeEdgeCaseTests
{
    private static readonly Uri Page = new("https://dashboard.example.net/");

    /// <summary>The member, read from the assembly by the name it is declared with.</summary>
    private static MethodInfo Member() =>
        typeof(ProbeUrl).GetMethod("ResolveRelative", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)!;

    /// <summary>What the member says of an address and a path.</summary>
    private static Uri? Resolve(Uri? baseAddress, string path) => (Uri?)Member().Invoke(null, [baseAddress, path]);

    // ---------------------------------------------------------------- the member itself

    /// <summary>The holder declares a second member, and this one is not public: <c>NodeProber</c>, its only
    /// caller, is of the same assembly, and the public surface <see cref="ProbeUrlEdgeCaseTests" /> pins - the
    /// one public member <c>Uri Combine(Uri baseAddress, string path)</c> - stands as that test writes it.</summary>
    [Fact]
    public void TheMemberIsTheHoldersSecondAndIsInternal()
    {
        var member = Member();

        Assert.False(member.IsPublic, "the second member of the holder is public: ProbeUrlEdgeCaseTests pins the public surface to Combine alone");
        Assert.True(member.IsStatic, "the member is called through an instance");
        Assert.Equal(typeof(Uri), member.ReturnType);
        Assert.Equal(["baseAddress", "path"], [.. member.GetParameters().Select(parameter => parameter.Name!)]);
        Assert.True(member.GetParameters()[0].ParameterType == typeof(Uri), "the page's address is read as an address");
        Assert.Equal(typeof(string), member.GetParameters()[1].ParameterType);
    }

    /// <summary>The path that is no path is refused by the name of the parameter it fills, as the join refuses
    /// its own: the caller is told at the door which of its two arguments it brought wrong.</summary>
    [Fact]
    public void ANullPathIsRefusedByTheNamePath()
    {
        var refused = Assert.IsType<ArgumentNullException>(Failure([Page, null!]));

        Assert.Equal("path", refused.ParamName);
    }

    // ---------------------------------------------------------------- the path, from the site's root, whatever it starts with

    /// <summary>A path of the page's own site is read from the site's root whatever it starts with - the front
    /// slash written or left out, a run of them standing - and the address it makes is the absolute one the
    /// request goes to: the same address the browser's client joined for itself before the member had a name.
    /// A page served under a path of its own keeps it, because the address of a page always ends in the slash
    /// that closes it.</summary>
    [Theory]
    [InlineData("https://dashboard.example.net/", "/build-facts.json", "https://dashboard.example.net/build-facts.json")]
    [InlineData("https://dashboard.example.net/", "build-facts.json", "https://dashboard.example.net/build-facts.json")]
    [InlineData("https://dashboard.example.net/", "///build-facts.json", "https://dashboard.example.net/build-facts.json")]
    [InlineData("https://dashboard.example.net/dash/", "/build-facts.json", "https://dashboard.example.net/dash/build-facts.json")]
    [InlineData("https://dashboard.example.net/dash/", "build-facts.json", "https://dashboard.example.net/dash/build-facts.json")]
    [InlineData("https://dashboard.example.net/dash/", "/runtime/uat.svg", "https://dashboard.example.net/dash/runtime/uat.svg")]
    [InlineData("http://localhost:5000/", "/build-facts.json", "http://localhost:5000/build-facts.json")]
    public void APathOfTheOwnSiteIsResolvedAgainstThePageAddress(string page, string path, string asked)
    {
        var address = Resolve(new Uri(page), path);

        Assert.NotNull(address);
        Assert.True(address.IsAbsoluteUri, "the address of a request is absolute");
        Assert.Equal(asked, address.AbsoluteUri);
    }

    /// <summary>The page's address contributes as much of itself as an address of a request can hold: its scheme,
    /// its host, its port, and the path the page stands under - and neither its query nor its fragment, which
    /// belong to the page being read and not to the file asked for.</summary>
    [Theory]
    [InlineData("https://dashboard.example.net/index.html?x=1#top", "/build-facts.json", "https://dashboard.example.net/build-facts.json")]
    [InlineData("https://dashboard.example.net/dash/?x=1", "build-facts.json", "https://dashboard.example.net/dash/build-facts.json")]
    [InlineData("http://localhost:5000/dash/#top", "/alive", "http://localhost:5000/dash/alive")]
    public void TheQueryAndTheFragmentOfThePageAddressDoNotStandInTheAddressAsked(string page, string path, string asked)
    {
        var address = Resolve(new Uri(page), path);

        Assert.NotNull(address);
        Assert.Equal(asked, address.AbsoluteUri);
        Assert.Empty(address.Query);
        Assert.Empty(address.Fragment);
    }

    /// <summary>The empty path, and the path of nothing but front slashes, is the page's own address: the file
    /// asked for is the page itself, and that is the answer rather than no answer.</summary>
    [Theory]
    [InlineData("")]
    [InlineData("/")]
    [InlineData("///")]
    public void AnEmptyPathIsThePageAddressItself(string path)
    {
        var address = Resolve(Page, path);

        Assert.Equal("https://dashboard.example.net/", address?.AbsoluteUri);
    }

    // ---------------------------------------------------------------- the refusal the member says

    /// <summary>A path that names another site, or carries a scheme of its own, is no path of this site and so
    /// no address at all: null, and the caller reads nothing. The address of another site is the one thing the
    /// page's own file must never be asked at, whatever the file of the topology says.</summary>
    [Theory]
    [InlineData("https://elsewhere.example/build-facts.json")]
    [InlineData("HTTP://elsewhere.example/build-facts.json")]
    [InlineData("ftp://elsewhere.example/build-facts.json")]
    [InlineData("mailto:build-facts.json")]
    [InlineData("a:b")]
    [InlineData("http://")]
    public void APathNamingAnotherSiteOrCarryingASchemeIsNoAddress(string path)
    {
        Assert.Null(Resolve(Page, path));
    }

    /// <summary>A page with no address of its own resolves nothing, and the path stands as the relative address
    /// it is, for the client to answer for as it answers for any relative request. This is the shape the
    /// browser's client met before the member had a name, kept on purpose.</summary>
    [Theory]
    [InlineData("/build-facts.json", "build-facts.json")]
    [InlineData("build-facts.json", "build-facts.json")]
    [InlineData("///runtime/uat.svg", "runtime/uat.svg")]
    public void WithoutAPageAddressThePathStandsAsARelativeAddress(string path, string relative)
    {
        var address = Resolve(null, path);

        Assert.NotNull(address);
        Assert.False(address.IsAbsoluteUri, "a path with nothing to resolve it against stays relative");
        Assert.Equal(relative, address.ToString());
    }

    // ---------------------------------------------------------------- the two idioms, where they differ

    /// <summary>The two idioms of the holder are not one: a node's base address is taken up to its path and
    /// joined with exactly one slash, while a page's own address is resolved as an address is resolved - so
    /// only an address that closes its path with a slash, as the address of a page always does, gives both of
    /// them the same answer. The difference is pinned here so the day one of the two is folded into the other,
    /// this says which answer is lost.</summary>
    [Theory]
    [InlineData("https://dashboard.example.net/dash/", "/build-facts.json")]
    [InlineData("https://dashboard.example.net/dash", "/build-facts.json")]
    public void TheJoinAndTheResolutionAJoinNeedsDifferWhereTheAddressHasAPathOfItsOwn(string baseAddress, string path)
    {
        var joined = ProbeUrl.Combine(new Uri(baseAddress), path);
        var resolved = Resolve(new Uri(baseAddress), path);

        Assert.NotNull(resolved);
        if (baseAddress.EndsWith('/'))
        {
            Assert.Equal(joined.AbsoluteUri, resolved.AbsoluteUri);
        }
        else
        {
            Assert.NotEqual(joined.AbsoluteUri, resolved.AbsoluteUri);
            Assert.Equal("https://dashboard.example.net/build-facts.json", resolved.AbsoluteUri);
        }
    }

    /// <summary>The exception the member threw, undressed of the reflection wrapper that carried it.</summary>
    private static Exception Failure(params object?[] arguments)
    {
        var thrown = Assert.Throws<TargetInvocationException>(() => Member().Invoke(null, arguments));

        return thrown.InnerException!;
    }
}

