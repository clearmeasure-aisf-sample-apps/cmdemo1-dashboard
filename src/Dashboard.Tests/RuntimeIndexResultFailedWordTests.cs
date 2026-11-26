using System.Reflection;
using System.Runtime.CompilerServices;

namespace Dashboard.Tests;

/// <summary>
/// The loader's own result says "no index, and here is why" in the word and the way of the parse records
/// (<see cref="RuntimeParseResult{T}" />): one internal static <c>Failed</c> that takes the problems as <c>params</c>,
/// so one reason is handed over as it is and a list gathered beforehand is handed over as the same list. Found by
/// reflection, as the app declares no <c>InternalsVisibleTo</c>.
/// </summary>
public class RuntimeIndexResultFailedWordTests
{
    private const BindingFlags InternalStatic = BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly;

    private static MethodInfo TheOnlyFailed(Type type)
    {
        var failed = Assert.Single(type.GetMethods(InternalStatic), method => method.Name == "Failed");
        Assert.True(failed.IsAssembly);
        return failed;
    }

    private static bool IsParams(ParameterInfo parameter) =>
        parameter.IsDefined(typeof(ParamArrayAttribute)) || parameter.IsDefined(typeof(ParamCollectionAttribute));

    [Fact]
    public void TheLoaderResultSaysNoValueInTheWordTheParseRecordsUse()
    {
        var loader = TheOnlyFailed(typeof(RuntimeIndexResult));
        var parse = TheOnlyFailed(typeof(RuntimeParseResult<RuntimeIndex>));

        Assert.Equal(parse.Name, loader.Name);
        Assert.Equal(typeof(RuntimeIndexResult), loader.ReturnType);
    }

    [Fact]
    public void TheProblemsAreHandedOverAsParamsLikeTheParseRecords()
    {
        var loader = Assert.Single(TheOnlyFailed(typeof(RuntimeIndexResult)).GetParameters());
        var parse = Assert.Single(TheOnlyFailed(typeof(RuntimeParseResult<RuntimeIndex>)).GetParameters());

        Assert.True(IsParams(parse));
        Assert.True(IsParams(loader));
        Assert.True(typeof(IReadOnlyList<string>).IsAssignableFrom(parse.ParameterType));
        Assert.Equal(typeof(IReadOnlyList<string>), loader.ParameterType);
    }

    [Fact]
    public void ManyProblemsGivenOneByOneKeepTheirWordsNumberAndOrder()
    {
        string[] problems = ["runtime/index.json: not JSON.", "runtime/index.json: environments: missing or not an array."];

        var result = Assert.IsType<RuntimeIndexResult>(TheOnlyFailed(typeof(RuntimeIndexResult)).Invoke(null, [problems]));

        Assert.Equal(RuntimeAvailability.Failed, result.Availability);
        Assert.Null(result.Index);
        Assert.Equal(problems, result.Errors);
    }
}
