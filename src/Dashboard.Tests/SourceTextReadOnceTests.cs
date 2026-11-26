namespace Dashboard.Tests;

/// <summary>
/// The shared source read is taken once per root per test run: the coverage guards all ask for the same folders,
/// and the walk, the disk read and what is read from it are the same bytes every time they ask. The cache is keyed
/// by the root, not global, so a test that writes files of its own and reads them back passes a root of its own
/// and gets a walk of its own.
/// </summary>
public class SourceTextReadOnceTests
{
    [Fact]
    public void TheSameRootIsReadOnceHoweverItIsSpelled()
    {
        var first = SourceText.Files(TestsRoot());
        var again = SourceText.Files(Path.Combine(TestsRoot(), "..", "Dashboard.Tests"));

        Assert.Same(first, again);
        Assert.Contains(first, file => file.Path == "SourceTextReadOnceTests.cs");
        Assert.DoesNotContain(first, file => SourceText.IsBuildCopy(file.Path));
    }

    [Fact]
    public void ARootOfItsOwnIsAWalkOfItsOwn()
    {
        var root = Directory.CreateTempSubdirectory("source-text-").FullName;

        try
        {
            File.WriteAllText(Path.Combine(root, "B.cs"), "public static class B\n{\n    public static int Two() => 2;\n}\n");
            File.WriteAllText(Path.Combine(root, "A.cs"), "public static class A\n{\n    public static int One() => 1;\n}\n");
            Directory.CreateDirectory(Path.Combine(root, "obj"));
            File.WriteAllText(Path.Combine(root, "obj", "A.cs"), "public static class Copy\n{\n    public static int One() => 1;\n}\n");

            var files = SourceText.Files(root);

            Assert.Equal(["A.cs", "B.cs"], files.Select(file => file.Path));
            Assert.Equal("A.One", Assert.Single(SourceText.Declarations(files[0].Path, files[0].Text)).Key);
            Assert.NotSame(SourceText.Files(TestsRoot()), files);
            Assert.Same(files, SourceText.Files(root));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static string TestsRoot() => MarkupGuards.TestsRoot();
}
